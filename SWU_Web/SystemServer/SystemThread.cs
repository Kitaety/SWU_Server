using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Logging;

namespace SWU_Web.SystemServer
{
    class SystemThread
    {
        public static SystemThread Instance = new SystemThread();
        public delegate void Event(string message);
        public event Event OnEvent;

        private ILogger<SystemThread> _logger;

        private TcpListener _tcpListener;
        private string _ipAdress { get; set; }
        private int _port { get; set; }
        private string _currentDirectory = Directory.GetCurrentDirectory();

        private Thread mainThread { get; set; }
        private List<Thread> clientsThread = new List<Thread>();

        public void LoadSettings(ILogger<SystemThread> logger)
        {
            _logger = logger;

            Notify(0,"Read settings file",TypeMessage.Info);
            if (!File.Exists(_currentDirectory + "/settings.json"))
            {
                Notify(0,"Create settings file", TypeMessage.Warning);

                byte[] data = Encoding.UTF8.GetBytes("{\"ipAdress\":\"127.0.0.1\",\"port\":\"8888\"}");
                using (FileStream file = File.Create(_currentDirectory + "/settings.json"))
                {
                    file.Write(data, 0, data.Length);
                    file.Close();
                }
            }

            using (StreamReader file = File.OpenText(_currentDirectory + "/settings.json"))
            {
                string json = file.ReadToEnd();
                Dictionary<string, string> map = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                this._ipAdress = map["ipAdress"] as string;
                this._port = int.Parse((map["port"] as string));
            }
            Notify(0,"Load settings complited.",TypeMessage.Success);
        }

        public void StartListener()
        {
            _tcpListener = new TcpListener(IPAddress.Parse(this._ipAdress), this._port);
            _tcpListener.Start();
            Notify(0, $"Server start. {this._ipAdress} {this._port}", TypeMessage.Success);
            Notify(0, $"Await clients...", TypeMessage.Info);

            mainThread = new Thread(new ThreadStart(() =>
            {
                while (!false)
                {
                    TcpClient client = this._tcpListener.AcceptTcpClient();
                    
                    Notify(0,"Client connected",TypeMessage.Warning);
                    ClientObject clientObject = new ClientObject(client);
                    clientObject.OnNotice += Notify;
                    clientObject.OnDisconnected += OnDisconnectedClient;

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                                 
                }
            }));
            mainThread.IsBackground = true;
            mainThread.Start();
        }

        private void OnDisconnectedClient(int id)
        {
            Notify(id, $"Diconnect", TypeMessage.Warning);
        }
        private void Notify(int id,string message, TypeMessage type)
        {
            string s = $"{DateTime.Now.ToString("[dd-MM-yyyy HH:mm:ss] ")} {(id == 0 ? "Server" : id.ToString())}: {message}";

            switch (type)
            {
                case TypeMessage.Info:
                    _logger.LogInformation(s);
                    break;
                case TypeMessage.Warning:
                    _logger.LogWarning(s);
                    break;
                case TypeMessage.Error:
                    _logger.LogError(s);
                    break;
                case TypeMessage.Success:
                    _logger.LogInformation(s);
                    break;
            }


          
            OnEvent?.Invoke(s);
        }

    }

    public enum TypeMessage
    {
        Info,
        Warning,
        Error,
        Success
    }
}
