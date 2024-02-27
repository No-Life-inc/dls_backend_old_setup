using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DLS_Backend.Components;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

namespace DLS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly string _connectionString;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
            _connectionString = "Server=localhost;Database=dls_backend;Trusted_Connection=True;TrustServerCertificate=True";
        }

        [HttpPost("register")]
        public IActionResult RegisterUser([FromBody] User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    byte[] salt = ConvertHexStringToByteArray(user.Salt);
                    byte[] hashedPassword = ConvertHexStringToByteArray(user.Password);

                    string insertQuery = "INSERT INTO Users (GUID, Email, Password, Salt, CreatedAt) VALUES (@GUID, @Email, @Password, @Salt, @CreatedAt)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@GUID", user.GUID);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Password", hashedPassword);
                        command.Parameters.AddWithValue("@Salt", salt);
                        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                        int rowsAffected = command.ExecuteNonQuery();
                        _logger.LogInformation($"Number of rows affected: {rowsAffected}");

                        return Ok("User registered successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user.");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private byte[] ConvertHexStringToByteArray(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }



        [HttpGet("uploadtestuser")]
        public async Task<IActionResult> UploadTestUser()
        {
            try
            {
                // Generate a random salt
                byte[] salt = GenerateSalt();

                // Hash the password with the salt
                string password = "testpassword";
                byte[] hashedPassword = HashPassword(password, salt);

                // Convert the hashed password and salt to hexadecimal strings
                string hashedPasswordHexString = BitConverter.ToString(hashedPassword).Replace("-", "");
                string saltHexString = BitConverter.ToString(salt).Replace("-", "");

                // Create the test user object
                User testUser = new User
                {
                    GUID = Guid.NewGuid().ToString(),
                    Email = "test@example.com",
                    Password = hashedPasswordHexString,
                    Salt = saltHexString,
                    CreatedAt = DateTime.Now
                };

                string jsonPayload = JsonSerializer.Serialize(testUser);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5159/api/users/");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("register", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok("Test user registered successfully!");
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, $"Error: {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading test user.");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return pbkdf2.GetBytes(32);
            }
        }


        [HttpGet("getusers")]
        public IActionResult GetUsers()
        {
            try
            {
                List<User> users = new List<User>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string selectQuery = "SELECT * FROM Users";

                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                byte[] passwordBytes = (byte[])reader["Password"];
                                string passwordString = Encoding.UTF8.GetString(passwordBytes);

                                byte[] saltBytes = (byte[])reader["Salt"];
                                string saltString = Encoding.UTF8.GetString(saltBytes);

                                User user = new User
                                {
                                    Id = (int)reader["Id"],
                                    GUID = reader["GUID"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = passwordString,
                                    Salt = saltString,
                                    CreatedAt = (DateTime)reader["CreatedAt"]
                                };
                                users.Add(user);
                            }
                        }
                    }
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users from the database.");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


    }
}
