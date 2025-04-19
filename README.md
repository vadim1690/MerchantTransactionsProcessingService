# Merchant Transaction Processing

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4) ![License](https://img.shields.io/badge/license-MIT-blue) ![API](https://img.shields.io/badge/API-REST-green)

A .NET 8.0 Web API project for processing merchant transactions, managing payment methods, and generating reports. This project serves as a demonstration of best architectural practices in .NET development.

## 📋 Overview

Merchant Transaction Processing is a RESTful API service that enables businesses to manage and process transactions across multiple payment methods. The system supports merchant management, payment method registration, transaction processing, and reporting. It showcases modern .NET architecture patterns, clean code principles, and software design best practices.

## ✨ Features

- **Merchant Management** - Add, retrieve, and manage merchant information
- **Payment Method Management** - Register and manage different payment methods for merchants
- **Transaction Processing** - Process payments with support for various payment methods
- **Transaction Filtering** - Search and filter transactions by multiple criteria
- **Merchant Reports** - Generate daily transaction reports with hourly breakdown and payment method stats
- **Background Processing** - Automatic payment processing via background service
- **Caching** - Performance optimization with Redis or in-memory caching

## 🛠️ Tech Stack

- **.NET 8.0** - Latest .NET runtime
- **ASP.NET Core Web API** - Framework for building RESTful APIs
- **Entity Framework Core** - ORM for data access
- **Entity Framework In-Memory Database** - For development and testing
- **Redis Cache** - Distributed caching (configurable fallback to in-memory cache)
- **Swagger/OpenAPI** - API documentation and testing

## 🏗️ Project Architecture

The application follows a layered architecture with separation of concerns, demonstrating several .NET architectural best practices:

- **Controllers** - Handle HTTP requests and responses
- **Services** - Implement business logic and orchestrate operations
- **Repositories** - Abstract data access with the Repository Pattern
- **Data Access** - Entity Framework Core with clean separation from business logic
- **Models** - Domain entities, DTOs, and parameters for clear data transfer
- **Middleware** - Custom exception handling and logging for cross-cutting concerns
- **Background Services** - Process transactions asynchronously using .NET hosted services
- **Dependency Injection** - All components are properly registered and injected
- **Caching** - Demonstrates both distributed and in-memory caching strategies

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK
- Redis (optional, can be configured to use in-memory cache)

### Installation

1. Clone the repository
   ```bash
   git clone https://github.com/yourusername/MerchantTransactionProcessing.git
   cd MerchantTransactionProcessing
   ```

2. Restore dependencies
   ```bash
   dotnet restore
   ```

3. Build the project
   ```bash
   dotnet build
   ```

4. Run the application
   ```bash
   dotnet run
   ```

5. Access Swagger UI
   ```
   https://localhost:7084/swagger
   ```

### Configuration

Configuration settings can be adjusted in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,allowAdmin=true,ssl=false,abortConnect=false,defaultDatabase=0,connectTimeout=15000,syncTimeout=15000"
  },
  "UseRedisCache": true,
  "RedisCache": {
    "InstanceName": "MTP_"
  }
}
```

- Set `UseRedisCache` to `false` to use in-memory caching instead of Redis
- Adjust Redis connection string as needed

## 📝 API Endpoints

### Merchants

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/merchants` | Get all merchants |
| GET | `/api/merchants/{merchantId}/payment-methods` | Get merchant payment methods |
| POST | `/api/merchants/{merchantId}/payment-methods` | Register payment method |
| GET | `/api/merchants/{merchantId}/transactions` | Get merchant transactions |
| GET | `/api/merchants/{merchantId}/reports/daily` | Get merchant daily report |

### Transactions

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/transactions` | Process new transaction |
| GET | `/api/transactions/{transactionId}` | Get transaction details |
| GET | `/api/transactions` | Get transactions with filtering |

## 📊 Data Models

### Merchant
```json
{
  "id": "guid",
  "name": "string"
}
```

### Payment Method
```json
{
  "id": "guid",
  "method": "string"
}
```

### Transaction
```json
{
  "transactionDate": "datetime",
  "amount": "decimal",
  "status": "string",
  "paymentMethodId": "guid",
  "paymentMethod": "string",
  "merchantId": "guid",
  "merchantName": "string"
}
```

## 🎓 Educational Value

This project demonstrates several architectural concepts and patterns:

- **Repository Pattern** - Clean separation of data access from business logic
- **Dependency Injection** - Proper use of the built-in .NET DI container
- **Service Layer Pattern** - Encapsulation of business logic in dedicated services
- **Middleware** - Custom implementation for logging and exception handling
- **Extension Methods** - Clean API response handling and utility functions
- **Background Services** - Long-running processes using IHostedService
- **Caching Strategies** - Abstraction with fallback mechanisms
- **Entity Framework** - Code-first approach with tracking overrides

## 👨‍💻 Development

### Seeded Data

The application includes seed data for testing:
- 12 merchant records
- Multiple payment methods per merchant
- Sample transactions with various statuses

### Background Processing

The system includes a background service that automatically processes pending transactions every minute.

## 🔮 Future Improvements

- Authentication and authorization
- Proper database implementation with migrations
- Unit and integration tests
- API versioning
- Improved validation using FluentValidation
- CQRS pattern implementation
- Production deployment configuration
- Transaction batching and bulk processing
- Advanced analytics and reporting
- Mediator pattern for decoupling components

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
