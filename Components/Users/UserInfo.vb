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
Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Authentication.ActiveDirectory
    Public Class ADUserInfo
        Inherits UserInfo
        Implements IAuthenticationObjectBase

        Private mGUID As String = ""
        Private mLocation As String = ""
        Private mCName As String = ""
        Private mPrincipalName As String = ""
        Private mDistinguishedName As String = ""
        Private msAMAccountName As String = ""
        Private mIsAuthenticated As Boolean
        Private mAuthenticationExists As Boolean = False
        ' Additional properties which are not provided by MemberRole
        Private mDepartment As String
        Private mManager As String
        Private mHomePhone As String
        Private mAssistant As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Sub New()
            MyBase.New()
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>IsNotSimplyUser
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]   10/05/2009 Created  - WorkItem:2943
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Property IsNotSimplyUser() As Boolean
            Get
                Return mIsAuthenticated
            End Get
            Set (ByVal value As Boolean)
                mIsAuthenticated = value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Name() As String Implements IAuthenticationObjectBase.Name
            Get
                Return sAMAccountName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ObjectClass() As ObjectClass Implements IAuthenticationObjectBase.ObjectClass
            Get
                Return ObjectClass.person
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Property AuthenticationExists() As Boolean
            Get
                Return mAuthenticationExists
            End Get
            Set (ByVal Value As Boolean)
                mAuthenticationExists = Value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Property CName() As String
            Get
                Return mCName
            End Get
            Set (ByVal Value As String)
                mCName = Value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Property DistinguishedName() As String
            Get
                Return mDistinguishedName
            End Get
            Set (ByVal Value As String)
                mDistinguishedName = Value
            End Set
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Property sAMAccountName() As String
            Get
                Return msAMAccountName
            End Get
            Set (ByVal Value As String)
                msAMAccountName = Value
            End Set
        End Property
    End Class
End Namespace
