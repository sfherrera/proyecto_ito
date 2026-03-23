package cl.itocloud.inspector.data.local

import android.content.Context
import androidx.room.Database
import androidx.room.Room
import androidx.room.RoomDatabase
import cl.itocloud.inspector.data.local.dao.AnswerDao
import cl.itocloud.inspector.data.local.dao.InspectionDao
import cl.itocloud.inspector.data.local.dao.ObservationDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.local.entities.AnswerEntity
import cl.itocloud.inspector.data.local.entities.InspectionEntity
import cl.itocloud.inspector.data.local.entities.ObservationEntity
import cl.itocloud.inspector.data.local.entities.SyncQueueEntity

@Database(
    entities = [
        InspectionEntity::class,
        AnswerEntity::class,
        ObservationEntity::class,
        SyncQueueEntity::class
    ],
    version = 1,
    exportSchema = false
)
abstract class AppDatabase : RoomDatabase() {

    abstract fun inspectionDao(): InspectionDao
    abstract fun answerDao(): AnswerDao
    abstract fun observationDao(): ObservationDao
    abstract fun syncQueueDao(): SyncQueueDao

    companion object {
        private const val DATABASE_NAME = "ito_inspector.db"

        @Volatile
        private var INSTANCE: AppDatabase? = null

        fun getInstance(context: Context): AppDatabase {
            return INSTANCE ?: synchronized(this) {
                INSTANCE ?: Room.databaseBuilder(
                    context.applicationContext,
                    AppDatabase::class.java,
                    DATABASE_NAME
                )
                    .fallbackToDestructiveMigration()
                    .build()
                    .also { INSTANCE = it }
            }
        }
    }
}
