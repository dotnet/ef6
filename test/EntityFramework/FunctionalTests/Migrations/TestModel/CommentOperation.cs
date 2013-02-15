// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;

    internal class CommentOperation : MigrationOperation
    {
        public CommentOperation(string text, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Text = text;
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        public string Text { get; private set; }
    }
}
