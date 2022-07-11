namespace Enyim.LoxTempl;

public abstract class LoxSensor : LoxConnectable, IAmSensor
{
    protected LoxInputRef Proxy { get; private set; }

    [FromAttribute("IName")]
    public string IName { get; set; }

    public virtual void ConnectProxy(LoxInputRef proxy)
    {
        Proxy = proxy;
    }

    public void ConnectTo(LoxConnector incoming)
    {
        if (Proxy == null) throw new InvalidOperationException($"{this} must be connected to a {nameof(LoxInputRef)} first");

        incoming.Accept(Proxy.Connectors.ByName("AQ"));
    }
}
