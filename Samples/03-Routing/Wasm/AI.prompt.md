# AI Instructions for Sample03 - Routing Tutorial

## CRITICAL: Implementation Order

Start with Sample03 project and build it up sequentially:

1. Create the initial project AS Sample03:
   ```pwsh
   dotnet new blazorwasm -n Sample03Wasm --use-program-main
   ```

2. First implement ALL Sample00 (StateActionHandler) features:
   - Follow ALL steps from ../00-StateActionHandler/README.md
   - Implement in Sample03Wasm project
   - Verify basic state management works

3. Then add ALL Sample01 (ReduxDevTools) features:
   - Follow ALL steps from ../01-ReduxDevTools/README.md
   - Implement in the SAME Sample03Wasm project
   - Verify Redux DevTools integration works

4. ONLY THEN add Sample03 (Routing) features:
   - Continue in the SAME Sample03Wasm project
   - Build on the previously implemented features
   - Add routing capabilities from ./README.md

## Key Requirements

- Create project as Sample03Wasm from the start
- Implement features in strict sequence within the SAME project:
  1. ../00-StateActionHandler/README.md
  2. ../01-ReduxDevTools/README.md
  3. ./README.md
- DO NOT create separate projects for each sample
- Verify each feature set works before proceeding
- Implementation order: StateActionHandler → ReduxDevTools → Routing

Skipping steps or implementing out of order will result in errors.

## Reference Paths
- StateActionHandler Tutorial: ../00-StateActionHandler/README.md
- ReduxDevTools Tutorial: ../01-ReduxDevTools/README.md
- Current Routing Tutorial: ./README.md