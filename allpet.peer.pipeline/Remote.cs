﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AllPet.Pipeline
{
    class RefSystemRemote : ISystemPipeline
    {
        public AllPet.peer.tcp.IPeer peer;
        public UInt64 PeerID
        {
            get;
            private set;
        }
        public IPEndPoint Remote
        {
            get;
            private set;
        }
        PipelineSystemV1 _System;
        global::System.Collections.Concurrent.ConcurrentDictionary<string, IModulePipeline> refPipelines;
        public RefSystemRemote(PipelineSystemV1 system, AllPet.peer.tcp.IPeer peer, IPEndPoint remote, UInt64 id, bool host)
        {
            this.IsHost = host;
            this._System = system;
            this.peer = peer;
            this.PeerID = id;
            this.Remote = remote;
            refPipelines = new System.Collections.Concurrent.ConcurrentDictionary<string, IModulePipeline>();
        }
        public bool IsLocal => false;
        public bool IsHost
        {
            get;
            private set;
        }
        public string remoteaddr
        {
            get
            {
                return Remote.ToString();
            }
        }

        public bool linked
        {
            get;
            private set;
        }

        public event Action<RefSystemRemote> OnPeerLink;
        public event Action<RefSystemRemote> OnPeerClose;
        bool closed = false;
        public void Close()
        {
            lock (this)
            {
                this.linked = false;
                this.closed = true;
                if (OnPeerClose != null)
                    this.OnPeerClose(this);
            }
        }
        //public void HaltLink()
        //{
        //    System.Threading.Monitor.Enter(this);
        //}
        //public void ResumeLink()
        //{
        //    System.Threading.Monitor.Exit(this);
        //}

        public void SetLinkEvent(Action<ISystemPipeline> methodOnLink)
        {
            lock (this)
            {
                if (this.linked)
                {
                    methodOnLink(this);
                }
                else
                {
                    this.OnPeerLink += methodOnLink;
                }
            }
        }
        public void SetLinkCloseEvent(Action<ISystemPipeline> methodOnClose)
        {
            lock (this)
            {
                if (this.closed)
                {
                    methodOnClose(this);
                }
                else
                {
                    this.OnPeerClose += methodOnClose;
                }
            }
        }

        public void Linked(UInt64 id, bool accept, IPEndPoint remote)
        {
            lock (this)
            {
                this.linked = true;
                if (OnPeerLink != null)
                    this.OnPeerLink(this);
            }
        }
        public IModulePipeline GetPipeline(IModuleInstance user, string path)
        {
            var pipestr = this.Remote.ToString() + "/" + path + "_" + user.path;
            if (this.refPipelines.TryGetValue(pipestr, out IModulePipeline pipe))
            {
                return pipe;
            }
            PipelineRefRemote _pipe = new PipelineRefRemote(_System.refSystemThis, user.path, this, path);
            this.refPipelines[pipestr] = _pipe;

            return _pipe;
        }

        public IModulePipeline GetPipeLineByFrom(IModulePipeline from, IModuleInstance to)
        {
            throw new NotImplementedException("all GetPipeline By From is To Local");
        }
    }
    class PipelineRefRemote : IModulePipeline
    {
        public PipelineRefRemote(ISystemPipeline usersystem, string userPath, RefSystemRemote remotesystem, string path)
        {
            this._usersystem = usersystem;
            this.userpath = userPath;

            this._remotesystem = remotesystem;
            this.path = path;
        }

        RefSystemRemote _remotesystem;
        ISystemPipeline _usersystem;
        string userpath;
        public ISystemPipeline system
        {
            get
            {
                return _remotesystem;
            }
        }

        public string path
        {
            get;
            private set;
        }

        public bool IsVaild
        {
            get
            {
                return system.linked;
            }
        }
        public bool IsLocal => false;
        byte[] GetFromBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(this.userpath);
        }
        byte[] GetToBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(this.path);
        }
        public unsafe void Tell(byte[] data)
        {
            if (data.Length == 0)
                throw new Exception("do not support  zero length bytearray.");

            byte[] from = GetFromBytes();
            byte[] to = GetToBytes();
            byte[] outbuf = new byte[from.Length + 1 + to.Length + 1 + data.Length];
            fixed (byte* pdiao = outbuf, pfrom = from, pto = to, pdata = data)
            {
                int seek = 0;
                outbuf[seek] = (byte)from.Length;
                seek++;

                Buffer.MemoryCopy(pfrom, pdiao + seek, from.Length, from.Length);
                seek += from.Length;

                outbuf[seek] = (byte)to.Length;
                seek++;

                Buffer.MemoryCopy(pto, pdiao + seek, to.Length, to.Length);
                seek += to.Length;

                Buffer.MemoryCopy(pdata, pdiao + seek, data.Length, data.Length);

            }
            _remotesystem.peer.Send(_remotesystem.PeerID, outbuf);
        }
        public void TellLocalObj(object obj)
        {
            throw new Exception("not support to telllocal obj on remote pipeline.");
        }

    }
}
