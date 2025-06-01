import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { RoleService } from 'app/core/auth/role.service';
import { map, take } from 'rxjs';

export const adminGuard: CanActivateFn = (route, state) => {
    const roleService = inject(RoleService);
    const router = inject(Router);

    return roleService.isAdmin().pipe(
        take(1),
        map(isAdmin => {
            if (!isAdmin) {
                // Redirect to appointments page if not admin
                router.navigate(['/appointments']);
                return false;
            }
            return true;
        })
    );
}; 