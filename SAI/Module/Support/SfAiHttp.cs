using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFramework.SAI.Module.Support
{
    /// <summary>
    /// AI 模块 HTTP（含 SSE 流式）
    /// </summary>
    public static class SfAiHttp
    {
        static readonly HttpClient SharedClient = new HttpClient();

        public static async Task<string> GetJsonAsync(
            string url,
            Dictionary<string, string> headers,
            int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, timeoutSeconds)));

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyHeaders(request, headers);

            using var response = await SharedClient.SendAsync(request, cts.Token).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {text}");

            return text;
        }

        public static async Task<string> PostJsonAsync(
            string url,
            string jsonBody,
            Dictionary<string, string> headers,
            int timeoutSeconds,
            CancellationToken cancellationToken = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, timeoutSeconds)));

            using var request = CreatePostRequest(url, jsonBody, headers);
            using var response = await SharedClient.SendAsync(request, cts.Token).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {text}");

            return text;
        }

        /// <summary>
        /// 流式 POST：按行回调（SSE 的 data: 行 或 NDJSON 行）
        /// </summary>
        public static async Task PostJsonStreamLinesAsync(
            string url,
            string jsonBody,
            Dictionary<string, string> headers,
            int timeoutSeconds,
            Action<string> onLine,
            CancellationToken cancellationToken = default)
        {
            if (onLine == null) throw new ArgumentNullException(nameof(onLine));

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(30, timeoutSeconds)));

            using var request = CreatePostRequest(url, jsonBody, headers);
            using var response = await SharedClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception($"HTTP {(int)response.StatusCode}: {err}");
            }

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                cts.Token.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null) break;
                if (line.Length > 0)
                    onLine(line);
            }
        }

        static HttpRequestMessage CreatePostRequest(
            string url,
            string jsonBody,
            Dictionary<string, string> headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            ApplyHeaders(request, headers);
            return request;
        }

        static void ApplyHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers == null) return;

            foreach (var kv in headers)
                request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
        }
    }
}
