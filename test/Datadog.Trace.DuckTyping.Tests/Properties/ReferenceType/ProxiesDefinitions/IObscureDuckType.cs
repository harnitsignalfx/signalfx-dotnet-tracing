namespace SignalFx.Tracing.DuckTyping.Tests.Properties.ReferenceType.ProxiesDefinitions
{
    public interface IObscureDuckType
    {
        string PublicStaticGetReferenceType { get; }

        string InternalStaticGetReferenceType { get; }

        string ProtectedStaticGetReferenceType { get; }

        string PrivateStaticGetReferenceType { get; }

        // *

        string PublicStaticGetSetReferenceType { get; set; }

        string InternalStaticGetSetReferenceType { get; set; }

        string ProtectedStaticGetSetReferenceType { get; set; }

        string PrivateStaticGetSetReferenceType { get; set; }

        // *

        string PublicGetReferenceType { get; }

        string InternalGetReferenceType { get; }

        string ProtectedGetReferenceType { get; }

        string PrivateGetReferenceType { get; }

        // *

        string PublicGetSetReferenceType { get; set; }

        string InternalGetSetReferenceType { get; set; }

        string ProtectedGetSetReferenceType { get; set; }

        string PrivateGetSetReferenceType { get; set; }

        // *

        [Duck(Name = "PublicStaticGetSetReferenceType")]
        string PublicStaticOnlySet { set; }

        // *

        string this[string index] { get; set; }
    }
}
