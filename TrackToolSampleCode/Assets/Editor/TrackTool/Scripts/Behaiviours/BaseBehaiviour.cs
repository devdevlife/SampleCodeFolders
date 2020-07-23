using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using UnityEditor.Compilation;
using ReflectionHelper;

namespace TrackTool
{
    /// <summary>
    /// 이벤트 데이터 가져오기 내보내기 기능 추후 추가.
    /// </summary>
    public interface IBaseBehaviour
    {
        void UpdateEventInfo();

        void SetParentTimelineClip(TimelineClip _parentTimelineClip);
        void SetBehaiviourInfo(float start, float end);
        void SetEventData<E>(E eventData) where E : TrackData.EventData;

        TrackData.EventData GetEventData();
    }

    public abstract class BaseBehaiviour<T> : PlayableBehaviour, IBaseBehaviour where T : TrackData.EventData
    {
        [SerializeField]
        public T EventData;

        [HideInInspector]
        protected TimelineClip parentTimelineClip;

        /// <summary>
        /// 신규 생성 및 클립수정될때마다 호출됨.
        /// </summary>
        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
            UpdateEventInfo();
        }

        #region // Override Interface //
        public void SetParentTimelineClip(TimelineClip _parentTimelineClip)
        {
            parentTimelineClip = _parentTimelineClip;
        }

        public virtual void UpdateEventInfo()
        {
            if (parentTimelineClip == null ||
                EventData == null)
                return;

            EventData.StartTime = (float)parentTimelineClip.start;
            EventData.EndTime = (float)parentTimelineClip.end;
        }

        public void SetBehaiviourInfo(float start, float end)
        {

        }

        public void SetEventData<E>(E _eventData) where E : TrackData.EventData
        {
            //EventData = _eventData;
        }

        public TrackData.EventData GetEventData()
        {
            return this.EventData;
        }
        #endregion
    }

   

    /// <summary>
    /// NotKeyable Attribute 애니메이션 커브를 사용하면 안됨.. float 필드 자료형 인식 못함.
    /// </summary>
    [Serializable, NotKeyable]
    public class SoundEventBehaiviour : BaseBehaiviour<TrackData.SoundEventData> 
    {
        private AudioSource audioSource = new AudioSource();

        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);

            if (EventData == null)
                return;

            if (string.IsNullOrEmpty(EventData.SoundName) == false)
            {
                var audioClip = DrawerHelper.LoadResourceObject<AudioClip>(DrawerHelper.ResourceType.Sound_Effect, EventData.SoundName);
                if (audioClip != null)
                {
                    PlayClip(audioClip);
                }
            }
        }

        public static void PlayClip(AudioClip clip)
        {
            //System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            //Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            ////System.Reflection.MethodInfo method = audioUtilClass.GetMethod("PlayClip", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new System.Type[] { typeof(AudioClip) }, null);
            //System.Reflection.MethodInfo method = audioUtilClass.GetMethod("PlayOneShot", new System.Type[] { typeof(AudioClip) });

            //method.Invoke(null, new object[] { clip });
        }
    }

    [Serializable, NotKeyable]
    public class EffectEventBehaiviour : BaseBehaiviour<TrackData.EffectEventData> { }

    [Serializable, NotKeyable]
    public class ScreenEffectEventBehaiviour : BaseBehaiviour<TrackData.ScreenEventData> { }
}


