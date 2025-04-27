#Plex

##How to Get Your Plex Token

1. Sign in to Plex in a web browser
1. Access any library content
1. Open browser developer tools (F12 or right-click → Inspect)
1. Go to the Network tab
1. Click on any request to the Plex server
1. Look for the X-Plex-Token parameter in the request headers or URL
	1. It will look something like: X-Plex-Token=ABC123def456ghi789

Alternatively, you can find your token by:

1. Sign in to Plex
1. Click on your username → Account Settings
1. In Account, find the "Authorized Devices" section
1. The token appears in the XML data when you click on a device

##How to Get Your Library ID

1. In your browser, navigate to: http://your-plex-server:32400/library/sections?X-Plex-Token=your-token
1. This returns XML containing all your libraries
1. Find the movie library you want to use
1. The key attribute is your Library ID

Example of the XML response:
```xml
<MediaContainer>
  <Directory key="1" title="Movies" type="movie" />
  <Directory key="2" title="TV Shows" type="show" />
</MediaContainer>
```

In this example, if "Movies" is your library, then your Library ID is "1".

