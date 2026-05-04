# Project Structure

```
/
├── backend/
│   ├── ExpenseTracker.sln
│   ├── ExpenseTracker.Api/          # ASP.NET Core Web API
│   │   ├── Controllers/             # Route handlers — thin, delegate to services
│   │   ├── DTOs/                    # Request/response records + FluentValidation validators
│   │   ├── Exceptions/              # Domain exception types (NotFoundException, ConflictException, BusinessRuleException)
│   │   ├── Middleware/              # ExceptionHandlerMiddleware — maps exceptions to JSON error responses
│   │   ├── Models/                  # Domain model classes and enums
│   │   ├── Repositories/           # IXxxRepository interfaces + InMemoryXxx implementations
│   │   ├── Services/                # IXxxService interfaces + implementations (business logic lives here)
│   │   └── Program.cs               # DI registration, middleware pipeline
│   └── ExpenseTracker.Tests/        # xunit test project
│       └── *Tests.cs                # Unit tests per service; integration tests via WebApplicationFactory
└── frontend/
    ├── src/
    │   ├── api/                     # Axios functions per resource (transactions.ts, categories.ts, analytics.ts)
    │   │   └── client.ts            # Shared Axios instance with error interceptor
    │   ├── components/              # Reusable UI components (no routing logic)
    │   ├── hooks/                   # Custom hooks (useTransactions, useCategories, useAnalytics)
    │   ├── pages/                   # Page-level components — compose hooks + components
    │   ├── types/                   # Shared TypeScript interfaces and types (index.ts)
    │   ├── App.tsx                  # Router setup
    │   └── main.tsx                 # Entry point
    ├── vite.config.ts
    └── package.json
```

## Architecture Patterns

### Backend

- **Layered architecture**: Controllers → Services → Repositories. Controllers must not contain business logic.
- **Interface-driven**: Every service and repository has an `IXxx` interface. Register the interface in DI, never the concrete type directly.
- **DTOs are C# records**: Use positional records for request/response types. Validators are co-located in the same `DTOs/` file or a sibling `*Validators.cs` file.
- **Domain exceptions**: Throw `NotFoundException`, `ConflictException`, or `BusinessRuleException` from services. The middleware handles mapping to HTTP status codes — do not return error responses directly from services.
- **Repository lifetime**: Repositories are `Singleton`; services are `Scoped`.
- **Enum serialization**: Enums serialize as lowercase strings over the wire (e.g. `"income"`, `"expense"`). Parse with `ignoreCase: true` in services.

### Frontend

- **API layer is pure functions**: `src/api/` files export async functions that call `apiClient`. No state, no hooks.
- **Hooks encapsulate state + side effects**: Custom hooks in `src/hooks/` own loading/error state and expose stable callbacks via `useCallback`.
- **Pages orchestrate, components display**: Pages hold state and pass data/callbacks down as props. Components are presentational.
- **All types in `src/types/index.ts`**: Shared interfaces live here. API-specific input types (e.g. `CreateTransactionData`) may be defined in the relevant `api/` file.
- **Error shape**: API errors always conform to `ApiError { error: string; message: string; details?: unknown }` after the interceptor normalizes them.

## Naming Conventions

| Layer | Pattern | Example |
|---|---|---|
| Backend controller | `{Resource}Controller` | `TransactionsController` |
| Backend service interface | `I{Resource}Service` | `ITransactionService` |
| Backend repository interface | `I{Resource}Repository` | `ITransactionRepository` |
| Backend DTO | `{Action}{Resource}Request/Response` | `CreateTransactionRequest` |
| Frontend hook | `use{Resource}` | `useTransactions` |
| Frontend page | `{Resource}Page` | `TransactionsPage` |
| Test class | `{Subject}Tests` | `TransactionServiceTests` |
