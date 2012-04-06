
/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!                                                                            !!
!!         ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION        !!
!!                                                                            !!
!!                     DO NOT CHANGE THIS FILE BY HAND!!!!                    !!
!!                          YOU HAVE BEEN WARNED !!!!                         !!
!!                                                                            !!
!!         ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION        !!
!!                                                                            !!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/


/*------------------------------------------------------------------------------
// <copyright file="CqlLexer.l" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  almineev
// @backupOwner simoncav
//----------------------------------------------------------------------------*/

using System;
using System.Globalization;
using System.Data.Common.EntitySql.AST;
using System.Data.Entity;

%%

%namespace System.Data.Common.EntitySql

%class CqlLexer

%partial

%type CqlLexer.Token

%eofval{
    return null;
%eofval}

%translate

%char

%line

%exception System.Data.EntitySqlException

%noacceptmessage EntitySqlException.GetGenericErrorMessage (_query, yychar)

%fatalerrormessage EntityRes.GetString(EntityRes.ParserFatalError)

%inputerrormessage EntityRes.GetString(EntityRes.ParserInputError)

A=[Aa]
B=[Bb]
C=[Cc]
D=[Dd]
E=[Ee]
F=[Ff]
G=[Gg]
H=[Hh]
I=[Ii]
J=[Jj]
K=[Kk]
L=[Ll]
M=[Mm]
N=[Nn]
O=[Oo]
P=[Pp]
Q=[Qq]
R=[Rr]
S=[Ss]
T=[Tt]
U=[Uu]
V=[Vv]
W=[Ww]
X=[Xx]
Y=[Yy]
Z=[Zz]

LETTER=[a-zA-Z]

DIGIT=[0-9]

HEX_DIGIT=[a-fA-F]|DIGIT

ALPHANUM={LETTER}|{DIGIT}|\_

INT_SUFFIX={U}|{L}|{U}{L}|{L}{U}

FLOAT_SUFFIX={F}|{D}

INTEGER_NUMBER=({DIGIT})+({INT_SUFFIX})?

FLOAT_NUMBER=({DIGIT})*(\.)?({DIGIT})+({E}[\+\-]?({DIGIT})+)?({FLOAT_SUFFIX})?

DECIMAL_NUMBER=({DIGIT})*(\.)?({DIGIT})+({M})?

STRING_VALUE=([^\'\"\r\n])*

SINGLE_QUOTED_STRING_VALUE=([^\']|\'\')*

DOUBLE_QUOTED_STRING_VALUE=([^\"]|\"\")*

BINARY_LITERAL=(({B}{I}{N}{A}{R}{Y})({NONNEWLINE_SPACE})*|{X})\'{STRING_VALUE}\'

GUID_LITERAL=({G}{U}{I}{D})({NONNEWLINE_SPACE})*\'{STRING_VALUE}\'

DATETIME_LITERAL=({D}{A}{T}{E}{T}{I}{M}{E})({NONNEWLINE_SPACE})*\'{STRING_VALUE}\'

TIME_LITERAL=({T}{I}{M}{E})({NONNEWLINE_SPACE})*\'{STRING_VALUE}\'

DATETIMEOFFSET_LITERAL=({D}{A}{T}{E}{T}{I}{M}{E}{O}{F}{F}{S}{E}{T})({NONNEWLINE_SPACE})*\'{STRING_VALUE}\'

SINGLE_QUOTED_STRING=N?\'{SINGLE_QUOTED_STRING_VALUE}\'

DOUBLE_QUOTED_STRING=N?\"{DOUBLE_QUOTED_STRING_VALUE}\"

BOOL_LITERAL=({T}{R}{U}{E})|({F}{A}{L}{S}{E})

NUMBER_LITERAL=({INTEGER_NUMBER})|({FLOAT_NUMBER})|({DECIMAL_NUMBER})

PARAMETER=@({LETTER}|{DIGIT}|_)+

PUNCTUATORS=(\,)|(\:)|(\;)|(\.)|(\?)|(\()|(\))|(\[)|(\])|(\{)|(\})|(\=)

OPERATORS=(\=\=)|(\!\=)|(\<\>)|(\<)|(\<=)|(\>)|(\>\=)|(\&\&)|(\|\|)|(\!)|(\+)|(\-)|(\*)|(\/)|(\%)

SIMPLE_IDENTIFIER=({LETTER}|\_)({ALPHANUM})*

NONNEWLINE_SPACE=([ \r])+

NEWLINE=(\n)+

LINE_COMMENT=(\-\-).*$

%%

<YYINITIAL> \[
{
    return HandleEscapedIdentifiers();
}

<YYINITIAL> {BINARY_LITERAL}
{
    return NewLiteralToken(YYText, LiteralKind.Binary);
}

<YYINITIAL> {DATETIME_LITERAL}
{
    return NewLiteralToken(YYText, LiteralKind.DateTime);
}

<YYINITIAL> {TIME_LITERAL}
{
    return NewLiteralToken(YYText, LiteralKind.Time);
}

<YYINITIAL> {DATETIMEOFFSET_LITERAL}
{
    return NewLiteralToken(YYText, LiteralKind.DateTimeOffset);
}

<YYINITIAL> {GUID_LITERAL}
{
    return NewLiteralToken(YYText, LiteralKind.Guid);
}

<YYINITIAL> {NUMBER_LITERAL}
{
    return NewLiteralToken(YYText, LiteralKind.Number);
}

<YYINITIAL> {BOOL_LITERAL}
{
    return NewLiteralToken(YYText, LiteralKind.Boolean);
}

<YYINITIAL> {SINGLE_QUOTED_STRING}
{
    return NewLiteralToken(YYText, LiteralKind.String);
}

<YYINITIAL> {DOUBLE_QUOTED_STRING}
{
    return MapDoubleQuotedString(YYText);
}

<YYINITIAL> {PARAMETER}
{
    return NewParameterToken(YYText);
}

<YYINITIAL> {OPERATORS}
{
    return MapOperator(YYText);
}

<YYINITIAL> {PUNCTUATORS}
{
    return MapPunctuator(YYText);
}

<YYINITIAL> {SIMPLE_IDENTIFIER}
{
    return MapIdentifierOrKeyword(YYText);
}

<YYINITIAL> {NONNEWLINE_SPACE}
{
    AdvanceIPos();
    ResetSymbolAsIdentifierState(false);
    return null;
}

<YYINITIAL> {NEWLINE}
{
    _lineNumber++;
    AdvanceIPos();
    ResetSymbolAsIdentifierState(false);
    return null;
}

<YYINITIAL> {LINE_COMMENT}
{
    _lineNumber++;
    AdvanceIPos();
    ResetSymbolAsIdentifierState(false);
    return null;
}
