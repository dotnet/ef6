' Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

Imports System.Data.Entity.Config
Imports FunctionalTests.TestHelpers

Namespace AdvancedPatternsVB

    <DbConfigurationType(GetType(FunctionalTestsConfiguration))> _
    Partial Friend Class AdvancedPatternsModelFirstContext

        Public Sub New(ByVal nameOrConnectionString As String)
            MyBase.New(nameOrConnectionString)
            MyBase.Configuration.LazyLoadingEnabled = False
        End Sub

    End Class

End Namespace

