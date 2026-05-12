FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend/CodeSync.Gateway/CodeSync.Gateway.csproj", "backend/CodeSync.Gateway/"]
RUN dotnet restore "backend/CodeSync.Gateway/CodeSync.Gateway.csproj"
COPY backend/ .
WORKDIR "/src/backend/CodeSync.Gateway"
RUN dotnet build "CodeSync.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeSync.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeSync.Gateway.dll"]
