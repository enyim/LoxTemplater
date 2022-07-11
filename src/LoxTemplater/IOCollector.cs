namespace Enyim.LoxTempl;

internal class IOCollector
{
    private readonly LoxProjectRef project;

    public IOCollector(LoxProjectRef project)
    {
        ArgumentNullException.ThrowIfNull(project);

        this.project = project;
    }

    public ILookup<string, OwnedIORef> GroupIOsByPlace()
    {
        var query = from list in GetAllIO()
                    from ioref in list
                    let place = CoalescePlace(ioref.Port, ioref.Owner) ?? ioref.ForcedPlace
                    where place != null
                    select (place, ioref);

        var retval = query.ToLookup(a => a.place, a => a.ioref);

        return retval;
    }

    record Tmp(LoxHaveMeta Owner, IConnectable Port, string? ForcedPlace = null);

    private IEnumerable<IEnumerable<OwnedIORef>> GetAllIO()
    {
        var modbusDevices = project.SerDes.Select<LoxModbusDev>(project.Root).ToList();

        yield return modbusDevices.SelectMany(m => m.Sensors, MkIORef);
        yield return modbusDevices.SelectMany(m => m.Actors, MkIORef);

        var validPages = project.Pages.Where(page => CoalescePlace(page) != null);

        yield return validPages.SelectMany(project.SerDes.Select<LoxHeatIRoomController2>).SelectMany(irc => irc.GetIORefs());

        var memoryFlags = validPages.SelectMany(project.SerDes.Select<LoxMemory>).ToDictionary(m => m.Id);
        var inputRefs = from page in validPages
                        from inputRef in project.SerDes.Select<LoxInputRef>(page)
                        where inputRef.Ref != null && memoryFlags.ContainsKey(inputRef.Ref)
                        select new OwnedIORef(inputRef, new BlockConnectorAsDataProvider(inputRef, inputRef.Connectors.ByName("AQ")), CoalescePlace(page));

        yield return memoryFlags.Values.Select(m => new OwnedIORef(m, new BlockConnectorAsDataReceiver(m, m.Connectors.ByName("Input"))));
        yield return inputRefs;

        var miniservers = project.SerDes.Select<LoxLive>(project.Root);

        yield return miniservers.SelectMany(s => s.VirtualIns, MkIORef);

        // TODO rest of the devices we want to support

        static OwnedIORef MkIORef(LoxHaveMeta owner, LoxConnectable io) => new OwnedIORef(owner, io);
    }

    private string? CoalescePlace(params IHaveMeta[] objects)
    {
        foreach (var o in objects)
        {
            var place = o?.Meta?.PlaceId;

            if (!String.IsNullOrEmpty(place) && place != project.EmptyPlace.Id)
            {
                return place;
            }
        }

        return null;
    }
}

public record OwnedIORef(LoxHaveMeta Owner, IConnectable Port, string? ForcedPlace = null);
