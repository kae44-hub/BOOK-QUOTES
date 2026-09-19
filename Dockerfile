FROM node:22-bookworm-slim AS frontend
WORKDIR /src/client
COPY client/package*.json ./
RUN npm ci
COPY client/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend
WORKDIR /src
COPY server/BookQuotes.Api.csproj server/
RUN dotnet restore server/BookQuotes.Api.csproj
COPY server/ server/
RUN dotnet publish server/BookQuotes.Api.csproj -c Release -o /out --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=backend /out ./
COPY --from=frontend /src/client/dist/client/browser ./wwwroot
ENV ASPNETCORE_HTTP_PORTS=10000
ENV ASPNETCORE_ENVIRONMENT=Production
USER $APP_UID
EXPOSE 10000
ENTRYPOINT ["dotnet", "BookQuotes.Api.dll"]
