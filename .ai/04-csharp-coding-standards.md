C# CODING RULES:

INDENTATION:
- Use 2 spaces (no tabs)
- Use LF line endings

BRACKET ALIGNMENT (Allman Style):
- All bracket types must be on their own line, aligned with the parent construct
- Applies to: { }, < >, ( ), [ ]
- Each opening and closing bracket gets its own line

✓ Correct indentation:
```csharp
public class Example
{
  private void Method(string param1, string param2)
  {
    List<int> numbers = new List<int>
    [
      1,
      2,
      3
    ];
    
    if (param1 == "test")
    {
      Dictionary<string, int> map = new()
      {
        ["key1"] = 1,
        ["key2"] = 2
      };
      
      DoSomething
      (
        param1,
        param2
      );
    }
  }
}
```

✗ Incorrect indentation:
```csharp
public class Example
{
    private void Method()  // Wrong - 4 spaces
    {
        DoSomething();
    }
}
```

NAMING CONVENTIONS:
1. Private Fields
   - No underscore prefix
   ✓ `private readonly HttpClient httpClient;`
   ✗ `private readonly HttpClient _httpClient;`

2. Scope-based Casing
   - Class Scope: PascalCase for all members
     ```csharp
     private readonly HttpClient HttpClient;     // Field
     private int RequestCount;                   // Field
     public string UserName { get; set; }        // Property
     public void HandleRequest() { }             // Method
     public event EventHandler DataChanged;      // Event
     ```
   - Method Scope: camelCase for local variables and parameters
     ```csharp
     public void ProcessData(string inputValue)  // Parameter in camelCase
     {
       int itemCount = 0;                        // Local variable
       string userName = GetUserName();          // Local variable
     }
     ```

LANGUAGE FEATURES:
1. Type Declaration
   - Use var only when type is apparent from right side

   ✓ `List<int> list = new();               // Type explicitly declared`
   ✓ `var customer = new Customer();        // Type apparent from new`
   ✓ `int count = 1 + 2;                    // Use explicit type for built-in types`
   
   ✗ `var items = GetItems();               // Type not apparent`
   ✗ `var count = 1 + 2;                    // Don't use var for built-in types`
   ✗ `var customer = await GetCustomer();   // Type not apparent`

2. New Operator
   - Use targeted type new
   ✓ `HttpClient client = new();`
   ✗ `HttpClient client = new HttpClient();`

3. Namespaces
   - Use file-scoped namespaces
   ✓ `namespace ExampleNamespace;`
   ✗ `namespace ExampleNamespace { ... }`

4. Using Statements
   - Prefer global usings in GlobalUsings.cs   

      ✓ Place in GlobalUsings.cs:
        ```csharp
        global using System;
        global using System.Collections.Generic;
        ```

      ✗ Don't place at top of each file:
        ```csharp
        using System;
        using System.Collections.Generic;
        ```

EXAMPLE CLASS PUTTING IT ALL TOGETHER:

```csharp
namespace ExampleNamespace;

public class UserService
{
  private readonly HttpClient HttpClient;
  private readonly Dictionary<string, UserData> CachedUsers;
  private int RequestCount;

  public string UserName { get; set; }
  
  public UserService
  (
    HttpClient httpClient,
    Dictionary<string, UserData> initialCache
  )
  {
    HttpClient = httpClient;
    CachedUsers = initialCache ?? new Dictionary<string, UserData>
    {
      ["default"] = new UserData
      {
        Id = "0",
        Name = "Default User"
      }
    };
  }

  public async Task<List<UserData>> GetUsersAsync
  (
    string[] userIds,
    bool useCache
  )
  {
    List<UserData> results = new();

    foreach (string userId in userIds)
    {
      if (useCache && CachedUsers.TryGetValue(userId, out UserData cachedData))
      {
        results.Add(cachedData);
      }
      else
      {
        string requestUrl = $"/users/{userId}";
        HttpResponseMessage response = await HttpClient.GetAsync(requestUrl);
        
        if (response.IsSuccessStatusCode)
        {
          UserData userData = await response.Content.ReadFromJsonAsync<UserData>();
          results.Add(userData);
          CachedUsers[userId] = userData;
        }
      }
      
      RequestCount++;
    }

    return results;
  }
}
