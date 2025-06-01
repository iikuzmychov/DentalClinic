import { FuseNavigationItem } from '@fuse/components/navigation';

export const defaultNavigation: FuseNavigationItem[] = [
    {
        id   : 'admin',
        title: 'Адміністрація',
        type : 'group',
        icon : 'heroicons_outline:cog-6-tooth',
        children: [
            {
                id   : 'admin.appointments',
                title: 'Розклад',
                type : 'basic',
                icon : 'heroicons_outline:calendar',
                link : '/admin/appointments'
            },
            {
                id   : 'admin.patients',
                title: 'Пацієнти',
                type : 'basic',
                icon : 'heroicons_outline:users',
                link : '/admin/patients'
            },
            {
                id   : 'admin.services',
                title: 'Послуги',
                type : 'basic',
                icon : 'heroicons_outline:wrench-screwdriver',
                link : '/admin/services'
            },
            {
                id   : 'admin.users',
                title: 'Користувачі',
                type : 'basic',
                icon : 'heroicons_outline:user-group',
                link : '/admin/users'
            },
            {
                id   : 'admin.reports',
                title: 'Фінансові звіти',
                type : 'basic',
                icon : 'heroicons_outline:chart-bar',
                link : '/reports'
            }
        ]
    }
]; 