package cl.itocloud.inspector.domain.repository

interface SyncRepository {
    suspend fun syncAll()
    suspend fun getPendingSyncCount(): Int
}
