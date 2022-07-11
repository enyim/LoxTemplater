namespace Enyim.LoxTempl;

[LoxType("Place")]
public class LoxPlace : LoxObject
{
    [FromAttribute("First")]
    public bool First { get; set; }
}
