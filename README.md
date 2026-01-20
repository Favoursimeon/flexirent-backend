# FlexiRent .NET 8 Backend (MySQL)

This repository contains a production-ready scaffold for FlexiRent backend using .NET 8, EF Core with MySQL, JWT auth, and Swagger.

Quick start (development):
1. Copy `src/FlexiRent.Api/appsettings.json.example` to `src/FlexiRent.Api/appsettings.json` and set connection strings & secrets.
2. Build and run with Docker:
   - docker-compose up --build
   - API will be available at http://localhost:5000
3. Run EF migrations (recommended) or apply `scripts/schema.mysql.sql`.

Supabase migration notes:
- Replace Supabase REST calls with endpoints under `/api/v1/...` as implemented.
- Replace Supabase auth with `/api/v1/auth/*` endpoints.
- Use `/api/v1/ratings/*` for RPC-style rating functions.
- Update frontend env to point to this API.

Postman/Insomnia:
- Import `postman/postman_collection_flexirent.json` for sample requests.
- Swagger is available at `/swagger`.

Notes:
- Email sending uses SendGrid (configure SendGrid:ApiKey in appsettings or secrets).
- SignalR is available at `/hubs/chat`.
- For production: configure HTTPS, strong JWT secret, CORS restrictions, logging and secrets via environment variables or secret manager.
