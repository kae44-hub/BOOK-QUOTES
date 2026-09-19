import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Router, CanActivateFn } from '@angular/router';
import { firstValueFrom, catchError, throwError } from 'rxjs';
export interface Book {
  id: number;
  title: string;
  author: string;
  publicationDate: string;
}
export interface Quote {
  id: number;
  text: string;
  author: string;
}
export interface User {
  id: string;
  username: string;
}
export function errorText(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) return 'Something went wrong. Please try again.';
  if (error.status === 0)
    return 'We could not reach the server. Please check your connection and try again.';
  if (error.status === 429) return 'Too many attempts. Please wait a few minutes and try again.';
  if (error.status === 401)
    return error.error?.message || 'Your session has ended. Please sign in again.';
  if (error.status === 404)
    return 'This item no longer exists. Return to your collection and try again.';
  if (error.error?.message) return error.error.message;
  if (error.error?.errors) return Object.values(error.error.errors).flat().join(' ');
  if (error.status === 400) return 'Please check the form, refresh the page and try again.';
  return 'We could not save your changes. Your entries are still here; please try again.';
}
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  readonly user = signal<User | null>(null);
  private restored = false;
  async csrf() {
    await firstValueFrom(this.http.get('/api/auth/csrf'));
  }
  async restore(): Promise<boolean> {
    if (this.restored) return !!this.user();
    try {
      this.user.set(await firstValueFrom(this.http.get<User>('/api/auth/me')));
    } catch {
      this.user.set(null);
    }
    this.restored = true;
    return !!this.user();
  }
  async login(username: string, password: string) {
    await this.csrf();
    this.user.set(
      await firstValueFrom(this.http.post<User>('/api/auth/login', { username, password })),
    );
    this.restored = true;
    await this.csrf();
  }
  async register(username: string, password: string) {
    await this.csrf();
    await firstValueFrom(this.http.post('/api/auth/register', { username, password }));
  }
  async logout() {
    await this.csrf();
    await firstValueFrom(this.http.post('/api/auth/logout', {}));
    this.user.set(null);
    this.restored = true;
    await this.csrf();
  }
}
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  get<T>(path: string) {
    return firstValueFrom(this.http.get<T>('/api/' + path));
  }
  async save<T>(path: string, body: unknown, edit = false): Promise<T> {
    await this.auth.csrf();
    return firstValueFrom(
      edit ? this.http.put<T>('/api/' + path, body) : this.http.post<T>('/api/' + path, body),
    );
  }
  async remove(path: string) {
    await this.auth.csrf();
    return firstValueFrom(this.http.delete('/api/' + path));
  }
}
@Injectable({ providedIn: 'root' })
export class NoticeService {
  readonly message = signal('');
  private timeout?: ReturnType<typeof setTimeout>;
  show(message: string) {
    clearTimeout(this.timeout);
    this.message.set(message);
    this.timeout = setTimeout(() => this.message.set(''), 5000);
  }
}
export const authGuard: CanActivateFn = async () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return (await auth.restore()) ? true : router.createUrlTree(['/login']);
};
export const sessionInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return next(req).pipe(
    catchError((error) => {
      if (error.status === 401 && auth.user() && !req.url.includes('/auth/login')) {
        auth.user.set(null);
        void router.navigate(['/login'], { queryParams: { expired: '1' } });
      }
      return throwError(() => error);
    }),
  );
};
