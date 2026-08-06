# Product Requirements

## Purpose
This document captures cross-cutting, product-level requirements that apply to all features.

## Scope
These requirements are global and should be referenced by all feature requirement documents.

## Functional Requirements
1. The application must support secure user authentication.
2. The application must support role-based authorization for protected capabilities.
3. The application must expose API and UI functionality aligned to the same permission model.

## Non-Functional Requirements
1. Security controls must follow OWASP-aligned secure coding practices.
2. Data persistence uses PostgreSQL.
3. Database schema changes are managed through DbUp SQL migrations.
4. Application components run in Docker containers, orchestrated by .NET Aspire.
5. APIs follow ASP.NET minimal API best practices.

## Architecture Constraints
1. Clean Architecture boundaries are enforced.
2. Vertical Slice Architecture is used for organizing features.
3. CQRS is implemented through `ThriftMedia.Mediator`.
4. Validation uses FluentValidation.
5. Data access uses Dapper.

## Quality Requirements
1. New behavior is covered with focused xUnit tests.
2. Tests should be isolated, fast, and reliable.
3. Changes should preserve backward-compatible behavior unless explicitly approved.
