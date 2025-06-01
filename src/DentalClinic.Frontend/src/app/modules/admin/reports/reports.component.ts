import { NgIf, NgFor, DatePipe, AsyncPipe, CurrencyPipe, KeyValuePipe } from '@angular/common';
import { Component, OnInit, ViewEncapsulation, inject, OnDestroy } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { from, Observable, forkJoin, Subject } from 'rxjs';
import { finalize, map, takeUntil } from 'rxjs/operators';
import { startOfMonth, endOfMonth, subMonths, format } from 'date-fns';
import { uk } from 'date-fns/locale';

// API imports
import { ApiClientService } from 'app/core/api/api-client.service';
import { RoleService } from 'app/core/auth/role.service';
import { 
    ListAppointmentsResponse, 
    ListAppointmentsResponseItem,
    ListUsersResponse,
    ListUsersResponseItem,
    ListServicesResponse,
    ListServicesResponseItem,
    AppointmentStatus
} from 'app/api/models';

export interface DoctorStats {
    doctorId: string;
    doctorName: string;
    totalRevenue: number;
    completedAppointments: number;
    averageCheck: number;
    services: { [serviceName: string]: { count: number; revenue: number } };
}

export interface RevenueStats {
    totalRevenue: number;
    completedAppointments: number;
    averageCheck: number;
    doctorStats: DoctorStats[];
    topServices: { name: string; count: number; revenue: number }[];
}

@Component({
    selector: 'reports',
    standalone: true,
    templateUrl: './reports.component.html',
    encapsulation: ViewEncapsulation.None,
    imports: [
        NgIf,
        NgFor,
        DatePipe,
        AsyncPipe,
        CurrencyPipe,
        KeyValuePipe,
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatIconModule,
        MatFormFieldModule,
        MatInputModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatProgressSpinnerModule,
        MatSnackBarModule
    ]
})
export class ReportsComponent implements OnInit, OnDestroy {
    
    // Services
    private readonly _apiClient = inject(ApiClientService);
    private readonly _roleService = inject(RoleService);

    // State
    appointments: ListAppointmentsResponseItem[] = [];
    doctors: ListUsersResponseItem[] = [];
    services: ListServicesResponseItem[] = [];
    isLoading = false;
    
    // Filters
    startDate: Date = startOfMonth(new Date());
    endDate: Date = endOfMonth(new Date());
    
    // Statistics
    revenueStats: RevenueStats | null = null;
    
    // Role check
    canViewReports$: Observable<boolean>;

    private destroy$ = new Subject<void>();

    constructor() {
        // Check if user can view reports (admin only)
        this.canViewReports$ = this._roleService.isAdmin();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    ngOnInit(): void {
        this.loadInitialData();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Load initial data (doctors, services)
     */
    loadInitialData(): void {
        this.isLoading = true;
        
        const doctorsRequest = from(this._apiClient.client.api.users.get({
            queryParameters: {
                role: 'Dentist',
                pageIndex: 0,
                pageSize: 100
            }
        })).pipe(
            map((response: ListUsersResponse) => response?.items || []),
            takeUntil(this.destroy$)
        );

        const servicesRequest = from(this._apiClient.client.api.services.get({
            queryParameters: {
                pageIndex: 0,
                pageSize: 1000
            }
        })).pipe(
            map((response: ListServicesResponse) => response?.items || []),
            takeUntil(this.destroy$)
        );

        forkJoin({
            doctors: doctorsRequest,
            services: servicesRequest
        }).pipe(
            finalize(() => this.isLoading = false),
            takeUntil(this.destroy$)
        ).subscribe({
            next: ({ doctors, services }) => {
                this.doctors = doctors;
                this.services = services;
                this.loadReportsData();
            },
            error: (error) => {
                console.error('Error loading initial data:', error);
            }
        });
    }

    /**
     * Load reports data based on current filters
     */
    loadReportsData(): void {
        this.isLoading = true;
        
        const queryParams: any = {
            startDateTime: this.startDate,
            endDateTime: this.endDate,
            pageIndex: 0,
            pageSize: 1000
        };

        from(this._apiClient.client.api.appointments.get({
            queryParameters: queryParams
        })).pipe(
            finalize(() => this.isLoading = false),
            takeUntil(this.destroy$)
        ).subscribe({
            next: (response: ListAppointmentsResponse) => {
                this.appointments = response?.items || [];
                this.calculateStatistics();
            },
            error: (error) => {
                console.error('Error loading appointments:', error);
            }
        });
    }

    /**
     * Calculate revenue statistics
     */
    private calculateStatistics(): void {
        // Filter only completed and paid appointments
        const revenueAppointments = this.appointments.filter(apt => 
            apt.status === 'Completed' || apt.status === 'Paid'
        );

        // Group by doctors
        const doctorStatsMap = new Map<string, DoctorStats>();
        let totalRevenue = 0;
        const serviceStatsMap = new Map<string, { count: number; revenue: number }>();

        revenueAppointments.forEach(appointment => {
            const doctorId = appointment.dentist?.id || 'unknown';
            const doctorName = this.getDoctorDisplayName(appointment.dentist);
            
            // Calculate appointment revenue
            const appointmentRevenue = this.calculateAppointmentRevenue(appointment);
            totalRevenue += appointmentRevenue;

            // Update doctor stats
            if (!doctorStatsMap.has(doctorId)) {
                doctorStatsMap.set(doctorId, {
                    doctorId,
                    doctorName,
                    totalRevenue: 0,
                    completedAppointments: 0,
                    averageCheck: 0,
                    services: {}
                });
            }

            const doctorStats = doctorStatsMap.get(doctorId)!;
            doctorStats.totalRevenue += appointmentRevenue;
            doctorStats.completedAppointments++;

            // Update service stats for doctor and global
            appointment.providedServices?.forEach(service => {
                const serviceName = service.name || 'Невідома послуга';
                const servicePrice = this.getServicePrice(service.id);

                // Doctor services
                if (!doctorStats.services[serviceName]) {
                    doctorStats.services[serviceName] = { count: 0, revenue: 0 };
                }
                doctorStats.services[serviceName].count++;
                doctorStats.services[serviceName].revenue += servicePrice;

                // Global services
                if (!serviceStatsMap.has(serviceName)) {
                    serviceStatsMap.set(serviceName, { count: 0, revenue: 0 });
                }
                const globalServiceStats = serviceStatsMap.get(serviceName)!;
                globalServiceStats.count++;
                globalServiceStats.revenue += servicePrice;
            });
        });

        // Calculate average checks
        doctorStatsMap.forEach(stats => {
            stats.averageCheck = stats.completedAppointments > 0 
                ? stats.totalRevenue / stats.completedAppointments 
                : 0;
        });

        // Convert to arrays and sort
        const doctorStats = Array.from(doctorStatsMap.values())
            .sort((a, b) => b.totalRevenue - a.totalRevenue);

        const topServices = Array.from(serviceStatsMap.entries())
            .map(([name, stats]) => ({ name, ...stats }))
            .sort((a, b) => b.revenue - a.revenue)
            .slice(0, 10);

        this.revenueStats = {
            totalRevenue,
            completedAppointments: revenueAppointments.length,
            averageCheck: revenueAppointments.length > 0 ? totalRevenue / revenueAppointments.length : 0,
            doctorStats,
            topServices
        };
    }

    /**
     * Calculate revenue for a single appointment
     */
    private calculateAppointmentRevenue(appointment: ListAppointmentsResponseItem): number {
        if (!appointment.providedServices) return 0;
        
        return appointment.providedServices.reduce((total, service) => {
            const servicePrice = this.getServicePrice(service.id);
            return total + servicePrice;
        }, 0);
    }

    /**
     * Get service price by ID
     */
    private getServicePrice(serviceId: string | undefined): number {
        if (!serviceId) return 0;
        const service = this.services.find(s => s.id === serviceId);
        return service?.price || 0;
    }

    /**
     * Get doctor display name
     */
    getDoctorDisplayName(doctor: any): string {
        if (!doctor) return 'Невідомий лікар';
        const parts = [doctor.lastName, doctor.firstName, doctor.surname]
            .filter(Boolean)
            .map(part => part?.trim())
            .filter(part => part && part.length > 0);
        return parts.length > 0 ? parts.join(' ') : doctor.email || 'Невідомий лікар';
    }

    /**
     * Filter change handlers
     */
    onDateRangeChange(): void {
        this.loadReportsData();
    }

    /**
     * Set predefined date ranges
     */
    setCurrentMonth(): void {
        this.startDate = startOfMonth(new Date());
        this.endDate = endOfMonth(new Date());
        this.loadReportsData();
    }

    setPreviousMonth(): void {
        const prevMonth = subMonths(new Date(), 1);
        this.startDate = startOfMonth(prevMonth);
        this.endDate = endOfMonth(prevMonth);
        this.loadReportsData();
    }

    /**
     * Get formatted date range
     */
    getDateRangeDisplay(): string {
        return `${format(this.startDate, 'd MMM yyyy', { locale: uk })} - ${format(this.endDate, 'd MMM yyyy', { locale: uk })}`;
    }
} 