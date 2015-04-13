using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using log4net;
using TwitchBot.Util;

namespace TwitchBot.IO.Tcp
{
    [ExcludeFromCodeCoverage]
    public class TcpClientAdapter : ITcpClient
    {
        private readonly TcpClient client = new TcpClient();
        private readonly ILog logger;
        private readonly Lazy<NetworkStream> networkStreamLazy;
        private readonly int port;
        private readonly string serverAddress;
        private readonly Lazy<StreamReader> streamReaderLazy;
        private readonly Lazy<StreamWriter> streamWriterLazy;

        public TcpClientAdapter(string serverAddress, int port, ILoggerFactory loggerFactory)
        {
            this.serverAddress = serverAddress;
            this.port = port;
            this.logger = loggerFactory.GetLogger<TcpClientAdapter>();
            this.networkStreamLazy = new Lazy<NetworkStream>(this.Connect);
            this.streamWriterLazy = new Lazy<StreamWriter>(this.CreateStreamWriter);
            this.streamReaderLazy = new Lazy<StreamReader>(this.CreateStreamReader);
        }

        public TextReader Reader
        {
            get { return this.streamReaderLazy.Value; }
        }

        public TextWriter Writer
        {
            get { return this.streamWriterLazy.Value; }
        }

        private NetworkStream NetworkStream
        {
            get { return this.networkStreamLazy.Value; }
        }

        public void Dispose()
        {
            if (this.client.Connected)
            {
                this.NetworkStream.Close();
                this.Writer.Dispose();
                this.Reader.Dispose();
                this.client.Close();
            }
        }

        private NetworkStream Connect()
        {
            this.logger.DebugFormat("Connecting to server {0}:{1}...", this.serverAddress, this.port);
            this.client.Connect(this.serverAddress, this.port);
            this.logger.Debug("Connected!");
            return this.client.GetStream();
        }

        private StreamReader CreateStreamReader()
        {
            return new StreamReader(this.NetworkStream);
        }

        private StreamWriter CreateStreamWriter()
        {
            return new StreamWriter(this.NetworkStream) { NewLine = "\r\n", AutoFlush = true };
        }
    }
}