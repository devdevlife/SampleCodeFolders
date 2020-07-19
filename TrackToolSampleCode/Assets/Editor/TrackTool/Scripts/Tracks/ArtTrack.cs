using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TrackTool
{
    [TrackColor(0.8f, 0.8f, 0.8f)]
    [TrackClipType(typeof(SoundEventClip))]
    [TrackClipType(typeof(EffectEventClip))]
    [TrackClipType(typeof(ScreenEffectEventClip))]
    public class ArtTrack : BaseTrack
    {

    }
}
