using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class blePeripheral_android : MonoBehaviour
{
    #region Singleton

    private static blePeripheral_android instance;

    public static blePeripheral_android Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (blePeripheral_android)FindObjectOfType(typeof(blePeripheral_android));

                if (instance == null)
                {
                    Debug.LogError(typeof(blePeripheral_android) + "is nothing");
                }
            }

            return instance;
        }
    }

    #endregion Singleton


    [SerializeField]
    string DiviceName = "AndroidBLE";
    [SerializeField]
    string ServiceUUID = "da61480e-4ad8-11e9-8646-d663bd873d93";
    //android -> ESP32
    [SerializeField]
    string ReadCharacteristicUUID = "7b15e552-4ad9-11e9-8646-d663bd873d93";
    //ESP32 -> android
    [SerializeField]
    string WriteCharacteristicUUID = "e1d67b1c-4ae3-11e9-8646-d663bd873d93";

    public enum STATUS
    {
        disConnect,
        connect,
    }
    public STATUS status { private set; get; }
    public string status_t { private set; get; }
    public string err { private set; get; }
    public string readData { private set; get; }
    public string writeData { private set; get; }


    public void Init()
    {
        BluetoothLEHardwareInterface.Initialize(false, true, () =>
        {

        }, (error) =>
        {
            err = error;
            BluetoothLEHardwareInterface.Log(err);
        });
    }

    public void StartAdvertising()
    {
        BluetoothLEHardwareInterface.PeripheralName(DiviceName);

        BluetoothLEHardwareInterface.RemoveServices();
        BluetoothLEHardwareInterface.RemoveCharacteristics();

        BluetoothLEHardwareInterface.CBAttributePermissions permissions =
            BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsReadable |
            BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsWriteable;

        //Setting Characteristic(Android -> ESP32)
        BluetoothLEHardwareInterface.CBCharacteristicProperties properties =
            BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyWrite;
        BluetoothLEHardwareInterface.CreateCharacteristic(ReadCharacteristicUUID, properties, permissions, null, 0, null);

        //Setting Characteristic(ESP32 -> Android)
        properties = BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyRead;
        BluetoothLEHardwareInterface.CreateCharacteristic(WriteCharacteristicUUID, properties, permissions, null, 5,
            (characteristic, bytes) =>
            {
                readData = Encoding.UTF8.GetString(bytes);
            });


        //Create Service
        BluetoothLEHardwareInterface.CreateService(ServiceUUID, true, (characteristic) =>
        {
            //なぜかここが呼ばれない。
        });

        BluetoothLEHardwareInterface.StartAdvertising(() =>
        {
            status = STATUS.connect;
            status_t = "Start Advertising";
        });
    }

    public void StopAdvertising()
    {
        BluetoothLEHardwareInterface.StopAdvertising(null);
    }

    public void Send(string t)
    {
        writeData = t;
        BluetoothLEHardwareInterface.UpdateCharacteristicValue(
            ReadCharacteristicUUID,
            Encoding.UTF8.GetBytes(t),
            t.Length
            );
    }

    public void Reset()
    {
        status = STATUS.disConnect;
        status_t = "Stop Advertising";
        err = "";
        readData = "";
        writeData = "";
    }
}
