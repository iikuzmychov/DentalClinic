import { Injectable } from '@angular/core';
import { AuthenticationProvider, RequestInformation } from '@microsoft/kiota-abstractions';

@Injectable({
    providedIn: 'root'
})
export class JwtAuthenticationProvider implements AuthenticationProvider {
    
    constructor() {}

    /**
     * Authenticates the provided request information.
     */
    async authenticateRequest(
        requestInformation: RequestInformation, 
        additionalAuthenticationContext?: Record<string, any>
    ): Promise<void> {
        if (!requestInformation) {
            throw new Error('Request information cannot be null');
        }

        // Get the access token from localStorage
        const accessToken = localStorage.getItem('accessToken');
        console.log('🔐 JWT Auth Provider - Access token from localStorage:', accessToken ? `${accessToken.substring(0, 20)}...` : 'null');
        
        if (accessToken && !this.isTokenExpired(accessToken)) {
            // Add Bearer token to the Authorization header
            requestInformation.headers.add('Authorization', `Bearer ${accessToken}`);
            console.log('✅ JWT Auth Provider - Added Authorization header');
        } else if (accessToken && this.isTokenExpired(accessToken)) {
            console.log('⚠️ JWT Auth Provider - Token is expired');
        } else {
            console.log('❌ JWT Auth Provider - No token available');
        }

        // Log all headers for debugging
        console.log('📋 JWT Auth Provider - Request headers:', requestInformation.headers);
    }

    /**
     * Check if token is expired
     */
    private isTokenExpired(token: string): boolean {
        try {
            // Split the token to get the payload part
            const parts = token.split('.');
            if (parts.length !== 3) {
                console.log('❌ JWT Auth Provider - Invalid token format');
                return true;
            }

            // Get the payload (second part) and fix base64url padding
            let payload = parts[1];
            payload = payload.replace(/-/g, '+').replace(/_/g, '/');
            while (payload.length % 4) {
                payload += '=';
            }

            const decodedPayload = JSON.parse(atob(payload));
            const currentTime = Math.floor(Date.now() / 1000);
            const isExpired = decodedPayload.exp < currentTime;
            console.log(`🕒 JWT Auth Provider - Token expiry check: exp=${decodedPayload.exp}, now=${currentTime}, expired=${isExpired}`);
            return isExpired;
        } catch (error) {
            console.log('❌ JWT Auth Provider - Error parsing token:', error);
            return true;
        }
    }
} 