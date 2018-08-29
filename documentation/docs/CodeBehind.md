---
uid: CodeBehind:CodeBehind.md
title: 
---

## Seperation of HTML and C#.
I like to keep my C# code separated from the HTML and yet co-located.
Instead of a `@functions` sections for each .cshtml file we 
will have an associated .cshtml.cs. file and Visual Studio will automatically group these together.
This is not required but I find it easier to reason about.

> [!NOTE]
> _Currently separation requires the page inheriting from the associated `Model` class.  
> But I believe this will become a normal feature of Blazor, similar to "Code behind."_
