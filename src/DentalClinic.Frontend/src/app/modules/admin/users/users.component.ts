import { NgIf, NgFor } from '@angular/common';
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { from } from 'rxjs';
import { type UsersRequestBuilderGetQueryParameters } from 'app/api/api/users';
import { ApiClientService } from 'app/core/api/api-client.service';
import { 
    ListUsersResponse, 
    ListUsersResponseItem, 
    AddUserRequest, 
    UpdateUserRequest,
    Role,
    RoleObject
} from 'app/api/models';
import { UserDialogComponent, UserDialogData } from './user-dialog/user-dialog.component';
import { DeleteConfirmationDialogComponent, DeleteConfirmationData } from '../services/delete-confirmation-dialog/delete-confirmation-dialog.component';

@Component({
    selector     : 'users',
    standalone   : true,
    templateUrl  : './users.component.html',
    encapsulation: ViewEncapsulation.None,
    imports: [
        NgIf,
        NgFor,
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatDialogModule,
        MatIconModule,
        MatMenuModule,
        MatTableModule,
        MatInputModule,
        MatFormFieldModule,
        MatSelectModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        MatTooltipModule
    ]
})
export class UsersComponent implements OnInit {
    users: ListUsersResponseItem[] = [];
    totalCount: number = 0;
    totalPages: number = 0;
    isLoading: boolean = false;
    isInitialLoad: boolean = true;
    searchTerm: string = '';
    previousSearchTerm: string = '';
    lastCompletedSearchTerm: string = '';
    selectedRole: string = ''; // для фильтра роли
    displayedColumns: string[] = ['fullName', 'email', 'phoneNumber', 'role', 'actions'];
    
    // Локализованные роли
    roleOptions = [
        { value: '', label: 'Всі ролі' },
        { value: RoleObject.Admin, label: 'Адміністратор' },
        { value: RoleObject.Dentist, label: 'Лікар' },
        { value: RoleObject.Receptionist, label: 'Менеджер' }
    ];
    
    private searchTimeout: any;

    constructor(
        private apiClient: ApiClientService,
        private _dialog: MatDialog,
        private _snackBar: MatSnackBar
    ) {}

    ngOnInit(): void {
        this.loadUsers();
    }

    loadUsers(): void {
        this.isLoading = true;
        const requestConfiguration = {
            queryParameters: {
                name: this.searchTerm || undefined,
                role: this.selectedRole || undefined,
                pageIndex: 0,
                pageSize: 20
            } as UsersRequestBuilderGetQueryParameters
        };
        from(this.apiClient.client.api.users.get(requestConfiguration)).subscribe({
            next: (response: ListUsersResponse) => {
                this.users = response?.items || [];
                this.totalCount = response?.totalCount || 0;
                this.totalPages = response?.totalPagesCount || 0;
                this.isLoading = false;
                this.isInitialLoad = false;
                this.previousSearchTerm = this.searchTerm;
                this.lastCompletedSearchTerm = this.searchTerm;
            },
            error: (error) => {
                console.error('Error loading users:', error);
                this._snackBar.open('Помилка завантаження користувачів', 'OK', { duration: 3000 });
                this.isLoading = false;
                this.isInitialLoad = false;
                this.previousSearchTerm = this.searchTerm;
                this.lastCompletedSearchTerm = this.searchTerm;
            }
        });
    }

    onSearch(): void {
        this.loadUsers();
    }

    onSearchInput(): void {
        if (this.searchTimeout) {
            clearTimeout(this.searchTimeout);
        }
        this.searchTimeout = setTimeout(() => {
            this.previousSearchTerm = this.searchTerm;
            this.onSearch();
        }, 300);
    }

    onRoleFilterChange(): void {
        this.loadUsers();
    }

    // Локализация роли для отображения
    getRoleLabel(role: Role | null | undefined): string {
        if (!role) return '—';
        const option = this.roleOptions.find(r => r.value === role);
        return option ? option.label : role.toString();
    }

    // CSS классы для значков ролей
    getRoleClass(role: Role | null | undefined): string {
        if (!role) return 'bg-gray-100 text-gray-800';
        
        switch (role) {
            case RoleObject.Admin:
                return 'bg-red-100 text-red-800';
            case RoleObject.Dentist:
                return 'bg-blue-100 text-blue-800';
            case RoleObject.Receptionist:
                return 'bg-purple-100 text-purple-800';
            default:
                return 'bg-gray-100 text-gray-800';
        }
    }

    // Получить полное имя пользователя
    getFullName(user: ListUsersResponseItem): string {
        const parts = [user.lastName, user.firstName, user.surname].filter(Boolean);
        return parts.join(' ') || 'Невідомий користувач';
    }

    // Track by function для ngFor
    trackByFn(index: number, item: any): any {
        return item.id || index;
    }

    // Добавить пользователя
    addUser(): void {
        const dialogData: UserDialogData = {
            mode: 'add'
        };

        const dialogRef = this._dialog.open(UserDialogComponent, {
            width: '800px',
            maxHeight: '90vh',
            data: dialogData,
            disableClose: true
        });

        dialogRef.afterClosed().subscribe((result) => {
            if (result && result.action === 'add') {
                this.createUser(result.data);
            }
        });
    }

    // Редактировать пользователя
    editUser(event: Event, user: ListUsersResponseItem): void {
        event.stopPropagation();
        
        const dialogData: UserDialogData = {
            user: user,
            mode: 'edit'
        };

        const dialogRef = this._dialog.open(UserDialogComponent, {
            width: '800px',
            maxHeight: '90vh',
            data: dialogData,
            disableClose: true
        });

        dialogRef.afterClosed().subscribe((result) => {
            if (result && result.action === 'update') {
                this.updateUser(user.id!, result.data);
            }
        });
    }

    // Просмотр пользователя
    viewUser(user: ListUsersResponseItem): void {
        const dialogData: UserDialogData = {
            user: user,
            mode: 'view'
        };

        const dialogRef = this._dialog.open(UserDialogComponent, {
            width: '800px',
            maxHeight: '90vh',
            data: dialogData,
            disableClose: true
        });

        dialogRef.afterClosed().subscribe((result) => {
            if (result && result.action === 'update') {
                this.updateUser(user.id!, result.data);
            }
        });
    }

    // Удалить пользователя
    deleteUser(event: Event, user: ListUsersResponseItem): void {
        event.stopPropagation();
        
        const dialogData: DeleteConfirmationData = {
            title: 'Видалення користувача',
            message: `Ви впевнені, що хочете видалити користувача "${this.getFullName(user)}"?`,
            confirmText: 'Видалити'
        };

        const dialogRef = this._dialog.open(DeleteConfirmationDialogComponent, {
            width: '400px',
            data: dialogData
        });

        dialogRef.afterClosed().subscribe((confirmed) => {
            if (confirmed) {
                this.deleteUserById(user.id!);
            }
        });
    }

    // API вызовы
    private async createUser(userData: AddUserRequest): Promise<void> {
        try {
            await this.apiClient.client.api.users.post(userData);
            this._snackBar.open('Користувача успішно створено', 'OK', { duration: 3000 });
            this.loadUsers();
        } catch (error) {
            console.error('Error creating user:', error);
            this._snackBar.open('Помилка створення користувача', 'OK', { duration: 3000 });
        }
    }

    private async updateUser(userId: string, userData: UpdateUserRequest): Promise<void> {
        try {
            await this.apiClient.client.api.users.byId(userId).put(userData);
            this._snackBar.open('Користувача успішно оновлено', 'OK', { duration: 3000 });
            this.loadUsers();
        } catch (error) {
            console.error('Error updating user:', error);
            this._snackBar.open('Помилка оновлення користувача', 'OK', { duration: 3000 });
        }
    }

    private async deleteUserById(userId: string): Promise<void> {
        try {
            await this.apiClient.client.api.users.byId(userId).delete();
            this._snackBar.open('Користувача успішно видалено', 'OK', { duration: 3000 });
            this.loadUsers();
        } catch (error) {
            console.error('Error deleting user:', error);
            this._snackBar.open('Помилка видалення користувача', 'OK', { duration: 3000 });
        }
    }
} 