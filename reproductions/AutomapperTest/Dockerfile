# Modified by SignalFx
FROM mcr.microsoft.com/dotnet/core/runtime:2.1-stretch-slim AS base
ARG TRACER_VERSION=0.1.15
RUN mkdir -p /opt/datadog
RUN mkdir -p /var/log/signalfx/dotnet
RUN curl -L https://github.com/DataDog/dd-trace-dotnet/releases/download/v$TRACER_VERSION/datadog-dotnet-apm-$TRACER_VERSION.tar.gz | tar xzf - -C /opt/datadog

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={B4C89B0F-9908-4F73-9F59-0D77C5A06874}
ENV CORECLR_PROFILER_PATH=/opt/signalfx-dotnet-tracing/SignalFx.Tracing.ClrProfiler.Native.so
ENV SIGNALFX_DOTNET_TRACER_HOME=/opt/signalfx-dotnet-tracing
ENV SIGNALFX_INTEGRATIONS=/opt/signalfx-dotnet-tracing/integrations.json
ENV SIGNALFX_TRACING_ENABLED=true

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR "/src"
COPY ["reproductions/AutomapperTest/AutomapperTest.csproj", "/src/reproductions/AutomapperTest/"]
COPY ["src/Datadog.Trace.ClrProfiler.Managed/Datadog.Trace.ClrProfiler.Managed.csproj", "/src/src/Datadog.Trace.ClrProfiler.Managed/"]
COPY ["src/Datadog.Trace/Datadog.Trace.csproj", "/src/src/Datadog.Trace/"]
RUN dotnet restore "/src/reproductions/AutomapperTest/AutomapperTest.csproj"
COPY . .
WORKDIR "/src/reproductions/AutomapperTest"
RUN dotnet build "AutomapperTest.csproj" -c Release -o /app

FROM build as publish
RUN dotnet publish "AutomapperTest.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AutomapperTest.dll"]