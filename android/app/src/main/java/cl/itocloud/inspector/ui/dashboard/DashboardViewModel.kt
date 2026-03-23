package cl.itocloud.inspector.ui.dashboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.model.InspectionStatus
import cl.itocloud.inspector.domain.repository.AuthRepository
import cl.itocloud.inspector.domain.repository.InspectionRepository
import cl.itocloud.inspector.domain.repository.ObservationRepository
import cl.itocloud.inspector.domain.repository.SyncRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import java.time.LocalDate
import java.time.format.DateTimeFormatter
import javax.inject.Inject

data class DashboardUiState(
    val userName: String = "",
    val pendingInspections: Int = 0,
    val todayInspections: Int = 0,
    val openObservations: Int = 0,
    val pendingSyncCount: Int = 0,
    val lastSyncTime: String? = null,
    val isLoading: Boolean = false
)

@HiltViewModel
class DashboardViewModel @Inject constructor(
    private val authRepository: AuthRepository,
    private val inspectionRepository: InspectionRepository,
    private val observationRepository: ObservationRepository,
    private val syncRepository: SyncRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(DashboardUiState())
    val uiState: StateFlow<DashboardUiState> = _uiState.asStateFlow()

    init {
        loadDashboardData()
    }

    fun loadDashboardData() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }

            try {
                // Load user info
                val user = authRepository.getCurrentUser()
                _uiState.update {
                    it.copy(userName = user?.firstName ?: "Inspector")
                }

                // Load inspections
                inspectionRepository.getMyInspections().collect { inspections ->
                    val today = LocalDate.now().format(DateTimeFormatter.ISO_LOCAL_DATE)
                    val pending = inspections.count {
                        it.status == InspectionStatus.Programada || it.status == InspectionStatus.EnProgreso
                    }
                    val todayCount = inspections.count {
                        it.scheduledDate?.startsWith(today) == true
                    }

                    _uiState.update {
                        it.copy(
                            pendingInspections = pending,
                            todayInspections = todayCount,
                            isLoading = false
                        )
                    }
                }
            } catch (e: Exception) {
                _uiState.update { it.copy(isLoading = false) }
            }
        }

        viewModelScope.launch {
            try {
                observationRepository.getObservations().collect { observations ->
                    val open = observations.count { it.status.lowercase() != "cerrada" && it.status.lowercase() != "resuelta" }
                    _uiState.update { it.copy(openObservations = open) }
                }
            } catch (_: Exception) { }
        }

        viewModelScope.launch {
            try {
                val pendingSync = syncRepository.getPendingSyncCount()
                _uiState.update { it.copy(pendingSyncCount = pendingSync) }
            } catch (_: Exception) { }
        }
    }
}
