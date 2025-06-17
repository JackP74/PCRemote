using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;

namespace PcRemote.Classes
{
    sealed class UserClient
    {
        public event ExceptionThrownEventHandler ExceptionThrown;
        public delegate void ExceptionThrownEventHandler(UserClient Sender, Exception Ex);
        public event StateChangedEventHandler StateChanged;
        public delegate void StateChangedEventHandler(UserClient Sender, bool Connected);
        public event ReadPacketEventHandler ReadPacket;
        public delegate void ReadPacketEventHandler(UserClient Sender, byte[] Data);
        public event ReadProgressChangedEventHandler ReadProgressChanged;
        public delegate void ReadProgressChangedEventHandler(UserClient Sender, double Progress, int BytesRead, int BytesToRead);
        public event WriteProgressChangedEventHandler WriteProgressChanged;
        public delegate void WriteProgressChangedEventHandler(UserClient Sender, double Progress, int BytesWritten, int BytesToWrite);
        public event WritePacketEventHandler WritePacket;
        public delegate void WritePacketEventHandler(UserClient Sender, int Size);
        private ushort CBufferSize = 8192;
        private int CMaxPacketSize = 10485760;
        private bool CKeepAlive = true;
        private object CUserState;
        private IPEndPoint CEndPoint;
        private bool CConnected;
        private AsyncOperation O;
        private Socket Handle;
        private int SendIndex;
        private byte[] SendBuffer;
        private int ReadIndex;
        private byte[] ReadBuffer;
        private Queue<byte[]> SendQueue;
        private SocketAsyncEventArgs[] Items;
        private bool[] Processing = new bool[2];

        private bool DisposedValue;
        private void OnExceptionThrown(Exception ex)
        {
            ExceptionThrown?.Invoke(this, ex);
        }

        private void OnStateChanged(bool connected)
        {
            StateChanged?.Invoke(this, connected);
        }

        private void OnReadPacket(byte[] data)
        {
            ReadPacket?.Invoke(this, data);
        }

        private void OnReadProgressChanged(double progress, int bytesRead, int bytesToRead)
        {
            ReadProgressChanged?.Invoke(this, progress, bytesRead, bytesToRead);
        }

        private void OnWritePacket(int size)
        {
            WritePacket?.Invoke(this, size);
        }

        private void OnWriteProgressChanged(double progress, int bytesWritten, int bytesToWrite)
        {
            WriteProgressChanged?.Invoke(this, progress, bytesWritten, bytesToWrite);
        }

        public ushort BufferSize
        {
            get { return CBufferSize; }
            set
            {
                if (value < 1)
                {
                    throw new Exception("Value must be greater than 0.");
                }
                else
                {
                    CBufferSize = value;
                }
            }
        }

        public int MaxPacketSize
        {
            get { return CMaxPacketSize; }
            set
            {
                if (value < 1)
                {
                    throw new Exception("Value must be greater than 0.");
                }
                else
                {
                    CMaxPacketSize = value;
                }
            }
        }

        public bool KeepAlive
        {
            get { return CKeepAlive; }
            set
            {
                if (CConnected)
                {
                    throw new Exception("Unable to change this option while connected.");
                }
                else
                {
                    CKeepAlive = value;
                }
            }
        }

        public object UserState
        {
            get { return CUserState; }
            set { CUserState = value; }
        }

        public IPEndPoint EndPoint
        {
            get
            {
                if (CEndPoint != null)
                {
                    return CEndPoint;
                }
                else
                {
                    return new IPEndPoint(IPAddress.None, 0);
                }
            }
        }

        public bool Connected
        {
            get { return CConnected; }
        }

        public UserClient()
        {
            O = AsyncOperationManager.CreateOperation(null);
        }

        public void Connect(string host, ushort port)
        {
            try
            {
                Disconnect();
                Initialize();
                IPAddress IP = IPAddress.None;
                if (IPAddress.TryParse(host, out IP))
                {
                    DoConnect(IP, port);
                }
                else
                {
                    Dns.BeginGetHostEntry(host, EndGetHostEntry, port);
                }
            }
            catch (Exception ex)
            {
                O.Post(x => OnExceptionThrown((Exception)x), ex);
                Disconnect();
            }
        }

        private void EndGetHostEntry(IAsyncResult r)
        {
            try
            {
                DoConnect(Dns.EndGetHostEntry(r).AddressList[0], (ushort)r.AsyncState);
            }
            catch (Exception ex)
            {
                O.Post(x => OnExceptionThrown((Exception)x), ex);
                Disconnect();
            }
        }

        private void DoConnect(IPAddress ip, ushort port)
        {
            try
            {
                Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };
                if (CKeepAlive)
                {
                    Handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 20000);
                }
                Items[0].RemoteEndPoint = new IPEndPoint(ip, port);
                if (!Handle.ConnectAsync(Items[0]))
                {
                    Process(null, Items[0]);
                }
            }
            catch (Exception ex)
            {
                O.Post(x => OnExceptionThrown((Exception)x), ex);
                Disconnect();
            }
        }

        private void Initialize()
        {
            Processing = new bool[2];
            SendIndex = 0;
            ReadIndex = 0;
            SendBuffer = new byte[-1 + 1];
            ReadBuffer = new byte[-1 + 1];
            SendQueue = new Queue<byte[]>();
            Items = new SocketAsyncEventArgs[2];
            Items[0] = new SocketAsyncEventArgs();
            Items[1] = new SocketAsyncEventArgs();
            Items[0].Completed += Process;
            Items[1].Completed += Process;
        }

        private void Process(object s, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    switch (e.LastOperation)
                    {
                        case SocketAsyncOperation.Connect:
                            CEndPoint = (IPEndPoint)Handle.RemoteEndPoint;
                            CConnected = true;
                            Items[0].SetBuffer(new byte[CBufferSize], 0, CBufferSize);
                            O.Post(state => OnStateChanged(true), null);
                            if (!Handle.ReceiveAsync(e))
                            {
                                Process(null, e);
                            }
                            break;
                        case SocketAsyncOperation.Receive:
                            if (!CConnected)
                                return;
                            if (!(e.BytesTransferred == 0))
                            {
                                HandleRead(e.Buffer, 0, e.BytesTransferred);
                                if (!Handle.ReceiveAsync(e))
                                {
                                    Process(null, e);
                                }
                            }
                            else
                            {
                                Disconnect();
                            }
                            break;
                        case SocketAsyncOperation.Send:
                            if (!CConnected)
                                return;
                            bool EOS = false;
                            SendIndex += e.BytesTransferred;
                            O.Post(WriteProgressChangedCallback, new object[] {
                            (SendIndex / SendBuffer.Length) * 100,
                            SendIndex,
                            SendBuffer.Length
                        });
                            if ((SendIndex >= SendBuffer.Length))
                            {
                                EOS = true;
                                O.Post(x => OnWritePacket((int)x), SendBuffer.Length - 4);
                            }
                            if (SendQueue.Count == 0 && EOS)
                            {
                                Processing[1] = false;
                            }
                            else
                            {
                                HandleSendQueue();
                            }
                            break;
                    }
                }
                else
                {
                    O.Post(x => OnExceptionThrown((SocketException)x), new SocketException((int) e.SocketError));
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                O.Post(x => OnExceptionThrown((Exception)x), ex);
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (Processing[0])
            {
                return;
            }
            else
            {
                Processing[0] = true;
            }
            bool Raise = CConnected;
            CConnected = false;
            if (Handle != null)
            {
                Handle.Close();
            }
            if (SendQueue != null)
            {
                SendQueue.Clear();
            }
            SendBuffer = new byte[-1 + 1];
            ReadBuffer = new byte[-1 + 1];
            if (Raise)
            {
                O.Post(state => OnStateChanged(false), null);
            }
            if (Items != null)
            {
                Items[0].Dispose();
                Items[1].Dispose();
            }
            CUserState = null;
            CEndPoint = null;
        }

        public void Send(byte[] data)
        {
            if (!CConnected)
                return;
            SendQueue.Enqueue(data);
            if (!Processing[1])
            {
                Processing[1] = true;
                HandleSendQueue();
            }
        }

        private void HandleSendQueue()
        {
            try
            {
                if (SendIndex >= SendBuffer.Length)
                {
                    SendIndex = 0;
                    SendBuffer = Header(SendQueue.Dequeue());
                }
                int Write = Math.Min(SendBuffer.Length - SendIndex, CBufferSize);
                Items[1].SetBuffer(SendBuffer, SendIndex, Write);
                if (!Handle.SendAsync(Items[1]))
                {
                    Process(null, Items[1]);
                }
            }
            catch (Exception ex)
            {
                O.Post(x => OnExceptionThrown((Exception)x), ex);
                Disconnect();
            }
        }

        private static byte[] Header(byte[] data)
        {
            byte[] T = new byte[data.Length + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, T, 0, 4);
            Buffer.BlockCopy(data, 0, T, 4, data.Length);
            return T;
        }

        private void HandleRead(byte[] data, int index, int length)
        {
            if (ReadIndex >= ReadBuffer.Length)
            {
                ReadIndex = 0;
                if (data.Length < 4)
                {
                    O.Post(state => OnExceptionThrown(new Exception("Missing or corrupt packet header.")), null);
                    Disconnect();
                    return;
                }
                int PacketSize = BitConverter.ToInt32(data, index);
                if (PacketSize > CMaxPacketSize)
                {
                    O.Post(state => OnExceptionThrown(new Exception("Packet size exceeds MaxPacketSize.")), null);
                    Disconnect();
                    return;
                }
                Array.Resize(ref ReadBuffer, PacketSize);
                index += 4;
            }
            int Read = Math.Min(ReadBuffer.Length - ReadIndex, length - index);
            Buffer.BlockCopy(data, index, ReadBuffer, ReadIndex, Read);
            ReadIndex += Read;
            O.Post(ReadProgressChangedCallback, new object[] {
            (ReadIndex / ReadBuffer.Length) * 100,
            ReadIndex,
            ReadBuffer.Length
        });
            if (ReadIndex >= ReadBuffer.Length)
            {
                byte[] BufferClone = new byte[ReadBuffer.Length];
                Buffer.BlockCopy(ReadBuffer, 0, BufferClone, 0, ReadBuffer.Length);
                O.Post(x => OnReadPacket((byte[])x), BufferClone);
            }
            if (Read < (length - index))
            {
                HandleRead(data, index + Read, length);
            }
        }

        private void ReadProgressChangedCallback(object arg)
        {
            object[] Params = (object[])arg;
            OnReadProgressChanged((int)Params[0], (int)Params[1], (int)Params[2]);
        }

        private void WriteProgressChangedCallback(object arg)
        {
            object[] Params = (object[])arg;
            OnWriteProgressChanged((int)Params[0], (int)Params[1], (int)Params[2]);
        }

        private void Dispose(bool disposing)
        {
            if (!DisposedValue && disposing)
                Disconnect();
            DisposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}