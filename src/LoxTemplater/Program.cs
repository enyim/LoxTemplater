﻿using System.Diagnostics;
using System.Xml.Linq;

using CommandLine;

using Enyim.LoxTempl;

using static System.StringComparer;

try
{
    new Parser(s =>
    {
        s.EnableDashDash = true;
        s.HelpWriter = Console.Out;
        s.CaseInsensitiveEnumValues = true;
    }).ParseArguments(args, typeof(ListOpts), typeof(GenerateOpts))
         .WithParsed<ListOpts>(RunListVerb)
         .WithParsed<GenerateOpts>(RunGenerateVerb);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

static void RunListVerb(ListOpts opts)
{
    var project = LoxProjectRef.Load(opts.ProjectPath);
    IEnumerable<LoxObject> list = opts.What switch
    {
        ListOpts.ListWhat.Categories => project.CategoriesById.Values,
        ListOpts.ListWhat.Places => project.PlacesById.Values,
        ListOpts.ListWhat.Pages => project.Pages,
        _ => throw new NotSupportedException()
    };

    foreach (var item in list.OrderBy(a => a.Title).ThenBy(a => a.Id))
    {
        Console.WriteLine(item.Title);
    }
}

static void RunGenerateVerb(GenerateOpts opts)
{
    opts.Validate();

    var project = LoxProjectRef.Load(opts.ProjectPath);

    var collector = new IOCollector(project);
    var allIO = collector.GroupIOsByPlace();
    AssertUniqueIONames(allIO);

    var template = FindPageByTitle(project, opts.TemplateName) ?? throw new InvalidOperationException($"Cannot find template page '{opts.TemplateName}'");

    CheckInvalidPlaces(project, template);

    foreach (var roomName in opts.Rooms)
    {
        var place = project.PlacesById.Values.First(p => OrdinalIgnoreCase.Equals(p.Title, roomName)) ?? throw new InvalidOperationException($"Invalid room {roomName}");

        GeneratePage(opts, template, project, allIO, place);
    }

    if (!opts.DryRun)
    {
        if (opts.DeleteTemplate) template.Element?.Remove();

        using var file = File.Create(opts.OutputPath);
        using var xw = System.Xml.XmlWriter.Create(file, new()
        {
            Indent = true,
            IndentChars = "\t",
            NewLineHandling = System.Xml.NewLineHandling.None,
            Encoding = System.Text.Encoding.UTF8,
        });

        project.Document.Save(xw);
    }
}

static void AssertUniqueIONames(ILookup<string, OwnedIORef> allIO)
{
    //var fail = false;

    foreach (var room in allIO)
    {
        var invalids = from o in room
                       group o by (o.Owner.Title, o.Port.Title) into g
                       select (name: g.Key, count: g.Count(), items: g) into p
                       where p.count > 1
                       select p;

        foreach (var p in invalids)
        {
            foreach (var item in p.items)
            {
                Log($"Duplicate input found in room {room.Key}: {p.name} - {item}");
                //fail = true;
            }
        }
    }

    //if (fail) throw new InvalidOperationException("this bug");
}

static void GeneratePage(GenerateOpts opts, LoxPage template, LoxProjectRef project, ILookup<string, OwnedIORef> allIO, LoxPlace desiredPlace)
{
    var serdes = project.SerDes;
    var clonePageName = String.IsNullOrEmpty(opts.PageNameTemplate)
                            ? $"{opts.TemplateName} - {desiredPlace.Title}"
                            : opts.PageNameTemplate.Replace("{Room}", desiredPlace.Title);

    var existingPage = FindPageByTitle(project, clonePageName);

    if (existingPage != null)
    {
        if (opts.OverwritePages)
        {
            ((IXmlBound)existingPage).Element!.Remove();
        }
        else
        {
            throw new InvalidOperationException($"Project already contains the page '{clonePageName}'");
        }
    }

    var clone = ClonePage(template);
    var page = serdes.Load<LoxPage>(clone);

    page.Title = clonePageName;
    SetPlacesOfObjects(project, clone, desiredPlace.Id);

    var templateParams = (from memory in serdes.Select<LoxMemory>(clone)
                          select ParseMemoryFlagName(memory) into param
                          where param is not null
                          select param).ToList();

    var matches = GetTemplateParamMatches(templateParams, allIO[desiredPlace.Id]);

    var possibleInputs = matches.Where(m => m.IO.Port is IHaveData);
    var possibleOutputs = matches.Where(m => m.IO.Port is IAcceptData);

    var dirty = ConnectDataProvidersToMemoryFlags(possibleInputs)
                    .Concat(ConnectDataReceiversToInputRefs(possibleOutputs))
                    .Where(b => b != null)
                    .ToHashSet();

    foreach (var element in dirty)
    {
        serdes.Commit(element);
    }

    serdes.Commit(page);

    const int CONNECTOR_GAP = 3072;

    IEnumerable<LoxBase> ConnectDataProvidersToMemoryFlags(IEnumerable<ParamMatch> matches)
    {
        foreach (var (io, param) in matches)
        {
            if (io.Port is not IHaveData amInput) throw new InvalidOperationException();

            if (io.Port is LoxSensor sensor)
            {
                var inputRef = new LoxInputRef(sensor)
                {
                    V = project.V,
                    Color = "0,138,207"
                };

                sensor.ConnectProxy(inputRef);

                clone.Add(inputRef.CreateElement());
                PositionObject(inputRef, param.Memory, dx: -CONNECTOR_GAP);

                yield return inputRef;
                yield return sensor;
            }

            amInput.ConnectTo(param.Memory.Connectors.ByName("Input"));

            yield return param.Memory;
            yield return amInput as LoxBase;
        }
    }

    IEnumerable<LoxBase> ConnectDataReceiversToInputRefs(IEnumerable<ParamMatch> matches)
    {
        foreach (var (io, param) in matches)
        {
            if (io.Port is not IAcceptData amOutput) throw new InvalidOperationException();

            var memoryInputRef = serdes.Select<LoxInputRef>(clone).FirstOrDefault(r => r.Ref == param.Memory.Id);
            if (memoryInputRef == null) throw new InvalidOperationException($"Expected to find the output of {param.Memory.Title} on the page");

            if (io.Port is LoxActuator actuator)
            {
                var outputRef = new LoxOutputRef(actuator)
                {
                    V = project.V,
                    Color = "138,0,207"
                };

                actuator.ConnectProxy(outputRef);
                if (outputRef.Analog)
                {
                    memoryInputRef.Analog = true;
                    param.Memory.Tp = LoxMemory.TP_ANALOG;
                }

                PositionObject(outputRef, memoryInputRef, dx: +CONNECTOR_GAP);
                clone.Add(outputRef.CreateElement());

                yield return outputRef;
                yield return actuator;
            }

            amOutput.Accept(memoryInputRef.Connectors.ByName("AQ"));

            yield return param.Memory;
            yield return memoryInputRef;
            yield return amOutput as LoxBase;
        }
    }

    IEnumerable<ParamMatch> GetTemplateParamMatches(IEnumerable<TemplateParam> templateParams, IEnumerable<OwnedIORef> ios)
    {
        foreach (var p in templateParams)
        {
            var match = ios.FirstOrDefault(io => DeviceIoMatcher(p, io));

            if (match != null)
            {
                Log($"Matched {p} to {match}");

                yield return new ParamMatch(match, p);
            }
            else
            {
                Log($"Cannot find matching input for {p}");
            }
        }

        bool DeviceIoMatcher(TemplateParam param, OwnedIORef io)
        {
            return (Ieq(io.Owner.Title, param.Device)
                    || io.Owner.Title.StartsWith(param.Device, StringComparison.OrdinalIgnoreCase)
                    || Ieq(io.Owner.Title, desiredPlace.Title + " " + param.Device))
                && (Ieq(io.Port.Title, param.IO));
        }

        static bool Ieq(string a, string b) => OrdinalIgnoreCase.Equals(a, b);
    }
}

static LoxPage? FindPageByTitle(LoxProjectRef project, string title)
{
    if (String.IsNullOrEmpty(title)) throw new ArgumentException("Page title must be specified");

    return project.Pages.FirstOrDefault(p => OrdinalIgnoreCase.Equals(p.Title, title));
}

static void CheckInvalidPlaces(LoxProjectRef project, LoxPage page)
{
    var naPlace = project.PlacesById.Values.First(p => p.First).Id;
    var metas = project.SerDes.Select<LoxMeta>(page).Where(m => (m.PlaceId ?? naPlace) != naPlace);

    foreach (var invalid in metas)
    {
        var parent = ((IXmlBound)invalid).Element.Parent!;
        Log($"Object {parent.Attribute("Title")} has unexpected location {project.PlacesById[invalid.PlaceId].Title}");
    }
}

static XElement ClonePage(LoxPage page)
{
    ArgumentNullException.ThrowIfNull(page);

    var template = ((IXmlBound)page).Element;
    Debug.Assert(template != null);

    var clone = new XElement(template);
    template.AddAfterSelf(clone);

    var idMap = (from c in template.DescendantsAndSelf()
                 select c.LoxId() into id
                 where !String.IsNullOrEmpty(id)
                 select id)
                .ToDictionary(id => id, _ => Lox.NewId());

    foreach (var e in clone.DescendantsAndSelf())
    {
        ReplaceId(e, "U");
        ReplaceId(e, "Input");
        ReplaceId(e, "Ref");
        ReplaceIdList(e, "linkC");
    }

    return clone;

    void ReplaceId(XElement e, string name)
    {
        var attr = e.Attribute(name);
        if (attr != null && idMap.TryGetValue(attr.Value, out var newGuid))
        {
            attr.Value = newGuid;
        }
    }

    void ReplaceIdList(XElement e, string name)
    {
        var attr = e.Attribute(name);
        var value = attr?.Value;

        if (value != null)
        {
            var list = value.Split(',');

            for (var i = 0; i < list.Length; i++)
            {
                if (idMap.TryGetValue(list[i], out var newGuid))
                {
                    list[i] = newGuid;
                }
            }

            Debug.Assert(attr != null);
            attr.Value = String.Join(',', list);
        }
    }
}

static void SetPlacesOfObjects(LoxProjectRef project, XElement root, string placeId)
{
    var metas = project.SerDes.Select<LoxMeta>(root);

    foreach (var meta in metas)
    {
        meta.PlaceId = placeId;

        project.SerDes.Commit(meta);
    }
}

static void PositionObject(LoxVisualObject what, LoxVisualObject relativeTo, int dx = 0, int dy = 0)
{
    what.Px = relativeTo.Px + dx;
    what.Py = relativeTo.Py + dy;

    what.Px2 = relativeTo.Px2 + dx;
    what.Py2 = relativeTo.Py2 + dy;
}

static TemplateParam? ParseMemoryFlagName(LoxMemory memory)
{
    var index = memory.Title.IndexOf(':');

    if (index <= 0 || index >= (memory.Title.Length - 1)) return null;

    var device = memory.Title.Remove(index);
    var io = memory.Title.Remove(0, index + 1);

    return new TemplateParam(memory, device, io);
}

static void Log(string message)
{
    Console.WriteLine(FormattableString.Invariant($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} {message}"));
}

internal record ParamMatch(OwnedIORef IO, TemplateParam Param);
internal record TemplateParam(LoxMemory Memory, string Device, string IO);
