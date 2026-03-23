package cl.itocloud.inspector.di

import cl.itocloud.inspector.domain.repository.InspectionRepository
import cl.itocloud.inspector.domain.repository.ObservationRepository
import cl.itocloud.inspector.domain.repository.SyncRepository
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import cl.itocloud.inspector.data.local.dao.AnswerDao
import cl.itocloud.inspector.data.local.dao.InspectionDao
import cl.itocloud.inspector.data.local.dao.ObservationDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.repository.InspectionRepositoryImpl
import cl.itocloud.inspector.data.repository.ObservationRepositoryImpl
import cl.itocloud.inspector.data.repository.SyncRepositoryImpl
import cl.itocloud.inspector.util.NetworkMonitor
import cl.itocloud.inspector.worker.SyncScheduler
import com.google.gson.Gson
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object RepositoryModule {

    @Provides
    @Singleton
    fun provideInspectionRepository(
        api: ItoApi,
        inspectionDao: InspectionDao,
        answerDao: AnswerDao,
        syncQueueDao: SyncQueueDao,
        networkMonitor: NetworkMonitor,
        syncScheduler: SyncScheduler,
        gson: Gson
    ): InspectionRepository {
        return InspectionRepositoryImpl(
            api = api,
            inspectionDao = inspectionDao,
            answerDao = answerDao,
            syncQueueDao = syncQueueDao,
            networkMonitor = networkMonitor,
            syncScheduler = syncScheduler,
            gson = gson
        )
    }

    @Provides
    @Singleton
    fun provideObservationRepository(
        api: ItoApi,
        observationDao: ObservationDao,
        syncQueueDao: SyncQueueDao,
        networkMonitor: NetworkMonitor,
        syncScheduler: SyncScheduler,
        gson: Gson
    ): ObservationRepository {
        return ObservationRepositoryImpl(
            api = api,
            observationDao = observationDao,
            syncQueueDao = syncQueueDao,
            networkMonitor = networkMonitor,
            syncScheduler = syncScheduler,
            gson = gson
        )
    }

    @Provides
    @Singleton
    fun provideSyncRepository(
        api: ItoApi,
        inspectionDao: InspectionDao,
        observationDao: ObservationDao,
        answerDao: AnswerDao,
        syncQueueDao: SyncQueueDao,
        gson: Gson
    ): SyncRepository {
        return SyncRepositoryImpl(
            api = api,
            inspectionDao = inspectionDao,
            observationDao = observationDao,
            answerDao = answerDao,
            syncQueueDao = syncQueueDao,
            gson = gson
        )
    }
}
