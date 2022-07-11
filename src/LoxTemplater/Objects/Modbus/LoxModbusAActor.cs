namespace Enyim.LoxTempl;

[LoxType("ModbusAActor")]
public class LoxModbusAActor : LoxActuator, IConnectable
{
    public override void ConnectProxy(LoxOutputRef proxy)
    {
        base.ConnectProxy(proxy);

        Connectors.ByName("I").Accept(proxy.Connectors.ByName("AQ"));

        proxy.Analog = true;
        proxy.LinkRefType = 156;
    }

    string IConnectable.Title => Title + " AQ";
}
