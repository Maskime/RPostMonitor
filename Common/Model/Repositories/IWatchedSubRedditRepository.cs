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

namespace Common.Model.Repositories
{
    public interface IWatchedSubRedditRepository
    {
        void UpdatedPollerStartedAt(string watchedSubReddit);

        TimeSpan UpdateSubRedditWatchedTime(string watchedSubReddit);

        TimeSpan GetSubRedditWatchedTime(string watchedSubReddit);
    }
}
