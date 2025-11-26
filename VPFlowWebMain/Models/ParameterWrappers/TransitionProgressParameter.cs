using ScriptPortal.Vegas;
using System;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Lib;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for Transition Progress envelopes (FadeIn/FadeOut).
    /// These envelopes have a bug where the last point has a maximum of ~6 frames
    /// regardless of actual fade length, requiring special timecode scaling.
    /// </summary>
    internal class TransitionProgressParameter : IAnimatableParameter
    {
        private readonly Envelope _envelope;
        private readonly Timecode _fadeLength;
        private readonly bool _isFadeIn;

        /// <summary>
        /// The last envelope point's time (used for scaling calculations)
        /// </summary>
        public long? LastPointNanos
        {
            get
            {
                var points = _envelope?.Points;
                if (points == null || points.Count == 0)
                    return null;
                return points[points.Count - 1].X.Nanos;
            }
        }

        public TransitionProgressParameter(Envelope envelope, Timecode fadeLength, bool isFadeIn)
        {
            _envelope = envelope;
            _fadeLength = fadeLength;
            _isFadeIn = isFadeIn;
        }

        /// <summary>
        /// Creates a TransitionProgressParameter from a VideoEvent's FadeIn transition
        /// </summary>
        public static TransitionProgressParameter FromFadeIn(VideoEvent videoEvent)
        {
            if (videoEvent?.FadeIn?.Transition == null)
                return null;

            var envelope = GetTransitionProgressEnvelope(videoEvent.FadeIn);
            if (envelope == null)
                return null;

            return new TransitionProgressParameter(envelope, videoEvent.FadeIn.Length, isFadeIn: true);
        }

        /// <summary>
        /// Creates a TransitionProgressParameter from a VideoEvent's FadeOut transition
        /// </summary>
        public static TransitionProgressParameter FromFadeOut(VideoEvent videoEvent)
        {
            if (videoEvent?.FadeOut?.Transition == null)
                return null;

            var envelope = GetTransitionProgressEnvelope(videoEvent.FadeOut);
            if (envelope == null)
                return null;

            return new TransitionProgressParameter(envelope, videoEvent.FadeOut.Length, isFadeIn: false);
        }

        /// <summary>
        /// Gets the TransitionProgress envelope from a Fade object.
        /// Note: Envelopes are on the Fade object, not the Effect/Transition.
        /// </summary>
        private static Envelope GetTransitionProgressEnvelope(Fade fade)
        {
            if (fade == null)
                return null;

            // Look for the transition progress envelope
            foreach (Envelope env in fade.Envelopes)
            {
                if (env.Type == EnvelopeType.TransitionProgress)
                    return env;
            }

            return null;
        }

        public string Name => _isFadeIn ? "FadeIn Transition Progress" : "FadeOut Transition Progress";
        public string Type => "TransitionProgress";

        public IEnumerable<object> GetKeyframes()
        {
            if (_envelope == null)
                return Enumerable.Empty<object>();

            return _envelope.Points.Cast<object>();
        }

        /// <summary>
        /// Gets the keyframe time, scaled from envelope space to real fade space
        /// </summary>
        public Timecode GetKeyframeTime(object keyframe)
        {
            var point = keyframe as EnvelopePoint;
            if (point == null)
                return Timecode.FromFrames(0);

            // Scale from envelope space (max ~6 frames) to real fade length
            var scaledNanos = KeyframeCalculations.ScaleCursorToFadeEnvelope(
                LastPointNanos,
                _fadeLength.Nanos,
                point.X.Nanos,
                scaleUp: true);

            return Timecode.FromNanos(scaledNanos);
        }

        /// <summary>
        /// Gets the raw (unscaled) envelope point time
        /// </summary>
        public Timecode GetRawKeyframeTime(object keyframe)
        {
            return ((EnvelopePoint)keyframe).X;
        }

        /// <summary>
        /// Scales a cursor position for envelope lookup.
        /// Converts from real fade time to envelope's internal time.
        /// </summary>
        public Timecode ScaleCursorForLookup(Timecode cursorPosition)
        {
            var scaledNanos = KeyframeCalculations.ScaleCursorToFadeEnvelope(
                LastPointNanos,
                _fadeLength.Nanos,
                cursorPosition.Nanos,
                scaleUp: false);

            return Timecode.FromNanos(scaledNanos);
        }

        public void AddKeyframe(Timecode time, object value)
        {
            if (_envelope == null)
                return;

            // Scale from real time to envelope time
            var scaledNanos = KeyframeCalculations.ScaleCursorToFadeEnvelope(
                LastPointNanos,
                _fadeLength.Nanos,
                time.Nanos,
                scaleUp: false);

            var y = Convert.ToDouble(value);
            var point = new EnvelopePoint(Timecode.FromNanos(scaledNanos), y);
            _envelope.Points.Add(point);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            var start = ((EnvelopePoint)startKf).Y;
            var end = ((EnvelopePoint)endKf).Y;
            return KeyframeCalculations.Lerp(start, end, t);
        }
    }
}
