import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthUtils } from 'app/core/auth/auth.utils';
import { UserService } from 'app/core/user/user.service';
import { Router } from '@angular/router';
import { catchError, Observable, of, switchMap, throwError } from 'rxjs';

@Injectable({providedIn: 'root'})
export class AuthService
{
    private _authenticated: boolean = false;
    private _httpClient = inject(HttpClient);
    private _userService = inject(UserService);
    private _router = inject(Router);

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
     * Forgot password
     *
     * @param email
     */
    forgotPassword(email: string): Observable<any>
    {
        return this._httpClient.post('api/auth/forgot-password', email);
    }

    /**
     * Reset password
     *
     * @param password
     */
    resetPassword(password: string): Observable<any>
    {
        return this._httpClient.post('api/auth/reset-password', password);
    }

    /**
     * Sign in
     *
     * @param credentials
     */
    signIn(credentials: { email: string; password: string }): Observable<any>
    {
        // Throw error, if the user is already logged in
        if ( this._authenticated )
        {
            return throwError('User is already logged in.');
        }

        return this._httpClient.post('/api/auth/login', credentials).pipe(
            switchMap((response: any) =>
            {
                console.log('Login response:', response);
                
                // Store the access token in the local storage
                this.accessToken = response.token;
                console.log('Stored token:', response.token);

                // Set the authenticated flag to true
                this._authenticated = true;

                // Extract user data from token
                this._setUserFromToken(response.token);

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
        console.log('AuthService.check() called');
        console.log('Access token:', this.accessToken);
        console.log('Token expired:', this.accessToken ? AuthUtils.isTokenExpired(this.accessToken) : 'No token');
        
        // Check if the access token exists and is not expired
        if (!this.accessToken || AuthUtils.isTokenExpired(this.accessToken)) {
            console.log('Authentication failed - no token or token expired');
            this._authenticated = false;
            return of(false);
        }

        // If the access token exists and is valid, set authenticated flag and return true
        console.log('Authentication successful');
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
            console.log('Decoded token:', decodedToken);

            // Extract user information from token
            const user: any = {};
            
            // User ID from sub claim
            if (decodedToken.sub) user.id = decodedToken.sub;
            
            // Email
            if (decodedToken.email) user.email = decodedToken.email;
            
            // Name - build full name from given_name and family_name
            let fullName = '';
            const nameParts = [];
            
            if (decodedToken.given_name) nameParts.push(decodedToken.given_name);
            if (decodedToken.family_name) nameParts.push(decodedToken.family_name);
            
            if (nameParts.length > 0) {
                fullName = nameParts.join(' ');
                user.name = fullName;
            }
            
            // Always set avatar as empty string for icon display
            user.avatar = '';
            
            // Only set user if we have at least some meaningful data
            if (user.id || user.email || user.name) {
                console.log('User extracted from token:', user);
                this._userService.user = user;
            } else {
                console.log('No meaningful user data found in token');
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
