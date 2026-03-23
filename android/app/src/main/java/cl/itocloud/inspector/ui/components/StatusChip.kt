package cl.itocloud.inspector.ui.components

import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Cancel
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.filled.PlayCircle
import androidx.compose.material.icons.filled.Schedule
import androidx.compose.material3.AssistChip
import androidx.compose.material3.AssistChipDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import cl.itocloud.inspector.domain.model.InspectionStatus
import cl.itocloud.inspector.ui.theme.StatusCancelada
import cl.itocloud.inspector.ui.theme.StatusCompletada
import cl.itocloud.inspector.ui.theme.StatusEnProgreso
import cl.itocloud.inspector.ui.theme.StatusProgramada

@Composable
fun StatusChip(
    status: InspectionStatus,
    modifier: Modifier = Modifier
) {
    val (label, color, icon) = when (status) {
        InspectionStatus.Programada -> Triple("Programada", StatusProgramada, Icons.Default.Schedule)
        InspectionStatus.EnProgreso -> Triple("En Progreso", StatusEnProgreso, Icons.Default.PlayCircle)
        InspectionStatus.Completada -> Triple("Completada", StatusCompletada, Icons.Default.CheckCircle)
        InspectionStatus.Cancelada -> Triple("Cancelada", StatusCancelada, Icons.Default.Cancel)
    }

    AssistChip(
        onClick = { },
        label = { Text(label) },
        modifier = modifier,
        leadingIcon = {
            Icon(
                imageVector = icon,
                contentDescription = null,
                modifier = Modifier.size(AssistChipDefaults.IconSize),
                tint = color
            )
        },
        colors = AssistChipDefaults.assistChipColors(
            containerColor = color.copy(alpha = 0.12f),
            labelColor = color
        ),
        border = AssistChipDefaults.assistChipBorder(
            enabled = true,
            borderColor = color.copy(alpha = 0.3f)
        )
    )
}

@Composable
fun StatusChip(
    status: String,
    modifier: Modifier = Modifier
) {
    val inspectionStatus = try {
        InspectionStatus.valueOf(status)
    } catch (_: Exception) {
        InspectionStatus.Programada
    }
    StatusChip(status = inspectionStatus, modifier = modifier)
}
