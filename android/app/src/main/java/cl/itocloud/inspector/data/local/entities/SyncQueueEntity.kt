package cl.itocloud.inspector.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "sync_queue")
data class SyncQueueEntity(
    @PrimaryKey(autoGenerate = true) val id: Long = 0,
    /** Type of operation: "inspection_submit", "observation_create" */
    val entityType: String,
    /** The ID of the entity being synced */
    val entityId: String,
    /** JSON payload to send to the API */
    val payloadJson: String,
    val createdAt: Long = System.currentTimeMillis(),
    val retryCount: Int = 0,
    val lastError: String? = null
)
