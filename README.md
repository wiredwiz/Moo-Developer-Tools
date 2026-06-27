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

The editor also provides intelligent **code completion** and **hover tooltips** — for an object's
verbs and properties, built-in functions, and Moo literal constants (type codes such as `NUM => 0`
and error values such as `E_PERM => Permission denied`). The object-aware parts of these features
(listing a target object's verbs/properties, resolving member chains, and fetching live constant
values from the running Moo) work by querying the connected server over the **`edgerunner-org-moo-query`
MCP package**; when that package isn't installed the editor falls back to its built-in knowledge. To
enable the live, server-aware behavior you must install that package on your Moo — see the
[installation instructions](Server%20Packages/edgerunner-org-moo-query-INSTALL.md), and use the
[package dump file](Server%20Packages/edgerunner-org-moo-query.moo) to create the package on your server.

More documentation to come later..

Attribution:
Program logo derived from an open use image, created by author Onsemeliot on OpenClipart.
