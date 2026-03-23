package cl.itocloud.inspector.ui.theme

import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.dynamicDarkColorScheme
import androidx.compose.material3.dynamicLightColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext

private val LightColorScheme = lightColorScheme(
    primary = ITOBlue,
    onPrimary = Color.White,
    primaryContainer = ITOBlueSurface,
    onPrimaryContainer = Color(0xFF001D36),
    secondary = ITOOrange,
    onSecondary = Color.White,
    secondaryContainer = Color(0xFFFFE0B2),
    onSecondaryContainer = Color(0xFF3E2700),
    tertiary = Color(0xFF6B5778),
    onTertiary = Color.White,
    error = ITORed,
    onError = Color.White,
    errorContainer = Color(0xFFFFDAD6),
    onErrorContainer = Color(0xFF410002),
    background = SurfaceLight,
    onBackground = Color(0xFF1A1C1E),
    surface = SurfaceLight,
    onSurface = Color(0xFF1A1C1E),
    surfaceVariant = Color(0xFFE0E2EC),
    onSurfaceVariant = Color(0xFF44474F),
    outline = Color(0xFF74777F),
    outlineVariant = Color(0xFFC4C6D0)
)

private val DarkColorScheme = darkColorScheme(
    primary = ITOBlueLight,
    onPrimary = Color(0xFF003258),
    primaryContainer = ITOBlueDark,
    onPrimaryContainer = ITOBlueSurface,
    secondary = ITOOrangeLight,
    onSecondary = Color(0xFF462A00),
    secondaryContainer = ITOOrangeDark,
    onSecondaryContainer = Color(0xFFFFE0B2),
    tertiary = Color(0xFFD7BDE4),
    onTertiary = Color(0xFF3B2948),
    error = Color(0xFFFFB4AB),
    onError = Color(0xFF690005),
    errorContainer = ITORedDark,
    onErrorContainer = Color(0xFFFFDAD6),
    background = SurfaceDark,
    onBackground = Color(0xFFE2E2E6),
    surface = SurfaceDark,
    onSurface = Color(0xFFE2E2E6),
    surfaceVariant = Color(0xFF44474F),
    onSurfaceVariant = Color(0xFFC4C6D0),
    outline = Color(0xFF8E9099),
    outlineVariant = Color(0xFF44474F)
)

@Composable
fun ITOCloudInspectorTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    dynamicColor: Boolean = false,
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        dynamicColor && Build.VERSION.SDK_INT >= Build.VERSION_CODES.S -> {
            val context = LocalContext.current
            if (darkTheme) dynamicDarkColorScheme(context)
            else dynamicLightColorScheme(context)
        }
        darkTheme -> DarkColorScheme
        else -> LightColorScheme
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = ITOTypography,
        content = content
    )
}
