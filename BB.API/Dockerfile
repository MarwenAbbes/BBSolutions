FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "./"]
COPY ["BB.API/BB.API.csproj", "BB.API/"]
COPY ["BB.Infrastructure/BB.Infrastructure.csproj", "BB.Infrastructure/"]
COPY ["Core.Common/Core.Common.csproj", "Core.Common/"]
RUN dotnet restore "BB.API/BB.API.csproj"
COPY . .
WORKDIR "/src/BB.API"
RUN dotnet build "./BB.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BB.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BB.API.dll"]
