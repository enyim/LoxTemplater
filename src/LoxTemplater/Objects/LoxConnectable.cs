namespace Enyim.LoxTempl;

public abstract class LoxConnectable : LoxHaveMeta, IConnectable
{
    [FromAttribute("Nio")]
    public int Nio => Connectors?.Count ?? 0;

    [FromElement]
    public TrackingList<LoxConnector> Connectors { get; set; } = new();
}
