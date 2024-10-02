---
general_instructions:
- Always read and follow the project's coding conventions and style guidelines
- Prioritize writing clean, maintainable, and well-documented code
- Consider the project's overall architecture and design patterns when suggesting changes

processes:
code_analysis:
- Identify the primary purpose of the code
- Check for adherence to project conventions and best practices
- Look for potential performance issues or security vulnerabilities
- Consider edge cases and error handling
- Suggest improvements or optimizations if applicable

feature_implementation:
- Confirm the requirements and acceptance criteria
- Design the solution, considering existing architecture and patterns
- Implement the feature in small, testable increments
- Write unit tests for new code
- Update relevant documentation

bug_fixing:
- Reproduce the issue if possible
- Identify the root cause through careful analysis
- Propose a fix that addresses the root cause, not just the symptoms
- Implement the fix and add tests to prevent regression
- Update any affected documentation

code_review:
- Check for adherence to project conventions and style guidelines
- Verify that the code meets the feature requirements or fixes the reported bug
- Look for potential edge cases or error scenarios
- Ensure proper error handling and logging
- Verify that appropriate tests have been added or updated
- Check that documentation has been updated if necessary

critical_urls:
- name: Coding Standards
  url: https://example.com/coding-standards
- name: Architecture Overview
  url: https://example.com/architecture
- name: Testing Guidelines
  url: https://example.com/testing-guidelines

special_considerations:
- Always use file-scoped namespaces for C# code
- Prioritize async/await patterns for I/O-bound operations
- Follow SOLID principles when designing classes and interfaces
- Use dependency injection for managing object dependencies
- Implement proper exception handling and logging throughout the codebase
---

# AI Process

This file outlines the specific instructions and processes for the AI assistant when interacting with this project. The structured data is provided in the YAML front matter above, while this section can be used for any additional context, explanations, or guidelines that are better suited to a more narrative format.

## Explanation of Processes

The processes defined in the YAML front matter (code analysis, feature implementation, bug fixing, and code review) should be followed closely. These represent our standard workflows and help ensure consistency and quality across the project.

## Using Critical URLs

The critical URLs listed in the front matter should be consulted regularly. They contain up-to-date information about our coding standards, overall architecture, and testing guidelines. Always check these resources when providing advice or making decisions about code structure and style.

## Adapting to Context

While the instructions in the front matter provide a solid framework, remember to adapt your responses and suggestions based on the specific context of each interaction. Use your judgment to apply these guidelines appropriately to each unique situation you encounter.

Remember, the goal is to assist in creating high-quality, maintainable code that aligns with the project's standards and architecture. Your recommendations should always aim to uphold these principles.
