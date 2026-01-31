using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TikTokRecorderGui.Models;

namespace TikTokRecorderGui.Services
{
    /// <summary>
    /// Service to read recorder status files from the .tiktok_status directory.
    /// </summary>
    public class StatusService
    {
        private readonly string _statusDirectory;
        private const int STALE_THRESHOLD_SECONDS = 60;

        public StatusService(string statusDirectory)
        {
            _statusDirectory = statusDirectory;
        }

        /// <summary>
        /// Reads all status files and returns a list of RecorderStatus objects.
        /// </summary>
        public List<RecorderStatus> GetAllStatuses()
        {
            var statuses = new List<RecorderStatus>();

            if (!Directory.Exists(_statusDirectory))
            {
                return statuses;
            }

            foreach (var file in Directory.GetFiles(_statusDirectory, "*.json"))
            {
                // Skip temp files
                if (file.EndsWith(".tmp"))
                    continue;

                try
                {
                    var json = ReadFileWithRetry(file);
                    if (string.IsNullOrEmpty(json))
                        continue;

                    var status = JsonConvert.DeserializeObject<RecorderStatus>(json);
                    if (status != null)
                    {
                        // Calculate staleness
                        var age = (DateTime.Now - status.LastHeartbeat).TotalSeconds;
                        status.AgeSeconds = (int)age;
                        status.IsStale = age > STALE_THRESHOLD_SECONDS;

                        statuses.Add(status);
                    }
                }
                catch (Exception)
                {
                    // Skip corrupted or locked files
                }
            }

            // Sort by username
            return statuses.OrderBy(s => s.Username).ToList();
        }

        /// <summary>
        /// Gets only the active (non-stale) statuses.
        /// </summary>
        public List<RecorderStatus> GetActiveStatuses()
        {
            return GetAllStatuses().Where(s => !s.IsStale).ToList();
        }

        /// <summary>
        /// Gets statuses where the recorder is currently recording.
        /// </summary>
        public List<RecorderStatus> GetRecordingStatuses()
        {
            return GetActiveStatuses()
                .Where(s => s.State == "RECORDING")
                .ToList();
        }

        /// <summary>
        /// Gets statuses where the recorder is waiting for the user to go live.
        /// </summary>
        public List<RecorderStatus> GetWaitingStatuses()
        {
            return GetActiveStatuses()
                .Where(s => s.State == "WAITING")
                .ToList();
        }

        /// <summary>
        /// Reads a file with retry logic for Windows file locking.
        /// </summary>
        private string ReadFileWithRetry(string path, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs))
                    {
                        return sr.ReadToEnd();
                    }
                }
                catch (IOException)
                {
                    if (i < maxRetries - 1)
                        System.Threading.Thread.Sleep(50);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets summary statistics for display.
        /// </summary>
        public StatusSummary GetSummary()
        {
            var all = GetAllStatuses();
            var active = all.Where(s => !s.IsStale).ToList();

            return new StatusSummary
            {
                TotalInstances = all.Count,
                ActiveInstances = active.Count,
                RecordingCount = active.Count(s => s.State == "RECORDING"),
                WaitingCount = active.Count(s => s.State == "WAITING"),
                TotalFileSizeMb = active.Sum(s => s.FileSizeMb)
            };
        }
    }

    /// <summary>
    /// Summary statistics for recorder instances.
    /// </summary>
    public class StatusSummary
    {
        public int TotalInstances { get; set; }
        public int ActiveInstances { get; set; }
        public int RecordingCount { get; set; }
        public int WaitingCount { get; set; }
        public double TotalFileSizeMb { get; set; }

        public string SummaryText => $"Active: {ActiveInstances} | Recording: {RecordingCount} | Waiting: {WaitingCount} | Total Size: {TotalFileSizeMb:F2} MB";
    }
}
