package cl.itocloud.inspector.domain.model

data class Project(
    val id: String,
    val code: String,
    val name: String,
    val status: String?,
    val companyName: String?
)
