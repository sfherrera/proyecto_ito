package cl.itocloud.inspector.domain.repository

import cl.itocloud.inspector.domain.model.Observation
import kotlinx.coroutines.flow.Flow

interface ObservationRepository {
    fun getObservations(): Flow<List<Observation>>
    suspend fun createObservation(
        title: String,
        severity: String,
        inspectionId: String?,
        description: String?,
        dueDate: String?,
        latitude: Double?,
        longitude: Double?
    ): Observation
    suspend fun syncObservations()
}
