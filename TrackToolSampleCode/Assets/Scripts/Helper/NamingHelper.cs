using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NamingHelper
{
    public static string GetTrackDataID(string _actorName, TrackData.eAction _actionType)
    {
        return string.Format("{0}_{1}", _actorName, _actionType.ToString());
    }
}
