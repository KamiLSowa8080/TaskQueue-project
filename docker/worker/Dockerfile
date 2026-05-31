FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TaskQueue.Worker/TaskQueue.Worker.csproj", "TaskQueue.Worker/"]
COPY ["TaskQueue.Core/TaskQueue.Core.csproj", "TaskQueue.Core/"]
COPY ["TaskQueue.Infrastructure/TaskQueue.Infrastructure.csproj", "TaskQueue.Infrastructure/"]
RUN dotnet restore "TaskQueue.Worker/TaskQueue.Worker.csproj"

COPY . .
WORKDIR "/src/TaskQueue.Worker"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "TaskQueue.Worker.dll"]
