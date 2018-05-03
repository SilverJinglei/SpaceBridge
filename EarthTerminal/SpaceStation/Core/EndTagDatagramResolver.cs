using System;
using System.Collections.Generic;

namespace SpaceStation.Core
{
    /// <summary>
    /// ���ݱ��ķ�����,ͨ���������յ���ԭʼ����,�õ����������ݱ���.
    /// �̳и������ʵ���Լ��ı��Ľ�������.
    /// ͨ���ı���ʶ�𷽷�����:�̶�����,���ȱ��,��Ƿ��ȷ���
    /// �������ʵ���Ǳ�Ƿ��ķ���,������ڼ̳�����ʵ�������ķ���
    /// </summary>
    public class EndTagDatagramResolver
    {
        /// <summary>
        /// ���Ľ������
        /// </summary>
        private readonly string _endTag;

        /// <summary>
        /// ���ؽ������
        /// </summary>
        private string EndTag => _endTag;

        /// <summary>
        /// �ܱ�����Ĭ�Ϲ��캯��,�ṩ���̳���ʹ��
        /// </summary>
        protected EndTagDatagramResolver()
        {

        }

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="endTag">���Ľ������</param>
        public EndTagDatagramResolver(string endTag)
        {
            if (endTag == null)
            {
                throw (new ArgumentNullException("������ǲ���Ϊnull"));
            }

            if (endTag == "")
            {
                throw (new ArgumentException("������Ƿ��Ų���Ϊ���ַ���"));
            }

            this._endTag = endTag;
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="rawDatagram">ԭʼ����,����δʹ�õı���Ƭ��,
        /// ��Ƭ�ϻᱣ����Session��Datagram������</param>
        /// <returns>��������,ԭʼ���ݿ��ܰ����������</returns>
        public virtual string[] Resolve(ref string rawDatagram)
        {
            var datagrams = new List<string>();

            //ĩβ���λ������
            int tagIndex = -1;

            while (true)
            {
                tagIndex = rawDatagram.IndexOf(_endTag, tagIndex + 1, StringComparison.Ordinal);

                if (tagIndex == -1)
                    break;

                //����ĩβ��ǰ��ַ�����Ϊ������������
                string newDatagram = rawDatagram.Substring(0, tagIndex + _endTag.Length);

                datagrams.Add(newDatagram);

                if (tagIndex + _endTag.Length >= rawDatagram.Length)
                {
                    rawDatagram = "";
                    break;
                }

                rawDatagram = rawDatagram.Substring(tagIndex + _endTag.Length,
                    rawDatagram.Length - newDatagram.Length);

                //�ӿ�ʼλ�ÿ�ʼ����
                tagIndex = 0;
            }

            string[] results = new string[datagrams.Count];

            datagrams.CopyTo(results);

            return results;
        }
    }
}