import { inject } from '@angular/core';
import { RoleBasedNavigationService } from 'app/core/navigation/role-based-navigation.service';
import { of } from 'rxjs';

export const initialDataResolver = () =>
{
    const roleBasedNavigationService = inject(RoleBasedNavigationService);
    
    // Initialize navigation based on current user role
    roleBasedNavigationService.initializeNavigation();
    
    // Return observable to satisfy resolver requirements
    return of(true);
};
