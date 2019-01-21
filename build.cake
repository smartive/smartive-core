///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

var coverageDirectory = MakeAbsolute(Directory("./coverage"));
var artifactsDirectory = MakeAbsolute(Directory("./artifacts"));

var version = GitLabCI.Environment.Build.Tag == string.Empty
    ? null
    : GitLabCI.Environment.Build.Tag.Substring(1);
var nugetKey = EnvironmentVariable("NUGET_KEY");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        Information("Clean up coverage directory");
        CleanDirectory(coverageDirectory);
        Information("Clean up artifacts directory");
        CleanDirectory(artifactsDirectory);
    });

#addin nuget:?package=Cake.Coverlet&version=2.1.2
Task("Test")
    .IsDependentOn("Clean")
    .DoesForEach(GetFiles("**/*.Test.csproj"), (file) =>
    {
        DotNetCoreTest(file.FullPath, new DotNetCoreTestSettings(), new CoverletSettings
            {
                CollectCoverage = true,
                CoverletOutputFormat = CoverletOutputFormat.opencover,
                CoverletOutputDirectory = Directory(@"./coverage/"),
                CoverletOutputName = $"cov-{file.GetFilenameWithoutExtension()}-{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss}.xml",
                Exclude = new System.Collections.Generic.List<string>{
                    "[xunit.*]*"
                }
            });
    });

#tool "nuget:?package=ReportGenerator&version=4.0.7.0"
Task("Coverage")
    .IsDependentOn("Test")
    .Does(() =>
    {
        DotNetCoreExecute(
            Context.Tools.Resolve("ReportGenerator.dll").FullPath,
            ProcessArgumentBuilder.FromString(
                "-reports:./coverage/*.xml -targetdir:./coverage/report -reporttypes:Badges;TextSummary;HTMLSummary"));
    })
    .Does(() =>
    {
        var summary = File("./coverage/report/Summary.txt");
        Information("Calculated, combined coverage:");
        Information(System.IO.File.ReadAllText(summary));
    });

Task("Pack")
    .WithCriteria(version != null, "Version must be set")
    .IsDependentOn("Clean")
    .DoesForEach(
        GetFiles("./**/*.csproj"), (file) =>
            {
                if(!file.FullPath.Contains("Smartive.Core") || file.FullPath.Contains(".Test"))
                {
                    return;
                }

                Information($"Build & Package {file.GetFilename()} in version {version}");
                DotNetCorePack(file.FullPath, new DotNetCorePackSettings
                {
                    OutputDirectory = artifactsDirectory,
                    Configuration = configuration,
                    MSBuildSettings = new DotNetCoreMSBuildSettings()
                        .SetVersion(version)
                        .SetFileVersion(version)
                });
            });

Task("Publish")
    .WithCriteria(nugetKey != null, "Nuget API Key must be present.")
    .IsDependentOn("Pack")
    .Does(() =>
        {
            Information($"Publish ./artifacts/*.nupkg to nuget under version {version}");
            DotNetCoreNuGetPush("./artifacts/*.nupkg", new DotNetCoreNuGetPushSettings
            {
                ApiKey = nugetKey,
                Source = "https://www.nuget.org/api/v2/package"
            });
        });

RunTarget(target);
