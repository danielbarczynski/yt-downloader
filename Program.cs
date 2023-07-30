using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using yt_downloader;

public class Program
{
  public static async Task Main(string[] args)
  {
    Console.WriteLine("Paste the video url: ");
    string videoUrl = Console.ReadLine();
    if (!videoUrl.Contains("youtu.be") && !videoUrl.Contains("youtube.com"))
    {
      Console.WriteLine("Video not found");
      return;
    }

    var youtube = new YoutubeClient();
    var videoId = await youtube.Videos.GetAsync(videoUrl);
    Console.WriteLine("Do you want to download audio (a) or video (v)?");
    string choice = Console.ReadLine();
    if (choice == "a" || choice == "A")
    {
      var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Id);
      var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
      if (audioStream != null)
      {
        string pattern = "[^a-zA-Z0-9 ]";
        string title = Regex.Replace(videoId.Title, pattern, "");
        string audioFile = $"{title}.mp3";

        ProgressBar audioProgress;
        Console.Write($"\nDownloading audio stream\n");
        using (audioProgress = new ProgressBar())
        {
          for (int i = 0; i <= 100; i++)
          {
            audioProgress.Report((double)i / 100);
            Thread.Sleep(20);
          }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Done\n");
        Console.ResetColor();

        await youtube.Videos.Streams.DownloadAsync(audioStream, audioFile, audioProgress);

        string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloadsPath = Path.Combine(specialFolder, "Downloads");

        if (!Directory.Exists(downloadsPath))
          downloadsPath = Path.Combine(downloadsPath, "Pobrane");

        string outputPath = "D:\\source\\repos\\yt-downloader\\bin\\Debug\\net7.0\\" + audioFile;
        string destinationPath = Path.Combine(downloadsPath ?? specialFolder, audioFile);
        File.Move(outputPath, destinationPath);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nCongratulations! Video has been downloaded and moved to folder: {destinationPath}");
        Console.ResetColor();

        try
        {
          File.Delete(audioFile);
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }

      else
        Console.WriteLine("No audio streams available for the video.");
    }

    else if (choice == "v" || choice == "V")
    {
      var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Id);
      var audioStream = streamManifest.GetAudioStreams().GetWithHighestBitrate();
      var videoStream = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();

      if (audioStream != null && videoStream != null)
      {
        string pattern = "[^a-zA-Z0-9 ]";
        string title = Regex.Replace(videoId.Title, pattern, "");

        string audioFile = $"{title}.mp3";
        string videoFile = $"{title}.{videoStream.Container.Name}";

        ProgressBar audioProgress;
        Console.Write($"\nDownloading audio stream\n");
        using (audioProgress = new ProgressBar())
        {
          for (int i = 0; i <= 100; i++)
          {
            audioProgress.Report((double)i / 100);
            Thread.Sleep(20);
          }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Done\n");
        Console.ResetColor();

        ProgressBar videoProgress;
        Console.Write($"Downloading video stream\n");
        using (videoProgress = new ProgressBar())
        {
          for (int i = 0; i <= 100; i++)
          {
            videoProgress.Report((double)i / 100);
            Thread.Sleep(20);
          }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Done\n");
        Console.ResetColor();

        await youtube.Videos.Streams.DownloadAsync(audioStream, audioFile, audioProgress);
        await youtube.Videos.Streams.DownloadAsync(videoStream, videoFile, videoProgress);

        string outputFile = $"{title}.mp4";
        string specialFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloadsPath = Path.Combine(specialFolder, "Downloads");

        if (!Directory.Exists(downloadsPath))
          downloadsPath = Path.Combine(downloadsPath, "Pobrane");

        string destinationPath = Path.Combine(downloadsPath ?? specialFolder, outputFile);
        string ffmpegPath = "ffmpeg.exe";
        string arguments = $"-i \"{videoFile}\" -i \"{audioFile}\" -c:v copy -c:a copy \"{destinationPath}\"";

        Console.WriteLine("Merging outputs into mp4 file...");
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
        Console.WriteLine($"\nCongratulations! Video has been downloaded and moved to folder: {destinationPath}");
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
}
