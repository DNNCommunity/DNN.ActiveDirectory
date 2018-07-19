# DNN.ActiveDirectory
Active Directory authentication for DNN

# Migration to "AD-Pro Authentication"
This chapter describes how in easy way migrate from "DNN.ActiveDirectory" to "AD-Pro Authentication" plugin. 

"AD-Pro Authentication" will mean fewer compatibility issues in DNN v8/9/Evoq. Additionally it offers more features around user profile/group sync.

## Settings worth to note
"DNN.ActiveDirectory" settings can be reused. Before you uninstall "DNN.ActiveDirectory" provider copy following settings because "AD-Pro Authentication" will need it:
* Default Domain
* Root Domain also known as LDAP
* Username
* Password, it's encrypted 
* Authentication type

See figure below:

![Provider options](http://doc.glanton.com/AD-Pro-Authentication/_images/migration-from-dnn-active-directory_01.png)

In case the provider menu is not available, same settings can be found in DNN database. Execute following SQL query to display "DNN Active Directory" config:

`SELECT * FROM PortalSettings WHERE SettingName LIKE 'AD_%'`

## Disable "DNN.ActiveDirectory"
Before you install AD-Pro plugin, the "DNN Active Directory" needs to be disabled to prevent any conflicts.
1. In Authentication Settings set plugin into `Disable` mode.
![Disabled provider](http://doc.glanton.com/AD-Pro-Authentication/_images/migration-from-dnn-active-directory_02.png)
2. In `web.config` file comment following lines:
```
<location path="DesktopModules/AuthenticationServices/ActiveDirectory/WindowsSignin.aspx">
<!-- Disable Forms Authentication -->
	<formsAuthenticationWrapper enabled="false" />
	<system.webServer>
		<security>
			<!-- Enable IIS Windows authentication for the login page -->
			<authentication>
				<windowsAuthentication enabled="true" useKernelMode="false">
					<providers>
						<clear/>
						<add value=”NTLM”/>
					</providers>
				</windowsAuthentication>
                           	<anonymousAuthentication enabled="false" />
			</authentication>
		</security>
	</system.webServer>
</location>
```
## Install "Connection Manager" & "AD-Pro Authentication" module
Now you are ready to install "Connection Manager" & "AD-Pro Authentication" module. 
This process is welly described in separate User Guides:
* ["Connection Manager" installation process](http://doc.glanton.com/Connection-Manager/installation.html)
* ["AD-Pro Authentication" installation process](http://doc.glanton.com/AD-Pro-Authentication/installation.html)

"DNN.ActiveDirectory" usually saves DNN usernames in format `Domain\Username`. To keep consistency, set in "AD-Pro Authentication" username format to `Default with domain`, here is [more info](http://doc.glanton.com/AD-Pro-Authentication/advanced-settings.html#username-formats) about available username formats.
Same username format allows reuse DNN identities created  by "DNN.ActiveDirectory" provider.

Same settings imported to "AD-Pro" plugin will look as on figure below:
![AD-Pro panel](http://doc.glanton.com/AD-Pro-Authentication/_images/migration-from-dnn-active-directory_03.png)

If you will have any troubles Glanton will support your journey to “AD-Pro Authentication”.
