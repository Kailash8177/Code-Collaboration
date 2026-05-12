FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/CodeSync.File/CodeSync.File.csproj", "backend/CodeSync.File/"]
COPY ["backend/CodeSync.Events/CodeSync.Events.csproj", "backend/CodeSync.Events/"]
RUN dotnet restore "backend/CodeSync.File/CodeSync.File.csproj"
COPY backend/ .
WORKDIR "/src/backend/CodeSync.File"
RUN dotnet build "CodeSync.File.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeSync.File.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeSync.File.dll"]
