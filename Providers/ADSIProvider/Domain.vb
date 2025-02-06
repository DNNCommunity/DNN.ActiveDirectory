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
Imports DotNetNuke.Common.Utilities

Namespace DotNetNuke.Authentication.ActiveDirectory.ADSI
    Public Class Domain
        Inherits DirectoryEntry

        Private mChildDomains As New ArrayList
        'One level child
        Private mAllChildDomains As New ArrayList
        'All level child
        Private mParentDomain As Domain
        Private mDistinguishedName As String = ""
        Private mNetBIOSName As String = ""
        Private mCanonicalName As String = ""
        Private mLevel As Integer
        Private mChildPopulate As Boolean = False
        Private config As ConfigInfo
        Private utilities As IUtilities
        Private adsiConfiguration As IConfiguration

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Sub New(ByVal configuration As IConfiguration,
                ByVal utilities As IUtilities)
            MyBase.New()
            Me.adsiConfiguration = configuration
            Me.config = configuration.GetConfig
            Me.utilities = utilities
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Sub New(ByVal Path As String, ByVal UserName As String, ByVal Password As String,
                 ByVal AuthenticationType As AuthenticationTypes,
                 ByVal configuration As IConfiguration,
                 ByVal utilities As IUtilities)
            MyBase.New(Path, UserName, Password, AuthenticationType)
            Me.adsiConfiguration = configuration
            Me.config = configuration.GetConfig
            Me.utilities = utilities
            PopulateInfo()
            PopulateChild(Me)
            mChildPopulate = True

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Sub New(ByVal Path As String,
                 ByVal configuration As IConfiguration,
                 ByVal utilities As IUtilities)
            MyBase.New(Path)
            Me.adsiConfiguration = configuration
            Me.config = configuration.GetConfig
            Me.utilities = utilities
            PopulateInfo()
            PopulateChild(Me)
            mChildPopulate = True
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Sub PopulateInfo()

            mDistinguishedName = CType (MyBase.Properties (Configuration.ADSI_DISTINGUISHEDNAME).Value, String)
            mCanonicalName = ADSI.Utilities.ConvertToCanonical(mDistinguishedName, False)

            ' Note that this property will be null string if LDAP is unaccessible
            mNetBIOSName = Utilities.CanonicalToNetBIOS (mCanonicalName)

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Sub PopulateChild(ByVal Domain As Domain)
            Dim objSearch As New Search(Domain)

            objSearch.SearchScope = SearchScope.OneLevel
            objSearch.AddFilter(ADSI.Configuration.ADSI_CLASS, CompareOperator.Is, ObjectClass.domainDNS.ToString)

            Dim resDomains As ArrayList = objSearch.GetEntries
            Dim entry As DirectoryEntry

            For Each entry In resDomains
                Dim child As Domain = GetDomain(entry.Path, adsiConfiguration, utilities)

                If Not child Is Nothing Then
                    child.ParentDomain = Domain
                    child.Level = Domain.Level + 1
                    ' Add this child into childDomains collection
                    Domain.ChildDomains.Add(child)
                    ' add this child and all it's child into allchilddomains collection
                    Domain.AllChildDomains.Add(child)
                    Domain.AllChildDomains.AddRange(child.AllChildDomains)
                End If
            Next

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     Accessing ADs costs lots of resource so we better put ADs object into app cache
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetDomain(ByVal Path As String, ByVal configuration As IConfiguration,
                 ByVal utilities As IUtilities) As Domain
            Return GetDomain(Path, "", "", AuthenticationTypes.Delegation, configuration,
                 utilities)
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     Accessing ADs costs lots of resource so we better put ADs object into app cache
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetDomain(ByVal Path As String, ByVal UserName As String, ByVal Password As String,
                                          ByVal AuthenticationType As AuthenticationTypes, ByVal configuration As IConfiguration,
                 ByVal utilities As IUtilities) As Domain

            Dim Domain As Domain = CType(DataCache.GetCache(Path), Domain)
            If Domain Is Nothing Then
                If (UserName.Length > 0) AndAlso (Password.Length > 0) Then
                    Domain = New Domain(Path, UserName, Password, AuthenticationType, configuration,
                 utilities)
                Else
                    Domain = New Domain(Path, configuration, utilities)
                End If

                DataCache.SetCache(Path, Domain)
            End If

            Return Domain

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     Clear domain object in cache
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Sub ResetDomain (ByVal Path As String)
            DataCache.RemoveCache (Path)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     Return one level child domains 
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ChildDomains() As ArrayList
            Get
                Return mChildDomains
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     Return child all level child domains 
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property AllChildDomains() As ArrayList
            Get
                Return mAllChildDomains
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     Return parent domain of this domain
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Property ParentDomain() As Domain
            Get
                Return mParentDomain
            End Get
            Set (ByVal Value As Domain)
                mParentDomain = Value
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
        Public Property Level() As Integer
            Get
                Return mLevel
            End Get
            Set (ByVal Value As Integer)
                mLevel = Value
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
        Public Property ChildPopulate() As Boolean
            Get
                Return mChildPopulate
            End Get
            Set (ByVal Value As Boolean)
                mChildPopulate = Value
            End Set
        End Property
    End Class
End Namespace
