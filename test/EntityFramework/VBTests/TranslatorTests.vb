' Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

Imports System.Data.Entity
Imports Xunit

Public Class TranslatorTests
    Inherits FunctionalTestBase

    Public Class Context
        Inherits DbContext

        Shared Sub New()
            Database.SetInitializer(Of Context)(Nothing)
        End Sub

        Public Property Entities As DbSet(Of Entity)
    End Class

    Public Class Entity
        Public Property Id As Integer
    End Class

    <Fact()> _
    Public Sub Can_translate_power_operator()
        Using context As New Context
            ' ReSharper disable once AccessToDisposedClosure
            Dim query = From entity In context.Entities Where entity.Id ^ 2 < 30 Select entity
            Assert.Equal(
"SELECT " & vbCrLf &
"    [Extent1].[Id] AS [Id]" & vbCrLf &
"    FROM [dbo].[Entities] AS [Extent1]" & vbCrLf &
"    WHERE (POWER( CAST( [Extent1].[Id] AS float), cast(2 as float(53)))) < cast(30 as float(53))",
                query.ToString())
        End Using
    End Sub

    <Fact()> _
    Public Sub Can_translate_math_pow_function()
        Using context As New Context
            ' ReSharper disable once AccessToDisposedClosure
            Dim query = From entity In context.Entities Where Math.Pow(entity.Id, 2) < 30 Select entity
            Assert.Equal(
"SELECT " & vbCrLf &
"    [Extent1].[Id] AS [Id]" & vbCrLf &
"    FROM [dbo].[Entities] AS [Extent1]" & vbCrLf &
"    WHERE (POWER( CAST( [Extent1].[Id] AS float), cast(2 as float(53)))) < cast(30 as float(53))",
                query.ToString())
        End Using
    End Sub

End Class
