namespace SignalFx.Tracing.DuckTyping.Tests.Fields.TypeChaining.ProxiesDefinitions
{
    public class ObscureDuckTypeVirtualClass
    {
        [Duck(Name = "_publicStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PublicStaticReadonlySelfTypeField { get; }

        [Duck(Name = "_internalStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject InternalStaticReadonlySelfTypeField { get; }

        [Duck(Name = "_protectedStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject ProtectedStaticReadonlySelfTypeField { get; }

        [Duck(Name = "_privateStaticReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PrivateStaticReadonlySelfTypeField { get; }

        // *

        [Duck(Name = "_publicStaticSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PublicStaticSelfTypeField { get; set; }

        [Duck(Name = "_internalStaticSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject InternalStaticSelfTypeField { get; set; }

        [Duck(Name = "_protectedStaticSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject ProtectedStaticSelfTypeField { get; set; }

        [Duck(Name = "_privateStaticSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PrivateStaticSelfTypeField { get; set; }

        // *

        [Duck(Name = "_publicReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PublicReadonlySelfTypeField { get; }

        [Duck(Name = "_internalReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject InternalReadonlySelfTypeField { get; }

        [Duck(Name = "_protectedReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject ProtectedReadonlySelfTypeField { get; }

        [Duck(Name = "_privateReadonlySelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PrivateReadonlySelfTypeField { get; }

        // *

        [Duck(Name = "_publicSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PublicSelfTypeField { get; set; }

        [Duck(Name = "_internalSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject InternalSelfTypeField { get; set; }

        [Duck(Name = "_protectedSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject ProtectedSelfTypeField { get; set; }

        [Duck(Name = "_privateSelfTypeField", Kind = DuckKind.Field)]
        public virtual IDummyFieldObject PrivateSelfTypeField { get; set; }
    }
}
