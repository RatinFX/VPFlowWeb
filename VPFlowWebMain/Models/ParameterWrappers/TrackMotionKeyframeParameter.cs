using ScriptPortal.Vegas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Lib;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for TrackMotion keyframe lists (Motion/Shadow/Glow).
    /// NOTE: This wrapper is for READ operations only (finding keyframe boundaries).
    /// Adding keyframes requires the parent TrackMotion object and its Insert*Keyframe methods.
    /// </summary>
    internal class TrackMotionKeyframeParameter : IAnimatableParameter
    {
        private readonly IList _keyframes;
        private readonly MotionKeyframeType _motionType;
        private readonly TrackMotion _trackMotion;

        public TrackMotionKeyframeParameter(IList keyframes, MotionKeyframeType motionType, TrackMotion trackMotion = null)
        {
            _keyframes = keyframes;
            _motionType = motionType;
            _trackMotion = trackMotion;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility
        /// </summary>
        public TrackMotionKeyframeParameter(IList keyframes, string name)
            : this(keyframes, ParseMotionType(name), null)
        {
        }

        private static MotionKeyframeType ParseMotionType(string name)
        {
            if (string.IsNullOrEmpty(name))
                return MotionKeyframeType.Motion;

            var lower = name.ToLower();
            if (lower == "shadow")
                return MotionKeyframeType.Shadow;
            if (lower == "glow")
                return MotionKeyframeType.Glow;

            return MotionKeyframeType.Motion;
        }

        public string Name
        {
            get { return "Track " + _motionType.ToString(); }
        }

        public string Type
        {
            get { return "Track" + _motionType.ToString() + "Keyframe"; }
        }

        public IEnumerable<object> GetKeyframes()
        {
            if (_keyframes == null)
                return Enumerable.Empty<object>();

            return _keyframes.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            // All track keyframe types inherit from BaseTrackMotionKeyframe which has Position
            if (keyframe is TrackMotionKeyframe motion)
                return motion.Position;
            if (keyframe is TrackShadowKeyframe shadow)
                return shadow.Position;
            if (keyframe is TrackGlowKeyframe glow)
                return glow.Position;

            // Fallback using reflection
            var prop = keyframe?.GetType().GetProperty("Position");
            return prop?.GetValue(keyframe) as Timecode ?? Timecode.FromFrames(0);
        }

        public void AddKeyframe(Timecode time, object value)
        {
            // Adding track motion keyframes requires the TrackMotion object
            // and its Insert*Keyframe methods. This is handled externally.
            if (_trackMotion == null)
                return;

            if (_motionType == MotionKeyframeType.Motion)
            {
                var kf = _trackMotion.InsertMotionKeyframe(time);
                kf.Type = VideoKeyframeType.Linear;
            }
            else if (_motionType == MotionKeyframeType.Shadow)
            {
                var kf = _trackMotion.InsertShadowKeyframe(time);
                kf.Type = VideoKeyframeType.Linear;
            }
            else if (_motionType == MotionKeyframeType.Glow)
            {
                var kf = _trackMotion.InsertGlowKeyframe(time);
                kf.Type = VideoKeyframeType.Linear;
            }
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            // Track motion keyframes have many properties that are set after insertion
            // We return null here; the actual value interpolation happens when 
            // applying the curve (using KeyframeCalculations.Lerp on individual properties)
            return null;
        }
    }
}
