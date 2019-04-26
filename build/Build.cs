using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using Nuke.Common;
using Nuke.Common.BuildServers;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
public class Build : NukeBuild
{
    const string NugetSource = "https://www.nuget.org/api/v2/package";

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version that is built. Needed for packaging. (format: x.x.x)")]
    readonly string Version = GitLab.Instance?.CommitTag?.Substring(1);

    readonly string NugetKey = EnvironmentInfo.Variable("NUGET_KEY");

    [Solution] readonly Solution Solution;

    IReadOnlyList<DotNetTool> _tools = new[]
    {
        new DotNetTool { PackageName = "coverlet.console", Executable = "coverlet", Version = "1.5.0" },
        new DotNetTool
            { PackageName = "dotnet-reportgenerator-globaltool", Executable = "reportgenerator", Version = "4.0.15" }
    };

    IEnumerable<Project> SourceProjects => Solution.AllProjects.Where(p => p.SolutionFolder?.Name == "src");
    IEnumerable<Project> TestProjects => Solution.AllProjects.Where(p => p.SolutionFolder?.Name == "test");

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "test";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath CoverageDirectory => RootDirectory / "coverage";
    AbsolutePath ToolsDirectory => RootDirectory / "tools";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
            EnsureCleanDirectory(CoverageDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));

            foreach (var tool in _tools.Where(t => !FileExists(ToolsDirectory / t.Executable)))
            {
                DotNetToolInstall(s => s
                    .SetToolInstallationPath(ToolsDirectory)
                    .SetPackageName(tool.PackageName)
                    .SetVersion(tool.Version));
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            Logger.Info("Cleanup coverage directory.");
            EnsureCleanDirectory(CoverageDirectory);
        })
        .Executes(() =>
        {
            foreach (var testProject in TestProjects)
            {
                Logger.Info($"Test & Coverage for {testProject.Name}");

                var testSettings = new DotNetTestSettings()
                    .EnableNoBuild()
                    .SetProjectFile(testProject)
                    .SetConfiguration(Configuration);

                CoverletTasks.Coverlet(s => s
                    .SetTargetSettings(testSettings)
                    .SetAssembly(testProject.Directory / $"bin/{Configuration}/{testProject.Name}.dll")
                    .SetFormat(CoverletOutputFormat.opencover)
                    .SetOutput(CoverageDirectory / $"cov-{testProject.Name}-{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss}.xml")
                    .SetToolPath(ToolsDirectory / "coverlet")
                    .AddExclude("[xunit.*]*", "[*.Test]*"));
            }
        });

    Target CoverageReport => _ => _
        .DependsOn(Test)
        .OnlyWhenDynamic(() => DirectoryExists(CoverageDirectory))
        .Executes(() =>
        {
            var reportDir = CoverageDirectory / "report";
            EnsureCleanDirectory(reportDir);
            ReportGeneratorTasks.ReportGenerator(s => s
                .SetToolPath(ToolsDirectory / "reportgenerator")
                .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                .SetReports(CoverageDirectory / "*.xml")
                .SetTargetDirectory(reportDir));

            using (Logger.Block("Coverage Report"))
            {
                Logger.Success(File.ReadAllText(CoverageDirectory / "report" / "Summary.txt"));
            }
        });

    Target Pack => _ => _
        .DependsOn(Clean, Compile)
        .Requires(() => Version != default)
        .Executes(() =>
        {
            Logger.Info($"Pack the projects for version {Version}");
            foreach(var project in SourceProjects)
            {
                DotNetPack(s => s
                    .EnableNoBuild()
                    .SetConfiguration(Configuration)
                    .SetProject(project)
                    .SetOutputDirectory(ArtifactsDirectory)
                    .SetVersion(Version)
                    .SetFileVersion(Version)
                    .SetAssemblyVersion(Version));
            }
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetKey != default)
        .Executes(() =>
        {
            Logger.Info("Publish Packages");

            foreach (var package in ArtifactsDirectory.GlobFiles("*.nupkg"))
            {
                DotNetNuGetPush(s => s
                    .SetApiKey(NugetKey)
                    .SetSource(NugetSource)
                    .SetTargetPath(package));
            }
        });
}
