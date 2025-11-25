using ScriptPortal.Vegas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Lib;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Enumeration for the type of track motion parameter
    /// </summary>
    public enum TrackMotionType
    {
        Motion,
        Shadow,
        Glow
    }

    /// <summary>
    /// Wrapper for Parent Track motion parameters.
    /// Parent track properties require IsCompositingParent to be true
    /// and need UndoBlock even for read access.
    /// </summary>
    internal class ParentTrackMotionParameter : IAnimatableParameter
    {
        private readonly VideoTrack _track;
        private readonly TrackMotionType _motionType;

        public ParentTrackMotionParameter(VideoTrack track, TrackMotionType motionType)
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

        public string Name => $"Parent Track {_motionType}";
        public string Type => $"ParentTrack{_motionType}";

        public IEnumerable<object> GetKeyframes()
        {
            if (_track?.ParentTrackMotion == null)
                return Enumerable.Empty<object>();

            IList keyframes = _motionType switch
            {
                TrackMotionType.Motion => _track.ParentTrackMotion.MotionKeyframes,
                TrackMotionType.Shadow => _track.ParentTrackMotion.ShadowKeyframes,
                TrackMotionType.Glow => _track.ParentTrackMotion.GlowKeyframes,
                _ => null
            };

            return keyframes?.Cast<object>() ?? Enumerable.Empty<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            // TrackMotionKeyframe has Position property
            var positionProp = keyframe.GetType().GetProperty("Position");
            return (Timecode)positionProp?.GetValue(keyframe);
        }

        public void AddKeyframe(Timecode time, object value)
        {
            if (_track?.ParentTrackMotion == null)
                return;

            // Get the appropriate keyframe collection
            IList keyframes = _motionType switch
            {
                TrackMotionType.Motion => _track.ParentTrackMotion.MotionKeyframes,
                TrackMotionType.Shadow => _track.ParentTrackMotion.ShadowKeyframes,
                TrackMotionType.Glow => _track.ParentTrackMotion.GlowKeyframes,
                _ => null
            };

            if (keyframes == null)
                return;

            // Create appropriate keyframe type using reflection
            var kfType = keyframes.GetType().GetGenericArguments()[0];
            var constructor = kfType.GetConstructor(new[] { typeof(Timecode) });
            if (constructor == null)
                return;

            var kf = constructor.Invoke(new object[] { time });

            // Copy properties from value if provided
            if (value != null)
            {
                CopyKeyframeProperties(value, kf);
            }

            keyframes.Add(kf);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            return _motionType switch
            {
                TrackMotionType.Motion => InterpolateMotionKeyframe(startKf, endKf, t),
                TrackMotionType.Shadow => InterpolateShadowKeyframe(startKf, endKf, t),
                TrackMotionType.Glow => InterpolateGlowKeyframe(startKf, endKf, t),
                _ => null
            };
        }

        /// <summary>
        /// Interpolates TrackMotionKeyframe properties
        /// </summary>
        private object InterpolateMotionKeyframe(object startKf, object endKf, double t)
        {
            var start = (TrackMotionKeyframe)startKf;
            var end = (TrackMotionKeyframe)endKf;

            // Create a dictionary of interpolated values
            return new TrackMotionValues
            {
                PositionX = KeyframeCalculations.Lerp(start.PositionX, end.PositionX, t),
                PositionY = KeyframeCalculations.Lerp(start.PositionY, end.PositionY, t),
                Width = KeyframeCalculations.Lerp(start.Width, end.Width, t),
                Height = KeyframeCalculations.Lerp(start.Height, end.Height, t),
                RotationX = KeyframeCalculations.Lerp(start.RotationX, end.RotationX, t),
                RotationY = KeyframeCalculations.Lerp(start.RotationY, end.RotationY, t),
                RotationZ = KeyframeCalculations.Lerp(start.RotationZ, end.RotationZ, t),
                OrientationX = KeyframeCalculations.Lerp(start.OrientationX, end.OrientationX, t),
                OrientationY = KeyframeCalculations.Lerp(start.OrientationY, end.OrientationY, t),
                OrientationZ = KeyframeCalculations.Lerp(start.OrientationZ, end.OrientationZ, t),
                RotationOffsetX = KeyframeCalculations.Lerp(start.RotationOffsetX, end.RotationOffsetX, t),
                RotationOffsetY = KeyframeCalculations.Lerp(start.RotationOffsetY, end.RotationOffsetY, t),
                RotationOffsetZ = KeyframeCalculations.Lerp(start.RotationOffsetZ, end.RotationOffsetZ, t),
                ScaleX = KeyframeCalculations.Lerp(start.ScaleX, end.ScaleX, t),
                ScaleY = KeyframeCalculations.Lerp(start.ScaleY, end.ScaleY, t),
                PositionZ = KeyframeCalculations.Lerp(start.PositionZ, end.PositionZ, t)
            };
        }

        /// <summary>
        /// Interpolates TrackShadowKeyframe properties
        /// </summary>
        private object InterpolateShadowKeyframe(object startKf, object endKf, double t)
        {
            var start = (TrackShadowKeyframe)startKf;
            var end = (TrackShadowKeyframe)endKf;

            return new TrackShadowGlowValues
            {
                OffsetX = KeyframeCalculations.Lerp(start.OffsetX, end.OffsetX, t),
                OffsetY = KeyframeCalculations.Lerp(start.OffsetY, end.OffsetY, t),
                Blur = KeyframeCalculations.Lerp(start.Blur, end.Blur, t),
                Intensity = KeyframeCalculations.Lerp(start.Intensity, end.Intensity, t),
                // Color interpolation
                ColorR = KeyframeCalculations.Lerp(start.Color.R / 255.0, end.Color.R / 255.0, t),
                ColorG = KeyframeCalculations.Lerp(start.Color.G / 255.0, end.Color.G / 255.0, t),
                ColorB = KeyframeCalculations.Lerp(start.Color.B / 255.0, end.Color.B / 255.0, t),
                ColorA = KeyframeCalculations.Lerp(start.Color.A / 255.0, end.Color.A / 255.0, t)
            };
        }

        /// <summary>
        /// Interpolates TrackGlowKeyframe properties
        /// </summary>
        private object InterpolateGlowKeyframe(object startKf, object endKf, double t)
        {
            var start = (TrackGlowKeyframe)startKf;
            var end = (TrackGlowKeyframe)endKf;

            return new TrackShadowGlowValues
            {
                OffsetX = KeyframeCalculations.Lerp(start.OffsetX, end.OffsetX, t),
                OffsetY = KeyframeCalculations.Lerp(start.OffsetY, end.OffsetY, t),
                Blur = KeyframeCalculations.Lerp(start.Blur, end.Blur, t),
                Intensity = KeyframeCalculations.Lerp(start.Intensity, end.Intensity, t),
                ColorR = KeyframeCalculations.Lerp(start.Color.R / 255.0, end.Color.R / 255.0, t),
                ColorG = KeyframeCalculations.Lerp(start.Color.G / 255.0, end.Color.G / 255.0, t),
                ColorB = KeyframeCalculations.Lerp(start.Color.B / 255.0, end.Color.B / 255.0, t),
                ColorA = KeyframeCalculations.Lerp(start.Color.A / 255.0, end.Color.A / 255.0, t)
            };
        }

        private void CopyKeyframeProperties(object source, object target)
        {
            if (source is TrackMotionValues motion && target is TrackMotionKeyframe kf)
            {
                kf.PositionX = motion.PositionX;
                kf.PositionY = motion.PositionY;
                kf.Width = motion.Width;
                kf.Height = motion.Height;
                kf.RotationX = motion.RotationX;
                kf.RotationY = motion.RotationY;
                kf.RotationZ = motion.RotationZ;
                kf.OrientationX = motion.OrientationX;
                kf.OrientationY = motion.OrientationY;
                kf.OrientationZ = motion.OrientationZ;
                kf.RotationOffsetX = motion.RotationOffsetX;
                kf.RotationOffsetY = motion.RotationOffsetY;
                kf.RotationOffsetZ = motion.RotationOffsetZ;
                kf.ScaleX = motion.ScaleX;
                kf.ScaleY = motion.ScaleY;
                kf.PositionZ = motion.PositionZ;
            }
            else if (source is TrackShadowGlowValues shadowGlow)
            {
                if (target is TrackShadowKeyframe shadow)
                {
                    shadow.OffsetX = shadowGlow.OffsetX;
                    shadow.OffsetY = shadowGlow.OffsetY;
                    shadow.Blur = shadowGlow.Blur;
                    shadow.Intensity = shadowGlow.Intensity;
                    shadow.Color = System.Drawing.Color.FromArgb(
                        (int)(shadowGlow.ColorA * 255),
                        (int)(shadowGlow.ColorR * 255),
                        (int)(shadowGlow.ColorG * 255),
                        (int)(shadowGlow.ColorB * 255)
                    );
                }
                else if (target is TrackGlowKeyframe glow)
                {
                    glow.OffsetX = shadowGlow.OffsetX;
                    glow.OffsetY = shadowGlow.OffsetY;
                    glow.Blur = shadowGlow.Blur;
                    glow.Intensity = shadowGlow.Intensity;
                    glow.Color = System.Drawing.Color.FromArgb(
                        (int)(shadowGlow.ColorA * 255),
                        (int)(shadowGlow.ColorR * 255),
                        (int)(shadowGlow.ColorG * 255),
                        (int)(shadowGlow.ColorB * 255)
                    );
                }
            }
        }
    }

    /// <summary>
    /// Helper class to hold interpolated TrackMotionKeyframe values
    /// </summary>
    public class TrackMotionValues
    {
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public double OrientationX { get; set; }
        public double OrientationY { get; set; }
        public double OrientationZ { get; set; }
        public double RotationOffsetX { get; set; }
        public double RotationOffsetY { get; set; }
        public double RotationOffsetZ { get; set; }
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
    }

    /// <summary>
    /// Helper class to hold interpolated Shadow/Glow keyframe values
    /// </summary>
    public class TrackShadowGlowValues
    {
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double Blur { get; set; }
        public double Intensity { get; set; }
        public double ColorR { get; set; }
        public double ColorG { get; set; }
        public double ColorB { get; set; }
        public double ColorA { get; set; }
    }
}
