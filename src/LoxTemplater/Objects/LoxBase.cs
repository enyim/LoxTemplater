using System.Xml.Linq;

namespace Enyim.LoxTempl;

public class LoxBase : IXmlBound, ISerializationCallback<BeforeLoad>
{
    protected internal XElement? Element { get; protected set; }

    XElement? IXmlBound.Element => Element;

    void ISerializationCallback<BeforeLoad>.Handle(BeforeLoad eventArgs)
    {
        Element = eventArgs.Element;
    }

    internal void SetElement(XElement e)
    {
        if (Element != null) throw new NotSupportedException();

        Element = e;
    }

    internal virtual XElement CreateElement()
    {
        if (Element != null) throw new NotSupportedException();

        Element = new XElement("C");

        return Element;
    }
}
