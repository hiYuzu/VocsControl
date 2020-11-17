using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using VocsControlHELP.model;

namespace VocsControlHELP.util
{
    public class Export
    {
        public static bool ExportExcel(List<DataModel> list)
        {
            bool success;
            try
            {
                //创建workbook
                var workbook = new HSSFWorkbook();
                //创建一个sheet
                var sheet = workbook.CreateSheet();
                //标题行
                var headerRow = sheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("时间");
                headerRow.CreateCell(1).SetCellValue("点火状态");
                headerRow.CreateCell(2).SetCellValue("总烃");
                headerRow.CreateCell(3).SetCellValue("甲烷");
                headerRow.CreateCell(4).SetCellValue("非甲烷总烃");
                //内容
                var index = 1;
                foreach (DataModel item in list)
                {
                    string state = "";
                    switch (item.State)
                    {
                        case 0:
                            state = "关";
                            break;
                        case 1:
                            state = "开";
                            break;
                        case 2:
                            state = "点火中...";
                            break;
                        default:
                            break;
                    }
                    var newRow = sheet.CreateRow(index);
                    newRow.CreateCell(0).SetCellValue(item.Time);
                    newRow.CreateCell(1).SetCellValue(state);
                    newRow.CreateCell(2).SetCellValue(item.Zt);
                    newRow.CreateCell(3).SetCellValue(item.Jw);
                    newRow.CreateCell(4).SetCellValue(item.Zt - item.Jw);
                    index++;
                }
                //创建文件
                FileStream file = new FileStream(Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xls", FileMode.CreateNew, FileAccess.Write);
                //创建IO流
                MemoryStream ms = new MemoryStream();
                //将Excel写入stream流
                workbook.Write(ms);
                byte[] bytes = ms.ToArray();
                file.Write(bytes, 0, bytes.Length);
                file.Flush();
                //释放资源
                ms.Close();
                ms.Dispose();

                file.Close();
                file.Dispose();

                workbook.Close();

                success = true;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return success;
        }
    }
}
