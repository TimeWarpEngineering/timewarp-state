# Kanban Board

This Kanban board helps manage and track tasks for the project using a simple folder structure.
Each task is represented by a Markdown file, and the status of a task is indicated by the folder it is in.

## Folders

1. **Backlog**: Contains tasks that are NOT yet ready to be worked on. These tasks have a temporary backlog scoped unique identifier.
2. **ToDo**: Contains tasks that are ready to be worked on. When a task from the Backlog becomes ready, it is assigned a unique identifier and moved to this folder.
3. **InProgress**: Contains tasks that are currently being worked on.
4. **Done**: Contains tasks that have been completed.

## File Naming Convention

- For tasks in the Backlog folder, use a short description with a 'B' prefix followed by a three-digit identifier,
such as `B001_Research-Authentication-Methods.md` or `B002_Design-Game-Rules.md`.
- When a task becomes "Ready," assign it a unique identifier (without the 'B' prefix) and move it to the ToDo folder, e.g.,
`001_Implement-User-Registration.md` or `002_Create-Game-Logic.md`.
- <3 digit Id>_<Short-Description-seperated-by-hyphens>

## Workflow

1. Create a task in the Backlog folder with a short description as the filename.
2. When a task is ready to be worked on, assign it a unique identifier and move it to the ToDo folder.
3. As you work on tasks, move them to the InProgress folder.
4. When a task is complete, move it to the Done folder.
