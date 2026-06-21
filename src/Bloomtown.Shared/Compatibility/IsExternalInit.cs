// Polyfill for netstandard2.1: `init`-only property setters and `record` types are a C#
// language feature, but the compiler also requires this specific marker type to exist in
// the BCL. It was added in .NET 5 — netstandard2.1 predates that, so without this the build
// fails with CS0518 on every `init` / `record` in the project.
//
// net8.0 already ships this type for real, so this whole file compiles to nothing there
// (avoids a duplicate-type error).
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
#endif
