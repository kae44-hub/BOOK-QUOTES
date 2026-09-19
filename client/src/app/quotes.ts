import { Component, ElementRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, Quote, NoticeService, errorText } from './core';
@Component({ selector: 'app-quotes', imports: [RouterLink], templateUrl: './quotes.html' })
export class QuotesPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly notice = inject(NoticeService);
  readonly quotes = signal<Quote[]>([]);
  readonly query = signal('');
  readonly loading = signal(true);
  readonly error = signal('');
  readonly pending = signal<Quote | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal('');
  @ViewChild('deleteDialog') dialog!: ElementRef<HTMLDialogElement>;
  readonly filtered = computed(() =>
    this.quotes().filter((q) =>
      (q.text + ' ' + q.author)
        .toLocaleLowerCase()
        .includes(this.query().trim().toLocaleLowerCase()),
    ),
  );
  ngOnInit() {
    void this.load();
  }
  async load() {
    this.loading.set(true);
    this.error.set('');
    try {
      this.quotes.set(await this.api.get<Quote[]>('quotes'));
    } catch (error) {
      this.error.set(errorText(error));
    } finally {
      this.loading.set(false);
    }
  }
  askDelete(quote: Quote) {
    this.pending.set(quote);
    this.deleteError.set('');
    this.dialog.nativeElement.showModal();
  }
  closeDialog() {
    if (!this.deleting()) {
      this.dialog.nativeElement.close();
      this.pending.set(null);
    }
  }
  async remove() {
    const quote = this.pending();
    if (!quote || this.deleting()) return;
    this.deleting.set(true);
    try {
      await this.api.remove('quotes/' + quote.id);
      this.quotes.update((quotes) => quotes.filter((q) => q.id !== quote.id));
      this.dialog.nativeElement.close();
      this.pending.set(null);
      this.notice.show('Quote removed.');
    } catch (error) {
      this.deleteError.set(errorText(error));
    } finally {
      this.deleting.set(false);
    }
  }
}
