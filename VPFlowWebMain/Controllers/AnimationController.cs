using ScriptPortal.Vegas;
using System;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Models;

namespace VPFlowWebMain.Controllers
{
    /// <summary>
    /// Handles detection and manipulation of animatable parameters in Vegas
    /// </summary>
    public class AnimationController
    {
        private readonly Vegas _vegas;

        public AnimationController(Vegas vegas)
        {
            _vegas = vegas;
        }

        /// <summary>
        /// Finds the first effect with keyframes surrounding the cursor position
        /// </summary>
        public (Effect effect, KeyframeBoundary boundary) FindActiveEffect(
            TrackEvent selectedEvent,
            Timecode adjustedCursorPosition)
        {
            if (!(selectedEvent is VideoEvent videoEvent))
            {
                return (null, null);
            }

            foreach (Effect effect in videoEvent.Effects)
            {
                // Check standard Effect keyframes
                var boundary = FindKeyframeBoundary(
                    new EffectKeyframeParameter(effect),
                    adjustedCursorPosition);

                if (boundary != null)
                    return (effect, boundary);

                // Check OFX parameters if available
                if (effect == null || !effect.IsOFX || effect.OFXEffect == null)
                    continue;

                foreach (OFXParameter param in effect.OFXEffect.Parameters)
                {
                    var ofxParam = WrapOFXParameter(param);
                    if (ofxParam == null)
                        continue;

                    boundary = FindKeyframeBoundary(ofxParam, adjustedCursorPosition);
                    if (boundary != null)
                        return (effect, boundary);
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Finds keyframe boundaries for VideoMotion (Pan/Crop)
        /// </summary>
        public KeyframeBoundary FindVideoMotionBoundary(
            VideoEvent videoEvent,
            Timecode adjustedCursorPosition)
        {
            if (videoEvent?.VideoMotion == null)
                return null;

            var param = new VideoMotionParameter(videoEvent.VideoMotion);
            return FindKeyframeBoundary(param, adjustedCursorPosition);
        }

        /// <summary>
        /// Finds keyframe boundaries for TrackMotion
        /// </summary>
        public KeyframeBoundary FindTrackMotionBoundary(
            VideoTrack track,
            Timecode adjustedCursorPosition)
        {
            if (track?.TrackMotion == null) return null;

            // Check motion keyframes
            var motionParam = new TrackMotionKeyframeParameter(
                track.TrackMotion.MotionKeyframes, "Motion");
            var boundary = FindKeyframeBoundary(motionParam, adjustedCursorPosition);
            if (boundary != null)
                return boundary;

            // Check shadow keyframes
            var shadowParam = new TrackMotionKeyframeParameter(
                track.TrackMotion.ShadowKeyframes, "Shadow");
            boundary = FindKeyframeBoundary(shadowParam, adjustedCursorPosition);
            if (boundary != null)
                return boundary;

            // Check glow keyframes
            var glowParam = new TrackMotionKeyframeParameter(
                track.TrackMotion.GlowKeyframes, "Glow");
            return FindKeyframeBoundary(glowParam, adjustedCursorPosition);
        }

        /// <summary>
        /// Finds keyframe boundaries for Envelopes (Fade, etc.)
        /// </summary>
        public KeyframeBoundary FindEnvelopeBoundary(
            Envelope envelope,
            Timecode adjustedCursorPosition)
        {
            if (envelope == null)
                return null;

            var param = new EnvelopeParameter(envelope);
            return FindKeyframeBoundary(param, adjustedCursorPosition);
        }

        /// <summary>
        /// Generic method to find start/end keyframe boundary
        /// </summary>
        private KeyframeBoundary FindKeyframeBoundary(
            IAnimatableParameter parameter,
            Timecode cursorPosition)
        {
            var keyframes = parameter.GetKeyframes().ToList();
            if (keyframes.Count < 2)
                return null;

            object startKeyframe = null;
            object endKeyframe = null;

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                var currentTime = parameter.GetKeyframeTime(keyframes[i]);
                var nextTime = parameter.GetKeyframeTime(keyframes[i + 1]);

                // Check if cursor is between these keyframes
                if (currentTime <= cursorPosition && cursorPosition <= nextTime)
                {
                    startKeyframe = keyframes[i];
                    endKeyframe = keyframes[i + 1];
                    break;
                }
            }

            if (startKeyframe == null || endKeyframe == null)
                return null;

            return new KeyframeBoundary
            {
                StartTime = parameter.GetKeyframeTime(startKeyframe),
                EndTime = parameter.GetKeyframeTime(endKeyframe),
                StartKeyframe = startKeyframe,
                EndKeyframe = endKeyframe,
                Parameter = parameter
            };
        }

        /// <summary>
        /// Creates keyframes along a bezier curve between start and end
        /// </summary>
        public void ApplyBezierCurve(
            KeyframeBoundary boundary,
            List<Point> curvePoints)
        {
            if (boundary == null || curvePoints == null || curvePoints.Count < 2)
                return;

            var duration = boundary.EndTime - boundary.StartTime;

            // Remove any existing keyframes between start and end (except the boundaries)
            // This is parameter-specific and needs to be handled carefully

            // Create new keyframes based on curve points
            foreach (var point in curvePoints)
            {
                // Skip start and end points (they already exist)
                if (point.x <= 0 || point.x >= 1)
                    continue;

                // Calculate time position
                var t = point.x;
                var time = boundary.StartTime + Timecode.FromNanos(
                    (long)(duration.Nanos * t));

                // Interpolate value based on curve's Y value
                var interpolatedValue = boundary.Parameter.InterpolateValue(
                    boundary.StartKeyframe,
                    boundary.EndKeyframe,
                    point.y);

                // Add the keyframe
                boundary.Parameter.AddKeyframe(time, interpolatedValue);
            }
        }

        /// <summary>
        /// Wraps an OFXParameter in the appropriate interface implementation
        /// </summary>
        private IAnimatableParameter WrapOFXParameter(OFXParameter param)
        {
            // Use reflection to determine the specific OFX type
            var type = param.GetType();

            if (type.IsGenericType)
            {
                var genericDef = type.GetGenericTypeDefinition();

                // Check if it's an OFXParameter<TValue, TKeyframe>
                if (genericDef.Name.StartsWith("OFXParameter"))
                {
                    var args = type.GetGenericArguments();
                    if (args.Length == 2)
                    {
                        var wrapperType = typeof(OFXParameterWrapper<,>)
                            .MakeGenericType(args[0], args[1]);
                        return (IAnimatableParameter)Activator.CreateInstance(
                            wrapperType, param);
                    }
                }
            }

            return null;
        }
    }
}
