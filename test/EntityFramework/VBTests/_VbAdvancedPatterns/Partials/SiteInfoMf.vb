'---------------------------------------------------------------------
' <copyright file="SiteInfoMf.vb" company="Microsoft">
'      Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
'
' @owner       avickers
' @backupOwner bricelam
'---------------------------------------------------------------------

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
