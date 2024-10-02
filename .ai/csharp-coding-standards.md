---
indentation:
  spaces: 2
  eol: LF
  align: false

bracket_alignment:
  rule: Align all brackets in the same column if not on the same line
  applies_to: ` { }, < >, ( ), [ ] `

naming_conventions:
  type_names: Explicit unless obvious from context
  namespaces: File-scoped
  private_fields: No underscore prefix
  variables: prefer naming the variable after the type
  class_scoped: PascalCase (fields, properties, methods, events)
  local_scoped: camelCase (variables inside methods)

language_features:
  - use targeted type new
  - use explicit types over var
  - prefer global usings (GlobalUsings.cs)
---

# C# Coding Standards

```csharp
namespace ExampleNamespace;

public class ExampleClass
{
  private readonly HttpClient HttpClient;
  private int Count;
  public string Name { get; set; }
  
  public void ExampleMethod()
  {
    int localVariable = 0;
    if (localVariable == 0)
    {
      // Correct bracket alignment
    }
  }
}
