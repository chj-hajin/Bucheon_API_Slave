using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections;
using System;

public class SocketClient : MonoBehaviour
{
    public string serverIP = "192.168.0.101";
    public int port = 9999;

    private TcpClient client;
    private NetworkStream stream;

    public VideoPlayerManager videoManager;

    void Start()
    {
        StartCoroutine(TryConnectLoop());
    }

    IEnumerator TryConnectLoop()
    {
        bool retry = true;
        while (retry)
        {
            try
            {
                client = new TcpClient();
                client.Connect(serverIP, port);
                stream = client.GetStream();
                Debug.Log("[Slave] ���� ���� ����");
                Send("READY");

                Thread receiveThread = new Thread(ReceiveLoop);
                receiveThread.IsBackground = true;
                receiveThread.Start();

                retry = false;
            }
            catch (Exception ex)
            {
                Debug.Log("[Slave] ���� ����. ��õ� ��..." + ex.Message);
            }

            if (retry)
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void Send(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("[Slave] READY ���� �Ϸ�");
        }
        catch (Exception e)
        {
            Debug.LogError("[Slave] ���� ����: " + e.Message);
        }
    }

    void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            int read = stream.Read(buffer, 0, buffer.Length);
            string msg = Encoding.UTF8.GetString(buffer, 0, read);
            Debug.Log("[Slave] ����: " + msg);

            if (msg.Contains("PLAY"))
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    videoManager.StartVideo();
                });
            }
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}