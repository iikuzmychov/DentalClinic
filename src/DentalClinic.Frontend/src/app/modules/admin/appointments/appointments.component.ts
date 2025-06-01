import { NgIf, NgFor, DatePipe, AsyncPipe } from '@angular/common';
import { Component, OnInit, ViewEncapsulation, inject, AfterViewInit, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { from, Observable, forkJoin, Subject } from 'rxjs';
import { finalize, map, takeUntil, filter, first } from 'rxjs/operators';

// Calendar imports
import { CalendarModule, CalendarEventTimesChangedEvent } from 'angular-calendar';
import { startOfWeek, endOfWeek, subWeeks, addWeeks, format } from 'date-fns';
import { uk } from 'date-fns/locale';

// API imports
import { type AppointmentsRequestBuilderGetQueryParameters } from 'app/api/api/appointments';
import { type UsersRequestBuilderGetQueryParameters } from 'app/api/api/users';
import { type PatientsRequestBuilderGetQueryParameters } from 'app/api/api/patients';
import { ApiClientService } from 'app/core/api/api-client.service';
import { RoleService } from 'app/core/auth/role.service';
import { UserService } from 'app/core/user/user.service';
import { 
    ListAppointmentsResponse, 
    ListAppointmentsResponseItem,
    ListUsersResponse,
    ListUsersResponseItem,
    ListPatientsResponse,
    ListPatientsResponseItem,
    AddAppointmentRequest,
    RoleObject
} from 'app/api/models';

// Local services and providers
import { CALENDAR_PROVIDERS } from 'app/shared/providers/calendar.provider';
import { CalendarService, AppointmentCalendarEvent } from 'app/shared/services/calendar.service';
import { CalendarEventsHook } from 'app/shared/hooks/calendar-events.hook';
import { AppointmentDialogComponent } from './appointment-dialog/appointment-dialog.component';

@Component({
    selector: 'appointments',
    standalone: true,
    templateUrl: './appointments.component.html',
    encapsulation: ViewEncapsulation.None,
    providers: CALENDAR_PROVIDERS,
    imports: [
        NgIf,
        NgFor,
        DatePipe,
        AsyncPipe,
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatDialogModule,
        MatIconModule,
        MatMenuModule,
        MatInputModule,
        MatFormFieldModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        MatTooltipModule,
        MatSelectModule,
        CalendarModule
    ]
})
export class AppointmentsComponent implements OnInit, AfterViewInit, OnDestroy {
    
    // Services
    private readonly _apiClient = inject(ApiClientService);
    private readonly _dialog = inject(MatDialog);
    private readonly _snackBar = inject(MatSnackBar);
    private readonly _calendarService = inject(CalendarService);
    private readonly _eventsHook = inject(CalendarEventsHook);
    private readonly _sanitizer = inject(DomSanitizer);
    private readonly _roleService = inject(RoleService);
    private readonly _userService = inject(UserService);

    // State
    appointments: ListAppointmentsResponseItem[] = [];
    calendarEvents: AppointmentCalendarEvent[] = [];
    viewDate: Date = new Date();
    isLoading = false;
    isInitialLoad = true;

    // Filter data
    dentists: ListUsersResponseItem[] = [];
    patients: ListPatientsResponseItem[] = [];
    
    // Filter values
    selectedDentistId: string | null = null;
    selectedPatientId: string | null = null;

    // Role-based permissions
    canCreate$: Observable<boolean>;
    canEdit$: Observable<boolean>;
    canCancel$: Observable<boolean>;
    canComplete$: Observable<boolean>;
    canPay$: Observable<boolean>;
    canDelete$: Observable<boolean>;

    // Config
    readonly weekStartsOn = 1; // Monday
    readonly hourSegments = 4; // 15-minute segments
    readonly dayStartHour = 0; // Начало с 00:00
    readonly dayEndHour = 24; // Конец в 24:00 (полночь)

    private destroy$ = new Subject<void>();

    constructor() {
        // Initialize permission observables
        this.canCreate$ = this._roleService.canCreateAppointments();
        this.canEdit$ = this._roleService.canEditAppointments();
        this.canCancel$ = this._roleService.canCancelAppointments();
        this.canComplete$ = this._roleService.canCompleteAppointments();
        this.canPay$ = this._roleService.canPayAppointments();
        this.canDelete$ = this._roleService.canDeleteAppointments();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    ngOnInit(): void {
        this.loadFiltersData();
    }

    ngAfterViewInit(): void {
        // Задержка чтобы убедиться что календарь отрендерился
        setTimeout(() => {
            this.scrollToCurrentTime();
        }, 500);
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Load filter data (dentists and patients) then appointments
     */
    loadFiltersData(): void {
        this.isLoading = true;
        
        const dentistsRequest = from(this._apiClient.client.api.users.get({
            queryParameters: {
                role: 'Dentist',
                pageIndex: 0,
                pageSize: 100
            } as UsersRequestBuilderGetQueryParameters
        })).pipe(
            map((response: ListUsersResponse) => response?.items || []),
            takeUntil(this.destroy$)
        );

        const patientsRequest = from(this._apiClient.client.api.patients.get({
            queryParameters: {
                pageIndex: 0,
                pageSize: 100
            } as PatientsRequestBuilderGetQueryParameters
        })).pipe(
            map((response: ListPatientsResponse) => response?.items || []),
            takeUntil(this.destroy$)
        );

        forkJoin({
            dentists: dentistsRequest,
            patients: patientsRequest
        }).pipe(
            finalize(() => {
                this.isLoading = false;
                this.isInitialLoad = false;
            }),
            takeUntil(this.destroy$)
        ).subscribe({
            next: ({ dentists, patients }) => {
                this.dentists = dentists;
                this.patients = patients;
                
                // Auto-select current user if they are a dentist - wait for user to load
                this._userService.user$
                    .pipe(
                        filter(user => !!user), // Wait for user to be defined
                        first(), // Take only the first valid user
                        takeUntil(this.destroy$)
                    )
                    .subscribe(user => {
                        this.autoSelectCurrentDentistWithUser(user);
                    });
                
                this.loadAppointments();
            },
            error: (error) => {
                console.error('Error loading filter data:', error);
                this._snackBar.open('Помилка завантаження даних фільтрів', 'OK', { duration: 3000 });
                // Load appointments anyway
                this.loadAppointments();
            }
        });
    }

    /**
     * Auto-select current dentist if user is a dentist
     */
    private autoSelectCurrentDentist(): void {
        const currentUser = this._userService.user;
        console.log('🏥 AppointmentsComponent - Auto-select dentist check:');
        console.log('👤 Current user:', currentUser);
        console.log('🦷 Available dentists:', this.dentists);
        console.log('🔍 Selected dentist ID before:', this.selectedDentistId);
        
        // Only auto-select if no dentist is already selected and user is a dentist
        if (!this.selectedDentistId && currentUser?.role === RoleObject.Dentist && currentUser?.id) {
            console.log('✅ User is dentist, looking for match...');
            // Find current user in dentists list
            const currentDentist = this.dentists.find(dentist => dentist.id === currentUser.id);
            console.log('🔎 Found matching dentist:', currentDentist);
            
            if (currentDentist) {
                this.selectedDentistId = currentDentist.id;
                console.log('✅ Auto-selected dentist ID:', this.selectedDentistId);
            } else {
                console.log('❌ Current user not found in dentists list');
            }
        } else {
            console.log('❌ Auto-select skipped:', {
                alreadySelected: !!this.selectedDentistId,
                userRole: currentUser?.role,
                expectedRole: RoleObject.Dentist,
                userId: currentUser?.id
            });
        }
    }

    /**
     * Load appointments for current week with filters
     */
    loadAppointments(): void {
        this.isLoading = true;
        
        // Calculate current week range
        const weekStart = startOfWeek(this.viewDate, { weekStartsOn: this.weekStartsOn });
        const weekEnd = endOfWeek(this.viewDate, { weekStartsOn: this.weekStartsOn });

        const queryParams: AppointmentsRequestBuilderGetQueryParameters = {
            startDateTime: weekStart,
            endDateTime: weekEnd
        };

        // Add filters if selected
        if (this.selectedDentistId) {
            queryParams.dentistId = this.selectedDentistId as any;
        }
        
        if (this.selectedPatientId) {
            queryParams.patientId = this.selectedPatientId as any;
        }

        const requestConfig = {
            queryParameters: queryParams
        };
        
        from(this._apiClient.client.api.appointments.get(requestConfig))
            .pipe(finalize(() => this.isLoading = false))
            .subscribe({
                next: async (response: ListAppointmentsResponse) => {
                    this.appointments = response?.items || [];
                    // Use the new method with price loading
                    this.calendarEvents = await this._calendarService.convertAppointmentsToEventsWithPrices(this.appointments);
                },
                error: (error) => {
                    console.error('Error loading appointments:', error);
                    this._snackBar.open(
                        'Помилка завантаження записів', 
                        'Повторити', 
                        { duration: 5000 }
                    ).onAction().subscribe(() => this.loadAppointments());
                }
            });
    }

    /**
     * Navigate to previous week
     */
    previousWeek(): void {
        this.viewDate = subWeeks(this.viewDate, 1);
        this.loadAppointments();
    }

    /**
     * Navigate to next week
     */
    nextWeek(): void {
        this.viewDate = addWeeks(this.viewDate, 1);
        this.loadAppointments();
    }

    /**
     * Go to current week
     */
    today(): void {
        this.viewDate = new Date();
        this.loadAppointments();
    }

    /**
     * Get current week range display in Ukrainian
     */
    getCurrentWeekRange(): string {
        const start = startOfWeek(this.viewDate, { weekStartsOn: this.weekStartsOn });
        const end = endOfWeek(this.viewDate, { weekStartsOn: this.weekStartsOn });
        return `${format(start, 'd MMM', { locale: uk })} - ${format(end, 'd MMM yyyy', { locale: uk })}`;
    }

    /**
     * Get status themes for legend
     */
    getStatusThemes() {
        return this._calendarService.getAllStatusThemes();
    }

    /**
     * Track by function for status items
     */
    trackByStatusId(index: number, item: any): string {
        return item.status;
    }

    /**
     * Handle event click
     */
    onEventClick(event: AppointmentCalendarEvent): void {
        // Find the original appointment data
        const appointment = this.appointments.find(apt => apt.id === event.meta.appointment.id);
        
        if (appointment) {
            const dialogRef = this._dialog.open(AppointmentDialogComponent, {
                width: '800px',
                maxWidth: '95vw',
                data: { 
                    mode: 'view',
                    appointment: appointment
                }
            });

            dialogRef.afterClosed().subscribe(result => {
                // Only show message and reload for actual actions, not dialog close
                if (result && result.action && ['updated', 'completed', 'paid', 'cancelled', 'deleted'].includes(result.action)) {
                    // Reload appointments after status change
                    this.loadAppointments();
                    
                    // Show success message
                    const messages = {
                        updated: 'Запис оновлено успішно',
                        completed: 'Запис завершено',
                        paid: 'Запис сплачено',
                        cancelled: 'Запис скасовано',
                        deleted: 'Запис видалено'
                    };
                    
                    this._snackBar.open(messages[result.action] || 'Статус змінено', 'OK', { duration: 3000 });
                }
            });
        }
    }

    /**
     * Handle event time change (drag & drop)
     */
    onEventTimeChange(changeEvent: CalendarEventTimesChangedEvent): void {
        this._eventsHook.handleEventTimeChange(changeEvent, () => {
            this.loadAppointments(); // Reload to reflect changes
        }).subscribe();
    }

    /**
     * Handle event resize
     */
    onEventResize(resizeEvent: any): void {
        this._eventsHook.handleEventResize(resizeEvent, () => {
            this.loadAppointments(); // Reload to reflect changes
        }).subscribe();
    }

    /**
     * Add new appointment
     */
    addAppointment(): void {
        const dialogRef = this._dialog.open(AppointmentDialogComponent, {
            width: '800px',
            maxWidth: '95vw',
            data: { mode: 'add' }
        });

        dialogRef.afterClosed().subscribe(result => {
            // Only show message and reload for actual actions, not dialog close
            if (result && result.action && ['created', 'updated', 'completed', 'paid', 'cancelled', 'deleted'].includes(result.action)) {
                this.loadAppointments();
                
                const messages = {
                    created: 'Запис створено успішно',
                    updated: 'Запис оновлено успішно',
                    completed: 'Запис завершено',
                    paid: 'Запис сплачено',
                    cancelled: 'Запис скасовано',
                    deleted: 'Запис видалено'
                };
                
                this._snackBar.open(messages[result.action] || 'Дію виконано', 'OK', { duration: 3000 });
            }
        });
    }

    /**
     * Handle dentist filter change
     */
    onDentistFilterChange(): void {
        this.loadAppointments();
    }

    /**
     * Handle patient filter change
     */
    onPatientFilterChange(): void {
        this.loadAppointments();
    }

    /**
     * Clear all filters
     */
    clearFilters(): void {
        this.selectedDentistId = null;
        this.selectedPatientId = null;
        this.loadAppointments();
    }

    /**
     * Get dentist display name
     */
    getDentistDisplayName(dentist: ListUsersResponseItem): string {
        const parts = [dentist.lastName, dentist.firstName, dentist.surname]
            .filter(Boolean)
            .map(part => part?.trim())
            .filter(part => part && part.length > 0);
        return parts.length > 0 ? parts.join(' ') : dentist.email || 'Невідомий лікар';
    }

    /**
     * Get patient display name
     */
    getPatientDisplayName(patient: ListPatientsResponseItem): string {
        const parts = [patient.lastName, patient.firstName, patient.surname]
            .filter(Boolean)
            .map(part => part?.trim())
            .filter(part => part && part.length > 0);
        return parts.length > 0 ? parts.join(' ') : patient.email || 'Невідомий пацієнт';
    }

    /**
     * Get appointments count
     */
    getAppointmentsCount(): number {
        return this.appointments.length;
    }

    /**
     * Get appointments count by status
     */
    getAppointmentsCountByStatus(status: string): number {
        return this.appointments.filter(apt => apt.status === status).length;
    }

    /**
     * TrackBy function for dentists ngFor
     */
    trackByDentistId(index: number, item: ListUsersResponseItem): string {
        return item.id || index.toString();
    }

    /**
     * TrackBy function for patients ngFor
     */
    trackByPatientId(index: number, item: ListPatientsResponseItem): string {
        return item.id || index.toString();
    }

    /**
     * Sanitize HTML content for safe display
     */
    sanitizeHtml(html: string): SafeHtml {
        return this._sanitizer.bypassSecurityTrustHtml(html);
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Private methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Scroll calendar to current time
     */
    private scrollToCurrentTime(): void {
        try {
            const calendarElement = document.querySelector('.weekly-calendar .cal-time-events');
            if (!calendarElement) {
                return;
            }

            const now = new Date();
            const currentHour = now.getHours();
            const currentMinutes = now.getMinutes();
            
            // Calculate the scroll position
            // Each hour takes approximately 60px (this may need adjustment based on your calendar styling)
            const pixelsPerHour = 60;
            const pixelsPerMinute = pixelsPerHour / 60;
            
            const scrollPosition = (currentHour * pixelsPerHour) + (currentMinutes * pixelsPerMinute);
            
            // Scroll to position with smooth behavior
            calendarElement.scrollTo({
                top: scrollPosition,
                behavior: 'smooth'
            });
            
        } catch (error) {
            console.error('Error scrolling to current time:', error);
        }
    }

    /**
     * Auto-select current dentist if user is a dentist
     */
    private autoSelectCurrentDentistWithUser(user: any): void {
        console.log('🏥 AppointmentsComponent - Auto-select dentist check:');
        console.log('👤 Current user:', user);
        console.log('🦷 Available dentists:', this.dentists);
        console.log('🔍 Selected dentist ID before:', this.selectedDentistId);
        
        // Only auto-select if no dentist is already selected and user is a dentist
        if (!this.selectedDentistId && user?.role === RoleObject.Dentist && user?.id) {
            console.log('✅ User is dentist, looking for match...');
            // Find current user in dentists list
            const currentDentist = this.dentists.find(dentist => dentist.id === user.id);
            console.log('🔎 Found matching dentist:', currentDentist);
            
            if (currentDentist) {
                this.selectedDentistId = currentDentist.id;
                console.log('✅ Auto-selected dentist ID:', this.selectedDentistId);
            } else {
                console.log('❌ Current user not found in dentists list');
            }
        } else {
            console.log('❌ Auto-select skipped:', {
                alreadySelected: !!this.selectedDentistId,
                userRole: user?.role,
                expectedRole: RoleObject.Dentist,
                userId: user?.id
            });
        }
    }
} 