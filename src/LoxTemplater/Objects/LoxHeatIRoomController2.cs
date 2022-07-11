namespace Enyim.LoxTempl;

[LoxType("HeatIRoomController2")]
public class LoxHeatIRoomController2 : LoxVisualObject
{
    private const string T = "ϑ";

    private static readonly (string XmlName, string UIName)[] OutputPorts = new[]
    {
        ("AQt", $"{T}t"),
        ("AQhm",  "HCm"),

        ("AQh",  "H"),
        ("AQc",  "C"),
        ("AQhc",  "HC"),

        ("AQh1", "H1"),
        ("AQh2", "H2"),
        ("AQh3", "H3"),
        ("AQh4", "H4"),

        ("AQc1", "C1"),
        ("AQc2", "C2"),
        ("AQc3", "C3"),
        ("AQc4", "C4"),

        ("AQhc1", "HC1"),
        ("AQhc2", "HC2"),
        ("AQhc3", "HC3"),
        ("AQhc4", "HC4"),
    };

    private static readonly (string XmlName, string UIName)[] InputPorts = new[]
    {
        ("Temp", $"{T}c")
    };

    public IEnumerable<OwnedIORef> GetIORefs()
    {
        var ios = Connectors.ToDictionary(c => c.K);

        foreach (var (XmlName, UIName) in OutputPorts)
        {
            if (ios.TryGetValue(XmlName, out var io))
            {
                yield return new(this, new BlockConnectorAsDataProvider(this, io, UIName));
            }
        }

        foreach (var (XmlName, UIName) in InputPorts)
        {
            if (ios.TryGetValue(XmlName, out var io))
            {
                yield return new(this, new BlockConnectorAsDataReceiver(this, io, UIName));
            }
        }
    }

}

internal abstract class FakeConnectable : IConnectable
{
    protected FakeConnectable(LoxConnectable owner)
    {
        Owner = owner;
    }

    protected LoxConnectable Owner { get; }

    public abstract string Title { get; }
    public IMeta Meta => Owner.Meta;
}

/// <summary>
/// Allows connecting an input connector of a Function Block to an output template parameter (InputRef of MemoryFlag)
/// </summary>
internal class BlockConnectorAsDataReceiver : FakeConnectable, IAcceptData
{
    private readonly LoxConnector output;

    public BlockConnectorAsDataReceiver(LoxConnectable owner, LoxConnector output, string? title = null)
        : base(owner)

    {
        this.output = output;
        Title = title ?? output.K;
    }

    public override string Title { get; }

    public void Accept(LoxConnector incoming)
    {
        output.Accept(incoming, true);
    }

    public override string ToString() => $"{Owner}:{Title}:{output}";
}

/// <summary>
/// Allows connecting an output connector of a Function Block to an input template parameter (Memory Flag)
/// </summary>
internal class BlockConnectorAsDataProvider : FakeConnectable, IHaveData
{
    private readonly LoxConnector input;

    public BlockConnectorAsDataProvider(LoxConnectable owner, LoxConnector input, string? title = null)
        : base(owner)
    {
        this.input = input;
        Title = title ?? input.K;
    }

    public override string Title { get; }

    public void ConnectTo(LoxConnector incoming)
    {
        incoming.Accept(input, true);
    }

    public override string ToString() => $"{Owner}:{Title}:{input}";
}
