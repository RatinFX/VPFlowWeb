using ScriptPortal.Vegas;
using System;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Lib;
using VPFlowWebMain.Models;

namespace VPFlowWebMain.Controllers
{
    /// <summary>
    /// Handles detection and manipulation of animatable parameters in Vegas.
    /// 
    /// Vegas API has many inconsistencies across different parameter types:
    /// - Different timecode properties (.Position vs .Time vs .X)
    /// - Different timecode scales (Media Generator, Transition Progress)
    /// - Rotation bug in VideoMotion bounds
    /// - UndoBlock required for some read operations
    /// </summary>
    public class AnimationController
    {
        private readonly Vegas _vegas;

        public AnimationController(Vegas vegas)
        {
            _vegas = vegas;
        }

        /// <summary>
        /// Gets the project frame rate for timecode calculations
        /// </summary>
        public double FrameRate => _vegas?.Project?.Video?.FrameRate ?? 30.0;

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

            // Check Media Generator first (has special timecode handling)
            if (MediaGeneratorParameter.IsMediaGenerator(videoEvent))
            {
                var mediaGenParam = new MediaGeneratorParameter(videoEvent, FrameRate);
                var scaledCursor = mediaGenParam.ScaleCursorForLookup(adjustedCursorPosition);
                var boundary = FindKeyframeBoundaryWithScaling(mediaGenParam, scaledCursor);
                if (boundary != null)
                {
                    var generator = videoEvent.ActiveTake?.Media?.Generator;
                    return (generator, boundary);
                }
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
        /// Finds keyframe boundaries for TrackMotion (motion/shadow/glow)
        /// </summary>
        public KeyframeBoundary FindTrackMotionBoundary(
            VideoTrack track,
            Timecode adjustedCursorPosition)
        {
            if (track?.TrackMotion == null) return null;

            // Check motion keyframes
            var motionParam = new TrackMotionKeyframeParameter(
                track.TrackMotion.MotionKeyframes, TrackMotionType.Motion);
            var boundary = FindKeyframeBoundary(motionParam, adjustedCursorPosition);
            if (boundary != null)
                return boundary;

            // Check shadow keyframes (only if enabled)
            if (track.TrackMotion.ShadowEnabled)
            {
                var shadowParam = new TrackMotionKeyframeParameter(
                    track.TrackMotion.ShadowKeyframes, TrackMotionType.Shadow);
                boundary = FindKeyframeBoundary(shadowParam, adjustedCursorPosition);
                if (boundary != null)
                    return boundary;
            }

            // Check glow keyframes (only if enabled)
            if (track.TrackMotion.GlowEnabled)
            {
                var glowParam = new TrackMotionKeyframeParameter(
                    track.TrackMotion.GlowKeyframes, TrackMotionType.Glow);
                boundary = FindKeyframeBoundary(glowParam, adjustedCursorPosition);
                if (boundary != null)
                    return boundary;
            }

            return null;
        }

        /// <summary>
        /// Finds keyframe boundaries for Parent Track Motion (motion/shadow/glow).
        /// Requires track.IsCompositingParent to be true.
        /// </summary>
        public KeyframeBoundary FindParentTrackMotionBoundary(
            VideoTrack track,
            Timecode adjustedCursorPosition)
        {
            if (!ParentTrackMotionParameter.IsCompositingParent(track))
                return null;

            // Check parent motion keyframes
            if (ParentTrackMotionParameter.HasAnimatedParentMotion(track))
            {
                var param = new ParentTrackMotionParameter(track, TrackMotionType.Motion);
                var boundary = FindKeyframeBoundary(param, adjustedCursorPosition);
                if (boundary != null)
                    return boundary;
            }

            // Check parent shadow keyframes
            if (ParentTrackMotionParameter.HasAnimatedParentShadow(track))
            {
                var param = new ParentTrackMotionParameter(track, TrackMotionType.Shadow);
                var boundary = FindKeyframeBoundary(param, adjustedCursorPosition);
                if (boundary != null)
                    return boundary;
            }

            // Check parent glow keyframes
            if (ParentTrackMotionParameter.HasAnimatedParentGlow(track))
            {
                var param = new ParentTrackMotionParameter(track, TrackMotionType.Glow);
                var boundary = FindKeyframeBoundary(param, adjustedCursorPosition);
                if (boundary != null)
                    return boundary;
            }

            // Check parent composite mode effect
            var compositeModeEffect = ParentTrackMotionParameter.GetParentCompositeModeEffect(track);
            if (compositeModeEffect != null)
            {
                var param = new EffectKeyframeParameter(compositeModeEffect);
                var boundary = FindKeyframeBoundary(param, adjustedCursorPosition);
                if (boundary != null)
                    return boundary;
            }

            return null;
        }

        /// <summary>
        /// Finds keyframe boundaries for Envelopes (Composite Level, Fade to Color, etc.)
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
        /// Finds keyframe boundaries for FadeIn Transition Progress envelope.
        /// Has special timecode scaling - envelope max is ~6 frames regardless of fade length.
        /// </summary>
        public KeyframeBoundary FindFadeInTransitionBoundary(
            VideoEvent videoEvent,
            Timecode cursorPositionRelativeToFade)
        {
            var param = TransitionProgressParameter.FromFadeIn(videoEvent);
            if (param == null)
                return null;

            // Scale cursor for envelope lookup
            var scaledCursor = param.ScaleCursorForLookup(cursorPositionRelativeToFade);
            return FindKeyframeBoundaryWithScaling(param, scaledCursor);
        }

        /// <summary>
        /// Finds keyframe boundaries for FadeOut Transition Progress envelope.
        /// Has special timecode scaling - envelope max is ~6 frames regardless of fade length.
        /// </summary>
        public KeyframeBoundary FindFadeOutTransitionBoundary(
            VideoEvent videoEvent,
            Timecode cursorPositionRelativeToFade)
        {
            var param = TransitionProgressParameter.FromFadeOut(videoEvent);
            if (param == null)
                return null;

            // Scale cursor for envelope lookup
            var scaledCursor = param.ScaleCursorForLookup(cursorPositionRelativeToFade);
            return FindKeyframeBoundaryWithScaling(param, scaledCursor);
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

                // Check if cursor is between these keyframes (with tolerance)
                if (KeyframeCalculations.AreTimesClose(currentTime, cursorPosition) ||
                    (currentTime <= cursorPosition && cursorPosition <= nextTime))
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
        /// Find keyframe boundary using pre-scaled cursor position.
        /// Used for Media Generator and Transition Progress which have non-standard timecode.
        /// </summary>
        private KeyframeBoundary FindKeyframeBoundaryWithScaling(
            IAnimatableParameter parameter,
            Timecode scaledCursorPosition)
        {
            var keyframes = parameter.GetKeyframes().ToList();
            if (keyframes.Count < 2)
                return null;

            object startKeyframe = null;
            object endKeyframe = null;

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                // For scaled parameters, we need raw time for comparison
                Timecode currentTime, nextTime;

                if (parameter is MediaGeneratorParameter mgp)
                {
                    currentTime = mgp.GetRawKeyframeTime(keyframes[i]);
                    nextTime = mgp.GetRawKeyframeTime(keyframes[i + 1]);
                }
                else if (parameter is TransitionProgressParameter tpp)
                {
                    currentTime = tpp.GetRawKeyframeTime(keyframes[i]);
                    nextTime = tpp.GetRawKeyframeTime(keyframes[i + 1]);
                }
                else
                {
                    currentTime = parameter.GetKeyframeTime(keyframes[i]);
                    nextTime = parameter.GetKeyframeTime(keyframes[i + 1]);
                }

                // Check if cursor is between these keyframes
                if (KeyframeCalculations.AreTimesClose(currentTime, scaledCursorPosition) ||
                    (currentTime <= scaledCursorPosition && scaledCursorPosition <= nextTime))
                {
                    startKeyframe = keyframes[i];
                    endKeyframe = keyframes[i + 1];
                    break;
                }
            }

            if (startKeyframe == null || endKeyframe == null)
                return null;

            // Return times in the parameter's native format (will be scaled when accessed)
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
        /// Wraps an OFXParameter in the appropriate interface implementation.
        /// Filters for animated and supported parameter types.
        /// </summary>
        private IAnimatableParameter WrapOFXParameter(OFXParameter param)
        {
            // Filter: must be animated and supported type
            if (!param.CanAnimate || !param.IsAnimated)
                return null;

            if (string.IsNullOrEmpty(param.Label) && string.IsNullOrEmpty(param.Name))
                return null;

            // Check for supported types and wrap accordingly
            return param switch
            {
                OFXDoubleParameter dp => new OFXParameterWrapper<double, OFXDoubleKeyframe>(dp),
                OFXDouble2DParameter d2p => new OFXParameterWrapper<OFXDouble2D, OFXDouble2DKeyframe>(d2p),
                OFXDouble3DParameter d3p => new OFXParameterWrapper<OFXDouble3D, OFXDouble3DKeyframe>(d3p),
                OFXIntegerParameter ip => new OFXParameterWrapper<int, OFXIntegerKeyframe>(ip),
                OFXInteger2DParameter i2p => new OFXParameterWrapper<OFXInteger2D, OFXInteger2DKeyframe>(i2p),
                OFXInteger3DParameter i3p => new OFXParameterWrapper<OFXInteger3D, OFXInteger3DKeyframe>(i3p),
                OFXRGBParameter rgbp => new OFXParameterWrapper<OFXColor, OFXRGBKeyframe>(rgbp),
                OFXRGBAParameter rgbap => new OFXParameterWrapper<OFXColor, OFXRGBAKeyframe>(rgbap),
                // OFXCustomParameter is not interpolatable in the standard way
                _ => null
            };
        }
    }
}
