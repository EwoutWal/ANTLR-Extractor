// define a grammar called Hello
grammar Hello;
r   : 'hello' ID;
ID  : ([a-z] | 'A'..'Z')+ ;
WS  : [ \t\r\n]+ -> skip ;