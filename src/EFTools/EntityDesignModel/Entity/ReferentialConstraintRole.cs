// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class ReferentialConstraintRole : PropertyRefContainer
    {
        private static string AttributeRole = "Role";
        private SingleItemBinding<AssociationEnd> _role;

        internal ReferentialConstraintRole(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }

        // For this class, EFTypeName property does not get used in a meaningful way
        // because ModelController sets the XElement name explicitly
        // It serves mainly for debugging / correctness
        internal override string EFTypeName
        {
            get
            {
                var referentialConstraint = Parent as ReferentialConstraint;
                if (referentialConstraint != null)
                {
                    if (referentialConstraint.Principal == this)
                    {
                        return ReferentialConstraint.ElementNamePrincipal;
                    }
                    else if (referentialConstraint.Dependent == this)
                    {
                        return ReferentialConstraint.ElementNameDependent;
                    }
                }

                return base.EFTypeName;
            }
        }

        internal SingleItemBinding<AssociationEnd> Role
        {
            get
            {
                if (_role == null)
                {
                    _role = new SingleItemBinding<AssociationEnd>(
                        this,
                        AttributeRole,
                        ReferentialConstraintRoleNameNormalizer);
                }
                return _role;
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeRole);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_role);
            _role = null;

            base.PreParse();
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            Role.Rebind();
            if (Role.Status == BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
        }

        internal static NormalizedName ReferentialConstraintRoleNameNormalizer(EFElement parent, string refName)
        {
            var role = parent as ReferentialConstraintRole;
            Debug.Assert(role != null, "role should not be null");

            var rc = role.Parent as ReferentialConstraint;
            Debug.Assert(rc != null, "role.Parent should be a ReferentialConstraint");

            var assoc = rc.Parent as Association;
            Debug.Assert(assoc != null, "referential constraint parent should be a Association");

            var symbol = new Symbol(assoc.EntityModel.NamespaceValue, assoc.LocalName.Value, refName);
            var normalizedName = new NormalizedName(symbol, null, null, refName);
            return normalizedName;
        }

        internal static NormalizedName ReferentialConstraintPropertyRefNameNormalizer(EFElement parent, string refName)
        {
            var pr = parent as PropertyRef;
            Debug.Assert(pr != null, "referential constraint parent should be a PropertyRef");

            var role = pr.Parent as ReferentialConstraintRole;
            Debug.Assert(role != null, "PropertyRef parent should be a ReferentialConstraintRole");

            Symbol symbol = null;

            if (role.Role.Status == BindingStatus.Known)
            {
                var end = role.Role.Target;
                Debug.Assert(end != null, "role.Role.Target should not be null");

                var isPrincipal = (role.Parent as ReferentialConstraint).Principal == role;
                if (isPrincipal)
                {
                    // For the principal end, the propertyref should still point at a key. If we are in an inheritance hierarchy,
                    // the key can only exist on the base type.

                    Symbol endTypeName;
                    var cet = end.Type.Target as ConceptualEntityType;
                    if (cet != null)
                    {
                        endTypeName = (end.Type.Status == BindingStatus.Known)
                                          ? cet.ResolvableTopMostBaseType.NormalizedName
                                          : end.Type.NormalizedName();
                    }
                    else
                    {
                        endTypeName = (end.Type.Status == BindingStatus.Known)
                                          ? end.Type.Target.NormalizedName
                                          : end.Type.NormalizedName();
                    }

                    symbol = new Symbol(endTypeName, refName);
                }
                else
                {
                    // Since we support FKs on the C-side, we need to look in the EntityType on the End and all base types for
                    // dependent properties. Because the given refname is only the local name, we will have to check
                    // the ArtifactSet for the resulting symbol, since there could be two properties with the same
                    // name within an inheritance hierarchy (it's a C-side error, but still designer-safe).

                    var artifactSet = parent.Artifact.ModelManager.GetArtifactSet(parent.Artifact.Uri);
                    Debug.Assert(artifactSet != null, "Could not find the ArtifactSet corresponding to the URI; was it renamed?");
                    if (artifactSet != null
                        && end.Type.Status == BindingStatus.Known)
                    {
                        var typesToSearch = new List<EntityType>();
                        typesToSearch.Add(end.Type.Target);

                        var cet = end.Type.Target as ConceptualEntityType;
                        if (cet != null)
                        {
                            typesToSearch.AddRange(cet.ResolvableBaseTypes);
                        }

                        foreach (var entityType in typesToSearch)
                        {
                            var endTypeName = entityType.NormalizedName;
                            var testSymbol = new Symbol(endTypeName, refName);

                            var item = artifactSet.LookupSymbol(testSymbol);
                            if (item != null)
                            {
                                symbol = testSymbol;
                                break;
                            }
                        }
                    }
                }
            }

            if (symbol == null)
            {
                symbol = new Symbol(refName);
            }

            return new NormalizedName(symbol, null, null, refName);
        }

        internal override SingleItemBinding<Property>.NameNormalizer GetNameNormalizerForPropertyRef()
        {
            return ReferentialConstraintPropertyRefNameNormalizer;
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }
                yield return Role;
            }
        }

        internal override DeleteEFElementCommand GetDeleteCommandForChild(PropertyRef pref)
        {
            return new DeleteReferentialConstraintPropertyRefCommand(pref);
        }
    }
}
