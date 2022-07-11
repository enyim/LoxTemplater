namespace Enyim.LoxTempl;

[ElementName("IoData")]
public class LoxMeta : LoxBase, IMeta
{
    [FromAttribute("Pr")]
    public string? PlaceId { get; set; }

    [FromAttribute("Cr")]
    public string? CategoryId { get; set; }
}

public interface IMeta
{
    public string? PlaceId { get; }
    public string? CategoryId { get; }
}