using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class PlayableAsset_WaitTrigger : PlayableAsset
{
    // シーン上のオブジェクトはExposedReference<T>を使用する
    public ExposedReference<PlayableDirector> _timeline;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var behaviour = new PlayableBehaviour_WaitTrigger();
        behaviour._timeline = _timeline.Resolve(graph.GetResolver());
        return ScriptPlayable<PlayableBehaviour_WaitTrigger>.Create(graph, behaviour);
    }
}
