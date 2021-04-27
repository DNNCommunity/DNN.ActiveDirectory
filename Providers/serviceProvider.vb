Imports DotNetNuke.Authentication.ActiveDirectory
''' <summary>
''' General service provider for dependency injection.
''' Author: Steven A West
''' </summary>
Public Class serviceProvider
    Public ReadOnly portalAliasService As Abstractions.Portals.IPortalAliasService
    Public ReadOnly portalService As Portals.IPortalController
    Public ReadOnly navigationManager As Abstractions.INavigationManager
    Public ReadOnly eventLogService As Abstractions.Logging.IEventLogger

    'add service params for each DNN service above
    Public Sub New(ByVal portalAliasService As Abstractions.Portals.IPortalAliasService,
                   ByVal navigationManager As Abstractions.INavigationManager,
                   ByVal eventLogService As Abstractions.Logging.IEventLogger)
        Me.portalAliasService = portalAliasService
        Me.navigationManager = navigationManager
        Me.eventLogService = eventLogService
        Me.portalService = Portals.PortalController.Instance
    End Sub
End Class
