namespace Enyim.LoxTempl;

public class LoxVisualObject : LoxConnectable
{
    [FromAttribute("Px")]
    public int Px { get; set; }
    [FromAttribute("Py")]
    public int Py { get; set; }

    [FromAttribute("Px2")]
    public int Px2 { get; set; }

    [FromAttribute("Py2")]
    public int Py2 { get; set; }

    [FromAttribute("Cl")]
    public string Color { get; set; }
}
