using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;

using UnityEngine;
using UnityUtility.CustomAttributes;

using static CommandHeadersGlossary;

[Serializable]
public class ArduinoConnectorManager : MonoBehaviour
{
    [Serializable]
    private class ConnectionSettings
    {
        public int ConnectorsToUse;
        public string SerialPort0;
        public string SerialPort1;
        public string SerialPort2;
    }

    [Button(nameof(GetAvailablePorts))]
    [SerializeField] private string m_connectionSettingsJsonPath;

    [NonSerialized] private bool m_ready = false;


    [NonSerialized] private bool m_enableArduinoConnection;

    [NonSerialized] private ArduinoConnector[] m_allArduinoConnectors;


    public void Awake()
    {
        string connectionSettingsJson = File.ReadAllText(Path.Combine(".", "ExternalAssets", m_connectionSettingsJsonPath));
        ConnectionSettings settings = JsonUtility.FromJson<ConnectionSettings>(connectionSettingsJson);

        m_ready = false;

        m_allArduinoConnectors = new ArduinoConnector[settings.ConnectorsToUse];

        InsertArduinoConenctor(0, settings.SerialPort0);
        InsertArduinoConenctor(1, settings.SerialPort1);
        InsertArduinoConenctor(2, settings.SerialPort2);

    }

    public void OnDestroy()
    {
        foreach(ArduinoConnector connector in m_allArduinoConnectors)
        {
            connector.OnMessageRecieved -= OnArduinoMessageRecieved;
            connector.Close();
        }

    }

    private void InsertArduinoConenctor(int index, string serialPort)
    {
        if (index >= m_allArduinoConnectors.Length)
        {
            return;
        }

        m_allArduinoConnectors[index] = new ArduinoConnector();
        m_allArduinoConnectors[index].Init(serialPort);
        m_allArduinoConnectors[index].OnMessageRecieved += OnArduinoMessageRecieved;
    }

    private void OnArduinoMessageRecieved(ArduinoConnector connector, byte[] buffer, int recievedBytesCount)
    {
        bool recievedEnoughDatas = true;
        while (connector.RecievedDatas.Count > 0 && recievedEnoughDatas)
        {
            byte queueHead = connector.RecievedDatas.Peek();
            switch (queueHead)
            {
                case BUTTON_STATE_HEADER:
                    recievedEnoughDatas &= TryProcessButtonState(connector.RecievedDatas);
                    break;

                case ERROR_HEADER:
                    recievedEnoughDatas &= TryProcessErrorDatas(connector.RecievedDatas);
                    break;

                default: // The header is discarded if unknown
                    Debug.LogError($"[{nameof(ArduinoConnectorManager)}] Unknown Header ({queueHead}) Next commands might not be working properly");
                    _ = connector.RecievedDatas.Dequeue();
                    break;
            }
        }
    }

    private bool TryProcessErrorDatas(Queue<byte> recievedDatas)
    {
        if (recievedDatas.Count < 2)
        {
            return false; // Should wait for more datas to arrive
        }

        _ = recievedDatas.Dequeue(); // Dequeue the header

        byte errorCode = recievedDatas.Dequeue();

        if (errorCode != 0)
        {
            Debug.LogError($"ArduinoError recieved : {errorCode}");
        }
        return true; // The command was processed and removed from the queue
    }

    private bool TryProcessButtonState(Queue<byte> recievedDatas)
    {
        if (recievedDatas.Count < 3)
        {
            return false; // Should wait for more datas to arrive
        }

        _ = recievedDatas.Dequeue(); // Dequeue the header

        byte buttonIndex = recievedDatas.Dequeue();
        byte buttonState = recievedDatas.Dequeue();

        InputManager.Instance.SetButtonState(buttonIndex, buttonState != 0);

        return true;
    }

    private void GetAvailablePorts()
    {
        Debug.Log("Available Ports :");
        foreach (string portName in SerialPort.GetPortNames())
        {
            Debug.Log($"- {portName}");
        }
    }

}
