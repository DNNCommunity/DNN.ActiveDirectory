Imports DotNetNuke.DependencyInjection
Imports Microsoft.Extensions.DependencyInjection

Public Class Startup
    Implements IDnnStartup

    Public Sub ConfigureServices(services As IServiceCollection) Implements IDnnStartup.ConfigureServices
        services.AddScoped(Of serviceProvider)()
        services.AddScoped(Of Authentication.ActiveDirectory.Configuration)()
        services.AddScoped(Of Authentication.ActiveDirectory.AuthenticationController)()
        'services.AddScoped(Of Authentication.ActiveDirectory.AuthenticationProvider)()
        services.AddScoped(Of Authentication.ActiveDirectory.UserController)()
        services.AddScoped(Of Authentication.ActiveDirectory.GroupController)()
        services.AddScoped(Of Authentication.ActiveDirectory.ADSI.Configuration)()
        services.AddScoped(Of Authentication.ActiveDirectory.ADSI.Domain)()
        services.AddScoped(Of Authentication.ActiveDirectory.ADSI.ADSIProvider)()
        services.AddScoped(Of Authentication.ActiveDirectory.ADSI.Utilities)()

    End Sub

End Class
