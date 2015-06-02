using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageHandle.mode
{

    /// <summary>
    /// 图文消息项
    /// </summary>
    public class NewsItem
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string PicUrl { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Url { get; set; }
    }
}
