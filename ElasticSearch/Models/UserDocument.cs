namespace ElasticSearch.Models;

public class UserDocument
{
    public required string UserId { get; set; }
    public required string UserName { get; set; }

    public required string TermId { get; set; }
    public required string Term { get; set; }
    public required string Language { get; set; }

    public required List<Translation> Translations { get; set; }

    public class Translation
    {
        public required string TargetLanguage { get; set; }
        public required string Text { get; set; }
    }

}



