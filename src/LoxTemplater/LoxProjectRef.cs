using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

using Enyim.LoxTempl;

public class LoxProjectRef
{
    private LoxProjectRef(XDocument document)
    {
        Debug.Assert(document.Root != null);

        Document = document;
        SerDes = new LoxmlSerializer(new[] { typeof(LoxBase).Assembly });

        var loxDoc = SerDes.Select<LoxDocument>(Document.Root).Single();

        V = loxDoc.V;

        CategoriesById = SerDes.Select<LoxCategory>(Document.RequireContainer("Document", "CategoryCaption")).ToDictionary(p => p.Id);
        PlacesById = SerDes.Select<LoxPlace>(Document.RequireContainer("Document", "PlaceCaption")).ToDictionary(p => p.Id);
        Pages = SerDes.Select<LoxPage>(Document.RequireContainer("Document", "Program")).ToList();

        EmptyPlace = PlacesById.Values.Single(p => p.First);
    }

    public XDocument Document { get; }
    public XElement Root => Document.Root!;

    public LoxmlSerializer SerDes { get; }

    public IReadOnlyDictionary<string, LoxPlace> PlacesById { get; }
    public IReadOnlyDictionary<string, LoxCategory> CategoriesById { get; }
    public IReadOnlyList<LoxPage> Pages { get; }

    public LoxPlace EmptyPlace { get; }

    public int V { get; }

    public static LoxProjectRef Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new XmlTextReader(stream);

        var doc = XDocument.Load(reader, LoadOptions.PreserveWhitespace);

        if (doc.Root == null) throw new InvalidOperationException("Invalid project file, missing root node");

        return new LoxProjectRef(doc);
    }
}
