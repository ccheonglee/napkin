/****
 * Copyright (c) 2013 Chris J Daly (github user cjdaly)
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 * 
 * Contributors:
 *   cjdaly - initial API and implementation
 ****/
using System;
using System.Collections;
using System.Text;
using Microsoft.SPOT;

namespace NapkinCommon
{
    public abstract class Sampler
    {
        public Sampler(string statusKeyPrefix, string keySetId)
        {
            Reset();
            _statusKeyPrefix = keySetId + statusKeyPrefix;
        }

        public string StatusKeyPrefix { get { return _statusKeyPrefix; } }
        protected string _statusKeyPrefix;

        protected int _samples = 0;
        public int Samples { get { return _samples; } }
        public bool HasSamples { get { return _samples > 0; } }

        public abstract void Sample();
        public abstract void Reset();
        public abstract string GetStatus(string headline = null);
    }

    public class LongSampler : Sampler
    {
        public delegate long TakeLongSample();
        private TakeLongSample _sampler;

        public LongSampler(TakeLongSample sampler, string statusKeyPrefix, string keySetId = "chatter.device.sensor.")
            : base(statusKeyPrefix, keySetId)
        {
            _sampler = sampler;
        }

        private long _total;
        private long _low;
        private long _high;

        public long Average { get { return _samples == 0 ? 0 : _total / _samples; } }

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
            sb.Append(StatusKeyPrefix).AppendLine("~samples=" + Samples);
            sb.Append(StatusKeyPrefix).AppendLine("~average=" + Average);
            sb.Append(StatusKeyPrefix).AppendLine("~high=" + _high);
            sb.Append(StatusKeyPrefix).AppendLine("~low=" + _low);
            return sb.ToString();
        }
    }

    public class DoubleSampler : Sampler
    {
        public delegate double TakeDoubleSample();
        private TakeDoubleSample _sampler;

        public DoubleSampler(TakeDoubleSample sampler, string statusKeyPrefix, string keySetId = "chatter.device.sensor.")
            : base(statusKeyPrefix, keySetId)
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
            sb.Append(StatusKeyPrefix).AppendLine("~samples=" + Samples);
            sb.Append(StatusKeyPrefix).AppendLine("~average=" + Average);
            sb.Append(StatusKeyPrefix).AppendLine("~high=" + _high);
            sb.Append(StatusKeyPrefix).AppendLine("~low=" + _low);
            return sb.ToString();
        }
    }

    public class SamplerBag
    {
        private ArrayList _samplerIds = new ArrayList();
        private Hashtable _samplerIdToSampler = new Hashtable();

        public SamplerBag(bool addMemorySampler = true)
        {
            if (addMemorySampler)
            {
                Add(new LongSampler(TakeMemorySample, "memory", "chatter.device.vitals."));
            }
        }

        private static long TakeMemorySample()
        {
            return Debug.GC(false);
        }

        public void Sample(string samplerId)
        {
            Sampler sampler = Get(samplerId);
            if (sampler != null)
            {
                sampler.Sample();
            }
        }

        public void Add(Sampler sampler)
        {
            _samplerIds.Add(sampler.StatusKeyPrefix);
            _samplerIdToSampler[sampler.StatusKeyPrefix] = sampler;
        }

        public Sampler Get(string samplerId, string keySetId = "chatter.device.sensor.")
        {
            return (Sampler)_samplerIdToSampler[keySetId + samplerId];
        }

        public void Reset()
        {
            foreach (String id in _samplerIds)
            {
                Sampler sampler = (Sampler)_samplerIdToSampler[id];
                sampler.Reset();
            }
        }

        public StringBuilder AppendStatus(StringBuilder sb = null)
        {
            if (sb == null) sb = new StringBuilder();
            foreach (String id in _samplerIds)
            {
                Sampler sampler = (Sampler)_samplerIdToSampler[id];
                if (sampler.HasSamples) sb.Append(sampler.GetStatus());
            }
            return sb;
        }

        public string GetStatus(string headline = null)
        {
            StringBuilder sb = new StringBuilder();
            if (headline != null) sb.AppendLine(headline);
            AppendStatus(sb);
            return sb.ToString();
        }

    }

}
