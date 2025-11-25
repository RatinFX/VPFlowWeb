using ScriptPortal.Vegas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Generic wrapper for OFX parameters
    /// </summary>
    internal class OFXParameterWrapper<TValue, TKeyframe> : IAnimatableParameter
        where TKeyframe : OFXKeyframe<TValue>
    {
        private readonly OFXParameter<TValue, TKeyframe> _parameter;

        public OFXParameterWrapper(OFXParameter<TValue, TKeyframe> parameter)
        {
            _parameter = parameter;
        }

        public string Name => _parameter.Name;
        public string Type => $"OFX_{typeof(TValue).Name}";

        public IEnumerable<object> GetKeyframes()
        {
            return _parameter.Keyframes.Cast<object>();
        }

        public Timecode GetKeyframeTime(object keyframe)
        {
            return ((TKeyframe)keyframe).Time;
        }

        public void AddKeyframe(Timecode time, object value)
        {
            var kf = (TKeyframe)Activator.CreateInstance(typeof(TKeyframe));
            // Use reflection to set Time and Value properties
            kf.GetType().GetProperty("Time")?.SetValue(kf, time);
            kf.GetType().GetProperty("Value")?.SetValue(kf, (TValue)value);
            _parameter.Keyframes.Add(kf);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            var start = ((TKeyframe)startKf).Value;
            var end = ((TKeyframe)endKf).Value;

            // Type-specific interpolation
            if (typeof(TValue) == typeof(double))
            {
                var s = Convert.ToDouble(start);
                var e = Convert.ToDouble(end);
                return s + (e - s) * t;
            }
            else if (typeof(TValue) == typeof(OFXDouble2D))
            {
                var s = (OFXDouble2D)(object)start;
                var e = (OFXDouble2D)(object)end;
                return new OFXDouble2D
                {
                    X = s.X + (e.X - s.X) * t,
                    Y = s.Y + (e.Y - s.Y) * t
                };
            }
            else if (typeof(TValue) == typeof(OFXDouble3D))
            {
                var s = (OFXDouble3D)(object)start;
                var e = (OFXDouble3D)(object)end;
                return new OFXDouble3D
                {
                    X = s.X + (e.X - s.X) * t,
                    Y = s.Y + (e.Y - s.Y) * t,
                    Z = s.Z + (e.Z - s.Z) * t
                };
            }
            else if (typeof(TValue) == typeof(int))
            {
                var s = Convert.ToInt32(start);
                var e = Convert.ToInt32(end);
                return (int)Math.Round(s + (e - s) * t);
            }
            else if (typeof(TValue) == typeof(OFXInteger2D))
            {
                var s = (OFXInteger2D)(object)start;
                var e = (OFXInteger2D)(object)end;
                return new OFXInteger2D
                {
                    X = (int)Math.Round(s.X + (e.X - s.X) * t),
                    Y = (int)Math.Round(s.Y + (e.Y - s.Y) * t)
                };
            }
            else if (typeof(TValue) == typeof(OFXInteger3D))
            {
                var s = (OFXInteger3D)(object)start;
                var e = (OFXInteger3D)(object)end;
                return new OFXInteger3D
                {
                    X = (int)Math.Round(s.X + (e.X - s.X) * t),
                    Y = (int)Math.Round(s.Y + (e.Y - s.Y) * t),
                    Z = (int)Math.Round(s.Z + (e.Z - s.Z) * t)
                };
            }
            else if (typeof(TValue) == typeof(OFXColor))
            {
                var s = (OFXColor)(object)start;
                var e = (OFXColor)(object)end;
                return new OFXColor
                {
                    R = s.R + (e.R - s.R) * t,
                    G = s.G + (e.G - s.G) * t,
                    B = s.B + (e.B - s.B) * t,
                    A = s.A + (e.A - s.A) * t
                };
            }
            // Add more type-specific interpolations as needed

            return start; // Fallback
        }
    }
}
