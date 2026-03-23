package cl.itocloud.inspector.domain.usecase

import cl.itocloud.inspector.domain.model.Observation
import cl.itocloud.inspector.domain.repository.ObservationRepository
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

class GetObservationsUseCase @Inject constructor(
    private val observationRepository: ObservationRepository
) {
    operator fun invoke(): Flow<List<Observation>> {
        return observationRepository.getObservations()
    }
}
