using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Plex
{
    class Program
    {
        private static IConfiguration Configuration { get; set; }
        private static string PlexBaseUrl { get; set; }
        private static string PlexToken { get; set; }
        private static string LibraryId { get; set; }

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

            bool continueRunning = true;
            ConsoleKeyInfo keyInfo;

            while (continueRunning)
            {
                Console.Clear();
                try
                {
                    var movie = await GetRandomMovie();
                    
                    if (movie != null)
                    {
                        Console.WriteLine("\nYour random movie selection:");
                        Console.WriteLine($"Title: {movie.Title}");
                        Console.WriteLine($"Year: {movie.Year}");
                        Console.WriteLine($"Summary: {movie.Summary}");
                        Console.WriteLine($"Rating: {movie.Rating}");
                        Console.WriteLine($"Duration: {FormatDuration(movie.Duration)}");
                    }
                    else
                    {
                        Console.WriteLine("No movies found in your library.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine("\nPress any key to select a different movie or 'q' to quit...");
                keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Q)
                {
                    continueRunning = false;
                }
            }
        }

        static async Task<Movie> GetRandomMovie()
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
                            var movie = new Movie
                            {
                                Title = item.TryGetProperty("title", out var title) ? title.GetString() : "Unknown Title",
                                Year = item.TryGetProperty("year", out var year) ? year.GetInt32() : 0,
                                Summary = item.TryGetProperty("summary", out var summary) ? summary.GetString() : "No summary available",
                                Rating = item.TryGetProperty("rating", out var rating) ? rating.GetDouble() : 0.0,
                                Duration = item.TryGetProperty("duration", out var duration) ? duration.GetInt64() : 0
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
