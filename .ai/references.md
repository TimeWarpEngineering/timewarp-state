---
references:
  - name: C#
    relationship: Main programming language for solution
    resources:
      - Official Documentation: https://learn.microsoft.com/en-us/dotnet/csharp/

  - name: TypeScript
    relationship: Language used for JavaScript interop
    resources:
      - Official Documentation: https://www.typescriptlang.org/docs/
      - TypeScript Handbook: https://www.typescriptlang.org/docs/handbook/intro.html
      - TypeScript Deep Dive: https://basarat.gitbook.io/typescript/

  - name: TimeWarp.Fixie
    relationship: Use this NuGet for creating a testing convention for integration and unit tests.
    resources:
      - TimeWarp Fixie testing convention: https://github.com/TimeWarpEngineering/timewarp-fixie

  - name: Fixie
    relationship: Testing framework for integration and unit tests
    resources:
      - Official Documentation: https://github.com/fixie/fixie/wiki

  - name: FluentAssertions
    relationship: Assertion library used for tests
    resources:
      - Official Documentation: https://fluentassertions.com/introduction
      - GitHub Repository: https://github.com/fluentassertions/fluentassertions

  - name: Playwright for .NET
    relationship: End to end testing framework
    resources:
      - Official Documentation: https://playwright.dev/dotnet/docs/intro

  - name: MediatR NuGet Library
    relationship: Used extensively by TimeWarp.State
    resources:
      - Official Documentation: https://github.com/jbogard/MediatR/wiki
      - Authors Blog: https://www.jimmybogard.com/
        
  - name: AnyClone NuGet Library
    relationship: Used as the default cloning ability if ICloneable has not been implemented.
    resources:
      - Official Repository: https://github.com/replaysMike/AnyClone

  - name: YAML
    relationship: Used in .yaml and .yml files
    resources:
      - Official Specification: https://yaml.org/spec/1.2.2/
      - Learn YAML in Y minutes: https://learnxinyminutes.com/docs/yaml/
      - Common YAML Gotchas: https://stackoverflow.com/questions/3790454/how-do-i-break-a-string-over-multiple-lines

  - name: JSON
    relationship: Used in .json files
    resources:
      - Official Specification: https://www.json.org/json-en.html
      - JSON Schema: https://json-schema.org/learn/getting-started-step-by-step
      - Working with JSON in TypeScript: https://blog.logrocket.com/working-with-json-typescript/

  - name: Markdown
    relationship: Used for documentation and ai.context files
    resources:
      - Official Guide: https://www.markdownguide.org/
      - GitHub Flavored Markdown Spec: https://github.github.com/gfm/
      - Mastering Markdown: https://guides.github.com/features/mastering-markdown/
---

This file contains a list of external dependencies and libraries.
The YAML front-matter above provides structured data about each dependency, 
including its relationship to the project and links to relevant resources. 
These resources include official documentation, articles, blogs, and Stack Overflow posts. 
This structure allows for easy parsing by automated tools while maintaining readability for developers.
