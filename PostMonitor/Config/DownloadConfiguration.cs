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

namespace PostMonitor.Config
{
    public class DownloadConfiguration
    {
        public List<string> DownloadMedia { get; set; }
        public string Location { get; set; }
    }
}
