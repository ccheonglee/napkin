using System;
using System.Text;
using Microsoft.SPOT;

namespace NapkinCommon
{
    public abstract class Sampler
    {
        public Sampler(string statusKeyPrefix)
        {
            Reset();
            _statusKeyPrefix = statusKeyPrefix;
        }

        public string StatusKeyPrefix { get { return _statusKeyPrefix; } }
        protected string _statusKeyPrefix;

        protected int _samples = 0;
        public int Samples { get { return _samples; } }

        public abstract void Sample();
        public abstract void Reset();
        public abstract string GetStatus(string headline = null);

        private static long TakeMemorySample()
        {
            return Debug.GC(false);
        }
        public static LongSampler CreateMemCheck(string statusKeyPrefix = "memory")
        {
            LongSampler sampler = new LongSampler(TakeMemorySample, statusKeyPrefix);
            return sampler;
        }

    }

    public class LongSampler : Sampler
    {
        public delegate long TakeLongSample();
        private TakeLongSample _sampler;

        public LongSampler(TakeLongSample sampler, string statusKeyPrefix)
            : base(statusKeyPrefix)
        {
            _sampler = sampler;
        }

        private long _total;
        private long _low;
        private long _high;

        public long Average { get { return _total / _samples; } }

        public override void Reset()
        {
            _samples = 0;
            _total = 0;
            _low = long.MaxValue;
            _high = long.MinValue;
        }

        public override void Sample()
        {
            long sample = _sampler();
            Sample(sample);
        }

        public void Sample(long sample)
        {
            _total += sample;
            if (sample < _low) _low = sample;
            if (sample > _high) _high = sample;
            _samples++;
        }

        public override string GetStatus(string headline = null)
        {
            StringBuilder sb = new StringBuilder();
            if (headline != null) sb.AppendLine(headline);
            sb.Append(StatusKeyPrefix).AppendLine("_samples=" + Samples);
            sb.Append(StatusKeyPrefix).AppendLine("_average=" + Average);
            sb.Append(StatusKeyPrefix).AppendLine("_high=" + _high);
            sb.Append(StatusKeyPrefix).AppendLine("_low=" + _low);
            return sb.ToString();
        }
    }

    public class DoubleSampler : Sampler
    {
        public delegate double TakeDoubleSample();
        private TakeDoubleSample _sampler;

        public DoubleSampler(TakeDoubleSample sampler, string statusKeyPrefix)
            : base(statusKeyPrefix)
        {
            _sampler = sampler;
        }

        private double _total;
        private double _low;
        private double _high;

        public double Average { get { return (_samples == 0) ? 0 : (_total / _samples); } }

        public override void Reset()
        {
            _samples = 0;
            _total = 0;
            _low = double.MaxValue;
            _high = double.MinValue;
        }

        public override void Sample()
        {
            double sample = _sampler();
            Sample(sample);
        }

        public void Sample(double sample)
        {
            _total += sample;
            if (sample < _low) _low = sample;
            if (sample > _high) _high = sample;
            _samples++;
        }

        public override string GetStatus(string headline = null)
        {
            StringBuilder sb = new StringBuilder();
            if (headline != null) sb.AppendLine(headline);
            sb.Append(StatusKeyPrefix).AppendLine("_samples=" + Samples);
            sb.Append(StatusKeyPrefix).AppendLine("_average=" + Average);
            sb.Append(StatusKeyPrefix).AppendLine("_high=" + _high);
            sb.Append(StatusKeyPrefix).AppendLine("_low=" + _low);
            return sb.ToString();
        }
    }

}