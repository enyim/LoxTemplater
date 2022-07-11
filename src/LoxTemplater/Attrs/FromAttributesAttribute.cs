using System.Xml.Linq;

namespace Enyim.LoxTempl;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FromAttributeAttribute : Attribute
{
    public FromAttributeAttribute(string name)
    {
        Name = name;
    }

    public XName Name { get; }
}
