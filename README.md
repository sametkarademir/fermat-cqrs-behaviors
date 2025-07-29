# Fermat.Cqrs.Behaviors

A comprehensive .NET library providing cross-cutting concerns as MediatR pipeline behaviors for CQRS applications. This library offers ready-to-use behaviors for authentication, authorization, logging, performance monitoring, retry mechanisms, transaction management, and validation.

## 🚀 Features

### 🔐 Authentication & Authorization
- **AuthorizationBehavior**: Enforces authentication and role-based authorization
- Supports multiple required roles per request
- Automatic user authentication verification
- Role-based access control with customizable role requirements

### 📝 Logging
- **LoggingBehavior**: Comprehensive request/response logging
- Request payload logging with structured data
- Response logging with execution time tracking
- Error logging with detailed exception information
- Performance metrics included in logs

### ⚡ Performance Monitoring
- **PerformanceBehavior**: Monitors request execution time
- Configurable performance thresholds (default: 500ms)
- Warning logs for long-running requests
- Request payload logging for performance analysis

### 🔄 Retry Mechanism
- **RetryBehavior**: Automatic retry for transient failures
- Configurable retry count and delay
- Smart transient exception detection
- Exponential backoff support via Polly
- Detailed retry logging

### 💾 Transaction Management
- **TransactionScopeBehavior**: Automatic transaction scope management
- Async transaction support
- Automatic rollback on exceptions
- Proper resource disposal

### ✅ Validation
- **ValidationBehavior**: FluentValidation integration
- Automatic request validation
- Structured validation error responses
- Support for multiple validators per request

## 📦 Installation

```bash
  dotnet add package Fermat.Cqrs.Behaviors
```

## 🛠️ Dependencies

- **.NET 8.0**
- **MediatR** (13.0.0) - For pipeline behavior support
- **FluentValidation** (12.0.0) - For request validation
- **Polly** (8.6.2) - For retry policies
- **Microsoft.AspNetCore.Http.Abstractions** (2.3.0) - For HTTP context access
- **Fermat.Domain.Exceptions** (0.0.1) - For custom exception types

## 🔧 Configuration

### Register Behaviors in DI Container

```csharp
// Program.cs or Startup.cs
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(YourAssembly).Assembly);
    
    // Register pipeline behaviors in execution order
    cfg.AddBehavior(typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(PerformanceBehavior<,>));
    cfg.AddBehavior(typeof(RetryBehavior<,>));
    cfg.AddBehavior(typeof(AuthorizationBehavior<,>));
    cfg.AddBehavior(typeof(TransactionScopeBehavior<,>));
});

// Required services
services.AddHttpContextAccessor();
services.AddLogging();
```

## 📖 Usage Examples

### 1. Authentication & Authorization

```csharp
public class CreateUserCommand : IRequest<CreateUserResponse>, IAuthorizeBehavior
{
    public IReadOnlyCollection<string> RequiredRoles => new[] { "Admin", "UserManager" };
    
    public string Username { get; set; }
    public string Email { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Your business logic here
        return new CreateUserResponse { UserId = Guid.NewGuid() };
    }
}
```

### 2. Logging

```csharp
public class GetUserQuery : IRequest<GetUserResponse>, ILoggingBehavior
{
    public Guid UserId { get; set; }
}
```

### 3. Performance Monitoring

```csharp
public class ComplexCalculationCommand : IRequest<CalculationResult>, IPerformanceBehavior
{
    public int ThresholdMilliseconds => 1000; // Custom threshold
    
    public decimal[] Numbers { get; set; }
}
```

### 4. Retry Mechanism

```csharp
public class ExternalApiCallCommand : IRequest<ApiResponse>, IRetryBehavior
{
    public int RetryCount => 5; // Custom retry count
    public int RetryDelayMilliseconds => 1000; // Custom delay
    
    public string ApiEndpoint { get; set; }
}
```

### 5. Transaction Management

```csharp
public class TransferMoneyCommand : IRequest<TransferResult>, ITransactionBehavior
{
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
}
```

### 6. Validation

```csharp
public class CreateProductCommand : IRequest<CreateProductResponse>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
```

## 🔄 Behavior Execution Order

The recommended execution order for pipeline behaviors:

1. **ValidationBehavior** - Validate request first
2. **LoggingBehavior** - Log incoming request
3. **PerformanceBehavior** - Start performance monitoring
4. **RetryBehavior** - Handle transient failures
5. **AuthorizationBehavior** - Check authentication/authorization
6. **TransactionScopeBehavior** - Manage transactions
7. **Your Handler** - Execute business logic
8. **Behaviors in reverse order** - Complete monitoring, logging, etc.

## 🎯 Best Practices

### 1. Interface Implementation
- Implement behavior interfaces only when needed
- Use default values when possible to reduce boilerplate
- Combine multiple behaviors for comprehensive request handling

### 2. Performance Considerations
- Set appropriate performance thresholds based on your application needs
- Monitor and adjust retry policies based on failure patterns
- Use logging judiciously to avoid performance impact

### 3. Security
- Always implement authorization for sensitive operations
- Use role-based access control effectively
- Validate all user inputs

### 4. Error Handling
- Leverage retry behavior for transient failures
- Use transaction behavior for data consistency
- Implement proper validation for data integrity

## 🐛 Troubleshooting

### Common Issues

1. **Behaviors not executing**: Ensure behaviors are registered in the correct order
2. **Validation not working**: Verify FluentValidation validators are registered in DI
3. **Authorization failing**: Check if `IHttpContextAccessor` is registered
4. **Retry not working**: Verify exception types are considered transient