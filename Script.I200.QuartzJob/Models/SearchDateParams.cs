using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Script.I200.QuartzJob.Models
{
   public class SearchDateParams
    {
        /// <summary>
        ///     开始时间
        /// </summary>
        public DateTime BeginDate { get; set; }

        /// <summary>
        ///     结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

    }
}
