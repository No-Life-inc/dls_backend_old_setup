using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Data.SqlClient;
using DLS_Backend.Components;
using System.Text.Json;
using System.Text;

namespace DLS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly ILogger<StoriesController> _logger;
        private readonly string _connectionString;

        public StoriesController(ILogger<StoriesController> logger)
        {
            _logger = logger;
            _connectionString = "Server=localhost;Database=dls_backend;Trusted_Connection=True;TrustServerCertificate=True";
        }

        [HttpGet("testconnection")]
        public IActionResult TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection established successfully.");
                    return Ok("Connection successful!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while establishing database connection.");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("uploadstory")]
        public IActionResult UploadStory([FromBody] Story story)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO Stories (Title, BodyText, ImgUrl, CreatedAt) VALUES (@Title, @BodyText, @ImgUrl, @CreatedAt)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Title", story.Title);
                        command.Parameters.AddWithValue("@BodyText", story.BodyText);
                        command.Parameters.AddWithValue("@ImgUrl", story.ImgUrl);
                        command.Parameters.AddWithValue("@CreatedAt", story.CreatedAt);

                        int rowsAffected = command.ExecuteNonQuery();
                        _logger.LogInformation($"Number of rows affected: {rowsAffected}");

                        return Ok("Story uploaded successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading story to database.");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("uploadteststory")]
        public async Task<IActionResult> UploadTestStory()
        {
            try
            {
                Story testStory = new Story
                {
                    Title = "Test Story",
                    BodyText = "This is a test story.",
                    ImgUrl = "https://example.com/test.jpg",
                    CreatedAt = DateTime.Now
                };

                string jsonPayload = JsonSerializer.Serialize(testStory);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5159/api/stories/");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("uploadstory", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok("Test story uploaded successfully!");
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, $"Error: {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading test story.");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("getstories")]
        public IActionResult GetStories()
        {
            try
            {
                List<Story> stories = new List<Story>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string selectQuery = "SELECT * FROM Stories";

                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Story story = new Story
                                {
                                    Id = (int)reader["Id"],
                                    Title = reader["Title"].ToString(),
                                    BodyText = reader["BodyText"].ToString(),
                                    ImgUrl = reader["ImgUrl"].ToString(),
                                    CreatedAt = (DateTime)reader["CreatedAt"]
                                };
                                stories.Add(story);
                            }
                        }
                    }
                }

                return Ok(stories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving stories from the database.");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
