using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sponge_PlotMeshUnit : MonoBehaviour {
    SerialConnect_Sponge _serial;
    int id;

    public void Init(SerialConnect_Sponge _s, int i)
    {
        _serial = _s;
        id = i;

        float ofs_y = 0.7f - 0.7f * id;

        Transform t = GetComponent<Transform>();
        t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y + ofs_y);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        GetComponent<MeshFilter>().sharedMesh = _serial.MeshPlot[id];

    }
}
