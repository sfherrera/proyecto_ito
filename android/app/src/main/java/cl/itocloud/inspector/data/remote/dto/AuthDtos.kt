package cl.itocloud.inspector.data.remote.dto

import com.google.gson.annotations.SerializedName

data class LoginRequest(
    @SerializedName("email") val email: String,
    @SerializedName("password") val password: String
)

data class LoginResponse(
    @SerializedName("accessToken") val accessToken: String,
    @SerializedName("expiresAt") val expiresAt: String,
    @SerializedName("userId") val userId: String,
    @SerializedName("fullName") val fullName: String,
    @SerializedName("email") val email: String,
    @SerializedName("tenantId") val tenantId: String,
    @SerializedName("tenantName") val tenantName: String,
    @SerializedName("roles") val roles: List<String>
)

data class ChangePasswordRequest(
    @SerializedName("currentPassword") val currentPassword: String,
    @SerializedName("newPassword") val newPassword: String,
    @SerializedName("confirmPassword") val confirmPassword: String
)

data class UserResponse(
    @SerializedName("id") val id: String,
    @SerializedName("firstName") val firstName: String,
    @SerializedName("lastName") val lastName: String,
    @SerializedName("fullName") val fullName: String,
    @SerializedName("email") val email: String,
    @SerializedName("rut") val rut: String?,
    @SerializedName("position") val position: String?,
    @SerializedName("avatarUrl") val avatarUrl: String?,
    @SerializedName("tenantId") val tenantId: String,
    @SerializedName("roles") val roles: List<String>
)

data class MessageResponse(
    @SerializedName("message") val message: String
)
