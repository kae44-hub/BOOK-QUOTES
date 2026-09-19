import { Component, ElementRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, Book, NoticeService, errorText } from './core';
@Component({
  selector: 'app-library',
  imports: [RouterLink, DatePipe],
  templateUrl: './library.html',
})
export class LibraryPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly notice = inject(NoticeService);
  readonly books = signal<Book[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly query = signal('');
  readonly sort = signal('newest');
  readonly pending = signal<Book | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal('');
  @ViewChild('deleteDialog') dialog!: ElementRef<HTMLDialogElement>;
  readonly filtered = computed(() => {
    const term = this.query().trim().toLocaleLowerCase();
    const result = this.books().filter((b) =>
      (b.title + ' ' + b.author).toLocaleLowerCase().includes(term),
    );
    return this.sort() === 'title'
      ? result.sort((a, b) => a.title.localeCompare(b.title))
      : result.sort((a, b) => b.id - a.id);
  });
  readonly authors = computed(
    () => new Set(this.books().map((b) => b.author.toLocaleLowerCase())).size,
  );
  ngOnInit() {
    void this.load();
  }
  async load() {
    this.loading.set(true);
    this.error.set('');
    try {
      this.books.set(await this.api.get<Book[]>('books'));
    } catch (error) {
      this.error.set(errorText(error));
    } finally {
      this.loading.set(false);
    }
  }
  askDelete(book: Book) {
    this.pending.set(book);
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
    const book = this.pending();
    if (!book || this.deleting()) return;
    this.deleting.set(true);
    try {
      await this.api.remove('books/' + book.id);
      this.books.update((books) => books.filter((b) => b.id !== book.id));
      this.dialog.nativeElement.close();
      this.pending.set(null);
      this.notice.show('Book removed from the library.');
    } catch (error) {
      this.deleteError.set(errorText(error));
    } finally {
      this.deleting.set(false);
    }
  }
}
