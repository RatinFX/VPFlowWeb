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
    /// TrackMotionKeyframe has 15+ animatable properties.
    /// </summary>
    internal class TrackMotionKeyframeParameter : IAnimatableParameter
    {
        private readonly IList _keyframes;
        private readonly TrackMotionType _motionType;

        public TrackMotionKeyframeParameter(IList keyframes, TrackMotionType motionType)
        {
            _keyframes = keyframes;
            _motionType = motionType;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility
        /// </summary>
        public TrackMotionKeyframeParameter(IList keyframes, string name)
            : this(keyframes, ParseMotionType(name))
        {
        }

        private static TrackMotionType ParseMotionType(string name)
        {
            return name?.ToLower() switch
            {
                "shadow" => TrackMotionType.Shadow,
                "glow" => TrackMotionType.Glow,
                _ => TrackMotionType.Motion
            };
        }

        public string Name => $"Track {_motionType}";
        public string Type => $"Track{_motionType}Keyframe";

        public IEnumerable<object> GetKeyframes()
        {
            return _keyframes.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            // All track keyframe types have Position property
            return keyframe switch
            {
                TrackMotionKeyframe motion => motion.Position,
                TrackShadowKeyframe shadow => shadow.Position,
                TrackGlowKeyframe glow => glow.Position,
                _ => (Timecode)keyframe.GetType().GetProperty("Position")?.GetValue(keyframe)
            };
        }

        public void AddKeyframe(Timecode time, object value)
        {
            if (_keyframes == null)
                return;

            switch (_motionType)
            {
                case TrackMotionType.Motion:
                    AddMotionKeyframe(time, value as TrackMotionValues);
                    break;
                case TrackMotionType.Shadow:
                    AddShadowKeyframe(time, value as TrackShadowGlowValues);
                    break;
                case TrackMotionType.Glow:
                    AddGlowKeyframe(time, value as TrackShadowGlowValues);
                    break;
            }
        }

        private void AddMotionKeyframe(Timecode time, TrackMotionValues values)
        {
            if (!(_keyframes is TrackMotionKeyframes motionKeyframes))
                return;

            var kf = new TrackMotionKeyframe(time);

            if (values != null)
            {
                kf.PositionX = values.PositionX;
                kf.PositionY = values.PositionY;
                kf.PositionZ = values.PositionZ;
                kf.Width = values.Width;
                kf.Height = values.Height;
                kf.RotationX = values.RotationX;
                kf.RotationY = values.RotationY;
                kf.RotationZ = values.RotationZ;
                kf.OrientationX = values.OrientationX;
                kf.OrientationY = values.OrientationY;
                kf.OrientationZ = values.OrientationZ;
                kf.RotationOffsetX = values.RotationOffsetX;
                kf.RotationOffsetY = values.RotationOffsetY;
                kf.RotationOffsetZ = values.RotationOffsetZ;
                kf.ScaleX = values.ScaleX;
                kf.ScaleY = values.ScaleY;
            }

            motionKeyframes.Add(kf);
        }

        private void AddShadowKeyframe(Timecode time, TrackShadowGlowValues values)
        {
            if (!(_keyframes is TrackShadowKeyframes shadowKeyframes))
                return;

            var kf = new TrackShadowKeyframe(time);

            if (values != null)
            {
                kf.OffsetX = values.OffsetX;
                kf.OffsetY = values.OffsetY;
                kf.Blur = values.Blur;
                kf.Intensity = values.Intensity;
                kf.Color = System.Drawing.Color.FromArgb(
                    (int)(values.ColorA * 255),
                    (int)(values.ColorR * 255),
                    (int)(values.ColorG * 255),
                    (int)(values.ColorB * 255)
                );
            }

            shadowKeyframes.Add(kf);
        }

        private void AddGlowKeyframe(Timecode time, TrackShadowGlowValues values)
        {
            if (!(_keyframes is TrackGlowKeyframes glowKeyframes))
                return;

            var kf = new TrackGlowKeyframe(time);

            if (values != null)
            {
                kf.OffsetX = values.OffsetX;
                kf.OffsetY = values.OffsetY;
                kf.Blur = values.Blur;
                kf.Intensity = values.Intensity;
                kf.Color = System.Drawing.Color.FromArgb(
                    (int)(values.ColorA * 255),
                    (int)(values.ColorR * 255),
                    (int)(values.ColorG * 255),
                    (int)(values.ColorB * 255)
                );
            }

            glowKeyframes.Add(kf);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            return _motionType switch
            {
                TrackMotionType.Motion => InterpolateMotion(startKf, endKf, t),
                TrackMotionType.Shadow => InterpolateShadow(startKf, endKf, t),
                TrackMotionType.Glow => InterpolateGlow(startKf, endKf, t),
                _ => null
            };
        }

        private TrackMotionValues InterpolateMotion(object startKf, object endKf, double t)
        {
            var start = (TrackMotionKeyframe)startKf;
            var end = (TrackMotionKeyframe)endKf;

            return new TrackMotionValues
            {
                PositionX = KeyframeCalculations.Lerp(start.PositionX, end.PositionX, t),
                PositionY = KeyframeCalculations.Lerp(start.PositionY, end.PositionY, t),
                PositionZ = KeyframeCalculations.Lerp(start.PositionZ, end.PositionZ, t),
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
                ScaleY = KeyframeCalculations.Lerp(start.ScaleY, end.ScaleY, t)
            };
        }

        private TrackShadowGlowValues InterpolateShadow(object startKf, object endKf, double t)
        {
            var start = (TrackShadowKeyframe)startKf;
            var end = (TrackShadowKeyframe)endKf;

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

        private TrackShadowGlowValues InterpolateGlow(object startKf, object endKf, double t)
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
    }
}
