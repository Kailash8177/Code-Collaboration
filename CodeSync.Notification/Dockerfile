FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/CodeSync.Notification/CodeSync.Notification.csproj", "backend/CodeSync.Notification/"]
COPY ["backend/CodeSync.Events/CodeSync.Events.csproj", "backend/CodeSync.Events/"]
RUN dotnet restore "backend/CodeSync.Notification/CodeSync.Notification.csproj"
COPY backend/ .
WORKDIR "/src/backend/CodeSync.Notification"
RUN dotnet build "CodeSync.Notification.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeSync.Notification.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeSync.Notification.dll"]
