using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileDownloader7;

public static class DownloadHelper
{
    public const int DEFAULT_FILE_PART_COUNT = 4;

    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private static bool _isPaused = false;
    private static object _pauseLock = new object();

    public static async Task<bool> DownloadFileAsync(string apiKey, FileDownloaderItem item, Action<Action> uiUpdater, CancellationTokenSource cancellationToken, int partCount = DEFAULT_FILE_PART_COUNT)
    {
        long totalFileSize = 0;
        long totalBytesReceived = item.TotalBytesReceived;

        Logger.Log($"[DownloadFileAsync][TotalBytesReceived] {item.Url} - {item.TotalBytesReceived}");

        if (File.Exists(item.DownloadPath))
        {
            totalBytesReceived = new FileInfo(item.DownloadPath).Length;

            string checksum = Utils.CalculateMD5FromFile(item.DownloadPath);
            Logger.Log($"[DownloadFileAsync][Checksum] {item.DownloadPath} - {checksum}");
        }

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            try
            {
                // Get the total file size
                using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, item.Url)))
                {
                    response.EnsureSuccessStatusCode();
                    totalFileSize = response.Content.Headers.ContentLength ?? 0;
                    item.TotalFileSize = totalFileSize;

                    Logger.Log($"[DownloadFileAsync][TotalFileSize][{item.Url}] {item.TotalFileSize}");
                }

                // if (totalBytesReceived >= totalFileSize)
                // {
                //     item.TotalBytesReceived = totalFileSize;
                //     uiUpdater(() => item.UpdateProgress(item.TotalBytesReceived, item.TotalFileSize));
                //     uiUpdater(() => item.UpdateStatus("다운로드 완료된 파일입니다."));
                //     Logger.Log($"[DownloadFileAsync] {item.DownloadPath} - 다운로드 완료된 화일");
                //     return;
                // }

                if (totalBytesReceived >= totalFileSize)
                {
                    uiUpdater(() => item.UpdateStatus(UpdateStatus.AlreadyCompleted, "ALREADY DOWNLOAD"));
                    Logger.Log($"[DownloadFileAsync][{item.Url}] {item.DownloadPath} - 다운로드 완료된 화일");
                }

                // Adjust part count if file is too small
                if (totalFileSize < partCount)
                {
                    partCount = 1;
                }

                long partSize = totalFileSize / partCount;
                List<Task> downloadTasks = new List<Task>();
                for (int i = 0; i < partCount; i++)
                {
                    long start = i * partSize;
                    long end = (i == partCount - 1) ? totalFileSize - 1 : (start + partSize - 1);
                    // downloadTasks.Add(DownloadPartAsync(apiKey, item, start, end, i, uiUpdater, _cancellationTokenSource.Token));
                    downloadTasks.Add(DownloadPartAsync(apiKey, item, start, end, i, uiUpdater, cancellationToken.Token));
                }

                await Task.WhenAll(downloadTasks);

                CombineParts(item.DownloadPath, partCount);

                uiUpdater(() => item.UpdateStatus(UpdateStatus.Complete, "DOWNLOAD COMPLETE"));

                return true;
            }
            catch (Exception ex)
            {
                Logger.ErrorLog($"[DownloadFileAsync] {item.Url} {ex.Message}");
                uiUpdater(() => item.UpdateStatus(UpdateStatus.Error, "DOWNLOAD FAIL: " + ex.Message));

                DeletePartFiles(item.DownloadPath, partCount);
                return false;
            }
        }
    }

    private static async Task DownloadPartAsync(string apiKey, FileDownloaderItem item, long start, long end, int partIndex, Action<Action> uiUpdater, CancellationToken cancellationToken)
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

        Logger.Log($"[DownloadPartAsync][{item.Url}] file: {Path.GetFileName(tempFilePath)}, start: {start}, end: {end}, existingLength: {existingLength}");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, item.Url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);
            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                using (Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        // Pause logic
                        await CheckForPauseAsync();

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

    public static void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
    }

    // 모두 task 취소: 예외 발생해서 part 까지 다 지운다.(이어받기 안됨)
    public static void CancelDownloads(CancellationTokenSource[] items)
    {
        foreach (var item in items)
        {
            item.Cancel();
        }
    }

    private static async Task CheckForPauseAsync()
    {
        while (_isPaused)
        {
            await Task.Delay(100);
        }
    }

    public static void PauseDownload()
    {
        lock (_pauseLock)
        {
            _isPaused = true;
        }
    }

    public static void ResumeDownload()
    {
        lock (_pauseLock)
        {
            _isPaused = false;
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

                    Logger.Log($"[CombineParts][Delete] {Path.GetFileName(tempFilePath)}({downloadPath})");
                }
            }
        }
    }

    private static void DeletePartFiles(string downloadPath, int partCount)
    {
        for (int i = 0; i < partCount; i++)
        {
            string tempFilePath = $"{downloadPath}.part{i}";
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
                Logger.Log($"[DeletePartFiles][Delete] {Path.GetFileName(tempFilePath)}");
            }
        }
    }

    // public static void DeleteAllPartFiles__(string filePath, string fileName)
    // {
    //     // Directory.GetFiles(filepath, $"{filename}.part.*", SearchOption.TopDirectoryOnly).ToList().ForEach(File.Delete);
    //     foreach (string tempFilePath in Directory.EnumerateFiles(filePath, $"{fileName}.part*"))
    //     {
    //         if (File.Exists(tempFilePath))
    //         {
    //             File.Delete(tempFilePath);
    //             Logger.Log($"[DeleteAllPartFiles][Delete] {Path.GetFileName(tempFilePath)}");
    //         }
    //     }
    // }

    public static void DeleteAllPartFiles(string downloadPath)
    {
        string filepath = Path.GetDirectoryName(downloadPath) ?? String.Empty;
        if (String.IsNullOrEmpty(filepath))
        {
            return;
        }

        string filename = Path.GetFileName(downloadPath);

        foreach (string tempFilePath in Directory.EnumerateFiles(filepath, $"{filename}.part*"))
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
                Logger.Log($"[DeleteAllPartFiles][Delete] {Path.GetFileName(tempFilePath)}");
            }
        }
    }

    // Directory.GetFiles("f:\\TestData", "*.zip", SearchOption.TopDirectoryOnly).ToList().ForEach(File.Delete);

    public static async Task<bool> IsDownloadableAsync(string url, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            try
            {
                // HEAD 요청을 사용하여 파일이 존재하는지 확인
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);
                HttpResponseMessage response = await client.SendAsync(request);

                // 상태 코드가 200(OK)인 경우 다운로드 가능
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                // 예외 발생 시 로그를 남기고 다운로드 불가능으로 간주
                Logger.ErrorLog($"URL 확인 중 오류 발생: {ex.Message}");
                return false;
            }
        }
    }

    public static async Task<long> GetFileSizeAsync(string url, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            try
            {
                using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)))
                {
                    response.EnsureSuccessStatusCode();
                    return response.Content.Headers.ContentLength ?? 0;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorLog($"Error fetching file size for URL {url}: {ex.Message}");
                return -1;
            }
        }
    }

    public static async Task<(bool, string)> GetFileContentAsync(string url, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            try
            {
                // 파일 내용을 다운로드
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    return (true, content);
                }
            }
            catch (Exception ex)
            {
                // 오류가 발생하면 예외 메시지를 반환
                return (false, $"다운로드 오류: {ex.Message}");
            }
        }
    }

    public static void WriteToJsonFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
    {
        TextWriter? writer = null;

        try
        {
            var contentsToWriteToFile = JsonSerializer.Serialize(objectToWrite, new JsonSerializerOptions { WriteIndented = true });
            writer = new StreamWriter(filePath, append);
            writer.Write(contentsToWriteToFile);
        }
        catch (Exception ex)
        {
            Logger.ErrorLog($"[WriteToJsonFile] {filePath} - {ex.Message}");
        }
        finally
        {
            writer?.Close();
        }
    }

    public static T ReadFromJsonFile<T>(string filePath) where T : new()
    {
        if (!File.Exists(filePath))
        {
            return new T();
        }

        TextReader? reader = null;

        try
        {
            reader = new StreamReader(filePath);
            var fileContents = reader.ReadToEnd();
            var deserializedObject = JsonSerializer.Deserialize<T>(fileContents);
            return deserializedObject ?? new T();
        }
        catch (Exception ex)
        {
            Logger.ErrorLog($"[ReadFromJsonFile] {filePath} - {ex.Message}");
            return new T();
        }
        finally
        {
            reader?.Close();
        }
    }
}
