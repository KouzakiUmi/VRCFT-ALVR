using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;

namespace ALVRModule
{
    public class FloatParams
    {
        private byte[] _buffer = Array.Empty<byte>();
        private int _offset;

        public float[] Params { get; private set; } = Array.Empty<float>();

        public float this[Enum index]
        {
            get
            {
                int i = Convert.ToInt32(index);
                if (i >= 0 && i < Params.Length)
                {
                    return Params[i];
                }
                return 0f;
            }
        }

        public void UpdateBuffer(byte[] buffer, int offset)
        {
            _buffer = buffer;
            _offset = offset;
        }

        public void Read(int count)
        {
            if (Params.Length < count)
            {
                Params = new float[count];
            }

            int bytesToCopy = count * 4;
            if (_offset + bytesToCopy > _buffer.Length)
            {
                Array.Clear(Params, 0, count);
                _offset = _buffer.Length;
                return;
            }

            Buffer.BlockCopy(_buffer, _offset, Params, 0, bytesToCopy);
            _offset += bytesToCopy;
        }

        public int GetOffset() => _offset;
    }

    public class FloatWeightParams
    {
        public static readonly FloatWeightParams Instance = new();

        public float this[UnifiedExpressions index]
        {
            set
            {
                UnifiedTracking.Data.Shapes[Convert.ToInt32(index)].Weight = value;
            }
        }

        private FloatWeightParams() { }
    }
}
