' Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
Namespace AdvancedPatternsVB

    Partial Friend Class SiteInfoMf

        Public Sub New()

        End Sub

        Public Sub New(ByVal zone As Integer, ByVal environment As String)
            Me.Zone = zone
            Me.Environment = environment
        End Sub

    End Class

End Namespace
