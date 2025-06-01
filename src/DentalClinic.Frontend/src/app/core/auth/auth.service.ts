import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthUtils } from 'app/core/auth/auth.utils';
import { UserService } from 'app/core/user/user.service';
import { Router } from '@angular/router';
import { catchError, Observable, of, switchMap, throwError, from } from 'rxjs';
import { ApiClientService } from 'app/core/api/api-client.service';
import { LoginRequest, LoginResponse } from 'app/api/models';

@Injectable({providedIn: 'root'})
export class AuthService
{
    private _authenticated: boolean = false;
    private _httpClient = inject(HttpClient);
    private _userService = inject(UserService);
    private _router = inject(Router);
    private _apiClient = inject(ApiClientService);

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Setter & getter for access token
     */
    set accessToken(token: string)
    {
        localStorage.setItem('accessToken', token);
    }

    get accessToken(): string
    {
        return localStorage.getItem('accessToken') ?? '';
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Forgot password - TODO: Use generated types when available
     *
     * @param email
     */
    forgotPassword(email: string): Observable<any>
    {
        return this._httpClient.post('api/auth/forgot-password', email);
    }

    /**
     * Reset password - TODO: Use generated types when available
     *
     * @param password
     */
    resetPassword(password: string): Observable<any>
    {
        return this._httpClient.post('api/auth/reset-password', password);
    }

    /**
     * Sign in using generated types and API client
     *
     * @param credentials
     */
    signIn(credentials: { email: string; password: string }): Observable<LoginResponse>
    {
        // Throw error, if the user is already logged in
        if ( this._authenticated )
        {
            return throwError('User is already logged in.');
        }

        // Create login request using generated type
        const loginRequest: LoginRequest = {
            email: credentials.email,
            password: credentials.password
        };

        return from(this._apiClient.client.api.auth.login.post(loginRequest)).pipe(
            switchMap((response: LoginResponse) =>
            {
                // Store the access token in the local storage
                if (response.token) {
                    this.accessToken = response.token;

                    // Set the authenticated flag to true
                    this._authenticated = true;

                    // Extract user data from token
                    this._setUserFromToken(response.token);
                }

                // Return a new observable with the response
                return of(response);
            }),
            catchError((error) =>
            {
                return throwError(error);
            })
        );
    }

    /**
     * Sign out
     */
    signOut(): Observable<any>
    {
        // Remove the access token from the local storage
        localStorage.removeItem('accessToken');

        // Set the authenticated flag to false
        this._authenticated = false;

        // Return the observable
        return of(true);
    }

    /**
     * Sign up
     *
     * @param user
     */
    signUp(user: { name: string; email: string; password: string; company: string }): Observable<any>
    {
        return this._httpClient.post('api/auth/sign-up', user);
    }

    /**
     * Unlock session
     *
     * @param credentials
     */
    unlockSession(credentials: { email: string; password: string }): Observable<any>
    {
        return this._httpClient.post('api/auth/unlock-session', credentials);
    }

    /**
     * Check the authentication status
     */
    check(): Observable<any>
    {
        // Check if the access token exists and is not expired
        if (!this.accessToken || AuthUtils.isTokenExpired(this.accessToken)) {
            this._authenticated = false;
            return of(false);
        }

        // If the access token exists and is valid, set authenticated flag and return true
        this._authenticated = true;
        
        // Extract user data from token if not already set
        this._setUserFromToken(this.accessToken);
        
        return of(true);
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Private methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Set user data from JWT token
     *
     * @param token
     * @private
     */
    private _setUserFromToken(token: string): void
    {
        try {
            // Decode the token to get user data
            const decodedToken = this._decodeJwtToken(token);
            console.log('🔍 AuthService - Decoded token:', decodedToken);

            // Extract user information from token
            const user: any = {};
            
            // User ID from sub claim
            if (decodedToken.sub) user.id = decodedToken.sub;
            
            // Email
            if (decodedToken.email) user.email = decodedToken.email;

            // Role from role field
            if (decodedToken.role) user.role = decodedToken.role;
            
            // Name - build full name from all available name fields
            let fullName = '';
            const nameParts = [];
            
            // Use standard JWT claims first (these are what we actually get in the token)
            // Ukrainian format: family_name given_name middle_name
            if (decodedToken.family_name) nameParts.push(decodedToken.family_name);
            if (decodedToken.given_name) nameParts.push(decodedToken.given_name);
            if (decodedToken.middle_name) nameParts.push(decodedToken.middle_name);
            
            // Fallback to custom Ukrainian fields if standard JWT fields not available
            if (nameParts.length === 0) {
                if (decodedToken.lastName) nameParts.push(decodedToken.lastName);
                if (decodedToken.firstName) nameParts.push(decodedToken.firstName);
                if (decodedToken.surname) nameParts.push(decodedToken.surname);
            }
            
            // Fallback to name field if available
            if (nameParts.length === 0 && decodedToken.name) {
                nameParts.push(decodedToken.name);
            }
            
            if (nameParts.length > 0) {
                fullName = nameParts.join(' ');
                user.name = fullName;
            }
            
            console.log('👤 AuthService - Extracted user data:', user);
            console.log('📝 AuthService - Name parts:', nameParts);
            
            // Always set avatar as empty string for icon display
            user.avatar = '';
            
            // Only set user if we have at least some meaningful data
            if (user.id || user.email || user.name) {
                this._userService.user = user;
            }
        } catch (error) {
            console.error('Error decoding token:', error);
        }
    }

    /**
     * Decode JWT token
     *
     * @param token
     * @private
     */
    private _decodeJwtToken(token: string): any
    {
        if (!token) {
            throw new Error('No token provided');
        }

        const parts = token.split('.');
        if (parts.length !== 3) {
            throw new Error('Invalid JWT token format');
        }

        // Decode the payload (second part)
        const payload = parts[1];
        // Fix base64 padding
        let base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
        while (base64.length % 4) {
            base64 += '=';
        }
        
        // Decode base64 and handle UTF-8
        const decoded = decodeURIComponent(
            atob(base64)
                .split('')
                .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        
        return JSON.parse(decoded);
    }
}
