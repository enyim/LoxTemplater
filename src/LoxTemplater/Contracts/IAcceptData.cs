namespace Enyim.LoxTempl;

public interface IAcceptData : IConnectable
{
    void Accept(LoxConnector incoming);
}
