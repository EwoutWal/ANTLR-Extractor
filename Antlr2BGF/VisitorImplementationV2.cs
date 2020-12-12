namespace Antlr2BGF
{
    using System;
    using System.Linq;
    using System.Xml;
    using Antlr4.Runtime.Tree;

    public class VisitorImplementationV2 : ANTLRv4ParserBaseVisitor<XmlNode>
    {
        private XmlWriter XmlWriter { get; }

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

            Visit(grammarDeclaration);

            if (prequelConstructs.Length != 0)
            {
                this.XmlWriter.WriteStartElement("prequelConstructs");

                foreach (var prequelConstructContext in prequelConstructs)
                {
                    Visit(prequelConstructContext);
                }

                this.XmlWriter.WriteEndElement();
            }

            Visit(rules);

            foreach (var modeSpecContext in modeSpec)
            {
                Visit(modeSpecContext);
            }

            this.XmlWriter.WriteEndElement();
            this.XmlWriter.WriteEndDocument();
            this.XmlWriter.Close();
            return "";
        }

        public override string VisitGrammarDecl(ANTLRv4Parser.GrammarDeclContext context)
        {
            this.XmlWriter.WriteStartElement("grammar");
            this.XmlWriter.WriteAttributeString("name", Visit(context.identifier()));
            Visit(context.grammarType());
            return "";
        }

        public override string VisitGrammarType(ANTLRv4Parser.GrammarTypeContext context)
        {
            var lexer = context.LEXER();
            var parser = context.PARSER();
            var result = "";

            if (lexer != null)
            {
                result = GetLiteral(lexer);
            }

            if (parser != null)
            {
                result = GetLiteral(parser);
            }

            if (result != "")
            {
                this.XmlWriter.WriteAttributeString("type", result);
            }

            return "";
        }

        public override string VisitPrequelConstruct(ANTLRv4Parser.PrequelConstructContext context)
        {
            var optionsSpec = context.optionsSpec();
            var delegateGrammars = context.delegateGrammars();
            var tokensSpec = context.tokensSpec();
            var channelsSpec = context.channelsSpec();
            var action_ = context.action_();

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

            if (channelsSpec != null)
            {
                Visit(channelsSpec);
            }

            if (action_ != null)
            {
                Visit(action_);
            }

            return "";
        }

        public override string VisitOptionsSpec(ANTLRv4Parser.OptionsSpecContext context)
        {
            var options = context.option();

            if (options.Length != 0)
            {
                this.XmlWriter.WriteStartElement("options");

                foreach (var optionContext in options)
                {
                    Visit(optionContext);
                }

                this.XmlWriter.WriteEndElement();
            }

            return "";
        }

        public override string VisitOption(ANTLRv4Parser.OptionContext context)
        {
            this.XmlWriter.WriteStartElement("option");
            this.XmlWriter.WriteAttributeString("name", Visit(context.identifier()));
            Visit(context.optionValue());
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitOptionValue(ANTLRv4Parser.OptionValueContext context)
        {
            var identifiers = context.identifier();
            var stringLiteral = context.STRING_LITERAL();
            var actionBlock = context.actionBlock();
            var integer = context.INT();

            if (identifiers.Length != 0)
            {
                var idStrings = identifiers.Select(Visit);
                this.XmlWriter.WriteStartElement("nonterminal");
                this.XmlWriter.WriteString(string.Join('.', idStrings));
                this.XmlWriter.WriteEndElement();
            }

            if (stringLiteral != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(stringLiteral));
                this.XmlWriter.WriteEndElement();
            }

            if (actionBlock != null)
            {
                Visit(actionBlock);
            }

            if (integer != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(integer.GetText());
                this.XmlWriter.WriteEndElement();
            }

            return "";
        }

        public override string VisitDelegateGrammars(ANTLRv4Parser.DelegateGrammarsContext context)
        {
            this.XmlWriter.WriteStartElement("sequence");
            this.XmlWriter.WriteAttributeString("type", "delegateGrammars");

            foreach (var delegateGrammarContext in context.delegateGrammar())
            {
                Visit(delegateGrammarContext);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitDelegateGrammar(ANTLRv4Parser.DelegateGrammarContext context)
        {
            var identifiers = context.identifier();

            this.XmlWriter.WriteStartElement("grammar");

            if (identifiers.Length == 2)
            {
                this.XmlWriter.WriteAttributeString("alias", Visit(identifiers.First()));
            }

            this.XmlWriter.WriteString(Visit(identifiers.Last()));
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitTokensSpec(ANTLRv4Parser.TokensSpecContext context)
        {
            var idList = context.idList();

            this.XmlWriter.WriteStartElement("sequence");
            this.XmlWriter.WriteAttributeString("type", "tokens");

            if (idList != null)
            {
                Visit(idList);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitChannelsSpec(ANTLRv4Parser.ChannelsSpecContext context)
        {
            var idList = context.idList();

            this.XmlWriter.WriteStartElement("sequence");
            this.XmlWriter.WriteAttributeString("type", "channels");

            if (idList != null)
            {
                Visit(idList);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitIdList(ANTLRv4Parser.IdListContext context)
        {
            foreach (var identifierContext in context.identifier())
            {
                this.XmlWriter.WriteStartElement("nonterminal");
                this.XmlWriter.WriteString(Visit(identifierContext));
                this.XmlWriter.WriteEndElement();
            }

            return "";
        }

        public override string VisitAction_(ANTLRv4Parser.Action_Context context)
        {
            var actionScopeName = context.actionScopeName();

            this.XmlWriter.WriteStartElement("action");

            if (actionScopeName != null)
            {
                this.XmlWriter.WriteAttributeString("name", Visit(actionScopeName));
            }

            this.XmlWriter.WriteStartElement("nonterminal");
            this.XmlWriter.WriteString(Visit(context.identifier()));
            this.XmlWriter.WriteEndElement();

            Visit(context.actionBlock());

            return "";
        }

        public override string VisitActionScopeName(ANTLRv4Parser.ActionScopeNameContext context)
        {
            var identifier = context.identifier();
            var lexer = context.LEXER();
            var parser = context.PARSER();

            if (identifier != null)
            {
                return Visit(identifier);
            }

            if (lexer != null)
            {
                return GetLiteral(lexer);
            }

            if (parser != null)
            {
                return GetLiteral(parser);
            }

            return "";
        }

        public override string VisitActionBlock(ANTLRv4Parser.ActionBlockContext context)
        {
            this.XmlWriter.WriteStartElement("action");
            this.XmlWriter.WriteString(string.Join("", context.ACTION_CONTENT().Select(c => c.GetText())));
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitArgActionBlock(ANTLRv4Parser.ArgActionBlockContext context)
        {
            this.XmlWriter.WriteStartElement("argAction");
            this.XmlWriter.WriteString(string.Join("", context.ARGUMENT_CONTENT().Select(c => c.GetText())));
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitModeSpec(ANTLRv4Parser.ModeSpecContext context)
        {
            this.XmlWriter.WriteStartElement("lexerMode");
            this.XmlWriter.WriteAttributeString("name", Visit(context.identifier()));

            foreach (var lexerRuleSpecContext in context.lexerRuleSpec())
            {
                Visit(lexerRuleSpecContext);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitRules(ANTLRv4Parser.RulesContext context)
        {
            foreach (var ruleSpecContext in context.ruleSpec())
            {
                Visit(ruleSpecContext);
            }
            return "";
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

            return "";
        }

        public override string VisitParserRuleSpec(ANTLRv4Parser.ParserRuleSpecContext context)
        {
            var ruleModifiers = context.ruleModifiers();
            var ruleRef = context.RULE_REF();
            var argActionBlock = context.argActionBlock();
            var ruleReturns = context.ruleReturns();
            var throwsSpec = context.throwsSpec();
            var localsSpec = context.localsSpec();
            var rulePrequels = context.rulePrequel();
            var ruleBlock = context.ruleBlock();
            var exceptionGroup = context.exceptionGroup();

            this.XmlWriter.WriteStartElement("production");
            this.XmlWriter.WriteAttributeString("fragment", "False");

            if (ruleModifiers != null)
            {
                Visit(ruleModifiers);
            }

            this.XmlWriter.WriteStartElement("terminal");
            this.XmlWriter.WriteString(GetLiteral(ruleRef));
            this.XmlWriter.WriteEndElement();

            if (argActionBlock != null || ruleReturns != null || throwsSpec != null || localsSpec != null || rulePrequels.Length != 0)
            {
                this.XmlWriter.WriteStartElement("options");

                if (argActionBlock != null)
                {
                    Visit(argActionBlock);
                }

                if (ruleReturns != null)
                {
                    Visit(ruleReturns);
                }

                if (throwsSpec != null)
                {
                    Visit(throwsSpec);
                }

                if (localsSpec != null)
                {
                    Visit(localsSpec);
                }

                foreach (var rulePrequelContext in rulePrequels)
                {
                    Visit(rulePrequelContext);
                }

                this.XmlWriter.WriteEndElement();
            }

            Visit(ruleBlock);
            Visit(exceptionGroup);

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitExceptionGroup(ANTLRv4Parser.ExceptionGroupContext context)
        {
            var exceptionHandlers = context.exceptionHandler();
            var finallyClause = context.finallyClause();

            if (exceptionHandlers.Length != 0 || finallyClause != null)
            {
                this.XmlWriter.WriteStartElement("exceptions");

                foreach (var exceptionHandlerContext in exceptionHandlers)
                {
                    Visit(exceptionHandlerContext);
                }

                if (finallyClause != null)
                {
                    Visit(finallyClause);
                }

                this.XmlWriter.WriteEndElement();
            }

            return "";
        }

        public override string VisitExceptionHandler(ANTLRv4Parser.ExceptionHandlerContext context)
        {
            this.XmlWriter.WriteStartElement("catch");
            Visit(context.argActionBlock());
            Visit(context.actionBlock());
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitFinallyClause(ANTLRv4Parser.FinallyClauseContext context)
        {
            this.XmlWriter.WriteStartElement("finally");
            Visit(context.actionBlock());
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitRulePrequel(ANTLRv4Parser.RulePrequelContext context)
        {
            var optionsSpec = context.optionsSpec();
            var ruleAction = context.ruleAction();

            if (optionsSpec != null)
            {
                Visit(optionsSpec);
            }

            if (ruleAction != null)
            {
                Visit(ruleAction);
            }

            return "";
        }

        public override string VisitRuleReturns(ANTLRv4Parser.RuleReturnsContext context)
        {
            this.XmlWriter.WriteStartElement("returns");
            Visit(context.argActionBlock());
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitThrowsSpec(ANTLRv4Parser.ThrowsSpecContext context)
        {
            this.XmlWriter.WriteStartElement("throws");

            foreach (var identifierContext in context.identifier())
            {
                Visit(identifierContext);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitLocalsSpec(ANTLRv4Parser.LocalsSpecContext context)
        {
            this.XmlWriter.WriteStartElement("locals");
            Visit(context.argActionBlock());
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitRuleAction(ANTLRv4Parser.RuleActionContext context)
        {
            this.XmlWriter.WriteStartElement("ruleAction");
            Visit(context.identifier());
            Visit(context.actionBlock());
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitRuleModifiers(ANTLRv4Parser.RuleModifiersContext context)
        {
            foreach (var ruleModifierContext in context.ruleModifier())
            {
                Visit(ruleModifierContext);
            }
            return "";
        }

        public override string VisitRuleModifier(ANTLRv4Parser.RuleModifierContext context)
        {
            var pub = context.PUBLIC();
            var priv = context.PRIVATE();
            var prot = context.PROTECTED();
            var fragment = context.FRAGMENT();
            var result = "";

            this.XmlWriter.WriteStartElement("terminal");

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

            if (fragment != null)
            {
                result = GetLiteral(fragment);
            }

            this.XmlWriter.WriteString(result);
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitRuleBlock(ANTLRv4Parser.RuleBlockContext context)
        {
            Visit(context.ruleAltList());
            return "";
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

            return "";
        }

        public override string VisitLabeledAlt(ANTLRv4Parser.LabeledAltContext context)
        {
            var identifier = context.identifier();

            this.XmlWriter.WriteStartElement("expression");

            if (identifier != null)
            {
                this.XmlWriter.WriteAttributeString("label", Visit(identifier));
            }

            Visit(context.alternative());
            this.XmlWriter.WriteEndElement();
            return "";
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
            return "";
        }

        public override string VisitLexerRuleBlock(ANTLRv4Parser.LexerRuleBlockContext context)
        {
            Visit(context.lexerAltList());
            return "";
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

            return "";
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
            return "";
        }

        public override string VisitLexerElements(ANTLRv4Parser.LexerElementsContext context)
        {
            this.XmlWriter.WriteStartElement("expression");

            foreach (var lexerElementContext in context.lexerElement())
            {
                Visit(lexerElementContext);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitLexerElement(ANTLRv4Parser.LexerElementContext context)
        {
            var ebnfSuffix = context.ebnfSuffix();
            var labeledLexerElement = context.labeledLexerElement();
            var lexerAtom = context.lexerAtom();
            var lexerBlock = context.lexerBlock();
            var actionBlock = context.actionBlock();
            var question = context.QUESTION();

            if (labeledLexerElement != null)
            {
                if (ebnfSuffix != null)
                {
                    Visit(ebnfSuffix);
                }

                Visit(labeledLexerElement);

                if (ebnfSuffix != null)
                {
                    this.XmlWriter.WriteEndElement();
                }
            }

            if (lexerAtom != null)
            {
                if (ebnfSuffix != null)
                {
                    Visit(ebnfSuffix);
                }

                Visit(lexerAtom);

                if (ebnfSuffix != null)
                {
                    this.XmlWriter.WriteEndElement();
                }
            }

            if (lexerBlock != null)
            {
                if (ebnfSuffix != null)
                {
                    Visit(ebnfSuffix);
                }

                Visit(lexerBlock);

                if (ebnfSuffix != null)
                {
                    this.XmlWriter.WriteEndElement();
                }
            }

            if (actionBlock != null)
            {
                if (question != null)
                {
                    this.XmlWriter.WriteStartElement("optional");
                    this.XmlWriter.WriteAttributeString("greedy", "True");
                }

                Visit(actionBlock);

                if (question != null)
                {
                    this.XmlWriter.WriteEndElement();
                }
            }

            return "";
        }

        public override string VisitLabeledLexerElement(ANTLRv4Parser.LabeledLexerElementContext context)
        {
            var assign = context.ASSIGN();
            var plusAssign = context.PLUS_ASSIGN();
            var lexerAtom = context.lexerAtom();
            var lexerBlock = context.lexerBlock();

            Visit(context.identifier());

            if (assign != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(assign));
                this.XmlWriter.WriteEndElement();
            }

            if (plusAssign != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(plusAssign));
                this.XmlWriter.WriteEndElement();
            }

            if (lexerAtom != null)
            {
                Visit(lexerAtom);
            }

            if (lexerBlock != null)
            {
                Visit(lexerBlock);
            }

            return "";
        }

        public override string VisitLexerBlock(ANTLRv4Parser.LexerBlockContext context)
        {
            Visit(context.lexerAltList());
            return "";
        }

        public override string VisitLexerCommands(ANTLRv4Parser.LexerCommandsContext context)
        {
            var lexerCommands = context.lexerCommand();

            this.XmlWriter.WriteStartElement("expression");

            foreach (var lexerCommandContext in lexerCommands)
            {
                Visit(lexerCommandContext);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitLexerCommand(ANTLRv4Parser.LexerCommandContext context)
        {
            var lexerCommandName = context.lexerCommandName();
            var lexerCommandExpr = context.lexerCommandExpr();

            this.XmlWriter.WriteStartElement("lexerCommand");
            Visit(lexerCommandName);

            if (lexerCommandExpr != null)
            {
                this.XmlWriter.WriteStartElement("expression");
                Visit(lexerCommandExpr);
                this.XmlWriter.WriteEndElement();
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitLexerCommandName(ANTLRv4Parser.LexerCommandNameContext context)
        {
            var identifier = context.identifier();
            var mode = context.MODE();

            if (identifier != null)
            {
                this.XmlWriter.WriteString(Visit(identifier));
            }

            if (mode != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(mode));
                this.XmlWriter.WriteEndElement();
            }

            return "";
        }

        public override string VisitLexerCommandExpr(ANTLRv4Parser.LexerCommandExprContext context)
        {
            var identifier = context.identifier();
            var integer = context.INT();

            if (identifier != null)
            {
                this.XmlWriter.WriteString(Visit(identifier));
            }

            if (integer != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(integer));
                this.XmlWriter.WriteEndElement();
            }

            return "";
        }

        public override string VisitAltList(ANTLRv4Parser.AltListContext context)
        {
            foreach (var alternativeContext in context.alternative())
            {
                Visit(alternativeContext);
            }

            return "";
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

            return "";
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

            if (actionBlock != null)
            {
                if (question != null)
                {
                    this.XmlWriter.WriteStartElement("optional");
                    this.XmlWriter.WriteAttributeString("greedy", "True");
                }

                Visit(actionBlock);

                if (question != null)
                {
                    this.XmlWriter.WriteEndElement();
                }
            }

            return "";
        }

        public override string VisitLabeledElement(ANTLRv4Parser.LabeledElementContext context)
        {
            var assign = context.ASSIGN();
            var plusAssign = context.PLUS_ASSIGN();
            var atom = context.atom();
            var block = context.block();

            Visit(context.identifier());

            if (assign != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(assign));
                this.XmlWriter.WriteEndElement();
            }

            if (plusAssign != null)
            {
                this.XmlWriter.WriteStartElement("terminal");
                this.XmlWriter.WriteString(GetLiteral(plusAssign));
                this.XmlWriter.WriteEndElement();
            }

            if (atom != null)
            {
                Visit(atom);
            }

            if (block != null)
            {
                Visit(block);
            }

            return "";
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

            return "";
        }

        public override string VisitBlockSuffix(ANTLRv4Parser.BlockSuffixContext context)
        {
            Visit(context.ebnfSuffix());
            return "";
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
                greedy = questions.Length == 1;
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
            return "";
        }

        public override string VisitLexerAtom(ANTLRv4Parser.LexerAtomContext context)
        {
            var charRange = context.characterRange();
            var terminal = context.terminal();
            var notSet = context.notSet();
            var lexerCharSet = context.LEXER_CHAR_SET();
            var dot = context.DOT();
            var elementOptions = context.elementOptions();

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
                this.XmlWriter.WriteStartElement("charset");
                this.XmlWriter.WriteString(lexerCharSet.GetText().Trim('[', ']'));
                this.XmlWriter.WriteEndElement();
            }

            if (dot != null)
            {
                this.XmlWriter.WriteStartElement("terminal");

                if (elementOptions != null)
                {
                    Visit(elementOptions);
                }

                this.XmlWriter.WriteString(GetLiteral(dot));
                this.XmlWriter.WriteEndElement();
            }

            return "";
        }

        public override string VisitAtom(ANTLRv4Parser.AtomContext context)
        {
            var terminal = context.terminal();
            var ruleref = context.ruleref();
            var notSet = context.notSet();
            var dot = context.DOT();
            var elementOptions = context.elementOptions();

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
                this.XmlWriter.WriteStartElement("terminal");

                if (elementOptions != null)
                {
                    Visit(elementOptions);
                }

                this.XmlWriter.WriteString(GetLiteral(dot));
                this.XmlWriter.WriteEndElement();
            }

            return "";
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
            return "";
        }

        public override string VisitBlockSet(ANTLRv4Parser.BlockSetContext context)
        {
            this.XmlWriter.WriteStartElement("choice");

            foreach (var setElementContext in context.setElement())
            {
                Visit(setElementContext);
            }

            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitSetElement(ANTLRv4Parser.SetElementContext context)
        {
            var token = context.TOKEN_REF();
            var literal = context.STRING_LITERAL();
            var charRange = context.characterRange();
            var lexerCharSet = context.LEXER_CHAR_SET();
            var options = context.elementOptions();

            if (token != null)
            {
                this.XmlWriter.WriteStartElement("nonterminal");

                if (options != null)
                {
                    Visit(options);
                }

                this.XmlWriter.WriteString(token.GetText());
                this.XmlWriter.WriteEndElement();
            }

            if (literal != null)
            {
                this.XmlWriter.WriteStartElement("terminal");

                if (options != null)
                {
                    Visit(options);
                }

                this.XmlWriter.WriteString(literal.GetText());
                this.XmlWriter.WriteEndElement();
            }

            if (lexerCharSet != null)
            {
                this.XmlWriter.WriteStartElement("charset");
                this.XmlWriter.WriteString(lexerCharSet.GetText().Trim('[', ']'));
                this.XmlWriter.WriteEndElement();
            }

            if (charRange != null)
            {
                Visit(charRange);
            }

            return "";
        }

        public override string VisitBlock(ANTLRv4Parser.BlockContext context)
        {
            var options = context.optionsSpec();
            var actions = context.ruleAction();
            this.XmlWriter.WriteStartElement("choice");

            if (context.COLON() != null)
            {
                if (options != null)
                {
                    Visit(options);
                }

                if (actions.Length != 0)
                {
                    var actionStrings = actions.Select(a => $"{{{Visit(a)}}}");
                    var actionString = string.Join(", ", actionStrings);
                    this.XmlWriter.WriteAttributeString("actions", actionString);
                }
            }

            Visit(context.altList());
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitRuleref(ANTLRv4Parser.RulerefContext context)
        {
            var rule = GetLiteral(context.RULE_REF());
            var actions = context.argActionBlock();
            var options = context.elementOptions();
            
            this.XmlWriter.WriteStartElement("nonterminal");

            if (actions != null)
            {
                Visit(actions);
            }

            if (options != null)
            {
                Visit(options);
            }

            this.XmlWriter.WriteString(rule);
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitCharacterRange(ANTLRv4Parser.CharacterRangeContext context)
        {
            var literals = context.STRING_LITERAL();
            this.XmlWriter.WriteStartElement("charset");
            var range = $"{GetLiteral(literals.ElementAt(0))}-{GetLiteral(literals.ElementAt(1))}";
            this.XmlWriter.WriteString(range);
            this.XmlWriter.WriteEndElement();
            return "";
        }

        public override string VisitTerminal(ANTLRv4Parser.TerminalContext context)
        {
            var literal = context.STRING_LITERAL();
            var token = context.TOKEN_REF();
            var options = context.elementOptions();
            string terminalType;
            string tokenString;

            if (literal != null)
            {
                terminalType = "terminal";
                tokenString = GetLiteral(literal);
            }
            else if (token != null)
            {
                terminalType = "nonterminal";
                tokenString = token.GetText();
            }
            else
            {
                throw new ParseException();
            }

            this.XmlWriter.WriteStartElement(terminalType);

            if (options != null)
            {
                Visit(options);
            }

            this.XmlWriter.WriteString(tokenString);
            this.XmlWriter.WriteEndElement();

            return "";
        }

        public override string VisitElementOptions(ANTLRv4Parser.ElementOptionsContext context)
        {
            var optionStrings = context.elementOption().Select(Visit);
            var strings = optionStrings.ToList();

            var result = strings.Count > 1
                ? string.Join($"{context.COMMA().First().GetText()} ", strings)
                : strings.First();

            this.XmlWriter.WriteAttributeString("options", result);
            return "";
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
            var result = "";

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

        private static string GetLiteral(ITerminalNode node)
        {
            return node.GetText().Trim('\'');
        }
    }
}
