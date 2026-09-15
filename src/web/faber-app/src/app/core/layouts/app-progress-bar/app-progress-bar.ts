import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  imports: [],
  templateUrl: './app-progress-bar.html',
  styleUrl: './app-progress-bar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppProgressBar {}
