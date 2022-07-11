namespace Enyim.LoxTempl;

public interface IAmSensor : IHaveData, IAmDevice
{
    void ConnectProxy(LoxInputRef proxy);
}
