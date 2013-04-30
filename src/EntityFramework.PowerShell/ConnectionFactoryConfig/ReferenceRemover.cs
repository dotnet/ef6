// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using EnvDTE;
    using VSLangProj;
    using VsWebSite;

    internal class ReferenceRemover
    {
        private const string SystemDataEntityIdentity = "System.Data.Entity";
        private const string SystemDataEntityPublicKeyToken = "b77a5c561934e089";

        private readonly Project _project;

        public ReferenceRemover(Project project)
        {
            DebugCheck.NotNull(project);

            _project = project;
        }

        public void TryRemoveSystemDataEntity()
        {
            TryRemoveReference(SystemDataEntityIdentity, SystemDataEntityPublicKeyToken);
        }

        public void TryRemoveReference(string identity, string publicKeyToken)
        {
            DebugCheck.NotEmpty(identity);
            DebugCheck.NotEmpty(publicKeyToken);

            var reference = TryFindReference(identity, publicKeyToken);
            if (reference != null)
            {
                reference.Remove();
            }
        }

        public dynamic TryFindReference(string identity, string publicKeyToken)
        {
            DebugCheck.NotEmpty(identity);
            DebugCheck.NotEmpty(publicKeyToken);

            var vsProject = _project.Object as VSProject;
            if (vsProject != null)
            {
                return vsProject.References
                           .OfType<Reference>()
                           .FirstOrDefault(
                               r => r.StrongName
                                    && r.Identity == identity
                                    && publicKeyToken.Equals(r.PublicKeyToken, StringComparison.OrdinalIgnoreCase));
            }

            var vsWebSite = _project.Object as VSWebSite;
            return vsWebSite == null
                ? null
                : vsWebSite.References.OfType<AssemblyReference>().FirstOrDefault(
                    ar =>
                    {
                        var r = new AssemblyName(ar.StrongName);
                        return r.Name == identity
                            && publicKeyToken.Equals(
                                r.GetPublicKeyToken().ToHexString(),
                                StringComparison.OrdinalIgnoreCase);
                    });
        }
    }
}
