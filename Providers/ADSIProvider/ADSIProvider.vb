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
Imports System.DirectoryServices
Imports DotNetNuke.Entities.Portals
Imports System.Runtime.InteropServices
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Security.Roles

Namespace DotNetNuke.Authentication.ActiveDirectory.ADSI
    Public Class ADSIProvider
        Implements IAuthenticationProvider

        Private _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings
        Private _adsiConfig As Configuration = Configuration.GetConfig()
        Private _config As ActiveDirectory.Configuration = ActiveDirectory.Configuration.GetConfig()

#Region "Private Methods"

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]   10/05/2009 Changed  - WorkItem:2943
        ''' </history>
        ''' -------------------------------------------------------------------

        Private Function GetSimplyUser(ByVal UserName As String) As ADUserInfo
            Dim objAuthUser As New ADUserInfo

            With objAuthUser
                .PortalID = _portalSettings.PortalId
                .IsNotSimplyUser = False
                .Username = UserName
                .FirstName = Utilities.TrimUserDomainName(UserName)
                .LastName = Utilities.GetUserDomainName(UserName)
                .IsSuperUser = False
                .DistinguishedName = Utilities.ConvertToDistinguished(UserName)

                Dim strEmail As String = _adsiConfig.DefaultEmailDomain
                If Not strEmail.Length = 0 Then
                    If strEmail.IndexOf("@") = -1 Then
                        strEmail = "@" & strEmail
                    End If
                    strEmail = .FirstName & strEmail
                Else
                    strEmail = .FirstName & "@" & .LastName & ".com"
                    ' confusing?
                End If
                ' Membership properties
                .Username = UserName
                .Email = strEmail
                .Membership.Approved = True
                .Membership.LastLoginDate = Date.Now
                .Membership.Password = Utilities.GetRandomPassword()
                .AuthenticationExists = False
            End With

            Return objAuthUser

        End Function

        Private Function IsAuthenticated(ByVal Path As String, ByVal UserName As String, ByVal Password As String) _
            As Boolean
            Try
                'Moved to private global for access from other functions - sawest
                'Dim _config As ActiveDirectory.Configuration = ActiveDirectory.Configuration.GetConfig()
                If _config.StripDomainName Then
                    Dim crossRef As CrossReferenceCollection.CrossReference
                    For Each crossRef In Configuration.GetConfig.RefCollection
                        UserName = crossRef.NetBIOSName & "\" & UserName
                    Next
                End If
                Dim userEntry As New DirectoryEntry(Path, UserName, Password, AuthenticationTypes.Signing)
                ' Bind to the native AdsObject to force authentication.
                Dim obj As Object = userEntry.NativeObject

            Catch exc As COMException
                Return False
            End Try

            Return True

        End Function

        ''' <summary>
        ''' Fill UserInfo object
        ''' </summary>
        ''' <param name="UserEntry"></param>
        ''' <param name="UserInfo"></param>
        ''' <history>
        '''     [sawest]    12/16/2016  Added set photo
        ''' </history>
        Private Sub FillUserInfo(ByVal UserEntry As DirectoryEntry, ByRef UserInfo As ADUserInfo)

            With UserInfo
                .IsSuperUser = False
                .Username = UserInfo.Username
                .Membership.Approved = True
                .Membership.LastLoginDate = Date.Now
                If Not UserEntry Is Nothing Then
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_EMAIL).Value) = "") Then
                        .Email = Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_EMAIL).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_CNAME).Value.ToString) = "") _
                        Then
                        .CName = Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_CNAME).Value.ToString)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_DISPLAYNAME).Value) = "") _
                        Then
                        .DisplayName =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_DISPLAYNAME).Value)
                    End If
                    If _
                        Not _
                        (Utilities.CheckNullString(
                                                    UserEntry.Properties(Configuration.ADSI_DISTINGUISHEDNAME).Value.
                                                       ToString) = "") Then
                        .DistinguishedName =
                            Utilities.CheckNullString(
                                                       UserEntry.Properties(Configuration.ADSI_DISTINGUISHEDNAME).Value.
                                                          ToString)
                    End If
                    If _
                        Not _
                        (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_ACCOUNTNAME).Value.ToString) =
                         "") Then
                        .sAMAccountName =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_ACCOUNTNAME).Value.ToString)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_CNAME).Value) = "") Then
                        .Profile.FirstName =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_FIRSTNAME).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_LASTNAME).Value) = "") Then
                        .Profile.LastName =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_LASTNAME).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_STREET).Value) = "") Then
                        .Profile.Street = Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_STREET).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_CITY).Value) = "") Then
                        .Profile.City = Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_CITY).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_REGION).Value) = "") Then
                        .Profile.Region = Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_REGION).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_POSTALCODE).Value) = "") _
                        Then
                        .Profile.PostalCode =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_POSTALCODE).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_COUNTRY).Value) = "") Then
                        .Profile.Country =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_COUNTRY).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_TELEPHONE).Value) = "") Then
                        .Profile.Telephone =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_TELEPHONE).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_FAX).Value) = "") Then
                        .Profile.Fax = Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_FAX).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_CELL).Value) = "") Then
                        .Profile.Cell = Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_CELL).Value)
                    End If
                    If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_WEBSITE).Value) = "") Then
                        .Profile.Website =
                            Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_WEBSITE).Value)
                    End If
                    If _config.Photo Then
                        'sync photo from AD if checked in settings
                        If Not (Utilities.CheckNullString(UserEntry.Properties(Configuration.ADSI_PHOTO).Value) = "") Then
                            .Profile.Photo =
                            Utilities.WritePhoto(UserInfo, UserEntry.Properties(Configuration.ADSI_PHOTO).Value)
                        End If
                    End If
                End If

                    If .Email = "" Then
                    .Email = Utilities.TrimUserDomainName(UserInfo.Username) & _adsiConfig.DefaultEmailDomain
                End If
                If .DisplayName = "" Then
                    .DisplayName = .CName
                End If

                .AuthenticationExists = True
                ' obtain firstname from username if admin has not enter enough user info
                If .Profile.FirstName.Length = 0 Then
                    .Profile.FirstName = Utilities.TrimUserDomainName(UserInfo.Username)
                End If
            End With
        End Sub

#End Region


        Public Overloads Overrides Function GetUser(ByVal LoggedOnUserName As String, ByVal LoggedOnPassword As String) _
            As ADUserInfo
            Dim objAuthUser As ADUserInfo

            If Not _adsiConfig.ADSINetwork Then
                Return Nothing
            End If

            Try
                'debug logging issue #54 steven west 2/6/2019
                If _config.EnableDebugMode Then
                    Utilities.objEventLog.AddLog("Description", "@GETUSER:Getting ready to getUserEntryByName.  LoggedOnUserName: " & LoggedOnUserName, _portalSettings, -1, Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT)
                End If
                Dim entry As DirectoryEntry = Utilities.GetUserEntryByName(LoggedOnUserName)

                'debug logging issue #54 steven west 2/6/2019
                If _config.EnableDebugMode Then
                    If Not entry Is Nothing Then
                        Dim key As String
                        Dim entryStr As New StringBuilder
                        For Each key In entry.Properties.PropertyNames
                            Dim sPropertyValues As String = ""
                            For Each value As Object In entry.Properties(key)
                                sPropertyValues += Convert.ToString(value) + ";"
                            Next
                            sPropertyValues = sPropertyValues.Substring(0, sPropertyValues.Length - 1)
                            entryStr.AppendLine(key + "=" + sPropertyValues)
                        Next
                        Utilities.objEventLog.AddLog("Description", "@GETUSER:Successfully retrieved user entry by name.  Username: " & LoggedOnUserName & " Entry object: " & entryStr.ToString, _portalSettings, -1, Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT)
                    Else
                        Utilities.objEventLog.AddLog("Description", "@GETUSER:Could not retrieve user entry by name.  Username: " & LoggedOnUserName, _portalSettings, -1, Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT)
                    End If
                End If
#If DEBUG Then
                Dim key As String
                For Each key In entry.Properties.PropertyNames
                    Dim sPropertyValues As String = ""
                    For Each value As Object In entry.Properties(key)
                        sPropertyValues += Convert.ToString(value) + ";"
                    Next
                    sPropertyValues = sPropertyValues.Substring(0, sPropertyValues.Length - 1)
                    Debug.Print(key + "=" + sPropertyValues)
                Next
#End If
                    'Check authenticated
                    Dim path As String
                If Not entry Is Nothing Then
                    path = entry.Path
                Else
                    path = _adsiConfig.RootDomainPath
                End If
                If Not IsAuthenticated(path, LoggedOnUserName, LoggedOnPassword) Then
                    Return Nothing
                End If

                ' Return authenticated if no error 
                objAuthUser = New ADUserInfo
                'ACD-6760
                InitializeUser(objAuthUser)
                Dim location As String = Utilities.GetEntryLocation(entry)
                If location.Length = 0 Then
                    location = _adsiConfig.ConfigDomainPath
                End If

                With objAuthUser
                    .PortalID = _portalSettings.PortalId
                    .IsNotSimplyUser = True
                    .Username = LoggedOnUserName
                    .Membership.Password = LoggedOnPassword
                End With

                FillUserInfo(entry, objAuthUser)

                'debug logging issue #54 steven west 2/6/2019
                If _config.EnableDebugMode Then
                    Utilities.objEventLog.AddLog("Description", "@GETUSER:Successfully filled objAuthUser object.  objAuthUser object JSON: " & Json.Serialize(Of ADUserInfo)(objAuthUser).ToString(), _portalSettings, -1, Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT)
                End If

                Return objAuthUser

            Catch exc As Exception
                'debug logging issue #54 steven west 2/6/2019
                If _config.EnableDebugMode Then
                    Utilities.objEventLog.AddLog("Description", "@GETUSER:ERROR:" & exc.Message, _portalSettings, -1, Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT)
                End If
                LogException(exc)
                Return Nothing
            End Try
        End Function

        Public Overloads Overrides Function GetUser(ByVal LoggedOnUserName As String) As ADUserInfo
            Dim objAuthUser As ADUserInfo
            Try
                If _adsiConfig.ADSINetwork Then
                    Dim entry As DirectoryEntry

                    entry = Utilities.GetUserEntryByName(LoggedOnUserName)
#If DEBUG Then
                    Dim key As String
                    For Each key In entry.Properties.PropertyNames
                        Dim sPropertyValues As String = ""
                        For Each value As Object In entry.Properties(key)
                            sPropertyValues += Convert.ToString(value) + ";"
                        Next
                        sPropertyValues = sPropertyValues.Substring(0, sPropertyValues.Length - 1)
                        Debug.Print(key + "=" + sPropertyValues)
                    Next
#End If

                    If Not entry Is Nothing Then
                        objAuthUser = New ADUserInfo
                        'ACD-6760
                        InitializeUser(objAuthUser)
                        Dim location As String = Utilities.GetEntryLocation(entry)
                        If location.Length = 0 Then
                            location = _adsiConfig.ConfigDomainPath
                        End If

                        With objAuthUser
                            .PortalID = _portalSettings.PortalId
                            .IsNotSimplyUser = True
                            .Username = LoggedOnUserName
                            .Membership.Password = Utilities.GetRandomPassword()
                        End With

                        FillUserInfo(entry, objAuthUser)

                    Else
                        objAuthUser = GetSimplyUser(LoggedOnUserName)
                    End If

                Else ' could not find it in AD, so populate user object with minumum info
                    objAuthUser = GetSimplyUser(LoggedOnUserName)
                End If

                Return objAuthUser

            Catch exc As COMException
                LogException(exc)
                Return Nothing
            End Try
        End Function

        Public Overloads Overrides Function GetGroups() As ArrayList
            ' Normally number of roles in DNN less than groups in Authentication,
            ' so start from DNN roles to get better performance
            Try
                Dim colGroup As New ArrayList
                Dim objRoleController As New RoleController
                Dim lstRoles As List(Of RoleInfo) = objRoleController.GetRoles(_portalSettings.PortalId)
                Dim objRole As RoleInfo
                Dim AllAdGroupNames As ArrayList = Utilities.GetAllGroupnames

                For Each objRole In lstRoles
                    ' Auto assignment roles have been added by DNN, so don't need to get them
                    If Not objRole.AutoAssignment Then

                        ' It's possible in multiple domains network that search result return more than one group with the same name (i.e Administrators)
                        ' We better check them all
                        If AllAdGroupNames.Contains(objRole.RoleName) Then
                            Dim group As New GroupInfo

                            With group
                                .PortalID = objRole.PortalID
                                .RoleID = objRole.RoleID
                                .RoleName = objRole.RoleName
                                .Description = objRole.Description
                                .ServiceFee = objRole.ServiceFee
                                .BillingFrequency = objRole.BillingFrequency
                                .TrialPeriod = objRole.TrialPeriod
                                .TrialFrequency = objRole.TrialFrequency
                                .BillingPeriod = objRole.BillingPeriod
                                .TrialFee = objRole.TrialFee
                                .IsPublic = objRole.IsPublic
                                .AutoAssignment = objRole.AutoAssignment
                            End With

                            colGroup.Add(group)
                        End If
                    End If
                Next

                Return colGroup

            Catch exc As COMException
                LogException(exc)
                Return Nothing
            End Try
        End Function



        Public Overloads Overrides Function GetGroups(ByVal arrUserPortalRoles As ArrayList) As ArrayList
            ' Normally number of roles in DNN less than groups in Authentication,
            ' so start from DNN roles to get better performance
            Try
                Dim colGroup As New ArrayList
                'Dim objRoleController As New RoleController
                'Dim lstRoles As ArrayList = objRoleController.GetPortalRoles(_portalSettings.PortalId)
                Dim objRole As RoleInfo
                'Dim AllAdGroupNames As ArrayList = Utilities.GetAllGroupnames

                For Each objRole In arrUserPortalRoles
                    ' Auto assignment roles have been added by DNN, so don't need to get them
                    If Not objRole.AutoAssignment Then

                        ' It's possible in multiple domains network that search result return more than one group with the same name (i.e Administrators)
                        ' We better check them all
                        Dim entry As DirectoryEntry
                        For Each entry In Utilities.GetGroupEntriesByName(objRole.RoleName)
                            Dim group As New GroupInfo

                            With group
                                .PortalID = objRole.PortalID
                                .RoleID = objRole.RoleID
                                .RoleName = objRole.RoleName
                                .Description = objRole.Description
                                .ServiceFee = objRole.ServiceFee
                                .BillingFrequency = objRole.BillingFrequency
                                .TrialPeriod = objRole.TrialPeriod
                                .TrialFrequency = objRole.TrialFrequency
                                .BillingPeriod = objRole.BillingPeriod
                                .TrialFee = objRole.TrialFee
                                .IsPublic = objRole.IsPublic
                                .AutoAssignment = objRole.AutoAssignment
                            End With

                            colGroup.Add(group)
                        Next

                    End If
                Next

                Return colGroup

            Catch exc As COMException
                LogException(exc)
                Return Nothing
            End Try
        End Function

        Public Overrides Function GetAuthenticationTypes() As Array
            Return [Enum].GetValues(GetType(AuthenticationTypes))
        End Function

        Public Overrides Function GetNetworkStatus() As String
            Dim sb As New StringBuilder
            ' Refresh settings cache first
            Configuration.ResetConfig()
            _adsiConfig = Configuration.GetConfig

            sb.Append("<b>[Global Catalog Status]</b>" & "<br>")
            Try
                If _adsiConfig.ADSINetwork Then
                    sb.Append("OK<br>")
                Else
                    sb.Append("FAIL<br>")
                End If
            Catch ex As COMException
                sb.Append("FAIL<br>")
                sb.Append(ex.Message & "<br>")
            End Try

            sb.Append("<b>[Root Domain Status]</b><br>")
            Try
                If Not Utilities.GetRootEntry() Is Nothing Then
                    sb.Append("OK<br>")
                Else
                    sb.Append("FAIL<br>")
                End If
            Catch ex As COMException
                sb.Append("FAIL<br>")
                sb.Append(ex.Message & "<br>")
            End Try

            sb.Append("<b>[LDAP Status]</b><br>")
            Try
                If _adsiConfig.LDAPAccesible Then
                    sb.Append("OK<br>")
                Else
                    sb.Append("FAIL<br>")
                End If
            Catch ex As COMException
                sb.Append("FAIL<br>")
                sb.Append(ex.Message & "<br>")
            End Try

            sb.Append("<b>[Network Domains Status]</b><br>")
            Try
                If Not _adsiConfig.RefCollection Is Nothing AndAlso _adsiConfig.RefCollection.Count > 0 Then
                    sb.Append(_adsiConfig.RefCollection.Count.ToString)
                    sb.Append(" Domain(s):<br>")
                    Dim crossRef As CrossReferenceCollection.CrossReference
                    For Each crossRef In _adsiConfig.RefCollection
                        sb.Append(crossRef.CanonicalName)
                        sb.Append(" (")
                        sb.Append(crossRef.NetBIOSName)
                        sb.Append(")<br>")
                    Next

                    If _adsiConfig.RefCollection.ProcesssLog.Length > 0 Then
                        sb.Append(_adsiConfig.RefCollection.ProcesssLog & "<br>")
                    End If

                Else
                    sb.Append("[LDAP Error Message]<br>")
                End If
            Catch ex As COMException
                sb.Append("[LDAP Error Message]<br>")
                sb.Append(ex.Message & "<br>")
            End Try

            If _adsiConfig.ProcessLog.Length > 0 Then
                sb.Append(_adsiConfig.ProcessLog & "<br>")
            End If

            Return sb.ToString

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]   27/04/2004  Initially the preferred local was set to the 
        '''                             CurrentCulture. Occasionaly this is reset to English and it 
        '''                             overwrites the user's Preferredlocale. I set it here to always
        '''                             use the portal's language setting.
        '''     [mhorton]   27/04/2009 Initialize the TimeZone.
        ''' </history>
        ''' -------------------------------------------------------------------

        Private Sub InitializeUser(ByVal objUser As ADUserInfo)
            objUser.Profile.InitialiseProfile(_portalSettings.PortalId)

            'ACD-9442
            objUser.Profile.PreferredLocale = _portalSettings.DefaultLanguage
            objUser.Profile.PreferredTimeZone = _portalSettings.TimeZone
        End Sub


    End Class
End Namespace