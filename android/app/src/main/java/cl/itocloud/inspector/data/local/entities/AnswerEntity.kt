package cl.itocloud.inspector.data.local.entities

import androidx.room.Entity

@Entity(
    tableName = "answers",
    primaryKeys = ["inspectionId", "questionId"]
)
data class AnswerEntity(
    val inspectionId: String,
    val questionId: String,
    val answerValue: String?,
    val isConforming: Boolean?,
    val isNa: Boolean,
    val notes: String?,
    val isSynced: Boolean = false
)
