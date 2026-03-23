package cl.itocloud.inspector.data.repository

import cl.itocloud.inspector.data.local.dao.AnswerDao
import cl.itocloud.inspector.data.local.dao.InspectionDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.local.entities.AnswerEntity
import cl.itocloud.inspector.data.local.entities.InspectionEntity
import cl.itocloud.inspector.data.local.entities.SyncQueueEntity
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.remote.dto.AnswerSubmitDto
import cl.itocloud.inspector.data.remote.dto.InspectionDetailDto
import cl.itocloud.inspector.data.remote.dto.InspectionDto
import cl.itocloud.inspector.data.remote.dto.SubmitInspectionRequest
import com.google.gson.Gson
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

/**
 * Offline-first repository for inspections.
 * Reads from Room, syncs from API. Writes go to local DB first,
 * then enqueue to sync queue for background upload.
 */
class InspectionRepository(
    private val api: ItoApi,
    private val inspectionDao: InspectionDao,
    private val answerDao: AnswerDao,
    private val syncQueueDao: SyncQueueDao,
    private val gson: Gson
) {

    /**
     * Get all inspections as a Flow from the local database.
     * Call [syncInspections] to refresh from the API.
     */
    fun getMyInspections(status: String? = null): Flow<List<InspectionEntity>> {
        return if (status != null) {
            inspectionDao.getByStatus(status)
        } else {
            inspectionDao.getAll()
        }
    }

    /**
     * Get a single inspection from local DB (with cached sections in JSON).
     */
    suspend fun getInspection(id: String): InspectionEntity? {
        return inspectionDao.getById(id)
    }

    /**
     * Get answers for an inspection from local DB.
     */
    fun getAnswers(inspectionId: String): Flow<List<AnswerEntity>> {
        return answerDao.getByInspection(inspectionId)
    }

    /**
     * Fetch inspections from the API and store in Room.
     * Returns true on success, false on failure (offline fallback).
     */
    suspend fun syncInspections(status: String? = null): Boolean {
        return try {
            val response = api.getMyInspections(status)
            if (response.success && response.data != null) {
                val entities = response.data.map { it.toEntity() }
                inspectionDao.insertAll(entities)
                true
            } else {
                false
            }
        } catch (e: Exception) {
            // Network error — use cached data
            false
        }
    }

    /**
     * Fetch full inspection detail from API and cache locally (including sections JSON).
     */
    suspend fun syncInspectionDetail(id: String): Boolean {
        return try {
            val response = api.getInspection(id, includeAnswers = true)
            if (response.success && response.data != null) {
                val detail = response.data
                val entity = detail.toEntity(gson)
                inspectionDao.insert(entity)
                true
            } else {
                false
            }
        } catch (e: Exception) {
            false
        }
    }

    /**
     * Start an inspection via the API and update local status.
     */
    suspend fun startInspection(id: String): Result<Unit> {
        return try {
            val response = api.startInspection(id)
            if (response.success) {
                // Update local entity status
                inspectionDao.getById(id)?.let { entity ->
                    inspectionDao.update(entity.copy(status = "EnProgreso"))
                }
                Result.success(Unit)
            } else {
                Result.failure(Exception(response.message ?: "Error al iniciar inspección"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    /**
     * Save a single answer locally (offline-first).
     */
    suspend fun saveAnswer(
        inspectionId: String,
        questionId: String,
        answerValue: String?,
        isConforming: Boolean?,
        isNa: Boolean,
        notes: String?
    ) {
        answerDao.upsert(
            AnswerEntity(
                inspectionId = inspectionId,
                questionId = questionId,
                answerValue = answerValue,
                isConforming = isConforming,
                isNa = isNa,
                notes = notes,
                isSynced = false
            )
        )
    }

    /**
     * Submit an inspection. Tries to send immediately; if offline, enqueues to sync queue.
     */
    suspend fun submitInspection(
        inspectionId: String,
        latitude: Double?,
        longitude: Double?,
        weatherConditions: String?,
        notes: String?
    ): Result<Unit> {
        val answers = answerDao.getByInspectionOnce(inspectionId)
        val submitRequest = SubmitInspectionRequest(
            answers = answers.map { a ->
                AnswerSubmitDto(
                    questionId = a.questionId,
                    answerValue = a.answerValue,
                    isConforming = a.isConforming,
                    isNa = a.isNa,
                    notes = a.notes
                )
            },
            latitude = latitude,
            longitude = longitude,
            weatherConditions = weatherConditions,
            notes = notes
        )

        return try {
            val response = api.submitInspection(inspectionId, submitRequest)
            if (response.success) {
                // Mark answers as synced and update local status
                answerDao.markSynced(inspectionId)
                inspectionDao.getById(inspectionId)?.let { entity ->
                    inspectionDao.update(entity.copy(status = "Completada"))
                }
                Result.success(Unit)
            } else {
                // API rejected — enqueue for retry
                enqueueSubmission(inspectionId, submitRequest)
                Result.failure(Exception(response.message ?: "Error al enviar inspección"))
            }
        } catch (e: Exception) {
            // Network error — enqueue for background sync
            enqueueSubmission(inspectionId, submitRequest)
            // Update local status to pending sync
            inspectionDao.getById(inspectionId)?.let { entity ->
                inspectionDao.update(entity.copy(status = "PendienteSync"))
            }
            Result.success(Unit) // Return success since it's saved locally
        }
    }

    private suspend fun enqueueSubmission(inspectionId: String, request: SubmitInspectionRequest) {
        val existing = syncQueueDao.findByTypeAndEntityId("inspection_submit", inspectionId)
        if (existing != null) {
            // Update existing entry
            syncQueueDao.delete(existing)
        }
        syncQueueDao.insert(
            SyncQueueEntity(
                entityType = "inspection_submit",
                entityId = inspectionId,
                payloadJson = gson.toJson(request)
            )
        )
    }
}

// ── Mapping extensions ──────────────────────────────────────────────────────

private fun InspectionDto.toEntity(): InspectionEntity {
    return InspectionEntity(
        id = id,
        code = code,
        title = title,
        status = status,
        inspectionType = "",
        priority = "",
        projectId = "",
        templateId = null,
        assignedToName = assignedToName,
        contractorName = null,
        scheduledDate = scheduledDate,
        startedAt = null,
        finishedAt = null,
        score = score,
        passed = null,
        totalQuestions = 0,
        answeredQuestions = 0,
        conformingCount = 0,
        nonConformingCount = 0,
        notes = null,
        sectionsJson = null
    )
}

private fun InspectionDetailDto.toEntity(gson: Gson): InspectionEntity {
    val sectionsJsonStr = if (sections != null) gson.toJson(sections) else null
    return InspectionEntity(
        id = id,
        code = code,
        title = title,
        status = status,
        inspectionType = inspectionType,
        priority = priority,
        projectId = projectId,
        templateId = templateId,
        assignedToName = assignedToName,
        contractorName = contractorName,
        scheduledDate = scheduledDate,
        startedAt = startedAt,
        finishedAt = finishedAt,
        score = score,
        passed = passed,
        totalQuestions = totalQuestions,
        answeredQuestions = answeredQuestions,
        conformingCount = conformingCount,
        nonConformingCount = nonConformingCount,
        notes = notes,
        sectionsJson = sectionsJsonStr
    )
}
