[System.Serializable]
public class TutorialStep
{
    public string title;
    public string description;

    public TutorialStep(string title, string description)
    {
        this.title = title;
        this.description = description;
    }
}