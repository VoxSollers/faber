import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { FbButton } from '../fb-button/fb-button';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
}

@Component({
  selector: 'fb-confirm-dialog',
  imports: [FbButton],
  templateUrl: './fb-confirm-dialog.html',
  styleUrl: './fb-confirm-dialog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbConfirmDialog {
  protected readonly data = inject<ConfirmDialogData>(DIALOG_DATA);
  private readonly dialogRef = inject(DialogRef<boolean>);

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
