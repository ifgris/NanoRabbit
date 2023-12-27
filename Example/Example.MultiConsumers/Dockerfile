FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Example/Example.MultiConsumers/Example.MultiConsumers.csproj", "Example/Example.MultiConsumers/"]
RUN dotnet restore "Example/Example.MultiConsumers/Example.MultiConsumers.csproj"
COPY . .
WORKDIR "/src/Example/Example.MultiConsumers"
RUN dotnet build "Example.MultiConsumers.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Example.MultiConsumers.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Example.MultiConsumers.dll"]
