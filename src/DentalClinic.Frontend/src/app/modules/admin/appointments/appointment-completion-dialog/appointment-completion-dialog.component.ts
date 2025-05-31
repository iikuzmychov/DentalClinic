import { NgIf, NgFor } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { from, Observable, Subject } from 'rxjs';
import { finalize, startWith, map, debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { ApiClientService } from 'app/core/api/api-client.service';
import { 
    CompleteAppointmentRequest,
    ListServicesResponse,
    ListServicesResponseItem
} from 'app/api/models';

export interface AppointmentCompletionDialogData {
    appointment: any;
}

@Component({
    selector: 'app-appointment-completion-dialog',
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
        MatCheckboxModule,
        MatProgressSpinnerModule,
        MatDividerModule,
        MatChipsModule,
        MatAutocompleteModule,
        ScrollingModule
    ],
    styles: [`
        :host {
            display: block;
            width: 100%;
        }
        
        ::ng-deep .mat-mdc-dialog-container .mdc-dialog__surface {
            padding: 0 !important;
        }
        
        .service-item {
            display: flex;
            align-items: center;
            padding: 12px 0;
            border-bottom: 1px solid #f0f0f0;
        }
        
        .service-item:last-child {
            border-bottom: none;
        }
        
        .service-info {
            flex: 1;
            margin-left: 12px;
        }
        
        .service-name {
            font-weight: 500;
            color: #1f2937;
        }
        
        .service-description {
            font-size: 0.875rem;
            color: #6b7280;
            margin-top: 2px;
        }
        
        .service-price {
            font-weight: 600;
            color: #059669;
        }
        
        .total-section {
            background: #f9fafb;
            border-radius: 8px;
            padding: 16px;
            margin-top: 16px;
        }
        
        .appointment-info {
            background: #eff6ff;
            border-radius: 8px;
            padding: 16px;
            margin-bottom: 24px;
        }
        
        .content-container {
            max-height: 70vh;
            overflow-y: auto;
        }
        
        .services-list {
            max-height: 300px;
            overflow-y: auto;
            border: 1px solid #e5e7eb;
            border-radius: 6px;
            padding: 8px;
        }
        
        .autocomplete-viewport {
            max-height: 200px;
        }
        
        ::ng-deep .autocomplete-viewport .cdk-virtual-scroll-content-wrapper {
            max-height: 200px;
        }
    `],
    template: `
        <div class="flex flex-col w-full h-full">
            
            <!-- Header -->
            <div class="flex flex-0 items-center justify-between h-16 px-6 bg-primary text-on-primary">
                <div class="flex items-center space-x-2">
                    <mat-icon class="text-current" [svgIcon]="'heroicons_outline:check'"></mat-icon>
                    <div class="text-lg font-medium">Завершення запису</div>
                </div>
                <button mat-icon-button (click)="cancel()">
                    <mat-icon class="text-current" [svgIcon]="'heroicons_outline:x-mark'"></mat-icon>
                </button>
            </div>

            <!-- Content -->
            <div class="flex flex-col flex-1 px-6 py-6 content-container">
                
                <!-- Appointment Info -->
                <div class="appointment-info">
                    <h3 class="text-lg font-semibold text-gray-900 mb-2">Інформація про запис</h3>
                    <div class="grid grid-cols-2 gap-4 text-sm">
                        <div>
                            <span class="text-gray-600">Пацієнт:</span>
                            <div class="font-medium">{{ getPatientName() }}</div>
                        </div>
                        <div>
                            <span class="text-gray-600">Лікар:</span>
                            <div class="font-medium">{{ getDentistName() }}</div>
                        </div>
                        <div>
                            <span class="text-gray-600">Дата:</span>
                            <div class="font-medium">{{ getFormattedDate() }}</div>
                        </div>
                        <div>
                            <span class="text-gray-600">Тривалість:</span>
                            <div class="font-medium">{{ getFormattedDuration() }}</div>
                        </div>
                    </div>
                </div>

                <!-- Selected Services (Chips) -->
                <div class="mb-4" *ngIf="selectedServices.length > 0">
                    <h3 class="text-lg font-semibold text-gray-900 mb-3">Надані послуги</h3>
                    <mat-chip-set>
                        <mat-chip-row 
                            *ngFor="let service of selectedServices" 
                            (removed)="removeService(service)">
                            {{ service.name }}
                            <span *ngIf="service.price" class="ml-2 text-green-600 font-semibold">{{ service.price }} ₴</span>
                            <mat-icon matChipRemove>cancel</mat-icon>
                        </mat-chip-row>
                    </mat-chip-set>
                </div>

                <!-- Service Search/Add -->
                <div class="mb-4">
                    <h3 class="text-lg font-semibold text-gray-900 mb-3">Додати послугу</h3>
                    <mat-form-field appearance="outline" class="w-full">
                        <mat-label>Пошук послуг</mat-label>
                        <mat-icon matPrefix [svgIcon]="'heroicons_outline:magnifying-glass'"></mat-icon>
                        <input 
                            matInput
                            [formControl]="serviceSearchControl"
                            [matAutocomplete]="serviceAutocomplete"
                            placeholder="Введіть назву послуги">
                        <mat-autocomplete 
                            #serviceAutocomplete="matAutocomplete"
                            [displayWith]="displayServiceFn"
                            (optionSelected)="onServiceSelected($event)">
                            <mat-option 
                                *ngFor="let service of filteredServices; trackBy: trackByServiceId"
                                [value]="service"
                                [disabled]="isServiceSelected(service.id)">
                                <div class="flex justify-between items-center w-full py-2">
                                    <span class="flex-1 mr-4">{{ service.name }}</span>
                                    <span *ngIf="service.price" class="text-green-600 font-semibold whitespace-nowrap">{{ service.price }} ₴</span>
                                </div>
                            </mat-option>
                            <mat-option *ngIf="isLoadingServices" disabled>
                                <div class="flex justify-center py-2">
                                    <mat-spinner diameter="20"></mat-spinner>
                                </div>
                            </mat-option>
                            <mat-option *ngIf="filteredServices.length === 0 && !isLoadingServices" disabled>
                                <div class="text-center text-gray-500 py-2">
                                    Послуги не знайдено
                                </div>
                            </mat-option>
                        </mat-autocomplete>
                    </mat-form-field>
                </div>

                <!-- Total -->
                <div class="total-section" *ngIf="selectedServices.length > 0">
                    <div class="flex justify-between items-center">
                        <span class="text-lg font-semibold">Всього надано послуг:</span>
                        <span class="text-xl font-bold text-primary">{{ selectedServices.length }}</span>
                    </div>
                    <div class="flex justify-between items-center mt-2" *ngIf="getTotalPrice() > 0">
                        <span class="text-lg font-semibold">Загальна сума:</span>
                        <span class="text-xl font-bold text-green-600">{{ getTotalPrice() }} ₴</span>
                    </div>
                </div>

                <!-- Validation message for empty services -->
                <div *ngIf="selectedServices.length === 0" class="mb-6 p-3 bg-yellow-50 border border-yellow-200 rounded-md">
                    <div class="flex items-center">
                        <mat-icon class="text-yellow-600 mr-2" [svgIcon]="'heroicons_outline:exclamation-triangle'"></mat-icon>
                        <span class="text-yellow-800 text-sm">Оберіть хоча б одну послугу для завершення запису</span>
                    </div>
                </div>

                <!-- Actions -->
                <div class="flex items-center justify-end space-x-3 pt-6 border-t mt-auto">
                    <button mat-stroked-button (click)="cancel()" class="px-6">
                        Скасувати
                    </button>
                    <button 
                        mat-flat-button 
                        color="primary" 
                        [disabled]="isCompleting || selectedServices.length === 0"
                        (click)="complete()" 
                        class="px-8">
                        <mat-spinner *ngIf="isCompleting" diameter="20" class="mr-2"></mat-spinner>
                        Завершити запис
                    </button>
                </div>
            </div>
        </div>
    `
})
export class AppointmentCompletionDialogComponent implements OnInit {
    services: ListServicesResponseItem[] = [];
    filteredServices: ListServicesResponseItem[] = [];
    selectedServices: ListServicesResponseItem[] = [];
    isLoading = false;
    isLoadingServices = false;
    isCompleting = false;
    
    // Pagination
    currentPage = 0;
    pageSize = 20;
    hasMoreServices = true;
    
    searchControl = new FormControl('');
    serviceSearchControl = new FormControl('');
    private searchSubject = new Subject<string>();

    constructor(
        private _dialogRef: MatDialogRef<AppointmentCompletionDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: AppointmentCompletionDialogData,
        private apiClient: ApiClientService
    ) {}

    ngOnInit(): void {
        this.setupServiceSearch();
        this.loadInitialServices();
    }

    private setupServiceSearch(): void {
        this.serviceSearchControl.valueChanges.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            map(value => typeof value === 'string' ? value : '')
        ).subscribe(searchTerm => {
            this.filterServices(searchTerm);
        });
    }

    private loadInitialServices(): void {
        this.searchServices('');
    }

    private searchServices(searchTerm: string): void {
        this.isLoadingServices = true;
        this.currentPage = 0;
        this.hasMoreServices = true;
        
        from(this.apiClient.client.api.services.get({
            queryParameters: {
                pageIndex: 0,
                pageSize: this.pageSize
            }
        }))
        .pipe(finalize(() => this.isLoadingServices = false))
        .subscribe({
            next: (response: ListServicesResponse) => {
                this.services = response?.items || [];
                this.filterServices(searchTerm);
                this.hasMoreServices = (response?.items?.length || 0) === this.pageSize;
            },
            error: (error) => {
                console.error('Error loading services:', error);
                this.services = [];
                this.filteredServices = [];
            }
        });
    }

    private loadMoreServices(): void {
        if (!this.hasMoreServices || this.isLoadingServices) return;
        
        this.isLoadingServices = true;
        const searchTerm = this.serviceSearchControl.value || '';
        
        from(this.apiClient.client.api.services.get({
            queryParameters: {
                pageIndex: this.currentPage + 1,
                pageSize: this.pageSize
            }
        }))
        .pipe(finalize(() => this.isLoadingServices = false))
        .subscribe({
            next: (response: ListServicesResponse) => {
                const newServices = response?.items || [];
                this.services = [...this.services, ...newServices];
                this.filterServices(searchTerm);
                this.currentPage++;
                this.hasMoreServices = newServices.length === this.pageSize;
            },
            error: (error) => {
                console.error('Error loading more services:', error);
            }
        });
    }

    private filterServices(searchTerm: string): void {
        if (!searchTerm) {
            this.filteredServices = [...this.services];
        } else {
            this.filteredServices = this.services.filter(service =>
                service.name?.toLowerCase().includes(searchTerm.toLowerCase())
            );
        }
    }

    isServiceSelected(serviceId: string): boolean {
        return this.selectedServices.some(s => s.id === serviceId);
    }

    toggleService(service: ListServicesResponseItem): void {
        const index = this.selectedServices.findIndex(s => s.id === service.id);
        if (index >= 0) {
            this.selectedServices.splice(index, 1);
        } else {
            this.selectedServices.push(service);
        }
    }

    getTotalPrice(): number {
        return this.selectedServices.reduce((total, service) => {
            return total + (service.price || 0);
        }, 0);
    }

    getPatientName(): string {
        const patient = this.data.appointment.patient;
        if (!patient) return 'Невідомий пацієнт';
        
        const parts = [patient.lastName, patient.firstName, patient.surname].filter(Boolean);
        return parts.join(' ') || 'Невідомий пацієнт';
    }

    getDentistName(): string {
        const dentist = this.data.appointment.dentist;
        if (!dentist) return 'Невідомий лікар';
        
        const parts = [dentist.lastName, dentist.firstName, dentist.surname].filter(Boolean);
        return parts.join(' ') || 'Невідомий лікар';
    }

    getFormattedDuration(): string {
        let duration = this.data.appointment.duration;
        if (duration && duration.split(':').length === 3) {
            // Remove seconds part for display
            const parts = duration.split(':');
            return `${parts[0]}:${parts[1]}`;
        }
        return duration || '01:00';
    }

    getFormattedDate(): string {
        const date = new Date(this.data.appointment.startTime);
        return date.toLocaleDateString('uk-UA', {
            day: '2-digit',
            month: 'long',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    complete(): void {
        this.isCompleting = true;
        
        // Convert duration to ЧЧ:ММ format if it has seconds
        let duration = this.data.appointment.duration;
        if (duration && duration.split(':').length === 3) {
            // Remove seconds part
            const parts = duration.split(':');
            duration = `${parts[0]}:${parts[1]}`;
        }
        
        const completeData: CompleteAppointmentRequest = {
            duration: duration,
            providedServiceIds: this.selectedServices.map(s => s.id)
        };
        
        from(this.apiClient.client.api.appointments.byId(this.data.appointment.id).complete.post(completeData))
            .pipe(finalize(() => this.isCompleting = false))
            .subscribe({
                next: () => {
                    this._dialogRef.close({ action: 'completed', services: this.selectedServices });
                },
                error: (error) => {
                    console.error('Error completing appointment:', error);
                }
            });
    }

    cancel(): void {
        this._dialogRef.close();
    }

    removeService(service: ListServicesResponseItem): void {
        const index = this.selectedServices.findIndex(s => s.id === service.id);
        if (index >= 0) {
            this.selectedServices.splice(index, 1);
        }
    }

    onServiceSelected(event: MatAutocompleteSelectedEvent): void {
        const selectedService = event.option.value;
        if (selectedService && !this.isServiceSelected(selectedService.id)) {
            this.selectedServices.push(selectedService);
        }
        // Clear the search field
        this.serviceSearchControl.setValue('');
    }

    displayServiceFn(service: ListServicesResponseItem): string {
        return service?.name || '';
    }

    trackByServiceId(index: number, service: ListServicesResponseItem): string {
        return service.id;
    }
} 