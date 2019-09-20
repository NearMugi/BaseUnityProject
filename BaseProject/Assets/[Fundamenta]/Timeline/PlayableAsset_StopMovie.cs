using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class PlayableAsset_StopMovie : PlayableAsset
{
    // シーン上のオブジェクトはExposedReference<T>を使用する
    public ExposedReference<GameObject> movieObj_1;
    public ExposedReference<GameObject> movieObj_2;
    public ExposedReference<GameObject> movieObj_3;
    public ExposedReference<GameObject> movieObj_4;
    public ExposedReference<GameObject> movieObj_5;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var behaviour = new PlayableBehaviour_StopMovie();
        behaviour.movieObj_1 = movieObj_1.Resolve(graph.GetResolver());
        behaviour.movieObj_2 = movieObj_2.Resolve(graph.GetResolver());
        behaviour.movieObj_3 = movieObj_3.Resolve(graph.GetResolver());
        behaviour.movieObj_4 = movieObj_4.Resolve(graph.GetResolver());
        behaviour.movieObj_5 = movieObj_5.Resolve(graph.GetResolver());
        return ScriptPlayable<PlayableBehaviour_StopMovie>.Create(graph, behaviour);
    }
}
