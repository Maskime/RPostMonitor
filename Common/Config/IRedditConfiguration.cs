namespace Common.Config
{
    public interface IRedditConfiguration
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string RedirectURI { get; set; }
        string Username { get; set; }
        string UserPassword { get; set; }
    }
}