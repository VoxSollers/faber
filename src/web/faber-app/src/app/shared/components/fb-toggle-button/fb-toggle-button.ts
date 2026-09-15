import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'fb-toggle-button',
  templateUrl: './fb-toggle-button.html',
  styleUrl: './fb-toggle-button.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbToggleButton {
  isOpen = input(false);
  clicked = output<void>();

  protected readonly stateClasses = computed(() =>
    this.isOpen()
      ? 'rounded-[20px] bg-primary text-primary-foreground'
      : 'rounded-[20px] bg-foreground text-card dark:bg-surface dark:text-foreground',
  );

  onButtonClick() {
    this.clicked.emit();
  }
}
