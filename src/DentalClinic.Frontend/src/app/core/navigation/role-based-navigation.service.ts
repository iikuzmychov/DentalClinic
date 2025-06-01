import { inject, Injectable } from '@angular/core';
import { NavigationService } from 'app/core/navigation/navigation.service';
import { RoleService } from 'app/core/auth/role.service';
import { Navigation } from 'app/core/navigation/navigation.types';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { take } from 'rxjs';

@Injectable({providedIn: 'root'})
export class RoleBasedNavigationService
{
    private _navigationService = inject(NavigationService);
    private _roleService = inject(RoleService);

    constructor() {
        // Subscribe to role changes and update navigation
        this._roleService.getCurrentRole().pipe(
            takeUntilDestroyed()
        ).subscribe((role) => {
            console.log('🔄 Role changed to:', role);
            this.updateNavigation();
        });
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Initialize navigation based on current user role
     */
    initializeNavigation(): void
    {
        console.log('🎯 Initializing navigation...');
        this.updateNavigation();
    }

    /**
     * Update navigation based on current user role
     */
    private updateNavigation(): void
    {
        this._roleService.isAdmin().pipe(take(1)).subscribe(isAdmin => {
            console.log('👤 User is admin:', isAdmin);
            const navigation = this.buildNavigation(isAdmin);
            this._navigationService.updateNavigation(navigation);
            console.log('🧭 Navigation updated');
        });
    }

    /**
     * Build navigation structure based on role
     */
    private buildNavigation(isAdmin: boolean): Navigation
    {
        // Base navigation items that are always visible
        const baseNavigation: any[] = [
            {
                id: 'appointments',
                title: 'Розклад',
                type: 'basic',
                link: '/appointments'
            },
            {
                id: 'patients',
                title: 'Пацієнти',
                type: 'basic',
                link: '/patients'
            },
            {
                id: 'services',
                title: 'Послуги',
                type: 'basic',
                link: '/services'
            }
        ];

        // Admin-only navigation items
        const adminNavigation: any[] = [
            {
                id: 'divider-1',
                type: 'divider'
            },
            {
                id: 'users',
                title: 'Користувачі',
                type: 'basic',
                link: '/users'
            }
        ];

        const navigation = [...baseNavigation];
        
        // Only add admin items if user is admin
        if (isAdmin) {
            navigation.push(...adminNavigation);
            console.log('🔐 Added admin navigation items');
        } else {
            console.log('🚫 Hiding admin navigation items');
        }
        
        return {
            default: navigation,
            horizontal: navigation,
            compact: [],
            futuristic: []
        };
    }
} 