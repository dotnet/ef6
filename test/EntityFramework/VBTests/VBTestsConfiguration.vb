Imports System.Data.Entity.Config
Imports FunctionalTests.TestHelpers

Public Class VBTestsConfiguration
    Inherits DbProxyConfiguration

    Public Overrides Function ConfigurationToUse() As Type
        Return GetType(FunctionalTestsConfiguration)
    End Function

End Class
