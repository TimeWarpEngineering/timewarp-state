# Developer Documentation Infrastructure

# Synopsis
Developers can access a documentation portal consisting of conceptual documents and reference-level documents linked back to source code, so that they can work productively within the code base.

# Key Features
* Use DocFX as a basis for creating statically-generated documentation pages
* The DocFX pages are automatically run as part of the CI build
* DocFX pages are automatically deployed to a location that developers can access
* Template documentation pages are in place that developers can add to in future user stories

# Acceptance Criteria

- [ ] Developers have access to an effective source of technical 
information that they will need to be productive as the project increases in complexity
- [ ] Developers have a starting point with some templates 
that point the way towards how they should maintain the documentation as the project evolves
- [ ] Documentation is baked into the normal developer workflow (CI builds and deploy)

# Tasks
- [x] Ensure DocFx is installed and latest version.
- [x] Add DocFx to the Development Environment document.
- [x] Initialize/run DocFx Project
- [x] Add DocFx build to build scripts
- [x] Add DocFx build to CI
- [x] Add DocFx build to CD
