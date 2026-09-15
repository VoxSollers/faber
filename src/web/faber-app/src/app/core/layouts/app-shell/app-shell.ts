import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppNavbar } from '../app-navbar/app-navbar';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, AppNavbar],
  templateUrl: './app-shell.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
})
export class AppShell {}
