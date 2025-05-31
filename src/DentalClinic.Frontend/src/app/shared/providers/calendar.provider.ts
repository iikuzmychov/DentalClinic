import { Provider } from '@angular/core';
import { DateAdapter } from 'angular-calendar';
import { CalendarA11y, CalendarDateFormatter, CalendarEventTitleFormatter, CalendarUtils, CalendarNativeDateFormatter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';

/**
 * Custom date formatter for Ukrainian locale and 24-hour time
 */
class UkrainianDateFormatter extends CalendarNativeDateFormatter {
    
    public weekViewHour({ date }: { date: Date; locale?: string }): string {
        return new Intl.DateTimeFormat('uk-UA', {
            hour: 'numeric', 
            minute: '2-digit',
            hour12: false
        }).format(date);
    }
    
    public dayViewHour({ date }: { date: Date; locale?: string }): string {
        return new Intl.DateTimeFormat('uk-UA', {
            hour: 'numeric', 
            minute: '2-digit',
            hour12: false
        }).format(date);
    }

    public weekViewColumnHeader({ date }: { date: Date; locale?: string }): string {
        return new Intl.DateTimeFormat('uk-UA', { 
            weekday: 'long' 
        }).format(date);
    }
    
    public weekViewColumnSubHeader({ date }: { date: Date; locale?: string }): string {
        return new Intl.DateTimeFormat('uk-UA', { 
            day: 'numeric', 
            month: 'short' 
        }).format(date);
    }

    public monthViewColumnHeader({ date }: { date: Date; locale?: string }): string {
        return new Intl.DateTimeFormat('uk-UA', { 
            weekday: 'narrow' 
        }).format(date);
    }
}

/**
 * Providers for Angular Calendar with Ukrainian localization
 */
export const CALENDAR_PROVIDERS: Provider[] = [
    {
        provide: DateAdapter,
        useFactory: adapterFactory
    },
    CalendarUtils,
    CalendarA11y,
    {
        provide: CalendarDateFormatter,
        useClass: UkrainianDateFormatter
    },
    CalendarEventTitleFormatter
]; 