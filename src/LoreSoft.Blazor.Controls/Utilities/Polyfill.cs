
#if !NET9_0_OR_GREATER
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
