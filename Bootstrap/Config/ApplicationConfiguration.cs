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
    public class ApplicationConfiguration:IApplicationConfiguration
    {
        public string Name { get; set; }
        public IRedditConfiguration Reddit { get; set; }
        
        public string DownloadDir { get; set; }
    }
}
