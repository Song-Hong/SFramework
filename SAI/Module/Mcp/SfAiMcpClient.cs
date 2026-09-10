using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;

namespace SFramework.SAI.Module.Mcp
{
    /// <summary>MCP Client：initialize → tools/list → tools/call</summary>
    public sealed class SfAiMcpClient : IDisposable
    {
        readonly SfAiMcpServerConfig _config;
        readonly SfAiMcpStdioTransport _transport = new SfAiMcpStdioTransport();
        int _nextId = 1;
        bool _initialized;

        public string ServerName => _config?.Name ?? "";
        public bool IsConnected => _transport.IsRunning && _initialized;

        public SfAiMcpClient(SfAiMcpServerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _transport.Start(_config);

            var initResult = await RequestAsync("initialize", BuildInitializeParams(), cancellationToken)
                .ConfigureAwait(false);
            if (initResult == null)
                throw new Exception($"MCP initialize 失败: {_config.Name}");

            await NotifyAsync("notifications/initialized", new JSONObject(), cancellationToken)
                .ConfigureAwait(false);

            _initialized = true;
            Debug.Log($"[SfAiMcp] 已连接: {_config.Name}");
        }

        public async Task<List<SfAiMcpRemoteTool>> ListToolsAsync(CancellationToken cancellationToken = default)
        {
            EnsureConnected();
            var result = await RequestAsync("tools/list", new JSONObject(), cancellationToken)
                .ConfigureAwait(false);

            var list = new List<SfAiMcpRemoteTool>();
            var tools = result?["tools"]?.AsArray;
            if (tools == null) return list;

            foreach (JSONNode item in tools)
            {
                list.Add(new SfAiMcpRemoteTool
                {
                    Name = item["name"]?.Value ?? "",
                    Description = item["description"]?.Value ?? "",
                    InputSchemaJson = item["inputSchema"]?.ToString() ??
                                      "{\"type\":\"object\",\"properties\":{}}"
                });
            }

            return list;
        }

        public async Task<string> CallToolAsync(
            string toolName,
            string argumentsJson,
            CancellationToken cancellationToken = default)
        {
            EnsureConnected();

            JSONNode argsNode;
            try
            {
                argsNode = JSON.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            }
            catch
            {
                argsNode = new JSONObject();
            }

            var p = new JSONObject();
            p["name"] = toolName;
            p["arguments"] = argsNode;

            var result = await RequestAsync("tools/call", p, cancellationToken).ConfigureAwait(false);
            return ExtractTextContent(result);
        }

        public void Dispose()
        {
            _initialized = false;
            _transport.Dispose();
        }

        void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException($"MCP 未连接: {_config.Name}");
        }

        static JSONObject BuildInitializeParams()
        {
            var p = new JSONObject();
            p["protocolVersion"] = "2024-11-05";
            p["capabilities"] = new JSONObject();
            var clientInfo = new JSONObject();
            clientInfo["name"] = "SFramework-SAI";
            clientInfo["version"] = "1.0.0";
            p["clientInfo"] = clientInfo;
            return p;
        }

        async Task NotifyAsync(string method, JSONNode parameters, CancellationToken cancellationToken)
        {
            var msg = new JSONObject();
            msg["jsonrpc"] = "2.0";
            msg["method"] = method;
            msg["params"] = parameters ?? new JSONObject();
            await _transport.WriteLineAsync(msg.ToString(), cancellationToken).ConfigureAwait(false);
        }

        async Task<JSONNode> RequestAsync(string method, JSONNode parameters, CancellationToken cancellationToken)
        {
            var id = _nextId++;
            var msg = new JSONObject();
            msg["jsonrpc"] = "2.0";
            msg["id"] = id;
            msg["method"] = method;
            msg["params"] = parameters ?? new JSONObject();

            await _transport.WriteLineAsync(msg.ToString(), cancellationToken).ConfigureAwait(false);

            while (true)
            {
                var line = await _transport.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(line)) continue;

                JSONNode node;
                try
                {
                    node = JSON.Parse(line);
                }
                catch
                {
                    continue;
                }

                // 跳过服务端通知
                if (node["id"] == null || node["id"].IsNull)
                    continue;

                if (node["id"].AsInt != id)
                    continue;

                if (node["error"] != null && !node["error"].IsNull)
                {
                    var err = node["error"]["message"]?.Value ?? node["error"].ToString();
                    throw new Exception($"MCP error: {err}");
                }

                return node["result"];
            }
        }

        static string ExtractTextContent(JSONNode result)
        {
            if (result == null) return "";
            var content = result["content"]?.AsArray;
            if (content == null) return result.ToString();

            var parts = new List<string>();
            foreach (JSONNode item in content)
            {
                if (item["type"]?.Value == "text")
                    parts.Add(item["text"]?.Value ?? "");
                else
                    parts.Add(item.ToString());
            }

            return string.Join("\n", parts);
        }
    }

    public class SfAiMcpRemoteTool
    {
        public string Name;
        public string Description;
        public string InputSchemaJson;
    }
}
