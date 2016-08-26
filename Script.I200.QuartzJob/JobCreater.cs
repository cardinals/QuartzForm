using System;

namespace Script.I200.QuartzJob
{
    /// <summary>
    /// 任务创建器
    /// </summary>
    public class JobCreater
    {
        /// <summary>
        /// 定时任务表达式
        /// </summary>
        public string CronExpression { get; private set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public Type JobType { get; private set; }

        public JobCreater(string type, string cronExpression)
        {
            this.JobType = Type.GetType(type, true);
            this.CronExpression = cronExpression;
        }
    }
}
