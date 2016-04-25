
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Server.Kestrel.Http 
{

    public partial class FrameRequestHeaders
    {
        
        private long _bits = 0;
        private HeaderReferences _headers;
        
        public StringValues HeaderDate
        {
            get
            {
                if (((_bits & 1L) != 0))
                {
                    return _headers._Date;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 1L;
                _headers._Date = value; 
            }
        }
        public StringValues HeaderContentLength
        {
            get
            {
                if (((_bits & 2L) != 0))
                {
                    return _headers._ContentLength;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 2L;
                _headers._ContentLength = value; 
            }
        }
        public StringValues HeaderContentType
        {
            get
            {
                if (((_bits & 4L) != 0))
                {
                    return _headers._ContentType;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 4L;
                _headers._ContentType = value; 
            }
        }
        public StringValues HeaderHost
        {
            get
            {
                if (((_bits & 8L) != 0))
                {
                    return _headers._Host;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 8L;
                _headers._Host = value; 
            }
        }
        
        protected override int GetCountFast()
        {
            return BitCount(_bits) + (MaybeUnknown?.Count ?? 0);
        }
        protected override StringValues GetValueFast(string key)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                return _headers._Date;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Host".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                return _headers._Host;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                return _headers._ContentLength;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
                            {
                                return _headers._ContentType;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;
}
            if (MaybeUnknown == null) 
            {
                ThrowKeyNotFoundException();
            }
            return MaybeUnknown[key];
        }
        protected override bool TryGetValueFast(string key, out StringValues value)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                value = _headers._Date;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Host".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                value = _headers._Host;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                value = _headers._ContentLength;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
                            {
                                value = _headers._ContentType;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    }
                    break;
}
            value = StringValues.Empty;
            return MaybeUnknown?.TryGetValue(key, out value) ?? false;
        }
        protected override void SetValueFast(string key, StringValues value)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 1L;
                            _headers._Date = value;
                            return;
                        }
                    
                        if ("Host".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 8L;
                            _headers._Host = value;
                            return;
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 2L;
                            _headers._ContentLength = value;
                            return;
                        }
                    }
                    break;

                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 4L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;
}
            Unknown[key] = value;
        }
        protected override void AddValueFast(string key, StringValues value)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 1L;
                            _headers._Date = value;
                            return;
                        }
                    
                        if ("Host".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 8L;
                            _headers._Host = value;
                            return;
                        }
                    }
                    break;
            
                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 2L;
                            _headers._ContentLength = value;
                            return;
                        }
                    }
                    break;
            
                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 4L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;
            }
            Unknown.Add(key, value);
        }
        protected override bool RemoveFast(string key)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                _bits &= ~1L;
                                _headers._Date = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Host".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                _bits &= ~8L;
                                _headers._Host = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                _bits &= ~2L;
                                _headers._ContentLength = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
                            {
                                _bits &= ~4L;
                                _headers._ContentType = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            }
            return MaybeUnknown?.Remove(key) ?? false;
        }
        protected override void ClearFast()
        {
            _bits = 0;
            _headers = default(HeaderReferences);
            MaybeUnknown?.Clear();
        }
        
        protected override void CopyToFast(KeyValuePair<string, StringValues>[] array, int arrayIndex)
        {
            if (arrayIndex < 0)
            {
                ThrowArgumentException();
            }
            
                if (((_bits & 1L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Date", _headers._Date);
                    ++arrayIndex;
                }
            
                if (((_bits & 2L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Length", _headers._ContentLength);
                    ++arrayIndex;
                }
            
                if (((_bits & 4L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Type", _headers._ContentType);
                    ++arrayIndex;
                }
            
                if (((_bits & 8L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Host", _headers._Host);
                    ++arrayIndex;
                }
            
            ((ICollection<KeyValuePair<string, StringValues>>)MaybeUnknown)?.CopyTo(array, arrayIndex);
        }
        
        
        public unsafe void Append(byte[] keyBytes, int keyOffset, int keyLength, string value)
        {
            fixed (byte* ptr = &keyBytes[keyOffset]) 
            { 
                var pUB = ptr; 
                var pUL = (ulong*)pUB; 
                var pUI = (uint*)pUB; 
                var pUS = (ushort*)pUB;
                switch (keyLength)
                {
                    case 4:
                        {
                            if ((((pUI[0] & 3755991007u) == 1163149636u))) 
                            {
                                if (((_bits & 1L) != 0))
                                {
                                    _headers._Date = AppendValue(_headers._Date, value);
                                }
                                else
                                {
                                    _bits |= 1L;
                                    _headers._Date = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUI[0] & 3755991007u) == 1414745928u))) 
                            {
                                if (((_bits & 8L) != 0))
                                {
                                    _headers._Host = AppendValue(_headers._Host, value);
                                }
                                else
                                {
                                    _bits |= 8L;
                                    _headers._Host = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 14:
                        {
                            if ((((pUL[0] & 18437701552104792031uL) == 3266321689424580419uL) && ((pUI[2] & 3755991007u) == 1196311884u) && ((pUS[6] & 57311u) == 18516u))) 
                            {
                                if (((_bits & 2L) != 0))
                                {
                                    _headers._ContentLength = AppendValue(_headers._ContentLength, value);
                                }
                                else
                                {
                                    _bits |= 2L;
                                    _headers._ContentLength = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 12:
                        {
                            if ((((pUL[0] & 18437701552104792031uL) == 3266321689424580419uL) && ((pUI[2] & 3755991007u) == 1162893652u))) 
                            {
                                if (((_bits & 4L) != 0))
                                {
                                    _headers._ContentType = AppendValue(_headers._ContentType, value);
                                }
                                else
                                {
                                    _bits |= 4L;
                                    _headers._ContentType = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                }
            }
            var key = System.Text.Encoding.ASCII.GetString(keyBytes, keyOffset, keyLength);
            StringValues existing;
            Unknown.TryGetValue(key, out existing);
            Unknown[key] = AppendValue(existing, value);
        }
        private struct HeaderReferences
        {
            public StringValues _Date;
            public StringValues _ContentLength;
            public StringValues _ContentType;
            public StringValues _Host;
            
        }

        public partial struct Enumerator
        {
            public bool MoveNext()
            {
                switch (_state)
                {
                    
                        case 0:
                            goto state0;
                    
                        case 1:
                            goto state1;
                    
                        case 2:
                            goto state2;
                    
                        case 3:
                            goto state3;
                    
                    default:
                        goto state_default;
                }
                
                state0:
                    if (((_bits & 1L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Date", _collection._headers._Date);
                        _state = 1;
                        return true;
                    }
                
                state1:
                    if (((_bits & 2L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Length", _collection._headers._ContentLength);
                        _state = 2;
                        return true;
                    }
                
                state2:
                    if (((_bits & 4L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Type", _collection._headers._ContentType);
                        _state = 3;
                        return true;
                    }
                
                state3:
                    if (((_bits & 8L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Host", _collection._headers._Host);
                        _state = 4;
                        return true;
                    }
                
                state_default:
                    if (!_hasUnknown || !_unknownEnumerator.MoveNext())
                    {
                        _current = default(KeyValuePair<string, StringValues>);
                        return false;
                    }
                    _current = _unknownEnumerator.Current;
                    return true;
            }
        }
    }

    public partial class FrameResponseHeaders
    {
        private static byte[] _headerBytes = new byte[]
        {
            13,10,68,97,116,101,58,32,13,10,67,111,110,116,101,110,116,45,76,101,110,103,116,104,58,32,13,10,67,111,110,116,101,110,116,45,84,121,112,101,58,32,13,10,83,101,114,118,101,114,58,32,
        };
        
        private long _bits = 0;
        private HeaderReferences _headers;
        
        public StringValues HeaderDate
        {
            get
            {
                if (((_bits & 1L) != 0))
                {
                    return _headers._Date;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 1L;
                _headers._Date = value; 
                _headers._rawDate = null;
            }
        }
        public StringValues HeaderContentLength
        {
            get
            {
                if (((_bits & 2L) != 0))
                {
                    return _headers._ContentLength;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 2L;
                _headers._ContentLength = value; 
                _headers._rawContentLength = null;
            }
        }
        public StringValues HeaderContentType
        {
            get
            {
                if (((_bits & 4L) != 0))
                {
                    return _headers._ContentType;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 4L;
                _headers._ContentType = value; 
            }
        }
        public StringValues HeaderServer
        {
            get
            {
                if (((_bits & 8L) != 0))
                {
                    return _headers._Server;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 8L;
                _headers._Server = value; 
                _headers._rawServer = null;
            }
        }
        
        public void SetRawDate(StringValues value, byte[] raw)
        {
            _bits |= 1L;
            _headers._Date = value; 
            _headers._rawDate = raw;
        }
        public void SetRawContentLength(StringValues value, byte[] raw)
        {
            _bits |= 2L;
            _headers._ContentLength = value; 
            _headers._rawContentLength = raw;
        }
        public void SetRawServer(StringValues value, byte[] raw)
        {
            _bits |= 8L;
            _headers._Server = value; 
            _headers._rawServer = raw;
        }
        protected override int GetCountFast()
        {
            return BitCount(_bits) + (MaybeUnknown?.Count ?? 0);
        }
        protected override StringValues GetValueFast(string key)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                return _headers._Date;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                return _headers._ContentLength;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
                            {
                                return _headers._ContentType;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 6:
                    {
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                return _headers._Server;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;
}
            if (MaybeUnknown == null) 
            {
                ThrowKeyNotFoundException();
            }
            return MaybeUnknown[key];
        }
        protected override bool TryGetValueFast(string key, out StringValues value)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                value = _headers._Date;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                value = _headers._ContentLength;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
                            {
                                value = _headers._ContentType;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    }
                    break;

                case 6:
                    {
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                value = _headers._Server;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    }
                    break;
}
            value = StringValues.Empty;
            return MaybeUnknown?.TryGetValue(key, out value) ?? false;
        }
        protected override void SetValueFast(string key, StringValues value)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 1L;
                            _headers._Date = value;
                            _headers._rawDate = null;
                            return;
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 2L;
                            _headers._ContentLength = value;
                            _headers._rawContentLength = null;
                            return;
                        }
                    }
                    break;

                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 4L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;

                case 6:
                    {
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 8L;
                            _headers._Server = value;
                            _headers._rawServer = null;
                            return;
                        }
                    }
                    break;
}
            Unknown[key] = value;
        }
        protected override void AddValueFast(string key, StringValues value)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 1L;
                            _headers._Date = value;
                            _headers._rawDate = null;
                            return;
                        }
                    }
                    break;
            
                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 2L;
                            _headers._ContentLength = value;
                            _headers._rawContentLength = null;
                            return;
                        }
                    }
                    break;
            
                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 4L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;
            
                case 6:
                    {
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 8L;
                            _headers._Server = value;
                            _headers._rawServer = null;
                            return;
                        }
                    }
                    break;
            }
            Unknown.Add(key, value);
        }
        protected override bool RemoveFast(string key)
        {
            switch (key.Length)
            {
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                _bits &= ~1L;
                                _headers._Date = StringValues.Empty;
                                _headers._rawDate = null;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                _bits &= ~2L;
                                _headers._ContentLength = StringValues.Empty;
                                _headers._rawContentLength = null;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
                            {
                                _bits &= ~4L;
                                _headers._ContentType = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 6:
                    {
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                _bits &= ~8L;
                                _headers._Server = StringValues.Empty;
                                _headers._rawServer = null;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            }
            return MaybeUnknown?.Remove(key) ?? false;
        }
        protected override void ClearFast()
        {
            _bits = 0;
            _headers = default(HeaderReferences);
            MaybeUnknown?.Clear();
        }
        
        protected override void CopyToFast(KeyValuePair<string, StringValues>[] array, int arrayIndex)
        {
            if (arrayIndex < 0)
            {
                ThrowArgumentException();
            }
            
                if (((_bits & 1L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Date", _headers._Date);
                    ++arrayIndex;
                }
            
                if (((_bits & 2L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Length", _headers._ContentLength);
                    ++arrayIndex;
                }
            
                if (((_bits & 4L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Type", _headers._ContentType);
                    ++arrayIndex;
                }
            
                if (((_bits & 8L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Server", _headers._Server);
                    ++arrayIndex;
                }
            
            ((ICollection<KeyValuePair<string, StringValues>>)MaybeUnknown)?.CopyTo(array, arrayIndex);
        }
        
        protected void CopyToFast(ref MemoryPoolIterator output)
        {
            
                if (((_bits & 1L) != 0)) 
                { 
                    if (_headers._rawDate != null) 
                    {
                        output.CopyFrom(_headers._rawDate, 0, _headers._rawDate.Length);
                    } 
                    else 
                        foreach (var value in _headers._Date)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 0, 8);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 2L) != 0)) 
                { 
                    if (_headers._rawContentLength != null) 
                    {
                        output.CopyFrom(_headers._rawContentLength, 0, _headers._rawContentLength.Length);
                    } 
                    else 
                        foreach (var value in _headers._ContentLength)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 8, 18);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 4L) != 0)) 
                { 
                        foreach (var value in _headers._ContentType)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 26, 16);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 8L) != 0)) 
                { 
                    if (_headers._rawServer != null) 
                    {
                        output.CopyFrom(_headers._rawServer, 0, _headers._rawServer.Length);
                    } 
                    else 
                        foreach (var value in _headers._Server)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 42, 10);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
        }
        
        private struct HeaderReferences
        {
            public StringValues _Date;
            public StringValues _ContentLength;
            public StringValues _ContentType;
            public StringValues _Server;
            
            public byte[] _rawDate;
            public byte[] _rawContentLength;
            public byte[] _rawServer;
        }

        public partial struct Enumerator
        {
            public bool MoveNext()
            {
                switch (_state)
                {
                    
                        case 0:
                            goto state0;
                    
                        case 1:
                            goto state1;
                    
                        case 2:
                            goto state2;
                    
                        case 3:
                            goto state3;
                    
                    default:
                        goto state_default;
                }
                
                state0:
                    if (((_bits & 1L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Date", _collection._headers._Date);
                        _state = 1;
                        return true;
                    }
                
                state1:
                    if (((_bits & 2L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Length", _collection._headers._ContentLength);
                        _state = 2;
                        return true;
                    }
                
                state2:
                    if (((_bits & 4L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Type", _collection._headers._ContentType);
                        _state = 3;
                        return true;
                    }
                
                state3:
                    if (((_bits & 8L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Server", _collection._headers._Server);
                        _state = 4;
                        return true;
                    }
                
                state_default:
                    if (!_hasUnknown || !_unknownEnumerator.MoveNext())
                    {
                        _current = default(KeyValuePair<string, StringValues>);
                        return false;
                    }
                    _current = _unknownEnumerator.Current;
                    return true;
            }
        }
    }
}