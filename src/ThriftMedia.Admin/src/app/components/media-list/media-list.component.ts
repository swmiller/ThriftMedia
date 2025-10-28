import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-media-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './media-list.component.html',
  styleUrls: ['./media-list.component.css']
})
export class MediaListComponent {
  mediaItems: any[] = [];
}
