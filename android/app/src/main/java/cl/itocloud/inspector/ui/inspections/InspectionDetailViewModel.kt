package cl.itocloud.inspector.ui.inspections

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.repository.InspectionRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class InspectionDetailUiState(
    val inspection: Inspection? = null,
    val isLoading: Boolean = false,
    val isStarting: Boolean = false,
    val errorMessage: String? = null
)

@HiltViewModel
class InspectionDetailViewModel @Inject constructor(
    savedStateHandle: SavedStateHandle,
    private val inspectionRepository: InspectionRepository
) : ViewModel() {

    private val inspectionId: String = checkNotNull(savedStateHandle["inspectionId"])

    private val _uiState = MutableStateFlow(InspectionDetailUiState())
    val uiState: StateFlow<InspectionDetailUiState> = _uiState.asStateFlow()

    init {
        loadInspection()
    }

    private fun loadInspection() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }
            try {
                inspectionRepository.getInspectionDetail(inspectionId).collect { inspection ->
                    _uiState.update {
                        it.copy(inspection = inspection, isLoading = false)
                    }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = e.message)
                }
            }
        }
    }

    fun startInspection() {
        viewModelScope.launch {
            _uiState.update { it.copy(isStarting = true) }
            try {
                val started = inspectionRepository.startInspection(inspectionId)
                _uiState.update {
                    it.copy(inspection = started, isStarting = false)
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isStarting = false, errorMessage = e.message)
                }
            }
        }
    }
}
