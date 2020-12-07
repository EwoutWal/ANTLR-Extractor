using System;

namespace Antlr2BGF
{
    using System.IO;
    using System.Xml;
    using Antlr4.Runtime;

    public class Program
    {
        private const string Filename = "ambiguity";
        private const string BasePath = "C:\\Users\\Ewout van der Wal\\Documents\\ANTLR Extractor\\Antlr2BGF";

        public static void Main()
        {
            var input = File.OpenRead($"{BasePath}\\Grammars\\{Filename}.g4");
            var inputStream = new AntlrInputStream(input);
            var antlrLexer = new ANTLRv4Lexer(inputStream);
            var commonTokenStream = new CommonTokenStream(antlrLexer);
            var antlrParser = new ANTLRv4Parser(commonTokenStream);
            var context = antlrParser.grammarSpec();
            var xmlWriter = XmlWriter.Create($"{BasePath}\\output.xml");
            var visitor = new VisitorImplementation(xmlWriter);
            visitor.Visit(context);
        }
    }
}
