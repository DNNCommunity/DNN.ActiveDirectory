Imports DotNetNuke.DependencyInjection
Imports Microsoft.Extensions.DependencyInjection

Public Class Startup
    Implements IDnnStartup

    Public Sub ConfigureServices(services As IServiceCollection) Implements IDnnStartup.ConfigureServices
        services.AddScoped(Of Configuration)()
        services.AddScoped(Of DotNetNuke.Authentication.ActiveDirectory.ADSI.Configuration)()
        services.AddScoped(Of DotNetNuke.Authentication.ActiveDirectory.ADSI.Utilities)()
        services.AddScoped(Of Authentication.AuthenticationController)()
        services.AddScoped(Of DotNetNuke.Authentication.ActiveDirectory.ADSI.ADSIProvider)()
    End Sub

End Class
