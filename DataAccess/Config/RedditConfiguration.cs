// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

namespace DataAccess.Config
{
    public class RedditConfiguration
    {
        public const string ConfigKey = "Reddit";

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectURI { get; set; }

        public string Username { get; set; }

        public string UserPassword { get; set; }
        public int MaxRetry { get; set; }
        public double TimeBetweenRetrySequenceInSeconds { get; set; }
        public double TimeBetweenRetryAttemptInSeconds { get; set; }
    }
}
