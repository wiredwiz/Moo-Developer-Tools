# Moo Developer Tools

[![Build Status](https://github.com/wiredwiz/Moo-Developer-Tools/actions/workflows/dotnet.yml/badge.svg)](https://github.com/wiredwiz/Moo-Developer-Tools/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/badge/license-BSD-blue.svg)](https://raw.githubusercontent.com/wiredwiz/Moo-Developer-Tools/master/LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com)

These are a set of tools to aid Moo developers in writing and debugging code faster as well as 
creating higher quality code.  The primary focus of this library is the Moo Udditor cilent.

![Editor Sample](https://github.com/wiredwiz/Moo-Developer-Tools/blob/Assets/RepositoryAssets/UdditorDemo.png?raw=true)

It provides
a client for connecting to Moos as well as a modern editor for editing Moo code.  It has a full language
parser embedded within the editor and gives real-time feedback of any syntax errors in your code.
Currently the editor supports the standard Moo local edit OOB protocol, but soon it will support various
MCP packages as well.  It supports three different dialects of Moo grammar.  The first is the original
Lambdamoo syntax.  Second is the ToastStuntMoo syntax, which added various new operators and types to the
language.  Lastly is the EdgerunnerMoo syntax, which is my personal fork of the base ToastStunt.  It adds
compound assignment operators as well as prefix/postfix increment and decrement operators.

More documentation to come later..

Attribution:
Program logo derived from an open use image, created by author Onsemeliot on OpenClipart.
