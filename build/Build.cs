using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    const string NugetSource = "https://api.nuget.org/v3/index.json";

    public static int Main() => Execute<Build>(x => x.Test);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version number that is built.")] readonly string Version = "0.0.0";

    [CanBeNull] readonly string NugetKey = Environment.GetEnvironmentVariable("NUGET_KEY");

    [Solution] readonly Solution Solution;

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
        .Executes(() => DotNetTest(s => s
            .SetConfiguration(Configuration)
            .EnableNoBuild()
            .CombineWith(TestProjects, (ss, p) => ss
                .SetProjectFile(p))));

    Target Pack => _ => _
        .DependsOn(Clean)
        .Requires(() => !string.IsNullOrWhiteSpace(Version))
        .Executes(() =>
        {
            Logger.Info($"Pack the projects for version {Version}");
            DotNetPack(s => s
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetVersion(Version)
                .SetFileVersion(Version)
                .SetAssemblyVersion(Version)
                .SetInformationalVersion(Version)
                .CombineWith(SourceProjects, (ss, project) => ss
                    .SetProject(project)), Environment.ProcessorCount);
        });

    Target Publish => _ => _
        .Requires(() => !string.IsNullOrWhiteSpace(NugetKey))
        .Executes(() =>
        {
            Logger.Info("Publish Packages");
            DotNetNuGetPush(s => s
                    .SetApiKey(NugetKey)
                    .SetSource(NugetSource)
                    .CombineWith(ArtifactsDirectory.GlobFiles("*.nupkg"), (ss, file) => ss.SetTargetPath(file)),
                Environment.ProcessorCount);
        });
}
