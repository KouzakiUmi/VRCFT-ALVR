using VRCFaceTracking.Core.Params.Data;

namespace ALVRModule
{
    public class EyeTracking
    {
        private static readonly float[] Identity = new float[] { 0, 0, 0, 1 };

        // Code taken from https://github.com/regzo2/VRCFaceTracking-QuestProOpenXR/blob/9b1b15b4b74fc070784a2f0370da1b47b756aac6/VRCFT%20-%20Quest%20OpenXR/QuestOpenXRTrackingModule.cs#L117
        private static (float pitch, float yaw) NormalizedGaze(float[] p, int offset)
        {
            float x = p[offset + 0];
            float y = p[offset + 1];
            float z = p[offset + 2];
            float w = p[offset + 3];

            float magnitude = (float)Math.Sqrt(x * x + y * y + z * z + w * w);
            x /= magnitude;
            y /= magnitude;
            z /= magnitude;
            w /= magnitude;

            float pitch = (float)Math.Asin(2.0 * (x * z - w * y));
            float yaw = (float)Math.Atan2(2.0 * (y * z + w * x), w * w - x * x - y * y + z * z);

            return (pitch, yaw);
        }

        public static void SetEyesQuatParams(FloatParams p, FloatWeightParams w, UnifiedEyeData eye)
        {
            p.Read(8);

            var arr = p.Params.Length >= 8 ? p.Params : Identity;
            (eye.Left.Gaze.x, eye.Left.Gaze.y) = NormalizedGaze(arr, 0);
            (eye.Right.Gaze.x, eye.Right.Gaze.y) = NormalizedGaze(arr, arr == Identity ? 0 : 4);

            eye.Left.PupilDiameter_MM = 5f;
            eye.Right.PupilDiameter_MM = 5f;

            eye._minDilation = 0;
            eye._maxDilation = 10;
        }

        public static void SetCombEyesQuatParams(FloatParams p, FloatWeightParams w, UnifiedEyeData eye)
        {
            p.Read(4);

            var arr = p.Params.Length >= 4 ? p.Params : Identity;
            (eye.Left.Gaze.x, eye.Left.Gaze.y) = NormalizedGaze(arr, 0);
            (eye.Right.Gaze.x, eye.Right.Gaze.y) = NormalizedGaze(arr, 0);

            eye.Left.PupilDiameter_MM = 5f;
            eye.Right.PupilDiameter_MM = 5f;

            eye._minDilation = 0;
            eye._maxDilation = 10;
        }
    }
}
