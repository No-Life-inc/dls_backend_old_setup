namespace DLS_Backend.Components;

public class User
{
    public int Id { get; set; }
    public string GUID { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User() { }

    public User(int id, string guid, string email, string password, string salt, DateTime createdAt)
    {
        Id = id;
        GUID = guid;
        Email = email;
        Password = password;
        Salt = salt;
        CreatedAt = createdAt;
    }

    public User(string guid, string email, string password, string salt, DateTime createdAt)
    {
        GUID = guid;
        Email = email;
        Password = password;
        Salt = salt;
        CreatedAt = createdAt;
    }
}

