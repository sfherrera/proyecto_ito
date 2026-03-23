package cl.itocloud.inspector.data.remote.dto

import com.google.gson.annotations.SerializedName

data class TemplateDetailDto(
    @SerializedName("id") val id: String,
    @SerializedName("name") val name: String,
    @SerializedName("templateType") val templateType: String?,
    @SerializedName("status") val status: String,
    @SerializedName("passingScore") val passingScore: Double?,
    @SerializedName("description") val description: String?,
    @SerializedName("requireGeolocation") val requireGeolocation: Boolean,
    @SerializedName("requireSignature") val requireSignature: Boolean,
    @SerializedName("allowPartialSave") val allowPartialSave: Boolean,
    @SerializedName("sections") val sections: List<SectionDto>
)
