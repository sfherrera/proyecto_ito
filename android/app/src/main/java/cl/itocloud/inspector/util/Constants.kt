package cl.itocloud.inspector.util

import cl.itocloud.inspector.BuildConfig

object Constants {
    // API — en debug apunta a 10.0.2.2:5095 (localhost del host desde el emulador)
    val API_BASE_URL: String = BuildConfig.API_BASE_URL

    // Database
    const val DATABASE_NAME = "ito_cloud_db"

    // DataStore / Preferences
    const val PREFERENCES_NAME = "ito_cloud_prefs"
    const val PREF_ACCESS_TOKEN = "access_token"
    const val PREF_TOKEN_EXPIRY = "token_expiry"
    const val PREF_USER_ID = "user_id"
    const val PREF_USER_EMAIL = "user_email"
    const val PREF_USER_FULL_NAME = "user_full_name"
    const val PREF_TENANT_ID = "tenant_id"
    const val PREF_USER_ROLES = "user_roles"
    const val PREF_IS_LOGGED_IN = "is_logged_in"

    // Sync
    const val SYNC_CHANNEL_ID = "ito_sync_channel"
    const val SYNC_NOTIFICATION_ID = 1001

    // Date formats
    const val API_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss"
    const val DISPLAY_DATE_FORMAT = "dd/MM/yyyy"
    const val DISPLAY_DATETIME_FORMAT = "dd/MM/yyyy HH:mm"

    // Inspection statuses (match API enum values)
    const val STATUS_PROGRAMADA = "Programada"
    const val STATUS_EN_PROGRESO = "EnProgreso"
    const val STATUS_COMPLETADA = "Completada"
    const val STATUS_CANCELADA = "Cancelada"

    // Observation severities
    const val SEVERITY_LOW = "Baja"
    const val SEVERITY_MEDIUM = "Media"
    const val SEVERITY_HIGH = "Alta"
    const val SEVERITY_CRITICAL = "Critica"
}
