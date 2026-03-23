package cl.itocloud.inspector.util

import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone

object DateUtils {

    private val apiDateFormat = SimpleDateFormat(Constants.API_DATE_FORMAT, Locale("es", "CL")).apply {
        timeZone = TimeZone.getTimeZone("UTC")
    }

    private val displayDateFormat = SimpleDateFormat(Constants.DISPLAY_DATE_FORMAT, Locale("es", "CL"))

    private val displayDateTimeFormat = SimpleDateFormat(Constants.DISPLAY_DATETIME_FORMAT, Locale("es", "CL"))

    /**
     * Parses an ISO date string from the API into a Date object.
     */
    fun parseApiDate(dateString: String?): Date? {
        if (dateString.isNullOrBlank()) return null
        return try {
            // Handle various ISO formats by trimming timezone suffix
            val cleaned = dateString
                .replace("Z", "")
                .replace(Regex("\\+\\d{2}:\\d{2}$"), "")
                .replace(Regex("\\.\\d+$"), "")
            apiDateFormat.parse(cleaned)
        } catch (e: Exception) {
            null
        }
    }

    /**
     * Formats a date string from API format to display format (dd/MM/yyyy).
     */
    fun formatDate(dateString: String?): String {
        val date = parseApiDate(dateString) ?: return "-"
        return displayDateFormat.format(date)
    }

    /**
     * Formats a date string from API format to display datetime format (dd/MM/yyyy HH:mm).
     */
    fun formatDateTime(dateString: String?): String {
        val date = parseApiDate(dateString) ?: return "-"
        return displayDateTimeFormat.format(date)
    }

    /**
     * Formats a Date object to display format (dd/MM/yyyy).
     */
    fun formatDate(date: Date?): String {
        if (date == null) return "-"
        return displayDateFormat.format(date)
    }

    /**
     * Formats a Date object to display datetime format (dd/MM/yyyy HH:mm).
     */
    fun formatDateTime(date: Date?): String {
        if (date == null) return "-"
        return displayDateTimeFormat.format(date)
    }

    /**
     * Returns a relative time description (e.g., "Hace 2 horas", "Ayer").
     */
    fun getRelativeTime(dateString: String?): String {
        val date = parseApiDate(dateString) ?: return "-"
        val now = System.currentTimeMillis()
        val diff = now - date.time

        val seconds = diff / 1000
        val minutes = seconds / 60
        val hours = minutes / 60
        val days = hours / 24

        return when {
            seconds < 60 -> "Ahora"
            minutes < 60 -> "Hace $minutes min"
            hours < 24 -> "Hace $hours h"
            days == 1L -> "Ayer"
            days < 7 -> "Hace $days dias"
            days < 30 -> "Hace ${days / 7} semanas"
            else -> formatDate(date)
        }
    }

    /**
     * Checks if a date string represents a date that is before today (overdue).
     */
    fun isOverdue(dateString: String?): Boolean {
        val date = parseApiDate(dateString) ?: return false
        return date.before(Date())
    }

    /**
     * Formats a Date to API format string.
     */
    fun toApiFormat(date: Date): String {
        return apiDateFormat.format(date)
    }
}
