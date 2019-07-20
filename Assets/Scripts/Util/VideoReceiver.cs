using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// State object for receiving data from remote device.  
public class StateObject
{
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024 * 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();

    public void Reset() {
        buffer = new byte[BufferSize];
    }

}


public class VideoReceiver 
{
    private int index = 0;
    byte[] buffer = new byte[1024 * 1024];  // 缓存数组
    int size = 0 ;
    private byte[] CurrentFrame;



    private Socket mediaSocket;
    private Thread mediaThread;


    private readonly byte[] JpegHeader = new byte[] { 0xff, 0xd8 };
    private readonly byte[] JpegEnd = new byte[] { 0xff, 0xd9 };



    Action<byte[]> _onFrameReceived;

    public VideoReceiver(Action<byte[]> OnFrameReceived) {
        _onFrameReceived = OnFrameReceived;
    }

    NetworkStream ns;


    /// <summary>
    /// 开始接收
    /// </summary>
    public void Receive() {
        mediaSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        mediaSocket.BeginConnect(IPAddress.Parse("192.168.9.67"), 890, new AsyncCallback(ConnectCallBack), mediaSocket);
    }

    /// <summary>
    ///     关闭 Socket
    /// </summary>
    public void Close() {
        mediaSocket.Close();
    }



    void ConnectCallBack(IAsyncResult result) {
        try
        {
            //建立一个接受信息的byte数组
            byte[] receiveBuffer = new byte[4098];
            //从回调参数中获取上面Conntect方法中的socket对象
            Socket socket = result.AsyncState as Socket;
            //判断是否和服务端的socket建立连接


            StateObject state = new StateObject();
            state.workSocket = socket;

            if (socket.Connected)
            {
                //开始 异步接收服务端传来的信息，同样将socket传入回调方法的参数中
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallBack), state);
            }
            else { ConnectCallBack(result); }
        }
        catch
        {
            Console.WriteLine("连接出错");
        }
    }


    private bool hasStart = false;



    void ReceiveCallBack(IAsyncResult result)
    {
        StateObject state = (StateObject)result.AsyncState;
        Socket client = state.workSocket;

        //读取从服务器端传来的数据，EndReceive是关闭异步接收方法，同时读取数据
        int count = client.EndReceive(result);

        if (count > 0)
        {
            try
            {
                byte[] receiveBytes = state.buffer;  // 缓存数组

                // 判断是否已接受开启信号
                if (hasStart)
                {
                    int imageEnd = FindBytes(receiveBytes, JpegEnd);

                    // 存在结束信号
                    if (imageEnd != -1)
                    {
                        // 拼接数组
                        Array.Copy(receiveBytes, 0, buffer, size, imageEnd + 2);
                        size += (imageEnd + 2);

                        // 生成完整的当前帧数组, 调用成功回调
                        byte[] frame = new byte[size];
                        Array.Copy(buffer, 0, frame, 0, size);
                        CurrentFrame = frame;

                        byte[] temp = { CurrentFrame[CurrentFrame.Length - 2], CurrentFrame[CurrentFrame.Length - 1] };

                        _onFrameReceived.Invoke(CurrentFrame);


                        // 处理剩余的
                        buffer = new byte[1024 * 1024];
                        Array.Copy(receiveBytes, 0, buffer, 0, count - imageEnd);

                        hasStart = false;
                    }
                    // 不存在结束信号
                    else {

                        //Debug.Log("buffer size : " + buffer.Length);
                        //Debug.Log("receiveBytes size : " + receiveBytes.Length);

                        Array.Copy(receiveBytes, 0, buffer, size, count);
                        size += count;
                    }
                }
                else {
                    int imageStart = FindBytes(receiveBytes, JpegHeader);
                    size = 0;

                    // 检测到开始信号
                    if (imageStart != -1) {

                        // 标记已开始
                        hasStart = true;

                        // 填充数组
                        int l = count - imageStart;
                        Array.Copy(receiveBytes, imageStart, buffer, 0, l);
                        size = l;
                    }
                }
            }
            catch(Exception ex)
            {

                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
            }
        }


        state.Reset();

        // 递归监听服务器端是否发来信息，一旦服务器再次发送信息，客户端仍然可以接收到
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallBack, state);
    }





    public static int FindBytes(byte[] buff, byte[] search)
    {
        // enumerate the buffer but don't overstep the bounds
        for (int start = 0; start < buff.Length - search.Length; start++)
        {
            // we found the first character
            if (buff[start] == search[0])
            {
                int next;

                // traverse the rest of the bytes
                for (next = 1; next < search.Length; next++)
                {
                    // if we don't match, bail
                    if (buff[start + next] != search[next])
                        break;
                }

                if (next == search.Length)
                    return start;
            }
        }
        // not found
        return -1;
    }

}
