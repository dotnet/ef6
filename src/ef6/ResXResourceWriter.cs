// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NETCOREAPP

using System.IO;
using System.Text;

namespace System.Resources
{
    // HACK: Work around dotnet/winforms#1342
    internal class ResXResourceWriter : IDisposable
    {
        private readonly TextWriter _writer;

        private bool _first = true;

        public ResXResourceWriter(string fileName)
            => _writer = new StreamWriter(
                new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read),
                Encoding.UTF8);

        public void AddResource(string name, object value)
        {
            if (_first)
            {
                _writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                _writer.WriteLine(@"<root>");
                _writer.WriteLine(@"  <!-- ");
                _writer.WriteLine(@"    Microsoft ResX Schema ");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    Version 2.0");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    The primary goals of this format is to allow a simple XML format ");
                _writer.WriteLine(@"    that is mostly human readable. The generation and parsing of the ");
                _writer.WriteLine(@"    various data types are done through the TypeConverter classes ");
                _writer.WriteLine(@"    associated with the data types.");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    Example:");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    ... ado.net/XML headers & schema ...");
                _writer.WriteLine(@"    <resheader name=""resmimetype"">text/microsoft-resx</resheader>");
                _writer.WriteLine(@"    <resheader name=""version"">2.0</resheader>");
                _writer.WriteLine(@"    <resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>");
                _writer.WriteLine(@"    <resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>");
                _writer.WriteLine(@"    <data name=""Name1""><value>this is my long string</value><comment>this is a comment</comment></data>");
                _writer.WriteLine(@"    <data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>");
                _writer.WriteLine(@"    <data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">");
                _writer.WriteLine(@"        <value>[base64 mime encoded serialized .NET Framework object]</value>");
                _writer.WriteLine(@"    </data>");
                _writer.WriteLine(@"    <data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">");
                _writer.WriteLine(@"        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>");
                _writer.WriteLine(@"        <comment>This is a comment</comment>");
                _writer.WriteLine(@"    </data>");
                _writer.WriteLine(@"                ");
                _writer.WriteLine(@"    There are any number of ""resheader"" rows that contain simple ");
                _writer.WriteLine(@"    name/value pairs.");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    Each data row contains a name, and value. The row also contains a ");
                _writer.WriteLine(@"    type or mimetype. Type corresponds to a .NET class that support ");
                _writer.WriteLine(@"    text/value conversion through the TypeConverter architecture. ");
                _writer.WriteLine(@"    Classes that don't support this are serialized and stored with the ");
                _writer.WriteLine(@"    mimetype set.");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    The mimetype is used for serialized objects, and tells the ");
                _writer.WriteLine(@"    ResXResourceReader how to depersist the object. This is currently not ");
                _writer.WriteLine(@"    extensible. For a given mimetype the value must be set accordingly:");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    Note - application/x-microsoft.net.object.binary.base64 is the format ");
                _writer.WriteLine(@"    that the ResXResourceWriter will generate, however the reader can ");
                _writer.WriteLine(@"    read any of the formats listed below.");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    mimetype: application/x-microsoft.net.object.binary.base64");
                _writer.WriteLine(@"    value   : The object must be serialized with ");
                _writer.WriteLine(@"            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter");
                _writer.WriteLine(@"            : and then encoded with base64 encoding.");
                _writer.WriteLine(@"    ");
                _writer.WriteLine(@"    mimetype: application/x-microsoft.net.object.soap.base64");
                _writer.WriteLine(@"    value   : The object must be serialized with ");
                _writer.WriteLine(@"            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter");
                _writer.WriteLine(@"            : and then encoded with base64 encoding.");
                _writer.WriteLine();
                _writer.WriteLine(@"    mimetype: application/x-microsoft.net.object.bytearray.base64");
                _writer.WriteLine(@"    value   : The object must be serialized into a byte array ");
                _writer.WriteLine(@"            : using a System.ComponentModel.TypeConverter");
                _writer.WriteLine(@"            : and then encoded with base64 encoding.");
                _writer.WriteLine(@"    -->");
                _writer.WriteLine(@"  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">");
                _writer.WriteLine(@"    <xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />");
                _writer.WriteLine(@"    <xsd:element name=""root"" msdata:IsDataSet=""true"">");
                _writer.WriteLine(@"      <xsd:complexType>");
                _writer.WriteLine(@"        <xsd:choice maxOccurs=""unbounded"">");
                _writer.WriteLine(@"          <xsd:element name=""metadata"">");
                _writer.WriteLine(@"            <xsd:complexType>");
                _writer.WriteLine(@"              <xsd:sequence>");
                _writer.WriteLine(@"                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />");
                _writer.WriteLine(@"              </xsd:sequence>");
                _writer.WriteLine(@"              <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />");
                _writer.WriteLine(@"              <xsd:attribute name=""type"" type=""xsd:string"" />");
                _writer.WriteLine(@"              <xsd:attribute name=""mimetype"" type=""xsd:string"" />");
                _writer.WriteLine(@"              <xsd:attribute ref=""xml:space"" />");
                _writer.WriteLine(@"            </xsd:complexType>");
                _writer.WriteLine(@"          </xsd:element>");
                _writer.WriteLine(@"          <xsd:element name=""assembly"">");
                _writer.WriteLine(@"            <xsd:complexType>");
                _writer.WriteLine(@"              <xsd:attribute name=""alias"" type=""xsd:string"" />");
                _writer.WriteLine(@"              <xsd:attribute name=""name"" type=""xsd:string"" />");
                _writer.WriteLine(@"            </xsd:complexType>");
                _writer.WriteLine(@"          </xsd:element>");
                _writer.WriteLine(@"          <xsd:element name=""data"">");
                _writer.WriteLine(@"            <xsd:complexType>");
                _writer.WriteLine(@"              <xsd:sequence>");
                _writer.WriteLine(@"                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />");
                _writer.WriteLine(@"                <xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />");
                _writer.WriteLine(@"              </xsd:sequence>");
                _writer.WriteLine(@"              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />");
                _writer.WriteLine(@"              <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />");
                _writer.WriteLine(@"              <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />");
                _writer.WriteLine(@"              <xsd:attribute ref=""xml:space"" />");
                _writer.WriteLine(@"            </xsd:complexType>");
                _writer.WriteLine(@"          </xsd:element>");
                _writer.WriteLine(@"          <xsd:element name=""resheader"">");
                _writer.WriteLine(@"            <xsd:complexType>");
                _writer.WriteLine(@"              <xsd:sequence>");
                _writer.WriteLine(@"                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />");
                _writer.WriteLine(@"              </xsd:sequence>");
                _writer.WriteLine(@"              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />");
                _writer.WriteLine(@"            </xsd:complexType>");
                _writer.WriteLine(@"          </xsd:element>");
                _writer.WriteLine(@"        </xsd:choice>");
                _writer.WriteLine(@"      </xsd:complexType>");
                _writer.WriteLine(@"    </xsd:element>");
                _writer.WriteLine(@"  </xsd:schema>");
                _writer.WriteLine(@"  <resheader name=""resmimetype"">");
                _writer.WriteLine(@"    <value>text/microsoft-resx</value>");
                _writer.WriteLine(@"  </resheader>");
                _writer.WriteLine(@"  <resheader name=""version"">");
                _writer.WriteLine(@"    <value>2.0</value>");
                _writer.WriteLine(@"  </resheader>");
                _writer.WriteLine(@"  <resheader name=""reader"">");
                _writer.WriteLine(@"    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>");
                _writer.WriteLine(@"  </resheader>");
                _writer.WriteLine(@"  <resheader name=""writer"">");
                _writer.WriteLine(@"    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>");
                _writer.WriteLine(@"  </resheader>");
                _first = false;
            }

            _writer.Write(@"  <data name="""); _writer.Write(name);
            _writer.WriteLine(@""" xml:space=""preserve"">");
            _writer.Write("    <value>");
            _writer.Write(value);
            _writer.WriteLine("</value>");
            _writer.WriteLine("  </data>");
        }

        public void Dispose()
        {
            _writer.Write(@"</root>");
            _writer.Dispose();
        }
    }
}

#endif
