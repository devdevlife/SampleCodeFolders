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

    [Serializable]
    public abstract class EventData
    {
        public virtual eEvent Type { get; }

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

        public override eEvent Type { get { return eEvent.Sound; } }

        [ShowInspector(ShowFieldType.Audio)]
        public string SoundName;

        [Range(0, 100)]
        public int Volume;
    }

    [Serializable]
    public class EffectEventData : EventData
    {
        public override eEvent Type { get { return eEvent.Effect; } }

        [ShowInspector(ShowFieldType.GameObject)]
        public string EffectName;

        [SerializeField]
        public Vector3 Offset;
    }

    [Serializable]
    public class ScreenEventData : EventData
    {
        public override eEvent Type { get { return eEvent.ScreenEffect; } }
    }

    [Serializable]
    public class SkillEventData : EventData
    {
        public override eEvent Type { get { return eEvent.Skill; } }
    }

    public string ID;
    public string AnimationName;
    public eAction ActionType;
    public EventData[] EventDataArray;
}
