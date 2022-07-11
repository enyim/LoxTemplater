namespace Enyim.LoxTempl;

[LoxType("Category")]
public class LoxCategory : LoxObject
{
    [FromAttribute("First")]
    public bool First { get; set; }
}
