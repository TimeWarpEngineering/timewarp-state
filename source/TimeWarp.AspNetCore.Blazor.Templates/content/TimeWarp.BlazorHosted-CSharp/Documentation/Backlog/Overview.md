# The Backlog

This is a text based system. You may be surprised how productive text files and checklists are.
The learning curve is low and flexibility is high.

All hierarchy of the backlog is maintained with just folders.
If you need to group something for clarity add a folder.

The backlog consists of 3 groups of tasks
* `New` Tasks that need to be triaged (reviewed to determine if they meet the definition of ready)
* `Ready` Tasks that have been successfully triaged but not yet placed in an iteration.
* `Iterations` Tasks that are in an iteration.

### New Tasks

We want to capture ideas, and we want that to be as simple 
as possible, so these ideas won't be lost.
Just use a file or folder under the `New` folder to capture as much detail as you like.

I recommend you add your proposed new Tasks in a folder with your name on it.

```
├───New
│   │
│   └───Cramer
│           NewTaskIdea1.md
│           NewTaskIdeaList.md
```

#### ReadyForReview

When you think you have your task ready for review 
you can move it to the `ReadyForReview` Folder.

#### Triage

The Triage group will review the items in the `ReadyForReview` 
folder and if approved they will be moved to the `Ready` folder 
if not they will be moved back to the responsible parties folder.

### Ready Tasks

This folder will contain the Items we think meet the definition of Ready.
The Checklist.md file will maintain the priority of the tasks.
To change the priority just move the task up or down in the checklist (Alt-Arrow)


### Tasks:

Tasks have a status and whatever documentation required to define it (one should see Definition of Ready).

#### Task Description:
This can be as simple as the text for the check-box, a single file or a folder with multiple files.

   **Text:**

   ```
    - [ ] <Task Description>
   ```

   **File:**

   MyTask.md

   **Folder:**

```
└───Iteration1
    │   Checklist.md
    │   Summary.md
    │
    └───MyTask
            MyTask.md
            MyTaskSupportingDocument.md
```

#### Status
Tasks status is represented by a simple checkbox:
```
- [ ] Todo
- [-] In progress
- [x] Complete
```

If you think you need to track percentage complete the task is probably too big and should be split. 
But you can track it if you like.( I told you it was flexible)
```
- [-] (%75)In progress
- [-] (3h/8h)In progress
```

### Iterations

Waterfall has one iteration. Agile/Scrum have more.  
The TimeWarp Process allows you to have as many or few as you like.

The Default Template has the following folder structure.

```
├───Iterations
│   ├───Current
│   │   └───Iteration2
│   ├───Future
│   │   └───Iteration3
│   │   └───Iteration4
│   └───Past
│       └───Iteration1
```


This will be the items we are working on in this iteration/Sprint.
We will use a checklist in markdown file somewhere to easily see what is done.
