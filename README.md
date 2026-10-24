# Playoffs

A multi-sport tournament manager built with **.NET** as a hands-on way to practice enterprise patterns that [shopshop](https://github.com/gurungsh/shopshop) does not cover: an MVC web app, gRPC between services, DDD and CQRS, sagas, and Keycloak for identity.

Soccer comes first. Pickleball and basketball follow, to prove the sport abstraction holds. Organizers create tournaments and register teams, the system generates brackets and fixtures, referees report results, and the public follows standings and live scores.

## Contents

- [Services](#services)
- [Technology stack](#technology-stack)
- [Getting started](#getting-started)
- [Roadmap](#roadmap)

## Services

| Service | What it does | Ports |
|---|---|---|
| Tournament | Tournaments and teams over gRPC. Later: registration, captain invitations, starting a tournament | 5101 (gRPC), 5100 (`/health`) |
| Web | MVC site, admin area, live scores (SignalR). Acts as the BFF, so tokens stay on the server | 5200 (planned) |
| Results | Referee assignment, score reporting and corrections, disputes, live match events. Hosted inside Tournament until it is split out | planned |
| Scheduling | Venues, fields and constraint-based slot assignment | planned |
| Notification | Event-driven invitation emails, reminders and updates | planned |
| Keycloak | Identity (OIDC) with the Admin, Organizer and Referee roles. Runs on SQL Server | 8080 |

Every gRPC call needs a token from Keycloak. `GET /health` is anonymous.

## Technology stack

| Area | Technology |
|---|---|
| Runtime and APIs | .NET 8, ASP.NET Core gRPC (`Grpc.AspNetCore`) |
| Contracts | Protocol Buffers, generated with `Grpc.Tools`, checked with Buf (lint and breaking changes) |
| Data | MongoDB 8 (single-node replica set) with `MongoDB.Driver` 3, no EF Core |
| Identity | Keycloak 26 on SQL Server 2022, JWT Bearer validation with a required audience |
| Logging | Serilog (console, request logging) |
| Testing | xUnit, `WebApplicationFactory`, Testcontainers (MongoDB) with a fake signing key instead of Keycloak |
| Code quality | .NET analyzers with warnings as errors, `dotnet format`, CSharpier, central package management |
| Local infrastructure | Docker Compose (MongoDB, SQL Server, Keycloak) |
| Planned | ASP.NET Core MVC and SignalR, RabbitMQ, Redis, OpenTelemetry with Grafana, Stateless, FsCheck, Playwright, NetArchTest, Kubernetes on kind |

## Getting started

Prerequisites: the .NET SDK from `global.json`, Docker, and Buf (`brew install bufbuild/buf/buf`).

1. **Create `.env`.** Copy `.env.example` to `.env` and choose your own values. Nothing secret is committed, so every value must be filled in, and Compose refuses to start without them.

   ```bash
   cp .env.example .env
   ```

2. **Start MongoDB, SQL Server and Keycloak.**

   ```bash
   docker compose up -d --wait
   ```

   Keycloak imports the `playoffs` realm with seed users (`admin`, `organizer1`, `organizer2`, `referee1`, `referee2`, all with `SEED_USER_PASSWORD`). The admin console is at http://localhost:8080.

3. **Build and test.**

   ```bash
   ./scripts/check.sh
   ```

   This runs Buf, build, format check and tests. The tests start their own MongoDB in Testcontainers and do not need Keycloak.

4. **Run the Tournament service.**

   ```bash
   dotnet run --project src/Playoffs.Tournament
   ```

   The launch profile sets `Development`, which seeds two sample tournaments. Check http://localhost:5100/health.

### Try a call

Get a token for `organizer1` and fetch the seeded Summer Cup with [grpcurl](https://github.com/fullstorydev/grpcurl) (`brew install grpcurl`):

```bash
source .env
TOKEN=$(curl -s http://localhost:8080/realms/playoffs/protocol/openid-connect/token \
  -d grant_type=password -d client_id=playoffs-dev-cli \
  -d username=organizer1 -d "password=$SEED_USER_PASSWORD" | jq -r .access_token)

grpcurl -plaintext \
  -import-path src/Playoffs.Contracts/proto -proto playoffs/tournament/v1/tournament.proto \
  -H "authorization: Bearer $TOKEN" \
  -d '{"id": "10000000-0000-4000-8000-000000000001"}' \
  localhost:5101 playoffs.tournament.v1.TournamentService/GetTournament
```

Without the `authorization` header the call returns `Unauthenticated`.

## Roadmap

Unchecked items are not implemented yet:

- [x] **Repo skeleton**: solution, central package management, analyzers with warnings as errors, and `scripts/check.sh` for restore, build, format check and tests
- [x] **Local infrastructure and identity**: MongoDB replica set, Keycloak on SQL Server with the `playoffs` realm, roles, clients and seed users
- [x] **Tournament service shell**: gRPC `GetTournament` and `ListTournaments` over MongoDB, JWT validation, dev seed data, integration tests and Buf checks
- [ ] **Web shell**: MVC layout, OIDC login and logout, BFF token handling, role policies, Admin and Organizer areas
- [ ] **Web calls Tournament**: typed gRPC client with token forwarding and deadlines, public tournament list
- [ ] **Domain logic**: sport rules (soccer first), round-robin fixture generator and standings with tiebreakers, with property-based tests
- [ ] **Tournament aggregate**: registration, capacity and unique team names, domain events, an in-house mediator with pipeline behaviors
- [ ] **Tournament persistence and API**: repository base class, optimistic concurrency, soft delete, audit, and gRPC commands
- [ ] **Organizer UI**: create tournaments and register teams, owner-only policy with Admin bypass
- [ ] **Results module**: Match aggregate with its own database and contract, idempotent match creation
- [ ] **Start tournament and referee assignment**: retry-safe start that generates fixtures and creates matches
- [ ] **Score reporting and Referee UI**: idempotent, concurrency-safe result reporting by the assigned referee
- [ ] **Public pages and MVP acceptance**: fixtures and standings pages, architecture tests, Playwright happy path
- [ ] **Observability**: OpenTelemetry traces, metrics and logs in Grafana, one trace from browser to MongoDB
- [ ] **Captains and invitations**: single-use invitation links, captains manage their own team
- [ ] **Match lifecycle**: full state machine, disputes, score corrections and team disqualification
- [ ] **Scheduling service**: venues, availability and constraint-based fixture scheduling over gRPC
- [ ] **Live scores**: bidirectional gRPC streaming from a scorekeeper client, Redis live state, SignalR to the public page
- [ ] **Messaging and read models**: RabbitMQ with outbox and inbox, Results as its own service, event-driven standings
- [ ] **Saga, notifications and jobs**: start-tournament saga with compensation, Notification service, reminders and auto-forfeit
- [ ] **More sports and formats**: basketball, pickleball, elimination, group + knockout and Swiss pairing
- [ ] **Web polish and security**: localization, caching, CSP, MFA, gRPC-Web and mTLS, rate limiting
- [ ] **Containers and Kubernetes**: images for every service, kind with probes and autoscaling, k6 load tests

There is no CI/CD pipeline. `scripts/check.sh` runs Buf, build, format check and tests locally and is the gate before every commit.
