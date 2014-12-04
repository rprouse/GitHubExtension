GitHub Visual Studio Extension
===============

[![Build status](https://ci.appveyor.com/api/projects/status/cq3t38xds110oxb8/branch/master?svg=true)](https://ci.appveyor.com/project/rprouse/githubextension/branch/master)

A visual studio extension for working with issues on GitHub. 

Access and manage GitHub issues for repositories that you have commit access to. You can filter and view issues for a repository, edit issues, add comments and close issue. This is the first Alpha release, more features are coming. 

## Download ##

The easiest way to download is by going to *Tools | Extensions* in Visual Studio and searching for the GitHub Extension. It is also available in the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/e4ba5ebd-bcd5-4e20-8375-bb8cbdd71d7e) and in the [GitHub Releases](https://github.com/rprouse/GitHubExtension/releases) for the project. 

## Instructions ##

- To view a list of open issues, go to **View | Other Windows | GitHub Issue List** (Ctrl+W, Ctrl+G)
- Log in to GitHub by clicking the logon icon at the upper right of the issue list window
- Open the issue window by double clicking an issue in the list, or by going to **View | Other Windows | GitHub Issue Window** (Ctrl+W, Ctrl+H)
- Add a new issue to the selected repository with the + button in the issue list, or from **Tools | New Issue on GitHub** (Ctrl+W, Ctrl+I)
- Edit an issue with the edit button on the Issue window
- Add comments to, or close and issue with the comment button on the issue window

## Two Factor Authentication ##

We do not currently support GitHub's Two-Factor Authentication system. However, you can generate a Personal Access Token to log in to your GitHub account instead.

1. Visit the following URL: https://github.com/settings/tokens/new
2. Enter a description in the Token description field, like "Visual Studio token".
3. Click Create Token.
4. Your new Personal Access token will be displayed.
5. Copy this token, and enter it in the Token text box in the logon dialog. You can now log in as usual.

If you ever want to revoke the token, visit the GitHub Applications settings page and click Delete next to the key you wish to remove.

## Credits ##

- Button and application images by [Font Awesome](http://fortawesome.github.io/Font-Awesome/) ([SIL OFL 1.1](http://scripts.sil.org/OFL))

## Screenshots ##

### Login Window ###

![Login](/images/logon.png)

### Issue List ###

![Issue List](/images/issue_list.png)

### Issue Window ###

![Issue Window](/images/issue.png)

## Building ##

This project supports Visual Studio 2012 and newer. You will need the Visual Studio SDK installed for the particular version of Visual Studio you are testing.

Optionally, set the GitHub `CLIENT_ID` and `CLIENT_SECRET` in `Secrets.cs` which is in the
`Model` folder of the `GitHubIssues` project. Note that if these values are not set, the
only supported authentication method will be specifying an access token generated according
to the steps described in **Two Factor Authentication**, above.

To debug, 

1. Set `GitHubExtension` as the startup project
2. Select **Debug** &rarr; **Start Debugging**
