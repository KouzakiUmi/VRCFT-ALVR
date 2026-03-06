using VRCFaceTracking.Core.Params.Data;

namespace ALVRModule
{
    public class EyeTracking
    {
        private static readonly float[] Identity = new float[] { 0, 0, 0, 1 };

        // Code taken from https://github.com/regzo2/VRCFaceTracking-QuestProOpenXR/blob/9b1b15b4b74fc070784a2f0370da1b47b756aac6/VRCFT%20-%20Quest%20OpenXR/QuestOpenXRTrackingModule.cs#L117
        private static (float pitch, float yaw) NormalizedGaze(float[] p)
        {
            float x = p[0];
            float y = p[1];
            float z = p[2];
            float w = p[3];

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

            var left = NormalizedGaze(p.Params?[0..4] ?? Identity);
            var right = NormalizedGaze(p.Params?[4..8] ?? Identity);

            // 修改这里的赋值逻辑：x对应yaw(左右)，y对应pitch(上下)
            eye.Left.Gaze.x = -left.yaw;
            eye.Left.Gaze.y = left.pitch;
            
            eye.Right.Gaze.x = right.yaw; // 右眼无负号，解决斗鸡眼
            eye.Right.Gaze.y = right.pitch;

            eye.Left.PupilDiameter_MM = 5f;
            eye.Right.PupilDiameter_MM = 5f;
            eye._minDilation = 0;
            eye._maxDilation = 10;
        }

        public static void SetCombEyesQuatParams(FloatParams p, FloatWeightParams w, UnifiedEyeData eye)
        {
            p.Read(4);
            var combined = NormalizedGaze(p.Params ?? Identity);

            // 同样修改混合眼动的映射
            eye.Left.Gaze.x = -combined.yaw;
            eye.Right.Gaze.x = combined.yaw;
            eye.Left.Gaze.y = eye.Right.Gaze.y = combined.pitch;

            eye.Left.PupilDiameter_MM = 5f;
            eye.Right.PupilDiameter_MM = 5f;
            eye._minDilation = 0;
            eye._maxDilation = 10;
        }
    }
}
