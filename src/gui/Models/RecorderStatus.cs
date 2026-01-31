using System;
using Newtonsoft.Json;

namespace TikTokRecorderGui.Models
{
    /// <summary>
    /// Represents the status of a TikTok Live Recorder instance.
    /// Maps to the JSON structure in .tiktok_status/*.json files.
    /// </summary>
    public class RecorderStatus
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("pid")]
        public int Pid { get; set; }

        [JsonProperty("started_at")]
        public DateTime StartedAt { get; set; }

        [JsonProperty("last_heartbeat")]
        public DateTime LastHeartbeat { get; set; }

        [JsonProperty("current_file")]
        public string CurrentFile { get; set; }

        [JsonProperty("file_size_mb")]
        public double FileSizeMb { get; set; }

        [JsonProperty("output_path")]
        public string OutputPath { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("last_online")]
        public DateTime? LastOnline { get; set; }

        // Computed properties (added by StatusService)
        [JsonIgnore]
        public bool IsStale { get; set; }

        [JsonIgnore]
        public int AgeSeconds { get; set; }

        /// <summary>
        /// Gets a display-friendly state with color hint.
        /// </summary>
        [JsonIgnore]
        public string StateDisplay
        {
            get
            {
                if (IsStale) return "STALE";
                return State ?? "UNKNOWN";
            }
        }

        /// <summary>
        /// Gets duration since started as a friendly string.
        /// </summary>
        [JsonIgnore]
        public string UptimeDisplay
        {
            get
            {
                var uptime = DateTime.Now - StartedAt;
                if (uptime.TotalDays >= 1)
                    return $"{(int)uptime.TotalDays}d {uptime.Hours}h";
                if (uptime.TotalHours >= 1)
                    return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
                return $"{(int)uptime.TotalMinutes}m";
            }
        }

        /// <summary>
        /// Gets file size as formatted string.
        /// </summary>
        [JsonIgnore]
        public string FileSizeDisplay => FileSizeMb > 0 ? $"{FileSizeMb:F2} MB" : "-";

        /// <summary>
        /// Gets last heartbeat as relative time.
        /// </summary>
        [JsonIgnore]
        public string LastHeartbeatDisplay
        {
            get
            {
                var ago = DateTime.Now - LastHeartbeat;
                if (ago.TotalSeconds < 60)
                    return $"{(int)ago.TotalSeconds}s ago";
                if (ago.TotalMinutes < 60)
                    return $"{(int)ago.TotalMinutes}m ago";
                return $"{(int)ago.TotalHours}h ago";
            }
        }
    }
}
