# Parcel Delivery System

A robust and scalable system for automating internal parcel handling in distribution centers. This system processes parcels based on configurable business rules for weight and value thresholds.

---

## Table of Contents

- [Requirements](#requirements)
- [Technologies](#technologies)
- [Quick Start](#quick-start)
- [Setup Instructions](#setup-instructions)
- [Project Structure & Architecture](#project-structure--architecture)
- [Features](#features)
- [API Usage](#api-usage)
- [Docker Management](#docker-management)

---

## Requirements

### System Requirements
- **.NET 9.0** SDK
- **C# 13** language features

### For Docker Setup (Recommended)
- Docker
- Docker Compose

### For Local Development Setup
- .NET 9.0 SDK
- Any IDE that supports .NET (Visual Studio, Rider, VS Code)
- Git

---

## Technologies

### Core Technologies
- **.NET 9.0** - Latest .NET framework with C# 13 features
- **ASP.NET Core Web API** - RESTful API framework
- **Clean Architecture** - Separation of concerns and testability

### Development & Testing
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for isolated testing
- **Coverlet** - Code coverage analysis
- **234+ Unit Tests** - Comprehensive test coverage

### API & Documentation
- **Swagger/OpenAPI** - Interactive API documentation
- **XML Documentation** - Comprehensive code documentation
- **Health Checks** - Application monitoring endpoints

### Deployment & Operations
- **Docker** - Containerization for consistent deployment
- **Docker Compose** - Multi-container orchestration
- **In-Memory Storage** - Simplified data persistence without database dependency

### Code Quality & Standards
- **Primary Constructors** - C# 13 language features
- **Nullable Reference Types** - Enhanced null safety
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Inversion of control

---

## Quick Start

### Option 1: Docker (Recommended)

```bash 
# Clone and start the application
git clone :diegosreis/ParcelDeliverySystem.git
cd ParcelDeliverySystem
./docker-management.sh start
```

That's it! The script will build, start, and show you all the URLs you need.

### Option 2: Local Development

```bash
# Clone and build
git clone git@github.com:diegosreis/ParcelDeliverySystem.git
cd ParcelDeliverySystem
dotnet restore
dotnet build

# Run the application
cd Api
dotnet run

# Access the application
# Swagger: http://localhost:5227
# API: http://localhost:5227/api
```

---

## Setup Instructions

### Docker Setup (Recommended)

1. **Start the application:**
   ```bash
   ./docker-management.sh start
   ```

2. **Access the application:**
   - API Documentation (Swagger): http://localhost:5227
   - Health Check: http://localhost:5227/health

3. **Run a quick demo:**
   ```bash
   ./docker-management.sh demo
   ```

4. **Stop the application:**
   ```bash
   ./docker-management.sh stop
   ```

### Local Development Setup

1. **Prerequisites:**
   ```bash
   # Check .NET version
   dotnet --version  # Should be 9.0.x or higher
   ```

2. **Clone and restore dependencies:**
   ```bash
   git clone <repository-url>
   cd ParcelDeliverySystem
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run tests (optional):**
   ```bash
   dotnet test
   ```

5. **Start the application:**
   ```bash
   cd Api
   dotnet run
   ```

6. **Access the application:**
   - Swagger UI: http://localhost:5227
   - API Base: http://localhost:5227/api
   - Health Check: http://localhost:5227/health

7. **Test the XML import:**
   ```bash
   curl -X POST http://localhost:5227/api/ShippingContainers/import \
        -H "Content-Type: multipart/form-data" \
        -F "file=@Container_2-MSX.xml"
   ```

### Development Workflow

```bash
# Watch for changes during development
cd Api
dotnet watch run

# Run tests with file watching
dotnet watch test

# Generate code coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

---

## Project Structure & Architecture

### Clean Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────┐
│   API Layer     │ ← Controllers, Swagger, Health Checks, Dependency Injection
├─────────────────┤
│ Application     │ ← Services, DTOs, Business Logic, Use Cases
├─────────────────┤
│   Domain        │ ← Entities, Value Objects, Enums, Business Rules
├─────────────────┤
│ Infrastructure  │ ← Repositories, Data Access, External Services
└─────────────────┘
```

### Project Structure

```
ParcelDeliverySystem/
├── Api/                          # Web API Layer
│   ├── Controllers/              # REST API Controllers
│   ├── Program.cs               # Application entry point & DI configuration
│   └── appsettings.json         # Configuration
├── Application/                  # Application Layer
│   ├── DTOs/                    # Data Transfer Objects
│   ├── Services/                # Business logic services
│   └── Models/                  # Application models
├── Domain/                       # Domain Layer
│   ├── Entities/                # Core business entities
│   ├── Enums/                   # Domain enumerations
│   ├── Constants/               # Domain constants
│   └── Interfaces/              # Domain interfaces
├── Infrastructure/              # Infrastructure Layer
│   └── Repositories/            # Data access implementations
├── Tests/                       # Test Projects
│   ├── Api/                     # API layer tests
│   ├── Application/             # Application layer tests
│   └── Domain/                  # Domain layer tests
└── Container_2-MSX.xml          # Sample XML file for testing
```

### Architecture Decision Rationale

**Why Clean Architecture?**
- **Separation of Concerns**: Each layer has a specific responsibility
- **Testability**: Business logic is isolated and easily testable
- **Maintainability**: Changes in one layer don't affect others
- **Scalability**: New features can be added without breaking existing code

**Key Design Decisions:**
- **In-Memory Storage**: Simplifies setup and meets the "without database" requirement
- **Repository Pattern**: Abstracts data access for easy testing and future database integration
- **Service Layer**: Encapsulates business logic and orchestrates operations
- **Primary Constructors**: Leverages C# 13 features for cleaner dependency injection
- **Comprehensive Testing**: High test coverage across all layers

---

## Features

### Feature 1: Weight-based Department Routing
- **Mail Department**: Parcels up to 1kg
- **Regular Department**: Parcels between 1kg and 10kg  
- **Heavy Department**: Parcels over 10kg

### Feature 2: Insurance Approval
- **Insurance Department**: Required for parcels with value > €1000

### Feature 3: Configurable Business Rules (Optional)
- Dynamic business rules via API
- Customer-specific rule configurations
- Support for adding/removing departments

### Additional Features
- **XML Import & Parsing**: Process shipping container XML files
- **Duplicate Detection**: Automatic detection and removal of duplicate parcels
- **Data Integrity Validation**: Prevents conflicting container imports
- **RESTful API**: Complete CRUD operations for all entities
- **Comprehensive Testing**: 234+ unit tests with high coverage
- **Docker Support**: One-command deployment and testing

---

## Dynamic Business Rules

One of the key features of this system is the ability to create and manage dynamic business rules via API, allowing for flexible parcel routing without hardcoded values.

### Default Rules (Fallback)

When no custom rules are configured, the system uses these default values:
- **Mail Department**: Parcels up to 1kg
- **Regular Department**: Parcels between 1kg and 10kg
- **Heavy Department**: Parcels over 10kg
- **Insurance Department**: Parcels with value > €1000

### Creating Custom Business Rules

You can override the default behavior by creating custom business rules via the API:

#### Weight-based Rules
```bash
# Create a custom weight rule for a new "Express" department
curl -X POST http://localhost:5227/api/BusinessRules \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Express Light Parcels",
       "description": "Express handling for lightweight parcels",
       "type": 0,
       "minValue": 0,
       "maxValue": 0.5,
       "targetDepartment": "Express"
     }'
```

#### Value-based Rules
```bash
# Create a custom insurance rule with higher threshold
curl -X POST http://localhost:5227/api/BusinessRules \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Premium Insurance",
       "description": "Insurance required for high-value items",
       "type": 1,
       "minValue": 2000,
       "maxValue": null,
       "targetDepartment": "Insurance"
     }'
```

#### Combined Rules
```bash
# Create a rule based on both weight and value
curl -X POST http://localhost:5227/api/BusinessRules \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Special Handling",
       "description": "Special department for heavy valuable items",
       "type": 2,
       "minValue": 5000,
       "maxValue": null,
       "targetDepartment": "Special"
     }'
```

### Rule Types

- **Type 0**: Weight-based rules (minValue/maxValue in kg)
- **Type 1**: Value-based rules (minValue/maxValue in euros)
- **Type 2**: Combined rules (considers both weight and value)

### Managing Rules

```bash
# List all business rules
curl http://localhost:5227/api/BusinessRules

# Get a specific rule
curl http://localhost:5227/api/BusinessRules/{ruleId}

# Update a rule
curl -X PUT http://localhost:5227/api/BusinessRules/{ruleId} \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Updated Rule Name",
       "description": "Updated description",
       "minValue": 10,
       "maxValue": 50,
       "targetDepartment": "UpdatedDepartment"
     }'

# Delete a rule
curl -X DELETE http://localhost:5227/api/BusinessRules/{ruleId}
```

### Rule Priority

- Custom business rules take **priority** over default rules
- Rules are evaluated in order of `MinValue` (lowest first)
- If no custom rules match, the system falls back to default behavior
- This ensures backward compatibility while allowing full customization

---

## API Usage

### Import XML File (Primary Feature)
```bash
curl -X POST http://localhost:5227/api/ShippingContainers/import \
     -H "Content-Type: multipart/form-data" \
     -F "file=@Container_2-MSX.xml"
```

### Manage Departments
```bash
# List all departments
curl http://localhost:5227/api/Departments

# Create new department
curl -X POST http://localhost:5227/api/Departments \
     -H "Content-Type: application/json" \
     -d '{"name":"Express","description":"Express delivery department"}'

# Update department
curl -X PUT http://localhost:5227/api/Departments/{id} \
     -H "Content-Type: application/json" \
     -d '{"name":"Express","description":"Updated description"}'
```

### Query Data
```bash
# List containers
curl http://localhost:5227/api/ShippingContainers

# List parcels
curl http://localhost:5227/api/Parcels

# Get parcel by ID
curl http://localhost:5227/api/Parcels/{id}
```

### Sample XML Format

```xml
<?xml version="1.0"?>
<Container xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Id>68465468</Id>
  <ShippingDate>2016-07-22T00:00:00+02:00</ShippingDate>
  <parcels>
    <Parcel>
      <Receipient>
        <Name>Vinny Gankema</Name>
        <Address>
          <Street>Marijkestraat</Street>
          <HouseNumber>28</HouseNumber>
          <PostalCode>4744AT</PostalCode>
          <City>Bosschenhoofd</City>
        </Address>
      </Receipient>
      <Weight>0.02</Weight>
      <Value>0.0</Value>
    </Parcel>
  </parcels>
</Container>
```

---

## Docker Management

The project includes a convenient `docker-management.sh` script that simplifies all Docker operations:

### Available Commands

```bash
./docker-management.sh help     # Display all available commands
./docker-management.sh start    # Start the application (recommended)
./docker-management.sh stop     # Stop the application
./docker-management.sh status   # Check application status and health
./docker-management.sh logs     # View real-time application logs
./docker-management.sh test     # Run all tests in Docker environment
./docker-management.sh demo     # Quick demo of system functionality
./docker-management.sh clean    # Clean up all Docker resources
```

### Typical Workflow

```bash
# 1. Start the application
./docker-management.sh start

# 2. Run a quick demo to see it working
./docker-management.sh demo

# 3. Check logs if needed
./docker-management.sh logs

# 4. Run tests to verify everything works
./docker-management.sh test

# 5. When done, clean up everything
./docker-management.sh clean
```
