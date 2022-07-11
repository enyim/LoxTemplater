using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Enyim.LoxTempl;

internal sealed class ElementPropertyHandler<TInstance, TValue> : IPropertyHandler<TInstance>
    where TInstance : class
    where TValue : LoxBase
{
    private readonly FastProperty<TInstance, TValue> property;
    private readonly XName? name;

    public ElementPropertyHandler(PropertyInfo property, FromElementAttribute attr)
    {
        Debug.Assert(property.PropertyType == typeof(TValue));

        this.property = new FastProperty<TInstance, TValue>(property);
        name = attr.Name;
    }

    public void Load(XElement source, TInstance instance, ISerializationContext context)
    {
        if (!property.CanWrite) return;

        var element = name == null
                        ? context.Elements<TValue>(source).FirstOrDefault()
                        : source.Element(name);

        if (element != null)
        {
            if (!context.TryLoad<TValue>(element, out var value))
            {
                throw new InvalidOperationException($"Cannot deserialize {element} as {typeof(TValue)}");
            }

            property.Set(instance, value);
        }
    }

    public void Save(TInstance instance, XElement target, ISerializationContext context)
    {
        if (!property.CanRead) return;

        var value = property.Get(instance);

        if (value != null && value != default(TValue))
        {
            var element = (value as IXmlBound)?.Element;
            if (element == null)
            {
                element = name == null ? context.Create<TValue>() : new XElement(name);
                target.Add(element);
            }

            context.Write(value, element);
        }
    }
}
