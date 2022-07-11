namespace Enyim.LoxTempl;

[LoxType("Memory")]
public class LoxMemory : LoxVisualObject
{
    public const int TP_DIGITAL = 0;
    public const int TP_ANALOG = 1;
    public const int TP_TEXT = 2;
    public const int TP_T5 = 3;

    [FromAttribute("Tp")]
    public int Tp { get; set; }
}
