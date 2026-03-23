package cl.itocloud.inspector.data.remote.api

import cl.itocloud.inspector.data.remote.dto.*
import retrofit2.http.*

/**
 * Retrofit interface for the ITO Cloud backend API.
 * All methods are suspend functions for coroutine-based calls.
 */
interface ItoApi {

    // ── Auth ────────────────────────────────────────────────────────────────────

    @POST("api/auth/login")
    suspend fun login(@Body request: LoginRequest): ApiResponse<LoginResponse>

    @GET("api/auth/me")
    suspend fun getMe(): ApiDataResponse<UserResponse>

    @POST("api/auth/change-password")
    suspend fun changePassword(@Body request: ChangePasswordRequest): ApiResponse<MessageResponse>

    // ── Inspections ─────────────────────────────────────────────────────────────

    @GET("api/inspections/mine")
    suspend fun getMyInspections(
        @Query("status") status: String? = null
    ): ApiDataResponse<List<InspectionDto>>

    @GET("api/inspections/{id}")
    suspend fun getInspection(
        @Path("id") id: String,
        @Query("includeAnswers") includeAnswers: Boolean = false
    ): ApiDataResponse<InspectionDetailDto>

    @POST("api/inspections/{id}/start")
    suspend fun startInspection(@Path("id") id: String): ApiDataResponse<Any>

    @POST("api/inspections/{id}/submit")
    suspend fun submitInspection(
        @Path("id") id: String,
        @Body dto: SubmitInspectionRequest
    ): ApiDataResponse<Any>

    // ── Observations ────────────────────────────────────────────────────────────

    @GET("api/observations")
    suspend fun getObservations(
        @Query("projectId") projectId: String? = null,
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 50
    ): ApiDataListResponse<ObservationDto>

    @POST("api/observations")
    suspend fun createObservation(
        @Body dto: CreateObservationRequest
    ): ApiDataResponse<CreatedIdResponse>

    // ── Projects ────────────────────────────────────────────────────────────────

    @GET("api/projects")
    suspend fun getProjects(
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 100
    ): ApiDataListResponse<ProjectDto>

    // ── Templates ───────────────────────────────────────────────────────────────

    @GET("api/templates/{id}")
    suspend fun getTemplate(
        @Path("id") id: String
    ): ApiDataResponse<TemplateDetailDto>
}
