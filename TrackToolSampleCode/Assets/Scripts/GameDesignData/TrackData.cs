using Boo.Lang;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using UnityEngine.Timeline;
using System.Runtime.Serialization;

[Serializable]
public partial class TrackData
{
    /// <summary>
    /// Clip Event Type
    /// </summary>
    public enum eEvent
    {
        Sound,
        Effect,
        ScreenEffect,
        Skill,
        CameraShake,
        SuperArmor,
    }

    public enum eEventGroupType
    {
        Art,
        GameDesing,
    }


    [Serializable]
    public abstract class EventData
    {
        public abstract string Type { get; }
        public abstract string EventGroupName { get; }

        [ShowInspector(ShowFieldType.Hide)]
        public string TrackTitle;

        [ShowInspector(ShowFieldType.HideAllSwitch)]
        public bool Disable;

        [ShowInspector(ShowFieldType.ReadOnly)]
        public float StartTime;

        [ShowInspector(ShowFieldType.ReadOnly)]
        public float EndTime;
    }

    [Serializable]
    public class SoundEventData : EventData
    {
        // showIf example
        //[ShowInspector(ShowFieldType.ShowIf, ConditionOperator.InverseAnd, new string[] { "Disable", "DisableTest" })]
        //public int TestInverseAnd;

        public override string Type { get { return eEvent.Sound.ToString(); } }
        public override string EventGroupName { get { return eEventGroupType.Art.ToString(); } }

        [ShowInspector(ShowFieldType.Audio)]
        public string SoundName;

        [Range(0, 100)]
        public int Volume;
    }

    [Serializable]
    public class EffectEventData : EventData
    {
        public override string Type { get { return eEvent.Effect.ToString(); } }
        public override string EventGroupName { get { return eEventGroupType.Art.ToString(); } }

        [ShowInspector(ShowFieldType.GameObject)]
        public string EffectName;

        [SerializeField]
        public Vector3 Offset;
    }

    [Serializable]
    public class ScreenEventData : EventData
    {
        public override string Type { get { return eEvent.ScreenEffect.ToString(); } }
        public override string EventGroupName { get { return eEventGroupType.GameDesing.ToString(); } }
    }

    [Serializable]
    public class SkillEventData : EventData
    {
        public override string Type { get { return eEvent.Skill.ToString(); } }
        public override string EventGroupName { get { return eEventGroupType.GameDesing.ToString(); } }
    }

    public string ID;
    public string AnimationName;
    public string ActionType;
    public EventData[] EventDataArray;
}
