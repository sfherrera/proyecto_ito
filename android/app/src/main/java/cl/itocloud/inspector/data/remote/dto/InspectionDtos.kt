package cl.itocloud.inspector.data.remote.dto

import com.google.gson.annotations.SerializedName

data class InspectionDto(
    @SerializedName("id") val id: String,
    @SerializedName("code") val code: String,
    @SerializedName("title") val title: String,
    @SerializedName("status") val status: String,
    @SerializedName("assignedToName") val assignedToName: String?,
    @SerializedName("scheduledDate") val scheduledDate: String?,
    @SerializedName("score") val score: Double?
)

data class InspectionDetailDto(
    @SerializedName("id") val id: String,
    @SerializedName("projectId") val projectId: String,
    @SerializedName("code") val code: String,
    @SerializedName("title") val title: String,
    @SerializedName("inspectionType") val inspectionType: String,
    @SerializedName("status") val status: String,
    @SerializedName("priority") val priority: String,
    @SerializedName("scheduledDate") val scheduledDate: String?,
    @SerializedName("startedAt") val startedAt: String?,
    @SerializedName("finishedAt") val finishedAt: String?,
    @SerializedName("assignedToName") val assignedToName: String?,
    @SerializedName("contractorName") val contractorName: String?,
    @SerializedName("score") val score: Double?,
    @SerializedName("passed") val passed: Boolean?,
    @SerializedName("totalQuestions") val totalQuestions: Int,
    @SerializedName("answeredQuestions") val answeredQuestions: Int,
    @SerializedName("conformingCount") val conformingCount: Int,
    @SerializedName("nonConformingCount") val nonConformingCount: Int,
    @SerializedName("notes") val notes: String?,
    @SerializedName("templateId") val templateId: String?,
    @SerializedName("sections") val sections: List<SectionDto>?
)

data class SectionDto(
    @SerializedName("id") val id: String,
    @SerializedName("title") val title: String,
    @SerializedName("orderIndex") val orderIndex: Int,
    @SerializedName("questions") val questions: List<QuestionDto>
)

data class QuestionDto(
    @SerializedName("id") val id: String,
    @SerializedName("questionText") val questionText: String,
    @SerializedName("questionType") val questionType: String,
    @SerializedName("isCritical") val isCritical: Boolean,
    @SerializedName("orderIndex") val orderIndex: Int,
    @SerializedName("options") val options: List<QuestionOptionDto>?
)

data class QuestionOptionDto(
    @SerializedName("id") val id: String,
    @SerializedName("label") val label: String,
    @SerializedName("value") val value: String,
    @SerializedName("isFailureOption") val isFailureOption: Boolean,
    @SerializedName("score") val score: Double
)

data class SubmitInspectionRequest(
    @SerializedName("answers") val answers: List<AnswerSubmitDto>,
    @SerializedName("latitude") val latitude: Double? = null,
    @SerializedName("longitude") val longitude: Double? = null,
    @SerializedName("weatherConditions") val weatherConditions: String? = null,
    @SerializedName("notes") val notes: String? = null
)

data class AnswerSubmitDto(
    @SerializedName("questionId") val questionId: String,
    @SerializedName("answerValue") val answerValue: String? = null,
    @SerializedName("isConforming") val isConforming: Boolean? = null,
    @SerializedName("isNa") val isNa: Boolean = false,
    @SerializedName("notes") val notes: String? = null
)
