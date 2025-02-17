// Modified by SignalFx
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SignalFx.Tracing.Logging;

namespace SignalFx.Tracing.OpenTracing
{
    internal class OpenTracingSpanContext : global::OpenTracing.ISpanContext
    {
        private static readonly SignalFx.Tracing.Vendors.Serilog.ILogger Log = SignalFxLogging.For<OpenTracingSpanContext>();

        public OpenTracingSpanContext(ISpanContext context)
        {
            Context = context;
        }

        public string TraceId => Context.TraceId.ToString();

        public string SpanId => Context.SpanId.ToString(CultureInfo.InvariantCulture);

        internal ISpanContext Context { get; }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }
    }
}
