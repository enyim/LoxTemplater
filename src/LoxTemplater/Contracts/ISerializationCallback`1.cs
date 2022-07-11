using System.Xml.Linq;

namespace Enyim.LoxTempl;

public interface ISerializationCallback<TEvent>
{
    void Handle(TEvent eventArgs);
}

public record struct BeforeSave(XElement Element);

public record struct AfterSave(XElement Element);

public record struct BeforeLoad(XElement Element);

public record struct AfterLoad(XElement Element);
