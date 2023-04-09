// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "https://github.com/dotnet/efcore/issues/26580", Scope = "type", Target = "~T:SqliteManager.DbManager")]
[assembly: SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "https://github.com/dotnet/efcore/issues/26580", Scope = "member", Target = "~M:SqliteManager.DbManager.Dispose")]
