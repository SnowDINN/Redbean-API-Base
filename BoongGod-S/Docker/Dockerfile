FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Development
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BoongGod-S/BoongGod-S.csproj", "BoongGod-S/"]
COPY ["RedbeanLibrary/RedbeanLibrary.csproj", "RedbeanLibrary/"]
RUN dotnet restore "BoongGod-S/BoongGod-S.csproj"
COPY . .
WORKDIR "/src/BoongGod-S"
RUN dotnet build "BoongGod-S.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BoongGod-S.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoongGod-S.dll"]
