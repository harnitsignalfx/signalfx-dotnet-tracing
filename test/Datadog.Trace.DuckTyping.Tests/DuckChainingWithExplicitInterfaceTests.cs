#pragma warning disable SA1201 // Elements must appear in the correct order
#pragma warning disable SA1302 // Interface names should begin with I

using System;
using FluentAssertions;
using Xunit;

namespace SignalFx.Tracing.DuckTyping.Tests
{
    public class DuckChainingWithExplicitInterfaceTests
    {
        [Fact]
        public void NormalTest()
        {
            var targetObject = new T_HostingApplication();
            var proxyObject = targetObject.DuckCast<P_IHostingApplication>();

            var logger = proxyObject.Diagnostics.Logger;
            var disposable = logger.BeginScope<object>(new object());

            disposable.Should().BeOfType<T_DisposableObject>();
        }

        public class T_HostingApplication
        {
            private T_HostingApplicationDiagnostics _diagnostics;

            public T_HostingApplication()
            {
                _diagnostics = new T_HostingApplicationDiagnostics();
            }
        }

        internal class T_HostingApplicationDiagnostics
        {
            private readonly T_ILogger _logger;

            public T_HostingApplicationDiagnostics()
            {
                _logger = new T_Logger<T_InternalObject>();
            }
        }

        public interface T_ILogger
        {
            IDisposable BeginScope<TState>(TState state);
        }

        public interface T_ILogger<out TCategoryName> : T_ILogger
        {
        }

        public class T_Logger<T> : T_ILogger<T>
        {
            private readonly T_ILogger _logger;

            public T_Logger()
            {
                _logger = new T_Logger();
            }

            IDisposable T_ILogger.BeginScope<TState>(TState state)
            {
                return _logger.BeginScope(state);
            }
        }

        public class T_Logger : T_ILogger
        {
            IDisposable T_ILogger.BeginScope<TState>(TState state)
            {
                return new T_DisposableObject();
            }
        }

        internal class T_InternalObject
        {
        }

        private class T_DisposableObject : IDisposable
        {
            public void Dispose()
            {
                // .
            }
        }

        // ***

        public interface P_IHostingApplication
        {
            [Duck(Name = "_diagnostics", Kind = DuckKind.Field)]
            P_IHostingApplicationDiagnostics Diagnostics { get; }
        }

        public interface P_IHostingApplicationDiagnostics
        {
            [Duck(Name = "_logger", Kind = DuckKind.Field)]
            P_ILogger Logger { get; }
        }

        public interface P_ILogger
        {
            [Duck(ExplicitInterfaceTypeName = "*")]
            IDisposable BeginScope<TState>(TState state);
        }
    }
}
