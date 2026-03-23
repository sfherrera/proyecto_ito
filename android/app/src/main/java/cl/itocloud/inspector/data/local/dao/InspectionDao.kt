package cl.itocloud.inspector.data.local.dao

import androidx.room.*
import cl.itocloud.inspector.data.local.entities.InspectionEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface InspectionDao {

    @Query("SELECT * FROM inspections ORDER BY scheduledDate DESC")
    fun getAll(): Flow<List<InspectionEntity>>

    @Query("SELECT * FROM inspections WHERE status = :status ORDER BY scheduledDate DESC")
    fun getByStatus(status: String): Flow<List<InspectionEntity>>

    @Query("SELECT * FROM inspections WHERE id = :id")
    suspend fun getById(id: String): InspectionEntity?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(inspections: List<InspectionEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(inspection: InspectionEntity)

    @Update
    suspend fun update(inspection: InspectionEntity)

    @Query("DELETE FROM inspections")
    suspend fun deleteAll()

    @Query("DELETE FROM inspections WHERE id = :id")
    suspend fun deleteById(id: String)
}
