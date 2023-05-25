using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Paste the video url: ");

        string videoUrl = Console.ReadLine();
        Console.WriteLine("The video is currently being downloaded in the highest quality available. Please wait while the download completes.");

        var youtube = new YoutubeClient();
        var videoId = await youtube.Videos.GetAsync(videoUrl);

        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Id);
        var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
        var videoStream = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();


        if (audioStream != null && videoStream != null)
        {
            string pattern = "[^a-zA-Z0-9 ]";
            string title = Regex.Replace(videoId.Title, pattern, "");

            string audioFile = $"{title}.mp3";
            string videoFile = $"{title}.{videoStream.Container.Name}";

            await youtube.Videos.Streams.DownloadAsync(audioStream, audioFile);
            await youtube.Videos.Streams.DownloadAsync(videoStream, videoFile);

            string outputFile = $"{title}.mp4";
            string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloadsPath = Path.Combine(specialFolder, "Downloads");

            if (!Directory.Exists(downloadsPath))
                downloadsPath = Path.Combine(downloadsPath, "Pobrane");

            string destinationPath = Path.Combine(downloadsPath ?? specialFolder, outputFile);
            string ffmpegPath = "ffmpeg.exe";
            string arguments = $"-i \"{videoFile}\" -i \"{audioFile}\" -c:v copy -c:a copy \"{destinationPath}\"";

            using (var process = new Process())
            {
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                process.WaitForExit(1000);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Congratulations! Video downloaded to folder: {destinationPath}");
            Console.ResetColor();

            try
            {
                File.Delete(audioFile);
                File.Delete(videoFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        else
            Console.WriteLine("No audio or video streams available for the video.");
    }
}
