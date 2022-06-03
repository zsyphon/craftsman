using System.Linq;
namespace Craftsman.Builders.Dtos
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.Text;

    public static class DtoBuilder
    {
        public static void CreateDtos(string solutionDirectory, Entity entity, string projectBaseName)
        {
            // ****this class path will have an invalid FullClassPath. just need the directory
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);
            var jsonObjs = entity.Properties.Where(p => p.IsJSONObject || p.IsJSONObjectList).ToList();
            if (jsonObjs != null & jsonObjs.Count() > 0)
            {
                foreach (var obj in jsonObjs)
                {
                    //@TODO recur over sub json object properties
                    CreateJSONObjectDtoFile(solutionDirectory, obj, Dto.Read, projectBaseName, entity.Name);
                    var nestedJsonObjs = obj.Properties.Where(p => p.IsJSONObject || p.IsJSONObjectList).ToList();
                    if (jsonObjs != null & jsonObjs.Count() > 0)
                    {
                        foreach (var nObj in nestedJsonObjs)
                        {
                            CreateJSONObjectDtoFile(solutionDirectory, nObj, Dto.Read, projectBaseName, entity.Name);
                         }
                    }
                }
            }

            CreateDtoFile(solutionDirectory, entity, Dto.Read, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Manipulation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Creation, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.Update, projectBaseName);
            CreateDtoFile(solutionDirectory, entity, Dto.ReadParamaters, projectBaseName);
        }

        public static string GetDtoFileText(string solutionDirectory, ClassPath classPath, Entity entity, Dto dto, string projectBaseName)
        {
            if (dto == Dto.ReadParamaters)
                return DtoFileTextGenerator.GetReadParameterDtoText(solutionDirectory, classPath.ClassNamespace, entity, dto, projectBaseName);
            else
                return DtoFileTextGenerator.GetDtoText(classPath, entity, dto);
        }
        public static string GetEntityPropertyDtoFileText(string solutionDirectory, ClassPath classPath, EntityProperty entity, Dto dto, string projectBaseName)
        {
            if (dto == Dto.ReadParamaters)
                return DtoFileTextGenerator.GetReadParameterDtoEntityPropText(solutionDirectory, classPath.ClassNamespace, entity, dto, projectBaseName);
            else
                return DtoFileTextGenerator.GetPropertyDtoText(classPath, entity, dto);
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

        public static void CreateJSONObjectDtoFile(string solutionDirectory, EntityProperty entityProp, Dto dto, string projectBaseName, string parentEntityName)
        {
            var dtoFileName = $"{Utilities.GetDtoName(entityProp.Name, dto)}.cs";
            var classPath = ClassPathHelper.DtoEntityPropertyClassPath(solutionDirectory, dtoFileName, parentEntityName, projectBaseName);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetEntityPropertyDtoFileText(solutionDirectory, classPath, entityProp, dto, projectBaseName);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
        }
    }
}