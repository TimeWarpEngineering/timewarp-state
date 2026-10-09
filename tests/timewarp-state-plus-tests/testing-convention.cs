#region Purpose
// Fixie testing convention for the TimeWarp.State.Plus tests.
#endregion

#region Design
// Empty subclass, so the project uses TimeWarp.Fixie's default discovery and lifecycle. The namespace is
// TimeWarp.State.Tests, the same as the core test project.
#endregion

namespace TimeWarp.State.Tests;

class TestingConvention : TimeWarp.Fixie.TestingConvention { }
