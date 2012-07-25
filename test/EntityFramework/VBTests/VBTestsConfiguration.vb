' Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
Imports System.Data.Entity.Config
Imports FunctionalTests.TestHelpers

Public Class VBTestsConfiguration
    Inherits DbConfigurationProxy

    Public Overrides Function ConfigurationToUse() As Type
        Return GetType(FunctionalTestsConfiguration)
    End Function

End Class
