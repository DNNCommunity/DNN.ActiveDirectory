'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2013
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Imports DotNetNuke.Authentication.ActiveDirectory.ADSI
Imports DotNetNuke.Entities.Portals
Imports Microsoft.Extensions.DependencyInjection

Namespace DotNetNuke.Authentication.ActiveDirectory.HttpModules
    Public Class AuthenticationModule
        '        Implements IHttpModule' Steven West - inherit for portalmodulebase for access to dependencyprovider
        Inherits DotNetNuke.Entities.Modules.PortalModuleBase

        Friend authenticationService As AuthenticationController = DependencyProvider.GetRequiredService(Of AuthenticationController)
        Friend config As Configuration = New Configuration(authenticationService.serviceProvider).GetConfig

        Public ReadOnly Property ModuleName() As String
            Get
                Return "AuthenticationModule"
            End Get
        End Property

        Public Shadows Sub Init(ByVal application As HttpApplication) 'Implements IHttpModule.Init
            AddHandler application.AuthenticateRequest, AddressOf OnAuthenticateRequest
        End Sub

        Public Sub OnAuthenticateRequest(ByVal s As Object, ByVal e As EventArgs)
            Dim request As HttpRequest = HttpContext.Current.Request
            Dim response As HttpResponse = HttpContext.Current.Response

            ''check if we are upgrading/installing/using a web service/rss feeds (ACD-7748)
            'Abort if NOT Default.aspx
            If Not request.Url.LocalPath.ToLower.EndsWith("default.aspx") _
               OrElse request.RawUrl.ToLower.Contains("rssid") Then
                Exit Sub
            End If
            'Check that Host/Admin user is not already logged into the site. 
            'If so then bypass authentication (ACD-2592)
            If Not (Users.UserController.Instance.GetCurrentUserInfo().Username = String.Empty) Then
                Dim bHost As Boolean = Users.UserController.Instance.GetCurrentUserInfo().IsSuperUser
                Dim _
                    bAdmin As Boolean = Users.UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators")
                If bAdmin Or bHost Then Exit Sub
            End If

            'Moved the following statement from the top to correct ACD-9746
            Dim portalSettings As PortalSettings = GetPortalSettings()

            'Dim config As Configuration = Configuration.GetConfig()

            If config Is Nothing Then
                Exit Sub
            End If

            'ACD-8846, WorkItems 6416,4766, 4077, 7805
            Dim strRequest As String = UCase(request.ServerVariables("HTTP_USER_AGENT"))
            If strRequest Is Nothing Then
                Exit Sub
            End If
            Dim arrBots() As String = config.Bots.Split(";")
            For intCount As Integer = 0 To arrBots.Length - 1
                Dim strBot As String = UCase(arrBots(intCount))
                If Not strBot = "" Then
                    If strRequest.Contains(strBot) Then
                        Exit Sub
                    End If
                End If
            Next
            If strRequest.Contains("gsa-crawler") Or strRequest Is Nothing Then
                Exit Sub
            End If

            Dim permission As AspNetHostingPermissionLevel = Utilities.GetCurrentTrustLevel()

            If (Not (permission = AspNetHostingPermissionLevel.Unrestricted)) Then
                response.Redirect("~/DesktopModules/AuthenticationServices/ActiveDirectory/trusterror.htm", True)
            End If

            'ACD-8589
            If (config.WindowsAuthentication Or config.HideWindowsLogin) Then
                Dim authStatus As AuthenticationStatus = AuthenticationController.GetStatus(portalSettings.PortalId)
                Dim _
                    blnWinLogon As Boolean =
                        (request.RawUrl.ToLower.IndexOf((Configuration.AUTHENTICATION_LOGON_PAGE).ToLower) > -1)
                Dim blnWinLogoff As Boolean = (authStatus = AuthenticationStatus.WinLogon) _
                                              AndAlso
                                              (request.RawUrl.ToLower.IndexOf(
                                                                               (Configuration.AUTHENTICATION_LOGOFF_PAGE) _
                                                                                  .ToLower) > -1)
                Dim blnWinProcess As Boolean = (authStatus = AuthenticationStatus.WinProcess) AndAlso (Not (blnWinLogon OrElse blnWinLogoff))

                SetDnnReturnToCookie(request, response, portalSettings)
                If (authStatus = AuthenticationStatus.Undefined) OrElse (blnWinProcess) Then
                    authenticationService.SetStatus(portalSettings.PortalId, AuthenticationStatus.WinProcess)
                    Dim url As String = request.RawUrl
                    Dim arrAutoIp() = config.AutoIP.Split(";")

                    'ACD-7664
                    Dim strClientIp As String = Utilities.GetIP4Address(request.UserHostAddress)

                    'Issue: 47 
                    'Check new settings feature enable auto login
                    'Steven A West 8/14/2018 check for no ip addresses, no ip addresses = all clients get windows authentication
                    Dim redirect As Boolean = False
                    If CBool(config.EnableAutoLogin) Then
                        If arrAutoIp.Length > 0 Then
                            For intCount As Integer = 0 To arrAutoIp.Length - 1
                                Dim strAutoIp As String = arrAutoIp(intCount)
                                If (InStr(strAutoIp, "-")) Then
                                    Dim arrIpRange() = strAutoIp.Split("-")
                                    Dim lClientIp As Long = IpAddressToLong(strClientIp)
                                    If _
                                lClientIp >= IpAddressToLong(Utilities.GetIP4Address(Trim(arrIpRange(0)))) And
                                lClientIp <= IpAddressToLong(Utilities.GetIP4Address(Trim(arrIpRange(1)))) Then
                                        url = GetRedirectUrl(request)
                                        redirect = True
                                        Exit For
                                    End If
                                ElseIf _
                            (Not InStr(Left(strClientIp.ToString, strAutoIp.Length), strAutoIp) = 0) Or
                            (strAutoIp = "") Then
                                    url = GetRedirectUrl(request)
                                    redirect = True
                                    Exit For
                                End If
                            Next
                        Else
                            url = GetRedirectUrl(request)
                            redirect = True
                        End If
                    End If
                    If redirect Then 'prevents infinite redirects issue: 47
                        response.Redirect(url & "?portalid=" & portalSettings.PortalId)
                    End If
                ElseIf (Not authStatus = AuthenticationStatus.WinLogoff) AndAlso blnWinLogoff Then
                    'Dim objAuthentication As New AuthenticationController use new service provider
                    AuthenticationService.AuthenticationLogoff()
                ElseIf (authStatus = AuthenticationStatus.WinLogoff) AndAlso blnWinLogon Then ' has been logoff before
                    authenticationService.SetStatus(portalSettings.PortalId, AuthenticationStatus.Undefined)
                    response.Redirect(request.RawUrl)
                End If

            End If
        End Sub

        Public Shadows Sub Dispose() ' Implements IHttpModule.Dispose
            ' Should check to see why this routine is never called
        End Sub

        Private Shared Function GetRedirectUrl(ByVal request As HttpRequest) _
            As String

            If request.ApplicationPath = "/" Then
                Return _
                    Configuration.AUTHENTICATION_PATH & Configuration.AUTHENTICATION_LOGON_PAGE
            Else
                Return _
                    request.ApplicationPath & Configuration.AUTHENTICATION_PATH &
                        Configuration.AUTHENTICATION_LOGON_PAGE
            End If
        End Function

        Private Shared Sub SetDnnReturnToCookie(ByVal request As HttpRequest, ByVal response As HttpResponse,
                                          ByVal portalSettings As PortalSettings)
            Try
                Dim refUrl As String = request.RawUrl
                response.Clear()
                response.Cookies("DNNReturnTo").Value = refUrl
                response.Cookies("DNNReturnTo").Path = "/"
                response.Cookies("DNNReturnTo").Expires =
                    DateTime.Now.AddMinutes(5)
            Catch
            End Try

        End Sub

        Private Shared Function IpAddressToLong(ByVal strPassedIp As String) As UInteger
            Dim x As Integer
            Dim pos As Integer
            Dim prevPos As Integer
            Dim num As Integer
            Dim lConvertToLong As Long = 0

            If UBound(Split(strPassedIp, ".")) = 3 Then

                ' On Error Resume Next

                For x = 1 To 4

                    pos = InStr(prevPos + 1, strPassedIp, ".", 1)

                    If x = 4 Then pos = Len(strPassedIp) + 1

                    num = Int(Mid(strPassedIp, prevPos + 1, pos - prevPos - 1))

                    If num > 255 Then

                        'lConvertToLong = "0"

                        Exit Function

                    End If

                    prevPos = pos

                    lConvertToLong = ((num Mod 256) * (256 ^ (4 - x))) + lConvertToLong

                Next

            End If
            Return lConvertToLong

        End Function
    End Class
End Namespace
