using System.Xml.Linq;

namespace Centeva.RequestBehaviors.Tests;

public class PackageVersionConstraintsTests
{
    [Fact]
    public void MediatR_ShouldNotExceedVersion12()
    {
        // Arrange & Act
        var mediatRVersion = GetPackageVersion("MediatR");

        // Assert
        Assert.NotNull(mediatRVersion);
        Assert.True(mediatRVersion.Major == 12, 
            $"MediatR version must be 12.x (found {mediatRVersion}). " +
            "Version 13+ has a commercial license.");
    }

    private static Version? GetPackageVersion(string packageName)
    {
        var packagesPropsPath = Path.Combine(
            GetSolutionRoot(),
            "Directory.Packages.props");

        var document = XDocument.Load(packagesPropsPath);
        var versionString = document
            .Descendants("PackageVersion")
            .FirstOrDefault(x => x.Attribute("Include")?.Value == packageName)
            ?.Attribute("Version")?.Value;

        return versionString is not null ? Version.Parse(versionString) : null;
    }

    private static string GetSolutionRoot()
    {
        var directory = Directory.GetCurrentDirectory();
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "Directory.Packages.props")) ||
                Directory.GetFiles(directory, "*.slnx").Length != 0)
            {
                return directory;
            }
            directory = Directory.GetParent(directory)?.FullName;
        }
        throw new InvalidOperationException("Could not find solution root directory");
    }
}
