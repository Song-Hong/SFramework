using System;
using System.Text;

namespace SFramework.Core.Extends.SfDataSupport
{
    /// <summary>
    /// 表示 SfFormat 解析过程中发生的错误。
    /// </summary>
    public sealed class SfDataException : Exception
    {
        /// <summary>
        /// 使用指定消息创建异常。
        /// </summary>
        /// <param name="message">错误消息。</param>
        public SfDataException(string message) : base(message) { }
    }

    /// <summary>
    /// 负责解析 SfFormat 文本，生成 SfData 结构。
    /// </summary>
    public class SfDataParser
    {
        private string _text;
        private int _idx;

        public SfDataParser(string text)
        {
            _text = text;
            _idx = 0;
        }

        /// <summary>
        /// 解析输入文本为 <see cref="SfData"/> 根节点。
        /// </summary>
        public SfData Parse()
        {
            SkipWhitespaceAndComments();
            if (_idx < _text.Length && _text[_idx] != '{' && _text[_idx] != '[')
            {
                // 顶层是 key:value 列表，解析为一个根对象
                return ParseBodyAsObject(endChar: '\0', isTopLevel: true);
            }
            return ParseValue();
        }

        /// <summary>
        /// 解析一个值，可为对象、数组、字符串或原始值。
        /// </summary>
        private SfData ParseValue()
        {
            SkipWhitespaceAndComments();
            if (_idx >= _text.Length) return new SfData();

            char c = _text[_idx];

            if (c == '{')
            {
                _idx++;
                return ParseBodyAsObject('}', isTopLevel: false);
            }
            else if (c == '[')
            {
                _idx++;
                return ParseSquareBracketBlock();
            }
            else if (c == '"')
            {
                return ParseQuotedString();
            }
            else
            {
                return ParsePrimitive();
            }
        }

        /// <summary>
        /// 处理方括号区块，自动区分数组或对象。
        /// </summary>
        private SfData ParseSquareBracketBlock()
        {
            SkipWhitespaceAndComments();
            if (_idx < _text.Length && _text[_idx] == ']') { _idx++; return new SfData(); } // 空 []

            var startPosAfterBracket = _idx;
            
            // 尝试解析第一个键（可能是值）
            var firstKeyOrVal = ParseValue(); 
            // 此时 _idx 已经移动到第一个键/值之后
            
            SkipWhitespaceAndComments();

            var isObject = false || _idx < _text.Length && _text[_idx] == ':';
            
            // 重置索引到第一个元素的开始位置（[ 之后），准备重新解析
            _idx = startPosAfterBracket; 
            
            if (isObject)
            {
                // 解析为对象 (Dictionary)
                return ParseBodyAsObject(']', isTopLevel: false);
            }
            else
            {
                // 解析为数组 (Array)
                var node = new SfData();

                while (_idx < _text.Length)
                {
                    SkipWhitespaceAndComments();
                    if (_text[_idx] == ']') { _idx++; break; }
                    
                    if (_idx < _text.Length && _text[_idx] == ',') _idx++;

                    SkipWhitespaceAndComments();
                    if (_idx < _text.Length && _text[_idx] == ']') { _idx++; break; }

                    node.Add(ParseValue());
                }
                return node;
            }
        }

        /// <summary>
        /// 解析对象体，支持以逗号分隔的 key:value 列表。
        /// </summary>
        /// <param name="endChar">结束字符，'}'、']' 或 '\0'。</param>
        /// <param name="isTopLevel">是否为顶层对象。</param>
        private SfData ParseBodyAsObject(char endChar, bool isTopLevel)
        {
            var node = new SfData(); 
            while (_idx < _text.Length)
            {
                SkipWhitespaceAndComments();
                if (endChar != '\0' && _text[_idx] == endChar) { _idx++; break; }

                // 如果是顶级，且遇到非 key 字符，则可能结束了
                if (isTopLevel && (IsSeparator(_text[_idx]) && _text[_idx] != ':')) break; 
                
                string key = ParseKey();
                SkipWhitespaceAndComments();

                // 必须有冒号分隔
                if (_idx < _text.Length && _text[_idx] == ':') _idx++;
                else throw new SfDataException($"Expected ':' after key '{key}' at index {_idx}.");

                SfData val = ParseValue();
                node[key] = val;

                SkipWhitespaceAndComments();
                if (_idx < _text.Length && _text[_idx] == ',') _idx++;
            }
            return node;
        }

        /// <summary>
        /// 解析对象键名，支持未引号形式与引号字符串。
        /// </summary>
        private string ParseKey()
        {
            SkipWhitespaceAndComments();
            if (_idx < _text.Length && _text[_idx] == '"') return ParseQuotedString().ToStringValue();
            
            int start = _idx;
            // Key读到 : 或 空格(遇到:之前的空格) 或 终结符
            while (_idx < _text.Length && _text[_idx] != ':' && !IsWhiteSpace(_text[_idx]) && !IsEndChar(_text[_idx]))
                _idx++;

            string key = _text.Substring(start, _idx - start).Trim();
            
            // 确保跳过Key和冒号之间的空格
            while (_idx < _text.Length && _text[_idx] != ':' && IsWhiteSpace(_text[_idx]))
                _idx++;

            return key;
        }

        /// <summary>
        /// 解析引号字符串并处理常见转义字符。
        /// </summary>
        private SfData ParseQuotedString()
        {
            _idx++; // 跳过首 "
            var sb = new StringBuilder();
            var escaped = false;
            while (_idx < _text.Length)
            {
                char c = _text[_idx];
                
                if (escaped)
                {
                    // 简单处理转义字符
                    switch (c)
                    {
                        case '"': sb.Append('"'); break;
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case '\\': sb.Append('\\'); break;
                        default: sb.Append(c); break; // 其他当普通字符处理
                    }
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    _idx++;
                    break;
                }
                else
                {
                    sb.Append(c);
                }
                _idx++;
            }
            return new SfData(sb.ToString());
        }

        /// <summary>
        /// 解析原始值，包括布尔、整数与浮点，其他视为字符串。
        /// </summary>
        private SfData ParsePrimitive()
        {
            // 读取直到分隔符
            int start = _idx;
            while (_idx < _text.Length && !IsSeparator(_text[_idx]) && !IsEndChar(_text[_idx]))
            {
                _idx++;
            }
            string raw = _text.Substring(start, _idx - start).Trim();

            if (raw.Length == 0)
            {
                // 如果解析到一个空字符串，可能是解析数组或字典时，值为空
                // 暂时返回一个空节点，在调用端决定如何处理
                return new SfData(); 
            }

            if (raw.ToLower() == "true") return new SfData(true);
            if (raw.ToLower() == "false") return new SfData(false);
            
            // 使用 InvariantCulture 尝试解析 double
            if (raw.Contains(".") && double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d)) return new SfData(d);
            
            if (int.TryParse(raw, out int i)) return new SfData(i);

            // 否则视为原始字符串值
            return new SfData(raw);
        }

        /// <summary>
        /// 跳过空白与以 '#' 开头的整行注释。
        /// </summary>
        private void SkipWhitespaceAndComments()
        {
            while (_idx < _text.Length)
            {
                char c = _text[_idx];
                if (IsWhiteSpace(c))
                {
                    _idx++;
                }
                else if (c == '#')
                {
                    // 跳过整行
                    while (_idx < _text.Length && _text[_idx] != '\n' && _text[_idx] != '\r')
                        _idx++;
                    _idx++; // 跳过换行符
                }
                else
                {
                    break;
                }
            }
        }

        private bool IsWhiteSpace(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r';
        private bool IsSeparator(char c) => IsWhiteSpace(c) || c == ',' || c == ':';
        private bool IsEndChar(char c) => c == ']' || c == '}';
    }
}
