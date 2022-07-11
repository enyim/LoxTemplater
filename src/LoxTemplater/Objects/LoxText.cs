namespace Enyim.LoxTempl;

[LoxType(ItemType)]
public class LoxText : LoxVisualObject
{
    public const string ItemType = "Text";

    public LoxText()
    {
        Type = ItemType;
    }

    [FromAttribute("Text")]
    public string Text { get; set; }

    [FromAttribute("Td")]
    public bool IsTodo { get; set; }
}
