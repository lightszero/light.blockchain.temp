﻿using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using AllPet.Common;
using System.Linq;
using AllPet.Module.Node;

namespace AllPet.Module
{
    partial class Module_Node : Module_MsgPack
    {
        void Tell_ReqJoinPeer(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_JoinPeer;
            dict["id"] = this.guid.data;
            dict["pubep"] = this.config.PublicEndPoint.ToString();
            //Console.WriteLine("Tell_ReqJoinPeer----->:"+ dict["pubep"]);
            dict["chaininfo"] = chainHash.data;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_ResponseAcceptJoin(IModulePipeline remote)
        {
            var link = this.linkNodes[remote.system.PeerID];
            link.CheckInfo = Guid.NewGuid().ToByteArray();
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_AcceptJoin;
            dict["checkinfo"] = link.CheckInfo;
            dict["plevel"] = this.pLevel;//告诉对方我的优先级
            //选个挑战信息
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Request_ProvePeer(IModulePipeline remote, byte[] addinfo, byte[] signdata)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_ProvePeer;
            dict["pubkey"] = this.pubkey;
            dict["addinfo"] = addinfo;
            dict["signdata"] = signdata;            
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Request_PeerList(IModulePipeline remote)
        {
            if (!this.beEnableQueryPeers) return;
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_PeerList;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_PeerList(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_PeerList;
            var list = new List<MessagePackObject>();
            foreach (var n in this.linkNodes.Values)
            {
                if (n.hadJoin && n.publicEndPoint != null)
                {
                    var item = new MessagePackObjectDictionary();
                    item["pubep"] = n.publicEndPoint.ToString();
                    item["pubkey"] = n.PublicKey;
                    item["id"] = n.ID.data;
                    //var ipep = n.publicEndPoint.ToString();
                    list.Add(new MessagePackObject(item));
                }
            }
            dict["nodes"] = list.ToArray();

            remote.Tell(new MessagePackObject(dict));
        }

        void Tell_BoradCast_PeerState(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.BoradCast_PeerState;
            dict["plevel"] = this.pLevel;//告诉对方我的优先级
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_SendRaw(IModulePipeline remote, byte[] message,byte[] signData)
        {
            logger.Info($"------Tell_SendRaw  To:{remote.ToString()}-------");
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Post_SendRaw;
            dict["message"] = message;
            dict["signData"] = signData;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Post_TouchProvedPeer(IModulePipeline remote,string pubep,string nodeid)
        {
            if (this.beObserver) return;
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Post_TouchProvedPeer;
            dict["pubep"] = pubep;
            dict["nodeid"] = nodeid;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_Iamhere(IModulePipeline remote, string provedpubep)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_Iamhere;
            dict["provedpubep"] = provedpubep;
            dict["isProved"] = this.isProved;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_ProvedRelay(IModulePipeline remote,string pubep,string provedpubep)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_ProvedRelay;
            dict["pubep"] = pubep;
            dict["provedpubep"] = provedpubep;
            dict["isProved"] = this.isProved;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_BoardCast_Tx(IModulePipeline remote, byte[] message, byte[] signData)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.BoardCast_Tx;
            dict["message"] = message;
            dict["signData"] = signData;
            remote.Tell(new MessagePackObject(dict));
        }


        void Tell_Request_plevel(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_Plevel;
            remote.Tell(new MessagePackObject(dict));
        }

        void Tell_Response_plevel(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_Plevel;
            dict["plevel"] = this.pLevel;
            remote.Tell(new MessagePackObject(dict));
        }

        void Tell_Request_BlockHeight(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_BlockHeight;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_BlockHeight(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_BlockHeight;
            dict["blockIndex"] = this.blockIndex;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Request_Block(IModulePipeline remote,ulong blockIndex)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_Block;
            dict["blockIndex"] = blockIndex;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_Block(IModulePipeline remote, byte[] header)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_Block;
            dict["blockHeader"] = header;
            remote.Tell(new MessagePackObject(dict));
        }

        void Tell_BoardCast_LosePlevel(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.BoardCast_LosePlevel;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Request_Tx(IModulePipeline remote,byte[] txid)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_Tx;
            dict["txid"] = txid;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_Tx(IModulePipeline remote, byte[] tx)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_Tx;
            dict["tx"] = tx;
            remote.Tell(new MessagePackObject(dict));
        }
    }
}
