// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using AutoMapper;

using Common.Reddit;

using PostMonitor.Poller;

namespace PostMonitor.Config
{
    public class PostMonitorAutoMapperProfile:Profile
    {
        public PostMonitorAutoMapperProfile()
        {
            CreateMap<IRedditPost, MonitoredPost>();
        }
    }
}
