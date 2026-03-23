package cl.itocloud.inspector.data.repository

import cl.itocloud.inspector.data.local.dao.AnswerDao
import cl.itocloud.inspector.data.local.dao.InspectionDao
import cl.itocloud.inspector.data.local.dao.SyncQueueDao
import cl.itocloud.inspector.data.local.entities.AnswerEntity
import cl.itocloud.inspector.data.remote.api.ItoApi
import cl.itocloud.inspector.data.remote.dto.SectionDto
import cl.itocloud.inspector.domain.model.Answer
import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.model.InspectionStatus
import cl.itocloud.inspector.domain.model.Question
import cl.itocloud.inspector.domain.model.Section
import cl.itocloud.inspector.domain.repository.InspectionRepository as DomainInspectionRepository
import cl.itocloud.inspector.util.NetworkMonitor
import cl.itocloud.inspector.worker.SyncScheduler
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

/**
 * Domain-layer adapter that wraps the concrete InspectionRepository
 * and maps data-layer entities to domain models.
 */
class InspectionRepositoryImpl(
    private val api: ItoApi,
    private val inspectionDao: InspectionDao,
    private val answerDao: AnswerDao,
    private val syncQueueDao: SyncQueueDao,
    private val networkMonitor: NetworkMonitor,
    private val syncScheduler: SyncScheduler,
    private val gson: Gson
) : DomainInspectionRepository {

    private val dataRepo = InspectionRepository(api, inspectionDao, answerDao, syncQueueDao, gson)

    override fun getMyInspections(): Flow<List<Inspection>> {
        return dataRepo.getMyInspections().map { entities ->
            entities.map { entity ->
                entity.toDomainModel(gson)
            }
        }
    }

    override fun getInspectionDetail(inspectionId: String): Flow<Inspection> {
        return inspectionDao.getAll().map { entities ->
            val entity = entities.find { it.id == inspectionId }
                ?: throw NoSuchElementException("Inspection $inspectionId not found")
            entity.toDomainModel(gson)
        }
    }

    override suspend fun startInspection(inspectionId: String): Inspection {
        val result = dataRepo.startInspection(inspectionId)
        if (result.isFailure) {
            throw result.exceptionOrNull() ?: Exception("Failed to start inspection")
        }
        val entity = inspectionDao.getById(inspectionId)
            ?: throw NoSuchElementException("Inspection $inspectionId not found after start")
        return entity.toDomainModel(gson)
    }

    override suspend fun saveAnswer(inspectionId: String, answer: Answer) {
        dataRepo.saveAnswer(
            inspectionId = inspectionId,
            questionId = answer.questionId,
            answerValue = answer.answerValue,
            isConforming = answer.isConforming,
            isNa = answer.isNa,
            notes = answer.notes
        )
    }

    override suspend fun submitInspection(inspectionId: String, notes: String?): Inspection {
        val result = dataRepo.submitInspection(
            inspectionId = inspectionId,
            latitude = null,
            longitude = null,
            weatherConditions = null,
            notes = notes
        )
        if (result.isFailure) {
            throw result.exceptionOrNull() ?: Exception("Failed to submit inspection")
        }
        if (!networkMonitor.isCurrentlyConnected()) {
            syncScheduler.enqueueImmediateSync()
        }
        val entity = inspectionDao.getById(inspectionId)
            ?: throw NoSuchElementException("Inspection $inspectionId not found after submit")
        return entity.toDomainModel(gson)
    }

    override suspend fun syncInspections() {
        val success = dataRepo.syncInspections()
        if (!success) {
            throw Exception("Failed to sync inspections from API")
        }
    }
}

// ── Entity to Domain mapping ────────────────────────────────────────────────

internal fun cl.itocloud.inspector.data.local.entities.InspectionEntity.toDomainModel(
    gson: Gson
): Inspection {
    val parsedSections = if (!sectionsJson.isNullOrBlank()) {
        try {
            val type = object : TypeToken<List<SectionDto>>() {}.type
            val sectionDtos: List<SectionDto> = gson.fromJson(sectionsJson, type)
            sectionDtos.map { sectionDto ->
                Section(
                    id = sectionDto.id,
                    title = sectionDto.title,
                    orderIndex = sectionDto.orderIndex,
                    questions = sectionDto.questions.map { q ->
                        Question(
                            id = q.id,
                            text = q.questionText,
                            type = q.questionType,
                            isCritical = q.isCritical,
                            orderIndex = q.orderIndex,
                            options = q.options?.joinToString("|") { "${it.label}:${it.value}" }
                        )
                    }
                )
            }
        } catch (e: Exception) {
            null
        }
    } else null

    return Inspection(
        id = id,
        code = code,
        title = title,
        status = try {
            InspectionStatus.valueOf(status)
        } catch (e: IllegalArgumentException) {
            InspectionStatus.Programada
        },
        inspectionType = inspectionType,
        priority = priority,
        projectId = projectId,
        assignedToName = assignedToName,
        contractorName = contractorName,
        scheduledDate = scheduledDate,
        startedAt = startedAt,
        finishedAt = finishedAt,
        score = score,
        passed = passed,
        totalQuestions = totalQuestions,
        answeredQuestions = answeredQuestions,
        conformingCount = conformingCount,
        nonConformingCount = nonConformingCount,
        notes = notes,
        sections = parsedSections
    )
}
