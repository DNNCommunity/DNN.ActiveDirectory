Imports DotNetNuke.Authentication.ActiveDirectory
Imports DotNetNuke.DependencyInjection
Imports Microsoft.Extensions.DependencyInjection

Public Class Startup
    Implements IDnnStartup

    Public Sub ConfigureServices(services As IServiceCollection) Implements IDnnStartup.ConfigureServices
        services.AddScoped(Of IConfiguration, Configuration)()
        services.AddScoped(Of ADSI.IConfiguration, ADSI.Configuration)()
        services.AddScoped(Of ADSI.IUtilities, ADSI.Utilities)()
        services.AddScoped(Of Authentication.ActiveDirectory.IAuthenticationController, Authentication.ActiveDirectory.AuthenticationController)()
        services.AddScoped(Of IUserController, UserController)()
        services.AddScoped(Of IGroupController, GroupController)()
        services.AddScoped(Of IAuthenticationProvider, ADSI.ADSIProvider)()
    End Sub

End Class
