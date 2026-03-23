package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.repository.InspectionRepository
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

class GetMyInspectionsUseCase @Inject constructor(
    private val inspectionRepository: InspectionRepository
) {
    operator fun invoke(): Flow<List<Inspection>> {
        return inspectionRepository.getMyInspections()
    }
}
