using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TikTokRecorderGui.Services
{
    /// <summary>
    /// Service to interact with TikTok API for live stream information.
    /// Uses WebClient for .NET 4.0 compatibility.
    /// </summary>
    public class TikTokApiService : IDisposable
    {
        private const string TIKREC_API = "https://tikrec.com";
        private const string TIKTOK_BASE = "https://www.tiktok.com";

        public TikTokApiService()
        {
            // Enable TLS 1.2 for HTTPS connections (3072 = Tls12, required for modern HTTPS)
            // .NET 4.0 doesn't have Tls12 enum, so we use the numeric value
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
        }

        /// <summary>
        /// Creates a configured WebClient with proper headers.
        /// </summary>
        private WebClient CreateClient()
        {
            var client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = 
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            client.Headers[HttpRequestHeader.Referer] = "https://www.tiktok.com/";
            client.Headers[HttpRequestHeader.Accept] = "application/json, text/plain, */*";
            client.Encoding = Encoding.UTF8;
            return client;
        }

        /// <summary>
        /// Checks if a user is currently live (synchronous).
        /// </summary>
        public LiveInfo GetLiveInfo(string username)
        {
            Console.WriteLine("[TikTokApi] GetLiveInfo: Fetching info for " + username);
            try
            {
                // First get room ID via TikRec API
                var roomId = GetRoomId(username);
                if (string.IsNullOrEmpty(roomId))
                {
                    Console.WriteLine("[TikTokApi] GetLiveInfo: No room ID for " + username);
                    return new LiveInfo { Username = username, IsLive = false };
                }

                Console.WriteLine("[TikTokApi] GetLiveInfo: Got room ID " + roomId + " for " + username);
                
                // Get room info with stream details
                using (var client = CreateClient())
                {
                    var roomInfoUrl = "https://webcast.tiktok.com/webcast/room/info/?aid=1988&room_id=" + roomId;
                    var response = client.DownloadString(roomInfoUrl);
                    var json = JObject.Parse(response);

                    var data = json["data"];
                    if (data == null)
                    {
                        Console.WriteLine("[TikTokApi] GetLiveInfo: No data in response for " + username);
                        return new LiveInfo { Username = username, IsLive = false };
                    }

                    var status = data["status"] != null ? data["status"].Value<int>() : 0;
                    var isLive = status == 2; // 2 = Live, 4 = Offline
                    Console.WriteLine("[TikTokApi] GetLiveInfo: " + username + " status=" + status + ", isLive=" + isLive);

                    var liveInfo = new LiveInfo
                    {
                        Username = username,
                        IsLive = isLive,
                        RoomId = roomId,
                        Title = data["title"] != null ? data["title"].Value<string>() : null,
                        ViewerCount = data["user_count"] != null ? data["user_count"].Value<int>() : 0
                    };

                    // Try to get owner's avatar image for non-live users
                    if (data["owner"] != null && data["owner"]["avatar_thumb"] != null)
                    {
                        var avatarThumb = data["owner"]["avatar_thumb"];
                        if (avatarThumb["url_list"] != null && avatarThumb["url_list"].HasValues)
                        {
                            liveInfo.AvatarUrl = avatarThumb["url_list"][0].Value<string>();
                            Console.WriteLine("[TikTokApi] GetLiveInfo: Got avatar URL for " + username + ": " + liveInfo.AvatarUrl);
                        }
                    }

                    // Try to get cover/thumbnail image
                    if (isLive)
                    {
                        var cover = data["cover"];
                        if (cover != null && cover["url_list"] != null && cover["url_list"].HasValues)
                        {
                            liveInfo.ThumbnailUrl = cover["url_list"][0].Value<string>();
                            Console.WriteLine("[TikTokApi] GetLiveInfo: Got cover URL for " + username + ": " + liveInfo.ThumbnailUrl);
                        }
                    }

                    // Use avatar as fallback thumbnail if no cover available
                    if (string.IsNullOrEmpty(liveInfo.ThumbnailUrl) && !string.IsNullOrEmpty(liveInfo.AvatarUrl))
                    {
                        liveInfo.ThumbnailUrl = liveInfo.AvatarUrl;
                        Console.WriteLine("[TikTokApi] GetLiveInfo: Using avatar as thumbnail for " + username);
                    }

                    if (string.IsNullOrEmpty(liveInfo.ThumbnailUrl))
                    {
                        Console.WriteLine("[TikTokApi] GetLiveInfo: No thumbnail URL available for " + username);
                    }

                    return liveInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[TikTokApi] GetLiveInfo ERROR for " + username + ": " + ex.Message);
                return new LiveInfo { Username = username, IsLive = false };
            }
        }

        /// <summary>
        /// Wrapper for backwards compatibility.
        /// </summary>
        public System.Threading.Tasks.Task<LiveInfo> GetLiveInfoAsync(string username)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<LiveInfo>();
            tcs.SetResult(GetLiveInfo(username));
            return tcs.Task;
        }

        /// <summary>
        /// Gets the room ID for a username via TikRec API.
        /// </summary>
        private string GetRoomId(string username)
        {
            try
            {
                using (var client = CreateClient())
                {
                    // Get signed URL from TikRec
                    var signUrl = TIKREC_API + "/tiktok/room/api/sign?unique_id=" + username;
                    var signResponse = client.DownloadString(signUrl);
                    var signJson = JObject.Parse(signResponse);
                    var signedPath = signJson["signed_path"] != null ? signJson["signed_path"].Value<string>() : null;

                    if (string.IsNullOrEmpty(signedPath))
                        return null;

                    // Get room info using signed path
                    var roomUrl = TIKTOK_BASE + signedPath;
                    var roomResponse = client.DownloadString(roomUrl);
                    
                    if (roomResponse.Contains("Please wait"))
                        return null;

                    var roomJson = JObject.Parse(roomResponse);
                    var data = roomJson["data"];
                    if (data != null && data["user"] != null && data["user"]["roomId"] != null)
                    {
                        return data["user"]["roomId"].Value<string>();
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Downloads a thumbnail image from URL.
        /// </summary>
        public Image GetThumbnail(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("[TikTokApi] GetThumbnail: URL is null or empty");
                return null;
            }

            Console.WriteLine("[TikTokApi] GetThumbnail: Downloading from " + url);

            try
            {
                using (var client = CreateClient())
                {
                    var bytes = client.DownloadData(url);
                    Console.WriteLine("[TikTokApi] GetThumbnail: Downloaded " + bytes.Length + " bytes");
                    
                    using (var ms = new MemoryStream(bytes))
                    {
                        // Create a copy of the image so the stream can be disposed
                        using (var original = Image.FromStream(ms))
                        {
                            Console.WriteLine("[TikTokApi] GetThumbnail: Image loaded successfully, size " + original.Width + "x" + original.Height);
                            return new Bitmap(original);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[TikTokApi] GetThumbnail ERROR: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Wrapper for backwards compatibility.
        /// </summary>
        public System.Threading.Tasks.Task<Image> GetThumbnailAsync(string url)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<Image>();
            tcs.SetResult(GetThumbnail(url));
            return tcs.Task;
        }

        public void Dispose()
        {
            // Nothing to dispose with WebClient pattern (created per-use)
        }
    }

    /// <summary>
    /// Information about a live stream.
    /// </summary>
    public class LiveInfo
    {
        public string Username { get; set; }
        public bool IsLive { get; set; }
        public string RoomId { get; set; }
        public string Title { get; set; }
        public string AvatarUrl { get; set; }
        public int ViewerCount { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
