// Modified by SignalFx
#if !NETSTANDARD2_0
using System;
using System.Net;
using System.ServiceModel.Channels;
using Datadog.Trace.ClrProfiler.Emit;
using SignalFx.Tracing;
using SignalFx.Tracing.ExtensionMethods;
using SignalFx.Tracing.Logging;
using SignalFx.Tracing.Propagation;

namespace Datadog.Trace.ClrProfiler.Integrations
{
    /// <summary>
    /// WcfIntegration
    /// </summary>
    public static class WcfIntegration
    {
        private const string IntegrationName = "Wcf";
        private const string Major4 = "4";
        private const string Major5 = "5";

        private const string ChannelHandlerTypeName = "System.ServiceModel.Dispatcher.ChannelHandler";

        private static readonly SignalFx.Tracing.Vendors.Serilog.ILogger Log = SignalFxLogging.GetLogger(typeof(WcfIntegration));

        /// <summary>
        /// Instrumentation wrapper for System.ServiceModel.Dispatcher.ChannelHandler
        /// </summary>
        /// <param name="channelHandler">The ChannelHandler instance.</param>
        /// <param name="requestContext">A System.ServiceModel.Channels.RequestContext implementation instance.</param>
        /// <param name="currentOperationContext">A System.ServiceModel.OperationContext instance.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>The value returned by the instrumented method.</returns>
        [InterceptMethod(
            TargetAssembly = "System.ServiceModel",
            TargetType = ChannelHandlerTypeName,
            TargetSignatureTypes = new[] { ClrNames.Bool, "System.ServiceModel.Channels.RequestContext", "System.ServiceModel.OperationContext" },
            TargetMinimumVersion = Major4,
            TargetMaximumVersion = Major5)]
        public static bool HandleRequest(
            object channelHandler,
            object requestContext,
            object currentOperationContext,
            int opCode,
            int mdToken,
            long moduleVersionPtr)
        {
            if (channelHandler == null)
            {
                throw new ArgumentNullException(nameof(channelHandler));
            }

            Func<object, object, object, bool> instrumentedMethod;
            var declaringType = channelHandler.GetInstrumentedType(ChannelHandlerTypeName);

            try
            {
                instrumentedMethod = MethodBuilder<Func<object, object, object, bool>>
                                    .Start(moduleVersionPtr, mdToken, opCode, nameof(HandleRequest))
                                    .WithConcreteType(declaringType)
                                    .WithParameters(requestContext, currentOperationContext)
                                    .WithNamespaceAndNameFilters(
                                         ClrNames.Bool,
                                         "System.ServiceModel.Channels.RequestContext",
                                         "System.ServiceModel.OperationContext")
                                    .Build();
            }
            catch (Exception ex)
            {
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: ChannelHandlerTypeName,
                    methodName: nameof(HandleRequest),
                    instanceType: channelHandler.GetType().AssemblyQualifiedName);
                throw;
            }

            using (var scope = CreateScope(requestContext as RequestContext))
            {
                try
                {
                    return instrumentedMethod(channelHandler, requestContext, currentOperationContext);
                }
                catch (Exception ex)
                {
                    scope.Span.SetException(ex);
                    throw;
                }
            }
        }

        private static Scope CreateScope(RequestContext requestContext)
        {
            var requestMessage = requestContext?.RequestMessage;

            if (requestMessage == null)
            {
                return null;
            }

            var tracer = Tracer.Instance;

            if (!tracer.Settings.IsIntegrationEnabled(IntegrationName))
            {
                // integration disabled, don't create a scope, skip this trace
                return null;
            }

            Scope scope = null;

            try
            {
                SpanContext propagatedContext = null;
                string host = null;
                string httpMethod = null;

                if (requestMessage.Properties.TryGetValue("httpRequest", out var httpRequestProperty) &&
                    httpRequestProperty is HttpRequestMessageProperty httpRequestMessageProperty)
                {
                    // we're using an http transport
                    host = httpRequestMessageProperty.Headers[HttpRequestHeader.Host];
                    httpMethod = httpRequestMessageProperty.Method?.ToUpperInvariant();

                    // try to extract propagated context values from http headers
                    if (tracer.ActiveScope == null)
                    {
                        try
                        {
                            var headers = httpRequestMessageProperty.Headers.Wrap();
                            propagatedContext = tracer.Propagator.Extract(headers);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error extracting propagated HTTP headers.");
                        }
                    }
                }

                var operationNameSuffix = requestMessage.Headers.Action ?? requestMessage.Headers.To?.LocalPath;
                var operationName = !string.IsNullOrEmpty(operationNameSuffix)
                    ? "wcf.request " + operationNameSuffix
                    : "wcf.request";

                scope = tracer.StartActive(operationName, propagatedContext);
                var span = scope.Span;

                span.DecorateWebServerSpan(
                    resourceName: null,
                    httpMethod,
                    host,
                    httpUrl: requestMessage.Headers.To?.AbsoluteUri);
                span.SetTag(Tags.InstrumentationName, IntegrationName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating or populating scope.");
            }

            // always returns the scope, even if it's null
            return scope;
        }
    }
}

#endif
