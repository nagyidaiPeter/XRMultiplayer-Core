﻿using XRMultiplayer.Models;
using UnityEngine;

namespace XRMultiplayer
{
    public class ObjectData
    {
        public ObjectTransform objectTransform;

        public Vector3 LastSentPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public GameObject gameObject;

        public bool IsPositionChanged()
        {
            return LastSentPos != objectTransform.Pos;
        }

        public ObjectData()
        {

        }
    }
}