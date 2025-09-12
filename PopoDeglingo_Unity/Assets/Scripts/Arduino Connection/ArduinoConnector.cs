using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

public class ArduinoConnector
{
    private const int BUFFER_SIZE = 32;
    private const int BAUD = 9600;
    private const int QUEUE_CAPACITY = 128;

    public event Action<ArduinoConnector, byte[], int> OnMessageRecieved;
    public event Action OnSynAckRecieved;

    public Queue<byte> RecievedDatas => m_recievedDatas;

    [NonSerialized] private SerialPort m_serialPort;
    [NonSerialized] private byte[] m_buffer;
    [NonSerialized] private bool m_open;

    [NonSerialized] private CancellationTokenSource m_awaitDataTaskCancellationTokenSource;
    [NonSerialized] private Task m_awaitDataTask;

    [NonSerialized] private Queue<byte> m_recievedDatas;


    public void Init(string portName)
    {
        m_buffer = new byte[BUFFER_SIZE];

        m_recievedDatas = new Queue<byte>(QUEUE_CAPACITY);

        m_serialPort = new SerialPort(portName, BAUD)
        {
            ReadTimeout = 50,

            RtsEnable = true,
            DtrEnable = true
        };

        try
        {
            m_serialPort.Open();
            m_open = true;

            m_awaitDataTaskCancellationTokenSource = new CancellationTokenSource();

            m_awaitDataTask = Task.Factory.StartNew(AwaitDatas, m_awaitDataTaskCancellationTokenSource.Token);

        }
        catch (IOException io)
        {
            Debug.LogException(io);
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Close()
    {
        m_awaitDataTaskCancellationTokenSource?.Cancel();
        m_awaitDataTask?.Dispose();
        m_serialPort?.Close();
        m_serialPort = null;
        m_open = false;
    }

    public void Send(Span<byte> buffer)
    {
        m_serialPort.BaseStream.Write(buffer);
        //m_serialPort.WriteLine(message);
        m_serialPort.BaseStream.Flush();
        Debug.Log($"Sent {buffer.Length} bytes");
    }

    private async Task AwaitDatas()
    {
        while (m_open)
        {
            int readBytesCount;
            try
            {
                readBytesCount = m_serialPort.Read(m_buffer, 0, BUFFER_SIZE);
            }
            catch (TimeoutException)
            {
                readBytesCount = 0;
            }

            if (readBytesCount != 0)
            {
                Debug.Log($"Recieved {readBytesCount} bytes");

                for (int i = 0; i < readBytesCount; i++)
                {
                    m_recievedDatas.Enqueue(m_buffer[i]);
                }

                try
                {
                    OnMessageRecieved?.Invoke(this, m_buffer, readBytesCount);
                }
                catch (Exception e) 
                {
                    Debug.LogException(e);
                }

            }
            await Task.Delay(10);
        }
    }
}

