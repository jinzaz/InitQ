﻿using InitQ.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InitQ
{
    public class InitQOptions
    {
        /// <summary>
        /// redis连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库索引
        /// </summary>
        public int DbIndex { get; set; }

        /// <summary>
        /// 没消息时挂起时长(毫秒)
        /// </summary>
        public int SuspendTime { get; set; }

        /// <summary>
        /// 每次消费消息间隔时间(毫秒)
        /// </summary>
        public int IntervalTime { get; set; }

        /// <summary>
        /// 是否显示日志
        /// </summary>
        public bool ShowLog { get; set; }

        /// <summary>
        /// 需要注入的类型
        /// </summary>
        public IList<Type> ListSubscribe { get; set; }

        public InitQOptions()
        {
            ConnectionString = "";
            IntervalTime = 0;
            SuspendTime = 1000;
            ShowLog = false;
            DbIndex = 0;
        }
    }
}
