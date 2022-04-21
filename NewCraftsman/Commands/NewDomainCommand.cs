namespace NewCraftsman.Commands;

using System.IO.Abstractions;
using Builders;
using Builders.Docker;
using Domain;
using Helpers;
using Services;
using Spectre.Console;
using Spectre.Console.Cli;

public class NewDomainCommand : Command<NewDomainCommand.Settings>
{
    private IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleWriter _consoleWriter;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;
    private readonly IDbMigrator _dbMigrator;
    private readonly IGitService _gitService;

    public NewDomainCommand(IAnsiConsole console,
        IFileSystem fileSystem,
        IConsoleWriter consoleWriter,
        ICraftsmanUtilities utilities,
        IScaffoldingDirectoryStore scaffoldingDirectoryStore, 
        IDbMigrator dbMigrator, 
        IGitService gitService)
    {
        _console = console;
        _fileSystem = fileSystem;
        _consoleWriter = consoleWriter;
        _utilities = utilities;
        _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        _dbMigrator = dbMigrator;
        _gitService = gitService;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<Filepath>")]
        public string Filepath { get; set; }
    }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        var rootDir = _utilities.GetRootDir();
        
        // TODO make injectable
        new FileParsingHelper(_fileSystem).RunInitialTemplateParsingGuards(rootDir);
        var domainProject = FileParsingHelper.ReadYamlString<DomainProject>(settings.Filepath);
        _consoleWriter.WriteLogMessage($"Your template file was parsed successfully");

        _scaffoldingDirectoryStore.SetSolutionDirectory(rootDir, domainProject.DomainName);
        CreateNewDomainProject(domainProject);

        _console.MarkupLine($"{Environment.NewLine}[bold yellow1]Your domain project is ready! Build something amazing. [/]");

        _consoleWriter.StarGithubRequest();
        return 0;
    }
    
    public void CreateNewDomainProject(DomainProject domainProject)
    {
        var solutionDirectory = _scaffoldingDirectoryStore.SolutionDirectory;
        _fileSystem.Directory.CreateDirectory(solutionDirectory);
        new SolutionBuilder(_fileSystem, _utilities, _consoleWriter).BuildSolution(solutionDirectory, domainProject.DomainName);
        
        // need this before boundaries to give them something to build against
        new DockerComposeBuilders(_utilities, _fileSystem).CreateDockerComposeSkeleton(solutionDirectory);
        new DockerComposeBuilders(_utilities, _fileSystem).AddJaegerToDockerCompose(solutionDirectory);
        // DockerBuilders.CreateDockerComposeDbSkeleton(solutionDirectory);
            
        //Parallel.ForEach(domainProject.BoundedContexts, (template) =>
        //    ApiScaffolding.ScaffoldApi(solutionDirectory, template, verbosity));
        foreach (var bc in domainProject.BoundedContexts)
            new ApiScaffoldingService(_console, _consoleWriter, _utilities, _scaffoldingDirectoryStore, _fileSystem).ScaffoldApi(solutionDirectory, bc);

        // auth server
        // if (domainProject.AuthServer != null)
        //     AddAuthServerCommand.AddAuthServer(solutionDirectory, domainProject.AuthServer);
        //     
        // // bff
        // if (domainProject.AuthServer != null)
        //     AddBffCommand.AddBff(domainProject.Bff, solutionDirectory);
        //
        // // messages
        // if (domainProject.Messages.Count > 0)
        //     AddMessageCommand.AddMessages(solutionDirectory, domainProject.Messages);
        
        // migrations
        _dbMigrator.RunDbMigrations(domainProject.BoundedContexts, solutionDirectory);
        
        //final
        new ReadmeBuilder(_utilities).CreateReadme(solutionDirectory, domainProject.DomainName);
        
        if (domainProject.AddGit)
            _gitService.GitSetup(solutionDirectory, domainProject.UseSystemGitUser);
    }
}