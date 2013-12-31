// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.CodeGeneration.Extensions;

    /// <summary>
    /// Helper methods for generating code.
    /// </summary>
    public abstract class CodeHelper
    {
        private static readonly string[] _importedNamespaces = new[]
            {
                "System",
                "System.Data.Entity.Spatial"
            };

        /// <summary>
        /// Gets the provider for the language of the generated code.
        /// </summary>
        protected abstract CodeDomProvider CodeProvider { get; }

        /// <summary>
        /// Gets the type identifier for the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The identifier.</returns>
        public string Type(EntityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            return Identifier(container.Name);
        }

        /// <summary>
        /// Gets the type identifier for the specified type.
        /// </summary>
        /// <param name="edmType">The type.</param>
        /// <returns>The identifier.</returns>
        public string Type(EdmType edmType)
        {
            if (edmType == null)
            {
                throw new ArgumentNullException("edmType");
            }

            return Identifier(edmType.Name);
        }

        /// <summary>
        /// Gets the type of the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The type.</returns>
        public string Type(EdmProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            var clrType = property.PrimitiveType.ClrEquivalentType;
            var type = StripQualifiers(CodeProvider.GetTypeOutput(new CodeTypeReference(clrType)));

            if (clrType.IsValueType && property.Nullable)
            {
                return type + "?";
            }

            return type;
        }

        /// <summary>
        /// Gets the type of the specified navigation property.
        /// </summary>
        /// <param name="navigationProperty">The navigation property.</param>
        /// <returns>The type.</returns>
        public string Type(NavigationProperty navigationProperty)
        {
            if (navigationProperty == null)
            {
                throw new ArgumentNullException("navigationProperty");
            }

            var toEndMember = navigationProperty.ToEndMember;
            var type = Identifier(toEndMember.GetEntityType().Name);

            if (toEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                return "ICollection" + TypeArgument(type);
            }

            return type;
        }

        /// <summary>
        /// Gets the property identifier for the specified entity set.
        /// </summary>
        /// <param name="entitySet">The entity set.</param>
        /// <returns>The identifier.</returns>
        public string Property(EntitySetBase entitySet)
        {
            if (entitySet == null)
            {
                throw new ArgumentNullException("entitySet");
            }

            return Identifier(entitySet.Name);
        }

        /// <summary>
        /// Gets the property identifier for the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The identifier.</returns>
        public string Property(EdmMember member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            return Identifier(member.Name);
        }

        /// <summary>
        /// Gets the data annotations attribute used to apply the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The attribute.</returns>
        public string Attribute(IAttributeConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return Attribute(configuration.GetAttributeBody(this));
        }

        /// <summary>
        /// Gets the Code First Fluent API method chain used to apply the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The method chain.</returns>
        public string MethodChain(IFluentConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return configuration.GetMethodChain(this);
        }

        internal string Literal(IEnumerable<string> values)
        {
            Debug.Assert(values != null, "values is null.");

            if (!values.MoreThan(1))
            {
                return Literal(values.First());
            }

            return StringArray(values);
        }

        internal string Literal(string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(value), "value is null or empty.");

            return "\"" + value + "\"";
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal string Literal(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        internal string Lambda(IEnumerable<EdmProperty> properties)
        {
            Debug.Assert(properties != null, "properties is null.");

            if (!properties.MoreThan(1))
            {
                return Lambda(properties.First());
            }

            return BeginLambda("e") + AnonymousType(properties.Select(p => "e." + Property(p)));
        }

        internal string Lambda(EdmMember member)
        {
            Debug.Assert(member != null, "member is null.");

            return BeginLambda("e") + "e." + Property(member);
        }

        internal abstract string TypeArgument(string value);

        internal abstract string Literal(bool value);

        internal abstract string BeginLambda(string control);

        /// <summary>
        /// Gets an attribute with the specified body.
        /// </summary>
        /// <param name="attributeBody">The body of the attribute.</param>
        /// <returns>The attribute.</returns>
        protected abstract string Attribute(string attributeBody);

        /// <summary>
        /// Gets the string array literal for the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The literal.</returns>
        protected abstract string StringArray(IEnumerable<string> values);

        /// <summary>
        /// Gets the anonymous type lambda for the specified properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>The lambda.</returns>
        protected abstract string AnonymousType(IEnumerable<string> properties);

        private static string StripQualifiers(string fullTypeName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fullTypeName), "fullTypeName is null or empty.");

            var lastDotIndex = fullTypeName.LastIndexOf('.');
            if (lastDotIndex != -1)
            {
                var typeNamespace = fullTypeName.Substring(0, lastDotIndex);
                if (_importedNamespaces.Contains(typeNamespace))
                {
                    return fullTypeName.Substring(lastDotIndex + 1);
                }
            }

            return fullTypeName;
        }

        private string Identifier(string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(value), "value is null or empty.");

            return CodeProvider.CreateValidIdentifier(value);
        }
    }
}
