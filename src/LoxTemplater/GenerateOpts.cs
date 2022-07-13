using CommandLine;
using CommandLine.Text;

namespace Enyim.LoxTempl;

[Verb("generate", HelpText = "Generate pages based on the specified template")]
internal class GenerateOpts
{
    [Usage]
    public static IEnumerable<Example> Usage
    {
        get
        {
            yield return new Example("Generate a page for Attic from AC_Template", new GenerateOpts
            {
                ProjectPath = "path/to/the/project.Loxone",
                Rooms = new[] { "Attic" },
                TemplateName = "AC_Template",
                OutputPath = "path/to/saved/file.Loxone"
            });
        }
    }

    [Value(0, MetaName = "PATH", HelpText = "Path to the source project file", Required = true)]
    public string ProjectPath { get; set; }

    [Option('r', "rooms", Required = true, HelpText = "Which rooms to generate pages for. Use 'list rooms' to get the list of room names from the project.")]
    public IEnumerable<string> Rooms { get; set; }

    [Option('t', "template", Required = true, HelpText = "Name of the template page.")]
    public string TemplateName { get; set; }

    [Option("page-name", Required = false, HelpText = "Template for naming the generated pages. Use {Room} for the room name. E.g. \"Heating - {Room}\"")]
    public string PageNameTemplate { get; set; }

    [Option('o', "output", Required = false, HelpText = "Target path for saving the project. If not specified a generated file name will be used.")]
    public string OutputPath { get; set; }

    [Option('n', "dry-run", Required = false, HelpText = "If set, the output will not be saved. Can be used for validating the source project.")]
    public bool DryRun { get; set; }

    [Option("overwrite-pages", Required = false, HelpText = "If set, pages with the same name as the generated pages will be overwritten in the project file. Useful when reprocessing a project that contains both template and generated pages.")]
    public bool OverwritePages { get; set; }

    [Option("delete-template", Required = false, HelpText = "If set, the template page will be removed from the output project.")]
    public bool DeleteTemplate { get; set; }

    public void Validate()
    {
        if (!Rooms.Any()) throw new InvalidOperationException("At least a single room must be specified");
        if (!File.Exists(ProjectPath)) throw new FileNotFoundException("Cannot find project file", ProjectPath);

        if (String.IsNullOrEmpty(OutputPath))
        {
            var ts = DateTime.Now.ToString("yyyyMMdd-HHmmssfff");
            OutputPath = Path.ChangeExtension(ProjectPath, "." + ts + Path.GetExtension(ProjectPath));
        }
        else
        {
            if (!Directory.Exists(Path.GetDirectoryName(OutputPath))) throw new DirectoryNotFoundException("Invalid output path: " + OutputPath);
        }
    }
}
