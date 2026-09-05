import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
interface Book {
  id: number;
  title: string;
  author: string;
  publicationDate: string;
}
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('client');
  books: Book[] = [
  {
    id: 1,
    title: 'Pride and Prejudice',
    author: 'Jane Austen',
    publicationDate: '1813-01-28'
  },
  {
    id: 2,
    title: 'The Hobbit',
    author: 'J. R. R. Tolkien',
    publicationDate: '1937-09-21'
  },
  {
    id: 3,
    title: 'Nineteen Eighty-Four',
    author: 'George Orwell',
    publicationDate: '1949-06-08'
  }
];
addPracticeBook(): void {
  const nextId =
    Math.max(0, ...this.books.map(book => book.id)) + 1;

  this.books = [
    ...this.books,
    {
      id: nextId,
      title: 'My Practice Book',
      author: 'Your Name',
      publicationDate: '2026-01-01'
    }
  ];
}
}
