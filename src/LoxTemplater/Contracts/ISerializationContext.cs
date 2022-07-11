using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Enyim.LoxTempl;

internal interface ISerializationContext
{
    bool TryLoad(XElement e, [NotNullWhen(true)] out LoxBase? retval);
    bool TryLoad<T>(XElement e, [NotNullWhen(true)] out T? retval) where T : LoxBase;

    void Write(LoxBase o, XElement target);

    XElement Create<T>();
    IEnumerable<XElement> Elements<T>(XElement e);
}
