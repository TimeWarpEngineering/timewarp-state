#region Purpose
// Project-wide global usings for the TimeWarp.State analyzer code fixes.
#endregion

#region Design
// One list so individual files need no using directives; no other design decisions.
#endregion

global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CodeActions;
global using Microsoft.CodeAnalysis.CodeFixes;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.Simplification;
global using Microsoft.CodeAnalysis.Text;
global using System.Collections.Immutable;
global using System.Composition;
global using System.Linq;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;
global using TimeWarp.State.Analyzer;
