using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System;
using System.Reflection;

using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.PlayerLoop;
using System.Linq;
using System.CodeDom;

namespace TrackTool
{
    public interface IBaseTrack
    {
        void SetTrackTitle(string _trackTitle);
        string GetTrackTitle();
        void CreateClip(TrackData.EventData _eventData);
    }

    [HideInMenu]
    public abstract class BaseTrack : TrackAsset, IBaseTrack
    {
        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);

            if (clip.duration != 1)
            {
                clip.duration = 1;
            }
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return base.CreateTrackMixer(graph, go, inputCount);
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

        IEnumerator SetTitle(string _title)
        {
            yield return new WaitForEndOfFrame();

            this.name = _title;
        }

        #region override interface

        public void SetTrackTitle(string _trackTitle)
        {
            if (string.IsNullOrEmpty(_trackTitle))
                return;

            TMPro.EditorUtilities.TMP_EditorCoroutine.StartCoroutine(SetTitle(_trackTitle));
        }

        public string GetTrackTitle()
        {
            return this.name;
        }

        public void CreateClip(TrackData.EventData _eventData)
        {
            var startTime = Double.Parse(_eventData.StartTime.ToString());
            var endTime = Double.Parse(_eventData.EndTime.ToString());
            var durationTime = endTime - startTime;

            var genericTypeName = string.Format("TrackTool.{0}EventClip", _eventData.Type);
            var genericType = Type.GetType(genericTypeName);
            if (genericType != null)
            {
                MethodInfo method = typeof(TrackAsset).GetMethod("CreateClip");
                MethodInfo generic = method.MakeGenericMethod(genericType);
                generic.Invoke(this, null);

                var createClip = GetClips().FirstOrDefault();
                if (createClip != null)
                {
                    createClip.start = startTime;
                    createClip.duration = durationTime;

                    if (createClip.asset is IBaseClip)
                    {
                        (createClip.asset as IBaseClip).SetEventData(_eventData);
                    }
                }
            }
        }

        #endregion
    }
}
