/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
// @ts-ignore
import { createGetPatientResponseFromDiscriminatorValue, createHttpValidationProblemDetailsFromDiscriminatorValue, serializeUpdatePatientRequest, type GetPatientResponse, type HttpValidationProblemDetails, type UpdatePatientRequest } from '../../../models/index';
// @ts-ignore
import { type BaseRequestBuilder, type Parsable, type ParsableFactory, type RequestConfiguration, type RequestInformation, type RequestsMetadata } from '@microsoft/kiota-abstractions';

/**
 * Builds and executes requests for operations under /api/patients/{id}
 */
export interface PatientsItemRequestBuilder extends BaseRequestBuilder<PatientsItemRequestBuilder> {
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @throws {HttpValidationProblemDetails} error when the service returns a 404 status code
     */
     delete(requestConfiguration?: RequestConfiguration<object> | undefined) : Promise<void>;
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {Promise<GetPatientResponse>}
     * @throws {HttpValidationProblemDetails} error when the service returns a 404 status code
     */
     get(requestConfiguration?: RequestConfiguration<object> | undefined) : Promise<GetPatientResponse | undefined>;
    /**
     * @param body The request body
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @throws {HttpValidationProblemDetails} error when the service returns a 404 status code
     * @throws {HttpValidationProblemDetails} error when the service returns a 409 status code
     */
     put(body: UpdatePatientRequest, requestConfiguration?: RequestConfiguration<object> | undefined) : Promise<void>;
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {RequestInformation}
     */
     toDeleteRequestInformation(requestConfiguration?: RequestConfiguration<object> | undefined) : RequestInformation;
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {RequestInformation}
     */
     toGetRequestInformation(requestConfiguration?: RequestConfiguration<object> | undefined) : RequestInformation;
    /**
     * @param body The request body
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {RequestInformation}
     */
     toPutRequestInformation(body: UpdatePatientRequest, requestConfiguration?: RequestConfiguration<object> | undefined) : RequestInformation;
}
/**
 * Uri template for the request builder.
 */
export const PatientsItemRequestBuilderUriTemplate = "{+baseurl}/api/patients/{id}";
/**
 * Metadata for all the requests in the request builder.
 */
export const PatientsItemRequestBuilderRequestsMetadata: RequestsMetadata = {
    delete: {
        uriTemplate: PatientsItemRequestBuilderUriTemplate,
        errorMappings: {
            404: createHttpValidationProblemDetailsFromDiscriminatorValue as ParsableFactory<Parsable>,
        },
        adapterMethodName: "sendNoResponseContent",
    },
    get: {
        uriTemplate: PatientsItemRequestBuilderUriTemplate,
        responseBodyContentType: "application/json, text/plain;q=0.9",
        errorMappings: {
            404: createHttpValidationProblemDetailsFromDiscriminatorValue as ParsableFactory<Parsable>,
        },
        adapterMethodName: "send",
        responseBodyFactory:  createGetPatientResponseFromDiscriminatorValue,
    },
    put: {
        uriTemplate: PatientsItemRequestBuilderUriTemplate,
        errorMappings: {
            404: createHttpValidationProblemDetailsFromDiscriminatorValue as ParsableFactory<Parsable>,
            409: createHttpValidationProblemDetailsFromDiscriminatorValue as ParsableFactory<Parsable>,
        },
        adapterMethodName: "sendNoResponseContent",
        requestBodyContentType: "application/json",
        requestBodySerializer: serializeUpdatePatientRequest,
        requestInformationContentSetMethod: "setContentFromParsable",
    },
};
/* tslint:enable */
/* eslint-enable */
