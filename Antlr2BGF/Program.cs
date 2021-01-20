using System;

namespace Antlr2BGF
{
    using System.IO;
    using System.Xml;
    using Antlr4.Runtime;

    public class Program
    {
        // This should work on Linux, as long as the path separators are replaced with /

        // File to be extracted from
        private const string Filename = "grammar";

        // Path to the folder containing the file, the relative path starts at the folder containing the compiled binaries
        private const string BasePath = @".\..\..\..\Grammars";

        public static void Main()
        {
            ExtractGrammar($@"{BasePath}\{Filename}.g4", BasePath, Filename);
        }

        private static void ExtractGrammar(string filepath, string path, string filename)
        {
            Console.Out.WriteLine(filepath);

            var input = File.OpenRead(filepath);
            var inputStream = new AntlrInputStream(input);
            var antlrLexer = new ANTLRv4Lexer(inputStream);
            var commonTokenStream = new CommonTokenStream(antlrLexer);
            var antlrParser = new ANTLRv4Parser(commonTokenStream);
            var context = antlrParser.grammarSpec();
            var xmlWriter = XmlWriter.Create(@$"{path}\{filename}.xml");
            var visitor = new VisitorImplementationV2(xmlWriter);
            visitor.Visit(context);
        }
    }
}
