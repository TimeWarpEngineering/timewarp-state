# Coding Standards

- 2 space indentation
- indent do NOT align
- align all brackets in the same column {} <> () [] that are not on the same line
- explicit type names unless it is obvious from the context
- file scoped namespaces
- DO NOT USE UNDERSCORE prefixing for private fields
- use PascalCase for ALL class scoped names, including:
  - Public, protected, and private fields
  - Properties
  - Methods
  - Events
- use camelCase for local scoped names (variables inside methods)

Examples:
```csharp
public class ExampleClass
{
  private readonly HttpClient HttpClient; // Correct
  private int Count; // Correct
  public string Name { get; set; } // Correct
  
  public void ExampleMethod()
  {
    int localVariable = 0; // Correct (local scope)
    // ...
  }
}
```
