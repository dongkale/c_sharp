#pragma warning disable CS8600

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileDownloader7;

public static class Utils
{
    public static string CalculateMD5FromFile(string filename)
    {
        try
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        catch (FileNotFoundException ex)
        {
            // 파일이 존재하지 않을 때
            Logger.ErrorLog($"파일을 찾을 수 없습니다: {filename}. 오류: {ex.Message}");
            return string.Empty;
        }
        catch (IOException ex)
        {
            // 파일을 읽을 수 없을 때
            Logger.ErrorLog($"파일을 읽는 중 오류가 발생했습니다: {filename}. 오류: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            // 기타 예외 처리
            Logger.ErrorLog($"예상치 못한 오류가 발생했습니다: {filename}. 오류: {ex.Message}");
            return string.Empty;
        }
    }

    // public static string CalculateMD5fromString1(string theString)
    // {
    //     string hash;
    //     using (MD5 md5 = MD5.Create())
    //     {
    //         hash = BitConverter.ToString(
    //           md5.ComputeHash(Encoding.UTF8.GetBytes(theString))
    //         ).Replace("-", String.Empty);
    //     }

    //     return hash;
    // }

    public static string CalculateMD5FromString(string theString)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(theString);
            var hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public static async Task<string> CalculateMD5FromUrlAsync(string url)
    {
        string tempFilePath = Path.GetTempFileName();

        using (HttpClient client = new HttpClient())
        {
            try
            {
                // 다운로드 및 임시 파일에 저장
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream inputStream = await response.Content.ReadAsStreamAsync())
                    using (Stream outputStream = File.Create(tempFilePath))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                }

                // MD5 체크섬 계산
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(tempFilePath))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to calculate MD5 checksum from URL.", ex);
            }
            finally
            {
                // 임시 파일 삭제
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
    }

    public static async Task<(bool, string)> GetFileContentAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
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

    public static async Task<(bool, string)> PostAsync(string url, string jsonContent, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            try
            {
                HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                // if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                // {
                //     throw new UnauthorizedAccessException("Invalid API key.");
                // }
                // if (response.StatusCode != System.Net.HttpStatusCode.OK)
                // {
                //     throw new UnauthorizedAccessException("Invalid API key.");
                // }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
                }

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return (true, responseBody);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                // throw new InvalidOperationException("Failed to send data to the API.", ex);                
                Console.WriteLine($"Error: {ex.Message}");
                return (false, ex.Message);
            }
        }
    }

    public static async Task<(bool, string)> GetAsync(string url, string apiKey)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
                }

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return (true, responseBody);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return (false, ex.Message);
            }
        }
    }

    // public static async Task<string> __PostAsync(string url, string jsonContent, string apiKey)
    // {
    //     using (HttpClient client = new HttpClient())
    //     {
    //         try
    //         {
    //             client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    //             HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
    //             HttpResponseMessage response = await client.PostAsync(url, content);
    //             response.EnsureSuccessStatusCode();
    //             string responseBody = await response.Content.ReadAsStringAsync();
    //             return responseBody;
    //         }
    //         catch (Exception ex)
    //         {
    //             throw new InvalidOperationException("Failed to send data to the API.", ex);
    //         }
    //     }
    // }


    // public static async Task<string> __GetAsync(string url, string apiKey)
    // {
    //     using (HttpClient client = new HttpClient())
    //     {
    //         try
    //         {
    //             client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    //             HttpResponseMessage response = await client.GetAsync(url);
    //             response.EnsureSuccessStatusCode();
    //             string responseBody = await response.Content.ReadAsStringAsync();
    //             return responseBody;
    //         }
    //         catch (Exception ex)
    //         {
    //             throw new InvalidOperationException("Failed to fetch data from the API.", ex);
    //         }
    //     }
    // }

    // public class ResponseData<T>
    // {
    //     public int resultCode { get; set; } = 0;
    //     public string resultMessage { get; set; } = string.Empty;
    //     public List<T> resultData { get; set; } = [];
    // }

    // public static async Task<(bool, string, List<T>?)> CallApi<T>(string url, string jsonContent, string apiKey)
    // {
    //     (bool result, string response) = await Utils.PostAsync(url, jsonContent, apiKey);
    //     if (!result)
    //     {
    //         return (false, response, default);
    //     }

    //     if (string.IsNullOrEmpty(response))
    //     {
    //         return (false, "Empty response from the API.", default);
    //     }

    //     ResponseData<T> deserializeResult = JsonSerializer.Deserialize<ResponseData<T>>(response);
    //     if (deserializeResult == null)
    //     {
    //         return (false, "Deserialize Fail.", default);
    //     }

    //     if (deserializeResult.resultCode != 0)
    //     {
    //         return (false, deserializeResult.resultMessage, default);
    //     }

    //     return (true, "Suceess", deserializeResult.resultData);
    // }

    public class ResponseData<T>
    {
        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; } = 0;
        [JsonPropertyName("resultMessage")]
        public string ResultMessage { get; set; } = string.Empty;
        [JsonPropertyName("resultData")]
        public T ResultData { get; set; } = default!;
    }

    public static async Task<(bool, string, T?)> CallApi<T>(string url, string jsonContent, string apiKey)
    {
        (bool result, string response) = await Utils.PostAsync(url, jsonContent, apiKey);
        if (!result)
        {
            return (false, response, default);
        }

        if (string.IsNullOrEmpty(response))
        {
            return (false, "Empty response from the API.", default);
        }

        ResponseData<T> deserializeResult = JsonSerializer.Deserialize<ResponseData<T>>(response);
        if (deserializeResult == null)
        {
            return (false, "Deserialize Fail.", default);
        }

        if (deserializeResult.ResultCode != 0)
        {
            return (false, deserializeResult.ResultMessage, default);
        }

        return (true, "Suceess", deserializeResult.ResultData);
    }

    /// <summary>
    /// 지정한 URL이 다운로드 가능한지 확인합니다.
    /// </summary>
    /// <param name="url">확인할 URL</param>
    /// <returns>다운로드 가능 여부</returns>
    public static async Task<bool> IsDownloadableAsync(string url)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                // HEAD 요청을 사용하여 파일이 존재하는지 확인
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);
                HttpResponseMessage response = await client.SendAsync(request);

                // 상태 코드가 200(OK)인 경우 다운로드 가능
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }
        catch (Exception ex)
        {
            // 예외 발생 시 로그를 남기고 다운로드 불가능으로 간주
            Logger.ErrorLog($"URL 확인 중 오류 발생: {ex.Message}");
            return false;
        }
    }

}