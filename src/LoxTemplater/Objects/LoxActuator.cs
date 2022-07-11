namespace Enyim.LoxTempl;

public abstract class LoxActuator : LoxConnectable, IAmActuator
{
    protected LoxOutputRef Proxy { get; private set; }

    [FromAttribute("IName")]
    public string IName { get; set; }

    public virtual void ConnectProxy(LoxOutputRef proxy)
    {
        Proxy = proxy;
    }

    public void Accept(LoxConnector incoming)
    {
        if (Proxy == null) throw new InvalidOperationException($"{this} must be connected to a {nameof(LoxOutputRef)} first");

        Proxy.Connectors.ByName("AI").Accept(incoming);
    }
}
