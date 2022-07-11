namespace Enyim.LoxTempl;

[LoxType(ItemType)]
public class LoxOutputRef : LoxIORef
{
    public const string ItemType = "OutputRef";

    private static readonly string[] ConnectorNames = "AI,AQ".Split(',');

    public LoxOutputRef() : base(ItemType) { }
    public LoxOutputRef(LoxObject refersTo) : base(ItemType, ConnectorNames, refersTo) { }

    public void Accept(LoxConnector connector, bool isSplit = false)
    {
        Connectors.ByName("I").Accept(connector, isSplit);
    }
}
