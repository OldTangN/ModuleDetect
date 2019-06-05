using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mkjcApp.usermodel
{
    public class TestResultModel
    {
        /// <summary>
        /// 方案名称
        /// </summary>
        public string scheme_name { get; set; }

        /// <summary>
        /// 检测项名称
        /// </summary>
        public string term { get; set; }
        /// <summary>
        /// 检测值
        /// </summary>
        public string result_data { get; set; }
        /// <summary>
        /// 结论 1=合格 2=不合格 3=未检测
        /// </summary>
        public string result { get; set; }

        public string module_barcode { get; set; }
    }
}
