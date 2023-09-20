
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Translating/Translating.gRPC/Translating.gRPC.csproj", "Translating/Translating.gRPC/"]
COPY ["Translating/Translating.Domain/Translating.Domain.csproj", "Translating/Translating.Domain/"]
COPY ["Translating/Translating.Infrastructure/Translating.Infrastructure.csproj", "Translating/Translating.Infrastructure/"]
RUN dotnet restore "Translating/Translating.gRPC/Translating.gRPC.csproj"
COPY . .
WORKDIR "/src/Translating/Translating.gRPC"
RUN dotnet build "Translating.gRPC.csproj" -c Release -o /app/build



FROM build AS publish
RUN dotnet publish "Translating.gRPC.csproj" -c Release -o /app/publish /p:UseAppHost=false
COPY ["/Translating/Translating.gRPC/Protos", "/app/publish/Protos"]

#CMD ["tail", "-f", "/dev/null"]
#якщо хочемо запустити специфічний stage і подививтись на fileSystem

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Translating.gRPC.dll"]
ENTRYPOINT ["./docker-entrypoint.sh"]