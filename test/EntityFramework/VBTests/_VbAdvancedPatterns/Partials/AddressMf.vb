Namespace AdvancedPatternsVB

    Partial Public Class AddressMf

        Public Sub New()

        End Sub

        Public Sub New(ByVal street As String, ByVal city As String, ByVal state As String, ByVal zipCode As String, ByVal zone As Integer, ByVal environment As String)
            Me.Street = street
            Me.City = city
            Me.State = state
            Me.ZipCode = zipCode
            SiteInfo = New SiteInfoMf(zone, environment)
        End Sub

    End Class

End Namespace
