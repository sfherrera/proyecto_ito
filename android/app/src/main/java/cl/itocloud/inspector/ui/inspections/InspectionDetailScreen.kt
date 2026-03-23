package cl.itocloud.inspector.ui.inspections

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Business
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.filled.Engineering
import androidx.compose.material.icons.filled.Flag
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.PlayArrow
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.HorizontalDivider
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
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import cl.itocloud.inspector.domain.model.Inspection
import cl.itocloud.inspector.domain.model.InspectionStatus
import cl.itocloud.inspector.ui.components.LoadingOverlay
import cl.itocloud.inspector.ui.components.StatusChip
import cl.itocloud.inspector.ui.theme.ConformeGreen
import cl.itocloud.inspector.ui.theme.ITOBlue
import cl.itocloud.inspector.ui.theme.NoConformeRed
import cl.itocloud.inspector.ui.theme.StatusCompletada
import cl.itocloud.inspector.ui.theme.StatusEnProgreso

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun InspectionDetailScreen(
    viewModel: InspectionDetailViewModel,
    onExecuteClick: (String) -> Unit,
    onBackClick: () -> Unit
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Text(uiState.inspection?.code ?: "Detalle Inspecci\u00f3n")
                },
                navigationIcon = {
                    IconButton(onClick = onBackClick) {
                        Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Volver")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surface
                )
            )
        }
    ) { innerPadding ->
        val inspection = uiState.inspection

        if (inspection != null) {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(innerPadding)
                    .verticalScroll(rememberScrollState())
                    .padding(16.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                // Header Card
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(12.dp),
                    colors = CardDefaults.cardColors(
                        containerColor = ITOBlue.copy(alpha = 0.05f)
                    )
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
                                text = inspection.code,
                                style = MaterialTheme.typography.labelLarge,
                                color = ITOBlue,
                                fontWeight = FontWeight.Bold
                            )
                            StatusChip(status = inspection.status)
                        }
                        Spacer(modifier = Modifier.height(8.dp))
                        Text(
                            text = inspection.title,
                            style = MaterialTheme.typography.titleLarge
                        )
                    }
                }

                // Info Card
                Card(
                    modifier = Modifier.fillMaxWidth(),
                    shape = RoundedCornerShape(12.dp)
                ) {
                    Column(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(16.dp),
                        verticalArrangement = Arrangement.spacedBy(12.dp)
                    ) {
                        Text(
                            text = "Informaci\u00f3n",
                            style = MaterialTheme.typography.titleMedium,
                            fontWeight = FontWeight.Bold
                        )

                        HorizontalDivider()

                        InfoRow(
                            icon = Icons.Default.Business,
                            label = "Proyecto",
                            value = inspection.projectId
                        )
                        InfoRow(
                            icon = Icons.Default.Person,
                            label = "Inspector",
                            value = inspection.assignedToName ?: "Sin asignar"
                        )
                        InfoRow(
                            icon = Icons.Default.Engineering,
                            label = "Contratista",
                            value = inspection.contractorName ?: "N/A"
                        )
                        InfoRow(
                            icon = Icons.Default.CalendarMonth,
                            label = "Fecha Programada",
                            value = inspection.scheduledDate?.take(10) ?: "Sin fecha"
                        )
                        InfoRow(
                            icon = Icons.Default.Flag,
                            label = "Prioridad",
                            value = inspection.priority
                        )
                    }
                }

                // Score Card (if completed)
                if (inspection.status == InspectionStatus.Completada) {
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(12.dp),
                        colors = CardDefaults.cardColors(
                            containerColor = if (inspection.passed == true)
                                StatusCompletada.copy(alpha = 0.08f)
                            else NoConformeRed.copy(alpha = 0.08f)
                        )
                    ) {
                        Column(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(16.dp),
                            horizontalAlignment = Alignment.CenterHorizontally
                        ) {
                            Text(
                                text = "Resultado",
                                style = MaterialTheme.typography.titleMedium,
                                fontWeight = FontWeight.Bold
                            )
                            Spacer(modifier = Modifier.height(12.dp))

                            Row(
                                verticalAlignment = Alignment.CenterVertically,
                                horizontalArrangement = Arrangement.spacedBy(8.dp)
                            ) {
                                Icon(
                                    if (inspection.passed == true) Icons.Default.CheckCircle
                                    else Icons.Default.Star,
                                    contentDescription = null,
                                    tint = if (inspection.passed == true) StatusCompletada else NoConformeRed,
                                    modifier = Modifier.size(32.dp)
                                )
                                Text(
                                    text = if (inspection.passed == true) "APROBADA" else "NO APROBADA",
                                    style = MaterialTheme.typography.titleLarge,
                                    fontWeight = FontWeight.Bold,
                                    color = if (inspection.passed == true) StatusCompletada else NoConformeRed
                                )
                            }

                            if (inspection.score != null) {
                                Spacer(modifier = Modifier.height(8.dp))
                                Text(
                                    text = "Puntaje: ${String.format("%.1f", inspection.score)}%",
                                    style = MaterialTheme.typography.headlineMedium,
                                    fontWeight = FontWeight.Bold,
                                    color = if (inspection.passed == true) StatusCompletada else NoConformeRed
                                )
                            }

                            Spacer(modifier = Modifier.height(12.dp))
                            HorizontalDivider()
                            Spacer(modifier = Modifier.height(12.dp))

                            Row(
                                modifier = Modifier.fillMaxWidth(),
                                horizontalArrangement = Arrangement.SpaceEvenly
                            ) {
                                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                    Text(
                                        text = "${inspection.conformingCount}",
                                        style = MaterialTheme.typography.titleLarge,
                                        fontWeight = FontWeight.Bold,
                                        color = ConformeGreen
                                    )
                                    Text(
                                        text = "Conforme",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                    Text(
                                        text = "${inspection.nonConformingCount}",
                                        style = MaterialTheme.typography.titleLarge,
                                        fontWeight = FontWeight.Bold,
                                        color = NoConformeRed
                                    )
                                    Text(
                                        text = "No Conforme",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                                    Text(
                                        text = "${inspection.totalQuestions}",
                                        style = MaterialTheme.typography.titleLarge,
                                        fontWeight = FontWeight.Bold
                                    )
                                    Text(
                                        text = "Total",
                                        style = MaterialTheme.typography.bodySmall,
                                        color = MaterialTheme.colorScheme.onSurfaceVariant
                                    )
                                }
                            }
                        }
                    }
                }

                // Action Buttons
                when (inspection.status) {
                    InspectionStatus.Programada -> {
                        Button(
                            onClick = { viewModel.startInspection() },
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(56.dp),
                            enabled = !uiState.isStarting,
                            shape = RoundedCornerShape(12.dp),
                            colors = ButtonDefaults.buttonColors(containerColor = ITOBlue)
                        ) {
                            if (uiState.isStarting) {
                                CircularProgressIndicator(
                                    modifier = Modifier.size(24.dp),
                                    color = Color.White,
                                    strokeWidth = 2.dp
                                )
                            } else {
                                Icon(Icons.Default.PlayArrow, contentDescription = null)
                                Spacer(modifier = Modifier.width(8.dp))
                                Text("Iniciar Inspecci\u00f3n", style = MaterialTheme.typography.titleMedium)
                            }
                        }
                    }
                    InspectionStatus.EnProgreso -> {
                        Button(
                            onClick = { onExecuteClick(inspection.id) },
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(56.dp),
                            shape = RoundedCornerShape(12.dp),
                            colors = ButtonDefaults.buttonColors(containerColor = StatusEnProgreso)
                        ) {
                            Icon(Icons.Default.PlayArrow, contentDescription = null)
                            Spacer(modifier = Modifier.width(8.dp))
                            Text("Continuar Inspecci\u00f3n", style = MaterialTheme.typography.titleMedium)
                        }
                    }
                    else -> { /* No action buttons */ }
                }

                Spacer(modifier = Modifier.height(16.dp))
            }
        }

        LoadingOverlay(isLoading = uiState.isLoading)
    }
}

@Composable
private fun InfoRow(
    icon: ImageVector,
    label: String,
    value: String
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Icon(
            imageVector = icon,
            contentDescription = null,
            modifier = Modifier.size(20.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Spacer(modifier = Modifier.width(12.dp))
        Column {
            Text(
                text = label,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Text(
                text = value,
                style = MaterialTheme.typography.bodyLarge
            )
        }
    }
}
