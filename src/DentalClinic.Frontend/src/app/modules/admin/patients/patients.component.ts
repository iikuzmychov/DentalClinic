import { NgIf, NgFor } from '@angular/common';
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { from } from 'rxjs';
import { type PatientsRequestBuilderGetQueryParameters } from 'app/api/api/patients';
import { ApiClientService } from 'app/core/api/api-client.service';
import { 
    ListPatientsResponse, 
    ListPatientsResponseItem, 
    AddPatientRequest, 
    UpdatePatientRequest
} from 'app/api/models';
import { PatientDialogComponent, PatientDialogData } from './patient-dialog/patient-dialog.component';
import { DeleteConfirmationDialogComponent, DeleteConfirmationData } from '../services/delete-confirmation-dialog/delete-confirmation-dialog.component';

@Component({
    selector     : 'patients',
    standalone   : true,
    templateUrl  : './patients.component.html',
    encapsulation: ViewEncapsulation.None,
    imports: [
        NgIf,
        NgFor,
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatDialogModule,
        MatIconModule,
        MatMenuModule,
        MatTableModule,
        MatInputModule,
        MatFormFieldModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        MatTooltipModule
    ]
})
export class PatientsComponent implements OnInit
{
    patients: ListPatientsResponseItem[] = [];
    totalCount: number = 0;
    totalPages: number = 0;
    isLoading: boolean = false;
    isInitialLoad: boolean = true;
    searchTerm: string = '';
    previousSearchTerm: string = '';
    lastCompletedSearchTerm: string = '';
    displayedColumns: string[] = ['fullName', 'email', 'phoneNumber', 'actions'];
    
    private searchTimeout: any;

    /**
     * Constructor - Using generated types and API client
     */
    constructor(
        private apiClient: ApiClientService,
        private _dialog: MatDialog,
        private _snackBar: MatSnackBar
    )
    {
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void
    {
        this.loadPatients();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Load patients using generated PatientsRequestBuilderGetQueryParameters
     */
    loadPatients(): void
    {
        this.isLoading = true;
        
        const requestConfiguration = {
            queryParameters: {
                name: this.searchTerm || undefined,
                pageIndex: 0,
                pageSize: 20
            } as PatientsRequestBuilderGetQueryParameters
        };
        
        from(this.apiClient.client.api.patients.get(requestConfiguration)).subscribe({
            next: (response: ListPatientsResponse) => {
                this.patients = response?.items || [];
                this.totalCount = response?.totalCount || 0;
                this.totalPages = response?.totalPagesCount || 0;
                this.isLoading = false;
                this.isInitialLoad = false;
                this.previousSearchTerm = this.searchTerm;
                this.lastCompletedSearchTerm = this.searchTerm;
            },
            error: (error) => {
                console.error('Error loading patients:', error);
                this._snackBar.open('Помилка завантаження пацієнтів', 'OK', { duration: 3000 });
                this.isLoading = false;
                this.isInitialLoad = false;
                this.previousSearchTerm = this.searchTerm;
                this.lastCompletedSearchTerm = this.searchTerm;
            }
        });
    }

    /**
     * Search patients
     */
    onSearch(): void
    {
        this.loadPatients();
    }

    /**
     * Handle search input with debounce
     */
    onSearchInput(): void
    {
        // Clear previous timeout
        if (this.searchTimeout) {
            clearTimeout(this.searchTimeout);
        }
        
        // Set new timeout for 300ms
        this.searchTimeout = setTimeout(() => {
            this.previousSearchTerm = this.searchTerm;
            this.onSearch();
        }, 300);
    }

    /**
     * Add new patient
     */
    addPatient(): void
    {
        const dialogRef = this._dialog.open(PatientDialogComponent, {
            width: '800px',
            maxWidth: '95vw',
            data: { mode: 'add' } as PatientDialogData
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.createPatient(result);
            }
        });
    }

    /**
     * Edit patient - separate method for edit button click
     */
    editPatient(event: Event, patient: ListPatientsResponseItem): void
    {
        // Stop event propagation to prevent row click
        event.stopPropagation();
        
        if (!patient.id) return;

        // Load full patient data and open in edit mode
        from(this.apiClient.client.api.patients.byId(patient.id).get()).subscribe({
            next: (fullPatient) => {
                const dialogRef = this._dialog.open(PatientDialogComponent, {
                    width: '800px',
                    maxWidth: '95vw',
                    data: { 
                        mode: 'edit', 
                        patient: fullPatient 
                    } as PatientDialogData
                });

                dialogRef.afterClosed().subscribe(result => {
                    if (result && patient.id) {
                        this.updatePatient(patient.id, result);
                    }
                });
            },
            error: (error) => {
                console.error('Error loading patient details:', error);
                this._snackBar.open('Помилка завантаження даних пацієнта', 'OK', { duration: 3000 });
            }
        });
    }

    /**
     * View patient - open dialog in view mode when row is clicked
     */
    viewPatient(patient: ListPatientsResponseItem): void
    {
        if (!patient.id) return;

        // First load full patient data
        from(this.apiClient.client.api.patients.byId(patient.id).get()).subscribe({
            next: (fullPatient) => {
                const dialogRef = this._dialog.open(PatientDialogComponent, {
                    width: '800px',
                    maxWidth: '95vw',
                    data: { 
                        mode: 'view', 
                        patient: fullPatient 
                    } as PatientDialogData
                });

                dialogRef.afterClosed().subscribe(result => {
                    if (result && patient.id) {
                        // Result returned when user clicked "Редагувати" and saved changes
                        this.updatePatient(patient.id, result);
                    }
                });
            },
            error: (error) => {
                console.error('Error loading patient details:', error);
                this._snackBar.open('Помилка завантаження даних пацієнта', 'OK', { duration: 3000 });
            }
        });
    }

    /**
     * Delete patient
     */
    deletePatient(event: Event, patient: ListPatientsResponseItem): void
    {
        // Stop event propagation to prevent row click
        event.stopPropagation();

        const fullName = this.getFullName(patient);
        const dialogRef = this._dialog.open(DeleteConfirmationDialogComponent, {
            width: '400px',
            data: {
                title: 'Видалення пацієнта',
                message: `Ви впевнені, що хочете видалити пацієнта "${fullName}"?`,
                confirmText: 'Видалити',
                cancelText: 'Скасувати'
            } as DeleteConfirmationData
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && patient.id) {
                this.deletePatientById(patient.id, fullName);
            }
        });
    }

    /**
     * Delete patient by ID
     */
    private async deletePatientById(patientId: any, fullName: string): Promise<void>
    {
        this.isLoading = true;
        
        try {
            const response = await this.apiClient.client.api.patients.byId(patientId).delete();
            this._snackBar.open('Пацієнта видалено', 'OK', { duration: 3000 });
            this.loadPatients();
        } catch (error: any) {
            console.error('Error deleting patient:', error);
            this._snackBar.open('Помилка видалення пацієнта', 'OK', { duration: 3000 });
            this.isLoading = false;
        }
    }

    /**
     * Get full name from patient data
     */
    getFullName(patient: ListPatientsResponseItem): string
    {
        const parts = [patient.lastName, patient.firstName, patient.surname].filter(Boolean);
        return parts.join(' ') || 'Невідомий пацієнт';
    }

    /**
     * Track by function for ngFor loops
     */
    trackByFn(index: number, item: any): any
    {
        return item.id || index;
    }

    /**
     * Create patient using generated structure
     */
    private async createPatient(patientData: AddPatientRequest): Promise<void>
    {
        this.isLoading = true;
        
        try {
            const response = await this.apiClient.client.api.patients.post(patientData);
            this._snackBar.open('Пацієнта створено', 'OK', { duration: 3000 });
            this.loadPatients();
        } catch (error: any) {
            console.error('Error creating patient:', error);
            this._snackBar.open('Помилка створення пацієнта', 'OK', { duration: 3000 });
            this.isLoading = false;
        }
    }

    /**
     * Update patient using generated structure
     */
    private async updatePatient(patientId: any, patientData: UpdatePatientRequest): Promise<void>
    {
        this.isLoading = true;
        
        try {
            const response = await this.apiClient.client.api.patients.byId(patientId).put(patientData);
            this._snackBar.open('Дані пацієнта оновлено', 'OK', { duration: 3000 });
            this.loadPatients();
        } catch (error: any) {
            console.error('Error updating patient:', error);
            this._snackBar.open('Помилка оновлення даних пацієнта', 'OK', { duration: 3000 });
            this.isLoading = false;
        }
    }
} 