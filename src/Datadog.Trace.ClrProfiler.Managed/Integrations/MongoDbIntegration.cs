// Modified by SignalFx
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.Emit;
using Datadog.Trace.ClrProfiler.Helpers;
using SignalFx.Tracing;
using SignalFx.Tracing.Logging;

namespace Datadog.Trace.ClrProfiler.Integrations
{
    /// <summary>
    /// Tracing integration for MongoDB.Driver.Core.
    /// </summary>
    public static class MongoDbIntegration
    {
        private const string IntegrationName = "MongoDb";
        private const string OperationName = "mongodb.query";

        private const string Major2 = "2";
        private const string Major2Minor1 = "2.1";
        private const string Major2Minor2 = "2.2"; // Synchronous methods added in 2.2
        private const string MongoDbClientAssembly = "MongoDB.Driver.Core";
        private const string IWireProtocol = "MongoDB.Driver.Core.WireProtocol.IWireProtocol";
        private const string IWireProtocolGeneric = "MongoDB.Driver.Core.WireProtocol.IWireProtocol`1";

        private static readonly SignalFx.Tracing.Vendors.Serilog.ILogger Log = SignalFxLogging.GetLogger(typeof(MongoDbIntegration));

        /// <summary>
        /// Wrap the original method by adding instrumentation code around it.
        /// </summary>
        /// <param name="wireProtocol">The IWireProtocol instance we are replacing.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="boxedCancellationToken">A cancellation token.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>The original method's return value.</returns>
        [InterceptMethod(
            TargetAssembly = MongoDbClientAssembly,
            TargetType = IWireProtocol,
            TargetSignatureTypes = new[] { ClrNames.Void, "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken },
            TargetMinimumVersion = Major2Minor2,
            TargetMaximumVersion = Major2)]
        public static object Execute(
            object wireProtocol,
            object connection,
            object boxedCancellationToken,
            int opCode,
            int mdToken,
            long moduleVersionPtr)
        {
            if (wireProtocol == null) { throw new ArgumentNullException(nameof(wireProtocol)); }

            const string methodName = nameof(Execute);
            Func<object, object, CancellationToken, object> execute;
            var wireProtocolType = wireProtocol.GetType();

            var cancellationToken = (CancellationToken)boxedCancellationToken;

            try
            {
                execute =
                    MethodBuilder<Func<object, object, CancellationToken, object>>
                       .Start(moduleVersionPtr, mdToken, opCode, methodName)
                       .WithConcreteType(wireProtocolType)
                       .WithParameters(connection, cancellationToken)
                       .WithNamespaceAndNameFilters(ClrNames.Void, "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken)
                       .Build();
            }
            catch (Exception ex)
            {
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: IWireProtocol,
                    methodName: methodName,
                    instanceType: wireProtocol.GetType().AssemblyQualifiedName);
                throw;
            }

            using (var scope = CreateScope(wireProtocol, connection))
            {
                try
                {
                    return execute(wireProtocol, connection, cancellationToken);
                }
                catch (Exception ex) when (scope?.Span.SetExceptionForFilter(ex) ?? false)
                {
                    // unreachable code
                    throw;
                }
            }
        }

        /// <summary>
        /// Wrap the original method by adding instrumentation code around it.
        /// </summary>
        /// <param name="wireProtocol">The IWireProtocol`1 instance we are replacing.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="boxedCancellationToken">A cancellation token.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>The original method's return value.</returns>
        [InterceptMethod(
            TargetAssembly = MongoDbClientAssembly,
            TargetType = IWireProtocolGeneric,
            TargetSignatureTypes = new[] { "T", "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken },
            TargetMethod = nameof(Execute),
            TargetMinimumVersion = Major2Minor2,
            TargetMaximumVersion = Major2)]
        public static object ExecuteGeneric(
            object wireProtocol,
            object connection,
            object boxedCancellationToken,
            int opCode,
            int mdToken,
            long moduleVersionPtr)
        {
            if (wireProtocol == null) { throw new ArgumentNullException(nameof(wireProtocol)); }

            const string methodName = nameof(Execute);
            Func<object, object, CancellationToken, object> execute;
            var wireProtocolType = wireProtocol.GetType();

            var cancellationToken = (CancellationToken)boxedCancellationToken;

            try
            {
                execute =
                    MethodBuilder<Func<object, object, CancellationToken, object>>
                       .Start(moduleVersionPtr, mdToken, opCode, methodName)
                       .WithConcreteType(wireProtocolType)
                       .WithParameters(connection, cancellationToken)
                       .WithNamespaceAndNameFilters(ClrNames.Ignore, "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken)
                       .Build();
            }
            catch (Exception ex)
            {
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: IWireProtocolGeneric,
                    methodName: methodName,
                    instanceType: wireProtocol.GetType().AssemblyQualifiedName);
                throw;
            }

            using (var scope = CreateScope(wireProtocol, connection))
            {
                try
                {
                    return execute(wireProtocol, connection, cancellationToken);
                }
                catch (Exception ex) when (scope?.Span.SetExceptionForFilter(ex) ?? false)
                {
                    // unreachable code
                    throw;
                }
            }
        }

        /// <summary>
        /// Wrap the original method by adding instrumentation code around it.
        /// </summary>
        /// <param name="wireProtocol">The IWireProtocol instance we are replacing.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="boxedCancellationToken">A cancellation token.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>The original method's return value.</returns>
        [InterceptMethod(
            TargetMethod = nameof(ExecuteAsync),
            TargetAssembly = MongoDbClientAssembly,
            TargetType = IWireProtocol,
            TargetSignatureTypes = new[] { ClrNames.Task, "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken },
            TargetMinimumVersion = Major2Minor1,
            TargetMaximumVersion = Major2)]
        public static object ExecuteAsync(
            object wireProtocol,
            object connection,
            object boxedCancellationToken,
            int opCode,
            int mdToken,
            long moduleVersionPtr)
        {
            if (wireProtocol == null) { throw new ArgumentNullException(nameof(wireProtocol)); }

            var cancellationToken = (CancellationToken)boxedCancellationToken;

            const string methodName = nameof(ExecuteAsync);
            var wireProtocolType = wireProtocol.GetType();
            var wireProtocolGenericArgs = GetGenericsFromWireProtocol(wireProtocolType);

            Func<object, object, CancellationToken, object> executeAsync;

            try
            {
                executeAsync =
                    MethodBuilder<Func<object, object, CancellationToken, object>>
                       .Start(moduleVersionPtr, mdToken, opCode, methodName)
                       .WithConcreteType(wireProtocolType)
                       .WithDeclaringTypeGenerics(wireProtocolGenericArgs)
                       .WithParameters(connection, cancellationToken)
                       .WithNamespaceAndNameFilters(ClrNames.Task, "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken)
                       .Build();
            }
            catch (Exception ex)
            {
                Log.ErrorRetrievingMethod(
                    exception: ex,
                    moduleVersionPointer: moduleVersionPtr,
                    mdToken: mdToken,
                    opCode: opCode,
                    instrumentedType: IWireProtocolGeneric,
                    methodName: methodName,
                    instanceType: wireProtocol.GetType().AssemblyQualifiedName);
                throw;
            }

            return ExecuteAsyncInternalNonGeneric(wireProtocol, connection, cancellationToken, executeAsync);
        }

        /// <summary>
        /// Wrap the original method by adding instrumentation code around it.
        /// </summary>
        /// <param name="wireProtocol">The IWireProtocol`1 instance we are replacing.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="boxedCancellationToken">A cancellation token.</param>
        /// <param name="opCode">The OpCode used in the original method call.</param>
        /// <param name="mdToken">The mdToken of the original method call.</param>
        /// <param name="moduleVersionPtr">A pointer to the module version GUID.</param>
        /// <returns>The original method's return value.</returns>
        [InterceptMethod(
            TargetMethod = nameof(ExecuteAsync),
            TargetAssembly = MongoDbClientAssembly,
            TargetType = IWireProtocolGeneric,
            TargetSignatureTypes = new[] { "System.Threading.Tasks.Task`1<T>", "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken },
            TargetMinimumVersion = Major2Minor1,
            TargetMaximumVersion = Major2)]
        public static object ExecuteAsyncGeneric(
            object wireProtocol,
            object connection,
            object boxedCancellationToken,
            int opCode,
            int mdToken,
            long moduleVersionPtr)
        {
            // The generic type for this method comes from the declaring type of wireProtocol
            if (wireProtocol == null) { throw new ArgumentNullException(nameof(wireProtocol)); }

            var cancellationToken = (CancellationToken)boxedCancellationToken;

            var wireProtocolType = wireProtocol.GetType();
            var wireProtocolGenericArgs = GetGenericsFromWireProtocol(wireProtocolType);

            const string methodName = nameof(ExecuteAsync);
            Func<object, object, CancellationToken, object> executeAsync;

            try
            {
                executeAsync =
                    MethodBuilder<Func<object, object, CancellationToken, object>>
                       .Start(moduleVersionPtr, mdToken, opCode, methodName)
                       .WithConcreteType(wireProtocolType)
                       .WithDeclaringTypeGenerics(wireProtocolGenericArgs)
                       .WithParameters(connection, cancellationToken)
                       .WithNamespaceAndNameFilters(ClrNames.GenericTask, "MongoDB.Driver.Core.Connections.IConnection", ClrNames.CancellationToken)
                       .Build();
            }
            catch (Exception ex)
            {
                // profiled app will not continue working as expected without this method
                Log.Error(ex, $"Error resolving {wireProtocolType.Name}.{methodName}(IConnection connection, CancellationToken cancellationToken)");
                throw;
            }

            return AsyncHelper.InvokeGenericTaskDelegate(
                wireProtocolType,
                wireProtocolGenericArgs[0],
                nameof(ExecuteAsyncInternalGeneric),
                typeof(MongoDbIntegration),
                wireProtocol,
                connection,
                cancellationToken,
                executeAsync);
        }

        private static Type[] GetGenericsFromWireProtocol(Type wireProtocolType)
        {
            var interfaces = wireProtocolType.GetInterfaces();
            Type typeWeInstrument = null;

            for (var i = 0; i < interfaces.Length; i++)
            {
                if (string.Equals($"{interfaces[i].Namespace}.{interfaces[i].Name}", IWireProtocolGeneric))
                {
                    typeWeInstrument = interfaces[i];
                    break;
                }
            }

            if (typeWeInstrument == null)
            {
                // We're likely in a non-generic context
                return null;
            }

            var genericArgs = typeWeInstrument.GetGenericArguments();

            if (genericArgs.Length == 0)
            {
                throw new ArgumentException($"Expected generics to determine TaskResult from {wireProtocolType.AssemblyQualifiedName}");
            }

            return genericArgs;
        }

        private static async Task ExecuteAsyncInternalNonGeneric(
            object wireProtocol,
            object connection,
            CancellationToken cancellationToken,
            Func<object, object, CancellationToken, object> originalMethod)
        {
            using (var scope = CreateScope(wireProtocol, connection))
            {
                try
                {
                    if (originalMethod == null)
                    {
                        throw new ArgumentNullException(nameof(originalMethod));
                    }

                    var taskObject = originalMethod(wireProtocol, connection, cancellationToken);
                    var task = (Task)taskObject;
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex) when (scope?.Span.SetExceptionForFilter(ex) ?? false)
                {
                    // unreachable code
                    throw;
                }
            }
        }

        private static async Task<T> ExecuteAsyncInternalGeneric<T>(
            object wireProtocol,
            object connection,
            CancellationToken cancellationToken,
            Func<object, object, CancellationToken, object> originalMethod)
        {
            using (var scope = CreateScope(wireProtocol, connection))
            {
                try
                {
                    var taskObject = originalMethod(wireProtocol, connection, cancellationToken);
                    var typedTask = (Task<T>)taskObject;
                    return await typedTask.ConfigureAwait(false);
                }
                catch (Exception ex) when (scope?.Span.SetExceptionForFilter(ex) ?? false)
                {
                    // unreachable code
                    throw;
                }
            }
        }

        private static Scope CreateScope(object wireProtocol, object connection)
        {
            if (!Tracer.Instance.Settings.IsIntegrationEnabled(IntegrationName))
            {
                // integration disabled, don't create a scope, skip this trace
                return null;
            }

            string databaseName = null;
            string host = null;
            string port = null;

            try
            {
                if (wireProtocol.TryGetFieldValue("_databaseNamespace", out object databaseNamespace))
                {
                    databaseNamespace?.TryGetPropertyValue("DatabaseName", out databaseName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Unable to access DatabaseName property.");
            }

            try
            {
                if (connection != null && connection.TryGetPropertyValue("EndPoint", out object endpoint))
                {
                    if (endpoint is IPEndPoint ipEndPoint)
                    {
                        host = ipEndPoint.Address.ToString();
                        port = ipEndPoint.Port.ToString();
                    }
                    else if (endpoint is DnsEndPoint dnsEndPoint)
                    {
                        host = dnsEndPoint.Host;
                        port = dnsEndPoint.Port.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Unable to access EndPoint properties.");
            }

            Tracer tracer = Tracer.Instance;
            string statement = null;
            string operationName = null;
            string collectionName = null;
            string query = null;

            try
            {
                if (wireProtocol.TryGetFieldValue("_command", out object command) && command != null)
                {
                    if (tracer.Settings.TagMongoCommands)
                    {
                        string document = command.ToString();
                        statement = document.Length > 1024 ? document.Substring(0, 1024) : document;
                    }

                    // the name of the first element in the command BsonDocument will be the operation type (insert, delete, find, etc)
                    // and its value is the collection name
                    if (command.TryCallMethod("GetElement", 0, out object firstElement) && firstElement != null)
                    {
                        firstElement.TryGetPropertyValue("Name", out operationName);

                        if (firstElement.TryGetPropertyValue("Value", out object collectionNameObj) && collectionNameObj != null)
                        {
                            collectionName = collectionNameObj.ToString();
                        }
                    }

                    // get the "query" element from the command BsonDocument, if it exists
                    if (command.TryCallMethod("Contains", "query", out bool found) && found)
                    {
                        if (command.TryCallMethod("GetElement", "query", out object queryElement) && queryElement != null)
                        {
                            if (queryElement.TryGetPropertyValue("Value", out object queryValue) && queryValue != null)
                            {
                                query = queryValue.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Unable to access IWireProtocol.Command properties.");
            }

            operationName = operationName ?? OperationName;

            Span parent = tracer.ActiveScope?.Span;

            if (parent != null &&
                parent.GetTag(Tags.DbType) == SpanTypes.MongoDb &&
                parent.OperationName == operationName &&
                parent.GetTag(Tags.DbStatement) == statement)
            {
                // we are already instrumenting this command execution,
                return null;
            }

            Scope scope = null;
            try
            {
                scope = tracer.StartActive(operationName, serviceName: tracer.DefaultServiceName);
                var span = scope.Span;
                span.SetTag(Tags.DbType, SpanTypes.MongoDb);
                span.SetTag(Tags.InstrumentationName, IntegrationName);
                span.SetTag(Tags.SpanKind, SpanKinds.Client);
                span.SetTag(Tags.DbName, databaseName);
                span.SetTag(Tags.DbStatement, statement);
                span.SetTag(Tags.MongoDbQuery, query);
                span.SetTag(Tags.MongoDbCollection, collectionName);
                span.SetTag(Tags.OutHost, host);
                span.SetTag(Tags.OutPort, port);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating or populating scope.");
            }

            return scope;
        }
    }
}
