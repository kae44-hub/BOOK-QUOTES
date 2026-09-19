import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { LibraryPage } from './library';

describe('Publication dates', () => {
  it('renders the saved calendar date without shifting to the previous day', async () => {
    TestBed.configureTestingModule({
      imports: [LibraryPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    const fixture = TestBed.createComponent(LibraryPage);
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/books').flush([
      { id: 1, title: 'Date check', author: 'Author', publicationDate: '2026-01-01' },
    ]);
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.book-date').textContent).toContain('Jan 1, 2026');
    http.verify();
  });
});
