import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

export interface DeleteConfirmationData {
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
}

@Component({
    selector: 'app-delete-confirmation-dialog',
    standalone: true,
    imports: [
        MatButtonModule,
        MatDialogModule,
        MatIconModule
    ],
    styles: [`
        :host {
            display: block;
            width: 100%;
        }
        
        ::ng-deep .mat-mdc-dialog-container .mdc-dialog__surface {
            padding: 0 !important;
        }
    `],
    template: `
        <div class="flex flex-col w-full h-full">
            
            <!-- Header -->
            <div class="flex flex-0 items-center justify-between h-16 px-6 bg-warn text-on-warn">
                <div class="text-lg font-medium">
                    {{ data.title }}
                </div>
                <button
                    mat-icon-button
                    (click)="cancel()"
                    [tabIndex]="-1">
                    <mat-icon
                        class="text-current"
                        [svgIcon]="'heroicons_outline:x-mark'"></mat-icon>
                </button>
            </div>

            <!-- Content -->
            <div class="flex flex-col flex-auto px-6 py-6">
                <div class="flex items-center mb-6">
                    <mat-icon 
                        [svgIcon]="'heroicons_outline:exclamation-triangle'" 
                        class="text-warn mr-3 w-8 h-8">
                    </mat-icon>
                    <p class="text-base">
                        {{ data.message }}
                    </p>
                </div>

                <!-- Actions -->
                <div class="flex items-center justify-end space-x-3">
                    <button
                        mat-stroked-button
                        type="button"
                        (click)="cancel()">
                        {{ data.cancelText || 'Скасувати' }}
                    </button>
                    <button
                        mat-flat-button
                        [color]="'warn'"
                        (click)="confirm()">
                        {{ data.confirmText || 'Видалити' }}
                    </button>
                </div>
            </div>
        </div>
    `
})
export class DeleteConfirmationDialogComponent {
    constructor(
        private _dialogRef: MatDialogRef<DeleteConfirmationDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: DeleteConfirmationData
    ) {}

    confirm(): void {
        this._dialogRef.close(true);
    }

    cancel(): void {
        this._dialogRef.close(false);
    }
} 