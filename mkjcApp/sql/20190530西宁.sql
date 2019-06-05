-- MySQL dump 10.13  Distrib 5.7.12, for Win64 (x86_64)
--
-- Host: localhost    Database: plc_scheme
-- ------------------------------------------------------
-- Server version	5.7.17-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `count`
--

DROP TABLE IF EXISTS `count`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `count` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `style_id` int(11) DEFAULT NULL,
  `actual_count` int(11) DEFAULT NULL,
  `plan_count` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `count`
--

LOCK TABLES `count` WRITE;
/*!40000 ALTER TABLE `count` DISABLE KEYS */;
INSERT INTO `count` VALUES (1,1,1,20),(2,2,3,30),(3,3,1,25),(4,4,1,35),(5,5,1,30);
/*!40000 ALTER TABLE `count` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `module`
--

DROP TABLE IF EXISTS `module`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `module` (
  `id` int(11) NOT NULL,
  `module_num` int(11) DEFAULT NULL,
  `module_style` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `module`
--

LOCK TABLES `module` WRITE;
/*!40000 ALTER TABLE `module` DISABLE KEYS */;
/*!40000 ALTER TABLE `module` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `module_style`
--

DROP TABLE IF EXISTS `module_style`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `module_style` (
  `id` int(11) NOT NULL,
  `module_style` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `module_style`
--

LOCK TABLES `module_style` WRITE;
/*!40000 ALTER TABLE `module_style` DISABLE KEYS */;
INSERT INTO `module_style` VALUES (1,'单相载波'),(2,'三相载波'),(3,'I型集中器载波'),(4,'I型集中器GPRS'),(5,'II型集中器GPRS');
/*!40000 ALTER TABLE `module_style` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mt_detect_out_equip`
--

DROP TABLE IF EXISTS `mt_detect_out_equip`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mt_detect_out_equip` (
  `DETECT_TASK_NO` varchar(32) NOT NULL,
  `SYS_NO` varchar(16) DEFAULT NULL,
  `IO_TASK_NO` varchar(32) NOT NULL,
  `ARRIVE_BATCH_NO` varchar(32) DEFAULT NULL,
  `EQUIP_CATEG` varchar(8) DEFAULT NULL,
  `BAR_CODE` varchar(32) NOT NULL,
  `BOX_BAR_CODE` varchar(32) DEFAULT NULL,
  `PILE_NO` varchar(256) DEFAULT NULL,
  `REDETECT_FLAG` varchar(8) DEFAULT NULL COMMENT '见附录I：抽检复检标识',
  `PLATFORM_NO` varchar(32) DEFAULT NULL COMMENT '站台号',
  `EMP_NO` varchar(32) DEFAULT NULL COMMENT '人员编号',
  `PLATFORM_TYPE` varchar(32) DEFAULT NULL COMMENT '见附录I：台体类型',
  `WRITE_DATE` varchar(32) DEFAULT NULL,
  `HANDLE_FLAG` varchar(8) DEFAULT NULL COMMENT '处理标记',
  `HANDLE_DATE` varchar(32) DEFAULT NULL COMMENT '处理时间',
  `YQ_FINISH_FLAG` varchar(8) DEFAULT NULL COMMENT '是否完成了检测 默认是0 没经过检测 1检测完成',
  `YQ_RSLT_FLAG` varchar(8) DEFAULT NULL COMMENT '是否是合格品',
  `YQ_TEST_OPERATOR` varchar(32) DEFAULT NULL COMMENT '测试人',
  `YQ_VERIFY_OPERATOR` varchar(32) DEFAULT NULL COMMENT '复检人',
  `YQ_temperature` varchar(8) DEFAULT NULL COMMENT '温度',
  `YQ_humidity` varchar(8) DEFAULT NULL COMMENT '湿度',
  `YQ_date` varchar(32) DEFAULT NULL,
  `UNPASS_REASON` varchar(45) DEFAULT NULL,
  `HPLC_CERT_CONC_CODE` varchar(8) DEFAULT NULL COMMENT 'hplc验证，01成功 02失败 03未检测',
  `POSITION_NO` varchar(16) DEFAULT NULL COMMENT '检测工位号 对应test_result的列station_barcode',
  PRIMARY KEY (`BAR_CODE`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='检定设备出库明细';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mt_detect_out_equip`
--

LOCK TABLES `mt_detect_out_equip` WRITE;
/*!40000 ALTER TABLE `mt_detect_out_equip` DISABLE KEYS */;
INSERT INTO `mt_detect_out_equip` VALUES ('3919052930736991','555','4419052930736993','2619031830736484','54','6330054005991802521023','','','','','00020088','','2019-05-29 14:27:30','0','','1','01','00020088','','','','2019/5/30 10:28:31','','01','181101010'),('3919052930736991','555','4419052930736993','2619031830736484','54','6330054005991802521122','','','','','00020088','','2019-05-29 14:27:30','0','','1','01','00020088','','','','2019/5/30 10:28:39','','01','181101009');
/*!40000 ALTER TABLE `mt_detect_out_equip` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mt_detect_rslt`
--

DROP TABLE IF EXISTS `mt_detect_rslt`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mt_detect_rslt` (
  `DETECT_TASK_NO` varchar(32) NOT NULL,
  `EQUIP_CATEG` varchar(8) DEFAULT NULL,
  `SYS_NO` varchar(16) DEFAULT NULL,
  `DETECT_EQUIP_NO` varchar(16) DEFAULT NULL,
  `DETECT_UNIT_NO` varchar(16) DEFAULT NULL,
  `POSITION_NO` varchar(16) DEFAULT NULL,
  `BAR_CODE` varchar(32) DEFAULT NULL,
  `DETECT_DATE` varchar(32) DEFAULT NULL COMMENT '检定时间',
  `CONC_CODE` varchar(8) DEFAULT NULL COMMENT '检定总结论',
  `INTUIT_CONC_CODE` varchar(8) DEFAULT NULL COMMENT '外观检查结论\n见附录I：结论',
  `BASICERR_CONC_CODE` varchar(8) DEFAULT NULL COMMENT '基本误差结论',
  `HPLC_CERT_CONC_CODE` varchar(8) DEFAULT NULL COMMENT 'HPLC芯片ID认证\nHPLC芯片ID认证结果，见附录Ⅰ：结论'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='检定/校准综合结论';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mt_detect_rslt`
--

LOCK TABLES `mt_detect_rslt` WRITE;
/*!40000 ALTER TABLE `mt_detect_rslt` DISABLE KEYS */;
/*!40000 ALTER TABLE `mt_detect_rslt` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mt_detect_task`
--

DROP TABLE IF EXISTS `mt_detect_task`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mt_detect_task` (
  `DETECT_TASK_NO` varchar(32) NOT NULL COMMENT '检定任务实体记录唯一性标识',
  `TASK_PRIO` varchar(32) DEFAULT '0' COMMENT '0~255，数值越小等级越高',
  `DETECT_MODE` varchar(8) DEFAULT NULL COMMENT '见附录I：检定方式',
  `SYS_NO` varchar(16) DEFAULT NULL COMMENT '生产线系统唯一标识（人工台时不使用该字段）',
  `ARRIVE_BATCH_NO` varchar(32) DEFAULT NULL COMMENT '到货批次号',
  `EQUIP_CATEG` varchar(8) DEFAULT NULL COMMENT '参见标准代码：设备类别',
  `MODEL_CODE` varchar(8) DEFAULT NULL COMMENT '依据设备类别参见标准代码：电能表（互感器等）设备型号',
  `ERP_BATCH_NO` varchar(32) DEFAULT NULL COMMENT '物料号',
  `SCHEMA_ID` varchar(16) DEFAULT NULL COMMENT '关联检定方案的检定方案标识 检定方案标识',
  `REDETECT_SCHEMA` varchar(16) DEFAULT NULL COMMENT '关联检定方案的检定方案标识，如果没有复检方案-1',
  `REDETECT_FLAG` varchar(8) DEFAULT NULL COMMENT '第一次检定完成后，是否继续再次复检，见附录I：是否标志\n是否复检',
  `REDETECT_QTY` varchar(32) DEFAULT '0' COMMENT '复检数量',
  `EQUIP_QTY` varchar(32) DEFAULT '0' COMMENT '本次检定任务总共计划需要检定的设备数量',
  `PILE_QTY` varchar(32) DEFAULT '0' COMMENT '本次检定任务垛总数，检定系统每次按垛申请检定设备和空箱',
  `EXEC_RESP_NAME` varchar(256) DEFAULT NULL COMMENT '检验人 检定任务执行负责人姓名',
  `APPR_NAME` varchar(64) DEFAULT NULL COMMENT '核验人姓名',
  `IS_AUTO_SEAL` varchar(32) DEFAULT '0' COMMENT '检定任务下发到流水线后，流水线根据此字段判断是否需要自动施封',
  `TASK_STATUS` varchar(8) DEFAULT NULL COMMENT '任务状态',
  `WRITE_DATE` varchar(32) DEFAULT NULL COMMENT '平台写入时间',
  `HANDLE_FLAG` varchar(8) DEFAULT NULL COMMENT '见附录I：处理标记',
  `HANDLE_DATE` varchar(32) DEFAULT NULL COMMENT '处理时间',
  `EQUIP_CODE_NEW` varchar(32) DEFAULT NULL COMMENT '根据“变更参数类型”决定此字段的内容。',
  `PARAM_TYPE` varchar(8) DEFAULT NULL COMMENT '变更参数类型 01-不变、02-类型、03-费率、04-电价',
  PRIMARY KEY (`DETECT_TASK_NO`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mt_detect_task`
--

LOCK TABLES `mt_detect_task` WRITE;
/*!40000 ALTER TABLE `mt_detect_task` DISABLE KEYS */;
INSERT INTO `mt_detect_task` VALUES ('3919031830736493','0','03','555','2619031830736484','54','','8000000020003662','8000000020007824','','','','1','','','','0','00','2019-03-18 16:39:23','1','','8000000020003662','01'),('3919052930736991','0','03','555','2619031830736484','54','','8000000020003662','8000000020007824','','','','2','','','','0','00','2019-05-29 15:56:39','2','','8000000020003662','01');
/*!40000 ALTER TABLE `mt_detect_task` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mt_hplcid_cert_info`
--

DROP TABLE IF EXISTS `mt_hplcid_cert_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mt_hplcid_cert_info` (
  `DETECT_TASK_NO` varchar(32) NOT NULL,
  `WEB_NOTICE_NO` varchar(32) DEFAULT NULL COMMENT '本次通知序列号',
  `SYS_NO` varchar(8) DEFAULT NULL,
  `BAR_CODE` varchar(32) DEFAULT NULL COMMENT '设备条码 设备条形码，若为电能表或采集终端检定任务时此字段必填',
  `MODULE_TYPE_CODE` varchar(8) DEFAULT NULL COMMENT '通信模块类别  通信模块类别。02：本地通信模块，03：远程通信模块，此字段必填',
  `MODULE_BAR_CODE` varchar(32) DEFAULT NULL COMMENT '通信模块条形码，若为通信模块检定任务时，此字段必填',
  `HPLCID` varchar(48) DEFAULT NULL COMMENT '芯片ID，此字段必填',
  `IS_LEGAL` varchar(2) DEFAULT NULL COMMENT '1:合法，0：不合法，此字段由MDS认证后回填',
  `CERT_DATE` varchar(32) DEFAULT NULL COMMENT '认证时间\nMDS认证时间，此字段由MDS认证后回填',
  `WRITE_DATE` varchar(32) DEFAULT NULL COMMENT '检定线写入时间',
  `HANDLE_FLAG` varchar(8) DEFAULT NULL COMMENT '处理标记 见附录I：处理标记',
  `HANDLE_DATE` varchar(32) DEFAULT NULL COMMENT '处理时间',
  PRIMARY KEY (`DETECT_TASK_NO`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='芯片ID认证信息表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mt_hplcid_cert_info`
--

LOCK TABLES `mt_hplcid_cert_info` WRITE;
/*!40000 ALTER TABLE `mt_hplcid_cert_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `mt_hplcid_cert_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `operator`
--

DROP TABLE IF EXISTS `operator`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `operator` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(45) DEFAULT NULL,
  `number` varchar(45) DEFAULT NULL,
  `password` varchar(45) DEFAULT NULL,
  `granted` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `operator`
--

LOCK TABLES `operator` WRITE;
/*!40000 ALTER TABLE `operator` DISABLE KEYS */;
INSERT INTO `operator` VALUES (1,'root','123456','123456',1);
/*!40000 ALTER TABLE `operator` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `style_ref_item`
--

DROP TABLE IF EXISTS `style_ref_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `style_ref_item` (
  `id` int(11) NOT NULL,
  `style_id` int(11) DEFAULT NULL,
  `item_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `style_ref_item`
--

LOCK TABLES `style_ref_item` WRITE;
/*!40000 ALTER TABLE `style_ref_item` DISABLE KEYS */;
INSERT INTO `style_ref_item` VALUES (1,1,0),(2,1,1),(3,1,2),(4,1,3),(5,1,4),(6,1,7),(7,1,11),(8,3,0),(9,3,1),(10,3,2),(11,3,3),(12,3,4),(13,3,7),(14,4,5),(15,4,6),(16,4,2),(17,4,3),(18,4,4),(19,4,12),(20,4,13),(21,1,16),(22,3,16),(23,4,16);
/*!40000 ALTER TABLE `style_ref_item` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_test`
--

DROP TABLE IF EXISTS `t_test`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_test` (
  `name` varchar(50) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_test`
--

LOCK TABLES `t_test` WRITE;
/*!40000 ALTER TABLE `t_test` DISABLE KEYS */;
INSERT INTO `t_test` VALUES ('桑啊发的所发生的发生的');
/*!40000 ALTER TABLE `t_test` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `task`
--

DROP TABLE IF EXISTS `task`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `task` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `module_id` int(11) DEFAULT NULL,
  `test_term_id` int(11) DEFAULT NULL,
  `workplace_id` int(11) DEFAULT NULL,
  `task_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=349 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `task`
--

LOCK TABLES `task` WRITE;
/*!40000 ALTER TABLE `task` DISABLE KEYS */;
INSERT INTO `task` VALUES (161,1,0,1,1),(162,1,0,2,1),(163,1,0,3,1),(164,1,0,4,1),(165,1,0,5,1),(166,1,0,6,1),(167,1,0,7,1),(168,1,0,8,1),(169,1,0,9,1),(170,1,0,10,1),(171,1,2,1,1),(172,1,2,2,1),(173,1,2,3,1),(174,1,2,4,1),(175,1,2,5,1),(176,1,2,6,1),(177,1,2,7,1),(178,1,2,8,1),(179,1,2,9,1),(180,1,2,10,1),(181,1,3,1,1),(182,1,3,2,1),(183,1,3,3,1),(184,1,3,4,1),(185,1,3,5,1),(186,1,3,6,1),(187,1,3,7,1),(188,1,3,8,1),(189,1,3,9,1),(190,1,3,10,1),(191,1,7,1,1),(192,1,7,2,1),(193,1,7,3,1),(194,1,7,4,1),(195,1,7,5,1),(196,1,7,6,1),(197,1,7,7,1),(198,1,7,8,1),(199,1,7,9,1),(200,1,7,10,1),(245,4,5,1,65),(246,4,5,2,65),(247,4,5,3,65),(248,4,5,4,65),(249,4,5,5,65),(250,4,5,6,65),(251,4,5,7,65),(252,4,5,8,65),(253,4,5,9,65),(254,4,5,10,65),(255,4,6,1,65),(256,4,6,2,65),(257,4,6,3,65),(258,4,6,4,65),(259,4,6,5,65),(260,4,6,6,65),(261,4,6,7,65),(262,4,6,8,65),(263,4,6,9,65),(264,4,6,10,65),(265,4,2,1,65),(266,4,2,2,65),(267,4,2,3,65),(268,4,2,4,65),(269,4,2,5,65),(270,4,2,6,65),(271,4,2,7,65),(272,4,2,8,65),(273,4,2,9,65),(274,4,2,10,65),(275,4,3,1,65),(276,4,3,2,65),(277,4,3,3,65),(278,4,3,4,65),(279,4,3,5,65),(280,4,3,6,65),(281,4,3,7,65),(282,4,3,8,65),(283,4,3,9,65),(284,4,3,10,65),(285,4,13,1,65),(286,4,13,2,65),(287,4,13,3,65),(288,4,13,4,65),(289,4,13,5,65),(290,4,13,6,65),(291,4,13,7,65),(292,4,13,8,65),(293,4,13,9,65),(294,4,13,10,65),(299,3,0,1,66),(300,3,0,2,66),(301,3,0,3,66),(302,3,0,4,66),(303,3,0,5,66),(304,3,0,6,66),(305,3,0,7,66),(306,3,0,8,66),(307,3,0,9,66),(308,3,0,10,66),(309,3,2,1,66),(310,3,2,2,66),(311,3,2,3,66),(312,3,2,4,66),(313,3,2,5,66),(314,3,2,6,66),(315,3,2,7,66),(316,3,2,8,66),(317,3,2,9,66),(318,3,2,10,66),(319,3,3,1,66),(320,3,3,2,66),(321,3,3,3,66),(322,3,3,4,66),(323,3,3,5,66),(324,3,3,6,66),(325,3,3,7,66),(326,3,3,8,66),(327,3,3,9,66),(328,3,3,10,66),(329,3,7,1,66),(330,3,7,2,66),(331,3,7,3,66),(332,3,7,4,66),(333,3,7,5,66),(334,3,7,6,66),(335,3,7,7,66),(336,3,7,8,66),(337,3,7,9,66),(338,3,7,10,66),(339,3,16,1,66),(340,3,16,2,66),(341,3,16,3,66),(342,3,16,4,66),(343,3,16,5,66),(344,3,16,6,66),(345,3,16,7,66),(346,3,16,8,66),(347,3,16,9,66),(348,3,16,10,66);
/*!40000 ALTER TABLE `task` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `test_result`
--

DROP TABLE IF EXISTS `test_result`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `test_result` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `task_id` int(11) DEFAULT NULL,
  `result_data` varchar(255) DEFAULT NULL,
  `date` datetime DEFAULT NULL,
  `test_operator` varchar(255) DEFAULT NULL,
  `verify_operator` varchar(255) DEFAULT NULL,
  `temperature` varchar(255) DEFAULT NULL,
  `humidity` varchar(255) DEFAULT NULL,
  `module_barcode` varchar(255) DEFAULT NULL,
  `station_barcode` varchar(255) DEFAULT NULL,
  `result` int(11) DEFAULT NULL,
  `item` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `test_result`
--

LOCK TABLES `test_result` WRITE;
/*!40000 ALTER TABLE `test_result` DISABLE KEYS */;
INSERT INTO `test_result` VALUES (1,1,'0','2019-05-28 09:38:59','','','','','6330054100661801597391','181101009',1,'0'),(2,1,'0.07583','2019-05-28 09:38:59','','','','','6330054100661801597391','181101009',1,'2'),(3,1,'0.26985','2019-05-28 09:39:00','','','','','6330054100661801597391','181101009',1,'3'),(4,1,'0','2019-05-28 09:39:00','','','','','6330054100661801597391','181101009',1,'7'),(5,1,'0','2019-05-28 09:39:30','','','','','633005410127180000552','181101004',1,'0'),(6,1,'0.11504','2019-05-28 09:39:30','','','','','633005410127180000552','181101004',1,'2'),(7,1,'0.60671','2019-05-28 09:39:30','','','','','633005410127180000552','181101004',1,'3'),(8,1,'0','2019-05-28 09:39:30','','','','','633005410127180000552','181101004',1,'7'),(9,1,'0','2019-05-28 09:40:14','','','','','633005410127180000787','181101003',1,'0'),(10,1,'0.07635','2019-05-28 09:40:14','','','','','633005410127180000787','181101003',1,'2'),(11,1,'0.33531','2019-05-28 09:40:14','','','','','633005410127180000787','181101003',1,'3'),(12,1,'0','2019-05-28 09:40:14','','','','','633005410127180000787','181101003',1,'7'),(13,66,'通过','2019-05-30 10:28:39','00020088','','','','6330054005991802521122','181101009',1,'0'),(14,66,'0.87859','2019-05-30 10:28:39','00020088','','','','6330054005991802521122','181101009',1,'2'),(15,66,'1.2094','2019-05-30 10:28:39','00020088','','','','6330054005991802521122','181101009',1,'3'),(16,66,'通过','2019-05-30 10:28:39','00020088','','','','6330054005991802521122','181101009',1,'7'),(17,66,'通过','2019-05-30 10:28:39','00020088','','','','6330054005991802521122','181101009',1,'16'),(18,66,'通过','2019-05-30 15:32:02','00020088','','','','6330054005991802521023','181101007',1,'0'),(19,66,'0.9010','2019-05-30 15:32:02','00020088','','','','6330054005991802521023','181101007',1,'2'),(20,66,'1.1725','2019-05-30 15:32:02','00020088','','','','6330054005991802521023','181101007',1,'3'),(21,66,'通过','2019-05-30 15:32:02','00020088','','','','6330054005991802521023','181101007',1,'7'),(22,66,'通过','2019-05-30 15:32:03','00020088','','','','6330054005991802521023','181101007',1,'16');
/*!40000 ALTER TABLE `test_result` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `test_scheme`
--

DROP TABLE IF EXISTS `test_scheme`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `test_scheme` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `scheme_name` varchar(45) DEFAULT NULL,
  `task_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=67 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `test_scheme`
--

LOCK TABLES `test_scheme` WRITE;
/*!40000 ALTER TABLE `test_scheme` DISABLE KEYS */;
INSERT INTO `test_scheme` VALUES (64,'测试',1),(65,'Ⅰ型集中器GPRS',65),(66,' 1型集中器测试',66);
/*!40000 ALTER TABLE `test_scheme` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `test_term`
--

DROP TABLE IF EXISTS `test_term`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `test_term` (
  `id` int(11) NOT NULL,
  `term` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `test_term`
--

LOCK TABLES `test_term` WRITE;
/*!40000 ALTER TABLE `test_term` DISABLE KEYS */;
INSERT INTO `test_term` VALUES (0,'载波通信'),(1,'载波衰减'),(2,'静态功耗'),(3,'动态功耗'),(4,'电源波动'),(5,'网口测试'),(6,'GPRS测试'),(7,'RESET管脚'),(8,'/SET管脚'),(9,'STA管脚'),(10,'STATE测试'),(11,'EVENT管脚'),(12,'加热控制'),(13,'ON/OFF'),(14,'工位重启'),(15,'下行通信'),(16,'HPLC ID'),(17,'模块不上电'),(18,'外观检测');
/*!40000 ALTER TABLE `test_term` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `workplace`
--

DROP TABLE IF EXISTS `workplace`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `workplace` (
  `id` int(11) NOT NULL,
  `workplace_num` int(11) NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `workplace`
--

LOCK TABLES `workplace` WRITE;
/*!40000 ALTER TABLE `workplace` DISABLE KEYS */;
INSERT INTO `workplace` VALUES (1,1),(2,10),(3,11),(4,100),(5,101),(6,110),(7,111),(8,1000),(9,1001),(10,1010);
/*!40000 ALTER TABLE `workplace` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `workplace_module`
--

DROP TABLE IF EXISTS `workplace_module`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `workplace_module` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `workplace_id` int(11) DEFAULT NULL,
  `module_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `workplace_module`
--

LOCK TABLES `workplace_module` WRITE;
/*!40000 ALTER TABLE `workplace_module` DISABLE KEYS */;
INSERT INTO `workplace_module` VALUES (25,1,1),(26,2,2),(27,3,3),(28,4,4),(29,5,5),(30,6,6),(31,7,7),(32,8,8),(33,9,9),(34,0,0);
/*!40000 ALTER TABLE `workplace_module` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2019-05-30 17:01:12
