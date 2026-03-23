package cl.itocloud.inspector.ui.observations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.model.Project
import cl.itocloud.inspector.domain.repository.ObservationRepository
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

data class ObservationNewUiState(
    val projects: List<Project> = emptyList(),
    val selectedProjectId: String? = null,
    val title: String = "",
    val description: String = "",
    val severity: String = "Media",
    val category: String = "",
    val locationDescription: String = "",
    val dueDate: String = "",
    val photoUri: String? = null,
    val isLoading: Boolean = false,
    val isSaving: Boolean = false,
    val errorMessage: String? = null
)

sealed class ObservationNewEvent {
    data object Created : ObservationNewEvent()
    data class Error(val message: String) : ObservationNewEvent()
}

@HiltViewModel
class ObservationNewViewModel @Inject constructor(
    private val observationRepository: ObservationRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(ObservationNewUiState())
    val uiState: StateFlow<ObservationNewUiState> = _uiState.asStateFlow()

    private val _events = MutableSharedFlow<ObservationNewEvent>()
    val events: SharedFlow<ObservationNewEvent> = _events.asSharedFlow()

    fun onProjectSelected(projectId: String) {
        _uiState.update { it.copy(selectedProjectId = projectId) }
    }

    fun onTitleChange(title: String) {
        _uiState.update { it.copy(title = title, errorMessage = null) }
    }

    fun onDescriptionChange(description: String) {
        _uiState.update { it.copy(description = description) }
    }

    fun onSeverityChange(severity: String) {
        _uiState.update { it.copy(severity = severity) }
    }

    fun onCategoryChange(category: String) {
        _uiState.update { it.copy(category = category) }
    }

    fun onLocationChange(location: String) {
        _uiState.update { it.copy(locationDescription = location) }
    }

    fun onDueDateChange(dueDate: String) {
        _uiState.update { it.copy(dueDate = dueDate) }
    }

    fun onPhotoTaken(uri: String) {
        _uiState.update { it.copy(photoUri = uri) }
    }

    fun submit() {
        val state = _uiState.value

        if (state.title.isBlank()) {
            _uiState.update { it.copy(errorMessage = "Ingrese un t\u00edtulo para la observaci\u00f3n") }
            return
        }

        viewModelScope.launch {
            _uiState.update { it.copy(isSaving = true, errorMessage = null) }
            try {
                observationRepository.createObservation(
                    title = state.title,
                    severity = state.severity,
                    inspectionId = null,
                    description = state.description.ifBlank { null },
                    dueDate = state.dueDate.ifBlank { null },
                    latitude = null,
                    longitude = null
                )
                _uiState.update { it.copy(isSaving = false) }
                _events.emit(ObservationNewEvent.Created)
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isSaving = false,
                        errorMessage = e.message ?: "Error al crear observaci\u00f3n"
                    )
                }
                _events.emit(ObservationNewEvent.Error(e.message ?: "Error"))
            }
        }
    }
}
