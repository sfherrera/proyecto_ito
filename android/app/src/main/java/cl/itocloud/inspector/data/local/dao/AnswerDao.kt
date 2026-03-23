package cl.itocloud.inspector.data.local.dao

import androidx.room.*
import cl.itocloud.inspector.data.local.entities.AnswerEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface AnswerDao {

    @Query("SELECT * FROM answers WHERE inspectionId = :inspectionId")
    fun getByInspection(inspectionId: String): Flow<List<AnswerEntity>>

    @Query("SELECT * FROM answers WHERE inspectionId = :inspectionId")
    suspend fun getByInspectionOnce(inspectionId: String): List<AnswerEntity>

    @Query("SELECT * FROM answers WHERE inspectionId = :inspectionId AND isSynced = 0")
    suspend fun getUnsyncedByInspection(inspectionId: String): List<AnswerEntity>

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsert(answer: AnswerEntity)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun upsertAll(answers: List<AnswerEntity>)

    @Query("UPDATE answers SET isSynced = 1 WHERE inspectionId = :inspectionId")
    suspend fun markSynced(inspectionId: String)

    @Query("DELETE FROM answers WHERE inspectionId = :inspectionId")
    suspend fun deleteByInspection(inspectionId: String)
}
