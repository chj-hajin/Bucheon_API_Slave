using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketClient : MonoBehaviour
{
    [Header("Network Settings")]
    public string serverIP = "192.168.x.y";
    public int port = 9999;

    [Header("References")]
    public VideoPlayerManager videoManager;

    private TcpClient client;
    private NetworkStream stream;

    // 받은 플레이 요청을 메인 스레드에서 처리하기 위한 큐
    private readonly Queue<int> clipQueue = new Queue<int>();
    private readonly object queueLock = new object();

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
            Debug.Log("[Slave] 서버 연결 성공");
            SendReady();
            StartReceiveLoop();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Slave] 연결 실패: " + e.Message);
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
                Debug.Log("[Slave] 전송: READY");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Slave] SendReady 오류: " + e);
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
                Debug.Log("[Slave] 수신: " + msg);

                int idx = -1;
                if (msg.StartsWith("PLAY_IMMEDIATE|"))
                {
                    idx = int.Parse(msg.Split('|')[1]);
                    Debug.Log($"[Slave] PLAY_IMMEDIATE 수신 → Clip {idx}");
                }
                else if (msg == "PLAY")
                {
                    idx = 1;
                    Debug.Log("[Slave] PLAY 수신 → 기본 재생 (Clip 1)");
                }

                if (idx > 0)
                {
                    lock (queueLock)
                        clipQueue.Enqueue(idx);
                }
            }
            catch (SocketException se)
            {
                Debug.LogError("[Slave] SocketException: " + se.Message);
                break;
            }
            catch (ObjectDisposedException ode)
            {
                Debug.LogError("[Slave] Stream이 닫혔습니다: " + ode.Message);
                break;
            }
            catch (ThreadAbortException tae)
            {
                Debug.LogWarning("[Slave] Thread가 중단되었습니다: " + tae.Message);
                break;
            }
            catch (Exception e)
            {
                Debug.LogError("[Slave] 알 수 없는 오류: " + e.Message);
                break;
            }
        }
    }

    void Update()
    {
        // 메인 스레드에서 큐 처리
        lock (queueLock)
        {
            while (clipQueue.Count > 0)
            {
                int idx = clipQueue.Dequeue();
                videoManager.PlayVideoClip(idx);
            }
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}
