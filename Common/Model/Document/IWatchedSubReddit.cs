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

namespace Common.Model.Document
{
    public interface IWatchedSubReddit
    {
        string SubRedditName { get; set; }

        TimeSpan WatchedTime { get; set; }

        DateTime PollerStartedAtUtc { get; set; }

        DateTime LastTickAtUtc { get; set; }
    }
}
