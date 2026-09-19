# Verification record

Verified on 19 September 2026.

- API integration suite: 11 passing cases, using isolated SQLite databases.
- Angular suite: 4 passing cases, including the calendar-date regression test; session expiry, CSRF-before-write ordering and save-error propagation passed in headless Chrome.
- Production Angular build passed.
- GitHub Actions independently passed API tests, Angular tests, production build and Docker build for commit c50ab1b.
- npm audit reported zero vulnerabilities after compatible dependency updates.
- NuGet reported no known vulnerable packages in the API dependency graph at check time.
- Real Neon smoke test passed: login, create/read/update/delete a disposable book, create/read/update/delete a disposable quote, and logout.
- Browser checks: registration, login, persistence after refresh, book validation, creation and editing, search, quote creation, delete cancellation, theme toggle and navigation.
- Layout checks at 1440px desktop, 768px tablet and 390px mobile. Tablet and mobile had no horizontal content overflow. Both themes inspected; mobile menu expands and navigates.
- Fixed a date-only timezone display issue caught during browser testing.
- Credential-pattern scan of source and Git history found only documented placeholders; actual connection and signing secrets stay outside Git.

## Scope limits

Responsive checks used a Chromium browser with viewport overrides, not physical phones or iOS Safari. API integration tests use SQLite, complemented by a separate PostgreSQL smoke test; they are not a full PostgreSQL regression suite. The public Render HTTPS deployment passed registration, login, Secure/HttpOnly cookie checks, five starter quotes, book and quote CRUD, logout and unauthenticated API rejection. Browser checks on the public site passed login, navigation, session persistence after reload and dark theme. A Render TLS-proxy issue found during deployment was fixed and covered by an added production antiforgery regression test.

