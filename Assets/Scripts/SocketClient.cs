

// SocketClient.cs (Slave)
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections;

public class SocketClient : MonoBehaviour
{
    [Header("Network Settings")]
    public string serverIP = "192.168.x.y";
    public int port = 9999;

    [Header("References")]
    public VideoPlayerManager videoManager;

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        StartCoroutine(TryConnectLoop());
    }

    IEnumerator TryConnectLoop()
    {
        while (true)
        {
            Debug.Log($"[Slave] Connecting to {serverIP}:{port}");
            if (AttemptConnect()) break;
            yield return new WaitForSeconds(1f);
        }
    }

    bool AttemptConnect()
    {
        try
        {
            client = new TcpClient();
            client.Connect(serverIP, port);
            stream = client.GetStream();
            Debug.Log("[Slave] ���� ���� ����");
            SendReady();
            StartReceiveLoop();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Slave] ���� ����: " + e.Message);
            return false;
        }
    }

    void SendReady()
    {
        try
        {
            if (stream != null && stream.CanWrite)
            {
                byte[] data = Encoding.UTF8.GetBytes("READY");
                stream.Write(data, 0, data.Length);
                Debug.Log("[Slave] ����: READY");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Slave] SendReady ����: " + e);
        }
    }

    void StartReceiveLoop()
    {
        var t = new Thread(ReceiveLoop) { IsBackground = true };
        t.Start();
    }

    void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            try
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) break;
                string msg = Encoding.UTF8.GetString(buffer, 0, read).Trim();
                Debug.Log("[Slave] ����: " + msg);

                if (msg.StartsWith("PLAY_IMMEDIATE|"))
                {
                    int idx = int.Parse(msg.Split('|')[1]);
                    Debug.Log($"[Slave] PLAY_IMMEDIATE ���� �� Clip {idx}");
                    videoManager.PlayVideoClip(idx);
                }
                else if (msg == "PLAY")
                {
                    Debug.Log("[Slave] PLAY ���� �� �⺻ ��� (Clip 1)");
                    videoManager.PlayVideoClip(1);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Slave] ReceiveLoop ����: " + e);
                break;
            }
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}

