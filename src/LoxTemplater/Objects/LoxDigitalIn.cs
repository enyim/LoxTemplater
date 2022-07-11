namespace Enyim.LoxTempl;

[LoxType("DigitalIn")]
public class LoxDigitalIn : LoxSensor
{
    public override void ConnectProxy(LoxInputRef proxy)
    {
        proxy.Connectors.ByName("AI").Accept(Connectors.ByName("Q"));
        proxy.Connectors.ByName("I").Accept(Connectors.ByName("Qe"));

        proxy.Analog = false;
        proxy.LinkRefType = 55;
    }
}
