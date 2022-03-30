// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Is Injected so it is created by the container. Even though the IDE says it is not used, it is.", Scope = "member", Target = "~P:Sample.Client.App.RouteManager")]
