package cl.itocloud.inspector.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "observations")
data class ObservationEntity(
    @PrimaryKey val id: String,
    val code: String,
    val title: String,
    val status: String,
    val severity: String,
    val dueDate: String?,
    val isOverdue: Boolean,
    val lastSyncedAt: Long = System.currentTimeMillis()
)
