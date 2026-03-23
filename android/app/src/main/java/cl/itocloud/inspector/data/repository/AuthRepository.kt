package cl.itocloud.inspector.data.repository

import android.content.Context
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.remote.authDataStore
import cl.itocloud.inspector.data.remote.dto.ChangePasswordRequest
import cl.itocloud.inspector.data.remote.dto.LoginRequest
import cl.itocloud.inspector.data.remote.dto.LoginResponse
import cl.itocloud.inspector.data.remote.dto.UserResponse
import cl.itocloud.inspector.domain.model.User
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map

class AuthRepository(
    private val api: ItoApi,
    private val context: Context
) {

    companion object {
        val KEY_TOKEN = stringPreferencesKey("auth_token")
        val KEY_USER_ID = stringPreferencesKey("user_id")
        val KEY_FULL_NAME = stringPreferencesKey("full_name")
        val KEY_EMAIL = stringPreferencesKey("email")
        val KEY_TENANT_ID = stringPreferencesKey("tenant_id")
        val KEY_TENANT_NAME = stringPreferencesKey("tenant_name")
        val KEY_ROLES = stringPreferencesKey("roles") // comma-separated
        val KEY_EXPIRES_AT = stringPreferencesKey("expires_at")
    }

    /**
     * Perform login, save the token and user info to DataStore.
     * Returns the LoginResponse on success or throws on failure.
     */
    suspend fun login(email: String, password: String): LoginResponse {
        val response = api.login(LoginRequest(email, password))

        if (!response.success || response.data == null) {
            throw Exception(response.message ?: "Error de autenticación")
        }

        val loginData = response.data
        saveSession(loginData)
        return loginData
    }

    /**
     * Persist session data in DataStore.
     */
    private suspend fun saveSession(data: LoginResponse) {
        context.authDataStore.edit { prefs ->
            prefs[KEY_TOKEN] = data.accessToken
            prefs[KEY_USER_ID] = data.userId
            prefs[KEY_FULL_NAME] = data.fullName
            prefs[KEY_EMAIL] = data.email
            prefs[KEY_TENANT_ID] = data.tenantId
            prefs[KEY_TENANT_NAME] = data.tenantName
            prefs[KEY_ROLES] = data.roles.joinToString(",")
            prefs[KEY_EXPIRES_AT] = data.expiresAt
        }
    }

    /**
     * Get the stored auth token, or null if not logged in.
     */
    suspend fun getToken(): String? {
        return context.authDataStore.data.map { it[KEY_TOKEN] }.first()
    }

    /**
     * Observe the auth token as a Flow.
     */
    fun getTokenFlow(): Flow<String?> {
        return context.authDataStore.data.map { it[KEY_TOKEN] }
    }

    /**
     * Check if a valid token exists.
     */
    suspend fun isLoggedIn(): Boolean {
        return !getToken().isNullOrBlank()
    }

    /**
     * Observe login state as a Flow.
     */
    fun isLoggedInFlow(): Flow<Boolean> {
        return context.authDataStore.data.map { prefs ->
            !prefs[KEY_TOKEN].isNullOrBlank()
        }
    }

    /**
     * Get locally stored user info.
     */
    suspend fun getUserInfo(): StoredUserInfo? {
        val prefs = context.authDataStore.data.first()
        val userId = prefs[KEY_USER_ID] ?: return null
        return StoredUserInfo(
            userId = userId,
            fullName = prefs[KEY_FULL_NAME] ?: "",
            email = prefs[KEY_EMAIL] ?: "",
            tenantId = prefs[KEY_TENANT_ID] ?: "",
            tenantName = prefs[KEY_TENANT_NAME] ?: "",
            roles = prefs[KEY_ROLES]?.split(",") ?: emptyList()
        )
    }

    /**
     * Fetch current user profile from the API.
     */
    suspend fun getMe(): UserResponse {
        val response = api.getMe()
        if (!response.success || response.data == null) {
            throw Exception(response.message ?: "No se pudo obtener el perfil")
        }
        return response.data
    }

    /**
     * Change the current user's password.
     */
    suspend fun changePassword(currentPassword: String, newPassword: String, confirmPassword: String): String {
        val response = api.changePassword(
            ChangePasswordRequest(currentPassword, newPassword, confirmPassword)
        )
        if (!response.success || response.data == null) {
            throw Exception(response.message ?: "Error al cambiar contraseña")
        }
        return response.data.message
    }

    /**
     * Clear all stored auth data (logout).
     */
    suspend fun clearToken() {
        context.authDataStore.edit { prefs ->
            prefs.clear()
        }
    }

    /**
     * Full logout: clear local session.
     */
    suspend fun logout() {
        clearToken()
    }

    /**
     * Returns the current user as a domain User model, or null if not logged in.
     */
    suspend fun getCurrentUser(): User? {
        val info = getUserInfo() ?: return null
        val nameParts = info.fullName.split(" ")
        return User(
            id = info.userId,
            firstName = nameParts.firstOrNull() ?: "",
            lastName = nameParts.drop(1).joinToString(" "),
            fullName = info.fullName,
            email = info.email,
            rut = null,
            position = null,
            avatarUrl = null,
            tenantId = info.tenantId,
            roles = info.roles
        )
    }
}

/**
 * Locally stored user info from the last login.
 */
data class StoredUserInfo(
    val userId: String,
    val fullName: String,
    val email: String,
    val tenantId: String,
    val tenantName: String,
    val roles: List<String>
)
