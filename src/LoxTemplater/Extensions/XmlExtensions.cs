using System.Xml.Linq;

using X = System.Linq.Expressions.Expression;

namespace Enyim.LoxTempl;

public static class XmlExtensions
{
    private static class XConvert<TSource, TValue>
    {
        public static readonly Func<TSource, TValue?> GetTypedValue = CompileLambda();

        private static Func<TSource, TValue?> CompileLambda()
        {
            var arg = X.Parameter(typeof(TSource));
            var lambda = X.Lambda<Func<TSource, TValue?>>(X.Convert(arg, typeof(TValue?)), arg);

            return lambda.Compile();
        }
    }

    public static T? As<T>(this XElement element)
    {
        return XConvert<XElement, T>.GetTypedValue(element);
    }

    public static T? As<T>(this XAttribute element)
    {
        return XConvert<XAttribute, T>.GetTypedValue(element);
    }

    public static T? AttributeValue<T>(this XElement element, XName name)
    {
        var attr = element.Attribute(name);

        return attr == null
                ? default
                : XConvert<XAttribute, T>.GetTypedValue(attr);
    }

    public static string? LoxId(this XElement self) => self.Attribute("U")?.Value;

    public static string? LoxType(this XElement self) => self.Attribute("Type")?.Value;

    public static XElement? TypedElement(this XContainer self, string type) => self.TypedElements(type).FirstOrDefault();

    public static IEnumerable<XElement> TypedElements(this XContainer self, string type) => self.Elements("C").Where(e => e.Attribute("Type")?.Value == type);

    public static XElement? TypedDescendant(this XContainer self, string type) => self.TypedDescendants(type).FirstOrDefault();

    public static IEnumerable<XElement> TypedDescendants(this XContainer self, string type) => self.Descendants("C").Where(e => e.Attribute("Type")?.Value == type);

    public static XContainer? ContainerByPath(this XDocument doc, params string[] types) => doc.Root?.Container(types);

    public static XContainer? Container(this XContainer start, params string[] types)
    {
        var current = start;

        foreach (var type in types)
        {
            var next = current.TypedElement(type);
            if (next == null) return null;

            current = next;
        }

        return current;
    }

    public static XContainer RequireContainer(this XDocument doc, params string[] path) => doc.ContainerByPath(path) ?? throw new InvalidOperationException($"Invalid project file, missing {path[^1]}");

}
