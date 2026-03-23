package cl.itocloud.inspector.data.remote

import android.content.Context
import androidx.datastore.preferences.core.stringPreferencesKey
import cl.itocloud.inspector.data.repository.AuthRepository
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response

/**
 * OkHttp interceptor that attaches the Bearer token from DataStore
 * to every outgoing request (except login).
 */
class AuthInterceptor(
    private val context: Context
) : Interceptor {

    companion object {
        val TOKEN_KEY = stringPreferencesKey("auth_token")
    }

    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        // Skip auth header for login endpoint
        if (originalRequest.url.encodedPath.endsWith("/auth/login")) {
            return chain.proceed(originalRequest)
        }

        val token = runBlocking {
            context.authDataStore.data.map { prefs ->
                prefs[TOKEN_KEY]
            }.first()
        }

        val newRequest = if (!token.isNullOrBlank()) {
            originalRequest.newBuilder()
                .addHeader("Authorization", "Bearer $token")
                .build()
        } else {
            originalRequest
        }

        return chain.proceed(newRequest)
    }
}
