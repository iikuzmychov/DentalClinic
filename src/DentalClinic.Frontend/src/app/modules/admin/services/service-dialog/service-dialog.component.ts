import { NgIf } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { 
    AddServiceRequest, 
    UpdateServiceRequest, 
    ListServicesResponseItem 
} from 'app/api/models';

export interface ServiceDialogData {
    mode: 'add' | 'edit';
    service?: ListServicesResponseItem;
}

@Component({
    selector: 'app-service-dialog',
    standalone: true,
    imports: [
        NgIf,
        ReactiveFormsModule,
        MatButtonModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
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
        
        ::ng-deep .mat-mdc-form-field-text-suffix {
            padding-left: 8px;
            color: rgba(0, 0, 0, 0.6);
        }
    `],
    template: `
        <div class="flex flex-col w-full h-full">
            
            <!-- Header -->
            <div class="flex flex-0 items-center justify-between h-16 px-6 bg-primary text-on-primary">
                <div class="text-lg font-medium">
                    {{ data.mode === 'add' ? 'Додати послугу' : 'Редагувати послугу' }}
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
            <form
                class="flex flex-col flex-1 px-6 py-6"
                [formGroup]="serviceForm"
                (ngSubmit)="save()">

                <!-- Name field -->
                <mat-form-field appearance="outline" class="w-full mb-4">
                    <mat-label>Назва послуги</mat-label>
                    <input
                        matInput
                        formControlName="name"
                        placeholder="Введіть назву послуги"
                        required>
                    <mat-error *ngIf="serviceForm.get('name')?.hasError('required')">
                        Назва послуги є обов'язковою
                    </mat-error>
                </mat-form-field>

                <!-- Price field -->
                <mat-form-field appearance="outline" class="w-full mb-6">
                    <mat-label>Ціна</mat-label>
                    <input
                        matInput
                        type="number"
                        formControlName="price"
                        placeholder="Введіть ціну"
                        min="0"
                        step="0.01"
                        required>
                    <span matTextSuffix>грн</span>
                    <mat-error *ngIf="serviceForm.get('price')?.hasError('required')">
                        Ціна є обов'язковою
                    </mat-error>
                    <mat-error *ngIf="serviceForm.get('price')?.hasError('min')">
                        Ціна повинна бути більше 0
                    </mat-error>
                </mat-form-field>

                <!-- Actions -->
                <div class="flex items-center justify-end space-x-3 pt-4 border-t mt-auto">
                    <button
                        mat-stroked-button
                        type="button"
                        (click)="cancel()"
                        class="px-6">
                        Скасувати
                    </button>
                    <button
                        mat-flat-button
                        type="submit"
                        [color]="'primary'"
                        [disabled]="serviceForm.invalid"
                        class="px-8">
                        {{ data.mode === 'add' ? 'Додати' : 'Зберегти' }}
                    </button>
                </div>
            </form>
        </div>
    `
})
export class ServiceDialogComponent implements OnInit {
    serviceForm: FormGroup;

    constructor(
        private _formBuilder: FormBuilder,
        private _dialogRef: MatDialogRef<ServiceDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: ServiceDialogData
    ) {
        this.serviceForm = this._formBuilder.group({
            name: ['', [Validators.required]],
            price: ['', [Validators.required, Validators.min(0.01)]]
        });
    }

    ngOnInit(): void {
        // If editing, populate form with existing data
        if (this.data.mode === 'edit' && this.data.service) {
            this.serviceForm.patchValue({
                name: this.data.service.name,
                price: this.data.service.price
            });
        }
    }

    save(): void {
        if (this.serviceForm.valid) {
            const formValue = this.serviceForm.value;
            const serviceData: AddServiceRequest | UpdateServiceRequest = {
                name: formValue.name,
                price: Number(formValue.price)
            };
            this._dialogRef.close(serviceData);
        }
    }

    cancel(): void {
        this._dialogRef.close();
    }
} 