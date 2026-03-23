package cl.itocloud.inspector.data.local.dao

import androidx.room.*
import cl.itocloud.inspector.data.local.entities.ObservationEntity
import kotlinx.coroutines.flow.Flow

@Dao
interface ObservationDao {

    @Query("SELECT * FROM observations ORDER BY dueDate ASC")
    fun getAll(): Flow<List<ObservationEntity>>

    @Query("SELECT * FROM observations WHERE id = :id")
    suspend fun getById(id: String): ObservationEntity?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insertAll(observations: List<ObservationEntity>)

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(observation: ObservationEntity)

    @Update
    suspend fun update(observation: ObservationEntity)

    @Query("DELETE FROM observations")
    suspend fun deleteAll()

    @Query("DELETE FROM observations WHERE id = :id")
    suspend fun deleteById(id: String)
}
