using CommandLine;

namespace Enyim.LoxTempl;

[Verb("list", HelpText = "List entities in the project file")]
internal class ListOpts
{
    [Value(0, HelpText = "What to list (" + nameof(ListWhat.Categories) + "|" + nameof(ListWhat.Places) + "|" + nameof(ListWhat.Pages) + ")", Required = true)]
    public ListWhat What { get; set; }
    [Value(1, MetaName = "PATH", HelpText = "Path to the source project file", Required = true)]
    public string ProjectPath { get; set; }

    public enum ListWhat { Categories, Places, Pages }
}
