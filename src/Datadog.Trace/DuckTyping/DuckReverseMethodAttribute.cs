using System;

namespace SignalFx.Tracing.DuckTyping
{
    /// <summary>
    /// Duck reverse method attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DuckReverseMethodAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuckReverseMethodAttribute"/> class.
        /// </summary>
        public DuckReverseMethodAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuckReverseMethodAttribute"/> class.
        /// </summary>
        /// <param name="arguments">Methods arguments</param>
        public DuckReverseMethodAttribute(params string[] arguments)
        {
            Arguments = arguments;
        }

        /// <summary>
        /// Gets the methods arguments
        /// </summary>
        public string[] Arguments { get; private set; }
    }
}
