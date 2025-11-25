using ScriptPortal.Vegas;
using System;

namespace VPFlowWebMain.Lib
{
    /// <summary>
    /// Utility class for keyframe calculations and timecode scaling.
    /// Vegas API has inconsistent timecode handling for different parameter types.
    /// </summary>
    public static class KeyframeCalculations
    {
        /// <summary>
        /// Scale UP the cursor position to Media Generator's internal time.
        /// Media Generators use a different timecode scale based on frame rate.
        /// Convert from Nanos to MS via dividing with 1_000_000.0
        /// </summary>
        /// <param name="time">The cursor position in normal timeline time</param>
        /// <param name="frameRate">The project's video frame rate</param>
        /// <returns>Scaled timecode for Media Generator keyframe lookup</returns>
        public static Timecode ScaleUpCursorForMediaGenerator(Timecode time, double frameRate)
        {
            return Timecode.FromNanos(
                (long)(time.Nanos * (1_000.0 / frameRate))
            );
        }

        /// <summary>
        /// Scale DOWN from Media Generator's internal time to normal timeline time.
        /// Convert from MS to Nanos via dividing with 1_000_000.0
        /// </summary>
        /// <param name="time">The Media Generator's keyframe time</param>
        /// <param name="frameRate">The project's video frame rate</param>
        /// <returns>Normal timeline timecode</returns>
        public static Timecode ScaleDownCursorForMediaGenerator(Timecode time, double frameRate)
        {
            return Timecode.FromNanos(
                (long)(time.Nanos * (frameRate / 1_000.0))
            );
        }

        /// <summary>
        /// Scale cursor position for Fade Transition Progress Envelope.
        /// The last point in Fade Transition Progress Envelope incorrectly has 
        /// a maximum of ~6 frames Timecode value regardless of actual fade length.
        /// </summary>
        /// <param name="lastPointNanos">The last point's Timecode in Nanos (the envelope's max)</param>
        /// <param name="lengthNanos">Actual length of the Fade in Nanos</param>
        /// <param name="cursorNanos">Current Cursor position in Nanos (relative to fade start)</param>
        /// <param name="scaleUp">If true, scales from envelope space to real space; if false, vice versa</param>
        /// <returns>Correctly scaled cursor position</returns>
        public static long ScaleCursorToFadeEnvelope(
            long? lastPointNanos,
            long lengthNanos,
            long cursorNanos,
            bool scaleUp = false)
        {
            var scale = lastPointNanos ?? 0;
            if (scale == 0 || lengthNanos == 0)
                return cursorNanos;

            var ratio = (double)scale / lengthNanos;
            var scaledCursor = scaleUp
                ? (long)(cursorNanos / ratio)
                : (long)(cursorNanos * ratio);

            return scaledCursor;
        }

        /// <summary>
        /// Check with a Tolerance rather than exact Equal comparison
        /// since Vegas has floating-point precision issues.
        /// </summary>
        /// <param name="a">First timecode</param>
        /// <param name="b">Second timecode</param>
        /// <param name="toleranceNanos">Tolerance in nanoseconds (default 500)</param>
        /// <returns>True if times are within tolerance</returns>
        public static bool AreTimesClose(Timecode a, Timecode b, long toleranceNanos = 500)
        {
            return Math.Abs(a.Nanos - b.Nanos) <= toleranceNanos;
        }

        /// <summary>
        /// Check with a Tolerance rather than exact Equal comparison
        /// since Vegas has floating-point precision issues.
        /// </summary>
        /// <param name="a">First time in nanoseconds</param>
        /// <param name="b">Second time in nanoseconds</param>
        /// <param name="toleranceNanos">Tolerance in nanoseconds (default 500)</param>
        /// <returns>True if times are within tolerance</returns>
        public static bool AreTimesClose(long a, long b, long toleranceNanos = 500)
        {
            return Math.Abs(a - b) <= toleranceNanos;
        }

        /// <summary>
        /// Linearly interpolate between two double values.
        /// </summary>
        public static double Lerp(double start, double end, double t)
        {
            return start + (end - start) * t;
        }

        /// <summary>
        /// Linearly interpolate between two int values.
        /// </summary>
        public static int LerpInt(int start, int end, double t)
        {
            return (int)Math.Round(start + (end - start) * t);
        }

        /// <summary>
        /// Linearly interpolate between two float values.
        /// </summary>
        public static float LerpFloat(float start, float end, double t)
        {
            return (float)(start + (end - start) * t);
        }
    }
}
