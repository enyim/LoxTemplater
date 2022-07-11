namespace Enyim.LoxTempl;

public interface IAmActuator : IAcceptData, IAmDevice
{
    void ConnectProxy(LoxOutputRef proxy);
}
