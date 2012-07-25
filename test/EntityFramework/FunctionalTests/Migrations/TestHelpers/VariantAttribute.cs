// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VariantAttribute : Attribute
    {
        public VariantAttribute(DatabaseProvider provider, ProgrammingLanguage language)
        {
            DatabaseProvider = provider;
            ProgrammingLanguage = language;
        }

        public DatabaseProvider DatabaseProvider { get; private set; }
        public ProgrammingLanguage ProgrammingLanguage { get; private set; }
    }
}