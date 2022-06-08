namespace Craftsman.Builders.Projects
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class SharedKernelCsProjBuilder
    {
        public static void CreateMessagesCsProj(string solutionDirectory, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.SharedKernelProjectClassPath(solutionDirectory);
            var fileText = GetMessagesCsProjFileText();
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetMessagesCsProjFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""FluentValidation"" Version=""10.3.6"" />
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
    <PackageReference Include=""NJsonSchema"" Version=""10.7.2"" />
    <PackageReference Include=""System.ComponentModel.Annotations"" Version=""6.0.0-preview.4.21253.7"" />
    <PackageReference Include=""System.Text.Json"" Version=""7.0.0-preview.4.22229.4"" />
  </ItemGroup>

</Project>";
        }
    }
}