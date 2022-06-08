using System;
using System.Text;
using NJsonSchema;
using NJsonSchema.CodeGeneration;

namespace Craftsman.Builders.Dtos
{
    using System.Linq;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json.Linq;
    using NJsonSchema;
    using System.Threading.Tasks;
    using System;
    using NJsonSchema.CodeGeneration.CSharp;
    using System.IO.Abstractions;
    using NJsonSchema.Generation;

    public static class DtoBuilder
    {
        public static async Task CreateDtosAsync(string solutionDirectory, Entity entity, string projectBaseName, string namingConvention)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);



            CreateDtoFile(solutionDirectory, entity, Dto.Read, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Manipulation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Creation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Update, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.ReadParamaters, projectBaseName);
            var schemas = entity.Properties.Where(e => e.IsJsonSchema).ToList();
            if (schemas != null && schemas.Count() > 0)
            {
                foreach (var schema in schemas)
                {
                    try
                    {
                        var fileClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, $"{schema.Name}.cs", entity.Name, projectBaseName);

                        var fileData = File.ReadAllText(schema.Schema);
                        var schemaData = JsonSchema.FromJsonAsync(fileData).Result;
                        CSharpGeneratorSettings settings = null;
                        if (namingConvention.ToLower().Equals("snakecase"))
                        {
                            settings = new CSharpGeneratorSettings
                            {
                                Namespace = $"{fileClassPath.ClassNamespace}",
                                JsonLibrary = CSharpJsonLibrary.SystemTextJson,
                                PropertyNameGenerator = new SnakePropertyNameGenerator(),
                                JsonConverters = new string [] {" Newtonsoft.Json.DateTimeZoneHandling.Local"}
                            };
                        } else {
                            settings = new CSharpGeneratorSettings
                            {
                                Namespace = $"{fileClassPath.ClassNamespace}",
                                JsonLibrary = CSharpJsonLibrary.SystemTextJson
                            };
                        }
                        //DefaultPropertyNameHandling
                        var generator = new CSharpGenerator(schemaData, settings);

                        var file = generator.GenerateFile();

                        using (FileStream fs = File.Create(fileClassPath.FullClassPath))
                        {
                            fs.Write(Encoding.UTF8.GetBytes(file));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        public static string GetDtoFileText(string solutionDirectory, ClassPath classPath, Entity entity, Dto dto, string projectBaseName)
        {
            if (dto == Dto.ReadParamaters)
                return DtoFileTextGenerator.GetReadParameterDtoText(solutionDirectory, classPath.ClassNamespace, entity, dto, projectBaseName);
            else
                return DtoFileTextGenerator.GetDtoText(classPath, entity, dto);
        }

        public static void CreateDtoFile(string solutionDirectory, Entity entity, Dto dto, string projectBaseName)
        {
            var dtoFileName = $"{Utilities.GetDtoName(entity.Name, dto)}.cs";
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entity.Name, projectBaseName);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetDtoFileText(solutionDirectory, classPath, entity, dto, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }


        public static string ToSnakeCase(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (text.Length < 2)
            {
                return text;
            }
            var sb = new StringBuilder();
            sb.Append(char.ToLower(text[0]));
            for (int i = 1; i < text.Length; ++i)
            {
                char c = text[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLower(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

}
class SnakePropertyNameGenerator : IPropertyNameGenerator
{
    public string Generate(JsonSchemaProperty property)
    {
        return Craftsman.Builders.Dtos.DtoBuilder.ToSnakeCase(property.Name);
    }

}

