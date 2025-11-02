FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CartService.Api/CartService.Api.csproj", "CartService.Api/"]
COPY ["CartService.Application/CartService.Application.csproj", "CartService.Application/"]
COPY ["CartService.Core/CartService.Core.csproj", "CartService.Core/"]
COPY ["CartService.Shared/CartService.Shared.csproj", "CartService.Shared/"]
COPY ["CartService.Infrastructure/CartService.Infrastructure.csproj", "CartService.Infrastructure/"]
RUN dotnet restore "CartService.Api/CartService.Api.csproj"
COPY . .
WORKDIR "/src/CartService.Api"
RUN dotnet build "CartService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CartService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CartService.Api.dll"]
