using System;
using System.Collections;
using System.Collections.Generic;

namespace SFramework.SFOffice.Module
{
    /// <summary>
    /// Excel数据
    /// </summary>
    public class SfExcelData : IEnumerable
    {
        /// <summary>
        /// Excel数据
        /// </summary>
        private Dictionary<string, SheetData> Datas = new Dictionary<string, SheetData>();

        /// <summary>
        /// Keys 值
        /// </summary>
        public Dictionary<string, SheetData>.KeyCollection Keys => Datas.Keys;

        /// <summary>
        /// 快速存取值
        /// </summary>
        /// <param name="sheetName">表名</param>
        public SheetData this[string sheetName]
        {
            get
            {
                var sheetData = new SheetData();
                if (Datas.TryGetValue(sheetName, out var data))
                {
                    sheetData = data;
                }

                return sheetData;
            }
        }

        /// <summary>
        /// 构造函数 初始化
        /// </summary>
        public SfExcelData()
        {
            Sheets().Add(new SheetData(new string[1, 1]));
        }

        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="isCover">是否覆盖</param>
        public void Add(SheetData sheet, bool isCover = false)
        {
            if (Datas.ContainsKey(sheet.name) && !isCover) return;
            else if (Datas.ContainsKey(sheet.name) && isCover)
                Datas.Remove(sheet.name);
            Datas.Add(sheet.name, sheet);
        }

        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="isCover">是否覆盖</param>
        /// <param name="sheets"></param>
        public void Add(bool isCover = false, params SheetData[] sheets)
        {
            foreach (var sheet in sheets)
            {
                if (Datas.ContainsKey(sheet.name) && !isCover) return;
                else if (Datas.ContainsKey(sheet.name) && isCover)
                    Datas.Remove(sheet.name);
                Datas.Add(sheet.name, sheet);
            }
        }

        /// <summary>
        /// 移除表
        /// </summary>
        /// <param name="sheetName">工作区名</param>
        public void Remove(string sheetName)
        {
            if (Datas.ContainsKey(sheetName))
                Datas.Remove(sheetName);
        }

        /// <summary>
        /// 全部工作表
        /// </summary>
        /// <returns>全部工作表</returns>
        public List<SheetData> Sheets()
        {
            var sheets = new List<SheetData>();
            foreach (var item in Datas.Values)
            {
                sheets.Add(item);
            }

            return sheets;
        }

        /// <summary>
        /// 遍历
        /// </summary>
        /// <returns>全部表</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => Datas.GetEnumerator();

        #region 运算符重载
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="dataOne">Excel数据</param>
        /// <param name="extendData">工作表</param>
        /// <returns>Excel数据</returns>
        public static SfExcelData operator +(SfExcelData dataOne, SheetData extendData)
        {
            dataOne.Add(extendData);
            return dataOne;
        }

        /// <summary>
        /// 移除表
        /// </summary>
        /// <param name="dataOne">Excel数据</param>
        /// <param name="extendData">工作表</param>
        /// <returns>Excel数据</returns>
        public static SfExcelData operator -(SfExcelData dataOne, SheetData extendData)
        {
            dataOne.Remove(extendData.name);
            return dataOne;
        }

        #endregion
    }

    /// <summary>
    /// 工作表
    /// </summary>
    public class SheetData : IEnumerable<string>
    {
        /// <summary>
        /// 工作表名称
        /// </summary>
        public string name;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Name">表名</param>
        public SheetData(string Name) => name = Name;

        /// <summary>
        /// 构造函数 自动表名NewSheet
        /// </summary>
        public SheetData() => name = "NewSheet";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="datas">值</param>
        public SheetData(string[,] datas)
        {
            Datas = datas;
            name = "NewSheet";
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="datas">值</param>
        /// <param name="Name">表名</param>
        public SheetData(string[,] datas, string Name)
        {
            Datas = datas;
            name = Name;
        }

        /// <summary>
        /// 数据
        /// </summary>
        private string[,] Datas;

        /// <summary>
        /// 添加行
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="datas">数据</param>
        public void SetRow(int rowIndex = 0, params string[] datas)
        {
            if (rowIndex < 0)
                throw new ArgumentException("行索引不能为负数", nameof(rowIndex));

            CheckArray(rowIndex, datas.Length - 1);
            for (int i = 0; i < datas.Length; i++)
            {
                Datas[rowIndex, i] = datas[i];
            }
        }

        /// <summary>
        /// 添加行
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="datas">数据</param>
        public void SetRow(int rowIndex = 0, params object[] datas)
        {
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            var newDatas = new List<string>();
            foreach (var item in datas)
            {
                newDatas.Add(item?.ToString() ?? string.Empty);
            }

            SetRow(rowIndex, newDatas.ToArray());
        }

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="colIndex">列索引</param>
        /// <param name="datas">数据</param>
        public void SetCol(int colIndex = 0, params string[] datas)
        {
            if (colIndex < 0)
                throw new ArgumentException("列索引不能为负数", nameof(colIndex));

            CheckArray(datas.Length - 1, colIndex);
            for (int i = 0; i < datas.Length; i++)
            {
                Datas[i, colIndex] = datas[i];
            }
        }

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="colIndex">列索引</param>
        /// <param name="datas">数据</param>
        public void SetCol(int colIndex = 0, params object[] datas)
        {
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            var newDatas = new List<string>();
            foreach (var item in datas)
            {
                newDatas.Add(item?.ToString() ?? string.Empty);
            }

            SetCol(colIndex, newDatas.ToArray());
        }

        /// <summary>
        /// 检查并调整数组大小
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="colIndex">列索引</param>
        public void CheckArray(int rowIndex = 0, int colIndex = 0)
        {
            if (rowIndex < 0 || colIndex < 0)
                throw new ArgumentException("索引不能为负数");

            if (Datas == null)
            {
                Datas = new string[rowIndex + 1, colIndex + 1];
            }
            else
            {
                int currentRows = Datas.GetLength(0);
                int currentCols = Datas.GetLength(1);

                int newRows = Math.Max(currentRows, rowIndex + 1);
                int newCols = Math.Max(currentCols, colIndex + 1);

                // 如果当前数组已经足够大，不需要重新分配
                if (newRows <= currentRows && newCols <= currentCols)
                    return;

                string[,] newDatas = new string[newRows, newCols];

                // 复制现有数据
                for (int i = 0; i < currentRows; i++)
                {
                    for (int j = 0; j < currentCols; j++)
                    {
                        newDatas[i, j] = Datas[i, j];
                    }
                }

                Datas = newDatas;
            }
        }

        /// <summary>
        /// 设置数值
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="colIndex">列索引</param>
        /// <param name="value">值</param>
        public void Set(int rowIndex, int colIndex, object value)
            => Set(rowIndex, colIndex, value?.ToString() ?? string.Empty);

        /// <summary>
        /// 设置数值
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <param name="colIndex">列索引</param>
        /// <param name="value">值</param>
        public void Set(int rowIndex, int colIndex, string value)
        {
            if (rowIndex < 0 || colIndex < 0)
                throw new ArgumentException("索引不能为负数");

            CheckArray(rowIndex, colIndex);
            Datas[rowIndex, colIndex] = value;
        }

        /// <summary> 
        /// 获取指定位置的值
        /// </summary> 
        /// <param name="rowIndex">行索引</param> 
        /// <param name="colIndex">列索引</param> 
        /// <returns>数值</returns> 
        public string Get(int rowIndex = 0, int colIndex = 0)
        {
            if (Datas == null || rowIndex < 0 || colIndex < 0 ||
                rowIndex >= RowCount || colIndex >= ColCount)
            {
                return null;
            }

            return Datas[rowIndex, colIndex];
        }

        /// <summary>
        /// 获取表格
        /// </summary>
        /// <returns>全部数据</returns>
        public string[,] Get() => Datas;

        /// <summary> 
        /// 行数量 
        /// </summary> 
        public int RowCount => Datas?.GetLength(0) ?? 0;

        /// <summary> 
        /// 列数量 
        /// </summary> 
        public int ColCount => Datas?.GetLength(1) ?? 0;

        /// <summary>
        /// 获取所有非空值的列表
        /// </summary>
        /// <param name="isShowNull">是否包含空值</param>
        public List<string> GetAllValues(bool isShowNull = false)
        {
            var values = new List<string>();
            for (var i = 0; i < RowCount; i++)
            {
                for (var k = 0; k < ColCount; k++)
                {
                    var value = Get(i, k);
                    if (!isShowNull && value == null) continue;
                    values.Add(value);
                }
            }

            return values;
        }

        /// <summary>
        /// 泛型遍历器
        /// </summary>
        /// <returns>字符串迭代器</returns>
        public IEnumerator<string> GetEnumerator()
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColCount; j++)
                {
                    yield return Get(i, j);
                }
            }
        }

        /// <summary>
        /// 非泛型遍历器
        /// </summary>
        /// <returns>对象迭代器</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}