package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.domain.model.Answer
import cl.itocloud.inspector.domain.repository.InspectionRepository
import javax.inject.Inject

class SaveAnswerUseCase @Inject constructor(
    private val inspectionRepository: InspectionRepository
) {
    suspend operator fun invoke(inspectionId: String, answer: Answer): Result<Unit> {
        return try {
            inspectionRepository.saveAnswer(inspectionId, answer)
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
