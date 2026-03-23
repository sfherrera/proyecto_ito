package cl.itocloud.inspector.ui.sync

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.repository.SyncRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class SyncUiState(
    val pendingSyncCount: Int = 0,
    val lastSyncTime: String? = null,
    val isSyncing: Boolean = false,
    val isOnline: Boolean = true,
    val syncMessage: String? = null,
    val errorMessage: String? = null
)

@HiltViewModel
class SyncViewModel @Inject constructor(
    private val syncRepository: SyncRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(SyncUiState())
    val uiState: StateFlow<SyncUiState> = _uiState.asStateFlow()

    init {
        loadSyncStatus()
    }

    private fun loadSyncStatus() {
        viewModelScope.launch {
            try {
                val count = syncRepository.getPendingSyncCount()
                _uiState.update { it.copy(pendingSyncCount = count) }
            } catch (_: Exception) { }
        }
    }

    fun syncNow() {
        viewModelScope.launch {
            _uiState.update { it.copy(isSyncing = true, errorMessage = null, syncMessage = null) }
            try {
                syncRepository.syncAll()
                val count = syncRepository.getPendingSyncCount()
                _uiState.update {
                    it.copy(
                        isSyncing = false,
                        pendingSyncCount = count,
                        syncMessage = "Sincronizaci\u00f3n completada exitosamente"
                    )
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(
                        isSyncing = false,
                        errorMessage = e.message ?: "Error al sincronizar"
                    )
                }
            }
        }
    }
}
