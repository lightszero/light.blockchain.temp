﻿using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.db.PP
{
    class MsgHelper
    {
        static Dictionary<MsgEnum, Func<BaseMsg>> msgDic = new Dictionary<MsgEnum, Func<BaseMsg>>();

        static void registes()
        {
            msgDic.Add(MsgEnum.Put,()=> {return new Msg_put();});
        }

        public static MsgEnum getMessageType(byte[] msg)
        {
            int id=StreamHelp.readByte(msg);
            if(Enum.IsDefined(typeof(MsgEnum),id))
            {
                return (MsgEnum)id;
            }else
            {
                return MsgEnum.None;
            }
        }

        public static BaseMsg DecodeMessage(byte[] msg)
        {
            MsgEnum type= getMessageType(msg);
            if(type!=MsgEnum.None&&msgDic.ContainsKey(type))
            {
                return msgDic[type]().decode(msg);
            }else
            {
                return null;
            }
        }

        public static byte[] EncodeMessage(BaseMsg msg)
        {
           return msg.encode();
        }
    }

    public abstract class BaseMsg
    {
        /// <summary>
        /// msg类型
        /// </summary>
        public MsgEnum msgtype;
        /// <summary>
        /// 输出字节流
        /// </summary>
        /// <returns></returns>
        public abstract BaseMsg decode(byte[] data);

        public abstract byte[] encode();
    }

    public enum MsgEnum
    {
        None,
        Put,
        Delete,
        CreateTable,
        DeleteTable
    }

    public class Msg_put: BaseMsg
    {
        public byte[] tableid;
        public byte[] key;
        public byte[] value;

        public Msg_put()
        {
            this.msgtype = MsgEnum.Put;
        }

        public override BaseMsg decode(byte[] data)
        {
            using (var stream = new System.IO.MemoryStream(data))
            {
                stream.ReadByte();
                StreamHelp.readLenAndByte(stream, out tableid);
                StreamHelp.readLenAndByte(stream, out key);
                StreamHelp.readLenAndByte(stream, out value);
                return this;
            }
        }

        public override byte[] encode()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                stream.WriteByte((byte)this.msgtype);
                StreamHelp.writeLenAndByte(stream, tableid);
                StreamHelp.writeLenAndByte(stream, key);
                StreamHelp.writeLenAndByte(stream, value);
                return stream.ToArray();
            }
        }
    }
}
