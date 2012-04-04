Imports System

Namespace AdvancedPatternsVB

    Partial Public Class BuildingMf

        Public Sub New()

        End Sub

        Public Sub New(ByVal buildingId As Guid, ByVal name As String, ByVal value As Decimal, ByVal address As AddressMf)
            Me.BuildingId = buildingId
            Me.Name = name
            Me.Value = value
            Me.Address = address
        End Sub

    End Class

End Namespace
