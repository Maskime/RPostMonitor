// ************************************************************************************************
// 
//  © 2019       General Electric Company
// 
//  Description  See class summary below.
// 
//  History      See source code control system.
// 
// ************************************************************************************************

namespace PostMonitor.Config
{
    public class CsvExportConfiguration
    {
        public const string ConfigKey = "CsvExport";
        
        /// <summary>
        /// How often should we export the content to a CSV file.
        /// </summary>
        public double ExportPeriodicityInHour { get; set; }
        /// <summary>
        /// Is the export feature enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        public string DestinationPath { get; set; }
    }
}
