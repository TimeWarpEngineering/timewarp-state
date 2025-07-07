# STOP! READ THIS FIRST!

## HEY AI (FUTURE SELF) - DO NOT SKIP AHEAD!

YOU MUST COMPLETE THESE IN ORDER:

1. FIRST: Create project named "Sample03Wasm":
   ```pwsh
   dotnet new blazorwasm -n Sample03Wasm --use-program-main
   ```

2. NOW STOP! Go to ../00-StateActionHandler/README.md
   - Implement EVERYTHING from Sample00
   - IN THE Sample03Wasm project
   - TEST IT WORKS
   - Reference implementation: Samples\00-StateActionHandler\Wasm\Sample00Wasm
   - DO NOT CONTINUE UNTIL THIS WORKS

3. ONLY AFTER Sample00 works:
   - Go to ../01-ReduxDevTools/README.md
   - Add EVERYTHING from Sample01
   - IN THE SAME Sample03Wasm project
   - TEST IT WORKS
   - Reference implementation: Samples\01-ReduxDevTools\Wasm\Sample01Wasm
   - DO NOT CONTINUE UNTIL THIS WORKS

4. ONLY AFTER Sample01 works:
   - NOW you can look at ./README.md
   - Add the routing features
   - Reference implementation: Samples\03-Routing\Wasm\Sample03Wasm

## NO SERIOUSLY, DO NOT SKIP STEPS

If you try to implement everything at once:
- IT WILL FAIL
- YOU WILL WASTE TIME
- THE HUMAN WILL GET ANGRY

## REFERENCE IMPLEMENTATIONS
1. Sample00: Samples\00-StateActionHandler\Wasm\Sample00Wasm
2. Sample01: Samples\01-ReduxDevTools\Wasm\Sample01Wasm
3. Sample03: Samples\03-Routing\Wasm\Sample03Wasm

## TUTORIAL PATHS (BUT DO THEM IN ORDER!)
1. FIRST: ../00-StateActionHandler/README.md
2. SECOND: ../01-ReduxDevTools/README.md
3. LAST: ./README.md

Remember: Source generators will create the extension methods. DO NOT CREATE THEM MANUALLY.