namespace Enyim.LoxTempl;

[ElementName("In")]
public class LoxIn : LoxBase
{
    public LoxIn() { }

    public LoxIn(LoxConnector c)
    {
        Input = c.Id;
    }

    [FromAttribute("Input")]
    public string Input { get; set; }

    [FromAttribute("CF")]
    public bool IsSplit { get; set; }
}