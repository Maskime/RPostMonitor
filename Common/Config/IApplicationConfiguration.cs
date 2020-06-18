namespace Common.Config
{
    public interface IApplicationConfiguration
    {
        string Name { get; set; }
        IRedditConfiguration Reddit { get; set; }
        string DownloadDir { get; set; }
    }
}