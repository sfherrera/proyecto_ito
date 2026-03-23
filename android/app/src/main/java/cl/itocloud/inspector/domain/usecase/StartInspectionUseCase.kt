package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.repository.InspectionRepository
import javax.inject.Inject

class StartInspectionUseCase @Inject constructor(
    private val inspectionRepository: InspectionRepository
) {
    suspend operator fun invoke(inspectionId: String): Result<Inspection> {
        return try {
            val inspection = inspectionRepository.startInspection(inspectionId)
            Result.success(inspection)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
