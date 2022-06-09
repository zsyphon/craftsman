namespace Craftsman.Builders.Projects;

using Helpers;
using Services;

public class SharedKernelCsProjBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public SharedKernelCsProjBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateSharedKernelCsProj(string solutionDirectory)
    {
        var classPath = ClassPathHelper.SharedKernelProjectClassPath(solutionDirectory);
        var fileText = GetMessagesCsProjFileText();
        _utilities.CreateFile(classPath, fileText);
    }

    public static string GetMessagesCsProjFileText()
    {
        return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""FluentValidation"" Version=""10.4.0"" />
     <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
    <PackageReference Include=""NJsonSchema"" Version=""10.7.2"" />
    <PackageReference Include=""System.ComponentModel.Annotations"" Version=""6.0.0-preview.4.21253.7"" />
    <PackageReference Include=""System.Text.Json"" Version=""7.0.0-preview.4.22229.4"" />
  </ItemGroup>

</Project>";
    }
}
