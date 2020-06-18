// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using Common.Config;

namespace PostPoller.Config
{
    public class RedditConfiguration:IRedditConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectURI { get; set; }

        public string Username { get; set; }

        public string UserPassword { get; set; }
    }
}
