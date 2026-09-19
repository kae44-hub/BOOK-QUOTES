import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService, ApiService } from './core';

describe('Session and mutation handling', () => {
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  it('treats an expired session as signed out', async () => {
    const auth = TestBed.inject(AuthService);
    const result = auth.restore();
    http.expectOne('/api/auth/me').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(await result).toBeFalse();
    expect(auth.user()).toBeNull();
  });
  it('loads CSRF protection before saving a book', async () => {
    const api = TestBed.inject(ApiService);
    const body = { title: 'A book', author: 'An author', publicationDate: '2000-01-01' };
    const result = api.save('books', body);
    http.expectNone('/api/books');
    http.expectOne('/api/auth/csrf').flush({ ready: true });
    await Promise.resolve();
    await Promise.resolve();
    const request = http.expectOne('/api/books');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ id: 1, ...body });
    expect(await result).toEqual({ id: 1, ...body });
  });
  it('propagates a failed save so the form can retain user input', async () => {
    const api = TestBed.inject(ApiService);
    const result = api.save('books', {});
    http.expectOne('/api/auth/csrf').flush({ ready: true });
    await Promise.resolve();
    await Promise.resolve();
    http
      .expectOne('/api/books')
      .flush({ message: 'Please enter a title.' }, { status: 400, statusText: 'Bad Request' });
    await expectAsync(result).toBeRejected();
  });
});
