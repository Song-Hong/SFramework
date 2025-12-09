using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SFramework.Core.Extends.SfDataSupport
{
    /// <summary>
    /// 核心数据节点类 SfData，支持索引器和隐式转换。
    /// </summary>
    public class SfData
    {
        private SfDataType _type = SfDataType.None;
        internal object Value;
        private Dictionary<string, SfData> _objectMap;
        public List<SfData> ArrayList;

        // --- 构造函数 ---
        public SfData() { }
        public SfData(string v) { _type = SfDataType.String; Value = v; }
        public SfData(int v) { _type = SfDataType.Int; Value = v; }
        public SfData(double v) { _type = SfDataType.Double; Value = v; }
        public SfData(bool v) { _type = SfDataType.Boolean; Value = v; }

        // --- 核心属性 ---
        public SfDataType Type => _type;
        public bool IsObject => _type == SfDataType.Object;
        public bool IsArray => _type == SfDataType.Array;

        // --- 索引器访问 ---
        public SfData this[string key]
        {
            get
            {
                if (_type != SfDataType.Object || _objectMap == null || !_objectMap.TryGetValue(key, out var item))
                    return new SfData(); 
                return item;
            }
            set
            {
                if (_type != SfDataType.Object) InitObject();
                _objectMap[key] = value;
            }
        }

        public SfData this[int index]
        {
            get
            {
                if (_type != SfDataType.Array || ArrayList == null || index < 0 || index >= ArrayList.Count)
                    return new SfData();
                return ArrayList[index];
            }
            set
            {
                if (_type != SfDataType.Array) InitArray();
                while (ArrayList.Count <= index) ArrayList.Add(new SfData());
                ArrayList[index] = value;
            }
        }

        // 常用属性
        public int Count => IsArray ? ArrayList.Count : (IsObject ? _objectMap.Count : 0);
        public ICollection<string> Keys => IsObject ? _objectMap.Keys : new List<string>();

        // --- 隐式转换 (提供便捷性) ---
        public static implicit operator string(SfData d) => d.ToStringValue();
        public static implicit operator int(SfData d) => d.ToIntValue();
        public static implicit operator double(SfData d) => d.ToDoubleValue();
        public static implicit operator bool(SfData d) => d.ToBoolValue();
        
        // 转换辅助方法，更安全地处理null
        internal string ToStringValue()
        {
            if (_type == SfDataType.String) return Value?.ToString();
            // 简单值类型直接返回字符串形式
            if (_type == SfDataType.Int || _type == SfDataType.Double || _type == SfDataType.Boolean) return Value?.ToString();
            return null;
        }

        private int ToIntValue()
        {
            if (_type == SfDataType.Int) return (int)Value;
            if (_type == SfDataType.Double) return (int)(double)Value;
            // 尝试从字符串转换
            if (_type == SfDataType.String && int.TryParse((string)Value, out int i)) return i;
            return 0;
        }

        private double ToDoubleValue()
        {
            if (_type == SfDataType.Double) return (double)Value;
            if (_type == SfDataType.Int) return (int)Value;
            // 尝试从字符串转换
            if (_type == SfDataType.String && double.TryParse((string)Value, out double d)) return d;
            return 0.0;
        }

        private bool ToBoolValue()
        {
            if (_type == SfDataType.Boolean) return (bool)Value;
            // 尝试从字符串转换
            if (_type == SfDataType.String && bool.TryParse((string)Value, out bool b)) return b;
            return false;
        }


        public override string ToString() => Dump();

        // --- 解析入口 (Load) ---
        /// <summary>
        /// 从 SfFormat 文本解析为 <see cref="SfData"/>。
        /// </summary>
        /// <param name="text">输入的 SfFormat 文本。</param>
        public static SfData Load(string text)
        {
            var parser = new SfDataParser(text);
            return parser.Parse();
        }

        /// <summary>
        /// 从文件读取 SfFormat 文本并解析为 <see cref="SfData"/>。
        /// </summary>
        /// <param name="path">文件路径。</param>
        public static SfData LoadFile(string path)
        {
            if (!File.Exists(path)) return new SfData();
            return Load(File.ReadAllText(path, Encoding.UTF8));
        }

        /// <summary>
        /// 将标准 <see cref="SfData"/> 转换为 Super 节点树。
        /// </summary>
        public SfDataSuperNode ToSuper()
        {
            switch (_type)
            {
                case SfDataType.String:
                    return new SfDataSuperNode((string)this);
                case SfDataType.Int:
                    return new SfDataSuperNode((int)this);
                case SfDataType.Double:
                    return new SfDataSuperNode((double)this);
                case SfDataType.Boolean:
                    return new SfDataSuperNode((bool)this);
                case SfDataType.Array:
                {
                    var n = new SfDataSuperNode();
                    foreach (var item in ArrayList)
                    {
                        n.Add(item.ToSuper());
                    }
                    return n;
                }
                case SfDataType.Object:
                {
                    var n = new SfDataSuperNode();
                    foreach (var kv in _objectMap)
                    {
                        n[kv.Key] = kv.Value.ToSuper();
                    }
                    return n;
                }
                default:
                    return new SfDataSuperNode();
            }
        }

        // --- 序列化入口 (Dump) ---
        /// <summary>
        /// 序列化为 SfFormat 文本。
        /// </summary>
        /// <param name="indent">是否缩进排版。</param>
        public string Dump(bool indent = true)
        {
            StringBuilder sb = new StringBuilder();
            DumpNode(this, sb, 0, indent);
            // 确保顶级对象的结尾不会多余的换行
            string result = sb.ToString();
            return result.TrimEnd(); 
        }

        // --- 内部初始化 ---
        private void InitObject()
        {
            _type = SfDataType.Object;
            _objectMap = new Dictionary<string, SfData>();
            Value = null;
        }

        private void InitArray()
        {
            _type = SfDataType.Array;
            ArrayList = new List<SfData>();
            Value = null;
        }

        /// <summary>
        /// 向数组追加一个元素。
        /// </summary>
        /// <param name="item">要追加的节点。</param>
        public void Add(SfData item)
        {
            if (_type != SfDataType.Array) InitArray();
            ArrayList.Add(item);
        }

        // --- 核心递归序列化逻辑 ---
        private void DumpNode(SfData node, StringBuilder sb, int depth, bool indent)
        {
            // 基础缩进字符串
            string pad = indent ? new string(' ', depth * 4) : "";
            // 下一层级缩进字符串
            string innerPad = indent ? new string(' ', (depth + 1) * 4) : "";
            // 换行符
            string newLine = indent ? "\n" : " ";

            switch (node.Type)
            {
                case SfDataType.String:
                    string val = node.Value?.ToString() ?? "";
                    // 简单判断是否需要引号：包含特殊字符时加引号
                    bool needQuotes = val.Contains(" ") || val.Contains(":") || val.Contains(",") || val.Contains("[") || val.Contains("]") || val.Contains("{") || val.Contains("}") || val.Contains("\n") || val.Contains("\t") || val.ToLower() == "true" || val.ToLower() == "false" || double.TryParse(val, out _);
                    sb.Append(needQuotes ? $"\"{val}\"" : val);
                    break;
                case SfDataType.Int:
                case SfDataType.Double:
                    // 小数点格式化为invariant culture，避免本地化问题
                    if (node.Type == SfDataType.Double)
                    {
                        sb.Append(((double)node.Value).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(node.Value);
                    }
                    break;
                case SfDataType.Boolean:
                    sb.Append(node.Value.ToString().ToLower());
                    break;
                
                case SfDataType.Array:
                    sb.Append("[");
                    bool firstArray = true;
                    foreach (var item in node.ArrayList)
                    {
                        if (!firstArray) sb.Append(",");
                        firstArray = false;
                        
                        // 数组内部的每个元素都换行和缩进
                        sb.Append(newLine + innerPad);
                        DumpNode(item, sb, depth + 1, indent);
                    }
                    // 闭合数组
                    if (node.ArrayList.Count > 0) sb.Append(newLine + pad);
                    sb.Append("]");
                    break;
                
                case SfDataType.Object:
                    bool isRoot = depth == 0;
                    
                    // 只有非根对象才添加开头的 {
                    if (!isRoot) sb.Append("{"); 
                    
                    int count = 0;
                    foreach (var kvp in node._objectMap)
                    {
                        // 1. 处理分隔符 (逗号)
                        if (count > 0)
                        {
                            sb.Append(","); 
                        }
                        
                        // 2. 处理换行和缩进
                        if (indent)
                        {
                             // 根对象：每个键值对都换行
                            if (isRoot) sb.Append(newLine + pad);
                            // 非根对象：在 { 之后，每个键值对都使用 innerPad 缩进
                            else sb.Append(newLine + innerPad);
                        }
                        
                        // 3. 输出键值对
                        // 键在 SfFormat 中通常不加引号，但为了安全，如果键中包含空格等特殊字符，可以考虑加引号。
                        // 这里沿用旧逻辑，不加引号
                        sb.Append(kvp.Key);
                        sb.Append(":"); 
                        DumpNode(kvp.Value, sb, depth + 1, indent); 
                        
                        count++;
                    }
                    
                    // 4. 闭合非根对象
                    if (!isRoot) 
                    {
                        // 如果启用了缩进且内容不为空，添加闭合前的换行和缩进
                        if (indent && count > 0) sb.Append(newLine + pad); 
                        sb.Append("}");
                    }
                    
                    break;
                default:
                    sb.Append("null");
                    break;
            }
        }
    }
}
