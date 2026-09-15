import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'fb-hero-blobs',
  imports: [],
  templateUrl: './fb-hero-blobs.html',
  styleUrl: './fb-hero-blobs.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'absolute inset-0 w-full h-full pointer-events-none' },
})
export class FbHeroBlobs {}
