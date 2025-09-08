# Use the official .NET 8 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/TPOR.Intranet.API/TPOR.Intranet.API.csproj", "src/TPOR.Intranet.API/"]
COPY ["src/TPOR.Shared/TPOR.Shared.csproj", "src/TPOR.Shared/"]

# Restore dependencies
RUN dotnet restore "src/TPOR.Intranet.API/TPOR.Intranet.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/TPOR.Intranet.API"
RUN dotnet build "TPOR.Intranet.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TPOR.Intranet.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TPOR.Intranet.API.dll"]
