# Work Item CheckList

- [ ] Add Placeholder test projects
  - [x] Add HercPwa.Server.Integration.Tests project
  - [ ] Add HercPwa.Client.Integration.Tests project
    - [ ] Add Respawn and Configure
  - [ ] Add HercPwa.Server.Unit.Tests project
  - [ ] Add HercPwa.Client.Unit.Tests project
- [ ] Add Blazor-State to the app and determine if update required
  > Cannot even add it to the project without the update. 
- [ ] Move over Edge Login Test Page from proof of concept app
- [ ] update Blazor-State.Js npm packages 

- [x] Add Entity Framework to project
- [x] Add DbInitializer
  - [x] Initialize Application Object
- [x] Add MediatR to Server and configure in Startup
- [x] Add Scrutor and configure in Startup
- [ ] Add the Api Docs to docs project


## Api Features
GetApplication
GetAssetDefinition
CreateAssetDefinition

### Feature GetApplication
- [x] Feature GetApplication
  - [x] Implementation
    - [x] *Controller
    - [x] *Request
    - [x] *Response
    - [x] *Handler 
    - [x] Mapper 
    - [x] Validator (NA empty request)
  - [x] Integration Tests
    - [x] Add Respawn to use with DB testing
    - [x] Add SliceFixture
    - [x] *Sunny Day Scenario
    - [x] *Validator Test (NA)
    - [x] Negative Testing 
      > No negative testing done. NA
  - [x] Documentation
    - [x] *Request class and properties
    - [x] *Response class and properties

### Feature GetAssetDefinition
- [x] Feature GetAssetDefinition
  - [x] Implementation
    - [x] *Controller
    - [x] *Request
    - [x] *Response
    - [x] *Handler 
    - [x] Mapper 
    - [x] Validator 
  - [x] Integration Tests
    - [x] *Sunny Day Scenario
    - [x] *Validation Test
    - [x] Negative Testing 
        > Check for when ID not found.
  - [x] Documentation
    - [x] *Request class and properties
    - [x] *Response class and properties
    - [x] *Update Model
      > Just reverse engineered the model from source.

### Feature CreateAssetDefinition

- [x] Implementation
  - [x] *Controller
  - [x] *Request
  - [x] *Response
  - [x] *Handler 
  - [x] Mapper 
  - [x] Validation
- [x] Integration Tests
  - [x] *Sunny Day Scenario
  - [x] *Validation Test
  - [ ] Negative Testing
    > Not sure other than validation tests what is a good negative test?
- [x] Documentation
  - [x] *Request class and properties
  - [x] *Response class and properties

### Refactor
  - [ ] Use GUID for ID vs Int.  Better to do early than late. Given blockchain and distributed DBs probably wise to have Unique across more than just the one DB.  
  - [ ] Investigate: file upload for images and refactor appropriately with validation on client and server.

