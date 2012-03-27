'---------------------------------------------------------------------
' <copyright file="AuditInfoMm.vb" company="Microsoft">
'      Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
'
' @owner       avickers
' @backupOwner bricelam
'---------------------------------------------------------------------

Imports System

Namespace Another.Place
    Partial Public Class AuditInfoMm
        Public Property ModifiedDate As Date
        Public Property ModifiedBy As String

        Public Property Concurrency As ConcurrencyInfoMm = New ConcurrencyInfoMm

    End Class
End Namespace
