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
Imports DotNetNuke.Abstractions.Logging
Imports DotNetNuke.Authentication.ActiveDirectory.ADSI
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Framework.Providers

Namespace DotNetNuke.Authentication.ActiveDirectory.HttpModules
    Public Class AuthenticationModule
        Implements IHttpModule

        ''' <summary>
        ''' Need to setup dependency injection. 
        ''' </summary>


        Public ReadOnly Property ModuleName() As String
            Get
                Return "AuthenticationModule"
            End Get
        End Property

        Public Sub Init(ByVal application As HttpApplication) Implements IHttpModule.Init
            AddHandler application.AuthenticateRequest, AddressOf OnAuthenticateRequest
        End Sub

        Public Sub OnAuthenticateRequest(ByVal s As Object, ByVal e As EventArgs)
            Dim request As HttpRequest = HttpContext.Current.Request
            Dim response As HttpResponse = HttpContext.Current.Response
            Dim config As ConfigInfo = Nothing
            Dim portalSettings As PortalSettings = GetPortalSettings()
            Dim providerConfiguration As ProviderConfiguration = ProviderConfiguration.GetProviderConfiguration(Configuration.AUTHENTICATION_KEY)
            ''check if we are upgrading/installing/using a web service/rss feeds (ACD-7748)
            'Abort if NOT Default.aspx
            If Not request.Url.LocalPath.ToLower.EndsWith("default.aspx") _
               OrElse request.RawUrl.ToLower.Contains("rssid") Then
                Exit Sub
            End If
            'Check that Host/Admin user is not already logged into the site. 
            If Not (Users.UserController.Instance.GetCurrentUserInfo().Username = String.Empty) Then
                Dim bHost As Boolean = Users.UserController.Instance.GetCurrentUserInfo().IsSuperUser
                Dim _
                    bAdmin As Boolean = Users.UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators")
                If bAdmin Or bHost Then Exit Sub
            End If

            'portalSettings = GetPortalSettings()
            If providerConfiguration.DefaultProvider IsNot Nothing Then
                config = New ConfigInfo(PortalController.Instance.GetPortalSettings(portalSettings.PortalId), portalSettings.PortalId)
            End If
            If config Is Nothing Then
                Exit Sub
            End If
            Dim strRequest As String = UCase(request.ServerVariables("HTTP_USER_AGENT"))
            If strRequest Is Nothing Then
                Exit Sub
            End If
            Dim arrBots() As String
            If config.Bots IsNot Nothing Then
                arrBots = config.Bots.Split(";")
                For intCount As Integer = 0 To arrBots.Length - 1
                    Dim strBot As String = UCase(arrBots(intCount))
                    If Not strBot = "" Then
                        If strRequest.Contains(strBot) Then
                            Exit Sub
                        End If
                    End If
                Next
            End If

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
                Dim blnWinLogon As Boolean = (request.RawUrl.ToLower.IndexOf((ActiveDirectory.Configuration.AUTHENTICATION_LOGON_PAGE).ToLower) > -1)
                Dim blnWinLogoff As Boolean = (authStatus = AuthenticationStatus.WinLogon) AndAlso (request.RawUrl.ToLower.IndexOf((ActiveDirectory.Configuration.AUTHENTICATION_LOGOFF_PAGE).ToLower) > -1)
                Dim blnWinProcess As Boolean = (authStatus = AuthenticationStatus.WinProcess) AndAlso (Not (blnWinLogon OrElse blnWinLogoff))
                SetDnnReturnToCookie(request, response, portalSettings)
                If (authStatus = AuthenticationStatus.Undefined) OrElse (blnWinProcess) Then
                    AuthenticationController.SetStatus(portalSettings.PortalId, AuthenticationStatus.WinProcess)
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
                                    If lClientIp >= IpAddressToLong(Utilities.GetIP4Address(Trim(arrIpRange(0)))) And lClientIp <= IpAddressToLong(Utilities.GetIP4Address(Trim(arrIpRange(1)))) Then
                                        url = GetRedirectUrl(request)
                                        redirect = True
                                        Exit For
                                    End If
                                ElseIf (Not InStr(Left(strClientIp.ToString, strAutoIp.Length), strAutoIp) = 0) Or (strAutoIp = "") Then
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
                    AuthenticationLogoff(portalSettings)
                ElseIf (authStatus = AuthenticationStatus.WinLogoff) AndAlso blnWinLogon Then ' has been logoff before
                    AuthenticationController.SetStatus(portalSettings.PortalId, AuthenticationStatus.Undefined)
                    response.Redirect(request.RawUrl)
                End If

            End If
        End Sub
        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' Using old globals.navigateURL because IHTTPModule (authenicationmodule) needs DI setup.
        ''' Once DI is setup for that module, we can switch this to the INavigateManager.
        ''' Function was moved from AuthenticationController to here. It was the only reference and was keeping DI from being
        ''' complete in that class.
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Sub AuthenticationLogoff(portalsettings As PortalSettings)

            ' Log User Off from Cookie Authentication System
            FormsAuthentication.SignOut()
            If AuthenticationController.GetStatus(portalsettings.PortalId) = AuthenticationStatus.WinLogon Then
                AuthenticationController.SetStatus(portalsettings.PortalId, AuthenticationStatus.WinLogoff)
            End If

            ' expire cookies
            HttpContext.Current.Response.Cookies("portalaliasid").Value = Nothing
            HttpContext.Current.Response.Cookies("portalaliasid").Path = "/"
            HttpContext.Current.Response.Cookies("portalaliasid").Expires = DateTime.Now.AddYears(-30)

            HttpContext.Current.Response.Cookies("portalroles").Value = Nothing
            HttpContext.Current.Response.Cookies("portalroles").Path = "/"
            HttpContext.Current.Response.Cookies("portalroles").Expires = DateTime.Now.AddYears(-30)

            ' Redirect browser back to portal 
            If portalsettings.HomeTabId <> -1 Then
                HttpContext.Current.Response.Redirect(NavigateURL(portalsettings.HomeTabId), True)
            Else

                HttpContext.Current.Response.Redirect(NavigateURL(), True)
            End If
        End Sub
        Public Sub Dispose() Implements IHttpModule.Dispose
            ' Should check to see why this routine is never called
        End Sub

        Private Shared Function GetRedirectUrl(ByVal request As HttpRequest) _
            As String

            If request.ApplicationPath = "/" Then
                Return _
                    ActiveDirectory.Configuration.AUTHENTICATION_PATH & ActiveDirectory.Configuration.AUTHENTICATION_LOGON_PAGE
            Else
                Return _
                    request.ApplicationPath & ActiveDirectory.Configuration.AUTHENTICATION_PATH &
                        ActiveDirectory.Configuration.AUTHENTICATION_LOGON_PAGE
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
