using System.Xml.Linq;

namespace Enyim.LoxTempl;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class FromElementAttribute : Attribute
{
    public FromElementAttribute(string? name = null)
    {
        if (name != null)
        {
            Name = name;
        }
    }

    public XName? Name { get; }
}
