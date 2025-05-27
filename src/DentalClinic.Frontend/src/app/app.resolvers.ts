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
                id: 'example',
                title: 'Example',
                type: 'basic',
                link: '/example'
            }
        ],
        horizontal: [
            {
                id: 'example',
                title: 'Example',
                type: 'basic',
                link: '/example'
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
