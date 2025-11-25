using ScriptPortal.Vegas;
using System.Collections.Generic;
using System.Linq;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for VideoMotion keyframes
    /// </summary>
    internal class VideoMotionParameter : IAnimatableParameter
    {
        private readonly VideoMotion _videoMotion;

        public VideoMotionParameter(VideoMotion videoMotion)
        {
            _videoMotion = videoMotion;
        }

        public string Name => "Video Motion";
        public string Type => "VideoMotion";

        public IEnumerable<object> GetKeyframes()
        {
            return _videoMotion.Keyframes.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            return ((VideoMotionKeyframe)keyframe).Position;
        }

        public void AddKeyframe(Timecode time, object value)
        {
            var kf = new VideoMotionKeyframe(time);
            // Copy bounds and other properties from interpolated value
            if (value is VideoMotionBounds bounds)
            {
                kf.Bounds = bounds;
            }
            _videoMotion.Keyframes.Add(kf);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            var start = ((VideoMotionKeyframe)startKf).Bounds;
            var end = ((VideoMotionKeyframe)endKf).Bounds;

            // Interpolate VideoMotionBounds
            return new VideoMotionBounds
            {
                Left = start.Left + (end.Left - start.Left) * t,
                Top = start.Top + (end.Top - start.Top) * t,
                Width = start.Width + (end.Width - start.Width) * t,
                Height = start.Height + (end.Height - start.Height) * t
            };
        }
    }
}
