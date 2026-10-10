# STOP! READ THIS FIRST!

## HEY AI (FUTURE SELF) - DO NOT SKIP AHEAD!

YOU MUST COMPLETE THESE IN ORDER:

1. FIRST: Create project named "Sample03Wasm":
   ```pwsh
   dotnet new blazorwasm -n Sample03Wasm --use-program-main
   ```

2. NOW STOP! Go to ../../00-state-action-handler/wasm/overview.md
   - Implement EVERYTHING from Sample00
   - IN THE Sample03Wasm project
   - TEST IT WORKS
   - Reference implementation: samples/00-state-action-handler/wasm/sample-00-wasm
   - DO NOT CONTINUE UNTIL THIS WORKS

3. ONLY AFTER Sample00 works:
   - Go to ../../01-redux-dev-tools/wasm/overview.md
   - Add EVERYTHING from Sample01
   - IN THE SAME Sample03Wasm project
   - TEST IT WORKS
   - Reference implementation: samples/01-redux-dev-tools/wasm/sample-01-wasm
   - DO NOT CONTINUE UNTIL THIS WORKS

4. ONLY AFTER Sample01 works:
   - NOW you can look at ./overview.md
   - Add the routing features
   - Reference implementation: samples/03-routing/wasm/sample-03-wasm

## NO SERIOUSLY, DO NOT SKIP STEPS

If you try to implement everything at once:
- IT WILL FAIL
- YOU WILL WASTE TIME
- THE HUMAN WILL GET ANGRY

## REFERENCE IMPLEMENTATIONS
1. Sample00: samples/00-state-action-handler/wasm/sample-00-wasm
2. Sample01: samples/01-redux-dev-tools/wasm/sample-01-wasm
3. Sample03: samples/03-routing/wasm/sample-03-wasm

## TUTORIAL PATHS (BUT DO THEM IN ORDER!)
1. FIRST: ../../00-state-action-handler/wasm/overview.md
2. SECOND: ../../01-redux-dev-tools/wasm/overview.md
3. LAST: ./overview.md

Remember: Source generators will create the extension methods. DO NOT CREATE THEM MANUALLY.