#if !NET9_0_OR_GREATER

#pragma warning disable IDE0161 // Convert to file-scoped namespace

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices
{
    [ExcludeFromCodeCoverage]
    [DebuggerNonUserCode]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, Inherited = false)]
    sealed class OverloadResolutionPriorityAttribute(int priority) : Attribute
    {
        public int Priority { get; } = priority;
    }
}
#endif
