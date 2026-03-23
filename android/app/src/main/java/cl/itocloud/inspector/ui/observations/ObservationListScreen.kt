package cl.itocloud.inspector.ui.observations

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FilterChip
import androidx.compose.material3.FilterChipDefaults
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import cl.itocloud.inspector.domain.model.Observation
import cl.itocloud.inspector.ui.components.EmptyState
import cl.itocloud.inspector.ui.components.LoadingOverlay
import cl.itocloud.inspector.ui.components.PullRefreshWrapper
import cl.itocloud.inspector.ui.components.SeverityChip
import cl.itocloud.inspector.ui.theme.ITOBlue
import cl.itocloud.inspector.ui.theme.NoConformeRed

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ObservationListScreen(
    viewModel: ObservationListViewModel,
    onNewObservationClick: () -> Unit,
    onBackClick: () -> Unit
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()
    val severities = listOf("Baja", "Media", "Alta", "Cr\u00edtica")

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Observaciones") },
                navigationIcon = {
                    IconButton(onClick = onBackClick) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surface
                )
            )
        },
        floatingActionButton = {
            FloatingActionButton(
                onClick = onNewObservationClick,
                containerColor = ITOBlue
            ) {
                Icon(Icons.Default.Add, contentDescription = "Nueva Observaci\u00f3n")
            }
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
        ) {
            // Severity filter chips
            LazyRow(
                contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp),
                horizontalArrangement = Arrangement.spacedBy(8.dp)
            ) {
                items(severities) { severity ->
                    FilterChip(
                        selected = uiState.selectedSeverity == severity,
                        onClick = { viewModel.filterBySeverity(severity) },
                        label = { Text(severity) },
                        colors = FilterChipDefaults.filterChipColors(
                            selectedContainerColor = ITOBlue.copy(alpha = 0.15f),
                            selectedLabelColor = ITOBlue
                        )
                    )
                }
            }

            PullRefreshWrapper(
                isRefreshing = uiState.isRefreshing,
                onRefresh = viewModel::refresh,
                modifier = Modifier.fillMaxSize()
            ) {
                if (uiState.filteredObservations.isEmpty() && !uiState.isLoading) {
                    EmptyState(
                        message = "No hay observaciones",
                        subtitle = "Las observaciones registradas aparecer\u00e1n aqu\u00ed",
                        icon = Icons.Default.Warning
                    )
                } else {
                    LazyColumn(
                        contentPadding = PaddingValues(16.dp),
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        items(
                            items = uiState.filteredObservations,
                            key = { it.id }
                        ) { observation ->
                            ObservationCard(observation = observation)
                        }
                    }
                }
            }
        }

        LoadingOverlay(isLoading = uiState.isLoading)
    }
}

@Composable
private fun ObservationCard(observation: Observation) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp)
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = observation.code,
                    style = MaterialTheme.typography.labelMedium,
                    color = ITOBlue,
                    fontWeight = FontWeight.Bold
                )
                SeverityChip(severity = observation.severity)
            }

            Spacer(modifier = Modifier.height(8.dp))

            Text(
                text = observation.title,
                style = MaterialTheme.typography.titleMedium,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )

            Spacer(modifier = Modifier.height(8.dp))

            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Text(
                    text = observation.status,
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )

                Spacer(modifier = Modifier.weight(1f))

                if (observation.dueDate != null) {
                    Icon(
                        Icons.Default.CalendarMonth,
                        contentDescription = null,
                        modifier = Modifier.height(14.dp),
                        tint = if (observation.isOverdue) NoConformeRed
                        else MaterialTheme.colorScheme.onSurfaceVariant
                    )
                    Spacer(modifier = Modifier.width(4.dp))
                    Text(
                        text = observation.dueDate.take(10),
                        style = MaterialTheme.typography.bodySmall,
                        color = if (observation.isOverdue) NoConformeRed
                        else MaterialTheme.colorScheme.onSurfaceVariant,
                        fontWeight = if (observation.isOverdue) FontWeight.Bold else FontWeight.Normal
                    )
                }
            }
        }
    }
}
