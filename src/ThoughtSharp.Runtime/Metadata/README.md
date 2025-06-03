# ThoughtSharp.Runtime

The runtime engine that powers everything in ThoughtSharp.

This package defines the core abstractions and execution model that all other ThoughtSharp components depend on.
It is the foundational building block for any program written in or executed by the ThoughtSharp system.

- All ThoughtSharp packages (except the code generator) reference this package.
- The code generator assumes your `.csproj` already depends on this package.

## Installation

```bash
dotnet add package ThoughtSharp.Runtime
```