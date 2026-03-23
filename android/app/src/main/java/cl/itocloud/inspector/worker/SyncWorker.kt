package cl.itocloud.inspector.worker

import android.content.Context
import android.util.Log
import androidx.hilt.work.HiltWorker
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.local.entities.SyncQueueEntity
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.remote.dto.CreateObservationRequest
import cl.itocloud.inspector.data.remote.dto.SubmitInspectionRequest
import com.google.gson.Gson
import dagger.assisted.Assisted
import dagger.assisted.AssistedInject
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

@HiltWorker
class SyncWorker @AssistedInject constructor(
    @Assisted appContext: Context,
    @Assisted workerParams: WorkerParameters,
    private val syncQueueDao: SyncQueueDao,
    private val api: ItoApi,
    private val gson: Gson
) : CoroutineWorker(appContext, workerParams) {

    companion object {
        const val TAG = "SyncWorker"
        const val MAX_RETRIES = 5

        // Entity types matching SyncQueueEntity.entityType
        const val TYPE_INSPECTION_SUBMIT = "inspection_submit"
        const val TYPE_OBSERVATION_CREATE = "observation_create"
    }

    override suspend fun doWork(): Result = withContext(Dispatchers.IO) {
        try {
            Log.d(TAG, "Starting sync work...")

            val pendingEntries = syncQueueDao.getPending()
            if (pendingEntries.isEmpty()) {
                Log.d(TAG, "No pending sync entries found.")
                return@withContext Result.success()
            }

            Log.d(TAG, "Processing ${pendingEntries.size} pending sync entries.")

            var hasFailures = false

            for (entry in pendingEntries) {
                if (entry.retryCount >= MAX_RETRIES) {
                    Log.w(TAG, "Entry ${entry.id} exceeded max retries, skipping.")
                    continue
                }

                try {
                    processEntry(entry)
                    Log.d(TAG, "Successfully synced entry ${entry.id} (${entry.entityType})")
                    syncQueueDao.deleteById(entry.id)
                } catch (e: Exception) {
                    Log.e(TAG, "Failed to sync entry ${entry.id}: ${e.message}", e)
                    hasFailures = true
                    syncQueueDao.incrementRetry(entry.id, e.message)
                }
            }

            if (hasFailures) {
                Log.w(TAG, "Sync completed with some failures, will retry.")
                Result.retry()
            } else {
                Log.d(TAG, "Sync completed successfully.")
                Result.success()
            }
        } catch (e: Exception) {
            Log.e(TAG, "Sync worker failed unexpectedly: ${e.message}", e)
            Result.retry()
        }
    }

    private suspend fun processEntry(entry: SyncQueueEntity) {
        when (entry.entityType) {
            TYPE_INSPECTION_SUBMIT -> {
                val request = gson.fromJson(entry.payloadJson, SubmitInspectionRequest::class.java)
                val response = api.submitInspection(entry.entityId, request)
                if (!response.success) {
                    throw Exception(response.message ?: "Failed to submit inspection")
                }
            }

            TYPE_OBSERVATION_CREATE -> {
                val request = gson.fromJson(entry.payloadJson, CreateObservationRequest::class.java)
                val response = api.createObservation(request)
                if (!response.success) {
                    throw Exception(response.message ?: "Failed to create observation")
                }
            }

            else -> {
                Log.w(TAG, "Unknown sync entity type: ${entry.entityType}")
                syncQueueDao.deleteById(entry.id)
            }
        }
    }
}
