# Playoffs

A multi-sport tournament manager built with **.NET** as a hands-on way to practice enterprise patterns that [shopshop](https://github.com/gurungsh/shopshop) does not cover: an MVC web app, gRPC between services, DDD and CQRS, sagas, and Keycloak for identity.

Soccer comes first. Pickleball and basketball follow, to prove the sport abstraction holds. Organizers create tournaments and register teams, the system generates brackets and fixtures, referees report results, and the public follows standings and live scores.

## Status

Early setup: the repo skeleton and local infrastructure are in place, with no application code yet. The project is built as an MVP first (one soccer tournament flow with Web and one backend host) and extended in releases. There is no CI/CD pipeline; checks run locally.

## Services at a glance

| Service | Responsibility |
|---|---|
| Web | MVC site, admin area, live scores (SignalR), acts as the BFF |
| Tournament | Tournaments, teams, registration, captain invitations |
| Scheduling | Venues, fixtures, constraint-based slot assignment |
| Results | Referee assignment, score reporting and corrections, disputes, live match events |
| Notification | Event-driven invitation emails, reminders and updates |
| Keycloak | Identity (OIDC) |

## Roles

| Role | Can do |
|---|---|
| Admin | Everything an Organizer can do on any tournament, plus user and role management |
| Organizer | On their own tournaments: create, invite team captains, assign referees, resolve disputes, correct scores, disqualify teams |
| Referee | Report results and live events for assigned matches |
| Captain | Manage their own team after accepting an invitation (a team relationship, not a Keycloak role). Not in the MVP. |

## Technology

.NET, ASP.NET Core MVC, gRPC, MongoDB (aggregates, read models, live event log), Redis (cache, locks, fan-out), RabbitMQ, Keycloak on SQL Server (the only relational database, used by Keycloak alone), OpenTelemetry with Grafana, Docker Compose, then Kubernetes.
