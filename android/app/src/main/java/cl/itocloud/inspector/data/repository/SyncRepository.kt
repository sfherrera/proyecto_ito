package cl.itocloud.inspector.data.repository

import android.util.Log
import cl.itocloud.inspector.data.local.dao.AnswerDao
import cl.itocloud.inspector.data.local.dao.InspectionDao
import cl.itocloud.inspector.data.local.dao.ObservationDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.local.entities.SyncQueueEntity
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.remote.dto.CreateObservationRequest
import cl.itocloud.inspector.data.remote.dto.SubmitInspectionRequest
import com.google.gson.Gson

/**
 * Processes the local sync queue, calling the API for each pending entry.
 * Handles retries and error recording.
 */
class SyncRepository(
    private val api: ItoApi,
    private val syncQueueDao: SyncQueueDao,
    private val inspectionDao: InspectionDao,
    private val answerDao: AnswerDao,
    private val observationDao: ObservationDao,
    private val gson: Gson
) {

    companion object {
        private const val TAG = "SyncRepository"
        private const val MAX_RETRIES = 5
    }

    /**
     * Process all pending items in the sync queue.
     * Returns the number of successfully processed items.
     */
    suspend fun processSyncQueue(): SyncResult {
        val pending = syncQueueDao.getPending()
        var succeeded = 0
        var failed = 0
        var skipped = 0

        for (entry in pending) {
            if (entry.retryCount >= MAX_RETRIES) {
                Log.w(TAG, "Skipping entry ${entry.id} (${entry.entityType}): max retries reached")
                skipped++
                continue
            }

            val success = processEntry(entry)
            if (success) {
                syncQueueDao.delete(entry)
                succeeded++
            } else {
                failed++
            }
        }

        return SyncResult(
            total = pending.size,
            succeeded = succeeded,
            failed = failed,
            skipped = skipped
        )
    }

    /**
     * Get the count of pending sync operations.
     */
    suspend fun getPendingCount(): Int {
        return syncQueueDao.getPendingCount()
    }

    /**
     * Process a single sync queue entry.
     */
    private suspend fun processEntry(entry: SyncQueueEntity): Boolean {
        return try {
            when (entry.entityType) {
                "inspection_submit" -> processInspectionSubmit(entry)
                "observation_create" -> processObservationCreate(entry)
                else -> {
                    Log.w(TAG, "Unknown entity type: ${entry.entityType}")
                    false
                }
            }
        } catch (e: Exception) {
            Log.e(TAG, "Error processing sync entry ${entry.id}: ${e.message}", e)
            syncQueueDao.incrementRetry(entry.id, e.message)
            false
        }
    }

    /**
     * Submit an inspection that was saved offline.
     */
    private suspend fun processInspectionSubmit(entry: SyncQueueEntity): Boolean {
        val request = gson.fromJson(entry.payloadJson, SubmitInspectionRequest::class.java)
        val response = api.submitInspection(entry.entityId, request)

        if (response.success) {
            // Mark answers as synced
            answerDao.markSynced(entry.entityId)

            // Update local inspection status
            inspectionDao.getById(entry.entityId)?.let { entity ->
                inspectionDao.update(entity.copy(status = "Completada"))
            }

            Log.i(TAG, "Inspection ${entry.entityId} submitted successfully")
            return true
        } else {
            Log.w(TAG, "Inspection submit failed: ${response.message}")
            syncQueueDao.incrementRetry(entry.id, response.message)
            return false
        }
    }

    /**
     * Create an observation that was saved offline.
     */
    private suspend fun processObservationCreate(entry: SyncQueueEntity): Boolean {
        val request = gson.fromJson(entry.payloadJson, CreateObservationRequest::class.java)
        val response = api.createObservation(request)

        if (response.success && response.data != null) {
            val serverId = response.data.id
            val localId = entry.entityId

            // Replace the local entity with the server version
            observationDao.deleteById(localId)
            observationDao.insert(
                cl.itocloud.inspector.data.local.entities.ObservationEntity(
                    id = serverId,
                    code = "",
                    title = request.title,
                    status = "Abierta",
                    severity = request.severity,
                    dueDate = request.dueDate,
                    isOverdue = false
                )
            )

            Log.i(TAG, "Observation created: local=$localId -> server=$serverId")
            return true
        } else {
            Log.w(TAG, "Observation create failed: ${response.message}")
            syncQueueDao.incrementRetry(entry.id, response.message)
            return false
        }
    }

    /**
     * Clear all sync queue entries (e.g., on logout).
     */
    suspend fun clearAll() {
        syncQueueDao.deleteAll()
    }
}

/**
 * Result of processing the sync queue.
 */
data class SyncResult(
    val total: Int,
    val succeeded: Int,
    val failed: Int,
    val skipped: Int
) {
    val hasErrors: Boolean get() = failed > 0 || skipped > 0
    val isComplete: Boolean get() = succeeded == total
}
