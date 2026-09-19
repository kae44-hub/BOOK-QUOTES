# Assignment requirement audit

Checked against the supplied internship brief on 19 September 2026.
Application features are implemented. Submission is not yet complete: public hosting and broader device/browser verification remain outstanding.

| Requirement | Status | Implementation / evidence |
|---|---|---|
| Angular 20 frontend | Complete | Angular 20 dependencies and production build |
| .NET 9 C# REST API | Complete | net9.0 target; controllers with HTTP GET/POST/PUT/DELETE |
| Single-page book application | Complete | Angular router; / redirects to /books after authentication |
| Home lists all books | Complete | LibraryPage loads GET /api/books |
| Add New Book opens a form | Complete | /books/new; title, author and publication date controls |
| Adding returns to list with saved book | Complete | EditorPage saves then navigates to /books; verified in browser |
| Edit opens populated form | Complete | /books/:id/edit; verified saved values load |
| Editing returns to updated list | Complete | PUT then navigate and reload; API and browser checks |
| Delete removes book from list | Implemented and API-tested | DELETE followed by signal-list update; confirmation dialog included |
| Registration and username/password sign-in | Complete | Separate registration and sign-in views; verified against Neon |
| Backend issues JWT after successful sign-in | Complete | Signed token returned in Set-Cookie |
| Secure token storage and subsequent requests | Complete locally; HTTPS deployment check pending | HttpOnly cookie, Secure in production, SameSite=Strict; browser attaches cookie |
| Backend token validation protects CRUD | Complete | Authorize on both controllers; signature, issuer, audience, expiry and session validation |
| Separate My Quotes view | Complete | /quotes with visible My Quotes heading |
| Five quotes shown | Complete | Five original editable starter quotes are created for every new account |
| Add, edit and remove quotes | Implemented and API-tested | Owner-scoped quote endpoints and shared editor; browser creation checked |
| Menu between books and quotes | Complete | Desktop navigation and collapsible mobile menu |
| Responsive desktop/tablet/mobile layouts | Viewport-tested | 1440px, 768px and 390px checks; spacing and overflow inspected |
| Menus collapse on smaller screens | Complete | Mobile toggle and navigation checked |
| Tests on different devices and browsers | Partial | Chromium viewport checks and headless browser tests; physical phones/tablets and broader browser UI flows not yet verified |
| Bootstrap buttons, forms and layout | Complete | Bootstrap 5.3.8; btn, form-control, form-select, container and other classes |
| Font Awesome icons render | Complete | Font Awesome 6.7.2 solid icons, bundled locally; inspected in UI |
| Light/dark toggle (extra challenge) | Complete | Toggle and both themes checked; preference stored locally |
| Free hosted application | Pending | Render Docker Blueprint is ready, free plan and Frankfurt configured; account sign-in/deployment needed |
| Published page link and GitHub link | Partial | GitHub is public; live site URL does not exist yet; email draft is not sent |

## Interpretation of the brief

"One-page" is implemented as a single-page application: forms and quotes have client-side routes within the same Angular app.

"Single sign-on page" is interpreted in the context of the brief as the requested username/password sign-in page with registration and JWT authentication. There is no external enterprise SSO provider.

The five initial quotes are original starter text attributed to Folio notes. They are editable so the applicant can replace them with five personally chosen favourites.

## Verified checks

The latest implementation commit, 8f5c681, passed GitHub Actions: API tests, Angular tests, production frontend build and Docker build.
- 10 API integration test cases.
- 4 Angular test cases.
- Separate real PostgreSQL CRUD smoke test.
- Responsive and theme browser checks described in VERIFICATION.md.

## Finish before sending

1. Complete Render sign-in and deploy the repository.
2. Test the public HTTPS URL: register, sign in, book CRUD, quote CRUD, refresh, logout.
3. Test that live URL in another browser and on an actual phone/tablet where available.
4. Add the verified live URL to README and the submission draft.
5. Review the draft and send both links to Marco. No email has been sent by the assistant.

