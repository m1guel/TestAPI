# TestAPI - Enterprise .NET 10 Web API

[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture%20%2F%20DDD-green)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A production-ready RESTful Web API built with **.NET 10**, implementing **Clean Architecture** and **Domain-Driven Design (DDD)** principles. Features real-time communication via WebSockets, JWT authentication, comprehensive logging, global exception handling, **Unit of Work pattern**, and **automatic entity auditing**.

---

##  Features

### Core Features
-  **RESTful API** - Full CRUD operations for Weather Forecasts and User Management
-  **JWT Authentication** - Secure token-based authentication with BCrypt password hashing
-  **Real-Time WebSocket** - One-way server-to-client communication for live updates
-  **Entity Framework Core** - Code-first approach with SQL Server
-  **AutoMapper** - Automatic object-to-object mapping between entities and DTOs
-  **Global Exception Handling** - Centralized error handling with custom domain exceptions
-  **Structured Logging** - Comprehensive logging using ILogger with custom extensions
-  **OpenAPI/Swagger** - Interactive API documentation
-  **Docker Support** - Containerization with Docker for easy deployment

### Architecture Features
-  **Clean Architecture** - Clear separation of concerns across layers
-  **Domain-Driven Design (DDD)** - Rich domain model with business logic encapsulation
-  **Repository Pattern** - Abstraction over data access layer
-  **Unit of Work Pattern** - Transactional integrity across multiple repositories
-  **RequestContext** - Thread-safe async local context for user auditing
-  **Dependency Injection** - Built-in .NET Core DI container
-  **SOLID Principles** - Maintainable and extensible codebase
-  **DTOs** - Separate domain entities from API contracts
-  **Base Entity** - DomainEntity with built-in auditing and soft delete support

### Security Features
-  **JWT Token Authentication** - HS256 algorithm with configurable expiration
-  **Password Hashing** - BCrypt with salt
-  **Token Validation** - Comprehensive validation (Issuer, Audience, Lifetime, Signature)
-  **WebSocket Authentication** - JWT-based WebSocket connections
-  **HTTPS Support** - Secure communication
-  **Structured Error Codes** - ErrorCodeType enum for consistent error handling
-  **User Auditing** - Automatic tracking of CreatedBy, UpdatedBy, DeletedBy
-  **Soft Deletes** - Non-destructive deletion with audit trail

---

##  Architecture

This project follows **Clean Architecture** principles with clear separation between layers:

### Layer Responsibilities

#### **Domain Layer** (`TestAPI.Domain`)
- **Purpose**: Core business logic and domain entities
- **Contains**:
  - Domain Entities (`User`, `WeatherForecast`)
  - Base Entity (`DomainEntity` with auditing and soft delete)
  - Business Interfaces (`IAuthService`, `IWeatherForecastService`)
  - Repository Interfaces (`IUserRepository`, `IWeatherForecastRepository`)
  - Domain Services (Business logic implementation)
  - Domain Exceptions (`DomainException`, `ErrorCodeFaultException`)
  - Value Objects and Types (`ErrorCodeType`)
  - Request Context (Thread-safe user context for auditing)
- **Dependencies**: None (independent layer)

#### **Application Layer** (`TestAPI.Application`)
- **Purpose**: Application use cases and API endpoints
- **Contains**:
  - API Controllers (HTTP endpoints)
  - DTOs (Data Transfer Objects)
  - AutoMapper Profiles
  - Middleware (Exception handling, WebSocket)
  - Request/Response handlers
- **Dependencies**: Domain Layer

#### **Infrastructure Layer** (`TestAPI.Infrastructure`)
- **Purpose**: External concerns and data persistence
- **Contains**:
  - Entity Framework Core DbContext
  - Repository Implementations
  - Unit of Work Implementation (Transaction management)
  - Database Migrations
  - WebSocket Services
  - External Service Integrations
  - Entity Configurations (Fluent API mappings)
- **Dependencies**: Domain Layer

---

##  Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/m1guel/TestAPI.git
cd TestAPI
```

### 2. Configure Database Connection

Edit `TestAPI\Application\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TestAPIDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**For SQL Server:**
```json
"DefaultConnection": "Server=localhost;Database=TestAPIDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

### 3. Run Database Migrations

```bash
# Navigate to solution directory
cd TestAPI

# Create migration
dotnet ef migrations add InitialCreate --project Infrastructure\Repositories\TestAPI.Infrastructure.Repositories --startup-project Application\TestAPI.Application

# Apply migration
dotnet ef database update --project Infrastructure\Repositories\TestAPI.Infrastructure.Repositories --startup-project Application\TestAPI.Application
```

### 4. Build the Solution

```bash
dotnet build
```

### 5. Run the Application

**Option A: Run Locally**
```bash
dotnet run --project TestAPI\Application\TestAPI.Application
```

**Option B: Run with Docker**
```bash
# Build the Docker image
docker build -t testapi:latest -f TestAPI\Application\TestAPI.Application\Dockerfile .

# Run the container
docker run -d -p 8080:8080 -p 8081:8081 --name testapi testapi:latest

---

##  API Documentation

### Authentication Endpoints

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "lastLoginAt": null
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "lastLoginAt": "2026-02-06T16:00:00Z"
  }
}
```

#### Get Current User
```http
GET /api/auth/me
Authorization: Bearer {token}
```

### Weather Forecast Endpoints (All require authentication)

#### Get All Weather Forecasts
```http
GET /api/weatherforecast
Authorization: Bearer {token}
```

#### Get Weather Forecast by ID
```http
GET /api/weatherforecast/{id}
Authorization: Bearer {token}
```

#### Create Weather Forecast
```http
POST /api/weatherforecast
Authorization: Bearer {token}
Content-Type: application/json

{
  "date": "2026-02-10",
  "temperatureC": 25,
  "summary": "Warm and sunny"
}
```

#### Update Weather Forecast
```http
PUT /api/weatherforecast/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": 1,
  "date": "2026-02-10",
  "temperatureC": 30,
  "summary": "Very hot"
}
```

#### Delete Weather Forecast
```http
DELETE /api/weatherforecast/{id}
Authorization: Bearer {token}
```

### Error Responses

**401 Unauthorized:**
```json
{
  "statusCode": 401,
  "message": "Invalid email or password.",
  "timestamp": "2026-02-06T15:30:00Z"
}
```

**404 Not Found:**
```json
{
  "statusCode": 404,
  "message": "WeatherForecast with id '999' was not found.",
  "timestamp": "2026-02-06T15:30:00Z"
}
```

---

##  WebSocket Integration

### Connection

**Endpoint**: `ws://localhost:5000/ws` or `wss://localhost:7001/ws`

**Authentication**: JWT token in query string

```javascript
const token = "your_jwt_token_here";
const ws = new WebSocket(`ws://localhost:5000/ws?access_token=${token}`);
```

---

##  Development

### Running in Development Mode

```bash
# Set environment
$env:ASPNETCORE_ENVIRONMENT="Development"  # Windows
export ASPNETCORE_ENVIRONMENT=Development  # Linux/Mac

# Run with hot reload
dotnet watch run --project Application\TestAPI.Application
```

---