using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class SerialHandler : MonoBehaviour {

    #region Singleton

    private static SerialHandler instance;

    public static SerialHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (SerialHandler)FindObjectOfType(typeof(SerialHandler));

                if (instance == null)
                {
                    Debug.LogError(typeof(SerialHandler) + "is nothing");
                }
            }
            return instance;
        }
    }

    #endregion Singleton

    public enum Def_PortName
    {
        NONE = 0,
        SPONGE = 1,
        UNIPOLAR = 0xFF,
        AIR = 0xFE,
        DCMOTOR = 0xFD,
        POTENTIONMETER = 0xFC,
        MPU6050 = 0xFB,
        BLUETOOTH = 0xFA,
    }

    public List<serial_unit> PortList;

    public class serial_unit
    {
        public delegate void SerialDataReceivedEventHandler(string[] message);
        public event SerialDataReceivedEventHandler OnDataReceived = delegate { };

        // Use this for initialization
        public Def_PortName portName_def;
        public string UserName; //分かりやすい名前
        public string portName; //ポート名
        public int baudRate;   //ボードレート

        private SerialPort serialPort_;
        private Thread _thread;
        private bool isRunning_ = false;

        private static int MESSAGE_SIZE = 20;//大きめに確保
        private string[] message_ = new string[MESSAGE_SIZE];
        private int readCnt;
        private bool isNewMessageReceived_ = false;
        private bool isMessageProcessing = false;

        [HideInInspector]
        public int err; //デバッグ用　通信が途切れた回数をカウント
        [HideInInspector]
        public string errMsg;


        public bool Open()
        {
            try
            {
                serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
                serialPort_.ReadTimeout = 1;
                serialPort_.DtrEnable = true;
                serialPort_.RtsEnable = true;
                serialPort_.Open();

                Debug.LogWarning("[Open]" + serialPort_.PortName + ", IsOpen " + serialPort_.IsOpen);

                isRunning_ = true;

                _thread = new Thread(Read);
                _thread.Start();

                errMsg = "OPEN";
                return true;
            }
            catch (System.IO.IOException ex)
            {
                errMsg = ex.Message;
                Debug.LogError(errMsg);
                err++;
            }
            catch (System.Exception ex)
            {
                errMsg = ex.Message;
                Debug.LogError(errMsg);
                err++;
            }
            return false;
        }

        public bool Close()
        {
            try
            {
                isNewMessageReceived_ = false;
                isRunning_ = false;

                if (_thread != null && _thread.IsAlive)
                {
                    _thread.Abort();
                    _thread.Join();
                }

                if (serialPort_ != null && serialPort_.IsOpen)
                {
                    serialPort_.Close();
                    serialPort_.Dispose();
                }

                errMsg = "CLOSE";
                return true;
            }
            catch (System.IO.IOException ex)
            {
                errMsg = ex.Message;
                err++;
            }
            catch (System.Exception ex)
            {
                errMsg = ex.Message;
                err++;
            }
            return false;

        }

        public void InitMessage()
        {
            readCnt = 0;
            for (int i = 0; i < MESSAGE_SIZE; i++)
            {
                message_[i] = string.Empty;
            }
            isNewMessageReceived_ = false;
        }

        public void chkReadMessage()
        {
            if (isNewMessageReceived_)
            {
                isMessageProcessing = true;
                //Debug.LogWarning("OnDataRead Call");
                OnDataReceived(message_);
                InitMessage();
                isMessageProcessing = false;
            }
            isNewMessageReceived_ = false;
        }
        
        private void Read()
        {
            while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
            {
                if (isMessageProcessing) continue;
                try
                {
                    message_[readCnt] = serialPort_.ReadLine();
                    //Debug.LogWarning("[Read] " + serialPort_.PortName+" message_[0] " + (int)message_[0].ToString()[0] + " message_[1] " + (int)message_[0].ToString()[1]);
                    readCnt++;
                    isNewMessageReceived_ = true;
                }
                catch (System.Exception)
                {
                    //errMsg = "[Read]" + ex.Message;
                    //Debug.LogWarning("[SerialHandler][Read] ErrMessage " + ex.Message);
                }
                Thread.Sleep(1);
            }
        }

        public void Write(string message)
        {
            try
            {
                serialPort_.Write(message);
            }
            catch (System.Exception ex)
            {
                errMsg = "[Write]" +  ex.Message;
            }
        }

        public void ReConnect()
        {
            Close();
            Open();
        }

        public bool isOpen()
        {
            return serialPort_.IsOpen;
        }

    }

    public serial_unit GetPortListData(string _portName)
    {
        serial_unit _data = null;

        foreach (serial_unit _serial in PortList)
        {
            if(_serial.portName == _portName)
            {
                _data = _serial;
                break;
            }
        }
        return _data;
    }

    public string DebugList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("--- SERIAL HANDLER ---");
        sb.Append("\n");

        foreach (serial_unit _serial in PortList)
        {
            sb.Append("[");
            sb.Append(_serial.UserName);
            sb.Append("]");
            sb.Append(_serial.errMsg);
            sb.Append(" @Err:");
            sb.Append(_serial.err);
            sb.Append("\n");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 使用するシリアルポートを設定する
    /// <para>親子関係になっている前提</para>
    /// </summary>
    void SetUpSerialPort()
    {
        PortList = new List<serial_unit>();

        int no = 0;
        serial_unit _unit;
        //子供を探してリストを追加する
        foreach (Transform child in transform)
        {
            //アクティブな子供だけを追加する
            if (!child.gameObject.activeSelf) continue;

            SerialPortName _sp = child.GetComponent<SerialPortName>();
            if (_sp != null)
            {
                _unit = new serial_unit();
                _unit.portName_def = _sp.portName_def;
                _unit.UserName = _sp.UserName;
                _unit.portName = _sp.portName;
                _unit.baudRate = _sp.baudRate;
                PortList.Add(_unit);

                _sp.SerialListNo = no++;
            }
        }
    }

    void Awake()
    {
        SetUpSerialPort();

        if (PortList.Count <= 0) return;
        foreach (serial_unit _serial in PortList)
        {
            _serial.err = 0;
            _serial.InitMessage();
        }

    }

    void Update()
    {
        if (PortList.Count <= 0) return;
        foreach (serial_unit _serial in PortList)
        {
            _serial.chkReadMessage();
        }


        foreach(string port in SerialPort.GetPortNames())
        {
            Debug.LogWarning(port);
        }


    }

    void OnDestroy()
    {
        if (PortList.Count <= 0) return;
        foreach (serial_unit _serial in PortList)
        {
            _serial.Close();          
        }
    }
}
