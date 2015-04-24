using System;
using System.Linq;

namespace MediatR.Extras
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Get full type name with full namespace names
        /// </summary>
        /// <param name="type">
        /// The type to get the C# name for (may be a generic type or a nullable type).
        /// </param>
        /// <param name="namespaceQualified"> 
        /// </param>
        /// <returns>
        /// Full type name, fully qualified namespaces
        /// </returns>
        public static string CSharpName(this Type type, bool namespaceQualified = false)
        {
            Type nullableType = Nullable.GetUnderlyingType(type);
            string nullableText;
            if (nullableType != null)
            {
                type = nullableType;
                nullableText = "?";
            }
            else
            {
                nullableText = string.Empty;
            }

            if (type.IsGenericType)
            {
                return string.Format(
                    "{0}(Of({1})){2}",
                    type.Name.Substring(0, type.Name.IndexOf('`')),
                    string.Join(", ", type.GetGenericArguments().Select(ga => ga.CSharpName(namespaceQualified))),
                    nullableText);
            }

            switch (type.Name)
            {
                case "String":
                    return "string";
                case "Int32":
                    return "int" + nullableText;
                case "Decimal":
                    return "decimal" + nullableText;
                case "Object":
                    return "object" + nullableText;
                case "Void":
                    return "void" + nullableText;
                default:
                    return (namespaceQualified ? type.FullName : type.Name) + nullableText;
            }
        }
    }
}