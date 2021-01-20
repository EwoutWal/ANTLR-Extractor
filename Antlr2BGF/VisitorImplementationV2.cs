namespace Antlr2BGF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Antlr4.Runtime.Tree;

    public class VisitorImplementationV2 : ANTLRv4ParserBaseVisitor<string>
    {
        private XmlWriter XmlWriter { get; }
        private List<string> _tokenSpecs = new List<string>();

        public VisitorImplementationV2(XmlWriter xmlWriter)
        {
            this.XmlWriter = xmlWriter;
        }

        public override string VisitGrammarSpec(ANTLRv4Parser.GrammarSpecContext context)
        {
            var grammarDeclaration = context.grammarDecl();
            var prequelConstructs = context.prequelConstruct();
            var rules = context.rules();
            var modeSpec = context.modeSpec();

            this.XmlWriter.WriteStartDocument();
            this.XmlWriter.WriteStartElement("bgf");

            // This function opens the main <grammar> tag
            Visit(grammarDeclaration);

            if (prequelConstructs.Length > 0)
            {
                this.XmlWriter.WriteStartElement("grammarInformation");

                foreach (var prequelConstructContext in prequelConstructs)
                {
                    Visit(prequelConstructContext);
                }

                this.XmlWriter.WriteEndElement();

                foreach (var tokenSpec in _tokenSpecs)
                {
                    this.XmlWriter.WriteStartElement("production");
                    this.XmlWriter.WriteAttributeString("fragment", "False");
                    this.XmlWriter.WriteStartElement("nonterminal");
                    this.XmlWriter.WriteString(tokenSpec);
                    this.XmlWriter.WriteEndElement();
                    this.XmlWriter.WriteEndElement();
                }
            }

            Visit(rules);

            // End the main <grammar> tag, after this we will open <grammar type="mode"> tags if there are lexer modes
            this.XmlWriter.WriteEndElement();

            foreach (var modeSpecContext in modeSpec)
            {
                // For each of these a new <grammar type="mode"> tag is made in the modeSpec visitor
                Visit(modeSpecContext);
            }

            this.XmlWriter.WriteEndElement();
            this.XmlWriter.WriteEndDocument();
            this.XmlWriter.Close();
            return string.Empty;
        }

        public override string VisitGrammarDecl(ANTLRv4Parser.GrammarDeclContext context)
        {
            this.XmlWriter.WriteStartElement("grammar");
            this.XmlWriter.WriteAttributeString("name", Visit(context.identifier()));
            return string.Empty;
        }

        public override string VisitPrequelConstruct(ANTLRv4Parser.PrequelConstructContext context)
        {
            var optionsSpec = context.optionsSpec();
            var delegateGrammars = context.delegateGrammars();
            var tokensSpec = context.tokensSpec();

            if (optionsSpec != null)
            {
                Visit(optionsSpec);
            }

            if (delegateGrammars != null)
            {
                Visit(delegateGrammars);
            }

            if (tokensSpec != null)
            {
                Visit(tokensSpec);
            }

            return string.Empty;
        }

        public override string VisitOptionsSpec(ANTLRv4Parser.OptionsSpecContext context)
        {
            var options = context.option();

            var tokenVocab = options.Where(o => o.identifier().GetText().Equals("tokenVocab")).ToList();
            var language = options.Where(o => o.identifier().GetText().Equals("language")).ToList();

            if (tokenVocab.Any() || language.Any())
            {
                this.XmlWriter.WriteStartElement("options");

                foreach (var optionContext in tokenVocab)
                {
                    Visit(optionContext);
                }

                foreach (var optionContext in language)
                {
                    Visit(optionContext);
                }

                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitOption(ANTLRv4Parser.OptionContext context)
        {
            this.XmlWriter.WriteStartElement("option");
            this.XmlWriter.WriteAttributeString("name", Visit(context.identifier()));
            Visit(context.optionValue());
            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitOptionValue(ANTLRv4Parser.OptionValueContext context)
        {
            var identifiers = context.identifier();
            var stringLiteral = context.STRING_LITERAL();
            var integer = context.INT();

            if (identifiers.Length != 0)
            {
                var idStrings = identifiers.Select(Visit);
                this.XmlWriter.WriteString(string.Join('.', idStrings));
            }

            if (stringLiteral != null)
            {
                this.XmlWriter.WriteString(GetLiteral(stringLiteral));
            }

            if (integer != null)
            {
                this.XmlWriter.WriteString(integer.GetText());
            }

            return string.Empty;
        }

        public override string VisitDelegateGrammars(ANTLRv4Parser.DelegateGrammarsContext context)
        {
            this.XmlWriter.WriteStartElement("imports");

            foreach (var delegateGrammarContext in context.delegateGrammar())
            {
                Visit(delegateGrammarContext);
            }

            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitDelegateGrammar(ANTLRv4Parser.DelegateGrammarContext context)
        {
            var identifiers = context.identifier();

            this.XmlWriter.WriteStartElement("grammar");
            this.XmlWriter.WriteString(Visit(identifiers.Last()));
            this.XmlWriter.WriteEndElement();

            return string.Empty;
        }

        public override string VisitTokensSpec(ANTLRv4Parser.TokensSpecContext context)
        {
            var idList = context.idList();

            if (idList != null)
            {
                Visit(idList);
            }

            return string.Empty;
        }

        public override string VisitIdList(ANTLRv4Parser.IdListContext context)
        {
            this._tokenSpecs = context.identifier().Select(Visit).ToList();

            return string.Empty;
        }

        public override string VisitActionBlock(ANTLRv4Parser.ActionBlockContext context)
        {
            this.XmlWriter.WriteString(string.Join(string.Empty, context.ACTION_CONTENT().Select(c => c.GetText())));
            return string.Empty;
        }

        public override string VisitModeSpec(ANTLRv4Parser.ModeSpecContext context)
        {
            this.XmlWriter.WriteStartElement("grammar");
            this.XmlWriter.WriteAttributeString("name", Visit(context.identifier()));
            this.XmlWriter.WriteAttributeString("type", "mode");

            foreach (var lexerRuleSpecContext in context.lexerRuleSpec())
            {
                Visit(lexerRuleSpecContext);
            }

            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitRules(ANTLRv4Parser.RulesContext context)
        {
            foreach (var ruleSpecContext in context.ruleSpec())
            {
                Visit(ruleSpecContext);
            }
            return string.Empty;
        }

        public override string VisitRuleSpec(ANTLRv4Parser.RuleSpecContext context)
        {
            var parserRuleSpec = context.parserRuleSpec();
            var lexerRuleSpec = context.lexerRuleSpec();

            if (parserRuleSpec != null)
            {
                Visit(parserRuleSpec);
            }

            if (lexerRuleSpec != null)
            {
                Visit(lexerRuleSpec);
            }

            return string.Empty;
        }

        public override string VisitParserRuleSpec(ANTLRv4Parser.ParserRuleSpecContext context)
        {
            var ruleModifiers = context.ruleModifiers();
            var ruleRef = context.RULE_REF();
            var ruleBlock = context.ruleBlock();

            this.XmlWriter.WriteStartElement("production");
            this.XmlWriter.WriteAttributeString("fragment", "False");

            if (ruleModifiers != null)
            {
                Visit(ruleModifiers);
            }

            this.XmlWriter.WriteStartElement("nonterminal");
            this.XmlWriter.WriteString(GetLiteral(ruleRef));
            this.XmlWriter.WriteEndElement();

            Visit(ruleBlock);

            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitRuleModifiers(ANTLRv4Parser.RuleModifiersContext context)
        {
            foreach (var ruleModifierContext in context.ruleModifier())
            {
                Visit(ruleModifierContext);
            }
            return string.Empty;
        }

        public override string VisitRuleModifier(ANTLRv4Parser.RuleModifierContext context)
        {
            var pub = context.PUBLIC();
            var priv = context.PRIVATE();
            var prot = context.PROTECTED();
            var result = string.Empty;

            if (pub != null)
            {
                result = GetLiteral(pub);
            }

            if (priv != null)
            {
                result = GetLiteral(priv);
            }

            if (prot != null)
            {
                result = GetLiteral(prot);
            }

            if (!result.Equals(string.Empty))
            {
                this.XmlWriter.WriteAttributeString("modifier", result);
            }

            return string.Empty;
        }

        public override string VisitRuleBlock(ANTLRv4Parser.RuleBlockContext context)
        {
            Visit(context.ruleAltList());
            return string.Empty;
        }

        public override string VisitRuleAltList(ANTLRv4Parser.RuleAltListContext context)
        {
            var labeledAlts = context.labeledAlt();

            if (labeledAlts.Length > 1)
            {
                this.XmlWriter.WriteStartElement("choice");
            }

            foreach (var labeledAltContext in labeledAlts)
            {
                Visit(labeledAltContext);
            }

            if (labeledAlts.Length > 1)
            {
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitLabeledAlt(ANTLRv4Parser.LabeledAltContext context)
        {
            Visit(context.alternative());
            return string.Empty;
        }

        public override string VisitLexerRuleSpec(ANTLRv4Parser.LexerRuleSpecContext context)
        {
            var fragment = context.FRAGMENT();
            var token = context.TOKEN_REF();
            var lexerRuleBlock = context.lexerRuleBlock();

            this.XmlWriter.WriteStartElement("production");
            this.XmlWriter.WriteAttributeString("fragment", (fragment != null).ToString());

            this.XmlWriter.WriteStartElement("nonterminal");
            this.XmlWriter.WriteString(GetLiteral(token));
            this.XmlWriter.WriteEndElement();

            Visit(lexerRuleBlock);

            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitLexerRuleBlock(ANTLRv4Parser.LexerRuleBlockContext context)
        {
            Visit(context.lexerAltList());
            return string.Empty;
        }

        public override string VisitLexerAltList(ANTLRv4Parser.LexerAltListContext context)
        {
            var lexerAlts = context.lexerAlt();

            if (lexerAlts.Length > 1)
            {
                this.XmlWriter.WriteStartElement("choice");
            }

            foreach (var lexerAltContext in lexerAlts)
            {
                Visit(lexerAltContext);
            }

            if (lexerAlts.Length > 1)
            {
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitLexerAlt(ANTLRv4Parser.LexerAltContext context)
        {
            var lexerElements = context.lexerElements();
            var lexerCommands = context.lexerCommands();

            if (lexerElements != null)
            {
                this.XmlWriter.WriteStartElement("expression");
                Visit(lexerElements);

                if (lexerCommands != null)
                {
                    Visit(lexerCommands);
                }
            }
            else
            {
                this.XmlWriter.WriteStartElement("empty");
            }

            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitLexerElements(ANTLRv4Parser.LexerElementsContext context)
        {

            foreach (var lexerElementContext in context.lexerElement())
            {
                Visit(lexerElementContext);
            }

            return string.Empty;
        }

        public override string VisitLexerElement(ANTLRv4Parser.LexerElementContext context)
        {
            var ebnfSuffix = context.ebnfSuffix();
            var labeledLexerElement = context.labeledLexerElement();
            var lexerAtom = context.lexerAtom();
            var lexerBlock = context.lexerBlock();
            var actionBlock = context.actionBlock();
            var question = context.QUESTION();

            if (ebnfSuffix != null)
            {
                Visit(ebnfSuffix);
            }

            if (labeledLexerElement != null)
            {
                Visit(labeledLexerElement);
            }

            if (lexerAtom != null)
            {
                Visit(lexerAtom);
            }

            if (lexerBlock != null)
            {
                Visit(lexerBlock);
            }

            if (ebnfSuffix != null)
            {
                this.XmlWriter.WriteEndElement();
            }

            if (actionBlock != null && question != null)
            {
                this.XmlWriter.WriteStartElement("predicate");
                Visit(actionBlock);
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitLabeledLexerElement(ANTLRv4Parser.LabeledLexerElementContext context)
        {
            var lexerAtom = context.lexerAtom();
            var lexerBlock = context.lexerBlock();

            if (lexerAtom != null)
            {
                Visit(lexerAtom);
            }

            if (lexerBlock != null)
            {
                Visit(lexerBlock);
            }

            return string.Empty;
        }

        public override string VisitLexerBlock(ANTLRv4Parser.LexerBlockContext context)
        {
            Visit(context.lexerAltList());
            return string.Empty;
        }

        public override string VisitLexerCommands(ANTLRv4Parser.LexerCommandsContext context)
        {
            var lexerCommands = context.lexerCommand();

            foreach (var lexerCommandContext in lexerCommands)
            {
                Visit(lexerCommandContext);
            }
            return string.Empty;
        }

        public override string VisitLexerCommand(ANTLRv4Parser.LexerCommandContext context)
        {
            var lexerCommandName = context.lexerCommandName();
            var lexerCommandExpr = context.lexerCommandExpr();

            var name = Visit(lexerCommandName);
            var expression = string.Empty;

            if (lexerCommandExpr != null)
            {
                expression = Visit(lexerCommandExpr);
            }

            if (name.Equals("pushMode"))
            {
                this.XmlWriter.WriteStartElement("pushGrammar");
                this.XmlWriter.WriteString(expression);
                this.XmlWriter.WriteEndElement();
            }

            if (name.Equals("popMode"))
            {
                this.XmlWriter.WriteStartElement("popGrammar");
                this.XmlWriter.WriteEndElement();
            }

            if (name.Equals("mode"))
            {
                this.XmlWriter.WriteStartElement("popGrammar");
                this.XmlWriter.WriteEndElement();
                this.XmlWriter.WriteStartElement("pushGrammar");
                this.XmlWriter.WriteString(expression);
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitLexerCommandName(ANTLRv4Parser.LexerCommandNameContext context)
        {
            var identifier = context.identifier();
            var mode = context.MODE();

            if (identifier != null)
            {
                return Visit(identifier);
            }

            return mode != null ? GetLiteral(mode) : string.Empty;
        }

        public override string VisitLexerCommandExpr(ANTLRv4Parser.LexerCommandExprContext context)
        {
            var identifier = context.identifier();
            var integer = context.INT();

            if (identifier != null)
            {
                return Visit(identifier);
            }

            return integer != null ? GetLiteral(integer) : string.Empty;
        }

        public override string VisitAltList(ANTLRv4Parser.AltListContext context)
        {
            var alternatives = context.alternative();

            if (alternatives.Length > 1)
            {
                this.XmlWriter.WriteStartElement("choice");
            }

            foreach (var alternativeContext in alternatives)
            {
                Visit(alternativeContext);
            }

            if (alternatives.Length > 1)
            {
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitAlternative(ANTLRv4Parser.AlternativeContext context)
        {
            var elementOptions = context.elementOptions();
            var elements = context.element();


            if (elements.Length != 0)
            {
                this.XmlWriter.WriteStartElement("expression");

                if (elementOptions != null)
                {
                    Visit(elementOptions);
                }

                foreach (var elementContext in elements)
                {
                    Visit(elementContext);
                }

                this.XmlWriter.WriteEndElement();
            }
            else
            {
                this.XmlWriter.WriteStartElement("empty");
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitElement(ANTLRv4Parser.ElementContext context)
        {
            var labeledElement = context.labeledElement();
            var atom = context.atom();
            var ebnfSuffix = context.ebnfSuffix();
            var ebnf = context.ebnf();
            var actionBlock = context.actionBlock();
            var question = context.QUESTION();

            if (labeledElement != null)
            {
                if (ebnfSuffix != null)
                {
                    Visit(ebnfSuffix);
                }

                Visit(labeledElement);

                if (ebnfSuffix != null)
                {
                    this.XmlWriter.WriteEndElement();
                }
            }

            if (atom != null)
            {
                if (ebnfSuffix != null)
                {
                    Visit(ebnfSuffix);
                }

                Visit(atom);

                if (ebnfSuffix != null)
                {
                    this.XmlWriter.WriteEndElement();
                }
            }

            if (ebnf != null)
            {
                Visit(ebnf);
            }

            if (actionBlock != null && question != null)
            {
                this.XmlWriter.WriteStartElement("predicate");
                Visit(actionBlock);
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitLabeledElement(ANTLRv4Parser.LabeledElementContext context)
        {
            var atom = context.atom();
            var block = context.block();

            if (atom != null)
            {
                Visit(atom);
            }

            if (block != null)
            {
                Visit(block);
            }

            return string.Empty;
        }

        public override string VisitEbnf(ANTLRv4Parser.EbnfContext context)
        {
            var blockSuffix = context.blockSuffix();

            if (blockSuffix != null)
            {
                Visit(blockSuffix);
            }

            Visit(context.block());

            if (blockSuffix != null)
            {
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitBlockSuffix(ANTLRv4Parser.BlockSuffixContext context)
        {
            Visit(context.ebnfSuffix());
            return string.Empty;
        }

        public override string VisitEbnfSuffix(ANTLRv4Parser.EbnfSuffixContext context)
        {
            var questions = context.QUESTION();
            var star = context.STAR();
            var plus = context.PLUS();
            var greedy = false;

            if (star == null && plus == null)
            {
                this.XmlWriter.WriteStartElement("optional");
            }

            if (star != null)
            {
                this.XmlWriter.WriteStartElement("star");
                greedy = questions.Length == 0;
            }

            if (plus != null)
            {
                this.XmlWriter.WriteStartElement("plus");
                greedy = questions.Length == 0;
            }

            this.XmlWriter.WriteAttributeString("greedy", greedy.ToString());

            // We do not end the XML element here on purpose. While it is "lower" in the grammar,
            // we want the resulting XML element to encompass the grammar element it modifies.
            // It is the responsibility of the rule making use of ebnfSuffix or blockSuffix
            // to close the element correctly.
            return string.Empty;
        }

        public override string VisitLexerAtom(ANTLRv4Parser.LexerAtomContext context)
        {
            var charRange = context.characterRange();
            var terminal = context.terminal();
            var notSet = context.notSet();
            var lexerCharSet = context.LEXER_CHAR_SET();
            var dot = context.DOT();

            if (charRange != null)
            {
                Visit(charRange);
            }

            if (terminal != null)
            {
                Visit(terminal);
            }

            if (notSet != null)
            {
                Visit(notSet);
            }

            if (lexerCharSet != null)
            {
                var text = lexerCharSet.GetText();
                this.ParseLexerCharSet(text);
            }

            if (dot != null)
            {
                this.XmlWriter.WriteStartElement("anything");
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitAtom(ANTLRv4Parser.AtomContext context)
        {
            var terminal = context.terminal();
            var ruleref = context.ruleref();
            var notSet = context.notSet();
            var dot = context.DOT();

            if (terminal != null)
            {
                Visit(terminal);
            }

            if (ruleref != null)
            {
                Visit(ruleref);
            }

            if (notSet != null)
            {
                Visit(notSet);
            }

            if (dot != null)
            {
                this.XmlWriter.WriteStartElement("anything");
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitNotSet(ANTLRv4Parser.NotSetContext context)
        {
            var setElement = context.setElement();
            var blockSet = context.blockSet();

            this.XmlWriter.WriteStartElement("not");

            if (setElement != null)
            {
                Visit(setElement);
            }

            if (blockSet != null)
            {
                Visit(blockSet);
            }

            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitBlockSet(ANTLRv4Parser.BlockSetContext context)
        {
            var setElements = context.setElement();

            if (setElements.Length > 1)
            {
                this.XmlWriter.WriteStartElement("choice");
            }

            foreach (var setElementContext in setElements)
            {
                Visit(setElementContext);
            }

            if (setElements.Length > 1)
            {
                this.XmlWriter.WriteEndElement();
            }

            return string.Empty;
        }

        public override string VisitSetElement(ANTLRv4Parser.SetElementContext context)
        {
            var token = context.TOKEN_REF();
            var literal = context.STRING_LITERAL();
            var charRange = context.characterRange();
            var lexerCharSet = context.LEXER_CHAR_SET();

            if (token != null)
            {
                this.XmlWriter.WriteStartElement("nonterminal");
                this.XmlWriter.WriteString(token.GetText());
                this.XmlWriter.WriteEndElement();
            }

            if (literal != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(literal));
                this.XmlWriter.WriteEndElement();
            }

            if (lexerCharSet != null)
            {
                var text = lexerCharSet.GetText();
                this.ParseLexerCharSet(text);
            }

            if (charRange != null)
            {
                Visit(charRange);
            }

            return string.Empty;
        }

        public override string VisitBlock(ANTLRv4Parser.BlockContext context)
        {
            Visit(context.altList());
            return string.Empty;
        }

        public override string VisitRuleref(ANTLRv4Parser.RulerefContext context)
        {
            var rule = GetLiteral(context.RULE_REF());

            this.XmlWriter.WriteStartElement("nonterminal");
            this.XmlWriter.WriteString(rule);
            this.XmlWriter.WriteEndElement();
            return string.Empty;
        }

        public override string VisitCharacterRange(ANTLRv4Parser.CharacterRangeContext context)
        {
            var literals = context.STRING_LITERAL();
            var lit1 = GetLiteral(literals.First());
            var lit2 = GetLiteral(literals.Last());

            this.XmlWriter.WriteStartElement("charSet");
            this.XmlWriter.WriteStartElement("charRange");

            this.WriteCharacter(lit1);
            this.WriteCharacter(lit2);

            this.XmlWriter.WriteEndElement();
            this.XmlWriter.WriteEndElement();

            return string.Empty;
        }

        public override string VisitTerminal(ANTLRv4Parser.TerminalContext context)
        {
            var literal = context.STRING_LITERAL();
            var token = context.TOKEN_REF();
            var terminalType = string.Empty;
            var tokenString = string.Empty;

            if (literal != null)
            {
                terminalType = "terminal";
                tokenString = GetLiteral(literal);
            }
            
            if (token != null)
            {
                terminalType = "nonterminal";
                tokenString = token.GetText();
            }

            this.XmlWriter.WriteStartElement(terminalType);
            this.XmlWriter.WriteString(tokenString);
            this.XmlWriter.WriteEndElement();

            return string.Empty;
        }

        public override string VisitElementOptions(ANTLRv4Parser.ElementOptionsContext context)
        {
            var optionStrings = context.elementOption().Select(Visit);
            var strings = optionStrings.ToList();

            var result = strings.Find(s => s.Contains("assoc"));

            if (result == null)
            {
                return string.Empty;
            }

            var assoc = result.Split('=').Last();

            if (assoc.Length != 2)
            {
                return string.Empty;
            }

            this.XmlWriter.WriteAttributeString("associativity", assoc);
            return string.Empty;
        }

        public override string VisitElementOption(ANTLRv4Parser.ElementOptionContext context)
        {
            var identifiers = context.identifier();
            if (identifiers.Length == 1 && context.STRING_LITERAL() == null)
            {
                return Visit(identifiers.First());
            }

            if (identifiers.Length == 1)
            {
                return $"{Visit(identifiers.First())}{GetLiteral(context.ASSIGN())}{GetLiteral(context.STRING_LITERAL())}";
            }

            return $"{Visit(identifiers.ElementAt(0))}{GetLiteral(context.ASSIGN())}{Visit(identifiers.ElementAt(1))}";
        }

        public override string VisitIdentifier(ANTLRv4Parser.IdentifierContext context)
        {
            var rule = context.RULE_REF();
            var token = context.TOKEN_REF();
            var result = string.Empty;

            if (rule != null)
            {
                result = rule.GetText();
            }
            else if (token != null)
            {
                result = token.GetText();
            }

            return result;
        }

        private void ParseLexerCharSet(string lexerCharSet)
        {
            if (lexerCharSet.StartsWith('[') && lexerCharSet.EndsWith(']'))
            {
                lexerCharSet = lexerCharSet[1..^1];
            }

            var characterList = new List<string>();
            var remaining = lexerCharSet;

            while (remaining.Length > 0)
            {
                remaining = GetNextCharacter(remaining, characterList);
            }

            this.XmlWriter.WriteStartElement("charSet");

            for (var i = 0; i < characterList.Count - 2; i++)
            {
                if (characterList[i + 1].Equals(@"-"))
                {
                    this.XmlWriter.WriteStartElement("charRange");
                    this.WriteCharacter(characterList[i]);
                    this.WriteCharacter(characterList[i+2]);
                    this.XmlWriter.WriteEndElement();
                    i += 2;
                }
                else
                {
                    this.WriteCharacter(characterList[i]);
                }
            }

            if (characterList.Count > 2 && !characterList[^2].Equals(@"-"))
            {
                this.WriteCharacter(characterList[^2]);
                this.WriteCharacter(characterList.Last());
            }
            else if (characterList.Count == 2)
            {
                this.WriteCharacter(characterList.First());
                this.WriteCharacter(characterList.Last());
            }
            else if (characterList.Count == 1)
            {
                this.WriteCharacter(characterList.First());
            }

            this.XmlWriter.WriteEndElement();
        }

        private void WriteCharacter(string character)
        {
            this.XmlWriter.WriteStartElement("char");

            this.XmlWriter.WriteAttributeString("encoding", character.StartsWith(@"\u") ? "Unicode" : "UTF-8");

            if (!character.StartsWith(@"\") || character.StartsWith(@"\u") || character.Equals(@"\t") || character.Equals(@"\r") || character.Equals(@"\n"))
            {
                this.XmlWriter.WriteString(character);
            }
            else
            {
                this.XmlWriter.WriteString(character[1..]);
            }

            this.XmlWriter.WriteEndElement();
        }

        private static string GetNextCharacter(string input, List<string> returnVal)
        {
            var unicodeChars = new List<char>("0123456789abcdefABCDEF");
            var res = input.First().ToString();
            var rest = input[1..];
            var count = 0;

            if (res.Equals(@"\"))
            {
                if (rest.StartsWith(@"u"))
                {
                    res += rest.First();
                    rest = rest[1..];

                    while (count < 4)
                    {
                        if (unicodeChars.Contains(rest.First()))
                        {
                            res += rest.First();
                            rest = rest[1..];
                            count++;
                        }

                        if (rest.StartsWith(@"{"))
                        {
                            rest = rest[1..];

                            while (!rest.StartsWith(@"}"))
                            {
                                res += rest.First();
                                rest = rest[1..];
                            }

                            rest = rest[1..];
                            count = 4;
                        }
                    }
                }
                else
                {
                    res += rest.First();
                    rest = rest[1..];
                }
            }

            returnVal.Add(res);
            return rest;
        }

        private static string GetLiteral(ITerminalNode node)
        {
            var text = node.GetText();
            Console.Out.WriteLine($@"Getting literal for original text: {text}");

            // You can't use the Trim() function here, because of the case
            // RULENAME: '\'';
            if (text.StartsWith('\'') && text.EndsWith('\''))
            {
                text = text[1..^1];
            }

            if (!text.StartsWith(@"\") || text.StartsWith(@"\u") || text.Equals(@"\t") || text.Equals(@"\r") || text.Equals(@"\n"))
            {
                return text;
            }

            return text[1..];
        }
    }
}
