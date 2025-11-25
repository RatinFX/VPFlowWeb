using ScriptPortal.Vegas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for Envelope points
    /// </summary>
    internal class EnvelopeParameter : IAnimatableParameter
    {
        private readonly Envelope _envelope;

        public EnvelopeParameter(Envelope envelope)
        {
            _envelope = envelope;
        }

        public string Name => _envelope.Type.ToString();
        public string Type => "Envelope";

        public IEnumerable<object> GetKeyframes()
        {
            return _envelope.Points.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            return ((EnvelopePoint)keyframe).X;
        }

        public void AddKeyframe(Timecode time, object value)
        {
            var y = Convert.ToDouble(value);
            var point = new EnvelopePoint(time, y);
            _envelope.Points.Add(point);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            var start = ((EnvelopePoint)startKf).Y;
            var end = ((EnvelopePoint)endKf).Y;
            return start + (end - start) * t;
        }
    }
}
