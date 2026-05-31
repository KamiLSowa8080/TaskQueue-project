FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TaskQueue.API/TaskQueue.API.csproj", "TaskQueue.API/"]
COPY ["TaskQueue.Core/TaskQueue.Core.csproj", "TaskQueue.Core/"]
COPY ["TaskQueue.Infrastructure/TaskQueue.Infrastructure.csproj", "TaskQueue.Infrastructure/"]
RUN dotnet restore "TaskQueue.API/TaskQueue.API.csproj"

COPY . .
WORKDIR "/src/TaskQueue.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "TaskQueue.API.dll"]
