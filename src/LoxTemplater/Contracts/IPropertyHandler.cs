using System.Xml.Linq;

namespace Enyim.LoxTempl;

internal interface IPropertyHandler<TInstance>
{
    void Load(XElement source, TInstance instance, ISerializationContext context);
    void Save(TInstance instance, XElement target, ISerializationContext context);
}
