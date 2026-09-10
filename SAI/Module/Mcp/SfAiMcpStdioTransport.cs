using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SFramework.SAI.Module.Mcp
{
    /// <summary>MCP stdio 传输（JSON-RPC 行协议）</summary>
    public sealed class SfAiMcpStdioTransport : IDisposable
    {
        Process _process;
        StreamWriter _stdin;
        readonly object _writeLock = new object();
        readonly StringBuilder _stdoutBuffer = new StringBuilder();
        readonly object _stdoutLock = new object();

        public bool IsRunning => _process != null && !_process.HasExited;

        public void Start(SfAiMcpServerConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.Command))
                throw new ArgumentException("MCP Command 不能为空");

            Stop();

            var psi = new ProcessStartInfo
            {
                FileName = config.Command,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            if (config.Args != null && config.Args.Length > 0)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < config.Args.Length; i++)
                {
                    if (i > 0) sb.Append(' ');
                    sb.Append(QuoteArg(config.Args[i]));
                }

                psi.Arguments = sb.ToString();
            }

            _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (_stdoutLock)
                {
                    _stdoutBuffer.AppendLine(e.Data);
                }
            };
            _process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Debug.LogWarning($"[SfAiMcp][{config.Name}] {e.Data}");
            };

            if (!_process.Start())
                throw new Exception($"无法启动 MCP 进程: {config.Command}");

            _stdin = new StreamWriter(_process.StandardInput.BaseStream, new UTF8Encoding(false))
            {
                AutoFlush = true
            };
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public async Task WriteLineAsync(string line, CancellationToken cancellationToken)
        {
            if (_stdin == null) throw new InvalidOperationException("MCP 未启动");
            lock (_writeLock)
            {
                _stdin.WriteLine(line);
            }

            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
        }

        public async Task<string> ReadLineAsync(CancellationToken cancellationToken, int timeoutMs = 30000)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string line = null;
                lock (_stdoutLock)
                {
                    var text = _stdoutBuffer.ToString();
                    var idx = text.IndexOf('\n');
                    if (idx >= 0)
                    {
                        line = text.Substring(0, idx).TrimEnd('\r');
                        _stdoutBuffer.Clear();
                        _stdoutBuffer.Append(text.Substring(idx + 1));
                    }
                }

                if (line != null)
                    return line;

                await Task.Delay(20, cancellationToken).ConfigureAwait(false);
            }

            throw new TimeoutException("MCP 读取超时");
        }

        public void Stop()
        {
            try
            {
                _stdin?.Dispose();
                _stdin = null;
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process.WaitForExit(2000);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SfAiMcp] 停止进程失败: {e.Message}");
            }
            finally
            {
                _process?.Dispose();
                _process = null;
                lock (_stdoutLock) _stdoutBuffer.Clear();
            }
        }

        public void Dispose() => Stop();

        static string QuoteArg(string arg)
        {
            if (string.IsNullOrEmpty(arg)) return "\"\"";
            if (arg.IndexOfAny(new[] { ' ', '\t', '"' }) < 0) return arg;
            return "\"" + arg.Replace("\"", "\\\"") + "\"";
        }
    }
}
