package cl.itocloud.inspector.ui.navigation

import androidx.compose.animation.AnimatedContentTransitionScope
import androidx.compose.animation.core.tween
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.navArgument
import cl.itocloud.inspector.ui.auth.LoginScreen
import cl.itocloud.inspector.ui.auth.LoginViewModel
import cl.itocloud.inspector.ui.dashboard.DashboardScreen
import cl.itocloud.inspector.ui.dashboard.DashboardViewModel
import cl.itocloud.inspector.ui.inspections.InspectionDetailScreen
import cl.itocloud.inspector.ui.inspections.InspectionDetailViewModel
import cl.itocloud.inspector.ui.inspections.InspectionExecuteScreen
import cl.itocloud.inspector.ui.inspections.InspectionExecuteViewModel
import cl.itocloud.inspector.ui.inspections.InspectionListScreen
import cl.itocloud.inspector.ui.inspections.InspectionListViewModel
import cl.itocloud.inspector.ui.observations.ObservationListScreen
import cl.itocloud.inspector.ui.observations.ObservationListViewModel
import cl.itocloud.inspector.ui.observations.ObservationNewScreen
import cl.itocloud.inspector.ui.observations.ObservationNewViewModel
import cl.itocloud.inspector.ui.profile.ProfileScreen
import cl.itocloud.inspector.ui.profile.ProfileViewModel
import cl.itocloud.inspector.ui.sync.SyncScreen
import cl.itocloud.inspector.ui.sync.SyncViewModel

@Composable
fun AppNavHost(
    navController: NavHostController,
    isLoggedIn: Boolean,
    modifier: Modifier = Modifier
) {
    val startDestination = if (isLoggedIn) NavRoutes.Dashboard.route else NavRoutes.Login.route

    NavHost(
        navController = navController,
        startDestination = startDestination,
        modifier = modifier,
        enterTransition = {
            fadeIn(animationSpec = tween(300)) + slideIntoContainer(
                towards = AnimatedContentTransitionScope.SlideDirection.Start,
                animationSpec = tween(300)
            )
        },
        exitTransition = {
            fadeOut(animationSpec = tween(300)) + slideOutOfContainer(
                towards = AnimatedContentTransitionScope.SlideDirection.Start,
                animationSpec = tween(300)
            )
        },
        popEnterTransition = {
            fadeIn(animationSpec = tween(300)) + slideIntoContainer(
                towards = AnimatedContentTransitionScope.SlideDirection.End,
                animationSpec = tween(300)
            )
        },
        popExitTransition = {
            fadeOut(animationSpec = tween(300)) + slideOutOfContainer(
                towards = AnimatedContentTransitionScope.SlideDirection.End,
                animationSpec = tween(300)
            )
        }
    ) {
        // Login
        composable(NavRoutes.Login.route) {
            val viewModel: LoginViewModel = hiltViewModel()
            LoginScreen(
                viewModel = viewModel,
                onLoginSuccess = {
                    navController.navigate(NavRoutes.Dashboard.route) {
                        popUpTo(NavRoutes.Login.route) { inclusive = true }
                    }
                }
            )
        }

        // Dashboard
        composable(NavRoutes.Dashboard.route) {
            val viewModel: DashboardViewModel = hiltViewModel()
            DashboardScreen(
                viewModel = viewModel,
                onNavigateToInspections = {
                    navController.navigate(NavRoutes.InspectionList.route)
                },
                onNavigateToNewObservation = {
                    navController.navigate(NavRoutes.ObservationNew.route)
                },
                onNavigateToObservations = {
                    navController.navigate(NavRoutes.ObservationList.route)
                },
                onNavigateToSync = {
                    navController.navigate(NavRoutes.Sync.route)
                }
            )
        }

        // Inspection List
        composable(NavRoutes.InspectionList.route) {
            val viewModel: InspectionListViewModel = hiltViewModel()
            InspectionListScreen(
                viewModel = viewModel,
                onInspectionClick = { inspectionId ->
                    navController.navigate(NavRoutes.InspectionDetail.createRoute(inspectionId))
                },
                onBackClick = { navController.popBackStack() }
            )
        }

        // Inspection Detail
        composable(
            route = NavRoutes.InspectionDetail.route,
            arguments = listOf(navArgument("inspectionId") { type = NavType.StringType })
        ) {
            val viewModel: InspectionDetailViewModel = hiltViewModel()
            InspectionDetailScreen(
                viewModel = viewModel,
                onExecuteClick = { inspectionId ->
                    navController.navigate(NavRoutes.InspectionExecute.createRoute(inspectionId))
                },
                onBackClick = { navController.popBackStack() }
            )
        }

        // Inspection Execute
        composable(
            route = NavRoutes.InspectionExecute.route,
            arguments = listOf(navArgument("inspectionId") { type = NavType.StringType })
        ) {
            val viewModel: InspectionExecuteViewModel = hiltViewModel()
            InspectionExecuteScreen(
                viewModel = viewModel,
                onSubmitSuccess = {
                    navController.popBackStack(NavRoutes.InspectionList.route, false)
                },
                onBackClick = { navController.popBackStack() }
            )
        }

        // Observation List
        composable(NavRoutes.ObservationList.route) {
            val viewModel: ObservationListViewModel = hiltViewModel()
            ObservationListScreen(
                viewModel = viewModel,
                onNewObservationClick = {
                    navController.navigate(NavRoutes.ObservationNew.route)
                },
                onBackClick = { navController.popBackStack() }
            )
        }

        // Observation New
        composable(NavRoutes.ObservationNew.route) {
            val viewModel: ObservationNewViewModel = hiltViewModel()
            ObservationNewScreen(
                viewModel = viewModel,
                onObservationCreated = { navController.popBackStack() },
                onBackClick = { navController.popBackStack() }
            )
        }

        // Sync
        composable(NavRoutes.Sync.route) {
            val viewModel: SyncViewModel = hiltViewModel()
            SyncScreen(
                viewModel = viewModel,
                onBackClick = { navController.popBackStack() }
            )
        }

        // Profile
        composable(NavRoutes.Profile.route) {
            val viewModel: ProfileViewModel = hiltViewModel()
            ProfileScreen(
                viewModel = viewModel,
                onLogout = {
                    navController.navigate(NavRoutes.Login.route) {
                        popUpTo(0) { inclusive = true }
                    }
                },
                onBackClick = { navController.popBackStack() }
            )
        }
    }
}
