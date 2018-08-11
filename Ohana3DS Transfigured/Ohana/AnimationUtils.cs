using System.Collections.Generic;

namespace Ohana3DS_Transfigured.Ohana
{
    class AnimationUtils
    {
        /// <summary>
        ///     Gets the rounded down frame relative to the given Key Frame.
        /// </summary>
        /// <param name="keyFrames">List with the Key Frames</param>
        /// <param name="frame">The frame number used as reference</param>
        /// <returns></returns>
        public static RenderBase.OAnimationKeyFrame GetLeftFrame(List<RenderBase.OAnimationKeyFrame> keyFrames, float frame)
        {
            if (keyFrames == null || keyFrames.Count == 0) return null;
            RenderBase.OAnimationKeyFrame value = keyFrames[0];
            foreach (RenderBase.OAnimationKeyFrame key in keyFrames)
            {
                if (key.frame >= value.frame && key.frame <= frame) value = key;
            }

            return value;
        }

        /// <summary>
        ///     Gets the rounded up frame relative to the given Key Frame.
        /// </summary>
        /// <param name="keyFrames">List with the Key Frames</param>
        /// <param name="frame">The frame number used as reference</param>
        /// <returns></returns>
        public static RenderBase.OAnimationKeyFrame GetRightFrame(List<RenderBase.OAnimationKeyFrame> keyFrames, float frame)
        {
            if (keyFrames == null || keyFrames.Count == 0) return null;
            RenderBase.OAnimationKeyFrame value = keyFrames[keyFrames.Count - 1];
            foreach (RenderBase.OAnimationKeyFrame key in keyFrames)
            {
                if (key.frame <= value.frame && key.frame >= frame) value = key;
            }

            return value;
        }

        /// <summary>
        ///     Gets the smaller point between two Key Frames.
        ///     It doesn't actually interpolates anything, just returns the closest value.
        /// </summary>
        /// <param name="keyFrames">The list with all available Key Frames (Linear format)</param>
        /// <param name="frame">The frame number that should be returned</param>
        /// <returns>The closest smaller frame value</returns>
        public static float InterpolateStep(List<RenderBase.OAnimationKeyFrame> keyFrames, float frame)
        {
            return GetLeftFrame(keyFrames, frame).value;
        }

        /// <summary>
        ///     Interpolates a point between two Key Frames on a given Frame using Linear Interpolation.
        /// </summary>
        /// <param name="keyFrames">The list with all available Key Frames (Linear format)</param>
        /// <param name="frame">The frame number that should be interpolated</param>
        /// <returns>The interpolated frame value</returns>
        public static float InterpolateLinear(List<RenderBase.OAnimationKeyFrame> keyFrames, float frame)
        {
            RenderBase.OAnimationKeyFrame a = GetLeftFrame(keyFrames, frame);
            RenderBase.OAnimationKeyFrame b = GetRightFrame(keyFrames, frame);
            if (a.frame == b.frame) return a.value;

            float mu = (frame - a.frame) / (b.frame - a.frame);
            return (a.value * (1 - mu) + b.value * mu);
        }

        /// <summary>
        ///     Interpolates a point between two vectors using Linear Interpolation.
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <param name="mu">Value between 0-1 of the interpolation amount</param>
        /// <returns></returns>
        public static RenderBase.OVector3 InterpolateLinear(RenderBase.OVector3 a, RenderBase.OVector3 b, float mu)
        {
            RenderBase.OVector3 output = new RenderBase.OVector3
            {
                x = InterpolateLinear(a.x, b.x, mu),
                y = InterpolateLinear(a.y, b.y, mu),
                z = InterpolateLinear(a.z, b.z, mu)
            };

            return output;
        }

        /// <summary>
        ///     Interpolates a point between two vectors using Linear Interpolation.
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <param name="mu">Value between 0-1 of the interpolation amount</param>
        /// <returns></returns>
        public static RenderBase.OVector4 InterpolateLinear(RenderBase.OVector4 a, RenderBase.OVector4 b, float mu)
        {
            RenderBase.OVector4 output = new RenderBase.OVector4
            {
                x = InterpolateLinear(a.x, b.x, mu),
                y = InterpolateLinear(a.y, b.y, mu),
                z = InterpolateLinear(a.z, b.z, mu),
                w = InterpolateLinear(a.w, b.w, mu)
            };

            return output;
        }

        /// <summary>
        ///     Interpolates a point between two points using Linear Interpolation.
        /// </summary>
        /// <param name="a">First point</param>
        /// <param name="b">Second point</param>
        /// <param name="mu">Value between 0-1 of the interpolation amount</param>
        /// <returns></returns>
        public static float InterpolateLinear(float a, float b, float mu)
        {
            return (a * (1 - mu) + b * mu);
        }

        /// <summary>
        ///     Interpolates a point between two Key Frames on a given Frame using Hermite Interpolation.
        /// </summary>
        /// <param name="keyFrames">The list with all available Key Frames (Hermite format)</param>
        /// <param name="frame">The frame number that should be interpolated</param>
        /// <returns>The interpolated frame value</returns>
        public static float InterpolateHermite(List<RenderBase.OAnimationKeyFrame> keyFrames, float frame)
        {
            RenderBase.OAnimationKeyFrame a = GetLeftFrame(keyFrames, frame);
            RenderBase.OAnimationKeyFrame b = GetRightFrame(keyFrames, frame);
            if (a.frame == b.frame) return a.value;

            float outSlope = a.outSlope;
            float inSlope = b.inSlope;
            float distance = frame - a.frame;
            float invDuration = 1f / (b.frame - a.frame);
            float t = distance * invDuration;
            float t1 = t - 1f;
            return (a.value + ((((a.value - b.value) * ((2f * t) - 3f)) * t) * t)) + ((distance * t1) * ((t1 * outSlope) + (t * inSlope)));
        }

        /// <summary>
        ///     Interpolates a Key Frame from a list of Key Frames.
        /// </summary>
        /// <param name="sourceFrame">The list of key frames</param>
        /// <param name="frame">The frame that should be returned or interpolated from the list</param>
        /// <returns></returns>
        public static float GetKey(RenderBase.OAnimationKeyFrameGroup sourceFrame, float frame)
        {
            switch (sourceFrame.interpolation)
            {
                case RenderBase.OInterpolationMode.step: return InterpolateStep(sourceFrame.keyFrames, frame);
                case RenderBase.OInterpolationMode.linear: return InterpolateLinear(sourceFrame.keyFrames, frame);
                case RenderBase.OInterpolationMode.hermite: return InterpolateHermite(sourceFrame.keyFrames, frame);
                default: return 0; //Shouldn't happen
            }
        }
    }
}
