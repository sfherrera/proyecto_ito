package cl.itocloud.inspector.ui.components

import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Error
import androidx.compose.material.icons.filled.Info
import androidx.compose.material.icons.filled.Warning
import androidx.compose.material3.AssistChip
import androidx.compose.material3.AssistChipDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import cl.itocloud.inspector.ui.theme.SeverityAlta
import cl.itocloud.inspector.ui.theme.SeverityBaja
import cl.itocloud.inspector.ui.theme.SeverityCritica
import cl.itocloud.inspector.ui.theme.SeverityMedia

@Composable
fun SeverityChip(
    severity: String,
    modifier: Modifier = Modifier
) {
    val (label, color, icon) = when (severity.lowercase()) {
        "baja" -> Triple("Baja", SeverityBaja, Icons.Default.Info)
        "media" -> Triple("Media", SeverityMedia, Icons.Default.Info)
        "alta" -> Triple("Alta", SeverityAlta, Icons.Default.Warning)
        "critica", "cr\u00edtica" -> Triple("Cr\u00edtica", SeverityCritica, Icons.Default.Error)
        else -> Triple(severity, SeverityMedia, Icons.Default.Info)
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
