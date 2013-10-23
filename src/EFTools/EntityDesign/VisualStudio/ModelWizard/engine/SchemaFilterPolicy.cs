// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    internal struct SchemaFilterPolicy
    {
        private readonly ObjectFilterPolicy _tables;
        private readonly ObjectFilterPolicy _views;
        private readonly ObjectFilterPolicy _sprocs;

        internal ObjectFilterPolicy Tables
        {
            get { return _tables; }
        }

        internal ObjectFilterPolicy Views
        {
            get { return _views; }
        }

        internal ObjectFilterPolicy Sprocs
        {
            get { return _sprocs; }
        }

        internal SchemaFilterPolicy(ObjectFilterPolicy tables, ObjectFilterPolicy views, ObjectFilterPolicy sprocs)
        {
            _tables = tables;
            _views = views;
            _sprocs = sprocs;
        }

        // Edmx files that aren't by-ref only take a one time snapshot, so we just pick whichever policy gives us the smallest changeset.
        internal static SchemaFilterPolicy GetByValEdmxPolicy()
        {
            return new SchemaFilterPolicy(ObjectFilterPolicy.Optimal, ObjectFilterPolicy.Optimal, ObjectFilterPolicy.Optimal);
        }
    }
}
