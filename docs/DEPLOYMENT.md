# Deploy to Render with the existing GitHub repository

This configuration deploys a single Docker web service in Frankfurt and uses the existing Neon PostgreSQL database. It does not create a paid database.

1. Sign in to Render. Connect GitHub and authorize only the repository you want to deploy when possible.
2. Choose **New → Blueprint** and select **kae44-hub/book-quotes**.
3. Select the branch containing this completed version and use the root **render.yaml**.
4. Review the proposed service: Docker, Frankfurt, **Free**.
5. Supply **ConnectionStrings__DefaultConnection** using the current Neon connection in Npgsql format:
   `Host=YOUR_HOST;Port=5432;Database=neondb;Username=YOUR_ROLE;Password=YOUR_PASSWORD;SSL Mode=Require;Channel Binding=Require`
6. The Blueprint generates **Jwt__Key**. Do not replace it on every deployment; changing it signs everyone out.
7. Deploy. The Docker build runs Angular production build and .NET publish; on startup EF applies pending migrations.
8. Open the assigned HTTPS URL. Test registration, sign-in, book CRUD, quote CRUD, refresh, dark mode and logout.
9. Add the verified URL to README and the submission message.

If using **New → Web Service** instead, choose this repository, Docker runtime, Free plan and Frankfurt region. Set:
- ASPNETCORE_ENVIRONMENT = Production
- ASPNETCORE_HTTP_PORTS = 10000
- Database__AutoMigrate = true
- ConnectionStrings__DefaultConnection = the private Npgsql connection
- Jwt__Key = a cryptographically random value of at least 32 bytes

Set health check path to /health. Leave Docker build/start commands at their defaults.

## Troubleshooting

- **Authentication failed for database user:** use the host, database, role and current password from the same Neon branch. Avoid old copied passwords.
- **Initialization string format error:** the API needs Host=...;Database=... format, not a PostgreSQL URL.
- **Cookie login works locally but fails online:** use the HTTPS Render URL and keep frontend/API on that same origin.
- **Blank page / route refresh:** Docker must copy client/dist/client/browser to wwwroot. ASP.NET serves index.html for frontend routes.
- **First request is slow:** free web services sleep after inactivity; wait for startup and retry.
- **Missing configuration:** inspect variable names including double underscores.
- **Build fails:** open the Render logs and fix the first actual error. Never paste secrets or full connection strings into a public issue.

Sources: [Render Blueprints](https://render.com/docs/blueprint-spec), [free service behavior](https://render.com/docs/free).
