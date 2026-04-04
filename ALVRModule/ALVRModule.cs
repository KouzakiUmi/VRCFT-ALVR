using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Data;

namespace ALVRModule
{
    public class ALVRModule : ExtTrackingModule
    {
        private delegate void ParamsConsumer(FloatParams p, FloatWeightParams w, UnifiedEyeData eye);

        private const int Port = 0xA1F7;
        private const int PrefixSize = 8;

        private readonly UdpClient Socket = new(Port);
        private readonly byte[] _receiveBuffer = new byte[4096];
        private readonly FloatParams _floatParams = new();
        private readonly Dictionary<string, ParamsConsumer> Consumers = new()
        {
            ["EyesQuat"] = EyeTracking.SetEyesQuatParams,
            ["CombQuat"] = EyeTracking.SetCombEyesQuatParams,
            ["FaceFb\0\0"] = FbFaceTracking.SetFace1FbParams,
            ["Face2Fb\0"] = FbFaceTracking.SetFace2FbParams,
            ["FacePico"] = PicoFaceTracking.SetFacePicoParams,
            ["EyesHtc\0"] = HtcFaceTracking.SetEyesHtcParams,
            ["LipHtc\0\0"] = HtcFaceTracking.SetLipHtcParams,
        };

        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            ModuleInformation.Name = "ALVR";

            var stream = GetType().Assembly.GetManifestResourceStream("ALVRModule.Assets.alvr.png");
            ModuleInformation.StaticImages = stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;

            Socket.Client.ReceiveTimeout = 100;

            return (true, true);
        }

        public override void Update()
        {
            int length;

            try
            {
                length = Socket.Client.Receive(_receiveBuffer);
            }
            catch (Exception)
            {
                return;
            }

            int offset = 0;

            while (length - offset >= PrefixSize)
            {
                string prefix = Encoding.ASCII.GetString(_receiveBuffer, offset, PrefixSize);
                offset += PrefixSize;

                if (Consumers.TryGetValue(prefix, out var consumer) && consumer != null)
                {
                    _floatParams.UpdateBuffer(_receiveBuffer, offset);
                    consumer(_floatParams, FloatWeightParams.Instance, UnifiedTracking.Data.Eye);
                    offset = _floatParams.GetOffset();
                    continue;
                }

                Logger.LogError("[ALVR Module] Unrecognized prefix: {}", prefix);
                break;
            }
        }

        public override void Teardown()
        {
            Socket.Close();
        }
    }
}
