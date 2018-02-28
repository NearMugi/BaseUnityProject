using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class PlayableBehaviour_ActiveObject : PlayableBehaviour
{
    public GameObject activeObj_1;
    public GameObject activeObj_2;
    public GameObject activeObj_3;
    public GameObject activeObj_4;
    public GameObject activeObj_5;

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable) {
		
	}

	// Called when the owning graph stops playing
	public override void OnGraphStop(Playable playable) {
		
	}

	// Called when the state of the playable is set to Play
	public override void OnBehaviourPlay(Playable playable, FrameData info) {
        if (activeObj_1 != null) activeObj_1.SetActive(true);
        if (activeObj_2 != null) activeObj_2.SetActive(true);
        if (activeObj_3 != null) activeObj_3.SetActive(true);
        if (activeObj_4 != null) activeObj_4.SetActive(true);
        if (activeObj_5 != null) activeObj_5.SetActive(true);
    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info) {
		
	}

	// Called each frame while the state is set to Play
	public override void PrepareFrame(Playable playable, FrameData info) {
		
	}
}
