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

using DataAccess.Documents;

using RedditSharp.Things;

namespace DataAccess.Config
{
    public class DataAccessAutoMapperProfile:Profile
    {
        public DataAccessAutoMapperProfile()
        {
            //MonitoredPost document needs the Id field for mongodb collection.
            CreateMap<IMonitoredPost, MonitoredPost>()
                .ForMember(
                    dest => dest.RedditId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, 
                    opt => opt.MapFrom(src => (string)null));
                ;
            CreateMap<MonitoredPost, IMonitoredPost>()
                .ForMember(dest => dest.Id, 
                    opt => opt.MapFrom(src => src.RedditId));
            CreateMap<Post, IRedditPost>();
        }
    }
}
