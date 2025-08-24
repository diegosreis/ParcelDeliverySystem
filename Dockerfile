# Use the official .NET 9.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5227
EXPOSE 7093

# Create a non-root user and set permissions
RUN adduser --disabled-password --gecos '' --uid 1000 appuser \
    && mkdir -p /app/data \
    && chown -R appuser:appuser /app

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files for better caching
COPY ["Api/Api.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Tests/Tests.csproj", "Tests/"]

# Restore dependencies for all projects
RUN dotnet restore "Api/Api.csproj"
RUN dotnet restore "Tests/Tests.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Test stage
FROM build AS test
WORKDIR /src
# Copy the XML test file to the project root (where tests expect it)
COPY Container_2-MSX.xml .
# Build tests explicitly and run them
RUN dotnet build "Tests/Tests.csproj" -c Release --no-restore
CMD ["dotnet", "test", "Tests/Tests.csproj", "--verbosity", "normal", "--configuration", "Release", "--no-build", "--no-restore"]

# Publish the application
FROM build AS publish
WORKDIR "/src/Api"
RUN dotnet publish "Api.csproj" -c Release -o /app/publish \
    /p:PublishReadyToRun=false \
    /p:PublishSingleFile=false \
    /p:PublishTrimmed=false

# Final stage/image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Copy the XML file for testing
COPY Container_2-MSX.xml /app/Container_2-MSX.xml

# Create a health check endpoint
HEALTHCHECK --interval=300s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5227/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5227
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Switch to non-root user for security
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "Api.dll"]
