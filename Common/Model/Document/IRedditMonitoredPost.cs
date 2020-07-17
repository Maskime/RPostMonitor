using System;
using System.Collections.Generic;

using Common.Reddit;

namespace Common.Model.Document
{
    public interface IRedditMonitoredPost:IRedditPost
    {
        int IterationNumber { get; set; }
        TimeSpan InactivityAge { get; set; }
    }
}