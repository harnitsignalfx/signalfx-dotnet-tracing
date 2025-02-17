using System;

namespace SignalFx.Tracing.DuckTyping
{
    internal readonly struct TypesTuple : IEquatable<TypesTuple>
    {
        /// <summary>
        /// The proxy definition type
        /// </summary>
        public readonly Type ProxyDefinitionType;

        /// <summary>
        /// The target type
        /// </summary>
        public readonly Type TargetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypesTuple"/> struct.
        /// </summary>
        /// <param name="proxyDefinitionType">The proxy definition type</param>
        /// <param name="targetType">The target type</param>
        public TypesTuple(Type proxyDefinitionType, Type targetType)
        {
            ProxyDefinitionType = proxyDefinitionType;
            TargetType = targetType;
        }

        /// <summary>
        /// Gets the struct hashcode
        /// </summary>
        /// <returns>Hashcode</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)2166136261;
                hash = (hash ^ ProxyDefinitionType.GetHashCode()) * 16777619;
                hash = (hash ^ TargetType.GetHashCode()) * 16777619;
                return hash;
            }
        }

        /// <summary>
        /// Gets if the struct is equal to other object or struct
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if both are equals; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is TypesTuple vTuple &&
                   ProxyDefinitionType == vTuple.ProxyDefinitionType &&
                   TargetType == vTuple.TargetType;
        }

        /// <inheritdoc />
        public bool Equals(TypesTuple other)
        {
            return ProxyDefinitionType == other.ProxyDefinitionType &&
                   TargetType == other.TargetType;
        }
    }
}
