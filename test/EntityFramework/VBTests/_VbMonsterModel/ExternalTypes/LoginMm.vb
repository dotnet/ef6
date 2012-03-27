'---------------------------------------------------------------------
' <copyright file="LoginMm.vb" company="Microsoft">
'      Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
'
' @owner       avickers
' @backupOwner bricelam
'---------------------------------------------------------------------

Imports System
Imports System.Collections.Generic

Namespace Another.Place
    Partial Public Class LoginMm
        Public Property Username As String
        Public Property CustomerId As Integer

        Public Overridable Property Customer As CustomerMm
        Public Overridable Property LastLogin As LastLoginMm
        Public Overridable Property SentMessages As ICollection(Of MessageMm) = New HashSet(Of MessageMm)
        Public Overridable Property ReceivedMessages As ICollection(Of MessageMm) = New HashSet(Of MessageMm)
        Public Overridable Property Orders As ICollection(Of OrderMm) = New HashSet(Of OrderMm)

    End Class
End Namespace

