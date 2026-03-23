package cl.itocloud.inspector.ui.navigation

sealed class NavRoutes(val route: String) {
    data object Login : NavRoutes("login")
    data object Dashboard : NavRoutes("dashboard")
    data object InspectionList : NavRoutes("inspections")
    data object InspectionDetail : NavRoutes("inspections/{inspectionId}") {
        fun createRoute(inspectionId: String) = "inspections/$inspectionId"
    }
    data object InspectionExecute : NavRoutes("inspections/{inspectionId}/execute") {
        fun createRoute(inspectionId: String) = "inspections/$inspectionId/execute"
    }
    data object ObservationList : NavRoutes("observations")
    data object ObservationNew : NavRoutes("observations/new")
    data object Sync : NavRoutes("sync")
    data object Profile : NavRoutes("profile")
}
