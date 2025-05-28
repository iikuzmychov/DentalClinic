import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
    ListServicesResponse, 
    ListServicesResponseItem, 
    AddServiceRequest, 
    UpdateServiceRequest,
    GetServiceResponse 
} from 'app/api/models';

export interface ServicesListParams {
    name?: string;
    pageIndex?: number;
    pageSize?: number;
}

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private readonly baseUrl = '/api';

    constructor(private _httpClient: HttpClient) {}

    // Services endpoints
    getServices(params?: ServicesListParams): Observable<ListServicesResponse> {
        let url = `${this.baseUrl}/services`;
        const queryParams: string[] = [];

        if (params?.name) {
            queryParams.push(`name=${encodeURIComponent(params.name)}`);
        }
        if (params?.pageIndex !== undefined) {
            queryParams.push(`pageIndex=${params.pageIndex}`);
        }
        if (params?.pageSize !== undefined) {
            queryParams.push(`pageSize=${params.pageSize}`);
        }

        if (queryParams.length > 0) {
            url += `?${queryParams.join('&')}`;
        }

        return this._httpClient.get<ListServicesResponse>(url);
    }

    getService(id: string): Observable<GetServiceResponse> {
        return this._httpClient.get<GetServiceResponse>(`${this.baseUrl}/services/${id}`);
    }

    createService(data: AddServiceRequest): Observable<any> {
        return this._httpClient.post(`${this.baseUrl}/services`, data);
    }

    updateService(id: string, data: UpdateServiceRequest): Observable<any> {
        return this._httpClient.put(`${this.baseUrl}/services/${id}`, data);
    }

    deleteService(id: string): Observable<any> {
        return this._httpClient.delete(`${this.baseUrl}/services/${id}`);
    }
} 