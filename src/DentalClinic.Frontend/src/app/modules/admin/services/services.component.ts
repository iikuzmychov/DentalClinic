import { NgIf, NgFor, CurrencyPipe, AsyncPipe } from '@angular/common';
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
import { from, Observable } from 'rxjs';
import { type ServicesRequestBuilderGetQueryParameters } from 'app/api/api/services';
import { ApiClientService } from 'app/core/api/api-client.service';
import { RoleService } from 'app/core/auth/role.service';
import { 
    ListServicesResponse, 
    ListServicesResponseItem, 
    AddServiceRequest, 
    UpdateServiceRequest
} from 'app/api/models';
import { ServiceDialogComponent, ServiceDialogData } from './service-dialog/service-dialog.component';
import { DeleteConfirmationDialogComponent, DeleteConfirmationData } from './delete-confirmation-dialog/delete-confirmation-dialog.component';

@Component({
    selector     : 'services',
    standalone   : true,
    templateUrl  : './services.component.html',
    encapsulation: ViewEncapsulation.None,
    imports: [
        NgIf,
        NgFor,
        CurrencyPipe,
        AsyncPipe,
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
export class ServicesComponent implements OnInit
{
    services: ListServicesResponseItem[] = [];
    totalCount: number = 0;
    totalPages: number = 0;
    isLoading: boolean = false;
    isInitialLoad: boolean = true;
    searchTerm: string = '';
    previousSearchTerm: string = '';
    lastCompletedSearchTerm: string = '';
    displayedColumns: string[] = ['name', 'price', 'actions'];
    
    private searchTimeout: any;

    // Role-based permissions
    canCreate$: Observable<boolean>;
    canEdit$: Observable<boolean>;
    canDelete$: Observable<boolean>;

    /**
     * Constructor - Using generated types and new API client
     */
    constructor(
        private apiClient: ApiClientService,
        private _dialog: MatDialog,
        private _snackBar: MatSnackBar,
        private _roleService: RoleService
    )
    {
        // Initialize permission observables
        this.canCreate$ = this._roleService.canCreateServices();
        this.canEdit$ = this._roleService.canEditServices();
        this.canDelete$ = this._roleService.canDeleteServices();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void
    {
        this.loadServices();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Load services using generated ServicesRequestBuilderGetQueryParameters
     */
    loadServices(): void
    {
        this.isLoading = true;
        
        // Using the exact same structure as generated ServicesRequestBuilder.get()
        const requestConfiguration = {
            queryParameters: {
                name: this.searchTerm || undefined,
                pageIndex: 0,
                pageSize: 20
            } as ServicesRequestBuilderGetQueryParameters
        };
        
        from(this.apiClient.client.api.services.get(requestConfiguration)).subscribe({
            next: (response: ListServicesResponse) => {
                this.services = response?.items || [];
                this.totalCount = response?.totalCount || 0;
                this.totalPages = response?.totalPagesCount || 0;
                this.isLoading = false;
                this.isInitialLoad = false;
                this.previousSearchTerm = this.searchTerm;
                this.lastCompletedSearchTerm = this.searchTerm;
            },
            error: (error) => {
                console.error('Error loading services:', error);
                this._snackBar.open('Помилка завантаження послуг', 'OK', { duration: 3000 });
                this.isLoading = false;
                this.isInitialLoad = false;
                this.previousSearchTerm = this.searchTerm;
                this.lastCompletedSearchTerm = this.searchTerm;
            }
        });
    }

    /**
     * Search services
     */
    onSearch(): void
    {
        this.loadServices();
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
     * Add new service
     */
    addService(): void
    {
        const dialogRef = this._dialog.open(ServiceDialogComponent, {
            width: '500px',
            maxWidth: '95vw',
            data: { mode: 'add' } as ServiceDialogData
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                this.createService(result);
            }
        });
    }

    /**
     * Edit service
     */
    editService(service: ListServicesResponseItem): void
    {
        const dialogRef = this._dialog.open(ServiceDialogComponent, {
            width: '500px',
            maxWidth: '95vw',
            data: { 
                mode: 'edit', 
                service: { ...service } 
            } as ServiceDialogData
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && service.id) {
                this.updateService(service.id, result);
            }
        });
    }

    /**
     * Delete service using generated structure: api.services.byId(id).delete()
     */
    deleteService(service: ListServicesResponseItem): void
    {
        const dialogRef = this._dialog.open(DeleteConfirmationDialogComponent, {
            width: '400px',
            data: {
                title: 'Видалення послуги',
                message: `Ви впевнені, що хочете видалити послугу "${service.name}"?`,
                confirmText: 'Видалити',
                cancelText: 'Скасувати'
            } as DeleteConfirmationData
        });

        dialogRef.afterClosed().subscribe(result => {
            if (result && service.id) {
                this.isLoading = true;
                
                // Using the same structure as generated api.services.byId(id).delete()
                from(this.apiClient.client.api.services.byId(service.id).delete()).subscribe({
                    next: () => {
                        this._snackBar.open('Послугу видалено', 'OK', { duration: 3000 });
                        this.loadServices();
                    },
                    error: (error) => {
                        console.error('Error deleting service:', error);
                        this._snackBar.open('Помилка видалення послуги', 'OK', { duration: 3000 });
                        this.isLoading = false;
                    }
                });
            }
        });
    }

    /**
     * Create service using generated structure: api.services.post(body)
     */
    private createService(serviceData: AddServiceRequest): void
    {
        this.isLoading = true;
        
        // Using the same structure as generated api.services.post(body)
        from(this.apiClient.client.api.services.post(serviceData)).subscribe({
            next: () => {
                this._snackBar.open('Послугу створено', 'OK', { duration: 3000 });
                this.loadServices();
            },
            error: (error) => {
                console.error('Error creating service:', error);
                this._snackBar.open('Помилка створення послуги', 'OK', { duration: 3000 });
                this.isLoading = false;
            }
        });
    }

    /**
     * Update service using generated structure: api.services.byId(id).put(body)
     */
    private updateService(serviceId: any, serviceData: UpdateServiceRequest): void
    {
        this.isLoading = true;
        
        // Using the same structure as generated api.services.byId(id).put(body)
        from(this.apiClient.client.api.services.byId(serviceId).put(serviceData)).subscribe({
            next: () => {
                this._snackBar.open('Послугу оновлено', 'OK', { duration: 3000 });
                this.loadServices();
            },
            error: (error) => {
                console.error('Error updating service:', error);
                this._snackBar.open('Помилка оновлення послуги', 'OK', { duration: 3000 });
                this.isLoading = false;
            }
        });
    }
}
