using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Timers;
using System.Linq;
using System.IO;
using System.Drawing;
using PcRemote.Models;
using PcRemote.Classes;

namespace PcRemote.classes
{
    public static class ServerGlobals
    {
        public static bool IsConnected = false;
        public static bool IsConnecting = false;
    }

    class Server
    {
        #region "variables"
        private readonly System.Timers.Timer MsgTimer;
        private readonly System.Timers.Timer MsgReconnect;

        private UserClient User;
        private readonly Pack Packer;
        private ICryptoTransform Encryptor;
        private ICryptoTransform Decryptor;
        private byte[] PublicKey;
        private readonly string Auth = "36OQoMfudmRV5aCljCRnV1Zg72tZUH";
        private readonly string FinalAuth = "oPd4B0P8WA4l3z6TQrY7";
        private readonly List<ServerCommand> Commands;
        bool SecureConnection = false;
        
        private const string Host = "10.0.2.2";
        private const int Port = 504;

        private readonly Random random = new(Guid.NewGuid().GetHashCode());
        #endregion

        #region "events"
        public delegate void StateChangedEventHandler(bool Connected);
        public event StateChangedEventHandler StateChanged;

        public delegate void NewMessageEventHandler(string Message);
        public event NewMessageEventHandler NewMessage;

        public delegate void NewImageEventHandler(byte[] NewImage);
        public event NewImageEventHandler NewImage;
        #endregion

        #region "proprieties"
        private RSACryptoServiceProvider RSA { get; set; } = new RSACryptoServiceProvider(2048);
        #endregion

        #region "enums"
        enum PackHeader : byte
        {
            Chatter = 0,
            Authorize = 2,
            HandShake = 3,
            FileTransfer = 7,
        }
        #endregion

        #region "functions"
        public Server()
        {
            User = new UserClient();
            Packer = new Pack();
            Commands = new List<ServerCommand>();

            User.ReadPacket += User_ReadPacket;
            User.ExceptionThrown += User_ExceptionThrown;
            User.StateChanged += User_StateChanged;

            User.KeepAlive = true;
            User.BufferSize = 32768;
            User.MaxPacketSize = 104857600;

            MsgTimer = new System.Timers.Timer { Interval = 100 };
            MsgTimer.Elapsed += MsgTimer_Elapsed;
            MsgTimer.Start();

            MsgReconnect = new System.Timers.Timer { Interval = 2000 };
            MsgReconnect.Elapsed += MsgReconnect_Elapsed;
            MsgReconnect.Start();
        }

        private string RandomString(int Length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, Length).Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }

        public void AddCommand(ServerCommand newCommand)
        {
            Commands.Add(newCommand);
        }

        public void ConnectToServer()
        {
            ServerGlobals.IsConnecting = true;

            if (User == null)
            {
                User = new UserClient();

                User.ReadPacket += User_ReadPacket;
                User.ExceptionThrown += User_ExceptionThrown;
                User.StateChanged += User_StateChanged;

                User.KeepAlive = true;
                User.BufferSize = 32768;
                User.MaxPacketSize = 104857600;
            }

            User.Connect(Host, Port);
        }

        public void DisconnectFromServer()
        {
            try
            {
                User.Disconnect();
            }
            catch
            {
                User.Dispose();

                User = new UserClient();

                User.ReadPacket += User_ReadPacket;
                User.ExceptionThrown += User_ExceptionThrown;
                User.StateChanged += User_StateChanged;

                User.KeepAlive = true;
                User.BufferSize = 32768;
                User.MaxPacketSize = 104857600;
            }

            ServerGlobals.IsConnecting = false;
            ServerGlobals.IsConnected = false;
        }

        private byte[] Encrypt(byte[] Data) => Encryptor.TransformFinalBlock(Data, 0, Data.Length);

        private byte[] Decrypt(byte[] Data) => Decryptor.TransformFinalBlock(Data, 0, Data.Length);

        private void SendPacket(params object[] Args)
        {
            byte[] Data = Packer.Serialize(Args);

            if (SecureConnection == true)
                Data = Encrypt(Data);

            User.Send(Data);
        }

        private void SendAuthorizePacket(string Auth)
        {
            SendPacket(Convert.ToByte(PackHeader.Authorize), Auth);
        }

        private void SendHandshakePacket(byte[] Data)
        {
            SendPacket(Convert.ToByte(PackHeader.HandShake), Data, "server");
        }

        public void SendChatterPacket(string Message)
        {
            SendPacket(Convert.ToByte(PackHeader.Chatter), Message, FinalAuth);
        }

        public void SendFileTransferPacket(byte[] FileData, string FileName)
        {
            SendPacket(Convert.ToByte(PackHeader.FileTransfer), FileName, FileData);
        }

        private void HandleHandshakePacket(object[] Values)
        {
            PublicKey = (byte[])Values[1];
            RSA = new RSACryptoServiceProvider(2048);
            RSA.ImportCspBlob(PublicKey);
            RijndaelManaged R = new RijndaelManaged();
            Encryptor = R.CreateEncryptor();
            Decryptor = R.CreateDecryptor();
            byte[] Data = Packer.Serialize(R.Key, R.IV);
            Data = RSA.Encrypt(Data, true);
            SendHandshakePacket(Data);

            SecureConnection = true;
        }

        private void HandleChatterPacket(object[] Values)
        {
            try
            {
                string Message = (string)Values[1];
                NewMessage?.Invoke(Message);
            }
            catch
            {
                // Error
            }
        }

        private void HandleFileTransferPacket(object[] Values)
        {
            string fileName = Values[1].ToString();
            byte[] fileData = (byte[])Values[2];

            if (fileName == "screencapture.jpg")
            {
                NewImage?.Invoke(fileData);
            }
            else
            {
                var pathToDownloads = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/PcRemoteDownloads";

                if (!Directory.Exists(pathToDownloads))
                    Directory.CreateDirectory(pathToDownloads);

                string pathToFile = pathToDownloads + @"/" + fileName;

                for (; ; )
                {
                    if (!File.Exists(pathToFile))
                        break;

                    pathToFile = pathToDownloads + @"/" + RandomString(3) + @"-" + fileName;
                }

                File.WriteAllBytes(pathToFile, fileData);
            }
        }

        private void SendFileAsync()
        {
            try
            {
                //@TODO FIX
                //FileData fileData = CrossFilePicker.Current.PickFile().Result;

                //string fileName = fileData.FileName;
                //byte[] contents = fileData.DataArray;

                //SendFileTransferPacket(contents, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception choosing file: " + ex.ToString());
            }
        }
        #endregion

        #region "handles"
        private void User_ReadPacket(UserClient Sender, byte[] Data)
        {
            try
            {
                if (SecureConnection == true)
                    Data = Decrypt(Data);
            }
            catch
            {
                DisconnectFromServer();
            }

            object[] Values = Packer.Deserialize(Data);
            if (Values == null || Values.Length == 0)
                return;
            switch ((PackHeader)Values[0])
            {
                case PackHeader.Chatter:
                    HandleChatterPacket(Values);
                    break;

                case PackHeader.HandShake:
                    HandleHandshakePacket(Values);
                    break;

                case PackHeader.FileTransfer:
                    HandleFileTransferPacket(Values);
                    break;
            }
        }

        private void User_ExceptionThrown(UserClient Sender, Exception Ex)
        {
            DisconnectFromServer();

            PublicKey = null;
            RSA = new RSACryptoServiceProvider(2048);
            SecureConnection = false;

            ServerGlobals.IsConnecting = false;
        }

        private void User_StateChanged(UserClient Sender, bool Connected)
        {
            if (Connected == true)
            {
                SendAuthorizePacket(Auth);
                ServerGlobals.IsConnected = true;
            }
            else
            {
                if (!ServerGlobals.IsConnected && !ServerGlobals.IsConnecting)
                {
                    PublicKey = null;
                    RSA = new RSACryptoServiceProvider(2048);
                    SecureConnection = false;

                    User.Connect(Host, Port);
                    ServerGlobals.IsConnecting = true;
                }
            }

            StateChanged?.Invoke(Connected);
        }

        private void MsgTimer_Elapsed(object sender, ElapsedEventArgs E)
        {
            if(Commands.Count() > 0)
            {
                ServerCommand commandToSend = Commands[0];
                Commands.RemoveAt(0);

                // W.I.P
                if (string.IsNullOrWhiteSpace(commandToSend.Text) == false)

                if (commandToSend.Text.Trim().ToLower() == "sendfile")
                {
                    SendFileAsync();
                    return;
                }

                SendChatterPacket(commandToSend.Text);
            }
        }

        private void MsgReconnect_Elapsed(object sender, ElapsedEventArgs E)
        {
            if (!ServerGlobals.IsConnected && !ServerGlobals.IsConnecting)
                ConnectToServer();
        }
        #endregion
    }
}
