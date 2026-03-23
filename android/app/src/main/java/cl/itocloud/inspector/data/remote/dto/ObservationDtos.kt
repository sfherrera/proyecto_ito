package cl.itocloud.inspector.data.remote.dto

import com.google.gson.annotations.SerializedName

data class ObservationDto(
    @SerializedName("id") val id: String,
    @SerializedName("code") val code: String,
    @SerializedName("title") val title: String,
    @SerializedName("status") val status: String,
    @SerializedName("severity") val severity: String,
    @SerializedName("dueDate") val dueDate: String?,
    @SerializedName("isOverdue") val isOverdue: Boolean
)

data class CreateObservationRequest(
    @SerializedName("projectId") val projectId: String,
    @SerializedName("title") val title: String,
    @SerializedName("description") val description: String?,
    @SerializedName("severity") val severity: String,
    @SerializedName("category") val category: String?,
    @SerializedName("locationDescription") val locationDescription: String?,
    @SerializedName("contractorId") val contractorId: String?,
    @SerializedName("assignedToId") val assignedToId: String?,
    @SerializedName("dueDate") val dueDate: String?,
    @SerializedName("inspectionId") val inspectionId: String? = null,
    @SerializedName("answerId") val answerId: String? = null,
    @SerializedName("specialtyId") val specialtyId: String? = null,
    @SerializedName("sectorId") val sectorId: String? = null,
    @SerializedName("unitId") val unitId: String? = null,
    @SerializedName("rootCause") val rootCause: String? = null
)

data class CreatedIdResponse(
    @SerializedName("id") val id: String
)
