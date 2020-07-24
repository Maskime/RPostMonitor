// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Common.Model.Document;

namespace Common.Model.Repositories
{
    public interface IWatchedSubRedditRepository
    {
        void UpdatedPollerStartedAt(string watchedSubReddit);

        TimeSpan UpdateSubRedditWatchedTime(string watchedSubReddit);

        TimeSpan GetSubRedditWatchedTime(string watchedSubReddit);

        Task<List<IWatchedSubReddit>> FindAllAsync();
    }
}
