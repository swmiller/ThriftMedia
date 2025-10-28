import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { MediaListComponent } from './components/media-list/media-list.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'media', component: MediaListComponent },
  { path: '**', redirectTo: '' }
];
