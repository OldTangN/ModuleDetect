using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mkjcApp.usermodel
{
    public class EquipModel
    {
        public string DETECT_TASK_NO { get; set; }

        public string BAR_CODE { get; set; }

        /// <summary>
        /// 是否完成了检测 默认是0 没经过检测 1检测完成
        /// </summary>
        public string YQ_FINISH_FLAG { get; set; }

        /// <summary>
        /// 1=合格品
        /// </summary>
        public string YQ_RSLT_FLAG { get; set; }

        /// <summary>
        /// 测试员
        /// </summary>
        public string YQ_TEST_OPERATOR { get; set; }

        /// <summary>
        /// 检测时间
        /// </summary>
        public string YQ_date { get; set; }

        /// <summary>
        /// hplc验证，01成功 02失败 03未检测
        /// </summary>
        public string HPLC_CERT_CONC_CODE { get; set; }

        /// <summary>
        /// 检测工位号 对应test_result的列station_barcode
        /// </summary>
        public string POSITION_NO { get; set; }
    }
}
