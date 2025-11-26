using ScriptPortal.Vegas;
using System.Collections.Generic;
using System.Linq;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for standard Effect keyframes (DXT style effects).
    /// Effect keyframes have a Preset property but no direct value interpolation.
    /// </summary>
    internal class EffectKeyframeParameter : IAnimatableParameter
    {
        private readonly Effect _effect;

        public EffectKeyframeParameter(Effect effect)
        {
            _effect = effect;
        }

        public string Name => _effect?.Description ?? "Effect";
        public string Type => "Effect";

        public IEnumerable<object> GetKeyframes()
        {
            if (_effect?.Keyframes == null)
                return Enumerable.Empty<object>();

            return _effect.Keyframes.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            var kf = keyframe as Keyframe;
            return kf?.Position ?? Timecode.FromFrames(0);
        }

        public void AddKeyframe(Timecode time, object value)
        {
            if (_effect?.Keyframes == null)
                return;

            // Create a new keyframe at the specified time
            var kf = new Keyframe(time);
            _effect.Keyframes.Add(kf);

            // Set to linear interpolation
            kf.Type = VideoKeyframeType.Linear;
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            // Effect keyframes use presets, not direct value interpolation
            // The interpolation happens internally based on keyframe positions
            // Return null as we only need to create keyframes at positions
            return null;
        }
    }
}
