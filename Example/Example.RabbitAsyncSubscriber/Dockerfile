FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Example/Example.RabbitAsyncSubscriber/Example.RabbitAsyncSubscriber.csproj", "Example/Example.RabbitAsyncSubscriber/"]
RUN dotnet restore "Example/Example.RabbitAsyncSubscriber/Example.RabbitAsyncSubscriber.csproj"
COPY . .
WORKDIR "/src/Example/Example.RabbitAsyncSubscriber"
RUN dotnet build "Example.RabbitAsyncSubscriber.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Example.RabbitAsyncSubscriber.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Example.RabbitAsyncSubscriber.dll"]
