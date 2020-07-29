using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Linq;

using TrackTool;
using System.CodeDom;
using UnityEditor;

public static class TimelineAssetControler
{
    public struct TimelineTrackInfo
    {
        public TimelineAsset TimelineAsset;
        public PlayableDirector PlayableDirector;
        public string ActorName;
        public bool IsPlayer;
        public string PrefabName;
        public TrackData.eAction ActionType;
        public Animator ActorAnimator;
        public TrackData TrackData;
    }

    private static readonly string m_ArtGroupName = "Art";
    private static readonly string m_GameDesignGroupName = "GameDesign";


    public static string GetAnimationName(TimelineAsset _playableAsset)
    {
        if (_playableAsset == null)
        {
            Debug.LogWarning("Null Reference timelineAsset Of GetAnimationNameOfTimeline");
            return string.Empty;
        }

        var findAnimationTrack = (from tracks in _playableAsset.GetOutputTracks()
                                  where (tracks is AnimationTrack) == true
                                  select tracks).FirstOrDefault();

        if (findAnimationTrack == null)
            return string.Empty;

        return findAnimationTrack.GetClips().FirstOrDefault()?.animationClip.name;
    }

    public static List<TrackData.EventData> GetEventDataList(TimelineAsset _timelineAsset)
    {
        if (_timelineAsset == null)
        {
            Debug.LogWarning("Null Reference timelineAsset Of GetTrackEventDataList");
            return null;
        }

        List<TrackData.EventData> loadEventData = new List<TrackData.EventData>();

        foreach (var trackAsset in _timelineAsset.GetOutputTracks())
        {
            if (trackAsset as BaseTrack)
            {
                foreach (var clip in trackAsset.GetClips())
                {
                    var addEventData = (clip.asset as IBaseClip).GetEventData();
                    addEventData.TrackTitle = (trackAsset as IBaseTrack).GetTrackTitle();

                    loadEventData.Add(addEventData);
                }
            }
        }

        return loadEventData;
    }

    public static void SetTimelineTrack(TimelineTrackInfo _trackInfo)
    {
        //if (CurrentActionType == TrackData.eAction.None)
        //    return;

        if (_trackInfo.TimelineAsset == null)
        {
            Debug.Log("Not Found playableAsset Of SetTimelineTrack");
            return;
        }

        if (_trackInfo.TimelineAsset == null)
            return;

        AnimationTrack animationTrack = null;
        TrackAsset artTrack = null;
        TrackAsset gameDesignTrack = null;
        foreach (var rootTrack in _trackInfo.TimelineAsset.GetRootTracks())
        {
            if(rootTrack.name.Contains("Markers"))
            {
                continue;
            }
            else if(rootTrack.name.Contains("Animation Track"))
            {
                animationTrack = rootTrack as AnimationTrack;
                continue;
            }
            else if (rootTrack.name.Contains(m_ArtGroupName))
            {
                artTrack = rootTrack;
            }
            else if(rootTrack.name.Contains(m_GameDesignGroupName))
            {
                gameDesignTrack = rootTrack;
            }
            else
            {
                _trackInfo.TimelineAsset.DeleteTrack(rootTrack);
            }
        }

        SetAnimationTrack(_trackInfo, animationTrack);
        SetEventTrack(_trackInfo, artTrack, gameDesignTrack);
    }

    private static void SetEventTrack(TimelineTrackInfo _trackInfo, TrackAsset _artTrackGroup, TrackAsset _gameDesignTrackGroup)
    {
        if (_artTrackGroup == null)
        {
            _artTrackGroup = _trackInfo.TimelineAsset.CreateTrack<GroupTrack>(m_ArtGroupName);
        }
        else
        {
            foreach (var childTracks in _artTrackGroup.GetChildTracks())
            {
                _trackInfo.TimelineAsset.DeleteTrack(childTracks);
            }
        }

        if (_gameDesignTrackGroup == null)
        {
            _gameDesignTrackGroup = _trackInfo.TimelineAsset.CreateTrack<GroupTrack>(m_GameDesignGroupName);
        }
        else
        {
            foreach (var childTracks in _gameDesignTrackGroup.GetChildTracks())
            {
                _trackInfo.TimelineAsset.DeleteTrack(childTracks);
            }
        }

        if (_trackInfo.TrackData.EventDataArray == null ||
            _trackInfo.TrackData.EventDataArray.Length <= 0)
            return;
       
        foreach(var eventData in _trackInfo.TrackData.EventDataArray)
        {
            IBaseTrack createTrack = null;
            if(eventData.EventGroupName.Contains(TrackData.eEventGroupType.Art.ToString()))
            {
                if (_artTrackGroup == null)
                    continue;

                createTrack = _trackInfo.TimelineAsset.CreateTrack<ArtTrack>(_artTrackGroup, string.Empty) as IBaseTrack;
            }
            else
            {
                if (_gameDesignTrackGroup == null)
                    continue;

                createTrack = _trackInfo.TimelineAsset.CreateTrack<GameDesignTrack>(_gameDesignTrackGroup, string.Empty) as IBaseTrack;
            }

            createTrack.CreateClip(eventData);
            createTrack.SetTrackTitle(eventData.TrackTitle);
        }
    }

    private static void SetAnimationTrack(TimelineTrackInfo _trackInfo, AnimationTrack _animationTrack)
    {
        if (_trackInfo.TimelineAsset == null)
            return;

        if (_animationTrack == null)
        {
            _animationTrack = _trackInfo.TimelineAsset.CreateTrack<AnimationTrack>();
        }

        _trackInfo.PlayableDirector.SetGenericBinding(_animationTrack, _trackInfo.ActorAnimator);

        TimelineClip animationClip = _animationTrack.GetClips().FirstOrDefault(); 
        if(animationClip == null)
        {
            animationClip = _animationTrack.CreateDefaultClip();
        }

        animationClip.displayName = _trackInfo.ActionType.ToString();

        if (animationClip.asset is AnimationPlayableAsset)
        {
            var animationPlayableAsset = animationClip.asset as AnimationPlayableAsset;
            var animationClipPath = string.Format("Animators/{0}/{1}/{2}", _trackInfo.IsPlayer ? "Player" : "Monster", _trackInfo.PrefabName, _trackInfo.ActionType.ToString().ToLower());
            animationPlayableAsset.clip = Resources.Load<AnimationClip>(animationClipPath);
        }
    }
}
