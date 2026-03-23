package cl.itocloud.inspector.domain.model

data class Inspection(
    val id: String,
    val code: String,
    val title: String,
    val status: InspectionStatus,
    val inspectionType: String,
    val priority: String,
    val projectId: String,
    val assignedToName: String?,
    val contractorName: String?,
    val scheduledDate: String?,
    val startedAt: String?,
    val finishedAt: String?,
    val score: Double?,
    val passed: Boolean?,
    val totalQuestions: Int,
    val answeredQuestions: Int,
    val conformingCount: Int,
    val nonConformingCount: Int,
    val notes: String?,
    val sections: List<Section>?
)

enum class InspectionStatus {
    Programada,
    EnProgreso,
    Completada,
    Cancelada
}

data class Section(
    val id: String,
    val title: String,
    val orderIndex: Int,
    val questions: List<Question>
)

data class Question(
    val id: String,
    val text: String,
    val type: String,
    val isCritical: Boolean,
    val orderIndex: Int,
    val options: String?
)

data class Answer(
    val questionId: String,
    val answerValue: String?,
    val isConforming: Boolean?,
    val isNa: Boolean,
    val notes: String?
)
