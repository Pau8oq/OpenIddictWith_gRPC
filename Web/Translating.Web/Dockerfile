FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Web/Translating.Web/Translating.Web.csproj", "Web/Translating.Web/"]
COPY ["Web/Translating.Web.Domain/Translating.Web.Domain.csproj", "Web/Translating.Web.Domain/"]
COPY ["Web/Translating.Web.Infrastructure/Translating.Web.Infrastructure.csproj", "Web/Translating.Web.Infrastructure/"]
RUN dotnet restore "Web/Translating.Web/Translating.Web.csproj"
COPY . .
WORKDIR "/src/Web/Translating.Web"
RUN dotnet build "Translating.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Translating.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./docker-entrypoint.sh"]
#ENTRYPOINT ["dotnet", "Translating.Web.dll"]