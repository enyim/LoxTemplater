namespace Enyim.LoxTempl;

public interface IHaveData : IConnectable
{
    void ConnectTo(LoxConnector outgoing);
}
