// Modified by SignalFx
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.Emit;
using SignalFx.Tracing;
using SignalFx.Tracing.ExtensionMethods;
using SignalFx.Tracing.Logging;
using SignalFx.Tracing.Propagation;

namespace Datadog.Trace.ClrProfiler.Integrations
{
    /// <summary>
    /// Tracer integration for WebRequest.
    /// </summary>
    public static class WebRequestIntegration
    {
        private const string WebRequestTypeName = "System.Net.WebRequest";
        private const string IntegrationName = "WebRequest";
        private const string Major4 = "4";
        private const string Major5 = "5";

        private static readonly SignalFx.Tracing.Vendors.Serilog.ILogger Log = SignalFxLogging.GetLogger(typeof(WebRequestIntegration));

        /// <summary>
        /// Instrumentation wrapper for <see cref="WebRequest.GetRequestStream"/>.
        /// </summary>
        /// <param name="webRequest">The <see cref="WebRequest"/> instance to instrument.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>Returns the value returned by the inner method call.</returns>
        [InterceptMethod(
            TargetAssembly = "System", // .NET Framework
            TargetType = WebRequestTypeName,
            TargetSignatureTypes = new[] { "System.IO.Stream" },
            TargetMinimumVersion = Major4,
            TargetMaximumVersion = Major5)]
        [InterceptMethod(
            TargetAssembly = "System.Net.Requests", // .NET Core
            TargetType = WebRequestTypeName,
            TargetSignatureTypes = new[] { "System.IO.Stream" },
            TargetMinimumVersion = Major4,
            TargetMaximumVersion = Major5)]
        public static object GetRequestStream(object webRequest, int opCode, int mdToken, long moduleVersionPtr)
        {
            if (webRequest == null)
            {
                throw new ArgumentNullException(nameof(webRequest));
            }

            const string methodName = nameof(GetRequestStream);

            Func<object, Stream> callGetRequestStream;

            try
            {
                var instrumentedType = webRequest.GetInstrumentedType("System.Net.WebRequest");
                callGetRequestStream =
                    MethodBuilder<Func<object, Stream>>
                        .Start(moduleVersionPtr, mdToken, opCode, methodName)
                        .WithConcreteType(instrumentedType)
                        .WithNamespaceAndNameFilters("System.IO.Stream")
                        .Build();
            }
            catch (Exception ex)
            {
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: WebRequestTypeName,
                    methodName: methodName,
                    instanceType: webRequest.GetType().AssemblyQualifiedName);
                throw;
            }

            var request = (WebRequest)webRequest;

            if (!(request is HttpWebRequest) || !IsTracingEnabled(request))
            {
                return callGetRequestStream(webRequest);
            }

            var spanContext = ScopeFactory.CreateHttpSpanContext(Tracer.Instance, request.Method, request.RequestUri, IntegrationName);
            if (spanContext != null)
            {
                // Add distributed tracing headers to the HTTP request. The actual span is going to be created
                // when GetResponse is called.
                Tracer.Instance.Propagator.Inject(spanContext, request.Headers.Wrap());
            }

            return callGetRequestStream(webRequest);
        }

        /// <summary>
        /// Instrumentation wrapper for <see cref="WebRequest.GetResponse"/>.
        /// </summary>
        /// <param name="webRequest">The <see cref="WebRequest"/> instance to instrument.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>Returns the value returned by the inner method call.</returns>
        [InterceptMethod(
            TargetAssembly = "System", // .NET Framework
            TargetType = WebRequestTypeName,
            TargetSignatureTypes = new[] { "System.Net.WebResponse" },
            TargetMinimumVersion = Major4,
            TargetMaximumVersion = Major5)]
        [InterceptMethod(
            TargetAssembly = "System.Net.Requests", // .NET Core
            TargetType = WebRequestTypeName,
            TargetSignatureTypes = new[] { "System.Net.WebResponse" },
            TargetMinimumVersion = Major4,
            TargetMaximumVersion = Major5)]
        public static object GetResponse(object webRequest, int opCode, int mdToken, long moduleVersionPtr)
        {
            if (webRequest == null)
            {
                throw new ArgumentNullException(nameof(webRequest));
            }

            const string methodName = nameof(GetResponse);

            Func<object, WebResponse> callGetResponse;

            try
            {
                var instrumentedType = webRequest.GetInstrumentedType("System.Net.WebRequest");
                callGetResponse =
                    MethodBuilder<Func<object, WebResponse>>
                        .Start(moduleVersionPtr, mdToken, opCode, methodName)
                        .WithConcreteType(instrumentedType)
                        .WithNamespaceAndNameFilters("System.Net.WebResponse")
                        .Build();
            }
            catch (Exception ex)
            {
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: WebRequestTypeName,
                    methodName: methodName,
                    instanceType: webRequest.GetType().AssemblyQualifiedName);
                throw;
            }

            var request = (WebRequest)webRequest;

            if (!(request is HttpWebRequest) || !IsTracingEnabled(request))
            {
                return callGetResponse(webRequest);
            }

            // The headers may have been set/propagated to the server on a previous method call, but no actual span was created for it yet.
            // Try to extract the context and if available use the already propagated span ID.
            var headers = request?.Headers?.Wrap();
            SpanContext spanContext = null;
            if (headers != null)
            {
                spanContext = Tracer.Instance.Propagator.Extract(headers);
            }

            using (var scope = ScopeFactory.CreateOutboundHttpScope(Tracer.Instance, request.Method, request.RequestUri, IntegrationName, propagatedSpanId: spanContext?.SpanId))
            {
                try
                {
                    if (scope != null)
                    {
                        // add distributed tracing headers to the HTTP request
                        Tracer.Instance.Propagator.Inject(scope.Span.Context, request.Headers.Wrap());
                    }

                    WebResponse response = callGetResponse(webRequest);

                    if (scope != null && response is HttpWebResponse webResponse)
                    {
                        scope.Span.SetTag(Tags.HttpStatusCode, ((int)webResponse.StatusCode).ToString());
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    scope?.Span.SetException(ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Instrumentation wrapper for <see cref="WebRequest.GetResponseAsync"/>.
        /// </summary>
        /// <param name="webRequest">The <see cref="WebRequest"/> instance to instrument.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>Returns the value returned by the inner method call.</returns>
        [InterceptMethod(
            TargetAssembly = "System", // .NET Framework
            TargetType = WebRequestTypeName,
            TargetSignatureTypes = new[] { "System.Threading.Tasks.Task`1<System.Net.WebResponse>" },
            TargetMinimumVersion = Major4,
            TargetMaximumVersion = Major5)]
        [InterceptMethod(
            TargetAssembly = "System.Net.Requests", // .NET Core
            TargetType = WebRequestTypeName,
            TargetSignatureTypes = new[] { "System.Threading.Tasks.Task`1<System.Net.WebResponse>" },
            TargetMinimumVersion = Major4,
            TargetMaximumVersion = Major5)]
        public static object GetResponseAsync(object webRequest, int opCode, int mdToken, long moduleVersionPtr)
        {
            const string methodName = nameof(GetResponseAsync);
            Func<object, Task<WebResponse>> callGetResponseAsync;

            try
            {
                var instrumentedType = webRequest.GetInstrumentedType("System.Net.WebRequest");
                callGetResponseAsync =
                    MethodBuilder<Func<object, Task<WebResponse>>>
                        .Start(moduleVersionPtr, mdToken, opCode, methodName)
                        .WithConcreteType(instrumentedType)
                        .WithNamespaceAndNameFilters(ClrNames.GenericTask)
                        .Build();
            }
            catch (Exception ex)
            {
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: WebRequestTypeName,
                    methodName: methodName,
                    instanceType: webRequest.GetType().AssemblyQualifiedName);
                throw;
            }

            return GetResponseAsyncInternal((WebRequest)webRequest, callGetResponseAsync);
        }

        private static async Task<WebResponse> GetResponseAsyncInternal(WebRequest webRequest, Func<object, Task<WebResponse>> originalMethod)
        {
            if (!(webRequest is HttpWebRequest) || !IsTracingEnabled(webRequest))
            {
                return await originalMethod(webRequest).ConfigureAwait(false);
            }

            // The headers may have been set/propagated to the server on a previous method call, but no actual span was created for it yet.
            // Try to extract the context and if available use the already propagated span ID.
            SpanContext spanContext = Tracer.Instance.Propagator.Extract(webRequest.Headers.Wrap());

            using (var scope = ScopeFactory.CreateOutboundHttpScope(Tracer.Instance, webRequest.Method, webRequest.RequestUri, IntegrationName, propagatedSpanId: spanContext?.SpanId))
            {
                try
                {
                    if (scope != null)
                    {
                        // add distributed tracing headers to the HTTP request
                        Tracer.Instance.Propagator.Inject(scope.Span.Context, webRequest.Headers.Wrap());
                    }

                    WebResponse response = await originalMethod(webRequest).ConfigureAwait(false);

                    if (scope != null && response is HttpWebResponse webResponse)
                    {
                        scope.Span.SetTag(Tags.HttpStatusCode, ((int)webResponse.StatusCode).ToString());
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    scope?.Span.SetException(ex);
                    throw;
                }
            }
        }

        private static bool IsTracingEnabled(WebRequest request)
        {
            // check if tracing is disabled for this request via http header
            string value = request.Headers[HttpHeaderNames.TracingEnabled];
            return !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }
    }
}
