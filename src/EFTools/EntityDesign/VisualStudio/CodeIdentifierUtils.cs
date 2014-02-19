// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.CSharp;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.VisualBasic;
    using System.Diagnostics.CodeAnalysis;

    internal class CodeIdentifierUtils
    {
        private readonly VisualStudioProjectSystem _applicationType;
        private readonly LangEnum _projectLanguage;

        private static readonly Regex _identifierRegex =
            new Regex(@"[^\p{Lu}^\p{Ll}^\p{Lt}^\p{Lm}^\p{Lo}^\p{Nl}^\p{Nd}^\p{Mn}^\p{Mc}^\p{Cf}^\p{Pc}]");

        public CodeIdentifierUtils(VisualStudioProjectSystem applicationType, LangEnum projectLanguage)
        {
            _applicationType = applicationType;
            _projectLanguage = projectLanguage;
        }

        public bool IsValidIdentifier(string identifier)
        {
            if (_applicationType == VisualStudioProjectSystem.Website)
            {
                // for WebSite projects we should check with both languages (C# and VB)
                // because they can use both C# and VB code
                return IsValidIdentifier(identifier, LangEnum.CSharp) && IsValidIdentifier(identifier, LangEnum.VisualBasic);
            }

            return IsValidIdentifier(identifier, _projectLanguage);
        }

        private static bool IsValidIdentifier(string identifier, LangEnum langEnum)
        {
            using (var provider = CreateCodeDomProvider(langEnum))
            {
                return provider.IsValidIdentifier(identifier);
            }
        }

        public string CreateValidIdentifier(string sourceIdentifier)
        {
            if (IsValidIdentifier(sourceIdentifier))
            {
                return sourceIdentifier;
            }

            var tempIdentifier = _identifierRegex.Replace(sourceIdentifier, string.Empty);
            if (tempIdentifier.Length == 0 || (!char.IsLetter(tempIdentifier[0]) && tempIdentifier[0] != '_'))
            {
                tempIdentifier = "_" + tempIdentifier;
            }

            if (_applicationType == VisualStudioProjectSystem.Website)
            {
                // WebSite projects can use both C# and VB code so we need to create an identifier that is valid for both
                // note that provider.CreateValidIdentifier basically prepends keywords with '_' so the result for one
                // language will not be an identifier that is invalid for the other language
                return CreateValidIdentifier(
                    CreateValidIdentifier(tempIdentifier, LangEnum.CSharp),
                    LangEnum.VisualBasic);
            }

            return CreateValidIdentifier(tempIdentifier, _projectLanguage);
        }

        private static string CreateValidIdentifier(string identifier, LangEnum langEnum)
        {
            using (var provider = CreateCodeDomProvider(langEnum))
            {
                return provider.CreateValidIdentifier(identifier);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by upstream callers")]
        private static CodeDomProvider CreateCodeDomProvider(LangEnum langEnum)
        {
            return langEnum == LangEnum.VisualBasic
                ? (CodeDomProvider)new VBCodeProvider()
                : new CSharpCodeProvider();
        }
    }
}
