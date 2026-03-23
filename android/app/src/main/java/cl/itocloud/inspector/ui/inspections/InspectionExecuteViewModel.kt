package cl.itocloud.inspector.ui.inspections

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.model.Answer
import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.model.Section
import cl.itocloud.inspector.domain.repository.InspectionRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class InspectionExecuteUiState(
    val inspection: Inspection? = null,
    val sections: List<Section> = emptyList(),
    val answers: Map<String, Answer> = emptyMap(),
    val expandedSections: Set<String> = emptySet(),
    val expandedNotes: Set<String> = emptySet(),
    val totalQuestions: Int = 0,
    val answeredQuestions: Int = 0,
    val conformingCount: Int = 0,
    val nonConformingCount: Int = 0,
    val isLoading: Boolean = false,
    val isSubmitting: Boolean = false,
    val showSubmitDialog: Boolean = false,
    val gpsLatitude: Double? = null,
    val gpsLongitude: Double? = null,
    val gpsAcquired: Boolean = false,
    val errorMessage: String? = null
)

sealed class ExecuteEvent {
    data object SubmitSuccess : ExecuteEvent()
    data class Error(val message: String) : ExecuteEvent()
}

@HiltViewModel
class InspectionExecuteViewModel @Inject constructor(
    savedStateHandle: SavedStateHandle,
    private val inspectionRepository: InspectionRepository
) : ViewModel() {

    private val inspectionId: String = checkNotNull(savedStateHandle["inspectionId"])

    private val _uiState = MutableStateFlow(InspectionExecuteUiState())
    val uiState: StateFlow<InspectionExecuteUiState> = _uiState.asStateFlow()

    private val _events = MutableSharedFlow<ExecuteEvent>()
    val events: SharedFlow<ExecuteEvent> = _events.asSharedFlow()

    init {
        loadInspection()
    }

    private fun loadInspection() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }
            try {
                inspectionRepository.getInspectionDetail(inspectionId).collect { inspection ->
                    val sections = inspection.sections ?: emptyList()
                    val totalQ = sections.sumOf { it.questions.size }
                    val firstSectionId = sections.firstOrNull()?.id

                    _uiState.update { state ->
                        state.copy(
                            inspection = inspection,
                            sections = sections,
                            totalQuestions = totalQ,
                            expandedSections = if (firstSectionId != null)
                                state.expandedSections + firstSectionId
                            else state.expandedSections,
                            isLoading = false
                        )
                    }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    fun saveAnswer(
        questionId: String,
        answerValue: String?,
        isConforming: Boolean?,
        isNa: Boolean,
        notes: String?
    ) {
        val answer = Answer(
            questionId = questionId,
            answerValue = answerValue,
            isConforming = isConforming,
            isNa = isNa,
            notes = notes
        )

        _uiState.update { state ->
            val newAnswers = state.answers.toMutableMap()
            newAnswers[questionId] = answer
            val answeredCount = newAnswers.size
            val conforming = newAnswers.values.count { it.isConforming == true }
            val nonConforming = newAnswers.values.count { it.isConforming == false && !it.isNa }

            state.copy(
                answers = newAnswers,
                answeredQuestions = answeredCount,
                conformingCount = conforming,
                nonConformingCount = nonConforming
            )
        }

        // Persist to Room immediately
        viewModelScope.launch {
            try {
                inspectionRepository.saveAnswer(inspectionId, answer)
            } catch (_: Exception) {
                // Already saved in state, will retry on sync
            }
        }
    }

    fun toggleSection(sectionId: String) {
        _uiState.update { state ->
            val expanded = state.expandedSections.toMutableSet()
            if (expanded.contains(sectionId)) {
                expanded.remove(sectionId)
            } else {
                expanded.add(sectionId)
            }
            state.copy(expandedSections = expanded)
        }
    }

    fun toggleNotes(questionId: String) {
        _uiState.update { state ->
            val expanded = state.expandedNotes.toMutableSet()
            if (expanded.contains(questionId)) {
                expanded.remove(questionId)
            } else {
                expanded.add(questionId)
            }
            state.copy(expandedNotes = expanded)
        }
    }

    fun showSubmitDialog() {
        _uiState.update { it.copy(showSubmitDialog = true) }
    }

    fun hideSubmitDialog() {
        _uiState.update { it.copy(showSubmitDialog = false) }
    }

    fun setGpsLocation(latitude: Double, longitude: Double) {
        _uiState.update {
            it.copy(gpsLatitude = latitude, gpsLongitude = longitude, gpsAcquired = true)
        }
    }

    fun submitInspection(weather: String, notes: String?) {
        viewModelScope.launch {
            _uiState.update { it.copy(isSubmitting = true, showSubmitDialog = false) }
            try {
                val fullNotes = buildString {
                    append("Clima: $weather")
                    if (!notes.isNullOrBlank()) {
                        append("\n$notes")
                    }
                }
                inspectionRepository.submitInspection(inspectionId, fullNotes)
                _uiState.update { it.copy(isSubmitting = false) }
                _events.emit(ExecuteEvent.SubmitSuccess)
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isSubmitting = false,
                        errorMessage = e.message ?: "Error al enviar inspecci\u00f3n"
                    )
                }
                _events.emit(ExecuteEvent.Error(e.message ?: "Error al enviar"))
            }
        }
    }
}
