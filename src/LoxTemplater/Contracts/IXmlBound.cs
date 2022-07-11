using System.Xml.Linq;

namespace Enyim.LoxTempl;

public interface IXmlBound
{
    XElement? Element { get; }
}
