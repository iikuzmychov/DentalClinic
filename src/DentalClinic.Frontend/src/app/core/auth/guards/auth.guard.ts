import { inject } from '@angular/core';
import { CanActivateChildFn, CanActivateFn, Router } from '@angular/router';
import { AuthService } from 'app/core/auth/auth.service';
import { of, switchMap } from 'rxjs';

export const AuthGuard: CanActivateFn | CanActivateChildFn = (route, state) =>
{
    const router: Router = inject(Router);

    console.log('AuthGuard called for route:', state.url);

    // Check the authentication status
    return inject(AuthService).check().pipe(
        switchMap((authenticated) =>
        {
            console.log('AuthGuard - authenticated:', authenticated);
            
            // If the user is not authenticated...
            if ( !authenticated )
            {
                console.log('AuthGuard - redirecting to sign-in');
                // Redirect to the sign-in page with a redirectUrl param
                const redirectURL = state.url === '/sign-out' ? '' : `redirectURL=${state.url}`;
                const urlTree = router.parseUrl(`sign-in?${redirectURL}`);

                return of(urlTree);
            }

            // Allow the access
            console.log('AuthGuard - allowing access');
            return of(true);
        }),
    );
};
