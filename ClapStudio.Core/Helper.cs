using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace ClapStudio.Interface.Video
{
    public class Helper
    {
        public async Task<File> LoadFile(string videoFilePath)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "ffprobe",
                Arguments = $"-v quiet -print_format json -show_chapters -show_format \"{videoFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = startInfo };
            process.Start();
            string jsonOutput = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new InvalidOperationException($"ffprobe failed with exit code {process.ExitCode}.");

            List<Chapter> chapters = [];
            TimeSpan videoDuration = TimeSpan.Zero;

            using JsonDocument document = JsonDocument.Parse(jsonOutput);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("format", out JsonElement formatElement) &&
                formatElement.TryGetProperty("duration", out JsonElement durationElement) &&
                double.TryParse(durationElement.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double totalSeconds))
                videoDuration = TimeSpan.FromSeconds(totalSeconds);

            if (!root.TryGetProperty("chapters", out JsonElement chaptersElement) || chaptersElement.ValueKind != JsonValueKind.Array)
                return new(videoFilePath, Path.GetFileName(videoFilePath), videoDuration, chapters);

            foreach (JsonElement chapter in chaptersElement.EnumerateArray())
            {
                string title = string.Empty;
                if (chapter.TryGetProperty("tags", out JsonElement tagsElement) &&
                    tagsElement.TryGetProperty("title", out JsonElement titleElement))
                    title = titleElement.GetString() ?? string.Empty;

                if (chapter.TryGetProperty("start_time", out JsonElement startTimeElement) &&
                    double.TryParse(startTimeElement.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double startSeconds))
                    chapters.Add(new(TimeSpan.FromSeconds(startSeconds), title));
            }

            chapters.Sort((Chapter left, Chapter right) => left.Timestamp.CompareTo(right.Timestamp));
            return new(videoFilePath, Path.GetFileName(videoFilePath), videoDuration, chapters);
        }
    }
}
