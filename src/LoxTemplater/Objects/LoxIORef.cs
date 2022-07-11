namespace Enyim.LoxTempl;

// TODO iorefs do not have <IoData />
public abstract class LoxIORef : LoxVisualObject
{
    protected LoxIORef(string type)
    {
        Type = type;
    }

    protected LoxIORef(string type, string[] connections, LoxObject refersTo)
        : this(type)
    {
        Id = Lox.NewId();
        Title = refersTo.Title;
        Ref = refersTo.Id ?? throw new InvalidOperationException("Missing ref id");

        foreach (var c in connections)
        {
            Connectors.Add(new LoxConnector(c));
        }
    }

    [FromAttribute("Analog")]
    public bool Analog { get; set; }

    [FromAttribute("Ref")]
    public string? Ref { get; set; }

    [FromAttribute("LinkRefType")]
    public int LinkRefType { get; set; }
}
