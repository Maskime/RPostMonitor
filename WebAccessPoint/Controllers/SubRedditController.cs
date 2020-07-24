// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;

using Common.Model.Document;
using Common.Model.Repositories;

using DataAccess;

using Microsoft.AspNetCore.Mvc;

namespace WebAccessPoint.Controllers
{
    [Route("api/{controller}")]
    public class SubRedditController: ControllerBase
    {
        private IMapper _mapper;
        private IWatchedSubRedditRepository _watchedSubRepo;

        public SubRedditController(
            IWatchedSubRedditRepository watchedSubredditRepositories
            , IMapper mapper
            )
        {
            _watchedSubRepo = watchedSubredditRepositories;
            _mapper = mapper;
        }

        public async Task<List<IWatchedSubReddit>> List()
        {
            return await _watchedSubRepo.FindAllAsync();
        }
    }
}
