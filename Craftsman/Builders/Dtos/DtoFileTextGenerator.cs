﻿namespace Craftsman.Builders.Dtos
{
    using Craftsman.Enums;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using Craftsman.Models.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class DtoFileTextGenerator
    {
        public static string GetReadParameterDtoText(string solutionDirectory, string classNamespace, Entity entity, Dto dto, string projectBaseName)
        {
            var sharedDtoClassPath = ClassPathHelper.SharedDtoClassPath(solutionDirectory, "");

            return @$"namespace {classNamespace}
{{
    using {sharedDtoClassPath.ClassNamespace};

    public class {Utilities.GetDtoName(entity.Name, dto)} : BasePaginationParameters
    {{
        public string Filters {{ get; set; }}
        public string SortOrder {{ get; set; }}
    }}
}}";
        }

        public static string GetDtoText(IClassPath dtoClassPath, Entity entity, Dto dto)
        {
            var propString = dto is Dto.Read ? $@"    public Guid Id {{ get; set; }}{Environment.NewLine}" : "";
            propString += DtoPropBuilder(entity.Properties, dto);
            if (dto is Dto.Update or Dto.Creation)
                propString = "";

            var abstractString = dto == Dto.Manipulation ? $"abstract " : "";

            var inheritanceString = "";
            if (dto is Dto.Creation or Dto.Update)
                inheritanceString = $": {Utilities.GetDtoName(entity.Name, Dto.Manipulation)}";

            return @$"namespace {dtoClassPath.ClassNamespace}
{{
    using System.Collections.Generic;
    using System;

    public {abstractString}class {Utilities.GetDtoName(entity.Name, dto)} {inheritanceString}
    {{
    {propString}
    }}
}}";
        }

        public static string DtoPropBuilder(List<EntityProperty> props, Dto dto)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                if (!props[eachProp].CanManipulate && dto == Dto.Manipulation)
                    continue;
                if (props[eachProp].IsForeignKey && props[eachProp].IsMany)
                    continue;

                var guidDefault = dto == Dto.Creation && props[eachProp].Type.IsGuidPropertyType()
                    ? " = Guid.NewGuid();"
                    : "";

                string newLine = eachProp == props.Count - 1 ? "" : Environment.NewLine;
               
                if (props[eachProp].IsJsonSchema)
                    propString += $@"        public {props[eachProp].Name} {props[eachProp].Name} {{ get; set; }}{guidDefault}{newLine}";
                else
                    propString += $@"        public {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{guidDefault}{newLine}";
            }

            return propString;
        }
    }
}
