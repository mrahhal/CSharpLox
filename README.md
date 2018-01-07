# CSharpLox

A cross-platform compiler/interpreter .NET Standard implementation of the [Lox](https://github.com/munificent/craftinginterpreters) language.

## Build

Run the `build.ps1` script in the root of the project:
```
$ ./build
```

An "artifacts" folder will be created, it contains the executables, one for each platform (windows, osx, linux).
(i.e The windows interpreter is at "artifacts/interpreter/win/cslox.exe")

After building, run `basic-run.ps1` to test drive the built interpreter (on windows).
```
$ ./basic-run
Hello, World!
```

## Usage

```
$ cslox [script]
```

You can also enter prompt mode by not specifying a script path:
```
$ cslox
> print 41 + 1;
42
```

## Syntax

Lox is a simple dynamic programming language that supports object oriented aspects. It supports classes and inheritance.

A simple lox program:

```lox
class Greeter {
	init(name) {
		this.name = name;
	}

	greet() {
		print "Hello, " + this.name + "!";
	}
}

var greeter = Greeter("mrahhal");

greeter.greet();
```

## Grammar

The [context-free-grammar.md](context-free-grammar.md) file contains the grammar of the whole language, expressed using a similar metasyntax to [EBNF](https://en.wikipedia.org/wiki/Extended_Backus–Naur_form).
