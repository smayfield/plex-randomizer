# Plex Movie Randomizer

A console application that helps you discover movies in your Plex library by randomly selecting films based on genre preferences.

## Features

- Random movie selection from your Plex library
- Filter movies by genre
- Display comprehensive movie information:
  - Title and release year
  - Genre(s)
  - Summary/plot description
  - User rating
  - Duration
  - Watched status (whether you've seen it before)
- Generate direct playback URLs to immediately watch the selected movie

## Getting Started

### Prerequisites

- .NET 9.0 or later
- A Plex Media Server
- Proper configuration (Plex server address, authentication token, and library ID)

### Configuration

Before using the application, you need to configure it to connect to your Plex server by editing the `appsettings.json` file:

```json
{
  "PlexSettings": {
    "BaseUrl": "http://your-plex-server:32400",
    "Token": "your-plex-token",
    "LibraryId": "your-library-id"
  }
}
```

Replace the placeholder values with your actual Plex server information:

- `BaseUrl`: The URL of your Plex server, including the port (typically 32400)
- `Token`: Your Plex authentication token
- `LibraryId`: The ID of the movie library you want to use

### How to Use

1. Run the application
2. When prompted, enter a genre you'd like to watch (or press Enter for any genre)
3. The application will display a randomly selected movie from your library that matches your genre choice
4. Copy and paste the provided URL into your web browser to start watching the movie
5. Press any key to select a different movie, or press ESC to quit the application

## How to Get Your Plex Token

1. Sign in to Plex in a web browser
2. Access any library content
3. Open browser developer tools (F12 or right-click → Inspect)
4. Go to the Network tab
5. Click on any request to the Plex server
6. Look for the X-Plex-Token parameter in the request headers or URL
   - It will look something like: X-Plex-Token=ABC123def456ghi789

Alternatively, you can find your token by:

1. Sign in to Plex
2. Click on your username → Account Settings
3. In Account, find the "Authorized Devices" section
4. The token appears in the XML data when you click on a device

## How to Get Your Library ID

1. In your browser, navigate to: http://your-plex-server:32400/library/sections?X-Plex-Token=your-token
2. This returns XML containing all your libraries
3. Find the movie library you want to use
4. The key attribute is your Library ID

Example of the XML response:
```xml
<MediaContainer>
  <Directory key="1" title="Movies" type="movie" />
  <Directory key="2" title="TV Shows" type="show" />
</MediaContainer>
```

In this example, if "Movies" is your library, then your Library ID is "1".

## Troubleshooting

### App Can't Find appsettings.json
Make sure the appsettings.json file is in the same directory as the executable. If you're running from source, it should be in the project's root directory.

### Incorrect Playback URL
If the playback URL doesn't work, ensure you have the correct machine identifier for your Plex server. The application should retrieve this automatically, but if you experience issues, you can verify your machine identifier by looking at the URL when you're browsing your Plex server in a web browser.

### No Movies Found in Genre
If no movies are found when you enter a genre, try checking the exact genre names in your Plex library. Genre names are case-insensitive, but they need to match exactly what Plex uses (e.g., "Sci-Fi" vs "Science Fiction").

## Building from Source

```
git clone https://github.com/yourusername/plex-movie-randomizer.git
cd plex-movie-randomizer
dotnet build
dotnet run
```

