﻿using Common;
using FunctionForm.Aggregation;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoGUICtl.ClientTree;
using MongoUtility.Core;
using System;
using System.Linq;
using System.Windows.Forms;

namespace FunctionForm.Operation
{
    public partial class frmCreateView : Form
    {
        public frmCreateView()
        {
            InitializeComponent();
        }

        private void frmCreateView_Load(object sender, EventArgs e)
        {
            cmbViewOn.Items.Clear();
            var ColList = RuntimeMongoDbContext.GetCurrentIMongoDataBase().ListCollections();
            var viewlist = RuntimeMongoDbContext.GetCurrentDBViewNameList();
            foreach (var item in ColList.ToList())
            {
                var ColName = item.GetElement("name").Value.ToString();
                if (!viewlist.Contains(ColName)) {
                    cmbViewOn.Items.Add(ColName);
                }
            }
        }

        /// <summary>
        ///     确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdOK_Click(object sender, EventArgs e)
        {
            try
            {
                var pipeline = new BsonDocumentStagePipelineDefinition<BsonDocument, BsonDocument>(stages.Values.Select(x => (BsonDocument)x));
                RuntimeMongoDbContext.GetCurrentIMongoDataBase().CreateView(txtViewName.Text, cmbViewOn.Text, pipeline);
            }
            catch (Exception ex)
            {
                Utility.ExceptionDeal(ex);
            }
        }

        /// <summary>
        ///     关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     聚合数组
        /// </summary>
        private BsonArray stages = new BsonArray();

        /// <summary>
        ///     生成管道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAggrBuilder_Click(object sender, EventArgs e)
        {
            RuntimeMongoDbContext.SetCurrentCollection(cmbViewOn.Text);
            var frmAggregationBuilder = new FrmStageBuilder();
            Utility.OpenForm(frmAggregationBuilder, false, true);
            foreach (var item in frmAggregationBuilder.Aggregation)
            {
                stages.Add(item);
            }
            UiHelper.FillDataToTreeView("stages", trvNewStage, stages.Values.ToList().Select(x => (BsonDocument)x).ToList(), 0);
        }
    }
}
