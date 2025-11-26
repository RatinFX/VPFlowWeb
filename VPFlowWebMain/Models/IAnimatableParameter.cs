using ScriptPortal.Vegas;
using System.Collections.Generic;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Interface for all animatable parameters to provide unified access
    /// </summary>
    public interface IAnimatableParameter
    {
        string Name { get; }
        string Type { get; }
        IEnumerable<object> GetKeyframes();
        Timecode GetKeyframeTime(object keyframe);
        void AddKeyframe(Timecode time, object value);
        object InterpolateValue(object startKf, object endKf, double t);
    }
}
