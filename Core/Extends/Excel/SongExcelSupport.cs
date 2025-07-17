using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Excel; 
using OfficeOpenXml;
using UnityEngine;

/* 案例
                var filePath = Application.streamingAssetsPath+"/SongTaskManager.xlsx";
                var myExcelData = SongExcelSupport.ReadToExcelData(filePath);
                if (myExcelData != null)
                {
                    foreach (var sheet in myExcelData.Sheets())
                    {
                        for (var i = 1; i < sheet.RowCount; i++)
                        {
                            for (var k = 0; k < sheet.ColCount; k++)
                            {
                            }
                        }
                    }
                }
 */


namespace Song.Core.Extends.Excel
{
    /// <summary>
    /// Excel支持
    /// </summary>
    public static class SongExcelSupport
    {
        #region 存储
        /// <summary>
        /// 存储Excel
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="IsCreate">为空创建</param>
        /// <param name="IsCover">是否覆盖</param>
        public static bool CreateExcel(string filePath, bool IsCover = true, bool IsCreate = true)
        => Save(filePath, new SongExcelData(), IsCover, IsCreate);



        /// <summary>
        /// 存储Excel
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="datas">Excel 工作表数据</param>
        /// <param name="IsCreate">为空创建</param>
        /// <param name="IsCover">是否覆盖</param>
        public static bool Save(string filePath, SongExcelData datas, bool IsCover = true, bool IsCreate = true)
        {
            if (!filePath.Contains(".xlsx")) filePath += ".xlsx";
            if (!IsCreate && !File.Exists(filePath)) return false;
            if (!IsCover && File.Exists(filePath)) return false;

            try 
            {
                var excelName = new FileInfo(filePath);
                if (excelName.Exists)
                {
                    excelName.Delete();
                    excelName = new FileInfo(filePath);
                }

                using var package = new ExcelPackage(excelName);
                foreach (var item in datas.Sheets())
                {
                    var worksheet = package.Workbook.Worksheets.Add(item.name);
                    for (var i = 0; i < item.RowCount; i++)
                    {
                        for (var k = 0; k < item.ColCount; k++)
                        {
                            worksheet.Cells[i + 1, k + 1].Value = item.Get(i, k);
                        }
                    }
                }
                package.Save();
                return true;
            }
            catch (IOException e) // Catch the specific error
            {
                // Log a helpful message to the Unity Console
                Debug.LogError($"[SongExcelSupport] Could not save file. It is likely open in another program (like Excel). Please close the file and try again. \nFilePath: {filePath}\nError: {e.Message}");
                return false;
            }
            catch (Exception e) // Catch any other potential errors
            {
                Debug.LogError($"[SongExcelSupport] An unexpected error occurred while saving the Excel file. \nError: {e.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 存储Excel
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="data">Excel 工作表数据</param>
        /// <param name="IsCreate">为空创建</param>
        /// <param name="IsCover">是否覆盖</param>
        public static bool Save(string filePath,SheetData data, bool IsCover = true, bool IsCreate = true)
        {
            if (!filePath.Contains(".xlsx")) filePath += ".xlsx";
            if (!IsCreate && !File.Exists(filePath)) return false;
            if (!IsCover && File.Exists(filePath)) return false;
            FileInfo excelName = new FileInfo(filePath);
            if (excelName.Exists)
            {
                excelName.Delete();
                excelName = new FileInfo(filePath);
            }
            using (ExcelPackage package = new ExcelPackage(excelName))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(data.name);
                for (int i = 0; i < data.RowCount; i++)
                {
                    for (int k = 0; k < data.ColCount; k++)
                    {
                        worksheet.Cells[i + 1, k + 1].Value = data.Get(i,k);
                    }
                }
                package.Save();
            }
            return true;
        }

        /// <summary>
        /// 存储Excel
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="data">Excel 工作表数据</param>
        /// <param name="IsCreate">为空创建</param>
        /// <param name="IsCover">是否覆盖</param>
        public static bool Save(string filePath, Dictionary<string, List<List<string>>> data, bool IsCover = true,
            bool IsCreate = true)
        {
            if (!filePath.Contains(".xlsx")) filePath += ".xlsx";
            if (!IsCreate && !File.Exists(filePath)) return false;
            if (!IsCover && File.Exists(filePath)) return false;
            FileInfo excelName = new FileInfo(filePath);
            if (excelName.Exists)
            {
                excelName.Delete();
                excelName = new FileInfo(filePath);
            }

            using (ExcelPackage package = new ExcelPackage(excelName))
            {
                foreach (var sheetData in data)
                {
                    ExcelWorksheet worksheet;
                    if (!IsCreate)
                    {
                        worksheet = package.Workbook.Worksheets[sheetData.Key];
                        if (worksheet == null)
                        {
                            worksheet = package.Workbook.Worksheets.Add(sheetData.Key);
                        }
                    }
                    else
                    {
                        worksheet = package.Workbook.Worksheets.Add(sheetData.Key);
                    }

                    int rowIndex = 1;
                    foreach (var rowData in sheetData.Value)
                    {
                        int colIndex = 1;
                        foreach (var cellData in rowData)
                        {
                            worksheet.Cells[rowIndex, colIndex].Value = cellData;
                            colIndex++;
                        }
                        rowIndex++;
                    }
                }
                package.Save();
                return true;
            }
        }
        #endregion

        #region 读取
        /// <summary>
        /// 读取Excel表至StringList
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>字符表</returns>
        public static List<string> ReadToStringList(string filePath)
            => DataSet2StringList(ReadToDataSet(filePath)); // 这里需要注意，如果ReadToDataSet逻辑改变，这里也可能受影响

        /// <summary>
        /// 读取Excel表至Excel数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Excel数据</returns>
        public static SongExcelData ReadToExcelData(string filePath)
        {
            SongExcelData excelData = new SongExcelData();
            FileInfo excelFile = new FileInfo(filePath);

            if (!excelFile.Exists)
            {
                Debug.LogError($"文件不存在: {filePath}");
                return excelData;
            }

            using (ExcelPackage package = new ExcelPackage(excelFile))
            {
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    string sheetName = worksheet.Name;
                    int rowCount = worksheet.Dimension?.Rows ?? 0;
                    int colCount = worksheet.Dimension?.Columns ?? 0;

                    // 如果工作表为空，仍然添加一个空的SheetData
                    if (rowCount == 0 || colCount == 0)
                    {
                        excelData.Add(new SheetData(sheetName));
                        continue;
                    }

                    string[,] sheetDatas = new string[rowCount, colCount];
                    for (int r = 0; r < rowCount; r++)
                    {
                        for (int c = 0; c < colCount; c++)
                        {
                            // ExcelPackage的Cells是1-based索引
                            sheetDatas[r, c] = worksheet.Cells[r + 1, c + 1].Value?.ToString();
                        }
                    }
                    excelData.Add(new SheetData(sheetDatas, sheetName));
                }
            }
            return excelData;
        }

        /// <summary>
        /// 读取Excel表至DataSet (保留原有逻辑，但可能不推荐直接使用此方法进行ExcelData转换)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>Excel数据</returns>
        public static DataSet ReadToDataSet(string filePath)
        {
            DataSet dataSet = new DataSet();
            FileInfo excelFile = new FileInfo(filePath);

            if (!excelFile.Exists)
            {
                Debug.LogError($"文件不存在: {filePath}");
                return dataSet;
            }
            
            // 使用try-catch确保文件流被正确关闭
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) // 更改FileShare为ReadWrite以避免潜在的访问冲突
            {
                // 使用ExcelReaderFactory.CreateReader或CreateOpenXmlReader
                // 如果是.xls文件使用CreateBinaryReader, 如果是.xlsx使用CreateOpenXmlReader
                IExcelDataReader excelReader = null;
                if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else if (filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else
                {
                    Debug.LogError($"不支持的文件格式: {filePath}");
                    return dataSet;
                }

                using (excelReader)
                {
                    // 配置DataSet转换选项，例如，确保第一行不被视为标题
                    excelReader.IsFirstRowAsColumnNames = false; 
                    dataSet = excelReader.AsDataSet();
                }
            }
            return dataSet;
        }


        /// <summary>
        /// Excel数据转StringList
        /// </summary>
        /// <param name="result">Excel数据</param>
        /// <returns>StringList</returns>
        public static List<string> DataSet2StringList(DataSet result)
        {
            List<string> datas = new List<string>();
            if (result == null || result.Tables.Count == 0) return datas;

            for (int i = 0; i < result.Tables.Count; i++)
            {
                DataTable table = result.Tables[i];
                if (table == null) continue;

                DataRowCollection DRC = table.Rows;
                int columnCount = table.Columns.Count;
                int rowCount = table.Rows.Count;
                for (int j = 0; j < rowCount; j++)
                {
                    for (int k = 0; k < columnCount; k++)
                    {
                        datas.Add(DRC[j][k]?.ToString()); // 使用?.操作符避免空引用异常
                    }
                }
            }
            return datas;
        }


        /// <summary>
        /// DataSet转Dictionary<string,string[,]>
        /// </summary>
        /// <param name="result">DataSet</param>
        /// <returns>Dictionary<string,string[,]></returns>
        public static Dictionary<string, string[,]> DataSet2DictionaryStringArrayTwo(DataSet result)
        {
            var dictionary = new Dictionary<string, string[,]>();

            if (result != null && result.Tables != null)
            {
                foreach (DataTable table in result.Tables)
                {
                    if (table != null && table.Rows != null && table.Columns != null)
                    {
                        var data = new string[table.Rows.Count, table.Columns.Count];
                        for (var i = 0; i < table.Rows.Count; i++)
                        {
                            var row = table.Rows[i];
                            for (var j = 0; j < table.Columns.Count; j++)
                            {
                                data[i, j] = row[j]?.ToString(); // 使用?.操作符避免空引用异常
                            }
                        }
                        dictionary.Add(table.TableName, data);
                    }
                    else
                    {
                        // 确保即使表为空也添加条目
                        dictionary.Add(table.TableName, new string[0, 0]);
                    }
                }
            }

            return dictionary;
        }

        /// <summary>
        /// DataSet转Dictionary<string, List<List<string>>>
        /// </summary>
        /// <param name="result">DataSet</param>
        /// <returns>Dictionary<string, List<List<string>>></returns>
        public static Dictionary<string, List<List<string>>> DataSet2DictionaryListList(DataSet result)
        {
            var dictionary = new Dictionary<string, List<List<string>>>();

            if (result != null && result.Tables != null)
            {
                foreach (DataTable table in result.Tables)
                {
                    if (table != null && table.Rows != null && table.Columns != null)
                    {
                        var data = new List<List<string>>();
                        for (var i = 0; i < table.Rows.Count; i++)
                        {
                            var row = table.Rows[i];
                            var rowData = new List<string>();
                            for (var j = 0; j < table.Columns.Count; j++)
                            {
                                rowData.Add(row[j]?.ToString()); // 使用?.操作符避免空引用异常
                            }
                            data.Add(rowData);
                        }
                        dictionary.Add(table.TableName, data);
                    }
                    else
                    {
                        // 确保即使表为空也添加条目
                        dictionary.Add(table.TableName, new List<List<string>>());
                    }
                }
            }

            return dictionary;
        }


        /// <summary>
        /// DataSet转ExcelData (此方法现在会调用新的ReadToExcelData，或者您可以决定废弃它转而直接使用ReadToExcelData)
        /// </summary>
        /// <param name="result">DataSet</param>
        /// <returns>ExcelData</returns>
        public static SongExcelData DataSet2ExcelData(DataSet result)
        {
            SongExcelData datas = new SongExcelData();
            if (result == null || result.Tables.Count == 0) return datas;

            for (int i = 0; i < result.Tables.Count; i++)
            {
                DataTable table = result.Tables[i];
                if (table == null) continue;

                DataRowCollection DRC = table.Rows;
                int rowCount = table.Rows.Count;
                int columnCount = table.Columns.Count;
                string TableName = table.TableName;

                // 检查是否有数据，如果行或列为0，则创建一个空的SheetData
                if (rowCount == 0 || columnCount == 0)
                {
                    datas.Add(new SheetData(TableName));
                    continue;
                }

                string[,] Sheetdatas = new string[rowCount, columnCount];
                for (int j = 0; j < rowCount; j++)
                {
                    for (int k = 0; k < columnCount; k++)
                    {
                        Sheetdatas[j, k] = (DRC[j][k]?.ToString()); // 使用?.操作符避免空引用异常
                    }
                }
                SheetData sheetData = new SheetData(Sheetdatas,TableName);
                datas.Add(sheetData);
            }
            return datas;
        }
        #endregion
    }
}