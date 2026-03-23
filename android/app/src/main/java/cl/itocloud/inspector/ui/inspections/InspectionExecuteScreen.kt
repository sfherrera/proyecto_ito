package cl.itocloud.inspector.ui.inspections

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.expandVertically
import androidx.compose.animation.shrinkVertically
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.automirrored.filled.Notes
import androidx.compose.material.icons.filled.Cancel
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.ExpandLess
import androidx.compose.material.icons.filled.ExpandMore
import androidx.compose.material.icons.filled.GpsFixed
import androidx.compose.material.icons.filled.HorizontalRule
import androidx.compose.material.icons.filled.Send
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.BottomAppBar
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.IconButtonDefaults
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedIconButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import cl.itocloud.inspector.domain.model.Answer
import cl.itocloud.inspector.domain.model.Question
import cl.itocloud.inspector.domain.model.Section
import cl.itocloud.inspector.ui.components.LoadingOverlay
import cl.itocloud.inspector.ui.theme.ConformeGreen
import cl.itocloud.inspector.ui.theme.ITOBlue
import cl.itocloud.inspector.ui.theme.NaGray
import cl.itocloud.inspector.ui.theme.NoConformeRed
import cl.itocloud.inspector.ui.theme.StatusCompletada

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun InspectionExecuteScreen(
    viewModel: InspectionExecuteViewModel,
    onSubmitSuccess: () -> Unit,
    onBackClick: () -> Unit
) {
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    LaunchedEffect(Unit) {
        viewModel.events.collect { event ->
            when (event) {
                is ExecuteEvent.SubmitSuccess -> onSubmitSuccess()
                is ExecuteEvent.Error -> { /* Error is shown in UI state */ }
            }
        }
    }

    if (uiState.showSubmitDialog) {
        SubmitInspectionDialog(
            onDismiss = viewModel::hideSubmitDialog,
            onConfirm = { weather, notes -> viewModel.submitInspection(weather, notes) }
        )
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Column {
                        Text(
                            text = uiState.inspection?.code ?: "",
                            style = MaterialTheme.typography.titleMedium,
                            maxLines = 1,
                            overflow = TextOverflow.Ellipsis
                        )
                        Text(
                            text = "${uiState.answeredQuestions}/${uiState.totalQuestions} respondidas",
                            style = MaterialTheme.typography.bodySmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
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
        },
        bottomBar = {
            BottomAppBar(
                containerColor = MaterialTheme.colorScheme.surface,
                tonalElevation = 8.dp
            ) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 16.dp),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    // GPS Indicator
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Icon(
                            Icons.Default.GpsFixed,
                            contentDescription = null,
                            tint = if (uiState.gpsAcquired) StatusCompletada else NaGray,
                            modifier = Modifier.size(20.dp)
                        )
                        Spacer(modifier = Modifier.width(4.dp))
                        Text(
                            text = if (uiState.gpsAcquired) "GPS: OK" else "GPS: ---",
                            style = MaterialTheme.typography.bodySmall,
                            color = if (uiState.gpsAcquired) StatusCompletada else NaGray
                        )
                    }

                    // Submit Button
                    Button(
                        onClick = viewModel::showSubmitDialog,
                        enabled = uiState.answeredQuestions > 0 && !uiState.isSubmitting,
                        shape = RoundedCornerShape(12.dp),
                        colors = ButtonDefaults.buttonColors(containerColor = ITOBlue)
                    ) {
                        if (uiState.isSubmitting) {
                            CircularProgressIndicator(
                                modifier = Modifier.size(20.dp),
                                color = Color.White,
                                strokeWidth = 2.dp
                            )
                        } else {
                            Icon(
                                Icons.Default.Send,
                                contentDescription = null,
                                modifier = Modifier.size(18.dp)
                            )
                            Spacer(modifier = Modifier.width(8.dp))
                            Text("Enviar")
                        }
                    }
                }
            }
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
        ) {
            // Progress bar
            val progress = if (uiState.totalQuestions > 0)
                uiState.answeredQuestions.toFloat() / uiState.totalQuestions.toFloat()
            else 0f

            LinearProgressIndicator(
                progress = { progress },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(6.dp),
                color = ITOBlue,
                trackColor = ITOBlue.copy(alpha = 0.12f)
            )

            // Summary bar
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .background(MaterialTheme.colorScheme.surfaceVariant.copy(alpha = 0.3f))
                    .padding(horizontal = 16.dp, vertical = 8.dp),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                SummaryItem(
                    label = "Conforme",
                    count = uiState.conformingCount,
                    color = ConformeGreen
                )
                SummaryItem(
                    label = "No Conforme",
                    count = uiState.nonConformingCount,
                    color = NoConformeRed
                )
                SummaryItem(
                    label = "Pendiente",
                    count = uiState.totalQuestions - uiState.answeredQuestions,
                    color = NaGray
                )
            }

            // Sections with questions
            LazyColumn(
                contentPadding = PaddingValues(16.dp),
                verticalArrangement = Arrangement.spacedBy(12.dp)
            ) {
                items(
                    items = uiState.sections,
                    key = { it.id }
                ) { section ->
                    SectionCard(
                        section = section,
                        isExpanded = uiState.expandedSections.contains(section.id),
                        answers = uiState.answers,
                        expandedNotes = uiState.expandedNotes,
                        onToggleSection = { viewModel.toggleSection(section.id) },
                        onToggleNotes = { questionId -> viewModel.toggleNotes(questionId) },
                        onAnswerChange = { questionId, answerValue, isConforming, isNa, notes ->
                            viewModel.saveAnswer(questionId, answerValue, isConforming, isNa, notes)
                        }
                    )
                }
            }
        }

        LoadingOverlay(
            isLoading = uiState.isLoading || uiState.isSubmitting,
            message = if (uiState.isSubmitting) "Enviando inspecci\u00f3n..." else null
        )
    }
}

@Composable
private fun SummaryItem(label: String, count: Int, color: Color) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(
            text = "$count",
            style = MaterialTheme.typography.titleMedium,
            fontWeight = FontWeight.Bold,
            color = color
        )
        Text(
            text = label,
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            fontSize = 10.sp
        )
    }
}

@Composable
private fun SectionCard(
    section: Section,
    isExpanded: Boolean,
    answers: Map<String, Answer>,
    expandedNotes: Set<String>,
    onToggleSection: () -> Unit,
    onToggleNotes: (String) -> Unit,
    onAnswerChange: (String, String?, Boolean?, Boolean, String?) -> Unit
) {
    val answeredInSection = section.questions.count { q -> answers.containsKey(q.id) }
    val totalInSection = section.questions.size

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        elevation = CardDefaults.cardElevation(defaultElevation = 2.dp)
    ) {
        Column(modifier = Modifier.fillMaxWidth()) {
            // Section Header
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .clickable(onClick = onToggleSection)
                    .padding(16.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                Column(modifier = Modifier.weight(1f)) {
                    Text(
                        text = section.title,
                        style = MaterialTheme.typography.titleMedium,
                        fontWeight = FontWeight.SemiBold
                    )
                    Text(
                        text = "$answeredInSection/$totalInSection respondidas",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }

                // Mini progress
                if (totalInSection > 0) {
                    val sectionProgress = answeredInSection.toFloat() / totalInSection.toFloat()
                    CircularProgressIndicator(
                        progress = { sectionProgress },
                        modifier = Modifier.size(32.dp),
                        strokeWidth = 3.dp,
                        color = if (answeredInSection == totalInSection) ConformeGreen else ITOBlue,
                        trackColor = ITOBlue.copy(alpha = 0.12f)
                    )
                }

                Spacer(modifier = Modifier.width(8.dp))

                Icon(
                    imageVector = if (isExpanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
                    contentDescription = if (isExpanded) "Colapsar" else "Expandir",
                    tint = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            // Questions
            AnimatedVisibility(
                visible = isExpanded,
                enter = expandVertically(),
                exit = shrinkVertically()
            ) {
                Column(
                    modifier = Modifier.padding(bottom = 8.dp)
                ) {
                    HorizontalDivider(modifier = Modifier.padding(horizontal = 16.dp))
                    Spacer(modifier = Modifier.height(8.dp))

                    section.questions.sortedBy { it.orderIndex }.forEach { question ->
                        QuestionItem(
                            question = question,
                            answer = answers[question.id],
                            isNotesExpanded = expandedNotes.contains(question.id),
                            onToggleNotes = { onToggleNotes(question.id) },
                            onAnswerChange = onAnswerChange
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun QuestionItem(
    question: Question,
    answer: Answer?,
    isNotesExpanded: Boolean,
    onToggleNotes: () -> Unit,
    onAnswerChange: (String, String?, Boolean?, Boolean, String?) -> Unit
) {
    var notesText by remember(answer?.notes) { mutableStateOf(answer?.notes ?: "") }

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp, vertical = 8.dp)
    ) {
        // Question text
        Row(
            modifier = Modifier.fillMaxWidth(),
            verticalAlignment = Alignment.Top
        ) {
            if (question.isCritical) {
                Icon(
                    Icons.Default.Star,
                    contentDescription = "Pregunta cr\u00edtica",
                    tint = NoConformeRed,
                    modifier = Modifier
                        .size(18.dp)
                        .padding(top = 2.dp)
                )
                Spacer(modifier = Modifier.width(4.dp))
            }
            Text(
                text = question.text,
                style = MaterialTheme.typography.bodyMedium,
                modifier = Modifier.weight(1f)
            )
        }

        Spacer(modifier = Modifier.height(8.dp))

        // Answer toggle buttons
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            // Conforme button
            AnswerToggleButton(
                text = "Conforme",
                icon = Icons.Default.Check,
                isSelected = answer?.isConforming == true && answer.isNa == false,
                selectedColor = ConformeGreen,
                modifier = Modifier.weight(1f),
                onClick = {
                    onAnswerChange(question.id, "Conforme", true, false, answer?.notes)
                }
            )

            // No Conforme button
            AnswerToggleButton(
                text = "No Conforme",
                icon = Icons.Default.Close,
                isSelected = answer?.isConforming == false && answer.isNa == false,
                selectedColor = NoConformeRed,
                modifier = Modifier.weight(1f),
                onClick = {
                    onAnswerChange(question.id, "No Conforme", false, false, answer?.notes)
                }
            )

            // N/A button
            AnswerToggleButton(
                text = "N/A",
                icon = Icons.Default.HorizontalRule,
                isSelected = answer?.isNa == true,
                selectedColor = NaGray,
                modifier = Modifier.weight(0.7f),
                onClick = {
                    onAnswerChange(question.id, "N/A", null, true, answer?.notes)
                }
            )

            // Notes toggle
            IconButton(
                onClick = onToggleNotes,
                modifier = Modifier.size(36.dp)
            ) {
                Icon(
                    Icons.AutoMirrored.Filled.Notes,
                    contentDescription = "Notas",
                    tint = if (notesText.isNotBlank()) ITOBlue
                    else MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.5f),
                    modifier = Modifier.size(20.dp)
                )
            }
        }

        // Notes field
        AnimatedVisibility(
            visible = isNotesExpanded,
            enter = expandVertically(),
            exit = shrinkVertically()
        ) {
            OutlinedTextField(
                value = notesText,
                onValueChange = { newNotes ->
                    notesText = newNotes
                    onAnswerChange(
                        question.id,
                        answer?.answerValue,
                        answer?.isConforming,
                        answer?.isNa ?: false,
                        newNotes.ifBlank { null }
                    )
                },
                label = { Text("Observaciones") },
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = 8.dp),
                minLines = 2,
                maxLines = 4,
                shape = RoundedCornerShape(8.dp),
                textStyle = MaterialTheme.typography.bodySmall
            )
        }

        Spacer(modifier = Modifier.height(4.dp))
        HorizontalDivider(
            color = MaterialTheme.colorScheme.outlineVariant.copy(alpha = 0.5f)
        )
    }
}

@Composable
private fun AnswerToggleButton(
    text: String,
    icon: androidx.compose.ui.graphics.vector.ImageVector,
    isSelected: Boolean,
    selectedColor: Color,
    modifier: Modifier = Modifier,
    onClick: () -> Unit
) {
    if (isSelected) {
        Button(
            onClick = onClick,
            modifier = modifier.height(40.dp),
            contentPadding = PaddingValues(horizontal = 8.dp),
            shape = RoundedCornerShape(8.dp),
            colors = ButtonDefaults.buttonColors(
                containerColor = selectedColor
            )
        ) {
            Icon(
                icon,
                contentDescription = null,
                modifier = Modifier.size(16.dp),
                tint = Color.White
            )
            Spacer(modifier = Modifier.width(4.dp))
            Text(
                text = text,
                style = MaterialTheme.typography.labelSmall,
                color = Color.White,
                maxLines = 1
            )
        }
    } else {
        OutlinedButton(
            onClick = onClick,
            modifier = modifier.height(40.dp),
            contentPadding = PaddingValues(horizontal = 8.dp),
            shape = RoundedCornerShape(8.dp),
            border = BorderStroke(1.dp, MaterialTheme.colorScheme.outline.copy(alpha = 0.5f))
        ) {
            Icon(
                icon,
                contentDescription = null,
                modifier = Modifier.size(16.dp),
                tint = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Spacer(modifier = Modifier.width(4.dp))
            Text(
                text = text,
                style = MaterialTheme.typography.labelSmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                maxLines = 1
            )
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun SubmitInspectionDialog(
    onDismiss: () -> Unit,
    onConfirm: (weather: String, notes: String?) -> Unit
) {
    val weatherOptions = listOf("Despejado", "Nublado", "Lluvia", "Viento")
    var selectedWeather by remember { mutableStateOf(weatherOptions[0]) }
    var generalNotes by remember { mutableStateOf("") }
    var weatherExpanded by remember { mutableStateOf(false) }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = {
            Text(
                "Finalizar Inspecci\u00f3n",
                style = MaterialTheme.typography.titleLarge
            )
        },
        text = {
            Column(
                modifier = Modifier.fillMaxWidth(),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                Text(
                    text = "Complete los siguientes datos para enviar la inspecci\u00f3n:",
                    style = MaterialTheme.typography.bodyMedium
                )

                // Weather dropdown
                ExposedDropdownMenuBox(
                    expanded = weatherExpanded,
                    onExpandedChange = { weatherExpanded = it }
                ) {
                    OutlinedTextField(
                        value = selectedWeather,
                        onValueChange = {},
                        readOnly = true,
                        label = { Text("Condiciones Clim\u00e1ticas") },
                        trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(expanded = weatherExpanded) },
                        modifier = Modifier
                            .fillMaxWidth()
                            .menuAnchor(),
                        shape = RoundedCornerShape(8.dp)
                    )
                    ExposedDropdownMenu(
                        expanded = weatherExpanded,
                        onDismissRequest = { weatherExpanded = false }
                    ) {
                        weatherOptions.forEach { option ->
                            DropdownMenuItem(
                                text = { Text(option) },
                                onClick = {
                                    selectedWeather = option
                                    weatherExpanded = false
                                }
                            )
                        }
                    }
                }

                // General notes
                OutlinedTextField(
                    value = generalNotes,
                    onValueChange = { generalNotes = it },
                    label = { Text("Notas Generales (opcional)") },
                    modifier = Modifier.fillMaxWidth(),
                    minLines = 3,
                    maxLines = 5,
                    shape = RoundedCornerShape(8.dp)
                )
            }
        },
        confirmButton = {
            Button(
                onClick = { onConfirm(selectedWeather, generalNotes.ifBlank { null }) },
                colors = ButtonDefaults.buttonColors(containerColor = ITOBlue)
            ) {
                Icon(Icons.Default.Send, contentDescription = null, modifier = Modifier.size(18.dp))
                Spacer(modifier = Modifier.width(8.dp))
                Text("Confirmar Env\u00edo")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancelar")
            }
        }
    )
}
