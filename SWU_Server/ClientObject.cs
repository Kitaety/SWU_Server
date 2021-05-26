using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
namespace SWU_Server
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
                byte[] data = new byte[64]; // буфер для получаемых данных
                while (true)
                {
                    if (client != null && client.Client.Connected)
                    {
                        if (stream.DataAvailable)
                        {
                            isReadState = true;
                            timerCheckConnect.Stop();
                            // получаем сообщение
                            StringBuilder builder = new StringBuilder();
                            int bytes = 0;
                            do
                            {
                                bytes = stream.Read(data, 0, data.Length);
                                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                            }
                            while (stream.DataAvailable);

                            string message = builder.ToString();
                            int id = 0;
                            if(int.TryParse(message,out id))
                            {
                                this.Id = id;
                            }
                            OnNotice?.Invoke(this.Id, message, TypeMessage.Info);
                            isReadState = false;
                            timerCheckConnect.Start();
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
                OnDisconnected?.Invoke(this.Id);
            }
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
                    OnNotice?.Invoke(this.Id, "Faild check connect.", TypeMessage.Info);
                }
            }
        }
    }
}
