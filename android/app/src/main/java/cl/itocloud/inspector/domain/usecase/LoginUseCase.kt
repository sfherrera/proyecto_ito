package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.data.repository.AuthRepository
import cl.itocloud.inspector.domain.model.User
import javax.inject.Inject

class LoginUseCase @Inject constructor(
    private val authRepository: AuthRepository
) {
    suspend operator fun invoke(email: String, password: String): Result<User> {
        return try {
            val loginResponse = authRepository.login(email, password)
            val user = User(
                id = loginResponse.userId,
                firstName = "",
                lastName = "",
                fullName = loginResponse.fullName,
                email = loginResponse.email,
                rut = null,
                position = null,
                avatarUrl = null,
                tenantId = loginResponse.tenantId,
                roles = loginResponse.roles
            )
            Result.success(user)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
