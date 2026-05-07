FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["global.json", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/BB.API/BB.API.csproj", "src/BB.API/"]
COPY ["src/BB.Domain/BB.Domain.csproj", "src/BB.Domain/"]
COPY ["src/BB.Infrastructure/BB.Infrastructure.csproj", "src/BB.Infrastructure/"]
RUN dotnet restore "src/BB.API/BB.API.csproj"
COPY . .
WORKDIR "/src/src/BB.API"
RUN dotnet build "./BB.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BB.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BB.API.dll"]
