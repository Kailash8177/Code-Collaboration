FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/CodeSync.Project/CodeSync.Project.csproj", "backend/CodeSync.Project/"]
COPY ["backend/CodeSync.Events/CodeSync.Events.csproj", "backend/CodeSync.Events/"]
RUN dotnet restore "backend/CodeSync.Project/CodeSync.Project.csproj"
COPY backend/ .
WORKDIR "/src/backend/CodeSync.Project"
RUN dotnet build "CodeSync.Project.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeSync.Project.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeSync.Project.dll"]
