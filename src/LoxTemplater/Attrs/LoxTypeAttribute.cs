using System.Xml.Linq;
using System.Xml.XPath;

namespace Enyim.LoxTempl;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class LoxTypeAttribute : Attribute, IXElementMatcher
{
    private static readonly XName AttrName = "Type";

    private readonly string selector;

    public LoxTypeAttribute(string type)
    {
        Type = type;
        selector = $".//C[@{AttrName}='{type}']";
    }

    public string Type { get; }

    bool IXElementMatcher.IsMatch(XElement e) => e.Attribute(AttrName)?.Value == Type;
    public IEnumerable<XElement> SelectElements(XNode e) => e.XPathSelectElements(selector);
    public XElement Create() => new XElement("C", new XAttribute(AttrName, Type));
}
