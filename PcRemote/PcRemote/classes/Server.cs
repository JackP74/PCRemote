using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Timers;
using System.Linq;
using System.IO;
using System.Drawing;
using Plugin.FilePicker;

using PcRemote.Models;
using PcRemote.Classes;

namespace PcRemote.classes
{
    class Server
    {
        #region "variables"
        System.Timers.Timer MsgTimer;

        private UserClient User;
        private Pack Packer;
        private ICryptoTransform Encryptor;
        private ICryptoTransform Decryptor;
        private byte[] PublicKey;
        private readonly string Auth = "%auth%";
        private readonly string FinalAuth = "%finalauth%";
        List<ServerCommand> Commands;
        bool SecureConnection = false;

        private const string Host = "%host%";
        private const int Port = %port%;
        private bool ForReconnect = false;

        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        #endregion

        #region "events"
        public delegate void StateChangedEventHandler(bool Connected);
        public event StateChangedEventHandler StateChanged;

        public delegate void NewMessageEventHandler(string Message);
        public event NewMessageEventHandler NewMessage;

        public delegate void NewImageEventHandler(System.Drawing.Image Message);
        public event NewImageEventHandler NewImage;
        #endregion

        #region "proprieties"
        private RSACryptoServiceProvider RSA1 { get; set; } = new RSACryptoServiceProvider(2048);
        #endregion

        #region "enums"
        enum PackHeader : byte
        {
            Chatter = 0,
            Authorize = 2,
            HandShake = 3,
            Image = 4,
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
            User.MaxPacketSize = 1048576;

            MsgTimer = new System.Timers.Timer { Interval = 100 };
            MsgTimer.Elapsed += MsgTimer_Elapsed;
            MsgTimer.Start();
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
            User.Connect(Host, Port);
        }

        public void DisconnectFromServer()
        {
            User.Disconnect();
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
            SendPacket(Convert.ToByte(PackHeader.HandShake), Data);
        }

        private void SendChatterPacket(string Message)
        {
            SendPacket(Convert.ToByte(PackHeader.Chatter), Message, FinalAuth);
        }

        private void SendImagePacket(Bitmap Message)
        {
            Byte[] MessageByte = ImageToByte(Message);

            SendPacket(Convert.ToByte(PackHeader.Chatter), MessageByte, FinalAuth);
        }

        private void SendFileTransferPacket(string FilePath)
        {
            string FileName = new FileInfo(FilePath).Name;
            byte[] FileData = File.ReadAllBytes(FilePath);

            SendPacket(Convert.ToByte(PackHeader.FileTransfer), FileName, FileData);
        }

        private void SendFileTransferPacket(byte[] FileData, string FileName)
        {
            SendChatterPacket("this");
            SendPacket(Convert.ToByte(PackHeader.FileTransfer), FileName, FileData);
        }

        private void HandleHandshakePacket(object[] Values)
        {
            PublicKey = (byte[])Values[1];
            RSA1 = new RSACryptoServiceProvider(2048);
            RSA1.ImportCspBlob(PublicKey);
            RijndaelManaged R = new RijndaelManaged();
            Encryptor = R.CreateEncryptor();
            Decryptor = R.CreateDecryptor();
            byte[] Data = Packer.Serialize(R.Key, R.IV);
            Data = RSA1.Encrypt(Data, true);
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
            catch (Exception ex)
            {
                // Error
            }
        }

        private void HandleImagePacket(object[] Values)
        {
            try
            {
                Byte[] Message = (Byte[])Values[1];

                Int32 Width = (Int32)Values[2];
                Int32 Height = (Int32)Values[3];

                //Int32 R = (Int32)Values[4];
                //Int32 G = (Int32)Values[5];
                //Int32 B = (Int32)Values[6];

                System.Drawing.Image MessageImg = ByteToImage(Message, Width, Height);

                //

                //Int32 RealR = MessageImg.GetPixel(30, 30).R;
                //Int32 RealG = MessageImg.GetPixel(30, 30).R;
                //Int32 RealB = MessageImg.GetPixel(30, 30).R;

                NewImage?.Invoke(MessageImg);
            }
            catch (Exception ex)
            {
                NewMessage?.Invoke(ex.Message);
            }
        }

        private void HandleFileTransferPacket(object[] Values)
        {
            string fileName = Values[1].ToString();
            byte[] fileData = (byte[])Values[2];

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
               User.Disconnect();
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

                case PackHeader.Image:
                    HandleImagePacket(Values);
                    break;

                case PackHeader.FileTransfer:
                    HandleFileTransferPacket(Values);
                    break;
            }
        }

        private void User_ExceptionThrown(UserClient Sender, Exception Ex)
        {
            try
            {
                User.Disconnect();
                User.Dispose();
            }
            catch { }
            PublicKey = null;
            RSA1 = new RSACryptoServiceProvider(2048);
            SecureConnection = false;
            User.KeepAlive = true;
            User.BufferSize = 32768;
            User.MaxPacketSize = 1048576;
            ForReconnect = true;
        }

        private void User_StateChanged(UserClient Sender, bool Connected)
        {
            if (Connected == true)
            {
                SendAuthorizePacket(Auth);
            }
            else
            {
                PublicKey = null;
                RSA1 = new RSACryptoServiceProvider(2048);
                SecureConnection = false;
                User.KeepAlive = true;
                User.BufferSize = 32768;
                User.MaxPacketSize = 1048576;
                User.Connect(Host, Port);
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
                //if(string.IsNullOrWhiteSpace(commandToSend.Text) == false)

                //    if (commandToSend.Text.Trim().ToLower() == "sendfile")
                //    {

                //        Thread sendThread = new Thread(async () =>
                //            {
                //                var file = await CrossFilePicker.Current.PickFile();

                //                SendChatterPacket(file.FileName);

                //                //var result = file.Result;

                //                //SendFileTransferPacket(result.DataArray, result.FileName);
                //            });

                //        Task task = new Task(async () => {
                //            var file = await CrossFilePicker.Current.PickFile();

                //            SendChatterPacket(file.FileName);
                //        });

                //        //var file = CrossFilePicker.Current.PickFile();

                //        //SendChatterPacket(file.Result.FileName);

                //        return;
                //    }

                    SendChatterPacket(commandToSend.Text);
            }
        }

        private Byte[] ImageToByte(Bitmap ToConvert)
        {
            //ByteBuffer byteBuffer = ByteBuffer.Allocate(ToConvert.ByteCount);

            //ToConvert.CopyPixelsToBuffer(byteBuffer);

            //byte[] bytes = byteBuffer.ToArray<byte>();

            //return bytes;

            MemoryStream ms = new MemoryStream();
            ToConvert.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();

        }

        private System.Drawing.Image ByteToImage(Byte[] ToConvert, Int32 Width, Int32 Height)
        {
            //Bitmap finalImage = BitmapFactory.DecodeByteArray(ToConvert, 0, ToConvert.Length);

            //ByteBuffer byteBuffer = ByteBuffer.Wrap(ToConvert);

            //byteBuffer.Rewind();

            //Bitmap Image = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);

            //Image.CopyPixelsFromBuffer(byteBuffer);

            //return Image;

            MemoryStream ms = new MemoryStream(ToConvert);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

        #endregion
    }
}
