// Modified by SignalFx

using SignalFx.Tracing;
using SignalFx.Tracing.OpenTracing;
using SignalFx.Tracing.Propagation;
using Xunit;

namespace Datadog.Trace.OpenTracing.Tests
{
    public class HttpHeaderCodecTests
    {
        // The values are duplicated here to make sure that if they are changed it will break tests
        private const string HttpHeaderTraceId = "x-b3-traceid";
        private const string HttpHeaderParentId = "x-b3-spanid";
        private const string HttpHeaderSamplingPriority = "x-b3-sampled";
        private const string HttpHeaderDebugSamplingPriority = "x-b3-flags";

        private readonly HttpHeadersCodec _codec = new HttpHeadersCodec(new B3SpanContextPropagator());

        [Fact]
        public void Extract_ValidParentAndTraceId_ProperSpanContext()
        {
            var traceId = TraceId.CreateFromInt(10);
            const ulong parentId = 120;

            var headers = new MockTextMap();
            headers.Set(HttpHeaderTraceId, traceId.ToString());
            headers.Set(HttpHeaderParentId, parentId.ToString("x16"));

            var spanContext = _codec.Extract(headers) as OpenTracingSpanContext;

            Assert.NotNull(spanContext);
            Assert.Equal(traceId, spanContext.Context.TraceId);
            Assert.Equal(parentId, spanContext.Context.SpanId);
        }

        [Fact]
        public void Extract_WrongHeaderCase_ExtractionStillWorks()
        {
            var traceId = TraceId.CreateFromInt(10);
            const ulong parentId = 120;
            const SamplingPriority samplingPriority = SamplingPriority.AutoKeep;

            var headers = new MockTextMap();
            headers.Set(HttpHeaderTraceId.ToUpper(), traceId.ToString());
            headers.Set(HttpHeaderParentId.ToUpper(), parentId.ToString("x16"));
            headers.Set(HttpHeaderSamplingPriority.ToUpper(), ((int)samplingPriority).ToString());

            var spanContext = _codec.Extract(headers) as OpenTracingSpanContext;

            Assert.NotNull(spanContext);
            Assert.Equal(traceId, spanContext.Context.TraceId);
            Assert.Equal(parentId, spanContext.Context.SpanId);
        }

        [Fact]
        public void Inject_SpanContext_HeadersWithCorrectInfo()
        {
            const ulong spanId = 10;
            var traceId = TraceId.CreateFromInt(7);
            const SamplingPriority samplingPriority = SamplingPriority.UserKeep;

            var ddSpanContext = new SpanContext(traceId, spanId, samplingPriority);
            var spanContext = new OpenTracingSpanContext(ddSpanContext);
            var headers = new MockTextMap();

            _codec.Inject(spanContext, headers);

            Assert.Equal(spanId.ToString("x16"), headers.Get(HttpHeaderParentId));
            Assert.Equal(traceId.ToString(), headers.Get(HttpHeaderTraceId));
            Assert.Equal("1", headers.Get(HttpHeaderDebugSamplingPriority));
        }
    }
}
