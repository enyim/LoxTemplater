namespace Enyim.LoxTempl;

[LoxType("VirtualIn")]
public class LoxVirtualIn : LoxSensor, IConnectable
{
    public override void ConnectProxy(LoxInputRef proxy)
    {
        base.ConnectProxy(proxy);

        proxy.Connectors.ByName("AI").Accept(Connectors.ByName("Q"));
        proxy.Connectors.ByName("I").Accept(Connectors.ByName("Qm"));

        proxy.Analog = true;
        proxy.LinkRefType = 71;
    }

    string IConnectable.Title => Title + " VI";
}

[LoxType("LoxLIVE")]
public class LoxLive: LoxHaveMeta
{
    [FromXPath("C[@Type='VirtualInCaption']/C")]
    public IReadOnlyList<LoxVirtualIn> VirtualIns { get; set; }
}
