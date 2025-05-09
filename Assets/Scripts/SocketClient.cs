using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;
using UnityEngine;
using System.Collections;

public class SocketClient : MonoBehaviour
{
    [Header("Network Settings")]
    public string serverIP = "192.168.0.101";
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
        bool retry = true;
        while (retry)
        {
            if (AttemptConnect())
                retry = false;
            else
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
            Debug.Log("[Slave] 서버 연결 성공");
            Send("READY");
            StartReceiveThread();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Slave] 연결 실패: " + ex.Message);
            return false;
        }
    }

    void StartReceiveThread()
    {
        var t = new Thread(ReceiveLoop);
        t.IsBackground = true;
        t.Start();
    }

    void Send(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("[Slave] READY 전송 완료");
        }
        catch (Exception e)
        {
            Debug.LogError("[Slave] 전송 실패: " + e.Message);
        }
    }

    void ReceiveLoop()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            try
            {
                int read = stream.Read(buffer, 0, buffer.Length);
                string msg = Encoding.UTF8.GetString(buffer, 0, read);
                Debug.Log("[Slave] 수신: " + msg);

                if (msg.StartsWith("PLAY|"))
                {
                    var parts = msg.Split('|');
                    if (parts.Length > 1 &&
                        DateTime.TryParse(parts[1], null,
                            DateTimeStyles.RoundtripKind,
                            out DateTime target))
                    {
                        float delay = (float)(target - DateTime.Now).TotalSeconds;
                        if (delay < 0f) delay = 0f;
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            StartCoroutine(DelayedStart(delay))
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Slave] 수신 루프 오류: " + ex.Message);
                break;
            }
        }
    }

    IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        videoManager.StartVideo();
        Debug.Log("[Slave] 영상 재생 시작 (" + DateTime.Now.ToString("HH:mm:ss") + ")");
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}
