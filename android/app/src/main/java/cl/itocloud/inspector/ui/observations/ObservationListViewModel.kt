package cl.itocloud.inspector.ui.observations

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.model.Observation
import cl.itocloud.inspector.domain.repository.ObservationRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class ObservationListUiState(
    val observations: List<Observation> = emptyList(),
    val filteredObservations: List<Observation> = emptyList(),
    val selectedSeverity: String? = null,
    val isLoading: Boolean = false,
    val isRefreshing: Boolean = false,
    val errorMessage: String? = null
)

@HiltViewModel
class ObservationListViewModel @Inject constructor(
    private val observationRepository: ObservationRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(ObservationListUiState())
    val uiState: StateFlow<ObservationListUiState> = _uiState.asStateFlow()

    init {
        loadObservations()
    }

    private fun loadObservations() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }
            try {
                observationRepository.getObservations().collect { observations ->
                    _uiState.update { state ->
                        state.copy(
                            observations = observations,
                            filteredObservations = applyFilter(observations, state.selectedSeverity),
                            isLoading = false,
                            isRefreshing = false
                        )
                    }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, isRefreshing = false, errorMessage = e.message)
                }
            }
        }
    }

    fun filterBySeverity(severity: String?) {
        _uiState.update { state ->
            val newSeverity = if (state.selectedSeverity == severity) null else severity
            state.copy(
                selectedSeverity = newSeverity,
                filteredObservations = applyFilter(state.observations, newSeverity)
            )
        }
    }

    fun refresh() {
        viewModelScope.launch {
            _uiState.update { it.copy(isRefreshing = true) }
            try {
                observationRepository.syncObservations()
            } catch (e: Exception) {
                _uiState.update { it.copy(isRefreshing = false, errorMessage = e.message) }
            }
        }
    }

    private fun applyFilter(
        observations: List<Observation>,
        severity: String?
    ): List<Observation> {
        return if (severity == null) observations
        else observations.filter { it.severity.equals(severity, ignoreCase = true) }
    }
}
