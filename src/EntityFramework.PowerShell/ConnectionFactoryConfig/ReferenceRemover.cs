// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Utilities;
    using System.Linq;
    using EnvDTE;
    using VSLangProj;

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

        public Reference TryFindReference(string identity, string publicKeyToken)
        {
            DebugCheck.NotEmpty(identity);
            DebugCheck.NotEmpty(publicKeyToken);

            var vsProject = _project.Object as VSProject;
            return vsProject == null
                       ? null
                       : vsProject.References
                                  .OfType<Reference>()
                                  .FirstOrDefault(
                                      r => r.StrongName
                                           && r.Identity == identity
                                           && publicKeyToken.Equals(r.PublicKeyToken, StringComparison.OrdinalIgnoreCase));
        }
    }
}
