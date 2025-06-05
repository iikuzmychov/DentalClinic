import { Injectable, inject } from '@angular/core';
import { CalendarEvent } from 'angular-calendar';
import { addMinutes } from 'date-fns';
import { ListAppointmentsResponseItem, AppointmentStatus, ListServicesResponseItem } from 'app/api/models';
import { ApiClientService } from 'app/core/api/api-client.service';
import { forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

export interface AppointmentCalendarEvent extends CalendarEvent {
    id: string;
    meta: {
        appointment: ListAppointmentsResponseItem;
        patientName: string;
        dentistName: string;
        status: AppointmentStatus;
        services: string;
        duration: number; // minutes
    };
}

export interface StatusTheme {
    primary: string;
    secondary: string;
    textColor: string;
    badgeClass: string;
    displayName: string;
}

@Injectable({
    providedIn: 'root'
})
export class CalendarService {
    
    private readonly _apiClient = inject(ApiClientService);
    
    private readonly statusThemes: Record<AppointmentStatus, StatusTheme> = {
        'Scheduled': {
            primary: '#eab308',
            secondary: '#eab308',
            textColor: '#ffffff',
            badgeClass: 'bg-yellow-500',
            displayName: 'Запланований'
        },
        'Completed': {
            primary: '#4caf50',
            secondary: '#4caf50',
            textColor: '#ffffff', 
            badgeClass: 'bg-green-500',
            displayName: 'Завершений'
        },
        'Cancelled': {
            primary: '#f44336',
            secondary: '#f44336',
            textColor: '#ffffff',
            badgeClass: 'bg-red-500', 
            displayName: 'Скасований'
        },
        'Paid': {
            primary: '#374151',
            secondary: '#374151',
            textColor: '#ffffff',
            badgeClass: 'bg-gray-700',
            displayName: 'Сплачений'
        }
    };

    /**
     * Convert API appointments to calendar events (legacy method without prices)
     */
    convertAppointmentsToEvents(appointments: ListAppointmentsResponseItem[]): AppointmentCalendarEvent[] {
        return appointments.map(appointment => this.convertSingleAppointment(appointment));
    }

    /**
     * Convert single appointment to calendar event (legacy method)
     */
    private convertSingleAppointment(appointment: ListAppointmentsResponseItem): AppointmentCalendarEvent {
        const startTime = this.parseDateTime(appointment.startTime);
        const duration = this.parseDuration(appointment.duration);
        const endTime = addMinutes(startTime, duration);
        
        const patientName = this.formatPersonName(
            appointment.patient?.firstName,
            appointment.patient?.lastName, 
            appointment.patient?.surname
        );
        
        const dentistName = this.formatPersonName(
            appointment.dentist?.firstName,
            appointment.dentist?.lastName,
            appointment.dentist?.surname
        );

        const status = appointment.status || 'Scheduled';
        const theme = this.getStatusTheme(status);
        const services = this.formatServices(appointment.providedServices);
        const totalPrice = this.calculateTotalPrice(appointment.providedServices);
        
        // Create comprehensive title
        const title = this.formatEventTitle(patientName, dentistName, appointment.providedServices, totalPrice, status);

        // Only allow drag & drop and resizing for scheduled appointments
        const isScheduled = status === 'Scheduled';

        return {
            id: appointment.id || '',
            start: startTime,
            end: endTime,
            title: title,
            color: {
                primary: theme.primary,
                secondary: theme.secondary
            },
            cssClass: `appointment-${status.toLowerCase()}`,
            resizable: {
                beforeStart: false,
                afterEnd: false
            },
            draggable: false,
            meta: {
                appointment,
                patientName,
                dentistName,
                status,
                services,
                duration
            }
        };
    }

    /**
     * Convert API appointments to calendar events with service prices cache
     */
    async convertAppointmentsToEventsWithPrices(appointments: ListAppointmentsResponseItem[]): Promise<AppointmentCalendarEvent[]> {
        // Create services cache first
        const servicesCache = await this.createServicesCache(appointments);
        
        return appointments.map(appointment => this.convertSingleAppointmentWithCache(appointment, servicesCache));
    }

    /**
     * Create cache of service prices
     */
    private async createServicesCache(appointments: ListAppointmentsResponseItem[]): Promise<Map<string, number>> {
        const cache = new Map<string, number>();
        
        try {
            // Get all unique service IDs
            const allServiceIds = new Set<string>();
            appointments.forEach(appointment => {
                appointment.providedServices?.forEach(service => {
                    if (service.id) {
                        allServiceIds.add(service.id);
                    }
                });
            });

            if (allServiceIds.size === 0) {
                return cache;
            }

            // Load all services in parallel
            const servicePromises = Array.from(allServiceIds).map(async id => {
                try {
                    const service = await this._apiClient.client.api.services.byId(id).get();
                    return { id, price: service?.price || 0 };
                } catch (error) {
                    return { id, price: 0 };
                }
            });

            const serviceResults = await Promise.all(servicePromises);
            
            // Populate cache
            serviceResults.forEach(({ id, price }) => {
                cache.set(id, price);
            });

            return cache;
        } catch (error) {
            console.error('Error creating services cache:', error);
            return cache;
        }
    }

    /**
     * Convert single appointment to calendar event with services cache
     */
    private convertSingleAppointmentWithCache(
        appointment: ListAppointmentsResponseItem, 
        servicesCache: Map<string, number>
    ): AppointmentCalendarEvent {
        const startTime = this.parseDateTime(appointment.startTime);
        const duration = this.parseDuration(appointment.duration);
        const endTime = addMinutes(startTime, duration);
        
        const patientName = this.formatPersonName(
            appointment.patient?.firstName,
            appointment.patient?.lastName, 
            appointment.patient?.surname
        );
        
        const dentistName = this.formatPersonName(
            appointment.dentist?.firstName,
            appointment.dentist?.lastName,
            appointment.dentist?.surname
        );

        const status = appointment.status || 'Scheduled';
        const theme = this.getStatusTheme(status);
        const services = this.formatServices(appointment.providedServices);
        
        // Calculate total price using cache
        const totalPrice = this.calculateTotalPriceFromCache(appointment.providedServices, servicesCache);
        
        // Create comprehensive title
        const title = this.formatEventTitle(patientName, dentistName, appointment.providedServices, totalPrice, status);

        // Only allow drag & drop and resizing for scheduled appointments
        const isScheduled = status === 'Scheduled';

        return {
            id: appointment.id || '',
            start: startTime,
            end: endTime,
            title: title,
            color: {
                primary: theme.primary,
                secondary: theme.secondary
            },
            cssClass: `appointment-${status.toLowerCase()}`,
            resizable: {
                beforeStart: false,
                afterEnd: false
            },
            draggable: false,
            meta: {
                appointment,
                patientName,
                dentistName,
                status,
                services,
                duration
            }
        };
    }

    /**
     * Calculate total price using services cache
     */
    private calculateTotalPriceFromCache(
        services: any[] | null | undefined, 
        servicesCache: Map<string, number>
    ): number {
        if (!services || !Array.isArray(services)) {
            return 0;
        }

        const total = services.reduce((sum, service) => {
            if (service?.id && servicesCache.has(service.id)) {
                return sum + servicesCache.get(service.id)!;
            }
            return sum;
        }, 0);

        return total;
    }

    /**
     * Parse datetime string safely
     */
    private parseDateTime(dateTime: Date | string | null | undefined): Date {
        if (!dateTime) {
            return new Date();
        }
        
        const parsed = new Date(dateTime);
        return isNaN(parsed.getTime()) ? new Date() : parsed;
    }

    /**
     * Parse duration string (HH:mm:ss or mm) to minutes
     */
    private parseDuration(duration: string | null | undefined): number {
        if (!duration) {
            return 60; // Default 1 hour
        }

        // Handle TimeSpan format (HH:mm:ss)
        if (duration.includes(':')) {
            const parts = duration.split(':').map(p => parseInt(p, 10) || 0);
            const hours = parts[0] || 0;
            const minutes = parts[1] || 0;
            return hours * 60 + minutes;
        }

        // Handle minutes as number
        const parsed = parseInt(duration, 10);
        return isNaN(parsed) ? 60 : parsed;
    }

    /**
     * Format person name from parts
     */
    private formatPersonName(
        firstName: string | null | undefined,
        lastName: string | null | undefined, 
        surname: string | null | undefined
    ): string {
        // In Ukrainian format: LastName FirstName Surname
        const parts = [lastName, firstName, surname]
            .filter(Boolean)
            .map(part => part?.trim())
            .filter(part => part && part.length > 0);
            
        return parts.length > 0 ? parts.join(' ') : 'Пацієнт не вказан';
    }

    /**
     * Format services list
     */
    private formatServices(services: any[] | null | undefined): string {
        if (!services || !Array.isArray(services) || services.length === 0) {
            return 'Послуги не вказані';
        }

        return services
            .map(service => service?.name || 'Невідома послуга')
            .filter(Boolean)
            .join(', ');
    }

    /**
     * Get status theme
     */
    getStatusTheme(status: AppointmentStatus): StatusTheme {
        return this.statusThemes[status] || this.statusThemes['Scheduled'];
    }

    /**
     * Get all status themes for legend
     */
    getAllStatusThemes(): { status: AppointmentStatus; theme: StatusTheme }[] {
        return Object.entries(this.statusThemes).map(([status, theme]) => ({
            status: status as AppointmentStatus,
            theme
        }));
    }

    /**
     * Create tooltip content for event
     */
    getEventTooltip(event: AppointmentCalendarEvent): string {
        const { patientName, dentistName, services, duration } = event.meta;
        const startTime = event.start.toLocaleTimeString('uk-UA', { 
            hour: '2-digit', 
            minute: '2-digit' 
        });
        const endTime = event.end?.toLocaleTimeString('uk-UA', { 
            hour: '2-digit', 
            minute: '2-digit' 
        });

        return [
            `👤 ${patientName}`,
            `👨‍⚕️ ${dentistName}`,
            `🕒 ${startTime} - ${endTime} (${duration} хв)`,
            `🔧 ${services}`
        ].join('\n');
    }

    /**
     * Validate appointment time slot
     */
    isValidTimeSlot(start: Date, end: Date, excludeEventId?: string): boolean {
        // Basic validation - can be extended
        const duration = (end.getTime() - start.getTime()) / (1000 * 60);
        
        // Minimum 15 minutes, maximum 8 hours
        return duration >= 15 && duration <= 480;
    }

    /**
     * Format comprehensive event title
     */
    private formatEventTitle(
        patientName: string, 
        dentistName: string, 
        services: any[] | null | undefined,
        totalPrice: number,
        status: AppointmentStatus
    ): string {
        const lines: string[] = [];
        
        // Patient name (main info)
        lines.push(`👤 ${this.shortenName(patientName)}`);
        
        // Doctor name (secondary info)
        lines.push(`👨‍⚕️ ${this.shortenName(dentistName)}`);
        
        // Price info for completed/paid appointments
        if ((status === 'Completed' || status === 'Paid') && services && services.length > 0) {
            if (totalPrice > 0) {
                lines.push(`💰 ${totalPrice} ₴`);
            } else {
                // If no total price calculated, calculate it on the fly
                const calculatedPrice = this.calculateTotalPrice(services);
                if (calculatedPrice > 0) {
                    lines.push(`💰 ${calculatedPrice} ₴`);
                }
            }
        }
        
        // Join with HTML line breaks
        return lines.join('<br>');
    }

    /**
     * Shorten name for compact display
     */
    private shortenName(fullName: string): string {
        if (!fullName || fullName === 'Пацієнт не вказан' || fullName === 'Невідомий лікар') {
            return fullName;
        }
        
        const parts = fullName.split(' ').filter(Boolean);
        if (parts.length === 0) return fullName;
        
        // Show: "Surname N.N." format for compact display
        if (parts.length >= 2) {
            const surname = parts[0];
            const initials = parts.slice(1)
                .map(part => part.charAt(0).toUpperCase() + '.')
                .join('');
            return `${surname} ${initials}`;
        }
        
        return parts[0];
    }

    /**
     * Calculate total price of services
     */
    private calculateTotalPrice(services: any[] | null | undefined): number {
        if (!services || !Array.isArray(services)) {
            return 0;
        }
        
        const total = services.reduce((total, service) => {
            return total + (service?.price || 0);
        }, 0);
        
        return total;
    }

    /**
     * Load service prices by IDs and calculate total
     */
    async calculateTotalPriceFromApi(serviceIds: string[]): Promise<number> {
        if (!serviceIds || serviceIds.length === 0) {
            return 0;
        }

        try {
            // Load full service information for each ID
            const serviceRequests = serviceIds.map(id => 
                this._apiClient.client.api.services.byId(id).get().then(
                    (response: any) => response,
                    () => null // Return null on error
                )
            );

            const services = await Promise.all(serviceRequests);
            
            // Calculate total from loaded services
            const total = services
                .filter(service => service && service.price)
                .reduce((sum, service) => sum + (service.price || 0), 0);

            return total;
        } catch (error) {
            console.error('Error loading service prices:', error);
            return 0;
        }
    }

    /**
     * Enhanced calculate total price that tries API if needed
     */
    async calculateEnhancedTotalPrice(services: any[] | null | undefined): Promise<number> {
        // First try direct calculation
        const directTotal = this.calculateTotalPrice(services);
        if (directTotal > 0) {
            return directTotal;
        }

        // If no prices in services, try loading from API
        if (services && services.length > 0) {
            const serviceIds = services
                .map(service => service?.id)
                .filter(Boolean);
            
            if (serviceIds.length > 0) {
                return await this.calculateTotalPriceFromApi(serviceIds);
            }
        }

        return 0;
    }
} 