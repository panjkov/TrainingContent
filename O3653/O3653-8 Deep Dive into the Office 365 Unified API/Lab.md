# Deep Dive into the Office 365 Microsoft Graph
In this lab, you will use the Microsoft Graph to access & program against Office 365 data using both the raw REST API as well as using Windows 10 application for the Microsoft Graph.

## Prerequisites
1. You must have an Office 365 tenant and Microsoft Azure subscription to complete this lab. If you do not have one, the lab for **O3651-7 Setting up your Developer environment in Office 365** shows you how to obtain a trial.
1. You must have access to an Exchange mailbox within an Office 365 developer tenancy.
1. You must have some files within your Office 365 OneDrive for Business. 
1. You must have Fiddler (http://www.telerik.com/fiddler) or another HTTP debugging proxy tool installed to complete exercise 2.
1. You must have Visual Studio 2015 to complete exercise 3. 
1. This lab requires you to use multiple starter files or an entire starter project from the GitHub location. You can either download the whole repo as a zip or clone the repo https://github.com/OfficeDev/TrainingContent.git for those familiar with git.

## Exercise 1: Create an Azure AD Application with Necessary Permissions for the Microsoft Graph 
In this exercise, you will create an Azure AD application using the Azure Management portal and grant the application the necessary permissions to work with the Microsoft Graph.

1. Within a browser, navigate to the **Azure Management Portal**: https://manage.windowsazure.com
1. Enter the email address and password of an account that have permissions to manage the directory of the Azure AD tenant (e.g. admin@sample.onmicrosoft.com).
1. In the left-hand navigation, scroll down to and click on Active Directory.
1. Click on the name of a directory to select it and display. Depending on the state of your portal, you will see the Quick Start page, or the list of Users. On either page, click **Applications** in the toolbar. 

	![Screenshot of the previous step](Images/Figure01.png)

1. Click the **Add** button at the bottom of the display.
1. On the **What do you want to do** page, click **Add an application my organization is developing**. This will start the **Add Application** wizard.
1. In the **Add Application** wizard, enter a name of **My First Microsoft Graph App** and choose the type **Web Application and/or Web API**. Click the arrow to advance to the next page of the wizard.
1. In the **App Properties** page, enter a **SIGN-ON URL** of **https://dev.office.com**
1. Enter an **App ID Uri** of **http://[your-O365-tenant-id].onmicrosoft.com/MicrosoftGraphApiApp**.

	> NOTE: The App ID Uri must be unique within the Azure tenancy. Using a host name that matches your tenant name helps to prevent confusion, and using a value for the path that matches the app name helps to enforce uniqueness. This value can be changed if the app name or purpose changes.

1. Click the **check** image in the lower right of the wizard to create the application. The application Quick Start page will display once the application is created.

	![Screenshot of the previous step](Images/Figure02.png)

1. On the application Quick Start page, click on **CONFIGURE** in the toolbar.
1. Scroll down to the **Keys** section. 
	1. In the **Select Duration** dropdown, select **1 year**. 
	1. Then click the **Save** button at the bottom of the page.

	*The page will refresh and include the value of the key. In addition, a message is displayed advising that the key will not be shown a second time.*

1. For both the **Client ID** and **Key**, copy these values to a text file as you will need them later in this lab.

	![Screenshot of the previous step](Images/Figure03.png)

### Grant App Necessary Permissions to the Microsoft Graph
1. Scroll down to the **permissions to other applications** section. 
	1. Click the **Add Application** button.
	1. In the **Permissions to other applications** dialog, click the **PLUS** icon next to the **Microsoft Graph** option.
	1. Click the **CHECK** icon in the lower right corner.
	1. For the new **Microsoft Graph** application permission entry, select the **Delegated Permissions** dropdown on the same line and then select the following permissions:
		- Read files that the user selects
		- Read user files and files shared with user
		- Read all groups
		- Read all users' full profiles
		- Sign in and read user profile
	1. Click the **Save** button at the bottom of the page.

### Get the Azure AD Tenant ID
1. Click on the **View Endpoints** button in the gutter at the bottom of the page.
1. The dialog that appears you will see a list of a number of different endpoints. All of them contain a GUID which is the unique ID of the Azure AD tenant for the application as shown in the following figure:

	![Screenshot of the previous step](Images/Figure04.png)

	Copy the GUID from any of the URLs and save them to a text file, just like you did for the client ID & key earlier, as you will need this later. 

In this exercise you created an Azure AD application using the Azure Management portal and granted the application the necessary permissions to work with the Microsoft Graph.


## Exercise 2: Use the Raw REST API Interface of the Microsoft Graph
In this exercise, you will use the raw REST API interface of the Microsoft Graph to interact with the different capabilities. In order to call the Microsoft Graph, you must pass along a valid OAuth2 access token. To obtain an access token you must first authenticate with Azure AD and obtain an authorization code.

### Authenticate & Obtain an Authorization Code from Azure AD 
Use the Azure AD authorization endpoint to authenticate & obtain an authorization code.

1. Take the following URL and replace the `{tenant-id}` & `{client-id}` tokens with values obtained / set on the Azure AD application.

	````
	https://login.microsoftonline.com/{tenant-id}/oauth2/authorize?
	client_id={client-id}
	&resource=https://graph.microsoft.com/
	&redirect_uri=https://dev.office.com
	&response_type=code
	````

1. Open Fiddler.
1. Open a browser navigate to the above URL after you replaced the tokens. Be sure to remove any line breaks from the above URL that were added for readability.
	1. You will be prompted to login using the same account you used to create the Azure AD application.
	1. After logging in you will be taken to a non-existent page. This is not an error, there is just no site setup for this application yet. The important information is in the actual data sent to the page.
1. Open Fiddler and find the last session that took you to the current page after logging into Azure AD. The following figure shows what Fiddler will likely look like for you, with the highlighted session you are interested in. Specifically, you are looking for a session that has a `/?code=` in the URL:

	![Screenshot of the previous step](Images/Figure05.png)

	> NOTE: To simplify the screenshot, session requests for script & image files have been removed.

1. With the session selected in Fiddler, click the **Inspector** tab and then click the **WebForms** button. This will show a list of all the values submitted to the current page.
1. Copy the value for the **code** to the text file; this is the authorization code that can be used to obtain an access token.

## Obtain an OAuth2 Access Token for the Microsoft Graph
Use the Azure AD token endpoint to obtain an access token for the Microsoft Graph using the authorization code you just obtained.

1. Take the following URL and replace the `{tenant-id}` token with the values obtained in the previous exercise:

	````
	https://login.microsoftonline.com/{tenant-id}/oauth2/token
	```` 

1. Within Fiddler, click the **Composer** tab.
1. Set the HTTP action to **POST** and copy the URL above with the replaced token into the address path.
1. Within the box just below the HTTP action & URL, add the following HTTP headers:

	````
	Accept: application/json
	Content-Type: application/x-www-form-urlencoded
	````

1. Now, take the following and replace the `{client-id}` token with the value from the first exercise. Replace the `{url-encoded-client-secret}` token with the URL encoded value of the client secret from the first exercise in the lab.

	> To get the URL encoded value, search for the phrase *url encode* on [http://www.bing.com]. It will display a utility to paste the value you obtained in the first exercise and conver it to the URL encoded version.
	
	Lastly, replace the `{authorization-code}` token with the code that you got from the previous step, using Fiddler.

	````
	grant_type=authorization_code
	&redirect_uri=https://dev.office.com
	&client_id={client-id}
	&client_secret={url-encoded-client-secret}
	&resource=https://graph.microsoft.com
	&code={authorization-code}
	````

1. Take the resulting string from all the previous changes and paste it into the **Request Body** box within the **Composer** tab. Be sure to remove all line breaks form the string so you are left with something that looks like the following:

	![Screenshot of the previous step](Images/Figure06.png)  

1. Click the **Execute** button to make the request.
1. Select the session that was just created and click the **Inspectors** tab. Here you see the all the values that were submitted in the request.
1. Click the **JSON** button in the lower part of the **Inspector** tab. This contains the access and refresh tokens from the successful request. 
1. Copy & save the access token just like you've done with the client ID, secret & tenant ID in the previous exercise.

	![Screenshot of the previous step](Images/Figure07.png)  

### Issue Requests to the Microsoft Graph REST Endpoint
Now that you have an access token, create a few requests to the Microsoft Graph REST endpoint.

1. First get information about the currently logged in user from the Microsoft Graph. Within Fiddler's **Composer** tab, do the following:
	1. Set the HTTP action to **GET**.
	1. Set the endpoint URL to **https://graph.microsoft.com/v1.0/me**
	1. Set the HTTP headers to the following values, replacing the `{access-token}` token to the actual token you just obtained in the last step:
	
		````
		Accept: application/json
		Authorization: Bearer {access-token}
		````

	1. Clear the box for the **Request Body**.
	1. Click the **Execute** button.
	1. Select the session you just created and click the **Inspectors** tab. Look at the results that came back to find information about you, the currently logged in user.
	
1. Look at the files in your OneDrive for Business. *This assumes you have at least some files within your OneDrive for Business account... if not the payload returned with be empty*:
	1. Within the Fiddler **Composer** tab...
	1. Set the endpoint URL to **https://graph.microsoft.com/v1.0/me/drive/root/children**
	1. Leave the same HTTP headers in place & click the **Execute** button.
	1. Select the session you just created and click the **Inspectors** tab. Look at the results that came back to find information about the files within your OneDrive for Business account.

1. Now, see how you can query for any user's information provided you have access to it.
	1. Within the Fiddler **Composer** tab...
	1. Set the endpoint URL to the following, replacing the `{tenant-id}` and `{userPrincipalName}` with the values for your tenant: **https://graph.microsoft.com/v1.0/{tenant-id}/users/{userPrincipalName}**
	1. Leave the same HTTP headers in place & click the **Execute** button.
	1. Select the session you just created and click the **Inspectors** tab. Look at the results and notice you are now seeing the details of a user within your Azure AD directory!
	
1. Next, try something the app has not been created access to. In the first exercise the app was not given access to Office 365 Contacts. Try to access contacts to see the error that is returned:
	1. Within the Fiddler **Composer** tab...
	1. Set the endpoint URL to the following, replacing the `{tenant-id}` and `{userPrincipalName}` with the values for your tenant: **https://graph.microsoft.com/v1.0/{tenant-id}/users/{userPrincipalName}/contacts**
	1. Leave the same HTTP headers in place & click the **Execute** button.
	1. Select the session you just created and click the **Inspectors** tab. Notice the request generated a HTTP 403 error with a error message of *Access is denied. Check credentials and try again.*

In this exercise, you used the raw REST API interface of the Microsoft Graph to interact with the different capabilities. 


## Exercise 3: Use the Microsoft Graph in an Native Client Application 
In this exercise, you will use the Microsoft Graph within a Windows 10 application.

### Create a Native Client Application in Azure AD
*Your custom Windows 10 application must be registered as an application in Azure AD in order to work, so we will do that now.*

1. Within a browser, navigate to the **Azure Management Portal**: https://manage.windowsazure.com
1. Enter the email address and password of an account that have permissions to manage the directory of the Azure AD tenant (e.g. admin@sample.onmicrosoft.com).
1. In the left-hand navigation, scroll down to and click on Active Directory.
1. Click on the name of a directory to select it and display. Depending on the state of your portal, you will see the Quick Start page, or the list of Users. On either page, click **Applications** in the toolbar. 
1. Click the **Add** button at the bottom of the display.
1. On the **What do you want to do** page, click **Add an application my organization is developing**. This will start the **Add Application** wizard.
1. In the **Add Application** wizard, enter a name of **My First Microsoft Graph Windows App** and choose the type **Native Client Application**. Click the arrow to advance to the next page of the wizard.
1. Next, set the **Redirect URI** of the application to **http://localhost/microsoftgraphapi** and click the check to save your changes.
1. Once the application has been created, click the **Configure** link the top navigation menu.
1. Find the **Client ID** on the **Configure** page & copy it for later use.
1. Scroll to the bottom of the page to the section **Permissions to Other Applications**.
1. Click the **Add Application** button & select the **Office 365 Microsoft Graph**, then click the check to add it to your application.
1. Select the **Delegated Permissions: 0** control and add the following permissions to the application:
	- Read files that the user selects
	- Read user files and files shared with user
	- Read all groups
	- Read all users' full profiles
	- Sign in and read user profile
1. Click the **Save** icon in the bottom menu.

### Prepare the Visual Studio Solution
Next, take an existing starter project and get it ready to write code that will use the Microsoft Graph.

1. Locate the [\\\O3653\O3653-8 Deep Dive into the Office 365 Unified API\Lab Files](Lab Files) folder that contains a starter project that contains the framework of a Windows 10 application that you will update to call the Microsoft Graph using the native for the Microsoft Graph. Open the solution **O365-Win-Profile** in Visual Studio.
1. In the Solution Explorer, right-click the **O365-Win-Profile** solution node and select **Manage Nuget Packages for Solution**.
1. Click the **Updates** tab.
1. Select the **Select all Packages** checkbox.
1. Click the **Update** button.
1. Click **OK**.
1. Click **I Accept**.
1. Add the Azure AD application's client ID to the project. Open the **App.xaml** file and locate the XML element with the string **ida:ClientID** in it. Paste in the GUID Client ID of the Azure AD application you copied previously in this XML element.
1. Update the login redirect URI for the application that is sent to Azure when logging in. Open the file **AuthenticationHelper.cs** and locate the line that looks like this:

	````c#
	private static Uri redirectUri = new Uri(" ");
	````
	
	Set the value of that string **http://localhost/microsoftgraphapi**.


### Update the Application to Retrieve Data via the Microsoft Graph
*Now you will update the project's codebase to retrieve data from the Microsoft Graph to display the values within the Windows 10 application.*

1. Open the file **UserOperations.cs**.
1. Update the **GetUsersAsync** function to get users from your Azure AD directory:
	1. Locate the function `GetUsersAsync()`.
	1. Replace the existing `return null;` line with the following code:

		````c#
            List<UserModel> retUsers = null;
            try
            {
                var restURL = string.Format("{0}/users?$filter={1}", AuthenticationHelper.EndpointUrl, "(userType eq 'Member')");
                string responseString = await GetJsonAsync(restURL);

                if (responseString != null)
                {
                    retUsers = JObject.Parse(responseString)["value"].ToObject<UserModel[]>().ToList();
                }
            }

            catch (Exception el)
            {
                el.ToString();
            }

            return retUsers;
		````

1. Update the **GetUserAsync** function to get details on a specific user:
	1. Locate the function `GetUserAsync(string userId)`.
	1. Replace the existing `return null;` line with the following code:
	
		````c#
            UserModel user = null;
            try
            {
                var restURL = string.Format("{0}/users/{1}", AuthenticationHelper.EndpointUrl, userId);
                string responseString = await GetJsonAsync(restURL);
                if (responseString != null)
                {
                    user = JObject.Parse(responseString).ToObject<UserModel>();
                }
            }

            catch (Exception el)
            {
                el.ToString();
            }

            return user;
		````

1. Update the **GetUserManagerAsync** function to get a specific user's direct manager:
	1. Locate the function `GetUserManagerAsync(string userId)`.
	1. Replace the existing `return null;` line with the following code:
	
		````c#
            UserModel user = null;
            try
            {
                var restURL = string.Format("{0}/users/{1}/manager", AuthenticationHelper.EndpointUrl, userId);
                string responseString = await GetJsonAsync(restURL);

                if (responseString != null)
                {
                    user = JObject.Parse(responseString).ToObject<UserModel>();
                }
            }
            catch (Exception el)
            {
                el.ToString();
            }
            return user;
		````

1. Update the **GetUserDirectReportsAsync** function to get a specific user's direct reports:
	1. Locate the function `GetUserDirectReportsAsync(string userId)`.
	1. Replace the existing `return null;` line with the following code:
	
		````c#
            List<UserModel> retUsers = null;
            try
            {
                var restURL = string.Format("{0}/users/{1}/directReports", AuthenticationHelper.EndpointUrl, userId);
                string responseString = await GetJsonAsync(restURL);
                if (responseString != null)
                {
                    retUsers = JObject.Parse(responseString)["value"].ToObject<List<UserModel>>();
                }
            }

            catch (Exception el)
            {
                el.ToString();
            }
            return retUsers;
		````

1. Update the **GetUserGroupsAsync** function to get all the groups a user belongs to:
	1. Locate the function `GetUserGroupsAsync(string userId)`.
	1. Replace the existing `return null;` line with the following code:
	
		````c#
            List<GroupModel> retUserGroups = null;
            try
            {
                var restURL = string.Format("{0}/users/{1}/memberof", AuthenticationHelper.EndpointUrl, userId);
                string responseString = await GetJsonAsync(restURL);
                if (responseString != null)
                {
                    var jsonresult = JObject.Parse(responseString)["value"];
                    retUserGroups = new List<GroupModel>();
                    foreach (var item in jsonresult)
                    {
                        if (item["@odata.type"].ToString() == "#microsoft.graph.group")
                        {
                            var group = item.ToObject<GroupModel>();
                            retUserGroups.Add(group);
                        }
                    }

                }
            }

            catch (Exception el)
            {
                el.ToString();
            }
            return retUserGroups;
		````

1. Update the **GetUserFilesAsync** function to get a specified user's files:
	1. Locate the function `GetUserFilesAsync(string userId)`.
	1. Replace the existing `return null;` line with the following code:
	
		````c#
            List<DriveItemModel> fileList = null;
            try
            {
                var restURL = string.Format("{0}/users/{1}/drive/root/children", AuthenticationHelper.EndpointUrl, userId);
                string responseString = await GetJsonAsync(restURL);
                if (responseString != null)
                {
                    fileList = JObject.Parse(responseString)["value"].ToObject<List<DriveItemModel>>();
                }
            }

            catch (Exception el)
            {
                el.ToString();
            }
            return fileList;
		````

1. Save your changes to the file.

### Test the Project
1. With all the changes complete, press **F5** to build & run the project.
1. When prompted, login using your Azure AD account.
1. After successfully logging in, you will see the application load a list of all the users in your Azure AD directory.
1. Select one of the users and you will see it get populated with data from the Azure AD directory.


In this exercise, you used the Microsoft Graph within Windows 10 application.


Congratulations! In this lab you have created your first Azure AD application that enabled access to the Microsoft Graph and used REST API for the Microsoft Graph!
