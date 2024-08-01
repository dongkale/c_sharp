using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileDownloader7;

public static class DownloadHelper
{
    public const int DEFAULT_FILE_PART_COUNT = 4;

    public static async Task DownloadFileAsync(FileDownloaderItem item, Action<Action> uiUpdater, int partCount = DEFAULT_FILE_PART_COUNT)
    {
        long totalFileSize = 0;
        long totalBytesReceived = item.TotalBytesReceived;

        if (File.Exists(item.DownloadPath))
        {
            totalBytesReceived = new FileInfo(item.DownloadPath).Length;

            string checksum = Utils.CalculateMD5FromFile(item.DownloadPath);

            Logger.Log($"[Checksum] {item.DownloadPath} - {checksum}"); ;
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Get the total file size
                using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, item.Url)))
                {
                    response.EnsureSuccessStatusCode();
                    totalFileSize = response.Content.Headers.ContentLength ?? 0;
                    item.TotalFileSize = totalFileSize;
                }

                if (totalBytesReceived >= totalFileSize)
                {
                    item.TotalBytesReceived = totalFileSize;
                    uiUpdater(() => item.UpdateProgress(item.TotalBytesReceived, item.TotalFileSize));
                    uiUpdater(() => item.UpdateStatus("다운로드 완료된 화일 입니다."));
                    return;
                }

                long partSize = totalFileSize / partCount;
                List<Task> downloadTasks = new List<Task>();
                for (int i = 0; i < partCount; i++)
                {
                    long start = i * partSize;
                    long end = (i == partCount - 1) ? totalFileSize - 1 : (start + partSize - 1);
                    downloadTasks.Add(DownloadPartAsync(item, start, end, i, uiUpdater));
                }

                await Task.WhenAll(downloadTasks);

                // Ensure parts are combined correctly
                CombineParts(item.DownloadPath, partCount);

                uiUpdater(() => item.UpdateStatus("다운로드 완료"));
            }
            catch (Exception ex)
            {
                uiUpdater(() => item.UpdateStatus("다운로드 오류: " + ex.Message));
            }
        }
    }

    private static async Task DownloadPartAsync(FileDownloaderItem item, long start, long end, int partIndex, Action<Action> uiUpdater)
    {
        string tempFilePath = $"{item.DownloadPath}.part{partIndex}";
        long existingLength = 0;

        if (File.Exists(tempFilePath))
        {
            existingLength = new FileInfo(tempFilePath).Length;
            if (existingLength >= (end - start + 1))
            {
                // Skip downloading if the part is already complete
                return;
            }
            start += existingLength;
        }

        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, item.Url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        lock (item)
                        {
                            item.TotalBytesReceived += bytesRead;
                            uiUpdater(() => item.UpdateProgress(item.TotalBytesReceived, item.TotalFileSize));
                        }
                    }
                }
            }
        }
    }

    private static void CombineParts(string downloadPath, int partCount)
    {
        using (FileStream output = new FileStream(downloadPath, FileMode.Create))
        {
            for (int i = 0; i < partCount; i++)
            {
                string tempFilePath = $"{downloadPath}.part{i}";
                if (File.Exists(tempFilePath))
                {
                    using (FileStream input = new FileStream(tempFilePath, FileMode.Open))
                    {
                        input.CopyTo(output);
                    }
                    File.Delete(tempFilePath);
                }
            }
        }
    }
}
