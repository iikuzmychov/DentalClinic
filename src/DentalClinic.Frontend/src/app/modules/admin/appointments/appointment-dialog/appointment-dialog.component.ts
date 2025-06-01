import { NgIf, NgFor, AsyncPipe } from '@angular/common';
import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { NgxMaskDirective } from 'ngx-mask';
import { from, forkJoin, Observable } from 'rxjs';
import { finalize, map, startWith } from 'rxjs/operators';
import { ApiClientService } from 'app/core/api/api-client.service';
import { RoleService } from 'app/core/auth/role.service';
import { 
    AddAppointmentRequest,
    UpdateAppointmentRequest,
    CompleteAppointmentRequest,
    ListPatientsResponse,
    ListPatientsResponseItem,
    ListUsersResponse,
    ListUsersResponseItem,
    ListServicesResponse,
    ListServicesResponseItem
} from 'app/api/models';
import { AppointmentCompletionDialogComponent } from '../appointment-completion-dialog/appointment-completion-dialog.component';
import { DeleteConfirmationDialogComponent } from '../../services/delete-confirmation-dialog/delete-confirmation-dialog.component';

export interface AppointmentDialogData {
    mode: 'add' | 'edit' | 'view';
    appointment?: any;
}

// Custom validator for minimum duration
function minimumDurationValidator(control: AbstractControl): ValidationErrors | null {
    const hours = control.get('durationHours')?.value || 0;
    const minutes = control.get('durationMinutes')?.value || 0;
    
    if (hours === 0 && minutes === 0) {
        return { minimumDuration: true };
    }
    
    return null;
}

@Component({
    selector: 'app-appointment-dialog',
    standalone: true,
    imports: [
        NgIf,
        NgFor,
        AsyncPipe,
        ReactiveFormsModule,
        MatButtonModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatIconModule,
        MatSelectModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatProgressSpinnerModule,
        MatAutocompleteModule,
        ScrollingModule,
        NgxMaskDirective
    ],
    styles: [`
        :host {
            display: block;
            width: 100%;
        }
        
        ::ng-deep .mat-mdc-dialog-container .mdc-dialog__surface {
            padding: 0 !important;
            max-height: 90vh !important;
            overflow: hidden;
        }
        
        .loading-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(255, 255, 255, 0.8);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 1000;
        }

        .duration-fields {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 16px;
        }

        /* Readonly field styling */
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

        /* Scrollable content area */
        .dialog-content {
            max-height: calc(90vh - 64px); /* Total height minus header */
            overflow-y: auto;
        }

        /* Rotate icon animation */
        .rotate-180 {
            transform: rotate(180deg);
        }
    `],
    template: `
        <div class="flex flex-col w-full h-full">
            
            <!-- Loading overlay -->
            <div *ngIf="isLoadingData" class="loading-overlay">
                <mat-spinner diameter="40"></mat-spinner>
            </div>
            
            <!-- Header -->
            <div class="flex flex-0 items-center justify-between h-16 px-6 bg-primary text-on-primary">
                <div class="flex items-center space-x-2">
                    <mat-icon 
                        *ngIf="data.mode === 'view'"
                        class="text-current"
                        [svgIcon]="'heroicons_outline:eye'"></mat-icon>
                    <mat-icon 
                        *ngIf="data.mode === 'edit'"
                        class="text-current"
                        [svgIcon]="'heroicons_outline:pencil'"></mat-icon>
                    <mat-icon 
                        *ngIf="data.mode === 'add'"
                        class="text-current"
                        [svgIcon]="'heroicons_outline:plus'"></mat-icon>
                    <div class="text-lg font-medium">
                        {{ data.mode === 'add' ? 'Новий запис на прийом' : data.mode === 'edit' ? 'Редагування запису' : 'Перегляд запису' }}
                    </div>
                </div>
                <button
                    mat-icon-button
                    [disabled]="isLoading"
                    (click)="close()">
                    <mat-icon
                        class="text-current"
                        [svgIcon]="'heroicons_outline:x-mark'"></mat-icon>
                </button>
            </div>

            <!-- Scrollable Content -->
            <div class="dialog-content">
            <form
                    class="flex flex-col px-6 py-6"
                [formGroup]="appointmentForm"
                (ngSubmit)="save()">

                <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                    
                    <!-- Patient -->
                        <mat-form-field appearance="outline" class="w-full" [class.readonly-field]="isReadonly">
                        <mat-label>Пацієнт</mat-label>
                            <mat-select formControlName="patientId" [disabled]="data.mode === 'view'" required>
                            <mat-option *ngFor="let patient of patients" [value]="patient.id">
                                {{ getPatientName(patient) }}
                            </mat-option>
                        </mat-select>
                        <mat-error *ngIf="appointmentForm.get('patientId')?.hasError('required')">
                            Оберіть пацієнта
                        </mat-error>
                    </mat-form-field>

                    <!-- Dentist -->
                        <mat-form-field appearance="outline" class="w-full" [class.readonly-field]="isReadonly">
                        <mat-label>Лікар</mat-label>
                            <mat-select formControlName="dentistId" [disabled]="data.mode === 'view'" required>
                            <mat-option *ngFor="let dentist of dentists" [value]="dentist.id">
                                {{ getDentistName(dentist) }}
                            </mat-option>
                        </mat-select>
                        <mat-error *ngIf="appointmentForm.get('dentistId')?.hasError('required')">
                            Оберіть лікаря
                        </mat-error>
                    </mat-form-field>

                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                    
                    <!-- Date -->
                        <mat-form-field appearance="outline" class="w-full" [class.readonly-field]="isReadonly">
                        <mat-label>Дата</mat-label>
                        <input
                            matInput
                            [matDatepicker]="datePicker"
                                [min]="data.mode === 'add' ? minDate : null"
                                [readonly]="data.mode === 'view'"
                            formControlName="date"
                            placeholder="Оберіть дату"
                            required>
                            <mat-datepicker-toggle matIconSuffix [for]="datePicker" [disabled]="data.mode === 'view'"></mat-datepicker-toggle>
                            <mat-datepicker #datePicker [disabled]="data.mode === 'view'"></mat-datepicker>
                        <mat-error *ngIf="appointmentForm.get('date')?.hasError('required')">
                            Оберіть дату прийому
                        </mat-error>
                            <mat-error *ngIf="appointmentForm.get('date')?.hasError('matDatepickerMin')">
                                Неможливо обрати минулу дату
                        </mat-error>
                    </mat-form-field>

                    <!-- Time -->
                        <mat-form-field appearance="outline" class="w-full" [class.readonly-field]="isReadonly">
                        <mat-label>Час</mat-label>
                        <input
                            matInput
                            formControlName="time"
                                mask="00:00"
                                [readonly]="data.mode === 'view'"
                                placeholder="--:--"
                            required>
                        <mat-error *ngIf="appointmentForm.get('time')?.hasError('required')">
                            Вкажіть час прийому
                        </mat-error>
                    </mat-form-field>

                </div>

                    <!-- Duration Section -->
                    <div class="mb-6">
                        <div class="text-sm font-medium text-gray-700 mb-3">Тривалість прийому</div>
                        <div class="duration-fields">
                            <!-- Hours -->
                            <mat-form-field appearance="outline" [class.readonly-field]="isReadonly">
                                <mat-label>Години</mat-label>
                                <input
                                    matInput
                                    type="number"
                                    formControlName="durationHours"
                                    min="0"
                                    max="8"
                                    [readonly]="data.mode === 'view'"
                                    placeholder="1"
                                    required>
                                <mat-error *ngIf="appointmentForm.get('durationHours')?.hasError('required')">
                                    Вкажіть години
                                </mat-error>
                                <mat-error *ngIf="appointmentForm.get('durationHours')?.hasError('min')">
                                    Мінімум 0 годин
                                </mat-error>
                                <mat-error *ngIf="appointmentForm.get('durationHours')?.hasError('max')">
                                    Максимум 8 годин
                    </mat-error>
                </mat-form-field>

                            <!-- Minutes -->
                            <mat-form-field appearance="outline" [class.readonly-field]="isReadonly">
                                <mat-label>Хвилини</mat-label>
                                <mat-select formControlName="durationMinutes" [disabled]="data.mode === 'view'" required>
                                    <mat-option [value]="0">0</mat-option>
                                    <mat-option [value]="15">15</mat-option>
                                    <mat-option [value]="30">30</mat-option>
                                    <mat-option [value]="45">45</mat-option>
                                </mat-select>
                                <mat-error *ngIf="appointmentForm.get('durationMinutes')?.hasError('required')">
                                    Оберіть хвилини
                                </mat-error>
                            </mat-form-field>
                        </div>
                        
                        <!-- Duration validation error -->
                        <div *ngIf="appointmentForm.hasError('minimumDuration')" class="text-red-500 text-sm mt-2">
                            Мінімальна тривалість прийому - 15 хвилин
                        </div>
                    </div>

                    <!-- Status (only for edit/view modes) -->
                    <div *ngIf="data.mode !== 'add'" class="mb-6">
                        <mat-form-field appearance="outline" class="w-full" [class.readonly-field]="true">
                            <mat-label>Статус</mat-label>
                            <mat-select [value]="currentAppointmentStatus" disabled>
                                <mat-option *ngFor="let statusOption of statusOptions" [value]="statusOption.value">
                                    {{ statusOption.label }}
                                </mat-option>
                            </mat-select>
                        </mat-form-field>
                    </div>

                    <!-- Provided Services (only for Completed/Paid appointments) -->
                    <div *ngIf="(currentStatus === 'Completed' || currentStatus === 'Paid') && getProvidedServices()?.length > 0" class="mb-6">
                        
                        <!-- Services Block -->
                        <div class="bg-gray-50 rounded-lg border overflow-hidden">
                            <!-- Clickable Header -->
                            <div 
                                class="flex items-center justify-between cursor-pointer p-3 hover:bg-gray-100 transition-colors duration-200"
                                (click)="toggleServicesExpansion()">
                                <span class="text-sm font-medium text-gray-700">Надані послуги</span>
                                <div class="flex items-center space-x-2">
                                    <span class="text-sm text-gray-600">{{ getProvidedServices()?.length || 0 }} послуг</span>
                                    <mat-icon 
                                        class="text-gray-500 transition-transform duration-200"
                                        [class.rotate-180]="isServicesExpanded"
                                        [svgIcon]="'heroicons_outline:chevron-down'">
                                    </mat-icon>
                                </div>
                            </div>

                            <!-- Expandable Services List -->
                            <div *ngIf="isServicesExpanded" class="border-t border-gray-200">
                                <div class="p-4 space-y-3">
                                    <div *ngFor="let service of getProvidedServices(); trackBy: trackByServiceId; last as isLast" 
                                         class="flex justify-between items-center py-3 px-3 bg-white rounded-md shadow-sm border"
                                         [class.mb-0]="isLast">
                                        <span class="font-medium text-gray-900">{{ service.name || 'Невідома послуга' }}</span>
                                        <span *ngIf="service.price" class="text-green-600 font-semibold">{{ service.price }} ₴</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Always Visible Summary -->
                        <div class="bg-white rounded-lg p-4 border shadow-sm mt-3">
                            <div class="flex justify-between items-center">
                                <span class="text-lg font-semibold text-gray-900">Загальна сума:</span>
                                <span class="text-xl font-bold text-green-600">{{ getTotalServicesPrice() }} ₴</span>
                            </div>
                        </div>
                    </div>

                    <!-- Actions - moved inside scrollable area -->
                    <div class="flex items-center justify-end space-x-3 pt-6 border-t mt-6">
                        
                        <!-- View Mode Actions -->
                        <div *ngIf="data.mode === 'view'" class="flex items-center justify-between w-full">
                            <!-- Status Actions -->
                            <div class="flex items-center space-x-2">
                                <!-- Complete Button - only for Scheduled -->
                                <button
                                    *ngIf="canComplete() | async"
                                    mat-flat-button
                                    type="button"
                                    style="background-color: #4caf50; color: white;"
                                    [disabled]="isLoading"
                                    (click)="completeAppointment()">
                                    <mat-icon [svgIcon]="'heroicons_outline:check'"></mat-icon>
                                    <span class="ml-2">Завершити</span>
                                </button>
                                
                                <!-- Pay Button - only for Completed -->
                                <button
                                    *ngIf="canPay() | async"
                                    mat-flat-button
                                    color="accent"
                                    type="button"
                                    [disabled]="isLoading"
                                    (click)="payAppointment()">
                                    <mat-icon [svgIcon]="'heroicons_outline:currency-dollar'"></mat-icon>
                                    <span class="ml-2">Сплатити</span>
                                </button>
                                
                                <!-- Cancel Button - only for Scheduled -->
                                <button
                                    *ngIf="canCancel() | async"
                                    mat-flat-button
                                    color="warn"
                                    type="button"
                                    [disabled]="isLoading"
                                    (click)="cancelAppointment()">
                                    <mat-icon [svgIcon]="'heroicons_outline:x-mark'"></mat-icon>
                                    <span class="ml-2">Скасувати</span>
                                </button>
                                
                                <!-- Delete Button - only for Cancelled -->
                                <button
                                    *ngIf="canDelete() | async"
                                    mat-flat-button
                                    color="warn"
                                    type="button"
                                    [disabled]="isLoading"
                                    (click)="deleteAppointment()">
                                    <mat-icon [svgIcon]="'heroicons_outline:trash'"></mat-icon>
                                    <span class="ml-2">Видалити</span>
                                </button>
                            </div>
                            
                            <!-- Close and Edit -->
                            <div class="flex items-center space-x-3">
                                <button
                                    mat-stroked-button
                                    type="button"
                                    [disabled]="isLoading"
                                    (click)="cancel()"
                                    class="px-6">
                                    Закрити
                                </button>
                                <!-- Edit button - only for Scheduled -->
                                <button
                                    *ngIf="canEdit() | async"
                                    mat-flat-button
                                    type="button"
                                    [color]="'primary'"
                                    [disabled]="isLoading"
                                    (click)="switchToEditMode()"
                                    class="px-8">
                                    Редагувати
                                </button>
                            </div>
                        </div>
                        
                        <!-- Edit/Add Mode Actions -->
                        <div *ngIf="data.mode !== 'view'" class="flex items-center space-x-3">
                    <button
                        mat-stroked-button
                        type="button"
                                [disabled]="isLoading"
                        (click)="cancel()"
                        class="px-6">
                        Скасувати
                    </button>
                    <button
                        mat-flat-button
                                type="button"
                        [color]="'primary'"
                                [disabled]="appointmentForm.invalid || isLoading || isLoadingData"
                                (click)="save()"
                        class="px-8">
                                <mat-spinner *ngIf="isLoading" diameter="20" class="mr-2"></mat-spinner>
                        {{ data.mode === 'add' ? 'Створити' : 'Зберегти' }}
                    </button>
                        </div>
                </div>
            </form>
            </div>
        </div>
    `
})
export class AppointmentDialogComponent implements OnInit {
    appointmentForm: FormGroup;
    patients: ListPatientsResponseItem[] = [];
    dentists: ListUsersResponseItem[] = [];
    isLoading: boolean = false;
    isLoadingData: boolean = false;
    isReadonly: boolean = false;
    minDate: Date = new Date(); // Today's date as minimum
    
    // Отдельное поле для отображения статуса (не в форме)
    currentAppointmentStatus: string = 'Scheduled';

    // Состояние сворачивания списка услуг
    isServicesExpanded: boolean = false;

    // Локализированные статусы для отображения
    statusOptions = [
        { value: 'Scheduled', label: 'Запланований' },
        { value: 'Completed', label: 'Завершений' },
        { value: 'Cancelled', label: 'Скасований' },
        { value: 'Paid', label: 'Сплачений' }
    ];

    constructor(
        private _formBuilder: FormBuilder,
        private _dialogRef: MatDialogRef<AppointmentDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: AppointmentDialogData,
        private apiClient: ApiClientService,
        private dialog: MatDialog,
        private changeDetectorRef: ChangeDetectorRef,
        private roleService: RoleService
    ) {
        // Initially set readonly based on mode only (status will be checked later)
        this.isReadonly = this.data.mode === 'view';
        
        this.appointmentForm = this._formBuilder.group({
            patientId: ['', [Validators.required]],
            dentistId: ['', [Validators.required]],
            date: ['', [Validators.required]],
            time: ['', [Validators.required]],
            durationHours: [1, [Validators.required, Validators.min(0), Validators.max(8)]],
            durationMinutes: [0, [Validators.required]]
        }, { validators: minimumDurationValidator });
    }

    ngOnInit(): void {
        this.loadData();
    }

    /**
     * Load patients and dentists data
     */
    loadData(): void {
        this.isLoadingData = true;
        
        if (this.data.appointment && (this.data.mode === 'edit' || this.data.mode === 'view')) {
            // First load full appointment details, then load specific patient/dentist and general lists
            this.loadFullAppointmentDetails().then(() => {
                this.loadSpecificPatientAndDentist().then(() => {
                    this.loadGeneralLists();
                });
            });
        } else {
            // For add mode, just load general lists
            this.loadGeneralLists();
        }
    }

    /**
     * Load full appointment details including services
     */
    private async loadFullAppointmentDetails(): Promise<void> {
        if (!this.data.appointment?.id) return;

        try {
            const fullAppointment = await this.apiClient.client.api.appointments.byId(this.data.appointment.id).get();
            
            // Update appointment data with full details
            this.data.appointment = { ...this.data.appointment, ...fullAppointment };
            
            // Populate form with updated data
            this.populateForm();
            
            // Force change detection
            this.changeDetectorRef.detectChanges();
        } catch (error) {
            console.error('Error loading full appointment details:', error);
        }
    }

    /**
     * Load specific patient and dentist from appointment data
     */
    private async loadSpecificPatientAndDentist(): Promise<void> {
        const appointment = this.data.appointment;
        if (!appointment) return;

        const requests: Promise<any>[] = [];

        // Load specific patient
        if (appointment.patient?.id) {
            const patientRequest = this.apiClient.client.api.patients.byId(appointment.patient.id).get()
                .then(patient => {
                    this.patients = [patient];
                })
                .catch(error => console.error('Error loading patient:', error));
            requests.push(patientRequest);
        }

        // Load specific dentist
        if (appointment.dentist?.id) {
            const dentistRequest = this.apiClient.client.api.users.byId(appointment.dentist.id).get()
                .then(dentist => {
                    this.dentists = [dentist];
                })
                .catch(error => console.error('Error loading dentist:', error));
            requests.push(dentistRequest);
        }

        await Promise.all(requests);
        
        // Now populate form with the loaded data
        this.populateForm();
    }

    /**
     * Load general lists of patients and dentists
     */
    private loadGeneralLists(): void {
        const patientsRequest = from(this.apiClient.client.api.patients.get({
            queryParameters: {
                pageIndex: 0,
                pageSize: 20
            }
        }));

        const dentistsRequest = from(this.apiClient.client.api.users.get({
            queryParameters: {
                role: 'Dentist',
                pageIndex: 0,
                pageSize: 20
            }
        }));

        forkJoin({
            patients: patientsRequest,
            dentists: dentistsRequest
        }).pipe(
            finalize(() => {
                this.isLoadingData = false;
                this.changeDetectorRef.detectChanges();
            })
        ).subscribe({
            next: ({ patients, dentists }) => {
                // Merge with existing data, replacing if IDs match
                this.mergePatientsList(patients?.items || []);
                this.mergeDentistsList(dentists?.items || []);
                this.changeDetectorRef.detectChanges();
            },
            error: (error) => {
                console.error('Error loading general lists:', error);
                this.isLoadingData = false;
                this.changeDetectorRef.detectChanges();
            }
        });
    }

    /**
     * Merge patients list, replacing existing if IDs match
     */
    private mergePatientsList(newPatients: ListPatientsResponseItem[]): void {
        for (const newPatient of newPatients) {
            const existingIndex = this.patients.findIndex(p => p.id === newPatient.id);
            if (existingIndex >= 0) {
                // Replace existing with fresh data
                this.patients[existingIndex] = newPatient;
            } else {
                // Add new patient
                this.patients.push(newPatient);
            }
        }
    }

    /**
     * Merge dentists list, replacing existing if IDs match
     */
    private mergeDentistsList(newDentists: ListUsersResponseItem[]): void {
        for (const newDentist of newDentists) {
            const existingIndex = this.dentists.findIndex(d => d.id === newDentist.id);
            if (existingIndex >= 0) {
                // Replace existing with fresh data
                this.dentists[existingIndex] = newDentist;
            } else {
                // Add new dentist
                this.dentists.push(newDentist);
            }
        }
    }

    /**
     * Get patient display name
     */
    getPatientName(patient: ListPatientsResponseItem): string {
        const parts = [patient.lastName, patient.firstName, patient.surname].filter(Boolean);
        return parts.join(' ') || 'Невідомий пацієнт';
    }

    /**
     * Get dentist display name
     */
    getDentistName(dentist: ListUsersResponseItem): string {
        const parts = [dentist.lastName, dentist.firstName, dentist.surname].filter(Boolean);
        return parts.join(' ') || 'Невідомий лікар';
    }

    /**
     * Populate form with appointment data
     */
    populateForm(): void {
        if (!this.data.appointment) return;
        
        const appointment = this.data.appointment;
        
        // Parse start time
        const startTime = new Date(appointment.startTime);
        const timeString = `${startTime.getHours().toString().padStart(2, '0')}:${startTime.getMinutes().toString().padStart(2, '0')}`;
        
        // Parse duration
        const durationParts = appointment.duration.split(':');
        const durationHours = parseInt(durationParts[0], 10) || 0;
        const durationMinutes = parseInt(durationParts[1], 10) || 0;
        
        // Set status separately (not in form)
        this.currentAppointmentStatus = appointment.status || 'Scheduled';
        
        this.appointmentForm.patchValue({
            patientId: appointment.patient?.id,
            dentistId: appointment.dentist?.id,
            date: startTime,
            time: timeString,
            durationHours: durationHours,
            durationMinutes: durationMinutes
        });
        
        // Force form validation update
        this.appointmentForm.updateValueAndValidity();
        
        // Force change detection for UI updates
        this.changeDetectorRef.detectChanges();
    }

    save(): void {
        if (this.appointmentForm.valid && !this.isLoading && !this.isLoadingData) {
            this.isLoading = true;
            
            const formValue = this.appointmentForm.value;
            
            // Parse time - handle both "HH:MM" and "HHMM" formats
            let hours: number, minutes: number;
            
            const timeStr = formValue.time.toString();
            if (timeStr.includes(':')) {
                // Format: "HH:MM"
                [hours, minutes] = timeStr.split(':').map(Number);
            } else {
                // Format: "HHMM" - pad with zeros if needed
                const paddedTime = timeStr.padStart(4, '0');
                hours = parseInt(paddedTime.substring(0, 2), 10);
                minutes = parseInt(paddedTime.substring(2, 4), 10);
            }
            
            // Create date safely - handle Luxon DateTime
            let startTime: Date;
            try {
                if (formValue.date && typeof formValue.date.toJSDate === 'function') {
                    // Luxon DateTime object
                    const jsDate = formValue.date.toJSDate();
                    startTime = new Date(jsDate.getFullYear(), jsDate.getMonth(), jsDate.getDate(), hours, minutes, 0, 0);
                } else if (formValue.date instanceof Date) {
                    // Regular Date object
                    startTime = new Date(formValue.date.getFullYear(), formValue.date.getMonth(), formValue.date.getDate(), hours, minutes, 0, 0);
                } else {
                    // String date
                    const dateObj = new Date(formValue.date);
                    startTime = new Date(dateObj.getFullYear(), dateObj.getMonth(), dateObj.getDate(), hours, minutes, 0, 0);
                }
                
                if (isNaN(startTime.getTime())) {
                    throw new Error('Invalid date created');
                }
            } catch (error) {
                console.error('Date creation error:', error);
                this.isLoading = false;
                return;
            }

            // Convert duration to time string format
            const durationHours = formValue.durationHours || 0;
            const durationMinutes = formValue.durationMinutes || 0;
            const durationString = `${durationHours.toString().padStart(2, '0')}:${durationMinutes.toString().padStart(2, '0')}`;

            const appointmentData: AddAppointmentRequest | UpdateAppointmentRequest = {
                patientId: formValue.patientId,
                dentistId: formValue.dentistId,
                startTime: startTime,
                duration: durationString
            };
            
            // Create or update appointment
            const apiCall = this.data.mode === 'add' 
                ? this.apiClient.client.api.appointments.post(appointmentData as AddAppointmentRequest)
                : this.apiClient.client.api.appointments.byId(this.data.appointment.id).put(appointmentData as UpdateAppointmentRequest);
            
            from(apiCall)
                .pipe(finalize(() => this.isLoading = false))
                .subscribe({
                    next: () => {
                        this._dialogRef.close({ action: this.data.mode === 'add' ? 'created' : 'updated' });
                    },
                    error: (error) => {
                        console.error('Error saving appointment:', error);
                    }
                });
        }
    }

    cancel(): void {
        if (this.data.mode === 'edit') {
            // Return to view mode without closing dialog or returning any result
            this.data.mode = 'view';
            this.isReadonly = true;
            // Restore original form data if needed
            this.populateForm();
        } else {
            // Close dialog completely for add mode or view mode
            this._dialogRef.close();
        }
    }

    close(): void {
        // Always close dialog completely (for X button)
        this._dialogRef.close();
    }

    /**
     * Complete appointment - open completion dialog
     */
    completeAppointment(): void {
        if (!this.data.appointment?.id) return;
        
        const dialogRef = this.dialog.open(AppointmentCompletionDialogComponent, {
            width: '700px',
            maxWidth: '95vw',
            data: { appointment: this.data.appointment }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && result.action === 'completed') {
                // Only close if actually completed
                this._dialogRef.close({ action: 'completed', services: result.services });
            }
            // If result is null/undefined (cancelled) - do nothing, keep dialog open
        });
    }

    /**
     * Pay appointment
     */
    payAppointment(): void {
        if (!this.data.appointment?.id) return;
        
        this.isLoading = true;
        
        from(this.apiClient.client.api.appointments.byId(this.data.appointment.id).pay.post())
            .pipe(finalize(() => this.isLoading = false))
            .subscribe({
                next: () => {
                    this._dialogRef.close({ action: 'paid' });
                },
                error: (error) => {
                    console.error('Error paying appointment:', error);
                }
            });
    }

    /**
     * Cancel appointment
     */
    cancelAppointment(): void {
        if (!this.data.appointment?.id) return;
        
        this.isLoading = true;
        
        from(this.apiClient.client.api.appointments.byId(this.data.appointment.id).cancel.post())
            .pipe(finalize(() => this.isLoading = false))
            .subscribe({
                next: () => {
                    this._dialogRef.close({ action: 'cancelled' });
                },
                error: (error) => {
                    console.error('Error cancelling appointment:', error);
                }
            });
    }

    /**
     * Switch to edit mode
     */
    switchToEditMode(): void {
        // Check if editing is allowed for current status and role
        this.canEdit().subscribe(canEdit => {
            if (canEdit) {
                this.data.mode = 'edit';
                this.isReadonly = false;
            } else {
                console.log('Cannot edit appointment: insufficient permissions or invalid status');
            }
        });
    }

    /**
     * Get current appointment status
     */
    get currentStatus(): string {
        const status = this.data.appointment?.status || 'Scheduled';
        return status;
    }

    /**
     * Check if appointment can be edited
     */
    canEdit(): Observable<boolean> {
        const statusCheck = this.currentStatus === 'Scheduled';
        return this.roleService.canEditAppointments().pipe(
            map(roleCheck => statusCheck && roleCheck)
        );
    }

    /**
     * Check if appointment can be cancelled
     */
    canCancel(): Observable<boolean> {
        const statusCheck = this.currentStatus === 'Scheduled';
        return this.roleService.canCancelAppointments().pipe(
            map(roleCheck => statusCheck && roleCheck)
        );
    }

    /**
     * Check if appointment can be completed
     */
    canComplete(): Observable<boolean> {
        const statusCheck = this.currentStatus === 'Scheduled';
        return this.roleService.canCompleteAppointments().pipe(
            map(roleCheck => statusCheck && roleCheck)
        );
    }

    /**
     * Check if appointment can be paid
     */
    canPay(): Observable<boolean> {
        const statusCheck = this.currentStatus === 'Completed';
        return this.roleService.canPayAppointments().pipe(
            map(roleCheck => statusCheck && roleCheck)
        );
    }

    /**
     * Check if appointment can be deleted
     */
    canDelete(): Observable<boolean> {
        const statusCheck = this.currentStatus === 'Cancelled';
        return this.roleService.canDeleteAppointments().pipe(
            map(roleCheck => statusCheck && roleCheck)
        );
    }

    /**
     * Check if appointment is readonly (cannot be edited at all)
     */
    isAppointmentReadonly(): boolean {
        return this.currentStatus === 'Cancelled' || this.currentStatus === 'Paid';
    }

    /**
     * Get provided services for the current appointment
     */
    getProvidedServices(): any[] | null {
        if (!this.data.appointment) {
            return null;
        }
        // Check different possible field names for services
        return this.data.appointment.providedServices || 
               this.data.appointment.services || 
               null;
    }

    /**
     * Get total services price for the current appointment
     */
    getTotalServicesPrice(): number {
        const services = this.getProvidedServices();
        if (services && services.length > 0) {
            return services.reduce((total, service) => total + (service.price || 0), 0);
        }
        return 0;
    }

    /**
     * Track by service ID
     */
    trackByServiceId(index: number, service: any): string {
        return service.id || index.toString();
    }

    /**
     * Toggle services list visibility
     */
    toggleServicesExpansion(): void {
        this.isServicesExpanded = !this.isServicesExpanded;
    }

    /**
     * Delete appointment
     */
    deleteAppointment(): void {
        if (!this.data.appointment?.id) return;
        
        const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
            width: '400px',
            maxWidth: '95vw',
            data: {
                title: 'Видалити запис',
                message: 'Ви впевнені, що хочете видалити цей запис? Цю дію неможливо відмінити.',
                confirmText: 'Видалити',
                cancelText: 'Скасувати'
            }
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result === true) {
                this.isLoading = true;
                
                from(this.apiClient.client.api.appointments.byId(this.data.appointment.id).delete())
                    .pipe(finalize(() => this.isLoading = false))
                    .subscribe({
                        next: () => {
                            this._dialogRef.close({ action: 'deleted' });
                        },
                        error: (error) => {
                            console.error('Error deleting appointment:', error);
                        }
                    });
            }
        });
    }
} 