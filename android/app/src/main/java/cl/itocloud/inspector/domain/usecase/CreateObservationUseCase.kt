package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.domain.model.Observation
import cl.itocloud.inspector.domain.repository.ObservationRepository
import javax.inject.Inject

class CreateObservationUseCase @Inject constructor(
    private val observationRepository: ObservationRepository
) {
    suspend operator fun invoke(
        title: String,
        severity: String,
        inspectionId: String? = null,
        description: String? = null,
        dueDate: String? = null,
        latitude: Double? = null,
        longitude: Double? = null
    ): Result<Observation> {
        return try {
            val observation = observationRepository.createObservation(
                title = title,
                severity = severity,
                inspectionId = inspectionId,
                description = description,
                dueDate = dueDate,
                latitude = latitude,
                longitude = longitude
            )
            Result.success(observation)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
