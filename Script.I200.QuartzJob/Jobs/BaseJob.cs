using System;
using System.Diagnostics;
using Quartz;

namespace Script.I200.QuartzJob.Jobs
{
    public abstract class BaseJob<T> : IJob
    {

        #region IJob 成员

        /// <summary>
        /// 实现定时任务接口
        /// </summary>
        /// <param name="context"></param>
        void IJob.Execute(IJobExecutionContext context)
        {
            var watch = Stopwatch.StartNew();
            Exception error = null;

            try
            {
                RunStart(context);

                //运行
                Run(context);

            }
            catch (Exception ex)
            {
                var errorMsg = ErrorHelperEx.GetExceptionMsg(ex.InnerException, ex.ToString());
                LogHelper.WriteLog(typeof(T), errorMsg);

                error = ex;
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }

                try
                {
                    RunComplete(context, error, watch.Elapsed);
                }
                catch
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// 定时任务表达式
        /// </summary>
        public string CronExpression { get; private set; }

        protected abstract void Run(IJobExecutionContext context);

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="context"></param>
        protected abstract void RunStart(IJobExecutionContext context);

        /// <summary>
        /// 完成执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="error"></param>
        /// <param name="elapsed"></param>
        protected abstract void RunComplete(IJobExecutionContext context, Exception error, TimeSpan elapsed);

        #endregion
    }
}
