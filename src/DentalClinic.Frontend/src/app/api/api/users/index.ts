/* tslint:disable */
/* eslint-disable */
// Generated by Microsoft Kiota
// @ts-ignore
import { createAddUserResponseFromDiscriminatorValue, createConflictFromDiscriminatorValue, createHttpValidationProblemDetailsFromDiscriminatorValue, createListUsersResponseFromDiscriminatorValue, serializeAddUserRequest, serializeAddUserResponse, type AddUserRequest, type AddUserResponse, type Conflict, type HttpValidationProblemDetails, type ListUsersResponse } from '../../models/index';
// @ts-ignore
import { type UsersItemRequestBuilder, UsersItemRequestBuilderRequestsMetadata } from './item/index';
// @ts-ignore
import { type BaseRequestBuilder, type Guid, type KeysToExcludeForNavigationMetadata, type NavigationMetadata, type Parsable, type ParsableFactory, type RequestConfiguration, type RequestInformation, type RequestsMetadata } from '@microsoft/kiota-abstractions';

/**
 * Builds and executes requests for operations under /api/users
 */
export interface UsersRequestBuilder extends BaseRequestBuilder<UsersRequestBuilder> {
    /**
     * Gets an item from the ApiSdk.api.users.item collection
     * @param id Unique identifier of the item
     * @returns {UsersItemRequestBuilder}
     */
     byId(id: Guid) : UsersItemRequestBuilder;
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {Promise<ListUsersResponse>}
     * @throws {HttpValidationProblemDetails} error when the service returns a 400 status code
     */
     get(requestConfiguration?: RequestConfiguration<UsersRequestBuilderGetQueryParameters> | undefined) : Promise<ListUsersResponse | undefined>;
    /**
     * @param body The request body
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {Promise<AddUserResponse>}
     * @throws {Conflict} error when the service returns a 409 status code
     */
     post(body: AddUserRequest, requestConfiguration?: RequestConfiguration<object> | undefined) : Promise<AddUserResponse | undefined>;
    /**
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {RequestInformation}
     */
     toGetRequestInformation(requestConfiguration?: RequestConfiguration<UsersRequestBuilderGetQueryParameters> | undefined) : RequestInformation;
    /**
     * @param body The request body
     * @param requestConfiguration Configuration for the request such as headers, query parameters, and middleware options.
     * @returns {RequestInformation}
     */
     toPostRequestInformation(body: AddUserRequest, requestConfiguration?: RequestConfiguration<object> | undefined) : RequestInformation;
}
export interface UsersRequestBuilderGetQueryParameters {
    name?: string;
    pageIndex?: number;
    pageSize?: number;
    role?: string;
}
/**
 * Uri template for the request builder.
 */
export const UsersRequestBuilderUriTemplate = "{+baseurl}/api/users{?name*,pageIndex*,pageSize*,role*}";
/**
 * Metadata for all the navigation properties in the request builder.
 */
export const UsersRequestBuilderNavigationMetadata: Record<Exclude<keyof UsersRequestBuilder, KeysToExcludeForNavigationMetadata>, NavigationMetadata> = {
    byId: {
        requestsMetadata: UsersItemRequestBuilderRequestsMetadata,
        pathParametersMappings: ["id"],
    },
};
/**
 * Metadata for all the requests in the request builder.
 */
export const UsersRequestBuilderRequestsMetadata: RequestsMetadata = {
    get: {
        uriTemplate: UsersRequestBuilderUriTemplate,
        responseBodyContentType: "application/json, text/plain;q=0.9",
        errorMappings: {
            400: createHttpValidationProblemDetailsFromDiscriminatorValue as ParsableFactory<Parsable>,
        },
        adapterMethodName: "send",
        responseBodyFactory:  createListUsersResponseFromDiscriminatorValue,
    },
    post: {
        uriTemplate: UsersRequestBuilderUriTemplate,
        responseBodyContentType: "application/json, text/plain;q=0.9",
        errorMappings: {
            409: createConflictFromDiscriminatorValue as ParsableFactory<Parsable>,
        },
        adapterMethodName: "send",
        responseBodyFactory:  createAddUserResponseFromDiscriminatorValue,
        requestBodyContentType: "application/json",
        requestBodySerializer: serializeAddUserRequest,
        requestInformationContentSetMethod: "setContentFromParsable",
    },
};
/* tslint:enable */
/* eslint-enable */
