using System;
using System.Text;

namespace TestServer.ComplexMode
{
    /// <summary>
    /// 通讯编码格式提供者,为通讯服务提供编码和解码服务
    /// 你可以在继承类中定制自己的编码方式如:数据加密传输等
    /// </summary>
    public class Coder
    {
        /// <summary>
        /// 编码方式
        /// </summary>
        private EncodingMothord _encodingMothord;

        protected Coder()
        {

        }

        public Coder(EncodingMothord encodingMothord)
        {
            _encodingMothord = encodingMothord;
        }

        public enum EncodingMothord
        {
            Default = 0,
            Unicode,
            UTF8,
            ASCII,
        }

        /// <summary>
        /// 通讯数据解码
        /// </summary>
        /// <param name="dataBytes">需要解码的数据</param>
        /// <returns>编码后的数据</returns>
        public virtual string GetEncodingString(byte[] dataBytes, int size)
        {
            switch (_encodingMothord)
            {
                case EncodingMothord.Default:
                {
                    return Encoding.Default.GetString(dataBytes, 0, size);
                }
                case EncodingMothord.Unicode:
                {
                    return Encoding.Unicode.GetString(dataBytes, 0, size);
                }
                case EncodingMothord.UTF8:
                {
                    return Encoding.UTF8.GetString(dataBytes, 0, size);
                }
                case EncodingMothord.ASCII:
                {
                    return Encoding.ASCII.GetString(dataBytes, 0, size);
                }
                default:
                {
                    throw (new Exception("未定义的编码格式"));
                }
            }

        }

        /// <summary>
        /// 数据编码
        /// </summary>
        /// <param name="datagram">需要编码的报文</param>
        /// <returns>编码后的数据</returns>
        public virtual byte[] GetEncodingBytes(string datagram)
        {
            switch (_encodingMothord)
            {
                case EncodingMothord.Default:
                {
                    return Encoding.Default.GetBytes(datagram);
                }
                case EncodingMothord.Unicode:
                {
                    return Encoding.Unicode.GetBytes(datagram);
                }
                case EncodingMothord.UTF8:
                {
                    return Encoding.UTF8.GetBytes(datagram);
                }
                case EncodingMothord.ASCII:
                {
                    return Encoding.ASCII.GetBytes(datagram);
                }
                default:
                {
                    throw (new Exception("未定义的编码格式"));
                }
            }
        }

    }
}