# Tech Stack

## Backend

- **Runtime**: .NET 8 (ASP.NET Core Web API)
- **Language**: C# with nullable reference types and implicit usings enabled
- **Validation**: FluentValidation.AspNetCore 11.3.1 — validators live in `DTOs/` alongside their request types
- **Serialization**: `System.Text.Json` with camelCase naming policy and string enum converter
- **Storage**: In-memory repositories (no database); data does not persist across restarts
- **Error handling**: Custom `ExceptionHandlerMiddleware` maps domain exceptions to structured JSON error responses

## Frontend

- **Framework**: React 19 with TypeScript (strict mode)
- **Build tool**: Vite 8
- **Routing**: React Router DOM v7
- **HTTP client**: Axios — single `apiClient` instance in `src/api/client.ts` with a response interceptor that normalizes errors to `ApiError`
- **State**: Local component state + custom hooks (no global state library)
- **Testing**: Vitest + React Testing Library + MSW for API mocking

## Testing (Backend)

- **Framework**: xunit 2.5.3
- **Assertions**: FluentAssertions 8.9.0
- **Mocking**: Moq 4.20.72
- **Property-based testing**: FsCheck.Xunit 3.3.3
- **Integration testing**: `Microsoft.AspNetCore.Mvc.Testing` via `WebApplicationFactory`

## Common Commands

### Backend
```bash
# Build
dotnet build backend/ExpenseTracker.sln

# Run API (dev)
dotnet run --project backend/ExpenseTracker.Api

# Run tests
dotnet test backend/ExpenseTracker.sln
```

### Frontend
```bash
# Install dependencies
cd frontend && npm install

# Dev server (proxies /api → http://localhost:5000)
npm run dev

# Type-check + production build
npm run build

# Run tests (single pass)
npx vitest --run

# Lint
npm run lint
```

## Dev Ports

| Service  | URL                    |
|----------|------------------------|
| Frontend | http://localhost:5173  |
| Backend  | http://localhost:5000  |

The Vite dev server proxies all `/api` requests to the backend, so no CORS issues during local development.
