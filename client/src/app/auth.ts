import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService, errorText } from './core';
@Component({
  selector: 'app-auth',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './auth.html',
})
export class AuthPage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  readonly register = this.route.snapshot.data['register'] === true;
  readonly expired = this.route.snapshot.queryParamMap.has('expired');
  readonly busy = signal(false);
  readonly error = signal('');
  readonly success = signal(false);
  readonly reveal = signal(false);
  readonly form = this.fb.nonNullable.group({
    username: [
      '',
      [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(30),
        Validators.pattern(/^[a-zA-Z0-9_]+$/),
      ],
    ],
    password: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(128)]],
  });
  async submit() {
    if (this.busy()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.busy.set(true);
    this.error.set('');
    const { username, password } = this.form.getRawValue();
    try {
      if (this.register) {
        await this.auth.register(username, password);
        this.form.reset();
        this.success.set(true);
      } else {
        await this.auth.login(username, password);
        await this.router.navigate(['/books']);
      }
    } catch (error) {
      this.error.set(errorText(error));
    } finally {
      this.busy.set(false);
    }
  }
}
