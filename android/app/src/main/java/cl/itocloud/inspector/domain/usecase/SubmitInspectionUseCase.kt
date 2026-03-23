package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.repository.InspectionRepository
import javax.inject.Inject

class SubmitInspectionUseCase @Inject constructor(
    private val inspectionRepository: InspectionRepository
) {
    suspend operator fun invoke(inspectionId: String, notes: String? = null): Result<Inspection> {
        return try {
            val inspection = inspectionRepository.submitInspection(inspectionId, notes)
            Result.success(inspection)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
