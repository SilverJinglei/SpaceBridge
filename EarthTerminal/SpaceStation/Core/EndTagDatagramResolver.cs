using System;
using System.Collections.Generic;

namespace SpaceStation.Core
{
    /// <summary>
    /// 数据报文分析器,通过分析接收到的原始数据,得到完整的数据报文.
    /// 继承该类可以实现自己的报文解析方法.
    /// 通常的报文识别方法包括:固定长度,长度标记,标记符等方法
    /// 本类的现实的是标记符的方法,你可以在继承类中实现其他的方法
    /// </summary>
    public class EndTagDatagramResolver
    {
        /// <summary>
        /// 报文结束标记
        /// </summary>
        private readonly string _endTag;

        /// <summary>
        /// 返回结束标记
        /// </summary>
        private string EndTag => _endTag;

        /// <summary>
        /// 受保护的默认构造函数,提供给继承类使用
        /// </summary>
        protected EndTagDatagramResolver()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endTag">报文结束标记</param>
        public EndTagDatagramResolver(string endTag)
        {
            if (endTag == null)
            {
                throw (new ArgumentNullException("结束标记不能为null"));
            }

            if (endTag == "")
            {
                throw (new ArgumentException("结束标记符号不能为空字符串"));
            }

            this._endTag = endTag;
        }

        /// <summary>
        /// 解析报文
        /// </summary>
        /// <param name="rawDatagram">原始数据,返回未使用的报文片断,
        /// 该片断会保存在Session的Datagram对象中</param>
        /// <returns>报文数组,原始数据可能包含多个报文</returns>
        public virtual string[] Resolve(ref string rawDatagram)
        {
            var datagrams = new List<string>();

            //末尾标记位置索引
            int tagIndex = -1;

            while (true)
            {
                tagIndex = rawDatagram.IndexOf(_endTag, tagIndex + 1, StringComparison.Ordinal);

                if (tagIndex == -1)
                    break;

                //按照末尾标记把字符串分为左右两个部分
                string newDatagram = rawDatagram.Substring(0, tagIndex + _endTag.Length);

                datagrams.Add(newDatagram);

                if (tagIndex + _endTag.Length >= rawDatagram.Length)
                {
                    rawDatagram = "";
                    break;
                }

                rawDatagram = rawDatagram.Substring(tagIndex + _endTag.Length,
                    rawDatagram.Length - newDatagram.Length);

                //从开始位置开始查找
                tagIndex = 0;
            }

            string[] results = new string[datagrams.Count];

            datagrams.CopyTo(results);

            return results;
        }
    }
}