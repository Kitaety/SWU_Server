using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Text.Json;
using SWU_Web.Data;
using System.Threading.Tasks;

namespace SWU_Web.SystemServer
{
    class ClientObject
    {
        public TcpClient client;
        public int Id { get; set; }


        public delegate void NoticeEventHandler(int id, string message, TypeMessage type);
        public event NoticeEventHandler OnNotice;

        public delegate void DisconnetedEventHandler(int id);
        public event DisconnetedEventHandler OnDisconnected;
        public bool isReadState = false;

        private System.Timers.Timer timerCheckConnect;
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public void Process()
        {
            timerCheckConnect = new System.Timers.Timer(1000);
            timerCheckConnect.Elapsed += CheckConnect;
            timerCheckConnect.Start();
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                while (true)
                {
                    if (client != null && client.Client!=null)
                    {
                        if (client.Client.Connected)
                        {
                            if (stream.DataAvailable)
                            {
                                isReadState = true;
                                timerCheckConnect.Stop();
                                StringBuilder builder = new StringBuilder();
                                int bytes = 0;
                                int length = 0;
                                byte[] buffer = new byte[4];
                                stream.Read(buffer, 0, 4);
                                length = BitConverter.ToInt32(buffer);
                                byte[] data = new byte[length];

                                bytes = stream.Read(data, 0, length);
                                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));

                                string message = builder.ToString();
                                SwuPackage package = JsonSerializer.Deserialize<SwuPackage>(message);

                                OnNotice?.Invoke(this.Id, message, TypeMessage.Info);

                                switch ((int)package.Operation)
                                {
                                    case (int)Operations.Authorization:
                                        {
                                            package.Error = !Authorization(package).Result;
                                            SendMessage(stream, package);
                                            break;
                                        }
                                    case (int)Operations.UpdateState:
                                        {
                                            package.Error = false;
                                            UpdateSystemValue(package).Wait();
                                            SendMessage(stream, package);
                                            break;
                                        }
                                    case (int)Operations.Disconnect:
                                        {
                                            client.Close();
                                            break;
                                        }

                                }
                                isReadState = false;
                                timerCheckConnect.Start();
                            }
                        }
                    }
                    else
                    {
                        this.timerCheckConnect.Stop();
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                OnNotice?.Invoke(this.Id, ex.Message, TypeMessage.Error);
                isReadState = false;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                this.timerCheckConnect.Stop();
                this.timerCheckConnect.Dispose();
                UpdateSystemStatus(0).Wait();
                OnDisconnected?.Invoke(this.Id);
            }
        }

        private void SendMessage(NetworkStream stream, SwuPackage package)
        {
            package.Data = "";
            string message = JsonSerializer.Serialize(package);
            byte[] mes = Encoding.UTF8.GetBytes(message);
            byte[] len = BitConverter.GetBytes(mes.Length);
            byte[] sendData = new byte[mes.Length + len.Length];
            len.CopyTo(sendData, 0);
            mes.CopyTo(sendData, len.Length);
            stream.Write(sendData, 0, sendData.Length);
            OnNotice?.Invoke(this.Id, "Send:" + message, TypeMessage.Warning);
        }

        private async Task<bool> Authorization(SwuPackage package)
        {
            IEnumerable<Detector> packageDetectors = JsonSerializer.Deserialize<List<PackageDetector>>(package.Data).Select(d => new Detector() { Id = d.Id, TypeDetectorId = d.Type });
            SWU_Web.Data.SwuSystem system = new Data.SwuSystem()
            {
                Id = package.Id,
                Detectors = packageDetectors
            };
            if (SystemDbContoller.Current.CheckSystem(system))
            {
                this.Id = package.Id;
                OnNotice?.Invoke(package.Id, "System authorized successfull", TypeMessage.Success);
                await UpdateSystemStatus(1);
                return true;
            }
            OnNotice?.Invoke(package.Id, "System not registered", TypeMessage.Error);
            return false;
        }
        private async Task UpdateSystemValue(SwuPackage package)
        {
            IEnumerable<PackageDetector> packageDetectors = JsonSerializer.Deserialize<List<PackageDetector>>(package.Data);
            await SystemDbContoller.Current.UpdateValueDetectorsSystem(this.Id, package.Date, packageDetectors);
        }
        private async Task UpdateSystemStatus(int status)
        {
            await SystemDbContoller.Current.UpdateSystemStatus(this.Id, status);
        }
        private void CheckConnect(Object source, System.Timers.ElapsedEventArgs e)
        {

            if (!this.isReadState && this.client.Connected)
            {
                try
                {
                    NetworkStream stream = this.client.GetStream();
                    stream.Write(new byte[0], 0, 0);
                }
                catch (System.IO.IOException ex)
                {
                    OnNotice?.Invoke(this.Id, "Faild check connect.", TypeMessage.Error);
                }
            }
        }
    }
}
