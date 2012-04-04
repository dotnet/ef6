Namespace AdvancedPatternsVB

    Partial Friend Class AdvancedPatternsModelFirstContext

        Public Sub New(ByVal nameOrConnectionString As String)
            MyBase.New(nameOrConnectionString)
            MyBase.Configuration.LazyLoadingEnabled = False
        End Sub

    End Class

End Namespace

