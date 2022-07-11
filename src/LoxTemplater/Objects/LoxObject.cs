namespace Enyim.LoxTempl;

public class LoxObject : LoxHaveId
{
    [FromAttribute(nameof(Title))]
    public string Title { get; set; }

    [FromAttribute(nameof(Type))]
    public string Type { get; set; }

    [FromAttribute("V")]
    public int V { get; set; } = 151;

    public override string ToString() => $"{{{Type}:{Title}}}";
}
