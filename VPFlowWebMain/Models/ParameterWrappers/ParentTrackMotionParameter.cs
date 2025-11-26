using ScriptPortal.Vegas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Lib;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Enumeration for the type of track motion parameter.
    /// Named differently from ScriptPortal.Vegas.TrackMotionType to avoid conflicts.
    /// </summary>
    public enum MotionKeyframeType
    {
        Motion,
        Shadow,
        Glow
    }

    /// <summary>
    /// Wrapper for Parent Track motion parameters.
    /// Parent track properties require IsCompositingParent to be true
    /// and need UndoBlock even for read access.
    /// NOTE: This is for READ operations only. Adding keyframes requires
    /// the TrackMotion object and its Insert*Keyframe methods.
    /// </summary>
    internal class ParentTrackMotionParameter : IAnimatableParameter
    {
        private readonly VideoTrack _track;
        private readonly MotionKeyframeType _motionType;

        public ParentTrackMotionParameter(VideoTrack track, MotionKeyframeType motionType)
        {
            _track = track;
            _motionType = motionType;
        }

        /// <summary>
        /// Checks if the track has parent track features available
        /// </summary>
        public static bool IsCompositingParent(VideoTrack track)
        {
            return track?.IsCompositingParent ?? false;
        }

        /// <summary>
        /// Checks if parent track motion is available and animated
        /// </summary>
        public static bool HasAnimatedParentMotion(VideoTrack track)
        {
            if (!IsCompositingParent(track))
                return false;

            return track.ParentTrackMotion?.MotionKeyframes?.Count > 1;
        }

        /// <summary>
        /// Checks if parent track shadow is available, enabled, and animated
        /// </summary>
        public static bool HasAnimatedParentShadow(VideoTrack track)
        {
            if (!IsCompositingParent(track))
                return false;

            var motion = track.ParentTrackMotion;
            return motion != null &&
                   motion.ShadowEnabled &&
                   motion.ShadowKeyframes?.Count > 1;
        }

        /// <summary>
        /// Checks if parent track glow is available, enabled, and animated
        /// </summary>
        public static bool HasAnimatedParentGlow(VideoTrack track)
        {
            if (!IsCompositingParent(track))
                return false;

            var motion = track.ParentTrackMotion;
            return motion != null &&
                   motion.GlowEnabled &&
                   motion.GlowKeyframes?.Count > 1;
        }

        /// <summary>
        /// Gets the parent composite mode effect if available and animated
        /// </summary>
        public static Effect GetParentCompositeModeEffect(VideoTrack track)
        {
            if (!IsCompositingParent(track))
                return null;

            var effect = track.ParentCompositeModeEffect;
            if (effect == null || effect.Keyframes.Count <= 1)
                return null;

            return effect;
        }

        public string Name
        {
            get { return "Parent Track " + _motionType.ToString(); }
        }

        public string Type
        {
            get { return "ParentTrack" + _motionType.ToString(); }
        }

        public IEnumerable<object> GetKeyframes()
        {
            if (_track?.ParentTrackMotion == null)
                return Enumerable.Empty<object>();

            IList keyframes = null;
            if (_motionType == MotionKeyframeType.Motion)
                keyframes = _track.ParentTrackMotion.MotionKeyframes;
            else if (_motionType == MotionKeyframeType.Shadow)
                keyframes = _track.ParentTrackMotion.ShadowKeyframes;
            else if (_motionType == MotionKeyframeType.Glow)
                keyframes = _track.ParentTrackMotion.GlowKeyframes;

            if (keyframes == null)
                return Enumerable.Empty<object>();

            return keyframes.Cast<object>();
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
            var positionProp = keyframe?.GetType().GetProperty("Position");
            return positionProp?.GetValue(keyframe) as Timecode ?? Timecode.FromFrames(0);
        }

        public void AddKeyframe(Timecode time, object value)
        {
            // Adding parent track motion keyframes requires the TrackMotion object
            // and its Insert*Keyframe methods. This is handled externally.
            if (_track?.ParentTrackMotion == null)
                return;

            var trackMotion = _track.ParentTrackMotion;

            if (_motionType == MotionKeyframeType.Motion)
            {
                var kf = trackMotion.InsertMotionKeyframe(time);
                kf.Type = VideoKeyframeType.Linear;
            }
            else if (_motionType == MotionKeyframeType.Shadow)
            {
                var kf = trackMotion.InsertShadowKeyframe(time);
                kf.Type = VideoKeyframeType.Linear;
            }
            else if (_motionType == MotionKeyframeType.Glow)
            {
                var kf = trackMotion.InsertGlowKeyframe(time);
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

    /// <summary>
    /// Helper class to hold interpolated TrackMotionKeyframe values.
    /// Based on BaseTrackMotionKeyframe properties from Vegas API.
    /// </summary>
    public class TrackMotionValues
    {
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public double OrientationX { get; set; }
        public double OrientationY { get; set; }
        public double OrientationZ { get; set; }
        public double RotationOffsetX { get; set; }
        public double RotationOffsetY { get; set; }
        public double RotationOffsetZ { get; set; }
    }

    /// <summary>
    /// Helper class to hold interpolated Shadow/Glow keyframe values.
    /// Shadow and Glow keyframes have: Blur, Intensity, Color (VideoColor).
    /// Position comes from BaseTrackMotionKeyframe (PositionX, PositionY).
    /// </summary>
    public class TrackShadowGlowValues
    {
        public double Blur { get; set; }
        public double Intensity { get; set; }
        public double ColorR { get; set; }
        public double ColorG { get; set; }
        public double ColorB { get; set; }
        public double ColorA { get; set; }
    }
}
