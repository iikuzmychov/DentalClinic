import { inject, Injectable } from '@angular/core';
import { Role, RoleObject } from 'app/api/models';
import { UserService } from 'app/core/user/user.service';
import { map, Observable } from 'rxjs';

@Injectable({providedIn: 'root'})
export class RoleService
{
    private _userService = inject(UserService);

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Get current user role
     */
    getCurrentRole(): Observable<Role | undefined>
    {
        return this._userService.user$.pipe(
            map(user => user?.role)
        );
    }

    /**
     * Check if current user has specific role
     */
    hasRole(role: Role): Observable<boolean>
    {
        return this.getCurrentRole().pipe(
            map(currentRole => currentRole === role)
        );
    }

    /**
     * Check if current user is admin
     */
    isAdmin(): Observable<boolean>
    {
        return this.hasRole(RoleObject.Admin);
    }

    /**
     * Check if current user is dentist
     */
    isDentist(): Observable<boolean>
    {
        return this.hasRole(RoleObject.Dentist);
    }

    /**
     * Check if current user is receptionist
     */
    isReceptionist(): Observable<boolean>
    {
        return this.hasRole(RoleObject.Receptionist);
    }

    /**
     * Check if current user has admin or dentist role
     */
    isAdminOrDentist(): Observable<boolean>
    {
        return this.getCurrentRole().pipe(
            map(role => role === RoleObject.Admin || role === RoleObject.Dentist)
        );
    }

    /**
     * Check if current user has admin or receptionist role
     */
    isAdminOrReceptionist(): Observable<boolean>
    {
        return this.getCurrentRole().pipe(
            map(role => role === RoleObject.Admin || role === RoleObject.Receptionist)
        );
    }

    // -----------------------------------------------------------------------------------------------------
    // @ SERVICES permissions
    // -----------------------------------------------------------------------------------------------------

    /**
     * Can create services (Admin only)
     */
    canCreateServices(): Observable<boolean>
    {
        return this.isAdmin();
    }

    /**
     * Can edit services (Admin only)
     */
    canEditServices(): Observable<boolean>
    {
        return this.isAdmin();
    }

    /**
     * Can delete services (Admin only)
     */
    canDeleteServices(): Observable<boolean>
    {
        return this.isAdmin();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ PATIENTS permissions
    // -----------------------------------------------------------------------------------------------------

    /**
     * Can create patients (Admin + Receptionist)
     */
    canCreatePatients(): Observable<boolean>
    {
        return this.isAdminOrReceptionist();
    }

    /**
     * Can edit patients (Admin + Receptionist)
     */
    canEditPatients(): Observable<boolean>
    {
        return this.isAdminOrReceptionist();
    }

    /**
     * Can delete patients (Admin only)
     */
    canDeletePatients(): Observable<boolean>
    {
        return this.isAdmin();
    }

    // -----------------------------------------------------------------------------------------------------
    // @ APPOINTMENTS permissions
    // -----------------------------------------------------------------------------------------------------

    /**
     * Can create appointments (Admin + Receptionist)
     */
    canCreateAppointments(): Observable<boolean>
    {
        return this.isAdminOrReceptionist();
    }

    /**
     * Can edit appointments (Admin + Receptionist)
     */
    canEditAppointments(): Observable<boolean>
    {
        return this.isAdminOrReceptionist();
    }

    /**
     * Can cancel appointments (Admin + Receptionist)
     */
    canCancelAppointments(): Observable<boolean>
    {
        return this.isAdminOrReceptionist();
    }

    /**
     * Can complete appointments (Admin + Dentist)
     */
    canCompleteAppointments(): Observable<boolean>
    {
        return this.isAdminOrDentist();
    }

    /**
     * Can process payment for appointments (Admin + Receptionist)
     */
    canPayAppointments(): Observable<boolean>
    {
        return this.isAdminOrReceptionist();
    }

    /**
     * Can delete appointments (Admin + Receptionist)
     */
    canDeleteAppointments(): Observable<boolean>
    {
        return this.isAdminOrReceptionist();
    }

    /**
     * Get role display name in Ukrainian
     */
    getRoleDisplayName(role: Role | undefined): string
    {
        if (!role) return 'Невідома роль';
        
        switch (role) {
            case RoleObject.Admin:
                return 'Адміністратор';
            case RoleObject.Dentist:
                return 'Стоматолог';
            case RoleObject.Receptionist:
                return 'Реєстратор';
            default:
                return 'Невідома роль';
        }
    }
} 