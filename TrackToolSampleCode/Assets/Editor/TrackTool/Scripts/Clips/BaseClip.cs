using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Net.NetworkInformation;

namespace TrackTool
{
    public interface IBaseClip
    {
        void SetParentTimelineClip(TimelineClip _parentTimelineClip);
        TrackData.EventData GetEventData();
    }

    [Serializable]
    public class BaseClip<T> : PlayableAsset, ITimelineClipAsset, IBaseClip where T : PlayableBehaviour, IBaseBehaviour, new() 
    {
        public T template = new T();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<T>.Create(graph, template);

            return playable;
        }

        #region Override Interface
        public void SetParentTimelineClip(TimelineClip _parentTimelineClip)
        {
            template.SetParentTimelineClip(_parentTimelineClip);
        }

        public TrackData.EventData GetEventData()
        {
            return template.GetEventData();
        }
        #endregion
    }
}


