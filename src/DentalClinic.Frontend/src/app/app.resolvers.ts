import { inject } from '@angular/core';
import { NavigationService } from 'app/core/navigation/navigation.service';
import { forkJoin, of, tap } from 'rxjs';

export const initialDataResolver = () =>
{
    const navigationService = inject(NavigationService);
    
    // Create simple navigation data
    const simpleNavigation = {
        default: [
            {
                id: 'appointments',
                title: 'Записи на прийом',
                type: 'basic',
                link: '/appointments'
            },
            {
                id: 'services',
                title: 'Послуги',
                type: 'basic',
                link: '/services'
            },
            {
                id: 'patients',
                title: 'Пацієнти',
                type: 'basic',
                link: '/patients'
            },
            {
                id: 'users',
                title: 'Користувачі',
                type: 'basic',
                link: '/users'
            }
        ],
        horizontal: [
            {
                id: 'appointments',
                title: 'Записи на прийом',
                type: 'basic',
                link: '/appointments'
            },
            {
                id: 'services',
                title: 'Послуги',
                type: 'basic',
                link: '/services'
            },
            {
                id: 'patients',
                title: 'Пацієнти',
                type: 'basic',
                link: '/patients'
            },
            {
                id: 'users',
                title: 'Користувачі',
                type: 'basic',
                link: '/users'
            }
        ],
        compact: [],
        futuristic: []
    };
    
    // Set the navigation data
    return of(simpleNavigation).pipe(
        tap((navigation) => {
            // Manually trigger navigation update
            (navigationService as any)._navigation.next(navigation);
        })
    );
};
