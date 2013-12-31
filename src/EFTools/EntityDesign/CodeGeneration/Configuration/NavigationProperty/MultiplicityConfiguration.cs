// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents a model configuration to set the multiplicity of an association.
    /// </summary>
    public class MultiplicityConfiguration : IFluentConfiguration
    {
        /// <summary>
        /// Gets or sets the entity type of the left end of the association.
        /// </summary>
        public EntityType LeftEntityType { get; set; }

        /// <summary>
        /// Gets or sets the navigation property of the left end of the association.
        /// </summary>
        public NavigationProperty LeftNavigationProperty { get; set; }

        /// <summary>
        /// Gets or sets the navigation property of the right end of the association.
        /// </summary>
        public NavigationProperty RightNavigationProperty { get; set; }

        /// <inheritdoc />
        // TODO: Handle formatting elsewhere
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(LeftEntityType != null, "LeftEntityType is null.");
            Debug.Assert(LeftNavigationProperty != null, "LeftNavigationProperty is null.");
            Debug.Assert(RightNavigationProperty != null, "RightNavigationProperty is null.");

            var builder = new StringBuilder();
            builder.Append(".Entity");
            builder.Append(code.TypeArgument(code.Type(LeftEntityType)));
            builder.AppendLine("()");

            var rightMultiplicity = RightNavigationProperty.FromEndMember.RelationshipMultiplicity;

            switch (rightMultiplicity)
            {
                case RelationshipMultiplicity.Many:
                    builder.Append("                .HasMany(");
                    builder.Append(code.Lambda(LeftNavigationProperty));
                    builder.Append(")");
                    break;

                case RelationshipMultiplicity.One:
                    builder.Append("                .HasRequired(");
                    builder.Append(code.Lambda(LeftNavigationProperty));
                    builder.Append(")");
                    break;

                case RelationshipMultiplicity.ZeroOrOne:
                    builder.Append("                .HasOptional(");
                    builder.Append(code.Lambda(LeftNavigationProperty));
                    builder.Append(")");
                    break;

                default:
                    Debug.Fail("rightMultiplicity is not a valid RelationshipMultiplicity value.");
                    break;
            }

            builder.AppendLine();

            switch (LeftNavigationProperty.FromEndMember.RelationshipMultiplicity)
            {
                case RelationshipMultiplicity.Many:
                    builder.Append("                .WithMany(");
                    builder.Append(code.Lambda(RightNavigationProperty));
                    builder.Append(")");
                    break;

                case RelationshipMultiplicity.One:
                    Debug.Assert(rightMultiplicity != RelationshipMultiplicity.One, "rightMultiplicity is One.");
                    builder.Append("                .WithRequired(");
                    builder.Append(code.Lambda(RightNavigationProperty));
                    builder.Append(")");
                    break;

                case RelationshipMultiplicity.ZeroOrOne:
                    Debug.Assert(
                        rightMultiplicity != RelationshipMultiplicity.ZeroOrOne,
                        "rightMultiplicity is ZeroOrOne.");
                    builder.Append("                .WithOptional(");
                    builder.Append(code.Lambda(RightNavigationProperty));
                    builder.Append(")");
                    break;

                default:
                    Debug.Fail("LeftNavigationProperty.FromEndMember.RelationshipMultiplicity is not a valid RelationshipMultiplicity value.");
                    break;
            }

            return builder.ToString();
        }
    }
}
