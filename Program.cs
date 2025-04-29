using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Plex
{
    class Program
    {
        private static IConfiguration Configuration { get; set; }
        private static string PlexBaseUrl { get; set; }
        private static string PlexToken { get; set; }
        private static string LibraryId { get; set; }
        private static string PlexMachineIdentifier { get; set; }

        static async Task Main(string[] args)
        {
            // Load configuration
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get settings from configuration
            PlexBaseUrl = Configuration["PlexSettings:BaseUrl"];
            PlexToken = Configuration["PlexSettings:Token"];
            LibraryId = Configuration["PlexSettings:LibraryId"];
            
            // Get Plex server machine identifier
            await GetPlexMachineIdentifier();

            bool continueRunning = true;
            ConsoleKeyInfo keyInfo;

            while (continueRunning)
            {
                Console.Clear();
                Console.WriteLine("Enter a genre you'd like to watch (or press Enter for any genre):");
                string preferredGenre = Console.ReadLine()?.Trim() ?? string.Empty;
                
                try
                {
                    var movie = await GetRandomMovie(preferredGenre);
                    
                    if (movie != null)
                    {
                        Console.WriteLine("\nYour random movie selection:");
                        Console.WriteLine($"Title: {movie.Title}");
                        Console.WriteLine($"Year: {movie.Year}");
                        Console.WriteLine($"Genre: {movie.Genre}");
                        Console.WriteLine($"Summary: {movie.Summary}");
                        Console.WriteLine($"Rating: {movie.Rating}");
                        Console.WriteLine($"Duration: {FormatDuration(movie.Duration)}");
                        Console.WriteLine($"Watched: {(movie.Watched ? "Yes" : "No")}");
                        Console.WriteLine("\nPlayback URL (paste into browser):");
                        Console.WriteLine(movie.PlaybackUrl);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(preferredGenre))
                        {
                            Console.WriteLine($"No movies found in the '{preferredGenre}' genre.");
                        }
                        else
                        {
                            Console.WriteLine("No movies found in your library.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("\nPress any key to select a different movie or press 'ESC' to quit...");
                keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    continueRunning = false;
                }
            }
        }

        static async Task GetPlexMachineIdentifier()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Plex-Token", PlexToken);

                var response = await client.GetAsync($"{PlexBaseUrl}/");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonDocumentOptions { AllowTrailingCommas = true };
                using (JsonDocument document = JsonDocument.Parse(content, options))
                {
                    var root = document.RootElement;
                    var mediaContainer = root.GetProperty("MediaContainer");

                    if (mediaContainer.TryGetProperty("machineIdentifier", out var machineIdentifier))
                    {
                        PlexMachineIdentifier = machineIdentifier.GetString();
                    }
                }
            }
        }

        static async Task<Movie> GetRandomMovie(string preferredGenre = "")
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Plex-Token", PlexToken);

                // Get all movies from the specified library
                var response = await client.GetAsync($"{PlexBaseUrl}/library/sections/{LibraryId}/all");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonDocumentOptions { AllowTrailingCommas = true };
                using (JsonDocument document = JsonDocument.Parse(content, options))
                {
                    var root = document.RootElement;
                    var mediaContainer = root.GetProperty("MediaContainer");
                    
                    if (mediaContainer.TryGetProperty("Metadata", out var metadata))
                    {
                        var movies = new List<Movie>();
                        foreach (var item in metadata.EnumerateArray())
                        {
                            // Extract genre information
                            string genreText = "Unknown";
                            var genres = new List<string>();
                            
                            if (item.TryGetProperty("Genre", out var genreArray))
                            {
                                foreach (var genre in genreArray.EnumerateArray())
                                {
                                    if (genre.TryGetProperty("tag", out var tag))
                                    {
                                        genres.Add(tag.GetString());
                                    }
                                }
                                if (genres.Any())
                                {
                                    genreText = string.Join(", ", genres);
                                }
                            }
                            
                            // Skip this movie if it doesn't match the preferred genre (if specified)
                            if (!string.IsNullOrEmpty(preferredGenre) && 
                                !genres.Any(g => g.Equals(preferredGenre, StringComparison.OrdinalIgnoreCase)))
                            {
                                continue;
                            }

                            // Extract the key/ratingKey for generating the playback URL
                            string ratingKey = "";
                            if (item.TryGetProperty("ratingKey", out var key))
                            {
                                ratingKey = key.GetString();
                            }

                            var movie = new Movie
                            {
                                Title = item.TryGetProperty("title", out var title) ? title.GetString() : "Unknown Title",
                                Year = item.TryGetProperty("year", out var year) ? year.GetInt32() : 0,
                                Summary = item.TryGetProperty("summary", out var summary) ? summary.GetString() : "No summary available",
                                Rating = item.TryGetProperty("rating", out var rating) ? rating.GetDouble() : 0.0,
                                Duration = item.TryGetProperty("duration", out var duration) ? duration.GetInt64() : 0,
                                Watched = item.TryGetProperty("viewCount", out var viewCount) ? viewCount.GetInt32() > 0 : false,
                                Genre = genreText,
                                PlaybackUrl = !string.IsNullOrEmpty(ratingKey) ? 
                                    $"{PlexBaseUrl}/web/index.html#!/server/{PlexMachineIdentifier}/details?key=%2Flibrary%2Fmetadata%2F{ratingKey}" : 
                                    "URL not available"
                            };
                            movies.Add(movie);
                        }

                        if (movies.Count > 0)
                        {
                            var random = new Random();
                            return movies[random.Next(movies.Count)];
                        }
                    }
                }
            }

            return null;
        }

        static string FormatDuration(long milliseconds)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(milliseconds);
            return $"{(int)t.TotalHours}h {t.Minutes}m";
        }
    }
}
