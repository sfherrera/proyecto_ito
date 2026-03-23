package cl.itocloud.inspector.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "inspections")
data class InspectionEntity(
    @PrimaryKey val id: String,
    val code: String,
    val title: String,
    val status: String,
    val inspectionType: String,
    val priority: String,
    val projectId: String,
    val templateId: String?,
    val assignedToName: String?,
    val contractorName: String?,
    val scheduledDate: String?,
    val startedAt: String?,
    val finishedAt: String?,
    val score: Double?,
    val passed: Boolean?,
    val totalQuestions: Int,
    val answeredQuestions: Int,
    val conformingCount: Int,
    val nonConformingCount: Int,
    val notes: String?,
    /** JSON-serialized sections + questions for offline use */
    val sectionsJson: String?,
    val lastSyncedAt: Long = System.currentTimeMillis()
)
