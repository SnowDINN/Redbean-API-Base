FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Development
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BoongGod-S.csproj", "./"]
RUN dotnet restore "BoongGod-S.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "BoongGod-S.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BoongGod-S.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS boonggod
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoongGod-S.dll"]
