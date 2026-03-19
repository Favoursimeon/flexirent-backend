# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY *.sln .
COPY src/FlexiRent.Api/FlexiRent.Api.csproj src/FlexiRent.Api/
COPY src/FlexiRent.Application/FlexiRent.Application.csproj src/FlexiRent.Application/
COPY src/FlexiRent.Domain/FlexiRent.Domain.csproj src/FlexiRent.Domain/
COPY src/FlexiRent.Infrastructure/FlexiRent.Infrastructure.csproj src/FlexiRent.Infrastructure/

# Restore dependencies
RUN dotnet restore "src/FlexiRent.Api/FlexiRent.Api.csproj"

# Copy everything else
COPY . .

# Publish
RUN dotnet publish "src/FlexiRent.Api/FlexiRent.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "FlexiRent.Api.dll"]
