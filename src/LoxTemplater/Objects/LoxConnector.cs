namespace Enyim.LoxTempl;

[ElementName("Co")]
public class LoxConnector : LoxHaveId
{
    public LoxConnector() { }

    public LoxConnector(string label)
    {
        Id = Lox.NewId();
        K = label;
    }

    [FromAttribute("K")]
    public string K { get; set; }

    [FromElement]
    public TrackingList<LoxIn> Inputs { get; set; } = new TrackingList<LoxIn>();

    [FromAttribute("Nc")]
    public int Nc => Inputs?.Count ?? 0;

    public void Accept(LoxConnector incoming, bool isSplit = false)
    {
        if (Inputs.None(c => c.Input == incoming.Id))
        {
            Inputs.Add(new LoxIn(incoming) { IsSplit = isSplit });
        }
    }
}
