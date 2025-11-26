using ScriptPortal.Vegas;
using System;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Lib;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for Media Generator effects.
    /// Media Generators use a different timecode scale that requires special handling.
    /// The cursor position must be scaled using ScaleUpCursorForMediaGenerator for lookups
    /// and the ActiveTake.Offset must be added to the cursor position.
    /// </summary>
    internal class MediaGeneratorParameter : IAnimatableParameter
    {
        private readonly Media _media;
        private readonly VideoEvent _event;
        private readonly double _frameRate;

        /// <summary>
        /// The take offset that must be added to cursor position before lookup
        /// </summary>
        public Timecode TakeOffset => _event?.ActiveTake?.Offset ?? Timecode.FromFrames(0);

        public MediaGeneratorParameter(VideoEvent videoEvent, double frameRate)
        {
            _event = videoEvent;
            _media = videoEvent?.ActiveTake?.Media;
            _frameRate = frameRate;
        }

        /// <summary>
        /// Checks if a VideoEvent has an animatable Media Generator
        /// </summary>
        public static bool IsMediaGenerator(VideoEvent videoEvent)
        {
            if (videoEvent == null)
                return false;

            var generator = videoEvent.ActiveTake?.Media?.Generator;
            if (generator?.PlugIn == null)
                return false;

            // Either OFX or has keyframes
            return (generator.PlugIn.IsOFX) || (generator.Keyframes.Count > 1);
        }

        public string Name => _media?.Generator?.PlugIn?.Name ?? "Media Generator";
        public string Type => "MediaGenerator";

        /// <summary>
        /// Gets the generator's keyframes (DXT style)
        /// </summary>
        public IEnumerable<object> GetKeyframes()
        {
            var generator = _media?.Generator;
            if (generator == null)
                return Enumerable.Empty<object>();

            return generator.Keyframes.Cast<object>();
        }

        /// <summary>
        /// Gets the keyframe time, scaled down from Media Generator's internal time.
        /// </summary>
        public Timecode GetKeyframeTime(object keyframe)
        {
            var kf = keyframe as Keyframe;
            if (kf == null)
                return Timecode.FromFrames(0);

            // Scale down from Media Generator time to normal timeline time
            return KeyframeCalculations.ScaleDownCursorForMediaGenerator(kf.Position, _frameRate);
        }

        /// <summary>
        /// Gets the raw (unscaled) keyframe time for internal use
        /// </summary>
        public Timecode GetRawKeyframeTime(object keyframe)
        {
            return ((Keyframe)keyframe).Position;
        }

        /// <summary>
        /// Scales a cursor position for Media Generator keyframe lookup.
        /// Also adds the TakeOffset.
        /// </summary>
        public Timecode ScaleCursorForLookup(Timecode cursorPosition)
        {
            var adjusted = cursorPosition + TakeOffset;
            return KeyframeCalculations.ScaleUpCursorForMediaGenerator(adjusted, _frameRate);
        }

        public void AddKeyframe(Timecode time, object value)
        {
            var generator = _media?.Generator;
            if (generator == null)
                return;

            // Scale up to Media Generator time
            var scaledTime = KeyframeCalculations.ScaleUpCursorForMediaGenerator(time, _frameRate);
            var kf = new Keyframe(scaledTime);
            generator.Keyframes.Add(kf);
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            // DXT keyframes store entire effect state - interpolation is not straightforward
            // Return null as a placeholder; actual implementation would need preset copying
            return null;
        }
    }
}
