Imports DotNetNuke.Authentication.ActiveDirectory
Imports DotNetNuke.DependencyInjection
Imports Microsoft.Extensions.DependencyInjection

Public Class Startup
    Implements IDnnStartup

    Public Sub ConfigureServices(services As IServiceCollection) Implements IDnnStartup.ConfigureServices
        services.AddScoped(Of Configuration)()
        services.AddScoped(Of ADSI.Configuration)()
        services.AddScoped(Of ADSI.Utilities)()
        services.AddScoped(Of Authentication.AuthenticationController)()
        services.AddScoped(Of UserController)()
        services.AddScoped(Of GroupController)()
        services.AddScoped(Of ADSI.ADSIProvider)()
    End Sub

End Class
