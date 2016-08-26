using Castle.Core.Resource;
using Castle.Windsor.Configuration.Interpreters;

namespace Script.I200.QuartzJob
{

    public class WindsorHelper
    {
        static readonly Castle.Windsor.WindsorContainer Container;

        static WindsorHelper()
        {
            var sectionKey = "yuanbei.task/castle";
            Container = new Castle.Windsor.WindsorContainer(new XmlInterpreter(new ConfigResource(sectionKey)));
        }

        /// <summary>
        /// 解析多个服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] ResolveAll<T>()
        {
            return Container.ResolveAll<T>();
        }

        /// <summary>
        /// 解析一个服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>(string key = "")
        {
            if (string.IsNullOrEmpty(key))
                return Container.Resolve<T>();
            else
                return Container.Resolve<T>(key);
        }
    }
}