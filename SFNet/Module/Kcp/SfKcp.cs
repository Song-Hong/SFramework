// KCP 协议核心，基于 skywind3000/kcp (ikcp.c v1.7) 的 C# 移植
using System;
using System.Collections.Generic;

namespace SFramework.SFNet.Module.Kcp
{
    /// <summary>
    /// KCP 协议常量
    /// </summary>
    public static class SfKcpConst
    {
        public const int RtoNdl = 30;
        public const int RtoMin = 100;
        public const int RtoDef = 200;
        public const int RtoMax = 60000;
        public const int AskSend = 1;
        public const int AskTell = 2;
        public const int WndSnd = 32;
        public const int WndRcv = 128;
        public const int MtuDef = 1200;
        public const int AckFast = 3;
        public const int Interval = 100;
        public const int Overhead = SfKcpSegment.HeaderSize;
        public const int FrgMax = byte.MaxValue;
        public const int DeadLink = 20;
        public const int ThreshInit = 2;
        public const int ThreshMin = 2;
        public const int ProbeInit = 7000;
        public const int ProbeLimit = 120000;
        public const int FastAckLimit = 5;
    }

    internal static class SfKcpUtils
    {
        public static int TimeDiff(uint later, uint earlier) => (int)(later - earlier);

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            return value > max ? max : value;
        }
    }

    internal struct SfKcpAckItem
    {
        public uint Sn;
        public uint Ts;
    }

    /// <summary>
    /// KCP 协议状态机。不负责 UDP 收发，通过 output 回调输出待发送数据。
    /// </summary>
    public class SfKcp
    {
        readonly uint _conv;
        readonly Action<byte[], int> _output;

        int _state;
        uint _mtu;
        uint _mss;
        uint _sndUna;
        uint _sndNxt;
        uint _rcvNxt;
        uint _ssthresh;
        int _rxRttval;
        int _rxSrtt;
        int _rxRto;
        int _rxMinRto;
        uint _sndWnd;
        uint _rcvWnd;
        uint _rmtWnd;
        uint _cwnd;
        uint _probe;
        uint _interval;
        uint _tsFlush;
        uint _xmit;
        uint _nodelay;
        bool _updated;
        uint _tsProbe;
        uint _probeWait;
        uint _deadLink;
        uint _incr;
        uint _current;
        int _fastResend;
        int _fastLimit;
        bool _noCwnd;

        readonly Queue<SfKcpSegment> _sndQueue = new Queue<SfKcpSegment>();
        readonly Queue<SfKcpSegment> _rcvQueue = new Queue<SfKcpSegment>();
        readonly List<SfKcpSegment> _sndBuf = new List<SfKcpSegment>();
        readonly List<SfKcpSegment> _rcvBuf = new List<SfKcpSegment>();
        readonly List<SfKcpAckItem> _ackList = new List<SfKcpAckItem>();
        byte[] _buffer;

        public uint Conv => _conv;
        public int State => _state;
        public int WaitSnd => _sndBuf.Count + _sndQueue.Count;

        public SfKcp(uint conv, Action<byte[], int> output)
        {
            _conv = conv;
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _sndWnd = SfKcpConst.WndSnd;
            _rcvWnd = SfKcpConst.WndRcv;
            _rmtWnd = SfKcpConst.WndRcv;
            _mtu = SfKcpConst.MtuDef;
            _mss = _mtu - SfKcpConst.Overhead;
            _rxRto = SfKcpConst.RtoDef;
            _rxMinRto = SfKcpConst.RtoMin;
            _interval = SfKcpConst.Interval;
            _tsFlush = SfKcpConst.Interval;
            _ssthresh = SfKcpConst.ThreshInit;
            _fastLimit = SfKcpConst.FastAckLimit;
            _deadLink = SfKcpConst.DeadLink;
            _buffer = new byte[(_mtu + SfKcpConst.Overhead) * 3];
        }

        SfKcpSegment NewSegment() => new SfKcpSegment();

        void DeleteSegment(SfKcpSegment seg) => seg?.Reset();

        uint WndUnused()
        {
            if (_rcvQueue.Count < _rcvWnd)
                return _rcvWnd - (uint)_rcvQueue.Count;
            return 0;
        }

        public int Receive(byte[] buffer, int len)
        {
            if (len < 0)
                throw new NotSupportedException("不支持 peek 模式");

            if (_rcvQueue.Count == 0) return -1;

            int peekSize = PeekSize();
            if (peekSize < 0) return -2;
            if (peekSize > len) return -3;

            bool recover = _rcvQueue.Count >= _rcvWnd;
            int offset = 0;
            len = 0;

            while (_rcvQueue.Count > 0)
            {
                var seg = _rcvQueue.Dequeue();
                int segLen = seg.Data?.Length ?? 0;
                if (segLen > 0)
                    Buffer.BlockCopy(seg.Data, 0, buffer, offset, segLen);
                offset += segLen;
                len += segLen;
                byte fragment = seg.Frg;
                DeleteSegment(seg);
                if (fragment == 0) break;
            }

            int removed = 0;
            foreach (var seg in _rcvBuf)
            {
                if (seg.Sn == _rcvNxt && _rcvQueue.Count < _rcvWnd)
                {
                    ++removed;
                    _rcvQueue.Enqueue(seg);
                    _rcvNxt++;
                }
                else break;
            }
            _rcvBuf.RemoveRange(0, removed);

            if (_rcvQueue.Count < _rcvWnd && recover)
                _probe |= SfKcpConst.AskTell;

            return len;
        }

        public int PeekSize()
        {
            if (_rcvQueue.Count == 0) return -1;

            var seq = _rcvQueue.Peek();
            if (seq.Frg == 0) return seq.Data?.Length ?? 0;
            if (_rcvQueue.Count < seq.Frg + 1) return -1;

            int length = 0;
            foreach (var seg in _rcvQueue)
            {
                length += seg.Data?.Length ?? 0;
                if (seg.Frg == 0) break;
            }
            return length;
        }

        public int Send(byte[] buffer, int offset, int len)
        {
            if (len < 0) return -1;

            int count = len <= _mss ? 1 : (int)((len + _mss - 1) / _mss);
            if (count > SfKcpConst.FrgMax)
                throw new Exception($"Send len={len} 需要 {count} 个分片，KCP 最多支持 {SfKcpConst.FrgMax} 个分片");
            if (count >= _rcvWnd) return -2;
            if (count == 0) count = 1;

            for (int i = 0; i < count; i++)
            {
                int size = len > (int)_mss ? (int)_mss : len;
                var seg = NewSegment();
                if (size > 0)
                {
                    seg.Data = new byte[size];
                    Buffer.BlockCopy(buffer, offset, seg.Data, 0, size);
                }
                else
                {
                    seg.Data = Array.Empty<byte>();
                }
                seg.Frg = (byte)(count - i - 1);
                _sndQueue.Enqueue(seg);
                offset += size;
                len -= size;
            }
            return 0;
        }

        public int Send(byte[] buffer) => Send(buffer, 0, buffer?.Length ?? 0);

        void UpdateAck(int rtt)
        {
            if (_rxSrtt == 0)
            {
                _rxSrtt = rtt;
                _rxRttval = rtt / 2;
            }
            else
            {
                int delta = rtt - _rxSrtt;
                if (delta < 0) delta = -delta;
                _rxRttval = (3 * _rxRttval + delta) / 4;
                _rxSrtt = (7 * _rxSrtt + rtt) / 8;
                if (_rxSrtt < 1) _rxSrtt = 1;
            }
            int rto = _rxSrtt + Math.Max((int)_interval, 4 * _rxRttval);
            _rxRto = SfKcpUtils.Clamp(rto, _rxMinRto, SfKcpConst.RtoMax);
        }

        void ShrinkBuf()
        {
            _sndUna = _sndBuf.Count > 0 ? _sndBuf[0].Sn : _sndNxt;
        }

        void ParseAck(uint sn)
        {
            if (SfKcpUtils.TimeDiff(sn, _sndUna) < 0 || SfKcpUtils.TimeDiff(sn, _sndNxt) >= 0)
                return;

            for (int i = 0; i < _sndBuf.Count; ++i)
            {
                var seg = _sndBuf[i];
                if (sn == seg.Sn)
                {
                    _sndBuf.RemoveAt(i);
                    DeleteSegment(seg);
                    break;
                }
                if (SfKcpUtils.TimeDiff(sn, seg.Sn) < 0) break;
            }
        }

        void ParseUna(uint una)
        {
            int removed = 0;
            foreach (var seg in _sndBuf)
            {
                if (seg.Sn < una)
                {
                    ++removed;
                    DeleteSegment(seg);
                }
                else break;
            }
            _sndBuf.RemoveRange(0, removed);
        }

        void ParseFastack(uint sn, uint ts)
        {
            if (sn < _sndUna || sn >= _sndNxt) return;

            foreach (var seg in _sndBuf)
            {
                if (sn < seg.Sn) break;
                if (sn != seg.Sn) seg.FastAck++;
            }
        }

        void AckPush(uint sn, uint ts) => _ackList.Add(new SfKcpAckItem { Sn = sn, Ts = ts });

        void ParseData(SfKcpSegment newseg)
        {
            uint sn = newseg.Sn;
            if (SfKcpUtils.TimeDiff(sn, _rcvNxt + _rcvWnd) >= 0 ||
                SfKcpUtils.TimeDiff(sn, _rcvNxt) < 0)
            {
                DeleteSegment(newseg);
                return;
            }
            InsertSegmentInReceiveBuffer(newseg);
            MoveReceiveBufferReadySegmentsToQueue();
        }

        void InsertSegmentInReceiveBuffer(SfKcpSegment newseg)
        {
            bool repeat = false;
            int i;
            for (i = _rcvBuf.Count - 1; i >= 0; i--)
            {
                var seg = _rcvBuf[i];
                if (seg.Sn == newseg.Sn)
                {
                    repeat = true;
                    break;
                }
                if (SfKcpUtils.TimeDiff(newseg.Sn, seg.Sn) > 0) break;
            }

            if (!repeat) _rcvBuf.Insert(i + 1, newseg);
            else DeleteSegment(newseg);
        }

        void MoveReceiveBufferReadySegmentsToQueue()
        {
            int removed = 0;
            foreach (var seg in _rcvBuf)
            {
                if (seg.Sn == _rcvNxt && _rcvQueue.Count < _rcvWnd)
                {
                    ++removed;
                    _rcvQueue.Enqueue(seg);
                    _rcvNxt++;
                }
                else break;
            }
            _rcvBuf.RemoveRange(0, removed);
        }

        public int Input(byte[] data, int offset, int size)
        {
            uint prevUna = _sndUna;
            uint maxack = 0;
            uint latestTs = 0;
            int flag = 0;

            if (data == null || size < SfKcpConst.Overhead) return -1;

            while (true)
            {
                if (size < SfKcpConst.Overhead) break;

                var (seg, consumed) = SfKcpSegment.Decode(data, offset);
                if (seg == null || consumed <= 0) return -2;

                if (seg.Conv != _conv) return -1;

                offset += consumed;
                size -= consumed;

                if (seg.Cmd != SfKcpCmd.Push && seg.Cmd != SfKcpCmd.Ack &&
                    seg.Cmd != SfKcpCmd.Wask && seg.Cmd != SfKcpCmd.Wins)
                    return -3;

                _rmtWnd = seg.Wnd;
                ParseUna(seg.Una);
                ShrinkBuf();

                if (seg.Cmd == SfKcpCmd.Ack)
                {
                    if (SfKcpUtils.TimeDiff(_current, seg.Ts) >= 0)
                        UpdateAck(SfKcpUtils.TimeDiff(_current, seg.Ts));
                    ParseAck(seg.Sn);
                    ShrinkBuf();
                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = seg.Sn;
                        latestTs = seg.Ts;
                    }
                    else if (SfKcpUtils.TimeDiff(seg.Sn, maxack) > 0)
                    {
                        maxack = seg.Sn;
                        latestTs = seg.Ts;
                    }
                }
                else if (seg.Cmd == SfKcpCmd.Push)
                {
                    if (SfKcpUtils.TimeDiff(seg.Sn, _rcvNxt + _rcvWnd) < 0)
                    {
                        AckPush(seg.Sn, seg.Ts);
                        if (SfKcpUtils.TimeDiff(seg.Sn, _rcvNxt) >= 0)
                        {
                            var pushSeg = NewSegment();
                            pushSeg.Conv = seg.Conv;
                            pushSeg.Cmd = seg.Cmd;
                            pushSeg.Frg = seg.Frg;
                            pushSeg.Wnd = seg.Wnd;
                            pushSeg.Ts = seg.Ts;
                            pushSeg.Sn = seg.Sn;
                            pushSeg.Una = seg.Una;
                            int dataLen = seg.Data?.Length ?? 0;
                            if (dataLen > 0)
                            {
                                pushSeg.Data = new byte[dataLen];
                                Buffer.BlockCopy(seg.Data, 0, pushSeg.Data, 0, dataLen);
                            }
                            else
                            {
                                pushSeg.Data = Array.Empty<byte>();
                            }
                            ParseData(pushSeg);
                        }
                    }
                }
                else if (seg.Cmd == SfKcpCmd.Wask)
                {
                    _probe |= SfKcpConst.AskTell;
                }
            }

            if (flag != 0) ParseFastack(maxack, latestTs);

            if (SfKcpUtils.TimeDiff(_sndUna, prevUna) > 0)
            {
                if (_cwnd < _rmtWnd)
                {
                    if (_cwnd < _ssthresh)
                    {
                        _cwnd++;
                        _incr += _mss;
                    }
                    else
                    {
                        if (_incr < _mss) _incr = _mss;
                        _incr += (_mss * _mss) / _incr + (_mss / 16);
                        if ((_cwnd + 1) * _mss <= _incr)
                            _cwnd = (_incr + _mss - 1) / (_mss > 0 ? _mss : 1);
                    }
                    if (_cwnd > _rmtWnd)
                    {
                        _cwnd = _rmtWnd;
                        _incr = _rmtWnd * _mss;
                    }
                }
            }
            return 0;
        }

        public int Input(byte[] data) => Input(data, 0, data?.Length ?? 0);

        void MakeSpace(ref int size, int space)
        {
            if (size + space > _mtu)
            {
                _output(_buffer, size);
                size = 0;
            }
        }

        void FlushBuffer(int size)
        {
            if (size > 0) _output(_buffer, size);
        }

        public void Flush()
        {
            if (!_updated) return;

            int size = 0;
            bool lost = false;

            var seg = NewSegment();
            seg.Conv = _conv;
            seg.Cmd = SfKcpCmd.Ack;
            seg.Wnd = (ushort)WndUnused();
            seg.Una = _rcvNxt;

            foreach (var ack in _ackList)
            {
                MakeSpace(ref size, SfKcpConst.Overhead);
                seg.Sn = ack.Sn;
                seg.Ts = ack.Ts;
                size += seg.Encode(_buffer, size);
            }
            _ackList.Clear();

            if (_rmtWnd == 0)
            {
                if (_probeWait == 0)
                {
                    _probeWait = SfKcpConst.ProbeInit;
                    _tsProbe = _current + _probeWait;
                }
                else if (SfKcpUtils.TimeDiff(_current, _tsProbe) >= 0)
                {
                    if (_probeWait < SfKcpConst.ProbeInit) _probeWait = SfKcpConst.ProbeInit;
                    _probeWait += _probeWait / 2;
                    if (_probeWait > SfKcpConst.ProbeLimit) _probeWait = SfKcpConst.ProbeLimit;
                    _tsProbe = _current + _probeWait;
                    _probe |= SfKcpConst.AskSend;
                }
            }
            else
            {
                _tsProbe = 0;
                _probeWait = 0;
            }

            if ((_probe & SfKcpConst.AskSend) != 0)
            {
                seg.Cmd = SfKcpCmd.Wask;
                MakeSpace(ref size, SfKcpConst.Overhead);
                size += seg.Encode(_buffer, size);
            }

            if ((_probe & SfKcpConst.AskTell) != 0)
            {
                seg.Cmd = SfKcpCmd.Wins;
                MakeSpace(ref size, SfKcpConst.Overhead);
                size += seg.Encode(_buffer, size);
            }

            _probe = 0;

            uint cwnd_ = Math.Min(_sndWnd, _rmtWnd);
            if (!_noCwnd) cwnd_ = Math.Min(_cwnd, cwnd_);

            while (SfKcpUtils.TimeDiff(_sndNxt, _sndUna + cwnd_) < 0)
            {
                if (_sndQueue.Count == 0) break;
                var newseg = _sndQueue.Dequeue();
                newseg.Conv = _conv;
                newseg.Cmd = SfKcpCmd.Push;
                newseg.Wnd = seg.Wnd;
                newseg.Ts = _current;
                newseg.Sn = _sndNxt++;
                newseg.Una = _rcvNxt;
                newseg.Resendts = _current;
                newseg.Rto = (uint)_rxRto;
                newseg.FastAck = 0;
                newseg.Xmit = 0;
                _sndBuf.Add(newseg);
            }

            uint resent = _fastResend > 0 ? (uint)_fastResend : 0xffffffff;
            uint rtomin = _nodelay == 0 ? (uint)_rxRto >> 3 : 0;

            int change = 0;
            foreach (var segment in _sndBuf)
            {
                bool needsend = false;

                if (segment.Xmit == 0)
                {
                    needsend = true;
                    segment.Xmit++;
                    segment.Rto = (uint)_rxRto;
                    segment.Resendts = _current + segment.Rto + rtomin;
                }
                else if (SfKcpUtils.TimeDiff(_current, segment.Resendts) >= 0)
                {
                    needsend = true;
                    segment.Xmit++;
                    _xmit++;
                    if (_nodelay == 0)
                        segment.Rto += Math.Max(segment.Rto, (uint)_rxRto);
                    else
                    {
                        int step = _nodelay < 2 ? (int)segment.Rto : _rxRto;
                        segment.Rto += (uint)(step / 2);
                    }
                    segment.Resendts = _current + segment.Rto;
                    lost = true;
                }
                else if (segment.FastAck >= resent)
                {
                    if (segment.Xmit <= _fastLimit || _fastLimit <= 0)
                    {
                        needsend = true;
                        segment.Xmit++;
                        segment.FastAck = 0;
                        segment.Resendts = _current + segment.Rto;
                        change++;
                    }
                }

                if (needsend)
                {
                    segment.Ts = _current;
                    segment.Wnd = seg.Wnd;
                    segment.Una = _rcvNxt;
                    int need = SfKcpConst.Overhead + (segment.Data?.Length ?? 0);
                    MakeSpace(ref size, need);
                    size += segment.Encode(_buffer, size);

                    if (segment.Xmit >= _deadLink)
                        _state = -1;
                }
            }

            DeleteSegment(seg);
            FlushBuffer(size);

            if (change > 0)
            {
                uint inflight = _sndNxt - _sndUna;
                _ssthresh = inflight / 2;
                if (_ssthresh < SfKcpConst.ThreshMin) _ssthresh = SfKcpConst.ThreshMin;
                _cwnd = _ssthresh + resent;
                _incr = _cwnd * _mss;
            }

            if (lost)
            {
                _ssthresh = cwnd_ / 2;
                if (_ssthresh < SfKcpConst.ThreshMin) _ssthresh = SfKcpConst.ThreshMin;
                _cwnd = 1;
                _incr = _mss;
            }

            if (_cwnd < 1)
            {
                _cwnd = 1;
                _incr = _mss;
            }
        }

        public void Update(uint currentTimeMilliSeconds)
        {
            _current = currentTimeMilliSeconds;

            if (!_updated)
            {
                _updated = true;
                _tsFlush = _current;
            }

            int slap = SfKcpUtils.TimeDiff(_current, _tsFlush);
            if (slap >= 10000 || slap < -10000)
            {
                _tsFlush = _current;
                slap = 0;
            }

            if (slap >= 0)
            {
                _tsFlush += _interval;
                if (_current >= _tsFlush)
                    _tsFlush = _current + _interval;
                Flush();
            }
        }

        public uint Check(uint current)
        {
            uint tsFlush = _tsFlush;
            int tmPacket = 0x7fffffff;

            if (!_updated) return current;

            if (SfKcpUtils.TimeDiff(current, tsFlush) >= 10000 ||
                SfKcpUtils.TimeDiff(current, tsFlush) < -10000)
                tsFlush = current;

            if (SfKcpUtils.TimeDiff(current, tsFlush) >= 0)
                return current;

            int tmFlush = SfKcpUtils.TimeDiff(tsFlush, current);

            foreach (var seg in _sndBuf)
            {
                int diff = SfKcpUtils.TimeDiff(seg.Resendts, current);
                if (diff <= 0) return current;
                if (diff < tmPacket) tmPacket = diff;
            }

            uint minimal = (uint)(tmPacket < tmFlush ? tmPacket : tmFlush);
            if (minimal >= _interval) minimal = _interval;
            return current + minimal;
        }

        public void SetMtu(uint mtu)
        {
            if (mtu < 50 || mtu < SfKcpConst.Overhead)
                throw new ArgumentException("MTU 必须大于 50 且大于 OVERHEAD");
            _buffer = new byte[(mtu + SfKcpConst.Overhead) * 3];
            _mtu = mtu;
            _mss = mtu - SfKcpConst.Overhead;
        }

        public void SetInterval(uint interval)
        {
            if (interval > 5000) interval = 5000;
            else if (interval < 10) interval = 10;
            _interval = interval;
        }

        /// <summary>
        /// nodelay: 0 普通模式, 1 快速模式; resend: 快速重传次数; noCwnd: 关闭拥塞控制
        /// </summary>
        public void SetNoDelay(uint nodelay, uint interval = SfKcpConst.Interval, int resend = 0, bool noCwnd = false)
        {
            _nodelay = nodelay;
            _rxMinRto = nodelay != 0 ? SfKcpConst.RtoNdl : SfKcpConst.RtoMin;

            if (interval >= 0)
            {
                if (interval > 5000) interval = 5000;
                else if (interval < 10) interval = 10;
                _interval = interval;
            }

            if (resend >= 0) _fastResend = resend;
            _noCwnd = noCwnd;
        }

        public void SetWindowSize(uint sendWindow, uint receiveWindow)
        {
            if (sendWindow > 0) _sndWnd = sendWindow;
            if (receiveWindow > 0)
                _rcvWnd = Math.Max(receiveWindow, SfKcpConst.WndRcv);
        }
    }
}
