namespace Enyim.LoxTempl;

[LoxType("ModbusDev")]
public class LoxModbusDev : LoxDevice
{
    [FromXPath("C[@Type='SensorCaption']/C")]
    public IReadOnlyList<LoxModbusASensor> Sensors { get; set; }

    [FromXPath("C[@Type='ActorCaption']/C")]
    public IReadOnlyList<LoxModbusAActor> Actors { get; set; }
}
