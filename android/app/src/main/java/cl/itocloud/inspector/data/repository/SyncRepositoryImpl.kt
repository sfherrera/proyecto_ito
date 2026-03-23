package cl.itocloud.inspector.data.repository

import cl.itocloud.inspector.data.local.dao.AnswerDao
import cl.itocloud.inspector.data.local.dao.InspectionDao
import cl.itocloud.inspector.data.local.dao.ObservationDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.domain.repository.SyncRepository as DomainSyncRepository
import com.google.gson.Gson

/**
 * Domain-layer adapter that wraps the concrete SyncRepository
 * and implements the domain SyncRepository interface.
 */
class SyncRepositoryImpl(
    private val api: ItoApi,
    private val inspectionDao: InspectionDao,
    private val observationDao: ObservationDao,
    private val answerDao: AnswerDao,
    private val syncQueueDao: SyncQueueDao,
    private val gson: Gson
) : DomainSyncRepository {

    private val dataRepo = SyncRepository(api, syncQueueDao, inspectionDao, answerDao, observationDao, gson)

    override suspend fun syncAll() {
        val result = dataRepo.processSyncQueue()
        if (result.hasErrors) {
            throw Exception("Sync completed with errors: ${result.failed} failed, ${result.skipped} skipped out of ${result.total}")
        }
    }

    override suspend fun getPendingSyncCount(): Int {
        return dataRepo.getPendingCount()
    }
}
