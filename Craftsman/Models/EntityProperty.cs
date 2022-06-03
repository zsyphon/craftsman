﻿using System.Collections.Generic;
using Craftsman.Helpers;

namespace Craftsman.Models
{
    public class EntityProperty
    {
        private bool _isRequired = false;
        private bool _canManipulate = true;
        private string _type = "string";
        private string _name;

        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name
        {
            get => _name.UppercaseFirstLetter();
            set => _name = value;
        }

        /// <summary>
        /// Type of property (e.g. string, int, DateTime?, etc.)
        /// </summary>
        public string Type
        {
            get => _type;
            set => _type = Utilities.PropTypeCleanup(value);
        }

        /// <summary>
        /// Determines if the property will be filterable by the API
        /// </summary>
        public bool CanFilter { get; set; } = false;

        /// <summary>
        /// Determines if the property will be sortable by the API
        /// </summary>
        public bool CanSort { get; set; } = false;

        /// <summary>
        /// Determines if the property can be manipulated when creating or updating the associated entity
        /// </summary>
        public bool CanManipulate
        {
            get =>  _canManipulate;
            set => _canManipulate = IsForeignKey ? false : value;
        }

        /// <summary>
        /// Denotes a required field in the database
        /// </summary>
        public bool IsRequired
        {
            get => _isRequired;
            set => _isRequired = value;
        }

        /// <summary>
        /// Designates the property as a foreign key for the entity
        /// </summary>
        public bool IsForeignKey
        {
            get => !string.IsNullOrEmpty(ForeignEntityName)
                   || IsMany;
        }

        public bool IsPrimativeType
        {
            get 
            {
                var rawType = Type.ToLower().Trim().Replace("?", "");
                return rawType is "string" 
                    or "byte"
                    or "sbyte"
                    or "short"
                    or "ushort"
                    or "int" 
                    or "nint" 
                    or "uint" 
                    or "nuint" 
                    or "long" 
                    or "ulong" 
                    or "double"
                    or "float" 
                    or "decimal" 
                    or "char" 
                    or "bool" 
                    or "dateonly"
                    or "timeonly" 
                    or "datetime" 
                    or "object" 
                    or "guid";
            }
        }

        public bool IsMany
        {
            get => Type.StartsWith("Collection<")
                   || Type.StartsWith("ICollection<")
                   || Type.StartsWith("IEnumerable<")
                   || Type.StartsWith("Enumerable<")
                   || Type.StartsWith("Hashset<")
                   || Type.StartsWith("Dictionary<")
                   || Type.StartsWith("IDictionary<")
                   || Type.StartsWith("List<");
        }

        public bool IsJSON 
        {
            get => Type.ToLower().Trim().Replace("?", "").Equals("json");
        }

        public bool IsJSONObject 
        {
            get => Type.ToLower().Trim().Replace("?", "").Equals("jsonobject");
        }

         public bool IsJSONObjectList
        {
            get => Type.ToLower().Trim().Replace("?", "").Equals("jsonobjectlist");
        }

        /// <summary>
        /// Captures the name of the entity this property is linked to as a foreign key.
        /// </summary>
        public string ForeignEntityName { get; set; }

        private string _plural;
        /// <summary>
        /// The plural value for the foreign entity, if applicable. Defaults to ForeignEntityName with an appended 's'.
        /// </summary>
        public string ForeignEntityPlural
        {
            get => _plural ?? $"{ForeignEntityName}s";
            set => _plural = value;
        }

        /// <summary>
        /// Default value for this property
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Database field name to use when it doesn't match the property name
        /// </summary>
        public string ColumnName { get; set; }

        public static EntityProperty GetPrimaryKey()
        {
            return new()
               {
                   ColumnName = "id",
                   IsRequired = true,
                   CanManipulate = false,
                   Name = "Id",
                   Type = "Guid"
               };
        }

                /// <summary>
        /// List of properties associated to the entity
        /// </summary>
        public List<EntityProperty> Properties { get; set; } = new List<EntityProperty>();
    }
}