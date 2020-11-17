using System.Collections.Generic;
using System.Windows.Controls;
using VocsControlHELP.model;
using VocsControlHELP.util;

namespace VocsControlUI
{
    /// <summary>
    /// DataPage.xaml 的交互逻辑
    /// </summary>
    public partial class DataPage : Page
    {
        public DataPage()
        {
            InitializeComponent();
            DisplayData();
        }

        private void DisplayData()
        {
            List<DataModel> datas = SQLiteHelper.QueryByDateTime(DateModel.StartTime, DateModel.StopTime);
            if(datas != null)
            {
                foreach (DataModel data in datas)
                {
                    string state = "";
                    switch (data.State)
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
                    todayDataGrid.Items.Add(new DataGridRow
                    {
                        Item = new
                        {
                            TodayTime = data.Time,
                            TodayState = state,
                            TodayZt = data.Zt,
                            TodayJw = data.Jw,
                            TodayZt_jw = data.Zt - data.Jw
                        }
                    });
                }
            }
        }
    }
}
