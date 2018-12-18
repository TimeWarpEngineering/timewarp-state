# New ideas

## Product Ideas
- [ ] Dark Theme everything
  > After cataract surgery one apprciates dark them much more.
  - [ ] Support Themes
    - [ ] Investigate proper way to do themes in Blazor.

## Documentation
- [] Toc Generate
  > See https://github.com/dotnet/docfx/issues/3124

## Code Generation
- [ ] Review Code Generation to improve productivity
  - [ ] Create template for creating a Feature
    >https://blogs.msdn.microsoft.com/dotnet/2017/04/02/how-to-create-your-own-templates-for-dotnet-new/
  - [ ] Consider Protobuf and gRPC... I think better than REST in the long run. (for my style MediatR)
  - [ ] Custom Analyizer and code fix 
    > https://github.com/dotnet/roslyn/wiki/How-To-Write-a-C%23-Analyzer-and-Code-Fix
  - [ ] T4 templates 
  >https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates

 - [ ] Look at StarUml Code gen https://github.com/staruml/mdgen


## Herc.Pwa Model 

- [ ] Unit of Measure needs vetted.  Maybe look at "Enterprise Applications MDA book"

### Add Share button to UI 
  - [ ] Social Media Share
    > See Stacks UI for details.
    > https://github.com/HERCone/NUI-UI-Layer/blob/b9113fe645b998d0f295ad44e241425617c3e49c/components/TxSwiper.js#L184


## Negative testing ideas from Pete

1. So if you have a service for validation, make sure it doesn't save if validation service says it is invalid?
2: Create something that violates a unique index (that the validation service cannot detect due to concurrency)
3: Person not permitted to create
4: Person not signed in
5. Database down can not connect.