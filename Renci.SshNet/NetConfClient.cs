﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet.Sftp;
using System.Text;
using Renci.SshNet.Common;
using System.Globalization;
using System.Threading;
using Renci.SshNet.NetConf;
using System.Xml;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public partial class NetConfClient : BaseClient
    {
        /// <summary>
        /// Holds SftpSession instance that used to communicate to the SFTP server
        /// </summary>
        private NetConfSession _netConfSession;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>The operation timeout.</value>
        public TimeSpan OperationTimeout { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        public NetConfClient(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
            this.AutomaticMessageIdHandling = true;
            this.OperationTimeout = new TimeSpan(0, 0, 0, 0, -1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        public NetConfClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        public NetConfClient(string host, string username, string password)
            : this(host, 22, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        public NetConfClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        public NetConfClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, 22, username, keyFiles)
        {
        }

        #endregion

        /// <summary>
        /// Gets NetConf server capabilities.
        /// </summary>
        public XmlDocument ServerCapabilities 
        {
            get
            {
                this.EnsureConnection();
                return this._netConfSession.ServerCapabilities;
            }
        }

        /// <summary>
        /// Gets NetConf client capabilities.
        /// </summary>
        public XmlDocument ClientCapabilities
        {
            get
            {
                this.EnsureConnection();
                return this._netConfSession.ClientCapabilities;
            }
        }

        public bool AutomaticMessageIdHandling { get; set; }

        public XmlDocument SendReceiveRpc(XmlDocument rpc)
        {
            this.EnsureConnection();
            return this._netConfSession.SendReceiveRpc(rpc, this.AutomaticMessageIdHandling);
        }

        public XmlDocument SendReceiveRpc(string xml)
        {
            var rpc = new XmlDocument();
            rpc.LoadXml(xml);
            return SendReceiveRpc(rpc);
        }

        public XmlDocument SendCloseRpc()
        {
            this.EnsureConnection();

            XmlDocument rpc = new XmlDocument();

            rpc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><rpc message-id=\"6666\" xmlns=\"urn:ietf:params:xml:ns:netconf:base:1.0\"><close-session/></rpc>");

            return this._netConfSession.SendReceiveRpc(rpc, this.AutomaticMessageIdHandling);
        }

        /// <summary>
        /// Called when client is connected to the server.
        /// </summary>
        protected override void OnConnected()
        {
            base.OnConnected();

            this._netConfSession = new NetConfSession(this.Session, this.OperationTimeout);

            this._netConfSession.Connect();            
        }

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            this._netConfSession.Disconnect();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected override void Dispose(bool disposing)
        {
            if (this._netConfSession != null)
            {
                this._netConfSession.Dispose();
                this._netConfSession = null;
            }

            base.Dispose(disposing);
        }
    }
}
