import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { CalendarEventTimesChangedEvent } from 'angular-calendar';
import { from, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { AppointmentCalendarEvent, CalendarService } from '../services/calendar.service';
import { ApiClientService } from 'app/core/api/api-client.service';
import { UpdateAppointmentRequest } from 'app/api/models';

@Injectable({
    providedIn: 'root'
})
export class CalendarEventsHook {
    
    private readonly _snackBar = inject(MatSnackBar);
    private readonly _dialog = inject(MatDialog);
    private readonly _apiClient = inject(ApiClientService);
    private readonly _calendarService = inject(CalendarService);

    /**
     * Handle event click with detailed information
     */
    handleEventClick(event: AppointmentCalendarEvent): void {
        const { patientName, dentistName } = event.meta;
        
        // Show basic information first
        const snackBarRef = this._snackBar.open(
            `${patientName} → ${dentistName}`,
            'Деталі',
            { 
                duration: 4000
            }
        );

        snackBarRef.onAction().subscribe(() => {
            this.showEventDetails(event);
        });
    }

    /**
     * Show detailed event information
     */
    private showEventDetails(event: AppointmentCalendarEvent): void {
        const { patientName, dentistName, services, duration, status } = event.meta;
        const theme = this._calendarService.getStatusTheme(status);
        
        // Format time
        const startTime = event.start.toLocaleString('uk-UA', {
            weekday: 'long',
            day: 'numeric',
            month: 'long',
            hour: '2-digit',
            minute: '2-digit'
        });

        const endTime = event.end?.toLocaleTimeString('uk-UA', {
            hour: '2-digit',
            minute: '2-digit'
        });

        const message = [
            `📅 ${startTime} - ${endTime}`,
            `👤 Пацієнт: ${patientName}`,
            `👨‍⚕️ Лікар: ${dentistName}`, 
            `🔧 Послуги: ${services}`,
            `⏱️ Тривалість: ${duration} хв`,
            `📊 Статус: ${theme.displayName}`
        ].join('\n');

        this._snackBar.open(message, 'OK', { 
            duration: 8000,
            panelClass: ['appointment-details-snackbar']
        });
    }

    /**
     * Handle event time change (drag & drop)
     */
    handleEventTimeChange(
        changeEvent: CalendarEventTimesChangedEvent,
        onSuccess?: () => void
    ): Observable<boolean> {
        const event = changeEvent.event as AppointmentCalendarEvent;
        const { newStart, newEnd } = changeEvent;
        
        // Validate time slot
        if (!this._calendarService.isValidTimeSlot(newStart, newEnd || newStart, event.id)) {
            this._snackBar.open(
                'Неправильний часовий слот. Мінімум 15 хв, максимум 8 годин.',
                'OK',
                { duration: 3000 }
            );
            return of(false);
        }

        const appointmentId = event.meta.appointment.id;
        if (!appointmentId) {
            this._snackBar.open('Помилка: ID запису не знайдено', 'OK', { duration: 3000 });
            return of(false);
        }

        // Calculate new duration in minutes
        const newDuration = newEnd ? 
            Math.round((newEnd.getTime() - newStart.getTime()) / (1000 * 60)) :
            event.meta.duration;

        // Format duration as HH:mm:ss
        const durationString = this.formatDurationForAPI(newDuration);

        const updateData: UpdateAppointmentRequest = {
            startTime: newStart,
            duration: durationString,
            // Keep existing values
            dentistId: event.meta.appointment.dentist?.id,
            patientId: event.meta.appointment.patient?.id
        };

        this._snackBar.open('Оновлення часу запису...', '', { duration: 1000 });

        return from(this._apiClient.client.api.appointments.byId(String(appointmentId)).put(updateData)).pipe(
            map(() => {
                this._snackBar.open(
                    `Час запису оновлено: ${newStart.toLocaleTimeString('uk-UA', { 
                        hour: '2-digit', 
                        minute: '2-digit' 
                    })}`,
                    'OK',
                    { duration: 3000 }
                );
                
                onSuccess?.();
                return true;
            }),
            catchError((error) => {
                console.error('Error updating appointment time:', error);
                this._snackBar.open(
                    'Помилка оновлення часу запису', 
                    'OK', 
                    { duration: 3000 }
                );
                return of(false);
            })
        );
    }

    /**
     * Handle event resize
     */
    handleEventResize(
        resizeEvent: any,
        onSuccess?: () => void
    ): Observable<boolean> {
        // Directly call time change handler with resize event data
        return this.handleEventTimeChange(resizeEvent, onSuccess);
    }

    /**
     * Format duration in minutes to HH:mm:ss string for API
     */
    private formatDurationForAPI(minutes: number): string {
        const hours = Math.floor(minutes / 60);
        const remainingMinutes = minutes % 60;
        return `${hours.toString().padStart(2, '0')}:${remainingMinutes.toString().padStart(2, '0')}:00`;
    }

    /**
     * Show confirmation for event deletion
     */
    confirmEventDeletion(event: AppointmentCalendarEvent): Observable<boolean> {
        return new Observable(observer => {
            const snackBarRef = this._snackBar.open(
                `Видалити запис для ${event.meta.patientName}?`,
                'Видалити',
                { 
                    duration: 5000,
                    panelClass: ['delete-confirmation-snackbar']
                }
            );

            snackBarRef.onAction().subscribe(() => {
                observer.next(true);
                observer.complete();
            });

            snackBarRef.afterDismissed().subscribe(() => {
                observer.next(false);
                observer.complete();
            });
        });
    }
} 