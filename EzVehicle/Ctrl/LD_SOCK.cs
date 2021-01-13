using SimpleTcp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EzVehicle.Ctrl
{
    internal class LD_SOCK : IDisposable
    {
        string _ip = null;
        private SimpleTcpClient sock = null;
        private ConcurrentQueue<byte> recvBuf = new ConcurrentQueue<byte>();
        private CancellationTokenSource cancelTock;
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<string> Evt_RecvdData;
        protected byte STX = 0x02, ETX = 0x03, LF = 0x0A, CR = 0x0D;
        public LD_SOCK()
        {

        }

        public void Dispose()
        {
            cancelTock.Cancel();
            sock?.Dispose();
            sock = null;
        }


        public async Task<bool> Conenct(string ip)
        {
            if (sock != null)
            {
                sock.Events.Connected -= Sock_Connected;
                sock.Events.DataReceived -= Sock_DataReceived;
                sock.Events.Disconnected -= Sock_Disconnected;
                sock.Dispose();
                sock = null;
            }
            sock = new SimpleTcpClient(_ip, 7171, false, null, null);
            sock.Events.Connected += Sock_Connected;
            sock.Events.DataReceived += Sock_DataReceived;
            sock.Events.Disconnected += Sock_Disconnected;
            try
            {
                cancelTock = new CancellationTokenSource();
                if (ip == "0.0.0.0")
                {
                    return false;
                }
                await Task.Run(() => sock.Connect());
                ParsRun();
                return true;
            }
            catch
            {
                sock.Dispose();
                sock = null;
                Debug.WriteLine($"{ip}:7171 Client 소켓연결 실패.");
                return false;
            }
        }

        
        private void Sock_Connected(object sender, EventArgs e)
        {
            Evt_Connection?.Invoke(this, true);
        }
        private void Sock_DataReceived(object sender, SimpleTcp.DataReceivedEventArgs e)
        {
            foreach (var item in e.Data)
            {
                recvBuf.Enqueue(item);
            }
        }
        private void Sock_Disconnected(object sender, EventArgs e)
        {
            Evt_Connection?.Invoke(this, false);
        }

        private async Task ParsRun()
        {
            await Task.Run(async () =>
            {
                var tempBuf = new List<byte>();
                while (!cancelTock.IsCancellationRequested)
                {
                    if (recvBuf.IsEmpty)
                    {
                        await Task.Delay(5);
                    }
                    else if (recvBuf.TryDequeue(out byte data))
                    {
                        tempBuf.Add(data);
                        if (data == LF)
                        {
                            var msg = Encoding.UTF8.GetString(tempBuf.ToArray());
                            tempBuf.Clear();
                            Evt_RecvdData?.Invoke(this, msg);
                        }
                    }
                }
            });
        }

        public void Send(string msg)
        {
            sock.Send(Encoding.Default.GetBytes(msg));
        }
    }
}
