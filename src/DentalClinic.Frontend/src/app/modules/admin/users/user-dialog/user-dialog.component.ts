import { NgIf, NgFor } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { 
    ListUsersResponseItem, 
    AddUserRequest, 
    UpdateUserRequest,
    Role,
    RoleObject
} from 'app/api/models';

export interface UserDialogData {
    mode: 'add' | 'edit' | 'view';
    user?: ListUsersResponseItem;
}

// Кастомный валидатор для пароля
function passwordValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) {
        return null; // required валидатор обработает пустое значение
    }
    
    const passwordRegex = /^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-])[A-Za-z0-9#?!@$%^&*-]+$/;
    
    if (!passwordRegex.test(value)) {
        return { invalidPassword: true };
    }
    
    return null;
}

@Component({
    selector: 'app-user-dialog',
    standalone: true,
    imports: [
        NgIf,
        NgFor,
        ReactiveFormsModule,
        MatButtonModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        MatSelectModule
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
        
        .readonly-field .mat-mdc-select {
            color: #757575;
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
                        {{ currentMode === 'add' ? 'Додати користувача' : currentMode === 'edit' ? 'Редагування користувача' : 'Перегляд користувача' }}
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
                [formGroup]="userForm"
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
                        <mat-error *ngIf="userForm.get('lastName')?.hasError('required')">
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
                        <mat-error *ngIf="userForm.get('firstName')?.hasError('required')">
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
                                placeholder="example@email.com"
                                required>
                            <mat-error *ngIf="userForm.get('email')?.hasError('required')">
                                Email є обов'язковим
                            </mat-error>
                            <mat-error *ngIf="userForm.get('email')?.hasError('email')">
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

                    <!-- Role -->
                    <mat-form-field appearance="outline" class="full-width" [class.readonly-field]="isReadonly">
                        <mat-label>Роль</mat-label>
                        <mat-select 
                            formControlName="role"
                            [disabled]="isReadonly || currentMode === 'edit'"
                            required>
                            <mat-option *ngFor="let roleOption of roleOptions" [value]="roleOption.value">
                                {{ roleOption.label }}
                            </mat-option>
                        </mat-select>
                        <mat-error *ngIf="userForm.get('role')?.hasError('required')">
                            Роль є обов'язковою
                        </mat-error>
                    </mat-form-field>

                    <!-- Password - only in add mode -->
                    <mat-form-field 
                        *ngIf="currentMode === 'add'" 
                        appearance="outline" 
                        class="full-width">
                        <mat-label>Пароль</mat-label>
                        <input 
                            matInput 
                            type="password"
                            formControlName="password"
                            placeholder="Введіть пароль"
                            #passwordField
                            required>
                        <button 
                            mat-icon-button 
                            matSuffix 
                            type="button"
                            (click)="passwordField.type === 'password' ? passwordField.type = 'text' : passwordField.type = 'password'">
                            <mat-icon
                                class="icon-size-5"
                                *ngIf="passwordField.type === 'password'"
                                [svgIcon]="'heroicons_solid:eye'"></mat-icon>
                            <mat-icon
                                class="icon-size-5"
                                *ngIf="passwordField.type === 'text'"
                                [svgIcon]="'heroicons_solid:eye-slash'"></mat-icon>
                        </button>
                        <mat-error *ngIf="userForm.get('password')?.hasError('required')">
                            Пароль є обов'язковим
                        </mat-error>
                        <mat-error *ngIf="userForm.get('password')?.hasError('minlength')">
                            Пароль повинен містити щонайменше 8 символів
                        </mat-error>
                        <mat-error *ngIf="userForm.get('password')?.hasError('invalidPassword')">
                            Пароль повинен містити: велику літеру, малу літеру, цифру та спеціальний символ
                        </mat-error>
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
                        [disabled]="userForm.invalid"
                        class="px-8">
                        {{ currentMode === 'add' ? 'Створити' : 'Зберегти' }}
                    </button>
                    <button
                        *ngIf="currentMode === 'view'"
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
export class UserDialogComponent implements OnInit {
    userForm: FormGroup;
    currentMode: 'add' | 'edit' | 'view';
    originalData: any = null;
    isReadonly: boolean = false;
    
    // Локализованные роли для селекта
    roleOptions = [
        { value: RoleObject.Admin, label: 'Адміністратор' },
        { value: RoleObject.Dentist, label: 'Лікар' },
        { value: RoleObject.Receptionist, label: 'Менеджер' }
    ];

    constructor(
        private _formBuilder: FormBuilder,
        private _dialogRef: MatDialogRef<UserDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: UserDialogData
    ) {
        this.currentMode = this.data.mode;
        this.isReadonly = this.currentMode === 'view';
        
        const formConfig: any = {
            firstName: ['', [Validators.required]],
            lastName: ['', [Validators.required]],
            surname: [''],
            email: ['', [Validators.required, Validators.email]],
            phoneNumber: [''],
            role: ['', [Validators.required]]
        };

        // Добавляем пароль только в режиме добавления
        if (this.currentMode === 'add') {
            formConfig.password = ['', [Validators.required, Validators.minLength(8), passwordValidator]];
        }

        this.userForm = this._formBuilder.group(formConfig);
    }

    ngOnInit(): void {
        // If editing or viewing, populate form with existing data
        if ((this.currentMode === 'edit' || this.currentMode === 'view') && this.data.user) {
            const userData = {
                firstName: this.data.user.firstName,
                lastName: this.data.user.lastName,
                surname: this.data.user.surname,
                email: this.data.user.email,
                phoneNumber: this.data.user.phoneNumber,
                role: this.data.user.role
            };
            
            this.userForm.patchValue(userData);
            // Store original data for potential cancellation
            this.originalData = { ...userData };
        }

        // Set readonly state
        this.isReadonly = this.currentMode === 'view';

        // Disable email and role fields in edit mode
        if (this.currentMode === 'edit') {
            this.userForm.get('email')?.disable();
            this.userForm.get('role')?.disable();
        }
    }

    switchToEditMode(): void {
        this.currentMode = 'edit';
        this.isReadonly = false;
        
        // Программно отключаем поля, которые нельзя редактировать
        this.userForm.get('email')?.disable();
        this.userForm.get('role')?.disable();
    }

    save(): void {
        if (this.currentMode === 'view') {
            this.switchToEditMode();
            return;
        }

        if (this.userForm.valid) {
            const formValue = this.userForm.value;
            
            if (this.currentMode === 'add') {
                const userData: AddUserRequest = {
                    firstName: formValue.firstName,
                    lastName: formValue.lastName,
                    surname: formValue.surname || null,
                    email: formValue.email,
                    phoneNumber: formValue.phoneNumber || null,
                    role: formValue.role as Role,
                    password: formValue.password
                };
                this._dialogRef.close({ action: 'add', data: userData });
            } else {
                const userData: UpdateUserRequest = {
                    firstName: formValue.firstName,
                    lastName: formValue.lastName,
                    surname: formValue.surname || null,
                    phoneNumber: formValue.phoneNumber || null
                };
                this._dialogRef.close({ action: 'update', data: userData });
            }
        }
    }

    cancel(): void {
        if (this.currentMode === 'edit' && this.originalData) {
            // Return to view mode with original data
            this.currentMode = 'view';
            this.isReadonly = true;
            this.userForm.patchValue(this.originalData);
            
            // Включаем поля обратно для view режима
            this.userForm.get('email')?.enable();
            this.userForm.get('role')?.enable();
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