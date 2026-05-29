public class User
{
    public string Name { get; set; }
    public string FavouriteTopic { get; set; }
    public int SessionQuestions { get; set; }
    public List<string> ConversationHistory { get; set; }

    public User(string name)
    {
        Name = name;
        FavouriteTopic = string.Empty;
        SessionQuestions = 0;
        ConversationHistory = new List<string>();
    }
}

