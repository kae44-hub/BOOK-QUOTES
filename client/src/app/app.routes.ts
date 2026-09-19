import { Routes } from '@angular/router';
import { authGuard } from './core';
import { AuthPage } from './auth';
import { LibraryPage } from './library';
import { QuotesPage } from './quotes';
import { EditorPage } from './editor';
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'books' },
  { path: 'login', component: AuthPage, title: 'Sign in · Folio' },
  {
    path: 'register',
    component: AuthPage,
    data: { register: true },
    title: 'Create account · Folio',
  },
  {
    path: 'books/new',
    component: EditorPage,
    canActivate: [authGuard],
    title: 'Add a book · Folio',
  },
  {
    path: 'books/:id/edit',
    component: EditorPage,
    canActivate: [authGuard],
    title: 'Edit a book · Folio',
  },
  { path: 'books', component: LibraryPage, canActivate: [authGuard], title: 'Library · Folio' },
  {
    path: 'quotes/new',
    component: EditorPage,
    data: { quote: true },
    canActivate: [authGuard],
    title: 'Add a quote · Folio',
  },
  {
    path: 'quotes/:id/edit',
    component: EditorPage,
    data: { quote: true },
    canActivate: [authGuard],
    title: 'Edit a quote · Folio',
  },
  { path: 'quotes', component: QuotesPage, canActivate: [authGuard], title: 'My Quotes · Folio' },
  { path: '**', redirectTo: 'books' },
];
