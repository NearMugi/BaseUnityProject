using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using System.Text;

public class sampleSerial : MonoBehaviour
{

    public string portName = "COM16";
    public int baudRate = 115200;

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;
    private string lastrcvd = "";

    private string message_;
    private bool isNewMessageReceived_ = false;

    private List<Vector3> angleCache = new List<Vector3>();
    public int angleCacheNum = 10;
    public Vector3 angle
    {
        private set
        {
            angleCache.Add(value);
            if (angleCache.Count > angleCacheNum)
            {
                angleCache.RemoveAt(0);
            }
        }
        get
        {
            if (angleCache.Count > 0)
            {
                var sum = Vector3.zero;
                angleCache.ForEach(angle => { sum += angle; });
                return sum / angleCache.Count;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    void Start()
    {
        lastrcvd = "";
        Open();
    }

    void Update()
    {
        if (isNewMessageReceived_)
        {
            OnDataReceived(message_);
        }
    }
    void OnDestroy()
    {
        Close();
    }

    private void Open()
    {
        serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        serialPort_.ReadTimeout = 5000;

        serialPort_.Open();

        isRunning_ = true;

        thread_ = new Thread(Read);
        thread_.Start();
    }

    private void Read()
    {
        byte rcv;
        char tmp;

        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                rcv = (byte)serialPort_.ReadByte();

                if (rcv == '\t')
                {
                    message_ = lastrcvd;
                    Debug.LogFormat("textLine:{0}", message_);
                    lastrcvd = "";
                    isNewMessageReceived_ = true;
                }
                else
                {
                    tmp = (char)rcv;
                    //                   Debug.LogFormat("rcv:{0}", tmp.ToString());
                    lastrcvd = lastrcvd + tmp.ToString();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }

    private void Close()
    {
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

    void OnDataReceived(string message_)
    {
        Debug.Log(message_);
    }

}
