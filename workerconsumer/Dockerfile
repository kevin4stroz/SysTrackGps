# Imagen base de .NET 8 para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen base de .NET SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WorkerConsumer/WorkerConsumer.csproj", "WorkerConsumer/"]
RUN dotnet restore "/src/WorkerConsumer/WorkerConsumer.csproj"
COPY . .
WORKDIR "/src/WorkerConsumer/"
RUN dotnet build -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Imagen final con solo los archivos necesarios
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkerConsumer.dll"]
