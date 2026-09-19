import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService, Book, Quote, NoticeService, errorText } from './core';
@Component({
  selector: 'app-editor',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './editor.html',
})
export class EditorPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);
  private readonly notice = inject(NoticeService);
  readonly isQuote = this.route.snapshot.data['quote'] === true;
  readonly id = this.route.snapshot.paramMap.get('id');
  readonly collection = this.isQuote ? 'quotes' : 'books';
  readonly noun = this.isQuote ? 'quote' : 'book';
  readonly busy = signal(false);
  readonly loading = signal(!!this.id);
  readonly loadFailed = signal(false);
  readonly error = signal('');
  readonly form = this.fb.nonNullable.group({
    title: [
      '',
      this.isQuote
        ? []
        : [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)],
    ],
    author: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    publicationDate: ['', this.isQuote ? [] : [Validators.required]],
    text: [
      '',
      this.isQuote
        ? [Validators.required, Validators.pattern(/\S/), Validators.maxLength(1000)]
        : [],
    ],
  });
  ngOnInit() {
    if (this.id) void this.load();
  }
  async load() {
    this.loading.set(true);
    this.error.set('');
    this.loadFailed.set(false);
    try {
      this.form.patchValue(await this.api.get<Book | Quote>(this.collection + '/' + this.id));
    } catch (error) {
      this.error.set(errorText(error));
      this.loadFailed.set(true);
    } finally {
      this.loading.set(false);
    }
  }
  invalid(field: 'title' | 'author' | 'text' | 'publicationDate') {
    const control = this.form.controls[field];
    return control.touched && control.invalid;
  }
  async save() {
    if (this.busy() || this.loading() || this.loadFailed()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.busy.set(true);
    this.error.set('');
    const v = this.form.getRawValue();
    const body = this.isQuote
      ? { text: v.text.trim(), author: v.author.trim() }
      : { title: v.title.trim(), author: v.author.trim(), publicationDate: v.publicationDate };
    try {
      await this.api.save(this.collection + (this.id ? '/' + this.id : ''), body, !!this.id);
      this.notice.show(
        (this.isQuote ? 'Quote' : 'Book') + (this.id ? ' updated.' : ' added to your collection.'),
      );
      await this.router.navigate(['/' + this.collection]);
    } catch (error) {
      this.error.set(errorText(error));
    } finally {
      this.busy.set(false);
    }
  }
}
