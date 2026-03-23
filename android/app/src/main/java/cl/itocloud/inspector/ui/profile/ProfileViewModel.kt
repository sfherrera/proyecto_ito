package cl.itocloud.inspector.ui.profile

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import cl.itocloud.inspector.domain.model.User
import cl.itocloud.inspector.data.repository.AuthRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class ProfileUiState(
    val user: User? = null,
    val isLoading: Boolean = false,
    val isLoggingOut: Boolean = false,
    val loggedOut: Boolean = false,
    val errorMessage: String? = null
)

@HiltViewModel
class ProfileViewModel @Inject constructor(
    private val authRepository: AuthRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(ProfileUiState())
    val uiState: StateFlow<ProfileUiState> = _uiState.asStateFlow()

    init {
        loadProfile()
    }

    private fun loadProfile() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }
            try {
                val user = authRepository.getCurrentUser()
                _uiState.update { it.copy(user = user, isLoading = false) }
            } catch (e: Exception) {
                _uiState.update { it.copy(isLoading = false, errorMessage = e.message) }
            }
        }
    }

    fun logout() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoggingOut = true) }
            try {
                authRepository.logout()
                _uiState.update { it.copy(isLoggingOut = false, loggedOut = true) }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoggingOut = false, errorMessage = e.message)
                }
            }
        }
    }
}
