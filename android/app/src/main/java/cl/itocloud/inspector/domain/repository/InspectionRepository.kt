package cl.itocloud.inspector.domain.repository

import cl.itocloud.inspector.domain.model.Answer
import cl.itocloud.inspector.domain.model.Inspection
import kotlinx.coroutines.flow.Flow

interface InspectionRepository {
    fun getMyInspections(): Flow<List<Inspection>>
    fun getInspectionDetail(inspectionId: String): Flow<Inspection>
    suspend fun startInspection(inspectionId: String): Inspection
    suspend fun saveAnswer(inspectionId: String, answer: Answer)
    suspend fun submitInspection(inspectionId: String, notes: String?): Inspection
    suspend fun syncInspections()
}
