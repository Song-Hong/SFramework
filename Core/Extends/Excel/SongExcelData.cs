using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Song.Core.Extends.Excel
{
    /// <summary>
    /// Excel数据
    /// </summary>
    public class SongExcelData : IEnumerable
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
        public SongExcelData()
        {
            Sheets().Add(new SheetData(new string[1, 1]));
        }

        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="IsCover">是否覆盖</param>
        public void Add(SheetData sheet, bool IsCover = false)
        {
            if (Datas.ContainsKey(sheet.name) && !IsCover) return;
            else if (Datas.ContainsKey(sheet.name) && IsCover)
                Datas.Remove(sheet.name);
            Datas.Add(sheet.name, sheet);
        }

        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="IsCover">是否覆盖</param>
        /// <param name="sheets"></param>
        public void Add(bool IsCover = false, params SheetData[] sheets)
        {
            foreach (var sheet in sheets)
            {
                if (Datas.ContainsKey(sheet.name) && !IsCover) return;
                else if (Datas.ContainsKey(sheet.name) && IsCover)
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
            List<SheetData> sheets = new List<SheetData>();
            foreach (SheetData item in Datas.Values)
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

        public static SongExcelData operator +(SongExcelData dataOne, SheetData ExtendData)
        {
            SongExcelData excelData = dataOne;
            excelData.Add(ExtendData);
            return excelData;
        }

        public static SongExcelData operator -(SongExcelData dataOne, SheetData ExtendData)
        {
            SongExcelData excelData = dataOne;
            excelData.Remove(ExtendData.name);
            return excelData;
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
        /// <param name="RowIndex">行索引</param>
        /// <param name="datas">数据</param>
        public void SetRow(int RowIndex = 0, params string[] datas)
        {
            if (RowIndex < 0)
                throw new ArgumentException("行索引不能为负数", nameof(RowIndex));

            CheckArray(RowIndex, datas.Length - 1);
            for (int i = 0; i < datas.Length; i++)
            {
                Datas[RowIndex, i] = datas[i];
            }
        }

        /// <summary>
        /// 添加行
        /// </summary>
        /// <param name="RowIndex">行索引</param>
        /// <param name="datas">数据</param>
        public void SetRow(int RowIndex = 0, params object[] datas)
        {
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            List<string> newDatas = new List<string>();
            foreach (var item in datas)
            {
                newDatas.Add(item?.ToString() ?? string.Empty);
            }

            SetRow(RowIndex, newDatas.ToArray());
        }

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="ColIndex">列索引</param>
        /// <param name="datas">数据</param>
        public void SetCol(int ColIndex = 0, params string[] datas)
        {
            if (ColIndex < 0)
                throw new ArgumentException("列索引不能为负数", nameof(ColIndex));

            CheckArray(datas.Length - 1, ColIndex);
            for (int i = 0; i < datas.Length; i++)
            {
                Datas[i, ColIndex] = datas[i];
            }
        }

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="ColIndex">列索引</param>
        /// <param name="datas">数据</param>
        public void SetCol(int ColIndex = 0, params object[] datas)
        {
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));

            List<string> newDatas = new List<string>();
            foreach (var item in datas)
            {
                newDatas.Add(item?.ToString() ?? string.Empty);
            }

            SetCol(ColIndex, newDatas.ToArray());
        }

        /// <summary>
        /// 检查并调整数组大小
        /// </summary>
        /// <param name="RowIndex">行索引</param>
        /// <param name="ColIndex">列索引</param>
        public void CheckArray(int RowIndex = 0, int ColIndex = 0)
        {
            if (RowIndex < 0 || ColIndex < 0)
                throw new ArgumentException("索引不能为负数");

            if (Datas == null)
            {
                Datas = new string[RowIndex + 1, ColIndex + 1];
            }
            else
            {
                int currentRows = Datas.GetLength(0);
                int currentCols = Datas.GetLength(1);

                int newRows = Math.Max(currentRows, RowIndex + 1);
                int newCols = Math.Max(currentCols, ColIndex + 1);

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
        /// <param name="RowIndex">行索引</param>
        /// <param name="ColIndex">列索引</param>
        /// <param name="value">值</param>
        public void Set(int RowIndex, int ColIndex, object value)
            => Set(RowIndex, ColIndex, value?.ToString() ?? string.Empty);

        /// <summary>
        /// 设置数值
        /// </summary>
        /// <param name="RowIndex">行索引</param>
        /// <param name="ColIndex">列索引</param>
        /// <param name="value">值</param>
        public void Set(int RowIndex, int ColIndex, string value)
        {
            if (RowIndex < 0 || ColIndex < 0)
                throw new ArgumentException("索引不能为负数");

            CheckArray(RowIndex, ColIndex);
            Datas[RowIndex, ColIndex] = value;
        }

        /// <summary> 
        /// 获取指定位置的值
        /// </summary> 
        /// <param name="RowIndex">行索引</param> 
        /// <param name="ColIndex">列索引</param> 
        /// <returns>数值</returns> 
        public string Get(int RowIndex = 0, int ColIndex = 0)
        {
            if (Datas == null || RowIndex < 0 || ColIndex < 0 ||
                RowIndex >= RowCount || ColIndex >= ColCount)
            {
                return null;
            }

            return Datas[RowIndex, ColIndex];
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
        /// <param name="IsShowNull">是否包含空值</param>
        public List<string> GetAllValues(bool IsShowNull = false)
        {
            List<string> values = new List<string>();
            for (int i = 0; i < RowCount; i++)
            {
                for (int k = 0; k < ColCount; k++)
                {
                    string value = Get(i, k);
                    if (!IsShowNull && value == null) continue;
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
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColCount; j++)
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