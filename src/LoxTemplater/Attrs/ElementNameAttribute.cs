using System.Xml.Linq;
using System.Xml.XPath;

namespace Enyim.LoxTempl;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ElementNameAttribute : Attribute, IXElementMatcher
{
    private readonly string selector;

    public ElementNameAttribute(string name)
    {
        Name = name;
        selector = $".//{name}";
    }

    public XName Name { get; }

    bool IXElementMatcher.IsMatch(XElement e) => e.Name == Name;
    public IEnumerable<XElement> SelectElements(XNode e) => e.XPathSelectElements(selector);
    public XElement Create() => new XElement(Name);
}
