# ThriftMedia

ThriftMedia is a distributed platform for discovering local thrift store inventory and helping businesses promote in-store visits. It is built with .NET Aspire, Blazor, PostgreSQL, and modern .NET architecture practices.

The current product requirements are defined in `doc/App Requirements.md` and this README reflects that baseline.

## Product Scope

ThriftMedia includes three primary product surfaces:

- **Public-Facing Website** for consumers to browse inventory, discover stores, and view promotions without registration.
- **Business Owner Administration Website** for store owners/team members to manage business profiles, inventory media, promotions, and business-scoped analytics.
- **Platform Administration Capabilities** for authorized ThriftMedia staff to manage business onboarding, moderation, platform settings, support workflows, and platform-wide analytics.

The platform business model is ad-supported (initially Google Ads), and the product explicitly **does not include e-commerce checkout**. The primary goal is driving in-person store visits.

## Functional Highlights

- Public browse/search with category, location, price, and availability filtering
- Location-aware discovery with configurable radius and manual location override
- Promotion/event visibility and time-sensitive offers
- Rich keyword search with all keywords AND'ed together for relevance
- Role-based business access (**Owner** and **Worker**) scoped to authorized businesses only
- Platform-level business account approval, suspension, and reactivation workflows
- Automated ingestion and processing pipeline for uploaded media, validation, moderation, and monitoring
- Secure API/integration support for real-time and scheduled data updates

## Non-Functional and Security Baseline

Requirements include:

- OWASP-aligned secure coding and strong input/output protection
- OAuth 2.0-based authentication and authorization controls
- Encryption in transit (TLS 1.2+) and at rest
- Auditing, logging, observability, and anomaly/security monitoring
- WCAG 2.1 AA accessibility targets
- Performance, scalability, availability, and disaster recovery targets
- Privacy and compliance alignment (GDPR/CCPA and related policies)

## Architecture and Technology Direction

- Clean Architecture with CQRS separation
- Custom mediator implementation: `ThriftMedia.Mediator` (in place of MediatR)
- FluentValidation for validation
- ASP.NET minimal APIs for service endpoints
- Blazor-based web experiences
- Dapper with PostgreSQL (containerized engine, data files bind-mounted to `c:\ThriftMediaDb` on the host)
- DbUp for database schema migrations
- .NET Aspire orchestration and containerized deployment model

## Repository Structure

- **src/ThriftMedia.Web**: Public-facing Blazor WebAssembly UI
- **src/ThriftMedia.Admin**: Business owner administration Blazor UI
- **src/ThriftMedia.Api**: Minimal API endpoints and application-facing services
- **src/ThriftMedia.AppHost**: .NET Aspire distributed application orchestrator
- **src/ThriftMedia.Application**: Application-layer use cases and CQRS handlers
- **src/ThriftMedia.Domain**: Domain models, rules, and core business logic
- **src/ThriftMedia.Infrastructure**: Dapper-based persistence and infrastructure implementations
- **src/ThriftMedia.DbMigrator**: DbUp-based schema migration runner
- **src/ThriftMedia.Mediator**: Custom mediator library used by CQRS
- **src/ThriftMedia.ServiceDefaults**: Shared service defaults, telemetry, and resilience setup
- **src/ThriftMedia.Tests**: Automated tests (xUnit)
- **.github/**: Project instructions, workflows, and documentation
- **doc/**: Product requirements and supporting documentation

## Getting Started

1. **Clone the repository**
2. **Build the solution**
   ```sh
   dotnet build ThriftMedia.sln
   ```
3. **Run the distributed app**
   ```sh
   dotnet run --project src/ThriftMedia.AppHost/ThriftMedia.AppHost.csproj
   ```

## Contributing

- Follow coding and architecture standards in `.github/copilot-instructions.md`
- Treat `doc/App Requirements.md` as the primary requirements source
- Keep README and related documentation aligned when requirements change

## License

This project is licensed under the MIT License:

Copyright (c) 2025 ThriftMedia contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
