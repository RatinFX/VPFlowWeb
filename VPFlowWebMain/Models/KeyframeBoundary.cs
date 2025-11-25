using ScriptPortal.Vegas;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Represents a keyframe boundary (start/end) for animation
    /// </summary>
    public class KeyframeBoundary
    {
        public Timecode StartTime { get; set; }
        public Timecode EndTime { get; set; }
        public object StartKeyframe { get; set; }
        public object EndKeyframe { get; set; }
        public IAnimatableParameter Parameter { get; set; }
    }
}
