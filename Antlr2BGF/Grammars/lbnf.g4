lexer grammar lbnf;

/** doc comment */

import importGrammar = MyGrammar;

options {
	tokenVocab=lbnfLexer;
	tokenVocab=lbnfAnotherLexer;
	language=CSharp;
}

tokens {
	TOK1, TOK2
}

spec: TOK;

TOK: FRAG -> pushMode(TestMode);

fragment FRAG: [a-zA-Z];

mode TestMode;

TM_TOK: TM_FRAG+ -> type(TOK), popMode;

fragment TM_FRAG: [a-z] -> type(FRAG);
