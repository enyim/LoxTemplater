using System.Xml.Linq;

namespace Enyim.LoxTempl;

internal interface IXElementMatcher
{
    bool IsMatch(XElement e);
    IEnumerable<XElement> SelectElements(XNode e);
    XElement Create();
}
