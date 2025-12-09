using System.Collections.Generic;
using System.Text;
using SFramework.Core.Extends.SfDataSupport;

namespace SFramework.Core.Extends.SfDataSupport
{
    public class SfDataSuperNode
    {
        private SfDataType _type = SfDataType.None;
        internal object Value;
        private Dictionary<string, SfDataSuperNode> _objectMap;
        public List<SfDataSuperNode> ArrayList;

        public SfDataSuperNode() { }
        public SfDataSuperNode(string v) { _type = SfDataType.String; Value = v; }
        public SfDataSuperNode(int v) { _type = SfDataType.Int; Value = v; }
        public SfDataSuperNode(double v) { _type = SfDataType.Double; Value = v; }
        public SfDataSuperNode(bool v) { _type = SfDataType.Boolean; Value = v; }

        public SfDataType Type => _type;
        public bool IsObject => _type == SfDataType.Object;
        public bool IsArray => _type == SfDataType.Array;

        public SfDataSuperNode this[string key]
        {
            get
            {
                if (_type != SfDataType.Object || _objectMap == null || !_objectMap.TryGetValue(key, out var item))
                    return new SfDataSuperNode();
                return item;
            }
            set
            {
                if (_type != SfDataType.Object) InitObject();
                _objectMap[key] = value;
            }
        }

        public SfDataSuperNode this[int index]
        {
            get
            {
                if (_type != SfDataType.Array || ArrayList == null || index < 0 || index >= ArrayList.Count)
                    return new SfDataSuperNode();
                return ArrayList[index];
            }
            set
            {
                if (_type != SfDataType.Array) InitArray();
                while (ArrayList.Count <= index) ArrayList.Add(new SfDataSuperNode());
                ArrayList[index] = value;
            }
        }

        private void InitObject()
        {
            _type = SfDataType.Object;
            _objectMap = new Dictionary<string, SfDataSuperNode>();
            Value = null;
        }

        private void InitArray()
        {
            _type = SfDataType.Array;
            ArrayList = new List<SfDataSuperNode>();
            Value = null;
        }

        public void Add(SfDataSuperNode item)
        {
            if (_type != SfDataType.Array) InitArray();
            ArrayList.Add(item);
        }

        public SfData ToStandard()
        {
            switch (_type)
            {
                case SfDataType.String:
                    return new SfData((string)Value);
                case SfDataType.Int:
                    return new SfData((int)Value);
                case SfDataType.Double:
                    return new SfData((double)Value);
                case SfDataType.Boolean:
                    return new SfData((bool)Value);
                case SfDataType.Array:
                {
                    var n = new SfData();
                    foreach (var item in ArrayList)
                    {
                        n.Add(item.ToStandard());
                    }
                    return n;
                }
                case SfDataType.Object:
                {
                    var n = new SfData();
                    foreach (var kv in _objectMap)
                    {
                        n[kv.Key] = kv.Value.ToStandard();
                    }
                    return n;
                }
                default:
                    return new SfData();
            }
        }
    }
}
