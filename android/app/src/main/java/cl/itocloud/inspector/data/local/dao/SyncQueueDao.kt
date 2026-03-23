package cl.itocloud.inspector.data.local.dao

import androidx.room.*
import cl.itocloud.inspector.data.local.entities.SyncQueueEntity

@Dao
interface SyncQueueDao {

    @Query("SELECT * FROM sync_queue ORDER BY createdAt ASC")
    suspend fun getPending(): List<SyncQueueEntity>

    @Query("SELECT * FROM sync_queue WHERE entityType = :type AND entityId = :entityId LIMIT 1")
    suspend fun findByTypeAndEntityId(type: String, entityId: String): SyncQueueEntity?

    @Query("SELECT COUNT(*) FROM sync_queue")
    suspend fun getPendingCount(): Int

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(entry: SyncQueueEntity): Long

    @Delete
    suspend fun delete(entry: SyncQueueEntity)

    @Query("UPDATE sync_queue SET retryCount = retryCount + 1, lastError = :error WHERE id = :id")
    suspend fun incrementRetry(id: Long, error: String?)

    @Query("DELETE FROM sync_queue WHERE id = :id")
    suspend fun deleteById(id: Long)

    @Query("DELETE FROM sync_queue")
    suspend fun deleteAll()
}
