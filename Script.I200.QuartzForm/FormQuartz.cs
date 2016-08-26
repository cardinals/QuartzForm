using System;
using System.Threading;
using System.Windows.Forms;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Script.I200.QuartzJob;
using Message = Script.I200.QuartzJob.Message;

namespace Script.I200.QuartzForm
{
    public partial class FormQuartz : Form
    {
        private const string DoText = "执行";
        private const string DoTextByScheduler = "定时执行";
        private const string LeftImportText = "【";
        private const string RightImportText = "】";
        private const string CloseThredWarning = "请先停止，等待线程关闭";
        private const string WarningTitle = "系统提示";
        private bool _canclose = true;
        private IScheduler _sched;
        public bool Stopping;

        public FormQuartz()
        {
            InitializeComponent();
        }

        private void FormQuartz_Load(object sender, EventArgs e)
        {
            //容器处理
            var creaters = WindsorHelper.ResolveAll<JobCreater>();

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Width = splitContainer1.Panel1.Width
            };

            var index = 0;
            foreach (var creater in creaters)
            {
                var button = new Button();
                if (creater.CronExpression == "false")
                    button.Text = DoText + LeftImportText + creater.JobType.Name + RightImportText;
                else
                    button.Text = DoTextByScheduler + LeftImportText + creater.JobType.Name + RightImportText;
                button.Tag = creater;
                button.Click += button_Click;
                button.Width = panel.Width - 30;
                button.Left = 10;
                button.Top = 5 + (index*(5 + button.Height));

                panel.Controls.Add(button);

                index++;
            }

            //添加到Panel
            splitContainer1.Panel1.Controls.Add(panel);

            //开始显示消息
            MessageCenter.PushMessage += MessageCenter_PushMessage;
        }

        /// <summary>
        ///     显示消息
        /// </summary>
        /// <param name="item"></param>
        private void MessageCenter_PushMessage(Message item)
        {
            var message = string.Format("{0} => {1}", item.CreateTime, item.Content);
            SafeSetText(message, richTimerPush, false);
        }

        /// <summary>
        ///     关闭任务窗口之前提示操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_canclose) return;
            MessageBox.Show(CloseThredWarning);
            e.Cancel = true;
        }

        /// <summary>
        ///     关闭任务窗口操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormBaiduPush_FormClosed(object sender, FormClosedEventArgs e)
        {
            //等待完成后退出
            if (_sched != null)
            {
                _sched.Shutdown(true);
            }


            Environment.Exit(0);
        }

        /// <summary>
        ///     按钮点击操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, EventArgs e)
        {
            var b = (Button) sender;
            var creator = b.Tag as JobCreater;
            if (creator == null) return;
            var showMessage = "确证执行任务[" + creator.JobType.Name + "]吗？";
            if (MessageBox.Show(showMessage, WarningTitle
                , MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                //异步执行任务
                ThreadPool.QueueUserWorkItem(item =>
                {
                    //执行任务
                    var job = Activator.CreateInstance(creator.JobType) as IJob;
                    if (job != null)
                        job.Execute(null);
                }, null);
            }
        }

        /// <summary>
        ///     任务开启操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            var showMessage = "是否开启定时任务？";
            var buttonText = "停止定时任务(&T)";
            if (MessageBox.Show(showMessage, WarningTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) ==
                DialogResult.Cancel)
            {
                return;
            }

            richTimerPush.Clear();

            Stopping = false;
            btnStart.Enabled = false;
            btnStop.Text = buttonText;
            _canclose = false;

            //开始定时任务
            StartTimerQuartzJobs();

            btnStop.Enabled = true;
        }

        /// <summary>
        ///     任务停止操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            var showMessage = "是否停止定时任务？";
            if (MessageBox.Show(showMessage, WarningTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) ==
                DialogResult.Cancel)
            {
                return;
            }

            try
            {
                Stopping = true;
                var stopMessage = "正在停止...";
                btnStop.Text = stopMessage;

                //等待完成后退出
                if (_sched != null)
                {
                    _sched.Shutdown(true);
                    _sched = null;
                }

                btnStop.Text = "已停止(&T)";
                btnStop.Enabled = false;

                btnStart.Enabled = true;
                _canclose = true;
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     开启定时任务操作
        /// </summary>
        public void StartTimerQuartzJobs()
        {
            if (_sched == null)
            {
                ISchedulerFactory sf = new StdSchedulerFactory(); //执行者  
                _sched = sf.GetScheduler();
            }

            var msg = string.Format("{0} => {1}", DateTime.Now, "正在启动定时任务...");
            SafeSetText(msg, richTimerPush, false);

            //容器处理
            var creaters = WindsorHelper.ResolveAll<JobCreater>();

            foreach (var creater in creaters)
            {
                if (creater.CronExpression == "false")
                    continue;

                msg = string.Format("{0} => {1}", DateTime.Now, "启动任务：" + creater.JobType.FullName + "...");
                SafeSetText(msg, richTimerPush, false);

                //添加定时任务
                AddJobToSchedule(creater.JobType, creater.CronExpression);
            }

            _sched.Start();

            msg = string.Format("{0} => {1}", DateTime.Now, "定时任务启动成功...");
            SafeSetText(msg, richTimerPush, false);
        }

        /// <summary>
        ///     添加任务到任务调度
        /// </summary>
        /// <param name="jobType"></param>
        /// <param name="cronExpression"></param>
        private void AddJobToSchedule(Type jobType, string cronExpression)
        {
            var key = jobType.Name.ToLower();
            var jobkey = new JobKey("myjob_" + key, "mygroup_" + key);
            var job = JobBuilder.Create(jobType).WithIdentity(jobkey).Build();
            var trigger = new CronTriggerImpl("simpleTrig_" + key, "simpleGroup_" + key, "myjob_" + key,
                "mygroup_" + key, cronExpression, TimeZoneInfo.Local);
            _sched.ScheduleJob(job, trigger);
        }

        /// <summary>
        ///     添加任务到任务调度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cronExpression"></param>
        private void AddJobToSchedule<T>(string cronExpression)
            where T : IJob
        {
            var key = typeof (T).Name.ToLower();
            var jobkey = new JobKey("myjob_" + key, "mygroup_" + key);
            var job = JobBuilder.Create<T>().WithIdentity(jobkey).Build();
            var trigger = new CronTriggerImpl("simpleTrig_" + key, "simpleGroup_" + key, "myjob_" + key,
                "mygroup_" + key, cronExpression, TimeZoneInfo.Local);
            _sched.ScheduleJob(job, trigger);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        #region 文本框控件显示代理

        private delegate void SafeSetTextCall(string text, RichTextBox box, bool isClear);

        private void SetText(string text, RichTextBox box, bool isClear)
        {
            try
            {
                //大于100行清除
                if (isClear || box.Lines.Length > 100)
                {
                    box.Clear();
                }

                box.AppendText(text);
                box.AppendText("\r\n");

                //滚动到尾部
                box.Select(box.TextLength, 0);
                box.ScrollToCaret();
            }
            catch (Exception exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     设置文本显示
        /// </summary>
        /// <param name="text"></param>
        /// <param name="box"></param>
        /// <param name="isClear"></param>
        private void SafeSetText(string text, RichTextBox box, bool isClear)
        {
            if (InvokeRequired)
            {
                SafeSetTextCall call = SetText;

                BeginInvoke(call, text, box, isClear);
            }
            else
            {
                SetText(text, box, isClear);
            }
        }

        #endregion
    }
}