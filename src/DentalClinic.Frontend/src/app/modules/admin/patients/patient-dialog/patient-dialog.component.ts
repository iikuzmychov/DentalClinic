import { NgIf, AsyncPipe } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { RoleService } from 'app/core/auth/role.service';
import { Observable } from 'rxjs';
import { 
    AddPatientRequest, 
    UpdatePatientRequest, 
    GetPatientResponse 
} from 'app/api/models';

export interface PatientDialogData {
    mode: 'add' | 'edit' | 'view';
    patient?: GetPatientResponse;
}

@Component({
    selector: 'app-patient-dialog',
    standalone: true,
    imports: [
        NgIf,
        AsyncPipe,
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
        
        .form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr 1fr;
            gap: 16px;
        }
        
        .form-grid .full-width {
            grid-column: 1 / -1;
        }
        
        .form-grid .two-columns {
            grid-column: span 2;
        }
        
        .contacts-row {
            grid-column: 1 / -1;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 16px;
        }
        
        /* Readonly field styling - just disable interaction, keep white background */
        .readonly-field {
            pointer-events: none;
        }
        
        .readonly-field input,
        .readonly-field textarea {
            color: #757575;
            cursor: default;
        }
        
        @media (max-width: 900px) {
            .form-grid {
                grid-template-columns: 1fr 1fr;
            }
            
            .contacts-row {
                grid-template-columns: 1fr 1fr;
            }
        }
        
        @media (max-width: 600px) {
            .form-grid {
                grid-template-columns: 1fr;
            }
            
            .contacts-row {
                grid-template-columns: 1fr;
            }
        }
    `],
    template: `
        <div class="flex flex-col w-full h-full">
            
            <!-- Header -->
            <div class="flex flex-0 items-center justify-between h-16 px-6 bg-primary text-on-primary">
                <div class="flex items-center space-x-2">
                    <mat-icon 
                        *ngIf="currentMode === 'view'"
                        class="text-current"
                        [svgIcon]="'heroicons_outline:eye'"></mat-icon>
                    <mat-icon 
                        *ngIf="currentMode === 'edit'"
                        class="text-current"
                        [svgIcon]="'heroicons_outline:pencil'"></mat-icon>
                    <mat-icon 
                        *ngIf="currentMode === 'add'"
                        class="text-current"
                        [svgIcon]="'heroicons_outline:plus'"></mat-icon>
                    <div class="text-lg font-medium">
                        {{ currentMode === 'add' ? 'Додати пацієнта' : currentMode === 'edit' ? 'Редагування пацієнта' : 'Перегляд пацієнта' }}
                    </div>
                </div>
                <button
                    mat-icon-button
                    (click)="close()">
                    <mat-icon
                        class="text-current"
                        [svgIcon]="'heroicons_outline:x-mark'"></mat-icon>
                </button>
            </div>

            <!-- Content -->
            <form
                class="flex flex-col flex-1 px-6 py-6"
                [formGroup]="patientForm"
                (ngSubmit)="save()">

                <div class="form-grid">
                    <!-- Last Name -->
                    <mat-form-field appearance="outline" [class.readonly-field]="isReadonly">
                        <mat-label>Прізвище</mat-label>
                        <input
                            matInput
                            formControlName="lastName"
                            placeholder="Введіть прізвище"
                            [readonly]="isReadonly"
                            required>
                        <mat-error *ngIf="patientForm.get('lastName')?.hasError('required')">
                            Прізвище є обов'язковим
                        </mat-error>
                    </mat-form-field>

                    <!-- First Name -->
                    <mat-form-field appearance="outline" [class.readonly-field]="isReadonly">
                        <mat-label>Ім'я</mat-label>
                        <input
                            matInput
                            formControlName="firstName"
                            placeholder="Введіть ім'я"
                            [readonly]="isReadonly"
                            required>
                        <mat-error *ngIf="patientForm.get('firstName')?.hasError('required')">
                            Ім'я є обов'язковим
                        </mat-error>
                    </mat-form-field>

                    <!-- Surname (Middle name) -->
                    <mat-form-field appearance="outline" [class.readonly-field]="isReadonly">
                        <mat-label>По батькові</mat-label>
                        <input
                            matInput
                            formControlName="surname"
                            [readonly]="isReadonly"
                            placeholder="Введіть по батькові">
                    </mat-form-field>

                    <!-- Contacts Row -->
                    <div class="contacts-row">
                        <!-- Email -->
                        <mat-form-field appearance="outline" [class.readonly-field]="isReadonly">
                            <mat-label>Email</mat-label>
                            <input
                                matInput
                                type="email"
                                formControlName="email"
                                [readonly]="isReadonly"
                                placeholder="example@email.com">
                            <mat-error *ngIf="patientForm.get('email')?.hasError('email')">
                                Введіть правильний email
                            </mat-error>
                        </mat-form-field>

                        <!-- Phone Number -->
                        <mat-form-field appearance="outline" [class.readonly-field]="isReadonly">
                            <mat-label>Номер телефону</mat-label>
                            <input
                                matInput
                                type="tel"
                                formControlName="phoneNumber"
                                [readonly]="isReadonly"
                                placeholder="+380671234567">
                        </mat-form-field>
                    </div>

                    <!-- Notes -->
                    <mat-form-field appearance="outline" class="full-width" [class.readonly-field]="isReadonly">
                        <mat-label>Примітки</mat-label>
                        <textarea
                            matInput
                            formControlName="notes"
                            [readonly]="isReadonly"
                            placeholder="Додаткова інформація про пацієнта"
                            rows="4">
                        </textarea>
                    </mat-form-field>
                </div>

                <!-- Actions -->
                <div class="flex items-center justify-end space-x-3 pt-6 border-t mt-6">
                    <button
                        mat-stroked-button
                        type="button"
                        (click)="cancel()"
                        class="px-6">
                        {{ currentMode === 'view' ? 'Закрити' : 'Скасувати' }}
                    </button>
                    <button
                        *ngIf="currentMode !== 'view'"
                        mat-flat-button
                        type="submit"
                        [color]="'primary'"
                        [disabled]="patientForm.invalid"
                        class="px-8">
                        {{ currentMode === 'add' ? 'Додати' : 'Зберегти' }}
                    </button>
                    <button
                        *ngIf="currentMode === 'view' && (canEdit$ | async)"
                        mat-flat-button
                        type="button"
                        [color]="'primary'"
                        (click)="switchToEditMode()"
                        class="px-8">
                        Редагувати
                    </button>
                </div>
            </form>
        </div>
    `
})
export class PatientDialogComponent implements OnInit {
    patientForm: FormGroup;
    currentMode: 'add' | 'edit' | 'view';
    originalData: any = null; // Store original data for cancellation
    isReadonly: boolean = false;
    canEdit$: Observable<boolean>;

    constructor(
        private _formBuilder: FormBuilder,
        private _dialogRef: MatDialogRef<PatientDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: PatientDialogData,
        private _roleService: RoleService
    ) {
        this.currentMode = this.data.mode;
        this.isReadonly = this.currentMode === 'view';
        this.canEdit$ = this._roleService.canEditPatients();
        this.patientForm = this._formBuilder.group({
            firstName: ['', [Validators.required]],
            lastName: ['', [Validators.required]],
            surname: [''],
            email: ['', [Validators.email]],
            phoneNumber: [''],
            notes: ['']
        });
    }

    ngOnInit(): void {
        // If editing or viewing, populate form with existing data
        if ((this.currentMode === 'edit' || this.currentMode === 'view') && this.data.patient) {
            const patientData = {
                firstName: this.data.patient.firstName,
                lastName: this.data.patient.lastName,
                surname: this.data.patient.surname,
                email: this.data.patient.email,
                phoneNumber: this.data.patient.phoneNumber,
                notes: this.data.patient.notes
            };
            
            this.patientForm.patchValue(patientData);
            // Store original data for potential cancellation
            this.originalData = { ...patientData };
        }

        // Ensure readonly state is correctly set
        this.isReadonly = this.currentMode === 'view';
    }

    switchToEditMode(): void {
        this.currentMode = 'edit';
        this.isReadonly = false;
    }

    save(): void {
        if (this.currentMode === 'view') {
            this.switchToEditMode();
            return;
        }

        if (this.patientForm.valid) {
            const formValue = this.patientForm.value;
            const patientData: AddPatientRequest | UpdatePatientRequest = {
                firstName: formValue.firstName,
                lastName: formValue.lastName,
                surname: formValue.surname || null,
                email: formValue.email || null,
                phoneNumber: formValue.phoneNumber || null,
                notes: formValue.notes || null
            };
            this._dialogRef.close(patientData);
        }
    }

    cancel(): void {
        if (this.currentMode === 'edit' && this.originalData) {
            // Return to view mode with original data
            this.currentMode = 'view';
            this.isReadonly = true;
            this.patientForm.patchValue(this.originalData);
        } else {
            // Close dialog completely
            this._dialogRef.close();
        }
    }

    close(): void {
        // Always close dialog completely (for X button)
        this._dialogRef.close();
    }
} 