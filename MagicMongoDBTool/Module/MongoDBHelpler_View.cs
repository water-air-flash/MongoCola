﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MagicMongoDBTool.Module;
namespace MagicMongoDBTool.Module
{
    public static partial class MongoDBHelpler
    {
        /// <summary>
        /// 管理中服务器列表
        /// </summary>
        private static Dictionary<String, MongoServer> mongosrvlst = new Dictionary<String, MongoServer>();
        /// <summary>
        /// 增加管理服务器
        /// </summary>
        /// <param name="connlst"></param>
        /// <returns></returns>
        public static Boolean AddServer(List<ConfigHelper.MongoConnectionConfig> connlst)
        {
            try
            {
                foreach (ConfigHelper.MongoConnectionConfig item in connlst)
                {
                    if (!mongosrvlst.ContainsKey(item.HostName))
                    {
                        MongoServerSettings mongosvrsetting = new MongoServerSettings();
                        mongosvrsetting.ConnectionMode = ConnectionMode.Direct;
                        //Can't Use SlaveOk to a Route！！！
                        mongosvrsetting.SlaveOk = item.IsSlaveOk;
                        mongosvrsetting.Server = new MongoServerAddress(item.IpAddr, item.Port);
                        if ((item.UserName == String.Empty) | (item.Password == String.Empty))
                        {
                            //认证的设定
                            mongosvrsetting.DefaultCredentials = new MongoCredentials(item.UserName, item.Password);
                        }
                        MongoServer Mastermongosvr = new MongoServer(mongosvrsetting);
                        mongosrvlst.Add(item.HostName, Mastermongosvr);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //各种节点的Tag前缀
        public const String ServiceTag = "MongoService";
        public const String DataBaseTag = "MongoDatabase";
        public const String CollectionTag = "MongoCollection";
        public const String DocumentTag = "MongoDocument";
        public const String GridFileSystemTag = "MongoGFS";
        public const String UserListTag = "MongoUserList";
        public const String UserTag = "MongoUser";

        /// <summary>
        /// 路径阶层[考虑到以后可能阶层会变换]
        /// </summary>
        enum PathLv:int
        {
            ServerLV = 0,
            DatabaseLv = 1,
            CollectionLV = 2
        }
        #region"展示数据"
        /// <summary>
        /// 将Mongodb的服务器在树形控件中展示
        /// </summary>
        /// <param name="trvMongoDB"></param>
        public static void FillMongoServiceToTreeView(TreeView trvMongoDB)
        {
            trvMongoDB.Nodes.Clear();
            foreach (String mongosvrKey in mongosrvlst.Keys)
            {
                MongoServer mongosvr = mongosrvlst[mongosvrKey];
                TreeNode mongosrvnode = new TreeNode(mongosvrKey + " [" + mongosvr.Settings.Server.Host + ":" + mongosvr.Settings.Server.Port + "]");
                mongosrvnode.Tag = ServiceTag + ":" + mongosvrKey;

                List<String> DatabaseNameList = mongosvr.GetDatabaseNames().ToList<String>();
                foreach (String strDBName in DatabaseNameList)
                {
                    mongosrvnode.Nodes.Add(FillDataBaseInfoToTreeNode(strDBName, mongosvr, mongosvrKey));
                }
                trvMongoDB.Nodes.Add(mongosrvnode);
            }
        }
        /// <summary>
        /// 获得一个表示数据库结构的节点
        /// </summary>
        /// <param name="strDBName"></param>
        /// <param name="mongosvr"></param>
        /// <param name="mongosvrKey"></param>
        /// <returns></returns>
        private static TreeNode FillDataBaseInfoToTreeNode(String strDBName, MongoServer mongosvr, String mongosvrKey)
        {
            TreeNode mongoDBNode;
            switch (strDBName)
            {
                case "admin":
                    mongoDBNode = new TreeNode("管理员权限(admin)");
                    break;
                case "local":
                    mongoDBNode = new TreeNode("本地(local)");
                    break;
                default:
                    mongoDBNode = new TreeNode(strDBName);
                    break;
            }


            mongoDBNode.Tag = DataBaseTag + ":" + mongosvrKey + "/" + strDBName;
            MongoDatabase Mongodb = mongosvr.GetDatabase(strDBName);
            List<String> ColNameList = Mongodb.GetCollectionNames().ToList<String>();
            foreach (String strColName in ColNameList)
            {
                TreeNode mongoColNode = FillCollectionInfoToTreeNode(strColName, Mongodb, mongosvrKey);
                mongoDBNode.Nodes.Add(mongoColNode);
            }
            return mongoDBNode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strColName"></param>
        /// <param name="Mongodb"></param>
        /// <param name="mongosvrKey"></param>
        /// <returns></returns>
        private static TreeNode FillCollectionInfoToTreeNode(String strColName, MongoDatabase Mongodb, String mongosvrKey)
        {
            TreeNode mongoColNode;
            String strTagColName = strColName;
            switch (strColName)
            {
                case "chunks":
                    if (Mongodb.Name == "config")
                    {
                        strColName = "数据块(" + strColName + ")";
                    }
                    break;
                case "databases":
                    if (Mongodb.Name == "config") {
                        strColName = "数据库(" + strColName + ")";
                    }
                    break;
                case "lockpings":
                    if (Mongodb.Name == "config")
                    {
                        strColName = "数据锁(" + strColName + ")";
                    }
                    break;
                case "locks":
                    if (Mongodb.Name == "config")
                    {
                        strColName = "数据锁(" + strColName + ")";
                    }
                    break;
                case "mongos":
                    if (Mongodb.Name == "config")
                    {
                        strColName = "路由服务器(" + strColName + ")";
                    }
                    break;
                case "settings":
                    if (Mongodb.Name == "config")
                    {
                        strColName = "配置(" + strColName + ")";
                    }
                    break;
                case "shards":
                    if (Mongodb.Name == "config")
                    {
                        strColName = "分片(" + strColName + ")";
                    }
                    break;
                case "version":
                    if (Mongodb.Name == "config")
                    {
                        strColName = "版本(" + strColName + ")";
                    }
                    break;
                case "fs.chunks":
                    strColName = "数据块(" + strColName + ")";
                    break;
                case "fs.files":
                    strColName = "文件系统(" + strColName + ")";
                    break;
                case "oplog.rs":
                    strColName = "操作结果(" + strColName + ")";
                    break;
                case "system.indexes":
                    strColName = "索引(" + strColName + ")";
                    break;
                case "system.js":
                    strColName = "存储Javascript(" + strColName + ")";
                    break;
                case "system.replset":
                    strColName = "副本组(" + strColName + ")";
                    break;
                case "replset.minvalid":
                    strColName = "初始化同步(" + strColName + ")";
                    break;
                case "system.users":
                    strColName = "用户列表(" + strColName + ")";
                    break;
                case "me":
                    if (Mongodb.Name == "local")
                    {
                        strColName = "副本组[从属信息](" + strColName + ")";
                    }
                    break;
                case "slaves":
                    if (Mongodb.Name == "local")
                    {
                        strColName = "副本组[本机信息](" + strColName + ")";
                    }
                    break;
                default:
                    break;
            }
            mongoColNode = new TreeNode(strColName);
            mongoColNode.Tag = CollectionTag + ":" + mongosvrKey + "/" + Mongodb.Name + "/" + strTagColName;

            MongoCollection mongoCol = Mongodb.GetCollection(strTagColName);

            //Start ListIndex
            TreeNode mongoIndex = new TreeNode("Indexes");
            List<BsonDocument> IndexList = mongoCol.GetIndexes().ToList<BsonDocument>();
            foreach (BsonDocument Indexdoc in IndexList)
            {
                TreeNode mongoIndexNode = new TreeNode("Index:" + Indexdoc.GetValue("name"));
                foreach (String item in Indexdoc.Names)
                {
                    TreeNode mongoIndexItemNode = new TreeNode(item + ":" + Indexdoc.GetValue(item));

                    mongoIndexNode.Nodes.Add(mongoIndexItemNode);
                }
                mongoIndex.Nodes.Add(mongoIndexNode);
            }
            mongoColNode.Nodes.Add(mongoIndex);
            //End ListIndex

            //Start Data
            TreeNode mongoData = new TreeNode("Data");
            mongoData.Tag = DocumentTag + ":" + mongosvrKey + "/" + Mongodb.Name + "/" + strTagColName;
            mongoColNode.Nodes.Add(mongoData);
            //End Data
            return mongoColNode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strTag"></param>
        /// <param name="lstData"></param>
        public static void FillDataToListView(String strTag, ListView lstData)
        {
            String CollectionPath = strTag.Split(":".ToCharArray())[1];
            String[] cp = CollectionPath.Split("/".ToCharArray());
            List<BsonDocument> DataList = new List<BsonDocument>();
            lstData.Clear();
            MongoCollection mongoCol = mongosrvlst[cp[(int)PathLv.ServerLV]].GetDatabase(cp[(int)PathLv.DatabaseLv]).GetCollection(cp[(int)PathLv.CollectionLV]);
            DataList = mongoCol.FindAllAs<BsonDocument>().SetSkip(SkipCnt).SetLimit(SystemManager.mConfig.LimitCnt).ToList<BsonDocument>();
            if (DataList.Count == 0) { return; }
            if (SkipCnt == 0)
            {
                //第一次显示，获得整个记录集的长度
                CurrentCollectionTotalCnt = (int)mongoCol.FindAllAs<BsonDocument>().Count();
            }
            SetPageEnable();
            switch (cp[2])
            {
                case "fs.files":
                    SetGridFileToListView(DataList, lstData);
                    break;
                case "system.users":
                    SetUserListToListView(DataList, lstData);
                    break;
                case "fs.chunks":
                default:
                    DataList = mongoCol.FindAllAs<BsonDocument>().SetSkip(SkipCnt).SetLimit(SystemManager.mConfig.LimitCnt).ToList<BsonDocument>();
                    if (DataList.Count == 0) { return; }
                    List<String> Columnlist = new List<String>();
                    foreach (BsonDocument Docitem in DataList)
                    {
                        ListViewItem lstItem = new ListViewItem();
                        foreach (String item in Docitem.Names)
                        {
                            if (!Columnlist.Contains(item))
                            {
                                Columnlist.Add(item);
                                lstData.Columns.Add(item);
                            }
                        }
                        //Key:_id
                        lstItem.Text = Docitem.GetValue(Columnlist[0]).ToString();
                        //OtherItems
                        for (int i = 1; i < Columnlist.Count; i++)
                        {
                            BsonValue val;
                            Docitem.TryGetValue(Columnlist[i].ToString(), out val);
                            if (val == null)
                            {
                                lstItem.SubItems.Add("");
                            }
                            else
                            {
                                if (val.IsBsonDocument)
                                {
                                    lstItem.SubItems.Add(val.ToString() + "[包含" + val.ToBsonDocument().ElementCount + "个元素的文档]");
                                }
                                else
                                {
                                    lstItem.SubItems.Add(val.ToString());
                                }
                            }
                        }
                        lstData.Items.Add(lstItem);
                    }
                    break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="lstData"></param>
        private static void SetUserListToListView(List<BsonDocument> DataList, ListView lstData)
        {
            lstData.Clear();
            lstData.Columns.Add("用户名");
            lstData.Columns.Add("是否只读");
            foreach (BsonDocument docfile in DataList)
            {
                ListViewItem lstItem = new ListViewItem();
                lstItem.Text = docfile.GetValue("user").ToString();
                lstItem.SubItems.Add(docfile.GetValue("readOnly").ToString());
                lstData.Items.Add(lstItem);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DataList"></param>
        /// <param name="lstData"></param>
        private static void SetGridFileToListView(List<BsonDocument> DataList, ListView lstData)
        {
            lstData.Clear();
            lstData.Columns.Add("文件名称");
            lstData.Columns.Add("文件大小");
            lstData.Columns.Add("块大小");
            lstData.Columns.Add("上传日期");
            lstData.Columns.Add("MD5");
            foreach (BsonDocument docfile in DataList)
            {
                ListViewItem lstItem = new ListViewItem();
                lstItem.Text = docfile.GetValue("filename").ToString();
                lstItem.SubItems.Add(GetSize((int)docfile.GetValue("length")));
                lstItem.SubItems.Add(GetSize((int)docfile.GetValue("chunkSize")));
                lstItem.SubItems.Add(docfile.GetValue("uploadDate").ToString());
                lstItem.SubItems.Add(docfile.GetValue("md5").ToString());
                lstData.Items.Add(lstItem);
            }
        }
        #endregion

        #region"展示状态"
        public static void FillDBStatusToList(ListView lstData)
        {
            lstData.Clear();
            lstData.Columns.Add("名称");
            lstData.Columns.Add("文档数量");
            lstData.Columns.Add("实际大小");
            lstData.Columns.Add("占用大小");
            lstData.Columns.Add("索引");
            lstData.Columns.Add("平均对象大小");
            lstData.Columns.Add("填充因子");
            foreach (String mongosvrKey in mongosrvlst.Keys)
            {
                MongoServer mongosvr = mongosrvlst[mongosvrKey];
                List<String> DatabaseNameList = mongosvr.GetDatabaseNames().ToList<String>();
                foreach (String strDBName in DatabaseNameList)
                {
                    MongoDatabase Mongodb = mongosvr.GetDatabase(strDBName);

                    List<String> ColNameList = Mongodb.GetCollectionNames().ToList<String>();
                    foreach (String strColName in ColNameList)
                    {

                        CollectionStatsResult dbstatus = Mongodb.GetCollection(strColName).GetStats();
                        ListViewItem lst = new ListViewItem(strDBName + "." + strColName);
                        lst.SubItems.Add(dbstatus.ObjectCount.ToString());
                        lst.SubItems.Add(GetSize(dbstatus.DataSize));
                        lst.SubItems.Add(GetSize(dbstatus.StorageSize));
                        lst.SubItems.Add(GetSize(dbstatus.TotalIndexSize));
                        try
                        {
                            //在某些条件下，这个值会抛出异常，IndexKeyNotFound
                            lst.SubItems.Add(GetSize((long)dbstatus.AverageObjectSize));
                        }
                        catch (Exception)
                        {
                            lst.SubItems.Add(GetSize(0));
                        }

                        lst.SubItems.Add(dbstatus.PaddingFactor.ToString());
                        lstData.Items.Add(lst);
                    }
                }
            }
        }
        public static void FillSrvStatusToList(ListView lstData)
        {
            lstData.Clear();
            lstData.Columns.Add("名称");
            lstData.Columns.Add("数据集数量");
            lstData.Columns.Add("数据大小");
            lstData.Columns.Add("文件大小");
            lstData.Columns.Add("索引数量");
            lstData.Columns.Add("索引数量大小");
            lstData.Columns.Add("对象数量");
            lstData.Columns.Add("占用大小");
            foreach (String mongosvrKey in mongosrvlst.Keys)
            {
                MongoServer mongosvr = mongosrvlst[mongosvrKey];
                List<String> DatabaseNameList = mongosvr.GetDatabaseNames().ToList<String>();
                foreach (String strDBName in DatabaseNameList)
                {
                    MongoDatabase Mongodb = mongosvr.GetDatabase(strDBName);
                    DatabaseStatsResult dbstatus = Mongodb.GetStats();
                    ListViewItem lst = new ListViewItem(mongosvrKey + "." + strDBName);
                    try
                    {
                        lst.SubItems.Add(dbstatus.CollectionCount.ToString());

                    }
                    catch (Exception)
                    {

                        lst.SubItems.Add(string.Empty);
                    }

                    lst.SubItems.Add(GetSize(dbstatus.DataSize));
                    lst.SubItems.Add(GetSize(dbstatus.FileSize));
                    lst.SubItems.Add(dbstatus.IndexCount.ToString());
                    lst.SubItems.Add(GetSize(dbstatus.IndexSize));
                    lst.SubItems.Add(dbstatus.ObjectCount.ToString());
                    lst.SubItems.Add(GetSize(dbstatus.StorageSize));
                    lstData.Items.Add(lst);
                }
            }
        }
        public static void FillSrvOprToList(ListView lstData)
        {
            lstData.Clear();
            Boolean HasHeader = false;
            foreach (String mongosvrKey in mongosrvlst.Keys)
            {
                MongoServer mongosvr = mongosrvlst[mongosvrKey];
                List<String> DatabaseNameList = mongosvr.GetDatabaseNames().ToList<String>();
                foreach (String strDBName in DatabaseNameList)
                {
                    MongoDatabase Mongodb = mongosvr.GetDatabase(strDBName);
                    BsonDocument dbstatus = Mongodb.GetCurrentOp();
                    if (!HasHeader)
                    {

                        lstData.Columns.Add("Name");
                        foreach (String item in dbstatus.GetValue("inprog").AsBsonArray[0].AsBsonDocument.Names)
                        {
                            lstData.Columns.Add(item);

                        }
                        HasHeader = true;
                    }

                    BsonArray doc = dbstatus.GetValue("inprog").AsBsonArray;
                    foreach (BsonDocument item in doc)
                    {
                        ListViewItem lst = new ListViewItem(mongosvrKey + "." + strDBName);
                        foreach (String itemName in item.Names)
                        {
                            lst.SubItems.Add(item.GetValue(itemName).ToString());
                        }
                        lstData.Items.Add(lst);
                    }
                }
            }
        }
        #endregion

        #region"数据导航"
        /// <summary>
        /// 数据集总记录数
        /// </summary>
        public static int CurrentCollectionTotalCnt = 0;
        /// <summary>
        /// Skip记录数
        /// </summary>
        public static int SkipCnt = 0;
        /// <summary>
        /// 是否存在下一页
        /// </summary>
        public static Boolean HasNextPage;
        /// <summary>
        /// 是否存在上一页
        /// </summary>
        public static Boolean HasPrePage;
        /// <summary>
        /// 数据导航
        /// </summary>
        public enum PageChangeOpr
        {
            /// <summary>
            /// 第一页
            /// </summary>
            FirstPage,
            /// <summary>
            /// 最后一页
            /// </summary>
            LastPage,
            /// <summary>
            /// 上一页
            /// </summary>
            PrePage,
            /// <summary>
            /// 下一页
            /// </summary>
            NextPage
        }

        /// <summary>
        /// 换页操作
        /// </summary>
        /// <param name="IsNext"></param>
        /// <param name="strTag"></param>
        /// <param name="lstData"></param>
        public static void PageChanged(PageChangeOpr PageChangeMode, String strTag, ListView lstData)
        {
            switch (PageChangeMode)
            {
                case PageChangeOpr.FirstPage:
                    SkipCnt = 0;
                    break;
                case PageChangeOpr.LastPage:
                    if (CurrentCollectionTotalCnt % SystemManager.mConfig.LimitCnt == 0)
                    {
                        //没有余数的时候，600 % 100 == 0  => Skip = 600-100 = 500
                        SkipCnt = CurrentCollectionTotalCnt - SystemManager.mConfig.LimitCnt;
                    }
                    else
                    {
                        // 630 % 100 == 30  => Skip = 630-30 = 600  
                        SkipCnt = CurrentCollectionTotalCnt - CurrentCollectionTotalCnt % SystemManager.mConfig.LimitCnt;
                    }
                    break;
                case PageChangeOpr.NextPage:
                    SkipCnt += SystemManager.mConfig.LimitCnt;
                    break;
                case PageChangeOpr.PrePage:
                    SkipCnt -= SystemManager.mConfig.LimitCnt;
                    break;
                default:
                    break;
            }
            FillDataToListView(strTag, lstData);
        }
        public static void SetPageEnable()
        {
            if (SkipCnt == 0)
            {
                HasPrePage = false;
            }
            else
            {
                HasPrePage = true;
            }
            if ((SkipCnt + SystemManager.mConfig.LimitCnt) >= CurrentCollectionTotalCnt)
            {
                HasNextPage = false;
            }
            else
            {
                HasNextPage = true;
            }
        }

        #endregion

        #region "辅助方法"
        private static String GetSize(long mSize)
        {
            String strSize = String.Empty;
            String[] Unit = new String[]{
                "Byte","KB","MB","GB","TB"
            };
            if (mSize == 0)
            {
                return "0 Byte";
            }
            byte UnitOrder = 2;
            Double tempSize = mSize / Math.Pow(2, 20);
            while (!(tempSize > 0.1 & tempSize < 1000))
            {
                if (tempSize < 0.1)
                {
                    tempSize = tempSize * 1024;
                    UnitOrder--;
                }
                else
                {

                    tempSize = tempSize / 1024;
                    UnitOrder++;
                }
            }
            return string.Format("{0:F2}", tempSize) + " " + Unit[UnitOrder];
        }
        #endregion

    }
}
