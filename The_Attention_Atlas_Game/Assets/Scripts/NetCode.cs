using System.Collections.Generic;
using UnityEngine;
using RealtimeBuffer;
using System.Threading;
using DataStructures;
using UnityEngine.SceneManagement;

public class NetCode : MonoBehaviour {

    // ---- needs to be on same drive as program ---- need to recode otherwise
    // ---- data size needs to match the nScans - close command windows currently required
    // ---- needs to use IPv4 address rather than localhost(?)

    public static bool isFlush = true;

    public enum BID
    {
        frameData,
        trialData,
    }

    static public List<BufferD> buffer;
    public static string networkPath;

    private void Start()
    {
        StartNetwork();
    }

    public void StartNetwork()
    {
        // danger zone - clears command windows including analyses and buffer memory for the sake of not having too many windows open
        if (GameManager.currentLevel <= 0)
            CentralMemory.StartExternalProcess("restart");

        if (GetComponent<CentralMemory>() == null) // for testing with CentralMemory attached
        {
            CentralMemory.GamePaths.Setup();
        }

        Debug.LogFormat("CentralMemory.GamePaths.network:{0}", CentralMemory.GamePaths.network);

        string localIP;

        //localIP = GetLocalIP(); // requires wifi/network connection?
        //localIP = "localhost"; // doesn't run properly?
        localIP = "127.0.0.1"; // should work doesn't require network connection

        Debug.LogFormat("localIP:{0}", localIP);

        buffer = new List<BufferD>();
        buffer.Add(new BufferD(name:"raycastFrameData", host:localIP, port:1, isHosted: true, labels: FrameRecorder.header, nChans: FrameRecorder.header.Length)); // labels not implemented in the buffer, but length must with nChans for correct loading into Python
        buffer.Add(new BufferD(name: "trialData", host: localIP, port: 2, isHosted: true, labels: GameRunner.header, nChans: GameRunner.header.Length));

        if (SceneManager.GetActiveScene().name == "Testing")
        {
            TestBuffers();
        }
    }


    public static void TestBuffers()
    {
        print("TestBuffers");

        for (int i = 0; i < buffer.Count; i++)
        {
            Debug.Log(buffer[i].name);

            Header header = GetHeader(buffer[i]);

            float[,] data = SaveData.RandomFloatArray(Random.Range(100, 1000), buffer[i].nChans);
            PrintArrayShape(data);
            //PrintArray2D(data);

            PutData(i, data);
            //PutDataThread((int)BID.origin, data);
            //PutDataThread2(buffer[(int)BID.origin], data);
            
            float[,] dataReceived = GetBufferData(buffer[i]);
            PrintArrayShape(dataReceived);
            //PrintArray2D(data);
        }
    }

    public static float[,] PrintArray2D(float[,] data)
    {
        System.Random rnd = new System.Random();
        int nRows = data.GetLength(0);
        int nCols = data.GetLength(1);

        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
                Debug.LogFormat("i:{0}, j:{1}, data[i, j]: {2}", i, j, data[i, j]);
        }
        return data;
    }

    public static void PrintArrayShape(float[,] array)
    {
        Debug.LogFormat("{0}:{1}", array.GetLength(0), array.GetLength(1));
    }

    public static string GetLocalIP()
    {
        using (System.Net.Sockets.Socket socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            System.Net.IPEndPoint endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
            print(endPoint.Address.ToString());
            return endPoint.Address.ToString();
        }
    }

    public static class NetworkPath
    {
        public static string network = CentralMemory.GamePaths.network;
        public static string realtimeHack = network + "realtimeHack.10.11.17\\";
        public static string fileName = network + "IsBufferRunning\\IsBufferRunning\\bin\\Debug\\IsBufferRunning.exe";
    }

    public class BufferD // my first class!
    {
        public string name;

        public string host;
        public int port;
        public string[] labels;
        public int nChans;

        public bool isHosted; // true = if local, false if remote
        public bool isRunning = false;

        public UnityBuffer socket = new UnityBuffer();

        public BufferD(string name, string host, int port, bool isHosted, string[] labels, int nChans)
        {
            this.name = name;
            this.host = host;
            this.port = port;
            this.labels = labels;

            print(this.host + ": " + this.port.ToString() + ": " + this.name);

            this.isHosted = isHosted;
            this.nChans = nChans;

            isRunning = CheckIsRunningNew();

            if (this.isHosted)
            {
                if (!isRunning)
                {
                    StartBuffer();
                }
                if (isFlush)
                {
                    Flush();
                    PutHeader();
                }
            }
        }

        public bool CheckIsRunning() // requires editor, doesn't work in build
        {
            bool isBufferRunning = false;

            var proc = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo // get process by window name - super dodge - also cannot run Process from game build
                {
                    FileName = NetworkPath.fileName,
                    Arguments = host + " " + port,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();

            string line = "";

            while (!proc.StandardOutput.EndOfStream)
            {
                line = proc.StandardOutput.ReadLine();
            }

            if (line == "False")
            {
                isBufferRunning = false;
            }
            else if (line == "True")
            {
                isBufferRunning = true;
            }
            else
            {
                isBufferRunning = false;
            }
            return isBufferRunning;
        }

        bool CheckIsRunningNew()
        {
            Header header = GetHeader(this);

            bool isBufferRunning;

            if (header == null)
                isBufferRunning = false;
            else
                isBufferRunning = true;

            return isBufferRunning;
        }

        public void StartBuffer()
        {
            print("NetCode2.StartBuffer()");
            string bufferCommand = "/k cd " + NetworkPath.realtimeHack + " & buffer.exe " + host + " " + port + " -&";

            if (Application.isEditor)
            {
                //System.Diagnostics.Process.Start("CMD.exe", bufferCommand); // fast startup
                CentralMemory.StartExternalProcess(bufferCommand); // slow startup
            }

            else
            {
                CentralMemory.StartExternalProcess(bufferCommand); // slow startup
            }

            PutHeader();
        }

        public void PutHeader()
        {
            while (true) // solution for slow startup of buffer due to build-friendly buffer initialisation method - beware may infinitely loop under some circumstances
            {
                if (socket.connect(host, port))
                {
                    try
                    {
                        Header header = socket.getHeader();
                        header.nChans = nChans;
                        header.dataType = DataType.FLOAT32;
                        //header.labels = labels; not working
                        socket.putHeader(header);
                        isRunning = true;
                        print("header added!");
                        break;
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        Debug.Log("problem adding header");
                    }

                    socket.disconnect();
                    isRunning = false;
                }
                else
                {
                    print("StartBuffer: !socket.connect(host, port)");
                    Thread.Sleep(100);
                }
            }
        }

        public void Flush()
        {
            if (socket.connect(host, port))
            {
                try
                {
                    socket.flushData();
                }
                catch (System.Net.Sockets.SocketException) { }
                socket.disconnect();
            }
        }
    }

    public static void PutDataThread(int idx, float[,] dataToPut)
    {
        new Thread(() => // Create a new Thread
        {
            PutData(idx, dataToPut);
        }).Start(); // Start the Thread
    }

    public static void PutDataThread2(BufferD buffer, float[,] dataToPut)
    {
        new Thread(() => // Create a new Thread
        {
            if (buffer.socket.connect(buffer.host, buffer.port))
            {
                buffer.socket.putData(dataToPut);
                //buffer[idx].socket.disconnect();
            }
        }).Start(); // Start the Thread
    }

    public static void PutDataThreadNew(int idx, float[,] dataToPut)
    {
        new Thread(() => // Create a new Thread
        {
            PutDataNew(idx, dataToPut);
        }).Start(); // Start the Thread
    }

    public static void PutData(int idx, float[,] dataToPut)
    {
        if (buffer[idx].socket.connect(buffer[idx].host, buffer[idx].port))
        {
            buffer[idx].socket.putData(dataToPut);
            //buffer[idx].socket.disconnect();
        }
        else
        {
            Debug.LogWarning("PutData connection failed!");
        }
    }


    public static void PutDataNew(int idx, float[,] dataToPut)
    {
        if (buffer[idx].socket.connect(buffer[idx].host, buffer[idx].port))
        {
            try
            {
                buffer[idx].socket.putData(dataToPut);
                buffer[idx].socket.disconnect();
            }
            catch (System.Net.Sockets.SocketException) {
                Debug.LogWarning("PutData failed!");
            }
        }
        else
        {
            buffer[idx].socket.disconnect();
            Debug.LogWarning("Buffer connection failed!");
        }
    }


    public static float[,] GetBufferData(BufferD buffer)
    {
        float[,] data = new float[,] { };

        if (buffer.socket.connect(buffer.host, buffer.port))
        {
            try
            {
                Header hdr = buffer.socket.getHeader();
                
                if (hdr.nSamples > 0)
                {
                    data = buffer.socket.getFloatData(0, hdr.nSamples - 1);
                }
                buffer.socket.disconnect();
            }
            catch (System.Net.Sockets.SocketException) { }
        }
        return data;
    }
    public static Header GetHeader(BufferD buffer, bool isDebug = false)
    {
        Header header = null;

        if (buffer.socket.connect(buffer.host, buffer.port))
        {
            try
            {
                header = buffer.socket.getHeader();

                if (isDebug)
                {
                    Debug.LogFormat("header == null:{0}", header == null);
                    Debug.LogFormat("header.nSamples:{0}", header.nSamples);
                    Debug.LogFormat("header.nChans:{0}", header.nChans);
                }
            }
            catch (System.Net.Sockets.SocketException) { Debug.LogError("failed to get header!"); }
        }
        else
        {
            print("connection failed!");
        }
        return header;
    }

}