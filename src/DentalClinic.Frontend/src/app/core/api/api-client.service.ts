import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FetchRequestAdapter } from '@microsoft/kiota-http-fetchlibrary';
import { createApiClient, type ApiClient } from 'app/api/apiClient';
import { JwtAuthenticationProvider } from './jwt-auth-provider';

@Injectable({
    providedIn: 'root'
})
export class ApiClientService {
    private readonly _apiClient: ApiClient;

    constructor(private jwtAuthProvider: JwtAuthenticationProvider) {
        // Create a request adapter using fetch with JWT authentication
        const requestAdapter = new FetchRequestAdapter(this.jwtAuthProvider);
        
        // Set the base URL - empty string since generated paths already include /api
        requestAdapter.baseUrl = "";
        
        // Create the API client using the generated factory function
        this._apiClient = createApiClient(requestAdapter);
    }

    /**
     * Get the generated API client instance
     */
    get client(): ApiClient {
        return this._apiClient;
    }
} 