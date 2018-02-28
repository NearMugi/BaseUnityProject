using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class PlayableAsset_ActiveObject : PlayableAsset
{
    // シーン上のオブジェクトはExposedReference<T>を使用する
    public ExposedReference<GameObject> activeObj_1;
    public ExposedReference<GameObject> activeObj_2;
    public ExposedReference<GameObject> activeObj_3;
    public ExposedReference<GameObject> activeObj_4;
    public ExposedReference<GameObject> activeObj_5;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var behaviour = new PlayableBehaviour_ActiveObject();
        behaviour.activeObj_1 = activeObj_1.Resolve(graph.GetResolver());
        behaviour.activeObj_2 = activeObj_2.Resolve(graph.GetResolver());
        behaviour.activeObj_3 = activeObj_3.Resolve(graph.GetResolver());
        behaviour.activeObj_4 = activeObj_4.Resolve(graph.GetResolver());
        behaviour.activeObj_5 = activeObj_5.Resolve(graph.GetResolver());
        return ScriptPlayable<PlayableBehaviour_ActiveObject>.Create(graph, behaviour);
    }
}
