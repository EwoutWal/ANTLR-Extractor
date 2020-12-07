using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Antlr2BGFTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Text.RegularExpressions;
    using Antlr2BGF;
    using Antlr4.Runtime;

    [TestClass]
    public class PredefinedGrammarsTest
    {
        private const string Path = "C:\\Users\\Ewout van der Wal\\Documents\\ANTLR Extractor\\Antlr2BGF\\Grammars";

        private static FileStream ReadFile(string filename)
        {
            return File.OpenRead($"{Path}\\{filename}.g4");
        }

        private static ANTLRv4Lexer GetLexer(string filename)
        {
            var inputStream = new AntlrInputStream(ReadFile(filename));
            return new ANTLRv4Lexer(inputStream);
        }

        private static ANTLRv4Parser GetParser(string filename)
        {
            var commonTokenStream = new CommonTokenStream(GetLexer(filename));
            return new ANTLRv4Parser(commonTokenStream);
        }

        private static void AssertNumberOfErrors(int errors, string input)
        {
            Assert.AreEqual(errors + 1, Regex.Matches(input, "<(\\w|\\s)+>").Count);
        }

        [TestMethod]
        public void Cpp14()
        {
            // Arrange
            var parser = GetParser("CPP14");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(0, result);
        }

        [TestMethod]
        public void Hello()
        {
            // Arrange
            var parser = GetParser("Hello");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(0, result);
        }

        [TestMethod]
        public void Issue1165()
        {
            // Arrange
            var parser = GetParser("Issue1165");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(0, result);
        }

        [TestMethod]
        public void Issue1165_2()
        {
            // Arrange
            var parser = GetParser("Issue1165_2");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(0, result);
        }

        [TestMethod]
        public void Issue1165_3()
        {
            // Arrange
            var parser = GetParser("Issue1165_3");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(0, result);
        }

        [TestMethod]
        public void Issue1165_4()
        {
            // Arrange
            var lexer = GetLexer("Issue1165_4");

            // Act
            var tokens = lexer.GetAllTokens();

            // Assert
            Assert.AreNotEqual(new List<IToken>(), tokens);
        }

        [TestMethod]
        public void Issue1567()
        {
            // Arrange
            var parser = GetParser("Issue1567");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(0, result);
        }

        [TestMethod]
        public void Three()
        {
            // Arrange
            var parser = GetParser("three");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(1, result);
        }

        [TestMethod]
        public void TwoComments()
        {
            // Arrange
            var parser = GetParser("twocomments");
            var context = parser.grammarSpec();
            var visitor = new VisitorImplementation();

            // Act
            var result = visitor.Visit(context);

            // Assert
            AssertNumberOfErrors(0, result);
        }
    }
}
