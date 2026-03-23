package cl.itocloud.inspector.domain.model

data class Observation(
    val id: String,
    val code: String,
    val title: String,
    val status: String,
    val severity: String,
    val dueDate: String?,
    val isOverdue: Boolean
)
