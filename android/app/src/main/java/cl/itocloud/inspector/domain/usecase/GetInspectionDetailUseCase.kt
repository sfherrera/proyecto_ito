package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.repository.InspectionRepository
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

class GetInspectionDetailUseCase @Inject constructor(
    private val inspectionRepository: InspectionRepository
) {
    operator fun invoke(inspectionId: String): Flow<Inspection> {
        return inspectionRepository.getInspectionDetail(inspectionId)
    }
}
