package cl.itocloud.inspector.data.remote.dto

import com.google.gson.annotations.SerializedName

/**
 * Generic API response wrapper for single-value responses (e.g., login).
 */
data class ApiResponse<T>(
    @SerializedName("success") val success: Boolean,
    @SerializedName("data") val data: T? = null,
    @SerializedName("message") val message: String? = null
)

/**
 * API response wrapper for endpoints that return { success, data }.
 */
data class ApiDataResponse<T>(
    @SerializedName("success") val success: Boolean,
    @SerializedName("data") val data: T? = null,
    @SerializedName("message") val message: String? = null
)

/**
 * API response wrapper for paginated list endpoints.
 */
data class ApiDataListResponse<T>(
    @SerializedName("success") val success: Boolean,
    @SerializedName("data") val data: List<T>? = null,
    @SerializedName("pagination") val pagination: PaginationMeta? = null,
    @SerializedName("message") val message: String? = null
)

/**
 * Pagination metadata returned by the API.
 */
data class PaginationMeta(
    @SerializedName("page") val page: Int,
    @SerializedName("pageSize") val pageSize: Int,
    @SerializedName("totalCount") val totalCount: Int
)
