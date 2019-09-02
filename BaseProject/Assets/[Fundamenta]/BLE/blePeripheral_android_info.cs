using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class blePeripheral_android_info : MonoBehaviour
{
    [SerializeField]
    InputField ValueInputField;
    [SerializeField]
    InputField ValueOutputField;
    [SerializeField]
    Text logField;

    blePeripheral_android ble;

    // Use this for initialization
    private void Start()
    {
        ble = new blePeripheral_android();
        ble.Reset();

        ble.Init();
        ble.StartAdvertising();
    }

    // Update is called once per frame
    private void Update()
    {
        ValueInputField.text = ble.readData;
        ValueOutputField.text = ble.writeData;
        logField.text = ble.status_t;

        ble.Send("100");
    }

}