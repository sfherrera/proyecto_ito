package cl.itocloud.inspector.ui.inspections

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.model.InspectionStatus
import cl.itocloud.inspector.domain.repository.InspectionRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class InspectionListUiState(
    val inspections: List<Inspection> = emptyList(),
    val filteredInspections: List<Inspection> = emptyList(),
    val selectedFilter: InspectionFilter = InspectionFilter.TODAS,
    val isLoading: Boolean = false,
    val isRefreshing: Boolean = false,
    val errorMessage: String? = null
)

enum class InspectionFilter(val label: String) {
    TODAS("Todas"),
    PROGRAMADAS("Programadas"),
    EN_PROGRESO("En Progreso"),
    COMPLETADAS("Completadas")
}

@HiltViewModel
class InspectionListViewModel @Inject constructor(
    private val inspectionRepository: InspectionRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(InspectionListUiState())
    val uiState: StateFlow<InspectionListUiState> = _uiState.asStateFlow()

    init {
        loadInspections()
    }

    private fun loadInspections() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }
            try {
                inspectionRepository.getMyInspections().collect { inspections ->
                    _uiState.update { state ->
                        state.copy(
                            inspections = inspections,
                            filteredInspections = applyFilter(inspections, state.selectedFilter),
                            isLoading = false,
                            isRefreshing = false
                        )
                    }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isLoading = false,
                        isRefreshing = false,
                        errorMessage = e.message
                    )
                }
            }
        }
    }

    fun filterByStatus(filter: InspectionFilter) {
        _uiState.update { state ->
            state.copy(
                selectedFilter = filter,
                filteredInspections = applyFilter(state.inspections, filter)
            )
        }
    }

    fun refresh() {
        viewModelScope.launch {
            _uiState.update { it.copy(isRefreshing = true) }
            try {
                inspectionRepository.syncInspections()
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isRefreshing = false, errorMessage = e.message)
                }
            }
        }
    }

    private fun applyFilter(
        inspections: List<Inspection>,
        filter: InspectionFilter
    ): List<Inspection> {
        return when (filter) {
            InspectionFilter.TODAS -> inspections
            InspectionFilter.PROGRAMADAS -> inspections.filter { it.status == InspectionStatus.Programada }
            InspectionFilter.EN_PROGRESO -> inspections.filter { it.status == InspectionStatus.EnProgreso }
            InspectionFilter.COMPLETADAS -> inspections.filter { it.status == InspectionStatus.Completada }
        }
    }
}
