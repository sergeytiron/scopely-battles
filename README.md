# ScopelyBattles

## Stack

* C# with .NET 10 and FastEndpoints
* PostgreSQL for storage and battle queueing

## Projects

* `ScopelyBattles.Api` - HTTP API
* `ScopelyBattles.Worker` - background battle processor
* `ScopelyBattles.Shared` - shared models and services
* `ScopelyBattles.UnitTests` - unit tests for battle logic and utilities
* `ScopelyBattles.IntegrationTests` - integration tests with Docker dependencies

## Scope

In scope:

* Player creation
* Battle submission
* Concurrent battle processing
* Battle report generation
* Leaderboard updates
* Unit and integration testing
* Basic authentication

Out of scope:

* Battle notifications
* Multi-region deployment
* Advanced retries, throttling, and circuit breakers
* Advanced authentication/authorization
* Full observability stack

## Design

Two services: API and Worker.

The API creates players, returns the leaderboard, and accepts battle submissions. A submitted battle lands in PostgreSQL as a queued record.

The Worker pulls queued battles and processes them in the background. For each battle it applies the result to both players, writes the battle report, updates the leaderboard, and marks the battle completed, all in one transaction.

Battles that share a player are serialized with row-level locks. Battles that don't share a player run concurrently across worker instances.

PostgreSQL is the source of truth because finishing a battle changes several rows that have to commit together: both players, the report, and the leaderboard score. That's a transaction, so a relational store fits. Redis would be a reasonable choice for a leaderboard or cache projection in production, but it's not worth the extra dependency for this exercise.

## Endpoints

```http
GET  /players/{id}
POST /players

GET  /battles/{id}
POST /battles

GET /leaderboard
```
