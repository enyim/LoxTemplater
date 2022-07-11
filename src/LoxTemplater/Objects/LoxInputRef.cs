namespace Enyim.LoxTempl;

[LoxType(ItemType)]
public class LoxInputRef : LoxIORef
{
    public const string ItemType = "InputRef";

    private static readonly string[] ConnectorNames = "AI,I,AQ,Q".Split(',');

    public LoxInputRef()
        : base(ItemType) { }

    public LoxInputRef(LoxObject refersTo)
        : base(ItemType, ConnectorNames, refersTo) { }
}
