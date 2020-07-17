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

using Common.Model.Document;
using Common.Reddit;

using PostMonitor.HostedServices;
using PostMonitor.Model;

namespace PostMonitor.Config
{
    public class PostMonitorAutoMapperProfile:Profile
    {
        public PostMonitorAutoMapperProfile()
        {
            CreateMap<IRedditPost, IRedditMonitoredPost>();
            CreateMap<IRedditMonitoredPost, CsvRow>();
        }
    }
}
