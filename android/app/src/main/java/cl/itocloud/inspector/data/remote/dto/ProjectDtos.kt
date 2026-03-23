package cl.itocloud.inspector.data.remote.dto

import com.google.gson.annotations.SerializedName

data class ProjectDto(
    @SerializedName("id") val id: String,
    @SerializedName("code") val code: String,
    @SerializedName("name") val name: String,
    @SerializedName("status") val status: String,
    @SerializedName("companyName") val companyName: String?,
    @SerializedName("startDate") val startDate: String?
)
