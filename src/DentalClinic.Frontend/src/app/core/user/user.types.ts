import { Role } from 'app/api/models';

export interface User
{
    id: string;
    name: string;
    email: string;
    avatar?: string;
    role?: Role;
}
