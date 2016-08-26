using System;
using Quartz;

namespace Script.I200.QuartzJob.Jobs
{
    /// <summary>
    ///     一周内店铺会员生日提醒
    /// </summary>
    public class WeekBirthdayUsersSearchJob : BaseJob<WeekBirthdayUsersSearchJob>
    {
        private const string TaskName = "一周内店铺会员生日提醒";

        #region IJob 成员

        protected override void RunStart(IJobExecutionContext context)
        {
            MessageCenter.Push(string.Format("开始执行任务【{0}】...", TaskName));
        }

        protected override void Run(IJobExecutionContext context)
        {
            //TODO: 调用接口处理业务逻辑
        }

        protected override void RunComplete(IJobExecutionContext context, Exception error, TimeSpan elapsed)
        {
            MessageCenter.Push(error != null
                ? string.Format("执行任务【{0}】出错：{1}，耗时{2} ms.", TaskName, error.Message, elapsed.TotalMilliseconds)
                : string.Format("完成执行任务【{0}】，耗时{1} ms.", TaskName, elapsed.TotalMilliseconds));
        }

        #endregion
    }
}