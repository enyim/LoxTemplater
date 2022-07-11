namespace Enyim.LoxTempl;

public class LoxHaveMeta : LoxObject, IHaveMeta
{

    [FromElement("IoData")]
    public LoxMeta Meta { get; set; }

    IMeta IHaveMeta.Meta => Meta;
}

public interface IHaveMeta
{
    public IMeta Meta { get; }
}