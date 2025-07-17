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
        public Dictionary<string,SheetData>.KeyCollection Keys => Datas.Keys;

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
            Sheets().Add(new SheetData(new string[1,1]));
        }

        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="IsCover">是否覆盖</param>
        public void Add(SheetData sheet, bool IsCover=false)
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
    public class SheetData : IEnumerable
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
            CheckArray(RowIndex, datas.Length);
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
            List<string> newDatas = new List<string>();
            foreach (var item in datas)
            {
                newDatas.Add(item.ToString());
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
            CheckArray(datas.Length, ColIndex);
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
            List<string> newDatas = new List<string>();
            foreach (var item in datas)
            {
                newDatas.Add(item.ToString());
            }
            SetCol(ColIndex, newDatas.ToArray());
        }

        /// <summary>
        /// 检查列索引
        /// </summary>
        /// <param name="RowIndex">行索引</param>
        /// <param name="ColIndex">列索引</param>
        public void CheckArray(int RowIndex = 0, int ColIndex = 0)
        {
            if (Datas == null)
            {
                Datas = new string[RowIndex + 1, ColIndex + 1];
            }
            else
            {
                string[,] newdatas = Datas;
                Datas = new string
                    [(newdatas.GetLength(0) > RowIndex ? newdatas.GetLength(0) : RowIndex)+1,
                    (newdatas.GetLength(1) > ColIndex ?
                    newdatas.GetLength(1) : ColIndex)+1];
                for (int i = 0; i < newdatas.GetLength(0); i++)
                {
                    for (int j = 0; j < newdatas.GetLength(1); j++)
                    {
                        Datas[i, j] = newdatas[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// 设置数值
        /// </summary>
        /// <param name="RowIndex">行索引</param>
        /// <param name="ColIndex">列索引</param>
        /// <param name="value">值</param>
        public void Set(int RowIndex, int ColIndex, object value) => Set(RowIndex, ColIndex, value.ToString());


        /// <summary>
        /// 设置数值
        /// </summary>
        /// <param name="RowIndex">行索引</param>
        /// <param name="ColIndex">列索引</param>
        /// <param name="value">值</param>
        public void Set(int RowIndex, int ColIndex, string value)
        {
            CheckArray(RowIndex, ColIndex);
            Datas[RowIndex, ColIndex] = value;
        }

        /// <summary> 
        /// 获取 
        /// </summary> 
        /// <param name="RowIndex">行索引</param> 
        /// <param name="ColIndex">列索引</param> 
        /// <returns>数值</returns> 
        public string Get(int RowIndex = 0, int ColIndex = 0) 
        { 
            if (Datas == null || RowIndex < 0 || ColIndex < 0 || RowIndex >= RowCount || ColIndex >= ColCount) 
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
        /// 遍历
        /// </summary>
        /// <param name="IsShowNull">是否显示为空的数值</param>
        public List<string> ForEach(bool IsShowNull = false)
        {
            List<string> datas = new List<string>();
            for (int i = 0; i < RowCount; i++)
            {
                for (int k = 0; k < ColCount; k++)
                {
                    string value = Get(i, k);
                    if (!IsShowNull && value == null) continue;
                    datas.Add(value);
                }
            }
            return datas;
        }

        /// <summary>
        /// 遍历
        /// </summary>
        /// <returns>二维数组</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => Datas.GetEnumerator();
    }
}