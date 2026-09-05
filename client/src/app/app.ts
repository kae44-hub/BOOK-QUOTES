import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Book {
  id: number;
  title: string;
  author: string;
  publicationDate: string;
}

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly http = inject(HttpClient);

  readonly books = signal<Book[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.http.get<Book[]>('/api/books').subscribe({
      next: (booksFromApi) => {
        this.books.set(booksFromApi);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set(
          'Could not load the books. Check that the API is running.'
        );
        this.loading.set(false);
      }
    });
  }
}