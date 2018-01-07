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

## The syntax

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
