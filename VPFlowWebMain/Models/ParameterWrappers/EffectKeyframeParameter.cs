using ScriptPortal.Vegas;
using System.Collections.Generic;
using System.Linq;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for standard Effect keyframes
    /// </summary>
    internal class EffectKeyframeParameter : IAnimatableParameter
    {
        private readonly Effect _effect;

        public EffectKeyframeParameter(Effect effect)
        {
            _effect = effect;
        }

        public string Name => _effect.Description;
        public string Type => "Effect";

        public IEnumerable<object> GetKeyframes()
        {
            return _effect.Keyframes.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            return ((Keyframe)keyframe).Position;
        }

        public void AddKeyframe(Timecode time, object value)
        {
            var kf = new Keyframe(_effect, time);
            // Copy parameter values - this is effect-specific
            // Would need more sophisticated value handling
            _effect.Keyframes.Add(kf);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            // Interpolation logic for effect parameters
            // This is complex as effects have multiple parameters
            return null; // Placeholder
        }
    }
}
