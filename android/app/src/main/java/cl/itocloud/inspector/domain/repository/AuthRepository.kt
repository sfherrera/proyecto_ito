package cl.itocloud.inspector.domain.repository

import cl.itocloud.inspector.domain.model.User

/**
 * Domain-layer auth contract.
 * Note: The existing data.repository.AuthRepository is a concrete class,
 * so use cases that need auth can inject it directly or use this interface
 * with an adapter implementation.
 */
interface DomainAuthRepository {
    suspend fun login(email: String, password: String): User
    suspend fun logout()
    suspend fun getCurrentUser(): User?
    suspend fun isLoggedIn(): Boolean
    suspend fun getAccessToken(): String?
}
