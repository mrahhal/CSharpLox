# Context-Free Grammar

## Expressions - First Attempt

```cfg
expression → literal
           | unary
           | binary
           | grouping ;

literal    → NUMBER | STRING | "true" | "false" | "nil" ;
grouping   → "(" expression ")" ;
unary      → ( "-" | "!" ) expression ;
binary     → expression operator expression ;
operator   → "==" | "!=" | "<" | "<=" | ">" | ">="
           | "+"  | "-"  | "*" | "/" ;
```

## Expressions

```cfg
expression     → assignment ;
assignment     → identifier "=" assignment
               | equality ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → addition ( ( ">" | ">=" | "<" | "<=" ) addition )* ;
addition       → multiplication ( ( "-" | "+" ) multiplication )* ;
multiplication → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary
               | primary ;
primary        → "true" | "false" | "nil" | "this"
               | NUMBER | STRING
               | "(" expression ")"
               | IDENTIFIER ;
```

## Statements

```cfg
program     → declaration* eof ;

declaration → varDecl
            | statement ;

varDecl     → "var" IDENTIFIER ( "=" expression )? ";" ;

statement   → exprStmt
            | printStmt ;

exprStmt    → expression ";" ;
printStmt   → "print" expression ";" ;
```
