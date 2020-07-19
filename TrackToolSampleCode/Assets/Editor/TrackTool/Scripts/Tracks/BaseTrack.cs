using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System;

using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.PlayerLoop;

namespace TrackTool
{
    [HideInMenu]
    public abstract class BaseTrack : TrackAsset
    {
        /// <summary>
        /// 클립 생성시 한번만 호출됨
        /// </summary>
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);

            if (clip.duration != 1)
            {
                clip.duration = 1;
            }
        }

        /// <summary>
        /// 왜 크립 하나 움직일때마다 모든 트랙의 클립에서 CreatePlayable 호출 되는지 모르겠다...
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            UpdateTrackInfo(clip);

            return base.CreatePlayable(graph, gameObject, clip);
        }

        private void UpdateTrackInfo(TimelineClip _clip)
        {
            this.name = _clip.displayName;

            var baseClip = (IBaseClip)_clip.asset;
            if (baseClip != null)
            {
                baseClip.SetParentTimelineClip(_clip);
            }
        }
    }
}
