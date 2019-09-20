using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class PlayableBehaviour_StartMovie : PlayableBehaviour
{
    public GameObject movieObj_1;
    public GameObject movieObj_2;
    public GameObject movieObj_3;
    public GameObject movieObj_4;
    public GameObject movieObj_5;

    // Called when the owning graph starts playing
    public override void OnGraphStart(Playable playable) {
    }

	// Called when the owning graph stops playing
	public override void OnGraphStop(Playable playable) {
		
	}

	// Called when the state of the playable is set to Play
	public override void OnBehaviourPlay(Playable playable, FrameData info) {
        if (movieObj_1 != null) movieObj_1.GetComponent<MovieDecodeAndPlay>().StartMovie();
        if (movieObj_2 != null) movieObj_2.GetComponent<MovieDecodeAndPlay>().StartMovie();
        if (movieObj_3 != null) movieObj_3.GetComponent<MovieDecodeAndPlay>().StartMovie();
        if (movieObj_4 != null) movieObj_4.GetComponent<MovieDecodeAndPlay>().StartMovie();
        if (movieObj_5 != null) movieObj_5.GetComponent<MovieDecodeAndPlay>().StartMovie();

    }

    // Called when the state of the playable is set to Paused
    public override void OnBehaviourPause(Playable playable, FrameData info) {
    }

    // Called each frame while the state is set to Play
    public override void PrepareFrame(Playable playable, FrameData info) {
    }
}
