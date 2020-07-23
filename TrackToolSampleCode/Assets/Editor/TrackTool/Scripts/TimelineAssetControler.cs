using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Linq;

using TrackTool;

public static class TimelineAssetControler
{
    private static readonly string m_ArtGroupName = "Art";
    private static readonly string m_GameDesignGroupName = "GameDesign";


    public static string GetAnimationName(PlayableAsset _playableAsset)
    {
        if (_playableAsset == null)
        {
            Debug.LogWarning("Null Reference _playableAsset Of GetAnimationNameOfTimeline");
            return string.Empty;
        }

        var timelineAsset = _playableAsset as TimelineAsset;
        var findAnimationTrack = (from tracks in timelineAsset.GetOutputTracks()
                                  where (tracks is AnimationTrack) == true
                                  select tracks).FirstOrDefault();

        if (findAnimationTrack == null)
            return string.Empty;

        return findAnimationTrack.GetClips().FirstOrDefault()?.animationClip.name;
    }

    public static List<TrackData.EventData> GetEventDataList(PlayableAsset _playableAsset)
    {
        if (_playableAsset == null)
        {
            Debug.LogWarning("Null Reference _playableAsset Of GetTrackEventDataList");
            return null;
        }

        List<TrackData.EventData> loadEventData = new List<TrackData.EventData>();

        var timelineAsset = _playableAsset as TimelineAsset;
        foreach (var trackAsset in timelineAsset.GetOutputTracks())
        {
            if (trackAsset as BaseTrack)
            {
                foreach (var clip in trackAsset.GetClips())
                {
                    loadEventData.Add((clip.asset as IBaseClip).GetEventData());
                }
            }
        }

        return loadEventData;
    }

    public static void SetTimelineTrack(PlayableAsset _playableAsset, TrackData _trackData)
    {
        //if (CurrentActionType == TrackData.eAction.None)
        //    return;

        if (_playableAsset == null)
            return;

        var timelineAsset = _playableAsset as TimelineAsset;
        if (timelineAsset == null)
            return;

        foreach (var rootTrack in timelineAsset.GetRootTracks())
        {
            if (rootTrack.name.Contains("Art") ||
                rootTrack.name.Contains("GameDesign"))
            {

            }
        }
    }
}
