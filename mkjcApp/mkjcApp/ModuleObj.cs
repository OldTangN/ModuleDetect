using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mkjcApp
{
    public class ModuleObj
    {
        public int CheckStep;
        public int TotalTimeOut=0;
        public int IndividualTimeOut = 0;
        public int InFiFo = 0;
        public int ModuleReady = 0;
        public int CurrentCheckItem = -1;//-1=没有检测项 0-15为正在检测项
        public int TotalResult = 0;//0-没结论 1=正确 2=错误
        public int ModuleType = 0;//工位方案，模块类型
        public int DCPowerCheckStation = 0;
        public int CheckItemTaskLock = 0; //在该标志非零状态下，调度未超时情况下不能停掉该任务
        public int InitSchemeOk = 0; //该标志用于表示该工位是否完成方案初始化,0=表示未初始化
        public String SoftVersion ="";

        public String barCode="";
        public int checktypenum = 17;

        //显示与检测内容对应关系配置项，与Scheme中CheckTypeName对应
        public int[] CheckItemToViewIndex = new int[17]//数值表示该测试项对应的界面显示序号
             //{1,2,3,4,5,0,0,6,   0,0,0,7,0,0,0,0 };//单三相测试项
             //{1,2,3,4,5,0,0,6,   0,0,0,0,0,0,0,0 };//I型载波
             {-1,-1,2,3,4,5,1,-1,   -1,-1,-1,-1,6,7,-1,-1,-1 };//GPRS载波

        //方案配置项，-1=不参与测试 其他=表示该测试项参与测试，数值为与Scheme中CheckTypeName对应检测项，先后顺序为检测顺序
        public int[] SchemeCheckItem = new int[17]
            {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1,-1 };//初始化为无检测项

        //检测结果缓冲区
        public int[] checkresult = new int[17]//序号为状态码，检测类型显示控制，0=无结果，1=测试通过，2=测试失败
        {0,0,0,0,0,0,0,0,   0,0,0,0,0,0,0,0,0 };
        //检测结果对应数值
        public decimal[] resultValue = new decimal[17]//序号为状态码，数值为检测结果相关数据
        {0,0,0,0,0,0,0,0,   0,0,0,0,0,0,0,0,0 };

        public void IniModuleObj()
        {
            Array.Clear(checkresult, 0, checkresult.Length);
            Array.Clear(resultValue, 0, resultValue.Length);
            ModuleReady = 0;
            InFiFo = 0;
            CurrentCheckItem = -1;
            TotalResult = 0;
        }
        public void SetResult(int index, int result, int value)
        {
            checkresult[index] = result;
            resultValue[index] = value;
        }
        public void SetSchemeCheckItem(int[] parm, int datalen)
        {
            int len = datalen;
            if (len > checktypenum) len = checktypenum;
            //Array.Clear(SchemeCheckItem, -1, SchemeCheckItem.Length);
            //Array.Copy(parm, SchemeCheckItem, datalen);
            for (int i = 0; i < checktypenum; i++)
            {
                SchemeCheckItem[i] = parm[i];
            }
            SetViewIndex();
        }
        public void SetViewIndex()
        {
            for (int i = 0; i < checktypenum; i++)
            {
                CheckItemToViewIndex[i] = -1;
            }
            //Array.Clear(CheckItemToViewIndex, -1, 16);
            for (int i = 0; i < checktypenum; i++)
            {
                if ((SchemeCheckItem[i] >= 0) && (SchemeCheckItem[i] < checktypenum))
                {
                    CheckItemToViewIndex[SchemeCheckItem[i]] = i;
                }
            }
        }
    }
}
