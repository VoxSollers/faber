import { ChangeDetectionStrategy, Component } from '@angular/core';
import { SectionList } from '../section-list/section-list';

@Component({
  selector: 'app-editor',
  imports: [SectionList],
  templateUrl: './editor.html',
  styleUrl: './editor.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Editor {}
