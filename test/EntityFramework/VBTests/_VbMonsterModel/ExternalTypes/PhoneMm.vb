'---------------------------------------------------------------------
' <copyright file="PhoneMm.vb" company="Microsoft">
'      Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
'
' @owner       avickers
' @backupOwner bricelam
'---------------------------------------------------------------------

Imports System

Namespace Another.Place
    Partial Public Class PhoneMm
        Public Property PhoneNumber As String
        Public Property Extension As String = "None"
        Public Property PhoneType As PhoneTypeMm
    End Class
End Namespace
