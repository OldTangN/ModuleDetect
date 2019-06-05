﻿//https://sun747.visualstudio.com/Mkjc/_git/Mkjc

using System;

public class Scheme
{
//*************************************研发定义区**************************************

    //实现方案设置返回值，转化为显示名称
    public string[] CommModelName = new string[6]
    { "","单相载波", "三相载波", "I型集中器载波", "I型集中器GPRS", "II型集中器GPRS"};

    //实现将ViewIndex中对应值，转换为检测项名称，显示在桌面模块控件中
    public string[] CheckTypeName = new string[17] 
    { "载波通信", "载波衰减", "静态功耗", "动态功耗", "电源波动", "网口测试", "GPRS测试", "RESET管脚",
    //   0             1          2           3           4           5           6           7
      "/SET管脚", "STA管脚", "STATE测试","EVENT管脚", "加热控制", "ON/OFF", "工位重启", "下行通信", "HPLC ID"};
    //   8             9          10          11          12          13       14          15           16

    //将状态码转换为相应的测试项编号
    public int[] StationCode = new int[19]//定义状态码对应类型序号，第0个保留，0xFF表示无对应
    {0xFF,0xFF,0xFF,0xFF,0xFF, 0,  1,  5,  6,  4,  10,  7,    8,  9,   11,  12,  13,   14,  3};
    //0     1   2     3    4   5   6   7   8   9   10   11   12   13   14   15   16    17   18

    //通过端口序号获得对应设备
    public int[] SchemeUart = new int[23]//端口配置,序号为串口序号，0=表示关闭，1-10表示工位，11=功耗测试仪，12-直流源，13-抄控器
    //{ 0,  12, 11, 0,  0,  0,  0, 13, 14, 15, 16, 17,  1,  2,  3,  4,  5,  6,  7,  8,  9,  10,  0};//正常配置
    { 0,  12, 11, 0,  0,  0,  0, 13, 14, 15, 16, 17,  1,  2,  3,  4,  5,  10,  9,  8,  7,  6,  0};//青海配置
    //0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16   17   18  19  20  21  22 //串口号

    //串口奇偶校验表
    public int[] SchemeUartCheck = new int[23]//端口配置，奇偶校验  0=无校验，1=奇校验，2=偶校验
    { 0,  0,  0,  0,  0, 0,  0,  2,   2,  2,  2,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0};
    //0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16   17   18  19  20  21  22

    //抄控器对应IO控制器序号
    public int[] ChaoKongIOIndex = new int[5]//序号为抄控器序号，-1=无抄控器 其他表示IO控制序号
    //{ 2,  4,  6,  8,  10};
    { 1,  3,  5,  7,  9};
    //0   1   2   3   4   
    
    //抄控器序号对应通信端口号
    public int[] ChaoKongPortNum = new int[5]//序号为抄控器序号，-1=无抄控器 其他表示通信端口号
    //{ 12,  8,  9,  10,  11};
    { 7,  8,  9,  10,  11};
    //0   1   2    3    4  

    public int[] gzmoduleUartNum = new int[10]//工位对应端口号
    //{ 12, 13, 14, 15, 16, 17, 18, 19, 20, 21};//正常配置，天津配置
    { 12, 13, 14, 15, 16, 21, 20, 19, 18, 17};//青海特殊配置
    //0   1   2   3   4   5   6   7   8   9

    //窄带载波模块类型对应静态标准值
    public decimal[] ZDModuleStaticPW = new decimal[6]
        { 0, 0.25M, 0.35M, 1M, 1M, 1M};
    //窄带载波模块类型对应动态标准值
    public decimal[] ZDModuleDynamicPw = new decimal[6]
        { 0, 1.5M, 2.5M, 6M, 6M, 6M};

    //HPLC载波模块类型对应静态标准值
    public decimal[] KDModuleStaticPW = new decimal[6]
        { 0, 0.4M, 0.55M, 1M, 1M, 1M};
    //HPLC载波模块类型对应动态标准值
    public decimal[] KDModuleDynamicPw = new decimal[6]
        { 0, 2M, 3M, 6M, 6M, 6M};

    //模块类型对应静态标准值初始化区
    public decimal[] ModuleStaticPW = new decimal[6]
    { 0, 0M, 0M, 0M, 0M, 0M};

    //模块类型对应动态标准值初始化区
    public decimal[] ModuleDynamicPw = new decimal[6]
    { 0, 0M, 0M, 0M, 0M, 0M};

    public decimal[] GwPWValue = new decimal[10]
    { 0.108M, 0.108M, 0.108M, 0.108M, 0.108M, 0.108M, 0.108M, 0.108M, 0.108M, 0.108M};

    //*************************************客户定义区**************************************

    //控制检测项，用于将需要的测试项重新排序进行显示
    public int[,] SchemeCheckItem = new int[6, 17]
    {
        /*
    {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1 },
    //{1,2,3,4,5,0,0,6,   0,0,0,7,0,0,0,0 },//单相测试项
    {0,1,2,3,4,7,11,-1,   -1,-1,-1,-1,-1,-1,-1,-1 },//单相测试项
    {0,1,2,3,4,7,11,-1,   -1,-1,-1,-1,-1,-1,-1,-1 },//三相测试项
    {0,1,2,3,4,7,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1 },//I型载波
    {6,2,3,4,5,12,13,-1,   -1,-1,-1,-1,-1,-1,-1,-1 },//GPRS载波
    //{0,0,2,3,4,5,1,0,   0,0,0,0,6,7,0,0 } //GPRS载波
    {6,2,3,4,5,12,13,-1,   -1,-1,-1,-1,-1,-1,-1,-1 } //GPRS载波
    */
         
        {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1,-1 },
        {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1,-1 },//单相测试项
        {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1,-1 },//三相测试项
        {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1,-1 },//I型载波
        {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1,-1 },//GPRS载波
        {-1,-1,-1,-1,-1,-1,-1,-1,   -1,-1,-1,-1,-1,-1,-1,-1,-1 } //GPRS载波

    };

    /// <summary>
    ///工位对应模块类型 0=表示关闭 1-单项 2-三相 3-I型集中器载波 4-I型GPRS 5-II型GPRS
    /// </summary>
    public int[] gzmoduleType = new int[10]//工装配置,序号为工装序号，0=表示关闭 1-单项 2-三相 3-I型集中器载波 4-I型GPRS 5-II型GPRS
    //{ 0,  0,  0,  0,  0,  0,  0,  0,  0,  0};
    { 1,  2,  3,  4,  5,  1,  2,  3,  4,  5};
    //0   1   2   3   4   5   6   7   8   9 

    /// <summary>
    /// 工位对应抄控器端口 通过配置界面选择 -1=无抄控器 其他表示抄控器号。例如 0-东软, 1-鼎信，2-宽带。。。
    /// </summary>
    public int[] gzChaoKongType = new int[10]//工装配置,序号为工装序号，-1=无抄控器 其他表示抄控器号
    { 2,  2,  2,  2,  2,  2,  2,  2,  2,  2};
    //0   1   2   3   4   5   6   7   8   9 

    /// <summary>
    /// 工位对应产品类型, 0-默认，1-I型HPLC， 2-东软I型HPLC  3-东软采集器模块 4-窄带698模块
    /// </summary>
    public int[] gzProductId = new int[10]
    { 3,  3,  3,  3,  3,  3,  3,  3,  3,  3};
    //0   1   2   3   4   5   6   7   8   9 

public Scheme()
	{
	}
}
