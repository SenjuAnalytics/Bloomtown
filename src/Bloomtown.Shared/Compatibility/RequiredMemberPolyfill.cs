// Polyfill for netstandard2.1: the `required` member modifier (C# 11) needs these two
// attribute types to exist in the BCL. They were added in .NET 7 — netstandard2.1 predates
// that, so without this every `required` property fails with CS0656.
//
// net8.0 already ships both types for real, so this whole file compiles to nothing there
// (avoids a duplicate-type error).
#if !NET7_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; }

        public bool IsOptional { get; init; }

        public const string RefStructs = nameof(RefStructs);
        public const string RequiredMembers = nameof(RequiredMembers);
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    // Lets a constructor promise it already sets every `required` member, so callers
    // aren't forced to use object-initializer syntax. Not strictly used yet in this
    // codebase, but cheap to include alongside the two attributes above.
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute
    {
    }
}
#endif
