DEVELOPMENT PROCESS:

KANBAN STRUCTURE:
- Track work using Kanban tasks
- Folders:
  - Kanban/Backlog/
  - Kanban/ToDo/
  - Kanban/InProgress/
  - Kanban/Done/

TASK MANAGEMENT:
- Task Template Location: `Kanban\Task-Template.md`
- Task File Format: <TaskID>_<Description>.md
  ✓ `002_Create-Game-Logic.md`

COMMIT CONVENTIONS:
- Make git commits between steps
- Format: Task: <TaskID> = <Status> <Description>
  ✓ `Task: 002 = Complete Create Game Logic`

TASK WORKFLOW:
✓ Example of proper task movement:
```pwsh
git mv Kanban/InProgress/002_Create-Game-Logic.md Kanban/Done/002_Create-Game-Logic.md
git commit -m "Task: 002 = Complete Create Game Logic"
```
