using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Enyim.LoxTempl;

internal sealed class AttributePropertyHandler<TInstance, TValue> : IPropertyHandler<TInstance>
    where TInstance : class
{
    private static readonly HashSet<Type> SupportedTypes = new(new[]
    {
        typeof(string),
        typeof(bool),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(int),
        typeof(long),
        typeof(uint),
        typeof(ulong),
        typeof(decimal),

        typeof(bool?),
        typeof(DateTime?),
        typeof(DateTimeOffset?),
        typeof(int?),
        typeof(long?),
        typeof(uint?),
        typeof(ulong?),
        typeof(decimal?)
    });

    private readonly FastProperty<TInstance, TValue> property;
    private readonly XName name;

    public AttributePropertyHandler(PropertyInfo property, FromAttributeAttribute attr)
    {
        Debug.Assert(property.PropertyType == typeof(TValue));

        if (!SupportedTypes.Contains(property.PropertyType))
            throw new NotSupportedException();

        this.property = new FastProperty<TInstance, TValue>(property);
        name = attr.Name;
    }

    public void Load(XElement source, TInstance instance, ISerializationContext context)
    {
        if (!property.CanWrite) return;

        var value = source.AttributeValue<TValue>(name);

        property.Set(instance, value);
    }

    public void Save(TInstance instance, XElement target, ISerializationContext context)
    {
        if (!property.CanRead) return;

        var value = property.Get(instance);

        if (Comparer<TValue>.Default.Compare(default, value) == 0)
        {
            target.SetAttributeValue(name, null);
        }
        else
        {
            target.SetAttributeValue(name, value);
        }
    }
}
