package cl.itocloud.inspector.data.repository

import cl.itocloud.inspector.data.local.dao.ObservationDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.remote.dto.CreateObservationRequest
import cl.itocloud.inspector.domain.model.Observation
import cl.itocloud.inspector.domain.repository.ObservationRepository as DomainObservationRepository
import cl.itocloud.inspector.util.NetworkMonitor
import cl.itocloud.inspector.worker.SyncScheduler
import com.google.gson.Gson
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

/**
 * Domain-layer adapter that wraps the concrete ObservationRepository
 * and maps data-layer entities to domain models.
 */
class ObservationRepositoryImpl(
    private val api: ItoApi,
    private val observationDao: ObservationDao,
    private val syncQueueDao: SyncQueueDao,
    private val networkMonitor: NetworkMonitor,
    private val syncScheduler: SyncScheduler,
    private val gson: Gson
) : DomainObservationRepository {

    private val dataRepo = ObservationRepository(api, observationDao, syncQueueDao, gson)

    override fun getObservations(): Flow<List<Observation>> {
        return dataRepo.getObservations().map { entities ->
            entities.map { entity ->
                Observation(
                    id = entity.id,
                    code = entity.code,
                    title = entity.title,
                    status = entity.status,
                    severity = entity.severity,
                    dueDate = entity.dueDate,
                    isOverdue = entity.isOverdue
                )
            }
        }
    }

    override suspend fun createObservation(
        title: String,
        severity: String,
        inspectionId: String?,
        description: String?,
        dueDate: String?,
        latitude: Double?,
        longitude: Double?
    ): Observation {
        val request = CreateObservationRequest(
            projectId = "",
            title = title,
            description = description,
            severity = severity,
            category = null,
            locationDescription = null,
            contractorId = null,
            assignedToId = null,
            dueDate = dueDate,
            inspectionId = inspectionId,
            answerId = null,
            specialtyId = null,
            sectorId = null,
            unitId = null,
            rootCause = null
        )

        val result = dataRepo.createObservation(request)
        val id = result.getOrThrow()

        if (!networkMonitor.isCurrentlyConnected()) {
            syncScheduler.enqueueImmediateSync()
        }

        return Observation(
            id = id,
            code = "",
            title = title,
            status = "Abierta",
            severity = severity,
            dueDate = dueDate,
            isOverdue = false
        )
    }

    override suspend fun syncObservations() {
        val success = dataRepo.syncObservations()
        if (!success) {
            throw Exception("Failed to sync observations from API")
        }
    }
}
