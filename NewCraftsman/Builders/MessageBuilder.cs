﻿namespace NewCraftsman.Builders
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Text;
    using Domain;
    using Helpers;
    using Services;

    public class MessageBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public MessageBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateMessage(string solutionDirectory, Message message)
        {
            var classPath = ClassPathHelper.MessagesClassPath(solutionDirectory, $"{message.Name}.cs");
            var fileText = GetMessageFileText(classPath.ClassNamespace, message);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetMessageFileText(string classNamespace, Message message)
        {
            var propString = MessagePropBuilder(message.Properties);

            return @$"namespace {classNamespace}
{{
    using System;
    using System.Text;

    public interface {message.Name}
    {{
        {propString}
    }}

    // add-on property marker - Do Not Delete This Comment
}}";
        }

        public static string MessagePropBuilder(List<MessageProperty> props)
        {
            var propString = "";
            for (var eachProp = 0; eachProp < props.Count; eachProp++)
            {
                string newLine = eachProp == props.Count - 1 ? "" : $"{Environment.NewLine}{Environment.NewLine}";
                propString += $@"    {props[eachProp].Type} {props[eachProp].Name} {{ get; set; }}{newLine}";
            }

            return propString;
        }
    }
}