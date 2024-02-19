namespace DLS_Backend.Components;

public class Story
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string BodyText { get; set; }
    public string ImgUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public Story() { }

    public Story(int id, string title, string bodyText, string imgUrl, DateTime createdAt)
    {
        Id = id;
        Title = title;
        BodyText = bodyText;
        ImgUrl = imgUrl;
        CreatedAt = createdAt;
    }

    public Story(string title, string bodyText, string imgUrl, DateTime createdAt)
    {
        Title = title;
        BodyText = bodyText;
        ImgUrl = imgUrl;
        CreatedAt = createdAt;
    }
}

