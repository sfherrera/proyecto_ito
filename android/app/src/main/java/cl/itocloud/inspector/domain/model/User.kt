package cl.itocloud.inspector.domain.model

data class User(
    val id: String,
    val firstName: String,
    val lastName: String,
    val fullName: String,
    val email: String,
    val rut: String?,
    val position: String?,
    val avatarUrl: String?,
    val tenantId: String,
    val roles: List<String>
)
