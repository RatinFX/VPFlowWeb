using ScriptPortal.Vegas;
using System;
using System.Collections.Generic;
using System.Linq;
using VPFlowWebMain.Lib;

namespace VPFlowWebMain.Models
{
    /// <summary>
    /// Wrapper for VideoMotion (Pan/Crop) keyframes.
    /// 
    /// IMPORTANT: VideoMotionKeyframe has a bug where setting Bounds resets Rotation.
    /// Workaround: Save rotation, reset via RotateBy(-rot), set bounds, then restore via RotateBy(rot).
    /// </summary>
    internal class VideoMotionParameter : IAnimatableParameter
    {
        private readonly VideoMotion _videoMotion;

        public VideoMotionParameter(VideoMotion videoMotion)
        {
            _videoMotion = videoMotion;
        }

        public string Name => "Pan/Crop";
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

            if (value is VideoMotionInterpolatedValue interpolated)
            {
                // Apply the interpolated values with rotation bug workaround
                ApplyInterpolatedValue(kf, interpolated);
            }

            _videoMotion.Keyframes.Add(kf);
        }

        /// <summary>
        /// Applies interpolated values to a keyframe, handling the rotation bug.
        /// </summary>
        private void ApplyInterpolatedValue(VideoMotionKeyframe kf, VideoMotionInterpolatedValue value)
        {
            // Save current rotation, reset it, set bounds, restore rotation
            var currentRot = kf.Rotation;
            kf.RotateBy(-currentRot);

            // Set bounds from the interpolated vertices
            kf.Bounds = new VideoMotionBounds(
                new VideoMotionVertex(value.TopLeftX, value.TopLeftY),
                new VideoMotionVertex(value.TopRightX, value.TopRightY),
                new VideoMotionVertex(value.BottomRightX, value.BottomRightY),
                new VideoMotionVertex(value.BottomLeftX, value.BottomLeftY)
            );

            // Set rotation center
            kf.Center = new VideoMotionVertex(value.CenterX, value.CenterY);

            // Apply the target rotation
            kf.RotateBy(value.Rotation);

            // Set keyframe type to linear
            kf.Type = VideoKeyframeType.Linear;
        }

        public object InterpolateValue(object startKf, object endKf, double t)
        {
            var start = (VideoMotionKeyframe)startKf;
            var end = (VideoMotionKeyframe)endKf;

            // Calculate the middle points for position interpolation
            var startMid = GetMiddlePoint(start);
            var endMid = GetMiddlePoint(end);

            // Calculate dimensions from vertices
            var startWidth = CalculateWidth(start.Bounds);
            var startHeight = CalculateHeight(start.Bounds);
            var endWidth = CalculateWidth(end.Bounds);
            var endHeight = CalculateHeight(end.Bounds);

            // Interpolate all properties
            var width = KeyframeCalculations.Lerp(startWidth, endWidth, t);
            var height = KeyframeCalculations.Lerp(startHeight, endHeight, t);
            var midX = KeyframeCalculations.LerpFloat(startMid.X, endMid.X, t);
            var midY = KeyframeCalculations.LerpFloat(startMid.Y, endMid.Y, t);
            var centerX = KeyframeCalculations.LerpFloat(start.Center.X, end.Center.X, t);
            var centerY = KeyframeCalculations.LerpFloat(start.Center.Y, end.Center.Y, t);
            var rotation = KeyframeCalculations.Lerp(start.Rotation, end.Rotation, t);

            // Calculate new corner positions based on interpolated midpoint and dimensions
            var halfWidth = (float)(width / 2);
            var halfHeight = (float)(height / 2);

            return new VideoMotionInterpolatedValue
            {
                TopLeftX = midX - halfWidth,
                TopLeftY = midY - halfHeight,
                TopRightX = midX + halfWidth,
                TopRightY = midY - halfHeight,
                BottomRightX = midX + halfWidth,
                BottomRightY = midY + halfHeight,
                BottomLeftX = midX - halfWidth,
                BottomLeftY = midY + halfHeight,
                CenterX = centerX,
                CenterY = centerY,
                Rotation = rotation
            };
        }

        /// <summary>
        /// Returns the middle point of a VideoMotionKeyframe's bounds.
        /// </summary>
        public static VideoMotionVertex GetMiddlePoint(VideoMotionKeyframe kf)
        {
            float x = 0, y = 0;
            var corners = new[] { kf.TopLeft, kf.TopRight, kf.BottomRight, kf.BottomLeft };
            foreach (var c in corners)
            {
                x += c.X;
                y += c.Y;
            }

            x /= corners.Length;
            y /= corners.Length;
            return new VideoMotionVertex(x, y);
        }

        /// <summary>
        /// Calculates width from bounds vertices.
        /// </summary>
        private double CalculateWidth(VideoMotionBounds bounds)
        {
            return Math.Sqrt(
                Math.Pow(bounds.TopRight.X - bounds.TopLeft.X, 2) +
                Math.Pow(bounds.TopRight.Y - bounds.TopLeft.Y, 2)
            );
        }

        /// <summary>
        /// Calculates height from bounds vertices.
        /// </summary>
        private double CalculateHeight(VideoMotionBounds bounds)
        {
            return Math.Sqrt(
                Math.Pow(bounds.TopLeft.X - bounds.BottomLeft.X, 2) +
                Math.Pow(bounds.TopLeft.Y - bounds.BottomLeft.Y, 2)
            );
        }
    }

    /// <summary>
    /// Helper class to hold interpolated VideoMotion values.
    /// Stores all four corners plus center and rotation.
    /// </summary>
    public class VideoMotionInterpolatedValue
    {
        public float TopLeftX { get; set; }
        public float TopLeftY { get; set; }
        public float TopRightX { get; set; }
        public float TopRightY { get; set; }
        public float BottomRightX { get; set; }
        public float BottomRightY { get; set; }
        public float BottomLeftX { get; set; }
        public float BottomLeftY { get; set; }
        public float CenterX { get; set; }
        public float CenterY { get; set; }
        public double Rotation { get; set; }
    }
}
