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
using DataAccess.RedditClient;

using RedditSharp.Things;

namespace DataAccess.Config
{
    public class DataAccessAutoMapperProfile : Profile
    {
        public DataAccessAutoMapperProfile()
        {
            //MonitoredPost document needs the Id field for mongodb collection.
            CreateMap<IRedditMonitoredPost, RedditMonitoredPostDocument>()
                .ForMember(
                    dest => dest.RedditId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => (string)null));
            ;
            CreateMap<IRedditPost, RedditMonitoredPostDocument>()
                .ForMember(
                    dest => dest.RedditId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => (string)null))
                ;
            CreateMap<Post, RedditFetchedPost>()
                .ForMember(dest => dest.FetchedAt,
                    opt => opt.MapFrom(src => src.FetchedAt.ToUniversalTime()))
                ;
            CreateMap<RedditMonitoredPostDocument, RedditMonitoredPostVersionDocument>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => (string)null))
                ;
        }
    }
}
