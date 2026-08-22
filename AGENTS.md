# Project rules

## Line endings

- Always use CRLF (`\r\n`) line endings when creating or modifying source code, configuration, documentation, Docker, or project files in this repository.
- Preserve CRLF when running formatters, generators, migrations, or other automated tools.
- Before completing a code change, verify that every newly created or modified text file uses CRLF.

## Architecture

- Preserve the current layered architecture and dependency direction: `Domain` has no project dependencies; `Application` depends only on `Domain`; `Infrastructure` implements `Application` contracts; and `API` composes `Application` and `Infrastructure`.
- Keep business rules and state transitions in domain entities. Domain code must not depend on ASP.NET Core, Entity Framework Core, persistence concerns, or HTTP concepts.
- Keep orchestration in application use cases. A use case may validate input, coordinate repositories, invoke domain behavior, persist through `IUnitOfWork`, and map entities to response contracts.
- Define repository and unit-of-work abstractions in `Application/Contracts`; place their Entity Framework Core implementations in `Infrastructure`.
- Keep controllers thin: bind the request, call one use case, propagate the `CancellationToken`, and translate the successful result into the appropriate HTTP response.

## Use cases and contracts

- Organize each operation under `Application/UseCases/<Aggregate>/<Operation>/`.
- Follow the existing naming pattern: `I<Operation>UseCase`, `<Operation>UseCase`, `<Operation>Request`, `<Operation>Response`, and `<Operation>Validator` when validation is required.
- Expose request and response DTOs at API boundaries. Never return domain entities directly from controllers or use cases.
- Map domain entities explicitly to response DTOs so only public fields are exposed.
- Register every new use case in `FinancialTransferProcessing.Application.DependencyInjection`.
- Keep repository interfaces focused by separating read-only and write-only contracts where the project already does so.

## Domain and validation

- Enforce domain invariants inside entities and value-oriented domain helpers, throwing `DomainException` for invalid business state or transitions.
- Use FluentValidation in the application layer for request validation and convert validation failures to `ErrorOnValidationException`.
- Translate domain failures at the application boundary to the appropriate application exception, such as `BusinessRuleException`, rather than exposing `DomainException` to the API.
- Use the existing application exceptions for HTTP semantics: validation, not found, conflict, and business-rule failures.
- Represent monetary values as integral cents using `long`; do not introduce floating-point or decimal currency representations without an explicit architectural decision.
- Use `DateTimeOffset` and UTC for persisted timestamps and domain state changes.

## Persistence

- Use asynchronous Entity Framework Core APIs and propagate the caller's `CancellationToken` through repositories and `IUnitOfWork`.
- Apply `AsNoTracking()` to read-only queries unless entity tracking is explicitly required by the operation.
- Persist writes through write-only repositories and commit them through `IUnitOfWork`.
- Keep EF Core mappings in `Infrastructure/Persistence/Configurations` and database migrations in `Infrastructure/Persistence/Migrations`.
- Register new repository implementations and infrastructure services in `FinancialTransferProcessing.Infrastructure.DependencyInjection`.
- Translate provider-specific persistence failures into application-level exceptions at the infrastructure boundary; do not leak EF Core or Npgsql exceptions into use cases.

## API conventions

- Derive API controllers from `ApiController` to preserve the versioned route pattern `api/v{version:apiVersion}/[controller]`.
- Inject use cases through action parameters with `[FromServices]`, following the existing controller style.
- Document every supported success and error status with `ProducesResponseType`, using the actual response contract returned at runtime.
- Return errors through the established `ResponseError` shape and extend `ExceptionFilter` when a new application exception requires an HTTP mapping.
- Keep automatic model-binding validation responses consistent with `ResponseError` through the configured `ApiBehaviorOptions` behavior.
- Do not place business logic, repository access, or entity mutation directly in controllers.

## General code conventions

- Keep nullable reference types enabled and model optional data explicitly with nullable types.
- Use the `Async` suffix for asynchronous repository and infrastructure methods. Use the existing `Execute` naming convention for application use cases.
- Prefer constructor injection and immutable response/request records, matching the surrounding code.
- Avoid unrelated refactors in task-scoped changes.
- Before completing a change, run `dotnet build FinancialTransferProcessing.slnx --no-restore` and resolve build warnings and errors introduced by the change.
