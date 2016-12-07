CREATE DATABASE  IF NOT EXISTS `mydb_new` /*!40100 DEFAULT CHARACTER SET utf8 COLLATE utf8_bin */;
USE `mydb_new`;
-- MySQL dump 10.13  Distrib 5.7.9, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: mydb_new
-- ------------------------------------------------------
-- Server version	5.7.13-log

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
-- Table structure for table `config_hero`
--

DROP TABLE IF EXISTS `config_hero`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `config_hero` (
  `heroId` int(11) NOT NULL COMMENT '英雄ID',
  `name` varchar(20) COLLATE utf8mb4_bin NOT NULL COMMENT '英雄名称',
  `rare` int(11) NOT NULL DEFAULT '11' COMMENT '稀有度（11-13）,默认为11',
  `type` int(11) DEFAULT NULL COMMENT '英雄职业（1：法师，2：战士，3：牧师，4：勇士）',
  `defaultStar` int(11) DEFAULT '0' COMMENT '英雄初始星数',
  `isOpen` tinyint(1) NOT NULL DEFAULT '1' COMMENT '当前是否在游戏中开放（即可在英雄图鉴看到，可以被抽卡抽到）',
  PRIMARY KEY (`heroId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='游戏配置表-英雄';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `config_hero`
--

LOCK TABLES `config_hero` WRITE;
/*!40000 ALTER TABLE `config_hero` DISABLE KEYS */;
INSERT INTO `config_hero` VALUES (1,'英雄法师',11,1,1,1),(2,'英雄战士',11,2,1,1),(3,'英雄牧师',12,3,1,1),(4,'英雄勇士',11,4,1,1);
/*!40000 ALTER TABLE `config_hero` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `config_hero_equipment`
--

DROP TABLE IF EXISTS `config_hero_equipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `config_hero_equipment` (
  `id` int(11) NOT NULL COMMENT 'ID',
  `heroId` int(11) NOT NULL COMMENT '英雄ID',
  `heroQuality` int(11) NOT NULL COMMENT '英雄品阶',
  `seq` int(11) NOT NULL COMMENT '装备槽位序号',
  `propId` int(11) NOT NULL COMMENT '装备ID',
  `equipRank` int(11) NOT NULL COMMENT '穿戴等级限制',
  PRIMARY KEY (`id`),
  UNIQUE KEY `LOGIC_INDEX` (`heroId`,`heroQuality`,`seq`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='游戏配置表-英雄可穿戴装备';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `config_hero_equipment`
--

LOCK TABLES `config_hero_equipment` WRITE;
/*!40000 ALTER TABLE `config_hero_equipment` DISABLE KEYS */;
INSERT INTO `config_hero_equipment` VALUES (1,1,10,1,200001,1),(2,1,10,2,200002,1),(3,1,10,3,200003,1),(4,1,10,4,200004,1),(5,1,20,1,200005,5),(6,1,20,2,200006,5),(7,1,20,3,200007,5),(8,1,20,4,200008,5),(9,1,21,1,200001,10),(10,1,21,2,200002,10),(11,1,21,3,200003,10),(12,1,21,4,200004,10),(13,1,30,1,200005,20),(14,1,30,2,200006,20),(15,1,30,3,200007,20),(16,1,30,4,200008,20),(17,1,31,1,200001,25),(18,1,31,2,200002,25),(19,1,31,3,200003,25),(20,1,31,4,200004,25),(21,1,32,1,200005,30),(22,1,32,2,200006,30),(23,1,32,3,200007,30),(24,1,32,4,200008,30),(25,1,40,1,200001,40),(26,1,40,2,200002,40),(27,1,40,3,200003,40),(28,1,40,4,200004,40),(29,1,41,1,200005,45),(30,1,41,2,200006,45),(31,1,41,3,200007,45),(32,1,41,4,200008,45),(33,1,42,1,200001,50),(34,1,42,2,200002,50),(35,1,42,3,200003,50),(36,1,42,4,200004,50),(37,1,43,1,200005,55),(38,1,43,2,200006,55),(39,1,43,3,200007,55),(40,1,43,4,200008,60);
/*!40000 ALTER TABLE `config_hero_equipment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `config_prop`
--

DROP TABLE IF EXISTS `config_prop`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `config_prop` (
  `id` int(11) NOT NULL COMMENT '道具ID',
  `type` int(11) DEFAULT NULL COMMENT '道具类型 （1：经验道具，2：装备，3：装备碎片，4：英雄灵魂石）',
  `subType` int(11) DEFAULT NULL COMMENT '子类型',
  `name` varchar(20) COLLATE utf8mb4_bin DEFAULT NULL COMMENT '名称',
  `desc` varchar(50) COLLATE utf8mb4_bin DEFAULT NULL COMMENT '描述',
  `quality` int(11) DEFAULT NULL COMMENT '品质',
  `icon` varchar(20) COLLATE utf8mb4_bin DEFAULT NULL COMMENT '图标',
  `sellPrice` int(11) DEFAULT NULL COMMENT '铜币卖出价格（填-1表示不允许卖）',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='游戏配置表-道具';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `config_prop`
--

LOCK TABLES `config_prop` WRITE;
/*!40000 ALTER TABLE `config_prop` DISABLE KEYS */;
INSERT INTO `config_prop` VALUES (100001,1,-1,'小号经验药水','使用增加英雄经验100点',1,'item1',500),(100002,1,-1,'中号经验药水','使用增加英雄经验200点',2,'item1',800),(200001,2,-1,'盾牌','盾牌',1,'item1',800),(200002,2,-1,'弓箭','弓箭',1,'item1',800),(200003,2,-1,'长矛','长矛',1,'item1',800),(200004,2,-1,'护甲','护甲',1,'item1',800),(200005,2,-1,'头盔','头盔',1,'item1',800),(200006,2,-1,'匕首','匕首',1,'item1',800),(200007,2,-1,'禅杖','禅杖',1,'item1',800),(200008,2,-1,'大刀','大刀',1,'item1',800),(200009,1,-1,'钱袋','增加100金币',1,'item1',100);
/*!40000 ALTER TABLE `config_prop` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `config_system`
--

DROP TABLE IF EXISTS `config_system`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `config_system` (
  `systemId` int(11) NOT NULL COMMENT '系统ID',
  `systemName` varchar(20) COLLATE utf8mb4_bin DEFAULT NULL COMMENT '系统名称',
  `help` varchar(100) COLLATE utf8mb4_bin DEFAULT NULL COMMENT '系统帮助信息',
  `openConditionRankLimit` int(11) DEFAULT NULL COMMENT '所需玩家等级（不限填-1）',
  `openConditionVipLimit` int(11) DEFAULT NULL COMMENT '所需Vip等级（不限填-1）',
  `openConditionLevelLimit` int(11) DEFAULT NULL COMMENT '所需通关关卡（不限填-1）',
  `rewardType` int(11) DEFAULT NULL COMMENT '类型',
  `rewardId` int(11) DEFAULT NULL COMMENT 'id',
  `rewardCount` int(11) DEFAULT NULL COMMENT '数量',
  PRIMARY KEY (`systemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='游戏配置表-游戏系统模块';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `config_system`
--

LOCK TABLES `config_system` WRITE;
/*!40000 ALTER TABLE `config_system` DISABLE KEYS */;
INSERT INTO `config_system` VALUES (1,'普通关卡','通过普通关卡可以获得一定几率掉落的道具和英雄经验，快来让你的英雄挑战吧！',NULL,NULL,NULL,NULL,NULL,NULL),(2,'精英关卡','通过精英关卡可以获得一定几率掉落的稀有英雄碎片用于英雄升阶，每天挑战次数有限哦！',NULL,NULL,NULL,NULL,NULL,NULL),(3,'竞技场','这里是各位英雄彼此切磋较量的场所，有丰富的奖励给予强大的英雄',25,-1,-1,1,100001,10),(4,'商店','在这里你可以花费金币或者钻石来兑换你想要的道具，商店会在每天9点、15点、21点进货新的一批商品哟，欢迎客官前来购买',15,-1,-1,1,100001,5);
/*!40000 ALTER TABLE `config_system` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `server_config`
--

DROP TABLE IF EXISTS `server_config`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `server_config` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `key` varchar(45) NOT NULL COMMENT '键名',
  `value` varchar(45) NOT NULL COMMENT '键值',
  `comment` varchar(45) DEFAULT NULL COMMENT '说明',
  `lastUpdateUser` varchar(45) DEFAULT NULL COMMENT '上次修改者',
  `lastUpdateTime` varchar(45) DEFAULT NULL COMMENT '上次修改时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `key_UNIQUE` (`key`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COMMENT='服务器属性';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `server_config`
--

LOCK TABLES `server_config` WRITE;
/*!40000 ALTER TABLE `server_config` DISABLE KEYS */;
INSERT INTO `server_config` VALUES (1,'channelId','2','所属渠道（1：苹果官方，2：谷歌官方）','管理员1','2016-12-12 8:00:00'),(2,'serverId','2','服务器编号','管理员1','2016-12-12 8:00:00'),(3,'openTime','2016-11-10 8:00:00','开服时间','管理员1','2016-10-10 8:00:00'),(4,'userConfigNickNameMaxLength','10','玩家昵称最大字数','管理员1','2016-10-10 8:00:00'),(5,'userConfigMaxRank','90','该版本中玩家等级上限','管理员1','2016-12-12 8:00:00');
/*!40000 ALTER TABLE `server_config` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nickName` varchar(45) NOT NULL COMMENT '昵称',
  `gold` int(11) DEFAULT '0' COMMENT '金钱数',
  `diamond` int(11) DEFAULT '0' COMMENT '钻石数',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COMMENT='玩家信息表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (1,'张三',100,100),(2,'李四',200,200),(4,'赵六',0,0);
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_hero`
--

DROP TABLE IF EXISTS `user_hero`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `user_hero` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `userId` int(11) NOT NULL COMMENT '玩家id',
  `heroId` int(11) NOT NULL COMMENT '英雄id',
  `rank` int(11) NOT NULL DEFAULT '0' COMMENT '英雄等级',
  `hasEquipment1` tinyint(1) DEFAULT '0' COMMENT '是否穿戴了第一个槽位的装备',
  `hasEquipment2` tinyint(1) DEFAULT '0' COMMENT '是否穿戴了第二个槽位的装备',
  `hasEquipment3` tinyint(1) DEFAULT '0' COMMENT '是否穿戴了第三个槽位的装备',
  `hasEquipment4` tinyint(1) DEFAULT '0' COMMENT '是否穿戴了第四个槽位的装备',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COMMENT='玩家拥有英雄信息';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_hero`
--

LOCK TABLES `user_hero` WRITE;
/*!40000 ALTER TABLE `user_hero` DISABLE KEYS */;
INSERT INTO `user_hero` VALUES (1,1,1,30,1,0,0,0),(2,2,1,40,0,1,1,1);
/*!40000 ALTER TABLE `user_hero` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2016-12-06 15:58:19
