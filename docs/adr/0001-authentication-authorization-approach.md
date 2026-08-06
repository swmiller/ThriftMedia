# 0001 - Authentication and Authorization Approach

## Status
Proposed

## Context
Authentication and authorization are the first vertical slice and establish security patterns for the rest of the system. We need a consistent approach across API and Blazor UI, aligned with Clean Architecture, VSA, and CQRS.

## Decision
1. Implement authentication/authorization as a dedicated feature slice first.
2. Enforce authorization checks server-side at API boundaries.
3. Keep feature logic in Application slices and infrastructure concerns in Infrastructure.
4. Use this ADR as the canonical decision record once token/session strategy is finalized.

## Consequences
1. Future feature slices can reuse a stable security foundation.
2. Endpoint and handler patterns will be established early and reused consistently.
3. Additional ADRs may be required for:
   - Auth mechanism choice (cookie vs JWT vs hybrid)
   - Role/permission model
   - Session/token lifetime and refresh strategy
