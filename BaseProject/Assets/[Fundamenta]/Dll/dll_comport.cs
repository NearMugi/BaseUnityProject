// 参考URL [UnityでC++を使う方法]
// https://qiita.com/8128/items/7cd0bf0b3f5bad60f709
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public class port : IEquatable<port>
{
    public string portName { get; set; }
    public string portInfo { get; set; }

    public int id { get; set; }

    public override string ToString()
    {
        return "ID:" + id + " Name:" + portName + "," + portInfo;
    }
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        port objAsport = obj as port;
        if (objAsport == null) return false;
        else return Equals(objAsport);
    }
    public override int GetHashCode()
    {
        return id;
    }
    public bool Equals(port other)
    {
        if (other == null) return false;
        return (this.id.Equals(other.id));
    }
}

public class dll_comport : MonoBehaviour
{
    List<port> portList = new List<port>();
    Coroutine NowCoroutine;
    bool isGet;

    public String[] getComport()
    {
        const int MAX_PORT = 5;
        String[] ret = new String[MAX_PORT];
        if (!isGet) return null;
        if (portList.Count <= 0) return null;

        int idx = 0;
        foreach (port p in portList)
        {
            ret[idx++] = p.ToString();
            if (idx >= MAX_PORT) break;
        }
        return ret;
    }

    public String[] getArduinoPort()
    {
        const int MAX_PORT = 5;
        String[] ret = new String[MAX_PORT];
        if (!isGet) return null;

        List<port> tmp = portList.FindAll(x => x.portInfo.Contains("Arduino"));
        if (tmp.Count <= 0) return null;

        int idx = 0;
        foreach (port p in tmp)
        {
            ret[idx++] = p.portName;
            if (idx >= MAX_PORT) break;
        }
        return ret;
    }
    private IEnumerator getComportListCoroutine()
    {
        var wait = new WaitForSeconds(0.1f);
        portList = new List<port>();
        isGet = false;
        while (!isGet)
        {
            StringBuilder str = new StringBuilder(256);
            int l = Lib.getComportList(str, str.Capacity);
            yield return wait;

            Debug.Log(str.ToString());
            if (str.Length > 0)
            {
                Debug.Log(l);
                isGet = true;
                String[] tmpPort = str.ToString().Split(',');
                int idx = 0;
                foreach (string p in tmpPort)
                {
                    if (p.Length <= 0) continue;
                    String[] tmp = p.Split(':');
                    if (tmp.Length != 2) continue;
                    portList.Add(new port()
                    {
                        id = idx++,
                        portName = tmp[0],
                        portInfo = tmp[1],
                    });
                }
            }
            yield return null;
        }
        yield break;
    }

    public void getComportList()
    {
        if (NowCoroutine != null) StopCoroutine(NowCoroutine);
        NowCoroutine = StartCoroutine(getComportListCoroutine());
    }
}