using ScriptPortal.Vegas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for TrackMotion keyframe lists (Motion/Shadow/Glow)
    /// </summary>
    internal class TrackMotionKeyframeParameter : IAnimatableParameter
    {
        private readonly IList _keyframes;
        private readonly string _name;

        public TrackMotionKeyframeParameter(IList keyframes, string name)
        {
            _keyframes = keyframes;
            _name = name;
        }

        public string Name => $"Track {_name}";
        public string Type => $"Track{_name}Keyframe";

        public IEnumerable<object> GetKeyframes()
        {
            return _keyframes.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            var positionProp = keyframe.GetType().GetProperty("Position");
            return (Timecode)positionProp?.GetValue(keyframe);
        }

        public void AddKeyframe(Timecode time, object value)
        {
            // Use reflection to create appropriate keyframe type
            var kfType = _keyframes.GetType().GetGenericArguments()[0];
            var kf = Activator.CreateInstance(kfType, time);
            
            // Copy properties from the interpolated value if provided
            if (value != null && value.GetType() == kfType)
            {
                // Copy relevant properties from value to kf
                // This needs to be done property by property for each keyframe type
            }
            
            _keyframes.Add(kf);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            // Complex interpolation for track motion parameters
            // This needs specific implementation for each type:
            // - TrackMotionKeyframe: Position, Scale, Rotation, etc.
            // - TrackShadowKeyframe: Shadow properties
            // - TrackGlowKeyframe: Glow properties
            
            // For now, return null as placeholder
            // Would need property-by-property interpolation using reflection
            return null;
        }
    }
}
