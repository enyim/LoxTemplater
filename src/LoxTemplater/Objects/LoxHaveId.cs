namespace Enyim.LoxTempl;

public class LoxHaveId : LoxBase
{
    [FromAttribute("U")]
    public string Id { get; set; }
}
