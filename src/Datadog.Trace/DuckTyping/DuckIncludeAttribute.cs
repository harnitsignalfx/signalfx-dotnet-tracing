using System;

namespace SignalFx.Tracing.DuckTyping
{
    /// <summary>
    /// Use to include a member that would normally be ignored
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DuckIncludeAttribute : Attribute
    {
    }
}
