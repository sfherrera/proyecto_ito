package cl.itocloud.inspector.data.repository

import cl.itocloud.inspector.data.local.dao.ObservationDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.local.entities.ObservationEntity
import cl.itocloud.inspector.data.local.entities.SyncQueueEntity
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.remote.dto.CreateObservationRequest
import cl.itocloud.inspector.data.remote.dto.ObservationDto
import com.google.gson.Gson
import kotlinx.coroutines.flow.Flow
import java.util.UUID

/**
 * Offline-first repository for observations.
 * Reads from Room, syncs from API. New observations are created locally
 * and enqueued for background sync.
 */
class ObservationRepository(
    private val api: ItoApi,
    private val observationDao: ObservationDao,
    private val syncQueueDao: SyncQueueDao,
    private val gson: Gson
) {

    /**
     * Get all observations from the local database as a Flow.
     */
    fun getObservations(): Flow<List<ObservationEntity>> {
        return observationDao.getAll()
    }

    /**
     * Get a single observation from local DB.
     */
    suspend fun getObservation(id: String): ObservationEntity? {
        return observationDao.getById(id)
    }

    /**
     * Fetch observations from the API and store in Room.
     * Returns true on success, false on failure.
     */
    suspend fun syncObservations(projectId: String? = null): Boolean {
        return try {
            val response = api.getObservations(projectId = projectId)
            if (response.success && response.data != null) {
                val entities = response.data.map { it.toEntity() }
                observationDao.insertAll(entities)
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
     * Create a new observation. Tries API first; on failure, saves locally
     * and enqueues for background sync.
     */
    suspend fun createObservation(request: CreateObservationRequest): Result<String> {
        return try {
            val response = api.createObservation(request)
            if (response.success && response.data != null) {
                val id = response.data.id
                // Save to local DB for caching
                observationDao.insert(
                    ObservationEntity(
                        id = id,
                        code = "",
                        title = request.title,
                        status = "Abierta",
                        severity = request.severity,
                        dueDate = request.dueDate,
                        isOverdue = false
                    )
                )
                Result.success(id)
            } else {
                // API rejected — save locally and enqueue
                val localId = createLocalObservation(request)
                Result.success(localId)
            }
        } catch (e: Exception) {
            // Network error — save locally and enqueue for sync
            val localId = createLocalObservation(request)
            Result.success(localId)
        }
    }

    /**
     * Create a local observation entity and enqueue for sync.
     */
    private suspend fun createLocalObservation(request: CreateObservationRequest): String {
        val localId = "local-${UUID.randomUUID()}"

        observationDao.insert(
            ObservationEntity(
                id = localId,
                code = "PENDING",
                title = request.title,
                status = "PendienteSync",
                severity = request.severity,
                dueDate = request.dueDate,
                isOverdue = false
            )
        )

        syncQueueDao.insert(
            SyncQueueEntity(
                entityType = "observation_create",
                entityId = localId,
                payloadJson = gson.toJson(request)
            )
        )

        return localId
    }

    /**
     * Clear all cached observations (for logout or data reset).
     */
    suspend fun clearCache() {
        observationDao.deleteAll()
    }
}

// ── Mapping extensions ──────────────────────────────────────────────────────

private fun ObservationDto.toEntity(): ObservationEntity {
    return ObservationEntity(
        id = id,
        code = code,
        title = title,
        status = status,
        severity = severity,
        dueDate = dueDate,
        isOverdue = isOverdue
    )
}
