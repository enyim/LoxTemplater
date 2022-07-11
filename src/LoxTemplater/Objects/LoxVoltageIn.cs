namespace Enyim.LoxTempl;

[LoxType("VoltageIn")]
public class LoxVoltageIn : LoxSensor
{
    public override void ConnectProxy(LoxInputRef proxy)
    {
        proxy.Connectors.ByName("AI").Accept(Connectors.ByName("AQ"));
        proxy.Connectors.ByName("I").Accept(Connectors.ByName("Q"));

        proxy.Analog = true;
        proxy.LinkRefType = 51;
    }
}
