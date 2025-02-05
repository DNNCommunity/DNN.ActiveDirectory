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
Imports DotNetNuke.Security.Membership
Imports DotNetNuke.Security.Roles
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Authentication.ActiveDirectory
    Public Class UserController

#Region "Private Shared Members"

        Private Shared dataProvider As DataProvider = DataProvider.Instance()
        Private Shared mRoleName As String = ""
        Private config As ConfigInfo
        Private authenticationProvider As IAuthenticationProvider
        Private portalController As IPortalController
        Private roleController As IRoleController
        Private groupController As IGroupController
        Private portalSettings As PortalSettings
#End Region

        Sub New(ByVal configuration As IConfiguration,
                ByVal authenticationProvider As IAuthenticationProvider,
                ByVal portalController As IPortalController,
                ByVal roleController As IRoleController,
                ByVal groupController As IGroupController,
                )
            Me.config = configuration.GetConfig()
            Me.authenticationProvider = authenticationProvider
            Me.portalController = portalController
            Me.roleController = roleController
            Me.groupController = groupController
            Me.portalSettings = Me.portalController.GetCurrentSettings
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     User object with info obtained from Active Directory
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Function GetUser (ByVal LoggedOnUserName As String) As ADUserInfo
            Return authenticationProvider.GetUser(LoggedOnUserName)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     User object with info obtained from Active Directory
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Function GetUser (ByVal LoggedOnUserName As String, ByVal LoggedOnPassword As String) As ADUserInfo
            Return authenticationProvider.GetUser(LoggedOnUserName, LoggedOnPassword)
        End Function

        '''' -----------------------------------------------------------------------------
        '''' <summary>
        '''' CreateDNNUser persists the DNN User information to the Database
        '''' </summary>
        '''' <remarks>
        '''' </remarks>
        '''' <param name="user">The user to persist to the Data Store.</param>
        '''' <returns>The UserId of the newly created user.</returns>
        '''' <history>
        ''''     [cnurse]	12/13/2005	created
        ''''     [mhorton]     06/12/2008  ACD-5737
        '''' </history>
        '''' -----------------------------------------------------------------------------
        '<Obsolete ("No longer used")> _
        'Private Function CreateDNNUser (ByRef user As ADUserInfo) As UserCreateStatus

        '    Dim objSecurity As New PortalSecurity
        '    Dim _
        '        userName As String = _
        '            objSecurity.InputFilter (user.Username, _
        '                                     PortalSecurity.FilterFlag.NoScripting Or _
        '                                     PortalSecurity.FilterFlag.NoAngleBrackets Or _
        '                                     PortalSecurity.FilterFlag.NoMarkup)
        '    Dim _
        '        email As String = _
        '            objSecurity.InputFilter (user.Email, _
        '                                     PortalSecurity.FilterFlag.NoScripting Or _
        '                                     PortalSecurity.FilterFlag.NoAngleBrackets Or _
        '                                     PortalSecurity.FilterFlag.NoMarkup)
        '    Dim _
        '        lastName As String = _
        '            objSecurity.InputFilter (user.LastName, _
        '                                     PortalSecurity.FilterFlag.NoScripting Or _
        '                                     PortalSecurity.FilterFlag.NoAngleBrackets Or _
        '                                     PortalSecurity.FilterFlag.NoMarkup)
        '    Dim _
        '        firstName As String = _
        '            objSecurity.InputFilter (user.FirstName, _
        '                                     PortalSecurity.FilterFlag.NoScripting Or _
        '                                     PortalSecurity.FilterFlag.NoAngleBrackets Or _
        '                                     PortalSecurity.FilterFlag.NoMarkup)
        '    Dim createStatus As UserCreateStatus = UserCreateStatus.Success
        '    Dim _
        '        displayName As String = _
        '            objSecurity.InputFilter (user.DisplayName, _
        '                                     PortalSecurity.FilterFlag.NoScripting Or _
        '                                     PortalSecurity.FilterFlag.NoAngleBrackets Or _
        '                                     PortalSecurity.FilterFlag.NoMarkup)
        '    Dim updatePassword As Boolean = user.Membership.UpdatePassword
        '    Dim isApproved As Boolean = user.Membership.Approved

        '    Try
        '        user.UserID = _
        '            CType ( _
        '                dataProvider.AddUser (user.PortalID, userName, firstName, lastName, user.AffiliateID, _
        '                                      user.IsSuperUser, email, displayName, updatePassword, isApproved, - 1), _
        '                Integer)
        '        DataCache.ClearPortalCache (user.PortalID, False)
        '        'ACD-5737
        '        If Not user.IsSuperUser Then

        '            Dim objRoles As New RoleController
        '            Dim objRole As RoleInfo

        '            ' autoassign user to portal roles
        '            Dim arrRoles As ArrayList = objRoles.GetPortalRoles (user.PortalID)
        '            Dim i As Integer
        '            For i = 0 To arrRoles.Count - 1
        '                objRole = CType (arrRoles (i), RoleInfo)
        '                If objRole.AutoAssignment = True Then
        '                    objRoles.AddUserRole (user.PortalID, user.UserID, objRole.RoleID, Null.NullDate, _
        '                                          Null.NullDate)
        '                End If
        '            Next
        '        End If
        '    Catch ex As Exception
        '        'Clear User (duplicate User information)
        '        user = Nothing
        '        createStatus = UserCreateStatus.ProviderError
        '    End Try

        '    Return createStatus

        'End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' This routine is more accurated, 
        ''' Prevent user assign to admin role in case user logon as LOCAL\Administrator
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]     20/06/2008  Fixed code for ACD-7422 and ACD-6960
        '''     [mhorton]     24/06/2008  Further fixes for ACD-6960
        '''     [mhorton]     30/11/2008  Fixed code for ACD-8844
        '''     [mhorton]     29/05/2011  Fixed code for Item 6735
        ''' </history>
        ''' -------------------------------------------------------------------

        Public Overloads Sub AddUserRoles(ByVal PortalID As Integer, ByVal AuthenticationUser As ADUserInfo)
            Try
                Dim objPortal As PortalInfo = portalController.GetPortal(PortalID)
                Dim objRoleInfo As New RoleInfo
                'Get all active directory groups the user belongs to.
                Dim arrUserADGroups As ArrayList = Utilities.GetADGroups(AuthenticationUser.Username)

                'Get all portal roles that the user does belong to.
                Dim strUserPortalRoles As List(Of UserRoleInfo) = roleController.GetUserRoles(AuthenticationUser, True)
                Dim arrUserPortalRoles As New ArrayList
                'We want to remove any Auto Assigned roles from the user's portal roles.
                For Each strRole As UserRoleInfo In strUserPortalRoles
                    objRoleInfo = roleController.GetRoleByName(PortalID, strRole.RoleName)
                    If Not (objRoleInfo.AutoAssignment) Then
                        arrUserPortalRoles.Add(objRoleInfo)
                    End If
                Next
                'Compare the active directory groups the user belongs to against the portal roles the user belongs to.
                'If both exist then do nothing, if only the group exists put the group into a separate array for later processing,
                'if only the role exists also put it into a separate array for later processing.
                Dim arrADGroupOnly As New ArrayList
                Dim arrRolesOnly As New ArrayList
                Dim bMatch As Boolean
                'Get the AD groups that don't match a portal role that the user belongs to.
                For Each strGroup As String In arrUserADGroups
                    bMatch = False
                    For Each strRole As UserRoleInfo In strUserPortalRoles
                        If strRole.RoleName = strGroup Then
                            bMatch = True
                            Exit For
                        End If
                    Next
                    If Not (bMatch) Then
                        arrADGroupOnly.Add(strGroup)
                    End If
                Next

                For Each objRoleInfo In arrUserPortalRoles
                    bMatch = False
                    For Each strGroup As String In arrUserADGroups
                        If strGroup = objRoleInfo.RoleName Then
                            bMatch = True
                            Exit For
                        End If
                    Next
                    If Not (bMatch) Then
                        arrRolesOnly.Add(objRoleInfo)
                    End If
                Next

                'Check the Active Directory groups the user belongs to only and see if there's a portal role that matches.
                Dim arrPortalRoles As List(Of RoleInfo) = roleController.GetRoles(PortalID)
                For Each objRoleInfo In arrPortalRoles
                    If Not (objRoleInfo.AutoAssignment) Then
                        If Not (objRoleInfo.RoleID = objPortal.AdministratorRoleId) Then
                            If arrADGroupOnly.Contains(objRoleInfo.RoleName) Then
                                roleController.AddUserRole(PortalID, AuthenticationUser.UserID, objRoleInfo.RoleID, RoleStatus.Approved, False, Date.Today,
                                                           Null.NullDate)
                            End If
                        End If
                    End If
                Next

                'Check the portal roles the user belongs to only and see if there's an Active Directory group that matches
                Dim arrADGroups As ArrayList = groupController.GetGroups(arrRolesOnly)
                For Each objRoleInfo In arrADGroups
                    If Not (objRoleInfo.RoleID = objPortal.AdministratorRoleId) Then
                        Security.Roles.RoleController.DeleteUserRole(AuthenticationUser, objRoleInfo, Me.portalSettings, False)
                    End If
                Next

            Catch exc As Exception
                LogException(exc)
            End Try
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' Search predicate returns true if a strings match.
        ''' </remarks>
        ''' <history>
        '''     [mhorton]     30/11/2008  Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Shared Function RolesExists (ByVal s As String) _
            As Boolean

            ' AndAlso prevents evaluation of the second Boolean
            ' expression if the string is so short that an error
            ' would occur.
            If (s.ToLower = mRoleName.ToLower) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' This functions updates the users information from Active Directory
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [Stuart]	September 12,2006	Created 
        '''     [mhorton]   02/17/2012 User's profile was getting blanked when getting updated - Item 7739
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Function UpdateDnnUser(ByVal authenticationUser As UserInfo) As Boolean
            'Updating user information
            Users.UserController.UpdateUser(authenticationUser.PortalID, authenticationUser)
            Return True

        End Function
    End Class
End Namespace
