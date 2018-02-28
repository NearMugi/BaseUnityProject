using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialHandler_Arduino : MonoBehaviour {

    #region Singleton

    private static SerialHandler_Arduino instance;

    public static SerialHandler_Arduino Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialHandler_Arduino)FindObjectOfType(typeof(SerialHandler_Arduino));

                if (instance == null)
                {
                    Debug.LogError(typeof(SerialHandler_Arduino) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton


    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived = delegate { };

    // Use this for initialization
    public string portName; //Arduinoのポート名
    public int baudRate;   //ボードレート
    
    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;

    private string message_;
    private bool isNewMessageReceived_ = false;

    public string DebugList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- SERIAL HANDLER ARDUINO---");
        sb.Append("\n");

        sb.Append("[");
        sb.Append(portName);
        sb.Append("]");
        sb.Append(serialPort_.IsOpen);
        sb.Append("\n");

        return sb.ToString();
    }

    void Awake()
    {
        Open();
    }

    void Update()
    {
        if (isNewMessageReceived_)
        {
            if(OnDataReceived != null ) OnDataReceived(message_);
        }
        isNewMessageReceived_ = false;
    }

    void OnDestroy()
    {
        Close();
    }

    public void Open()
    {
        serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);

        try
        {
            serialPort_.Open();

            isRunning_ = true;

            thread_ = new Thread(Read);
            thread_.Start();
        }
        catch(System.Exception ex)
        {
            Debug.LogWarning("[SerialHandler_Arduino Open()]" + ex.Message);
        }

    }

    public void Close()
    {
        isNewMessageReceived_ = false;
        isRunning_ = false;

        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }

    private void Read()
    {
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                message_ = serialPort_.ReadLine();
                isNewMessageReceived_ = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    public void Write(string message)
    {
        try
        {
            serialPort_.Write(message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void ReConnect()
    {
        Close();
        Open();
    }
}
