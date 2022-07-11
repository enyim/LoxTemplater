namespace Enyim.LoxTempl;

[LoxType("ModbusASensor")]
public class LoxModbusASensor : LoxSensor, IConnectable
{
    [FromAttribute("Analog")]
    public bool Analog { get; set; }

    public override void ConnectProxy(LoxInputRef proxy)
    {
        base.ConnectProxy(proxy);

        proxy.Connectors.ByName("AI").Accept(Connectors.ByName("Q"));
        proxy.Connectors.ByName("I").Accept(Connectors.ByName("Qe"));

        proxy.Analog = true;
        proxy.LinkRefType = 153;
    }

    string IConnectable.Title => Title + " AI";
}
