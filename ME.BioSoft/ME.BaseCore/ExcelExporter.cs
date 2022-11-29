
using ME.BaseCore.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ME.BaseCore
{
    public class ExcelExporter
    {
        #region IExcelExporter 成员
        private HSSFWorkbook hssfworkbook = null;

        public bool ExportToExcel(string excelTemplateFilePath, string targetExcelFilePath, DataTable[] datas)
        {
            bool bResult = true;
            try
            {
                InitializeWorkbook(excelTemplateFilePath);
                int i = 0;
                foreach (DataTable data in datas)
                {
                    if (string.IsNullOrEmpty(data.TableName))
                    {
                        WriteTotargetExcel(data, new ExcelParparameter() { SheetName = "Sheet" + (++i), StartColoum = 0, StartRow = 1 });
                    }
                    else
                    {
                        WriteTotargetExcel(data, new ExcelParparameter() { SheetName = data.TableName, StartColoum = 0, StartRow = 1 });
                    }
                }
                WriteToFile(targetExcelFilePath);
            }
            catch (Exception ee)
            {
                string a = ee.ToString();
                bResult = false;
            }
            finally
            {

            }
            return bResult;
        }

        /// <summary>
        /// 读取模板excel到nopi对象里边进行初始化
        /// </summary>
        /// <param name="excelTemplateFilePath">传入模板路径</param>
        private void InitializeWorkbook(string excelTemplateFilePath)
        {

            FileStream file = new FileStream(excelTemplateFilePath, FileMode.Open, FileAccess.Read);

            hssfworkbook = new HSSFWorkbook(file);

            //DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            //dsi.Company = "OysSoft";
            //hssfworkbook.DocumentSummaryInformation = dsi;

            //SummaryInformation si = PropertySetFactory.CreateSummaryInformation();

            //hssfworkbook.SummaryInformation = si;
        }
        /// <summary>
        /// 写入到对应的目标模板
        /// </summary>
        /// <param name="data">需要写入的数据</param>
        private void WriteTotargetExcel(DataTable data, ExcelParparameter ExcelPars)
        {
            //string NUMBER_FORMAT = " #,##0.00 ";
            //string DATE_FORMAT = " yyyy年m月d日";
            //string PERCENT_FORMAT = "0.00%";
            string DATE_FORMATALL = " yyyy年m月d日 h:m";
            string DOTNUM_FROMAT = "0.000";
            //HSSFSheet sheetTemp = hssfworkbook.GetSheet(ExcelPars.SheetName);
            var sheetTemp = hssfworkbook.GetSheet(ExcelPars.SheetName);

            sheetTemp = sheetTemp ?? hssfworkbook.CreateSheet(ExcelPars.SheetName);

            int rowNumFirst = 0;
            //HSSFRow rowFirst = sheetTemp.GetRow(rowNumFirst);
            var rowFirst = sheetTemp.GetRow(rowNumFirst);
            rowFirst = rowFirst ?? sheetTemp.CreateRow(rowNumFirst);
            int dcIndex = 0;
            foreach (DataColumn dc in data.Columns)
            {
                //HSSFCell cell = rowFirst.GetCell(dcIndex);
                var cell = rowFirst.GetCell(dcIndex);
                cell = cell ?? rowFirst.CreateCell(dcIndex);
                dcIndex++;
                cell.SetCellValue(dc.ColumnName);
            }

            string[] arrStr;
            //首先创建要写入数据的行和列
            for (int i = 0; i < data.Rows.Count; i++)
            {
                //modified by lcl 
                int rowNum = i + ExcelPars.StartRow;
                //HSSFRow row = sheetTemp.GetRow(rowNum);
                var row = sheetTemp.GetRow(rowNum);
                row = row ?? sheetTemp.CreateRow(rowNum);

                ////HSSFCellStyle cellStyle = hssfworkbook.CreateCellStyle();
                var cellStyle = hssfworkbook.CreateCellStyle();


                for (int j = 0; j < data.Columns.Count; j++)
                {
                    int colNum = j + ExcelPars.StartColoum;
                    //HSSFCell cell = row.GetCell(colNum);
                    var cell = row.GetCell(colNum);
                    cell = cell ?? row.CreateCell(colNum);



                    ////cellStyle.Alignment = HSSFCellStyle.ALIGN_LEFT;
                    ////cellStyle.VerticalAlignment = HSSFCellStyle.VERTICAL_TOP;



                    switch (data.Columns[j].DataType.FullName)
                    {
                        case "System.Int32":
                            {

                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(Convert.ToInt32(data.Rows[i][j]));
                                break;
                            }
                        case "System.Decimal":
                            {

                                cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat(DOTNUM_FROMAT);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(Convert.ToDouble(data.Rows[i][j].ToString()));
                                break;
                            }
                        case "System.Double":
                            {

                                cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat(DOTNUM_FROMAT);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(Convert.ToDouble(data.Rows[i][j].ToString()));
                                break;
                            }
                        case "System.Boolean":
                            {
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(Convert.ToBoolean(data.Rows[i][j]));
                                break;
                            }
                        case "System.DateTime":
                            {
                                sheetTemp.SetColumnWidth(j + ExcelPars.StartColoum, 256 * 20);
                                //HSSFDataFormat format = hssfworkbook.CreateDataFormat();
                                var format = hssfworkbook.CreateDataFormat();
                                cellStyle.DataFormat = format.GetFormat(DATE_FORMATALL);

                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(Convert.ToDateTime(data.Rows[i][j]));
                                break;
                            }

                        case "System.String":
                            {
                                //cell.CellStyle = cellStyle;
                                //cell.SetCellValue(data.Rows[i][j].ToString());
                                try
                                {
                                    string v = data.Rows[i][j].ToString().Trim();
                                    if (string.IsNullOrEmpty(v))
                                    {
                                        cell.SetCellValue(v);
                                    }
                                    else
                                    {
                                        arrStr = v.Split(':');
                                        if (arrStr.Length >= 2)
                                        {
                                            cell.SetCellValue(v);
                                            //cell.SetCellValue(data.Rows[i][j].ToString());
                                        }
                                        else
                                        {
                                            arrStr = v.Split('.');
                                            if (arrStr.Length >= 2)
                                            {
                                                cell.SetCellValue(v);
                                                //cell.SetCellValue(data.Rows[i][j].ToString());
                                            }
                                            else if (v.Contains("-"))
                                            {
                                                cell.SetCellValue(v);
                                            }
                                            else if (v.Contains("F"))
                                            {
                                                cell.SetCellValue(v);
                                            }
                                            else if (v.Contains("C"))
                                            {
                                                cell.SetCellValue(v);
                                            }
                                            else if (v.Contains("R"))
                                            {
                                                cell.SetCellValue(v);
                                            }
                                            else if (v.Contains("男"))
                                            {
                                                cell.SetCellValue(v);
                                            }
                                            else if (v.Contains("女"))
                                            {
                                                cell.SetCellValue(v);
                                            }
                                            else
                                            {
                                                //double value = 0;
                                                //if (v.Contains("%"))
                                                //{
                                                //    int idx = v.IndexOf("%");
                                                //    v = v.Remove(idx);
                                                //    value = double.Parse(v) / 100;
                                                //}
                                                //else
                                                //{
                                                //    value = double.Parse(v);
                                                //}
                                                //cell.SetCellValue(value);
                                                cell.SetCellValue(v);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    cell.SetCellValue(data.Rows[i][j].ToString());
                                }
                                break;
                            }
                        default:

                            break;

                    }



                }
            }
            sheetTemp.ForceFormulaRecalculation = true;

            if (data.TableName != "")
            {
                string filename = string.Format("{0}{1}.png", TempSavePngFile, data.TableName);

                byte[] bytes = System.IO.File.ReadAllBytes(filename);
                int pictureIdx = hssfworkbook.AddPicture(bytes, NPOI.SS.UserModel.PictureType.PNG);

                // Create the drawing patriarch.  This is the top level container for all shapes. 
                var patriarch = sheetTemp.CreateDrawingPatriarch();

                //add a picture
                HSSFClientAnchor anchor = new HSSFClientAnchor(0, 0, 1023, 0, 10, 5, 20, 15);
                var pict = patriarch.CreatePicture(anchor, pictureIdx);
            }
        }
        string TempSavePngFile = AppDomain.CurrentDomain.BaseDirectory + "TempPng" + Path.DirectorySeparatorChar;

        /// <summary>
        /// 通过nopi对象写入到目标模板
        /// </summary>
        /// <param name="targetExcelFilePath">目标模板路径</param>
        private void WriteToFile(string targetExcelFilePath)
        {
            //Write the stream data of workbook to the root directory
            FileStream file = new FileStream(targetExcelFilePath, FileMode.Create);
            //hssfworkbook.
            var sheetTemp = hssfworkbook.GetSheet("Sheet1");
            if (sheetTemp != null)
            {
                int index = hssfworkbook.GetSheetIndex("Sheet1");
                hssfworkbook.RemoveSheetAt(index);
            }


            hssfworkbook.Write(file);
            file.Close();
        }

        #endregion

        /// <summary>
        /// 将DataTable数据导出到Excel文件中(xlsx)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="filePath"></param>
        public static void TableToExcelForXLSX(DataTable dt, string filePath, string sheetName = "sheet1")
        {
            try
            {
                var xssfworkbook = new NPOI.XSSF.UserModel.XSSFWorkbook();
                var sheet = xssfworkbook.CreateSheet(sheetName);

                //表头
                var row = sheet.CreateRow(0);
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    var cell = row.CreateCell(i);
                    cell.SetCellValue(dt.Columns[i].ColumnName);
                }

                //数据
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    var row1 = sheet.CreateRow(i + 1);
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        var cell = row1.CreateCell(j);
                        cell.SetCellValue(dt.Rows[i][j].ToString());
                    }
                }

                //转为字节数组
                var stream = new MemoryStream();
                xssfworkbook.Write(stream);
                var buf = stream.ToArray();

                //保存为Excel文件
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(buf, 0, buf.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        /// <summary>
        /// 导出Excel(多Sheet)
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static void DataSetToExcel(DataSet ds, string filePath)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet(ds.Tables[i].TableName);
                var rowHeader = sheet.CreateRow(0);
                for (var j = 0; j < ds.Tables[i].Columns.Count; j++)
                {
                    var cell = rowHeader.CreateCell(j);
                    cell.SetCellValue(ds.Tables[i].Columns[j].ColumnName);
                }
                for (var k = 0; k < ds.Tables[i].Rows.Count; k++)
                {
                    var row = sheet.CreateRow(k + 1);
                    for (var j = 0; j < ds.Tables[i].Columns.Count; j++)
                    {
                        var cell = row.CreateCell(j);
                        cell.SetCellValue(ds.Tables[i].Rows[k][j].ToString());
                    }
                }
            }

            var stream = new MemoryStream();
            workbook.Write(stream);
            var buf = stream.ToArray();

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buf, 0, buf.Length);
                fs.Flush();
                fs.Close();
            }
        }

        /// <summary>
        /// 导出Excel(多Sheet)
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="Path">存储路径</param>
        /// <param name="strFirstLine">第一行</param>
        /// <param name="strHeader">表头</param>
        /// <returns></returns>
        public static bool DataSetToExcel(DataSet ds, string Path, List<Dictionary<string, string[]>> listDictionary)
        {
            bool result = false;
            FileStream fs = null;
            XSSFWorkbook workbook = new XSSFWorkbook();
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet(ds.Tables[i].TableName);

                XSSFCellStyle dateStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                XSSFDataFormat format = (XSSFDataFormat)workbook.CreateDataFormat();
                dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

                int rowIndex = 0;

                #region 新建表，填充表头，填充列头，样式
                if (rowIndex == 0)
                {
                    #region 列头及样式
                    {
                        XSSFRow headerRow = (XSSFRow)sheet.CreateRow(0);
                        XSSFCellStyle headStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                        //headStyle.Alignment = CellHorizontalAlignment.CENTER;
                        XSSFFont font = (XSSFFont)workbook.CreateFont();
                        font.FontHeightInPoints = 10;
                        font.Boldweight = 700;
                        headStyle.SetFont(font);
                    }
                    #endregion

                    rowIndex = 2;
                }
                #endregion
                if (listDictionary != null && listDictionary.Count > 0)
                {
                    //第一行
                    var rowTitle = sheet.CreateRow(0);
                    var rowHeader = sheet.CreateRow(1);
                    for (int k = 0; k < listDictionary.Count; k++)
                    {
                        var cellTitle = rowTitle.CreateCell(listDictionary[k].Values.First().Length * k + k);
                        cellTitle.SetCellValue(listDictionary[k].Keys.First());
                        //CellRangeAddress（开始行、结束行、开始列、结束列）
                        sheet.AddMergedRegion(new CellRangeAddress(0, 0, listDictionary[k].Values.First().Length * k + k, listDictionary[k].Values.First().Length * (k + 1) + k - 1)); //合并
                        //表头
                        if (listDictionary[k].Values.First().Length > 0)
                        {
                            for (var j = 0; j < listDictionary[k].Values.First().Length; j++)
                            {
                                var cell = rowHeader.CreateCell(listDictionary[k].Values.First().Length * k + k + j);
                                cell.SetCellValue(listDictionary[k].Values.First()[j]);
                            }
                        }
                        else
                        {
                            for (var j = 0; j < ds.Tables[i].Columns.Count; j++)
                            {
                                var cell = rowHeader.CreateCell(j);
                                cell.SetCellValue(ds.Tables[i].Columns[j].ColumnName);
                            }
                        }
                    }

                    foreach (DataRow row in ds.Tables[i].Rows)
                    {
                        XSSFRow dataRow = (XSSFRow)sheet.CreateRow(rowIndex);
                        #region 填充内容
                        foreach (DataColumn column in ds.Tables[i].Columns)
                        {
                            XSSFCell newCell = (XSSFCell)dataRow.CreateCell(column.Ordinal);
                            string type = row[column].GetType().FullName.ToString();
                            newCell.SetCellValue(GetValue(row[column].ToString(), type));
                        }
                        #endregion
                        rowIndex++;
                    }
                }
            }

            using (fs = File.OpenWrite(Path))
            {
                workbook.Write(fs);//向打开的这个xls文件中写入数据
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Dataset结果集转换成excel，一个Dataset一个Sheet页
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="filePath"></param>
        public static void DataSetToExcelOnSheet(List<DataSet> ds,string filePath)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();

            foreach (var item in ds)
            {
                XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet(item.DataSetName);
                int rowIndex = 0;//总行
                for (int i = 0; i < item.Tables.Count; i++)
                {
                    var table = item.Tables[i];
                    //表格标题
                    var rowTitle = sheet.CreateRow(rowIndex);
                    var cellTitle = rowTitle.CreateCell(0);
                    cellTitle.SetCellValue(table.TableName);
                    //表格列头
                    var rowHeader = sheet.CreateRow(++rowIndex);
                    for (var j = 0; j < table.Columns.Count; j++)
                    {
                        var cell = rowHeader.CreateCell(j);
                        cell.SetCellValue(table.Columns[j].ColumnName);
                    }
                    for (var k = 0; k < table.Rows.Count; k++)
                    {
                        var row = sheet.CreateRow(++rowIndex);
                        for (var j = 0; j < table.Columns.Count; j++)
                        {
                            var cell = row.CreateCell(j);
                            cell.SetCellValue(table.Rows[k][j].ToString());
                        }
                    }
                    rowIndex = rowIndex + 5;//每个table间隔两行
                }
            }

            var stream = new MemoryStream();
            workbook.Write(stream);
            var buf = stream.ToArray();

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buf, 0, buf.Length);
                fs.Flush();
                fs.Close();
            }
        }

        /// <summary>
        /// 导出Excel(多Sheet)
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <param name="Path">存储路径</param>
        /// <param name="strFirstLine">第一行</param>
        /// <param name="strHeader">表头</param>
        /// <returns></returns>
        public static bool DataSetToExcel(DataSet ds, string Path, List<ExportExcelTableInfo> exportExcelTableList)
        {
            bool result = false;
            FileStream fs = null;
            XSSFWorkbook workbook = new XSSFWorkbook();
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                //获取Sheet中表标题和表头信息
                var sheetList = exportExcelTableList.Where(x => x.Sheet == i + 1).ToList();

                XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet(ds.Tables[i].TableName);

                XSSFCellStyle dateStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                XSSFDataFormat format = (XSSFDataFormat)workbook.CreateDataFormat();
                dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");

                int rowIndex = 0;

                #region 新建表，填充表头，填充列头，样式
                if (rowIndex == 0)
                {
                    #region 列头及样式
                    {
                        XSSFRow headerRow = (XSSFRow)sheet.CreateRow(0);
                        XSSFCellStyle headStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                        //headStyle.Alignment = CellHorizontalAlignment.CENTER;
                        XSSFFont font = (XSSFFont)workbook.CreateFont();
                        font.FontHeightInPoints = 10;
                        font.Boldweight = 700;
                        headStyle.SetFont(font);
                    }
                    #endregion

                    rowIndex = 2;
                }
                #endregion
                if (sheetList != null && sheetList.Count > 0)
                {
                    //第一行
                    var rowTitle = sheet.CreateRow(0);
                    var rowHeader = sheet.CreateRow(1);
                    for (int k = 0; k < sheetList.Count; k++)
                    {
                        var cellTitle = rowTitle.CreateCell(sheetList[k].Header.Length * k + k);
                        cellTitle.SetCellValue(sheetList[k].Title);
                        //CellRangeAddress（开始行、结束行、开始列、结束列）
                        sheet.AddMergedRegion(new CellRangeAddress(0, 0, sheetList[k].Header.Length * k + k, sheetList[k].Header.Length * (k + 1) + k - 1)); //合并
                        //表头
                        if (sheetList[k].Header.Length > 0)
                        {
                            for (var j = 0; j < sheetList[k].Header.Length; j++)
                            {
                                var cell = rowHeader.CreateCell(sheetList[k].Header.Length * k + k + j);
                                cell.SetCellValue(sheetList[k].Header[j]);
                            }
                        }
                        else
                        {
                            for (var j = 0; j < ds.Tables[i].Columns.Count; j++)
                            {
                                var cell = rowHeader.CreateCell(j);
                                cell.SetCellValue(ds.Tables[i].Columns[j].ColumnName);
                            }
                        }
                    }

                    foreach (DataRow row in ds.Tables[i].Rows)
                    {
                        XSSFRow dataRow = (XSSFRow)sheet.CreateRow(rowIndex);
                        #region 填充内容
                        foreach (DataColumn column in ds.Tables[i].Columns)
                        {
                            XSSFCell newCell = (XSSFCell)dataRow.CreateCell(column.Ordinal);
                            string type = row[column].GetType().FullName.ToString();
                            newCell.SetCellValue(GetValue(row[column].ToString(), type));
                        }
                        #endregion
                        rowIndex++;
                    }
                }
            }

            using (fs = File.OpenWrite(Path))
            {
                workbook.Write(fs);//向打开的这个xls文件中写入数据
                result = true;
            }
            return result;
        }

        private static string GetValue(string cellValue, string type)
        {
            object value = string.Empty;
            switch (type)
            {
                case "System.String"://字符串类型
                    value = cellValue;
                    break;
                case "System.DateTime"://日期类型
                    System.DateTime dateV;
                    System.DateTime.TryParse(cellValue, out dateV);
                    value = dateV;
                    break;
                case "System.Boolean"://布尔型
                    bool boolV = false;
                    bool.TryParse(cellValue, out boolV);
                    value = boolV;
                    break;
                case "System.Int16"://整型
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                    int intV = 0;
                    int.TryParse(cellValue, out intV);
                    value = intV;
                    break;
                case "System.Decimal"://浮点型
                case "System.Double":
                    double doubV = 0;
                    double.TryParse(cellValue, out doubV);
                    value = doubV;
                    break;
                case "System.DBNull"://空值处理
                    value = string.Empty;
                    break;
                default:
                    value = string.Empty;
                    break;
            }
            return value.ToString();
        }
    }
    public class ExcelParparameter
    {
        /// <summary>
        /// 设置开始输入数据的行
        /// </summary>
        public int StartRow = 0;
        /// <summary>
        /// 设置开始输入数据的列
        /// </summary>
        public int StartColoum = 0;
        /// <summary>
        /// 设置要读取页签的名称
        /// </summary>
        public string SheetName = "Sheet1";
    }

}
