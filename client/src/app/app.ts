import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService, NoticeService, errorText } from './core';
@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly auth = inject(AuthService);
  readonly notice = inject(NoticeService);
  private readonly router = inject(Router);
  readonly menuOpen = signal(false);
  readonly dark = signal(false);
  readonly leaving = signal(false);
  constructor() {
    let preference: string | null = null;
    try {
      preference = localStorage.getItem('folio-theme');
    } catch {}
    this.dark.set(
      preference ? preference === 'dark' : matchMedia('(prefers-color-scheme: dark)').matches,
    );
    this.applyTheme();
  }
  toggleTheme() {
    this.dark.update((value) => !value);
    this.applyTheme();
    try {
      localStorage.setItem('folio-theme', this.dark() ? 'dark' : 'light');
    } catch {}
  }
  private applyTheme() {
    document.documentElement.setAttribute('data-bs-theme', this.dark() ? 'dark' : 'light');
  }
  async logout() {
    this.leaving.set(true);
    try {
      await this.auth.logout();
      this.menuOpen.set(false);
      await this.router.navigate(['/login']);
    } catch (error) {
      this.notice.show(errorText(error));
    } finally {
      this.leaving.set(false);
    }
  }
}
