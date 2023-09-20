FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Identity/IdentityServer/IdentityServer.csproj", "Identity/IdentityServer/"]
COPY ["Certificates/Certificates.csproj", "Certificates/"]
RUN dotnet restore "Identity/IdentityServer/IdentityServer.csproj"
COPY . .
WORKDIR "/src/Identity/IdentityServer"
RUN dotnet build "IdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./docker-entrypoint.sh"]