
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
        
        public StringValues HeaderCacheControl
        {
            get
            {
                if (((_bits & 1L) != 0))
                {
                    return _headers._CacheControl;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 1L;
                _headers._CacheControl = value; 
            }
        }
        public StringValues HeaderConnection
        {
            get
            {
                if (((_bits & 2L) != 0))
                {
                    return _headers._Connection;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 2L;
                _headers._Connection = value; 
            }
        }
        public StringValues HeaderDate
        {
            get
            {
                if (((_bits & 4L) != 0))
                {
                    return _headers._Date;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 4L;
                _headers._Date = value; 
            }
        }
        public StringValues HeaderKeepAlive
        {
            get
            {
                if (((_bits & 8L) != 0))
                {
                    return _headers._KeepAlive;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 8L;
                _headers._KeepAlive = value; 
            }
        }
        public StringValues HeaderPragma
        {
            get
            {
                if (((_bits & 16L) != 0))
                {
                    return _headers._Pragma;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 16L;
                _headers._Pragma = value; 
            }
        }
        public StringValues HeaderTransferEncoding
        {
            get
            {
                if (((_bits & 32L) != 0))
                {
                    return _headers._TransferEncoding;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 32L;
                _headers._TransferEncoding = value; 
            }
        }
        public StringValues HeaderUpgrade
        {
            get
            {
                if (((_bits & 64L) != 0))
                {
                    return _headers._Upgrade;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 64L;
                _headers._Upgrade = value; 
            }
        }
        public StringValues HeaderContentLength
        {
            get
            {
                if (((_bits & 128L) != 0))
                {
                    return _headers._ContentLength;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 128L;
                _headers._ContentLength = value; 
            }
        }
        public StringValues HeaderContentType
        {
            get
            {
                if (((_bits & 256L) != 0))
                {
                    return _headers._ContentType;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 256L;
                _headers._ContentType = value; 
            }
        }
        public StringValues HeaderContentEncoding
        {
            get
            {
                if (((_bits & 512L) != 0))
                {
                    return _headers._ContentEncoding;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 512L;
                _headers._ContentEncoding = value; 
            }
        }
        public StringValues HeaderAccept
        {
            get
            {
                if (((_bits & 1024L) != 0))
                {
                    return _headers._Accept;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 1024L;
                _headers._Accept = value; 
            }
        }
        public StringValues HeaderAuthorization
        {
            get
            {
                if (((_bits & 2048L) != 0))
                {
                    return _headers._Authorization;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 2048L;
                _headers._Authorization = value; 
            }
        }
        public StringValues HeaderCookie
        {
            get
            {
                if (((_bits & 4096L) != 0))
                {
                    return _headers._Cookie;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 4096L;
                _headers._Cookie = value; 
            }
        }
        public StringValues HeaderExpect
        {
            get
            {
                if (((_bits & 8192L) != 0))
                {
                    return _headers._Expect;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 8192L;
                _headers._Expect = value; 
            }
        }
        public StringValues HeaderHost
        {
            get
            {
                if (((_bits & 16384L) != 0))
                {
                    return _headers._Host;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 16384L;
                _headers._Host = value; 
            }
        }
        public StringValues HeaderUserAgent
        {
            get
            {
                if (((_bits & 32768L) != 0))
                {
                    return _headers._UserAgent;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 32768L;
                _headers._UserAgent = value; 
            }
        }
        public StringValues HeaderOrigin
        {
            get
            {
                if (((_bits & 65536L) != 0))
                {
                    return _headers._Origin;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 65536L;
                _headers._Origin = value; 
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                return _headers._CacheControl;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Authorization".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2048L) != 0))
                            {
                                return _headers._Authorization;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                return _headers._Connection;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                return _headers._KeepAlive;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("User-Agent".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 32768L) != 0))
                            {
                                return _headers._UserAgent;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
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
                            if (((_bits & 16384L) != 0))
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

                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 16L) != 0))
                            {
                                return _headers._Pragma;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Accept".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                return _headers._Accept;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Cookie".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                return _headers._Cookie;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Expect".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                return _headers._Expect;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Origin".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 65536L) != 0))
                            {
                                return _headers._Origin;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 32L) != 0))
                            {
                                return _headers._TransferEncoding;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 64L) != 0))
                            {
                                return _headers._Upgrade;
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
                            if (((_bits & 128L) != 0))
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
                            if (((_bits & 256L) != 0))
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

                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 512L) != 0))
                            {
                                return _headers._ContentEncoding;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                value = _headers._CacheControl;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Authorization".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2048L) != 0))
                            {
                                value = _headers._Authorization;
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

                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                value = _headers._Connection;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                value = _headers._KeepAlive;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("User-Agent".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 32768L) != 0))
                            {
                                value = _headers._UserAgent;
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

                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
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
                            if (((_bits & 16384L) != 0))
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

                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 16L) != 0))
                            {
                                value = _headers._Pragma;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Accept".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                value = _headers._Accept;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Cookie".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                value = _headers._Cookie;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Expect".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                value = _headers._Expect;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Origin".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 65536L) != 0))
                            {
                                value = _headers._Origin;
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

                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 32L) != 0))
                            {
                                value = _headers._TransferEncoding;
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

                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 64L) != 0))
                            {
                                value = _headers._Upgrade;
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
                            if (((_bits & 128L) != 0))
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
                            if (((_bits & 256L) != 0))
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

                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 512L) != 0))
                            {
                                value = _headers._ContentEncoding;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 1L;
                            _headers._CacheControl = value;
                            return;
                        }
                    
                        if ("Authorization".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 2048L;
                            _headers._Authorization = value;
                            return;
                        }
                    }
                    break;

                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 2L;
                            _headers._Connection = value;
                            return;
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 8L;
                            _headers._KeepAlive = value;
                            return;
                        }
                    
                        if ("User-Agent".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 32768L;
                            _headers._UserAgent = value;
                            return;
                        }
                    }
                    break;

                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 4L;
                            _headers._Date = value;
                            return;
                        }
                    
                        if ("Host".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 16384L;
                            _headers._Host = value;
                            return;
                        }
                    }
                    break;

                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 16L;
                            _headers._Pragma = value;
                            return;
                        }
                    
                        if ("Accept".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 1024L;
                            _headers._Accept = value;
                            return;
                        }
                    
                        if ("Cookie".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 4096L;
                            _headers._Cookie = value;
                            return;
                        }
                    
                        if ("Expect".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 8192L;
                            _headers._Expect = value;
                            return;
                        }
                    
                        if ("Origin".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 65536L;
                            _headers._Origin = value;
                            return;
                        }
                    }
                    break;

                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 32L;
                            _headers._TransferEncoding = value;
                            return;
                        }
                    }
                    break;

                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 64L;
                            _headers._Upgrade = value;
                            return;
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 128L;
                            _headers._ContentLength = value;
                            return;
                        }
                    }
                    break;

                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 256L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;

                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 512L;
                            _headers._ContentEncoding = value;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 1L;
                            _headers._CacheControl = value;
                            return;
                        }
                    
                        if ("Authorization".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2048L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 2048L;
                            _headers._Authorization = value;
                            return;
                        }
                    }
                    break;
            
                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 2L;
                            _headers._Connection = value;
                            return;
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 8L;
                            _headers._KeepAlive = value;
                            return;
                        }
                    
                        if ("User-Agent".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 32768L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 32768L;
                            _headers._UserAgent = value;
                            return;
                        }
                    }
                    break;
            
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 4L;
                            _headers._Date = value;
                            return;
                        }
                    
                        if ("Host".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 16384L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 16384L;
                            _headers._Host = value;
                            return;
                        }
                    }
                    break;
            
                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 16L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 16L;
                            _headers._Pragma = value;
                            return;
                        }
                    
                        if ("Accept".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 1024L;
                            _headers._Accept = value;
                            return;
                        }
                    
                        if ("Cookie".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 4096L;
                            _headers._Cookie = value;
                            return;
                        }
                    
                        if ("Expect".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 8192L;
                            _headers._Expect = value;
                            return;
                        }
                    
                        if ("Origin".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 65536L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 65536L;
                            _headers._Origin = value;
                            return;
                        }
                    }
                    break;
            
                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 32L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 32L;
                            _headers._TransferEncoding = value;
                            return;
                        }
                    }
                    break;
            
                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 64L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 64L;
                            _headers._Upgrade = value;
                            return;
                        }
                    }
                    break;
            
                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 128L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 128L;
                            _headers._ContentLength = value;
                            return;
                        }
                    }
                    break;
            
                case 12:
                    {
                        if ("Content-Type".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 256L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 256L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;
            
                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 512L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 512L;
                            _headers._ContentEncoding = value;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                _bits &= ~1L;
                                _headers._CacheControl = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Authorization".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2048L) != 0))
                            {
                                _bits &= ~2048L;
                                _headers._Authorization = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                _bits &= ~2L;
                                _headers._Connection = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                _bits &= ~8L;
                                _headers._KeepAlive = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("User-Agent".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 32768L) != 0))
                            {
                                _bits &= ~32768L;
                                _headers._UserAgent = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
                            {
                                _bits &= ~4L;
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
                            if (((_bits & 16384L) != 0))
                            {
                                _bits &= ~16384L;
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
            
                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 16L) != 0))
                            {
                                _bits &= ~16L;
                                _headers._Pragma = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Accept".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                _bits &= ~1024L;
                                _headers._Accept = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Cookie".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                _bits &= ~4096L;
                                _headers._Cookie = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Expect".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                _bits &= ~8192L;
                                _headers._Expect = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Origin".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 65536L) != 0))
                            {
                                _bits &= ~65536L;
                                _headers._Origin = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 32L) != 0))
                            {
                                _bits &= ~32L;
                                _headers._TransferEncoding = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 64L) != 0))
                            {
                                _bits &= ~64L;
                                _headers._Upgrade = StringValues.Empty;
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
                            if (((_bits & 128L) != 0))
                            {
                                _bits &= ~128L;
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
                            if (((_bits & 256L) != 0))
                            {
                                _bits &= ~256L;
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
            
                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 512L) != 0))
                            {
                                _bits &= ~512L;
                                _headers._ContentEncoding = StringValues.Empty;
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

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Cache-Control", _headers._CacheControl);
                    ++arrayIndex;
                }
            
                if (((_bits & 2L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Connection", _headers._Connection);
                    ++arrayIndex;
                }
            
                if (((_bits & 4L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Date", _headers._Date);
                    ++arrayIndex;
                }
            
                if (((_bits & 8L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Keep-Alive", _headers._KeepAlive);
                    ++arrayIndex;
                }
            
                if (((_bits & 16L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Pragma", _headers._Pragma);
                    ++arrayIndex;
                }
            
                if (((_bits & 32L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Transfer-Encoding", _headers._TransferEncoding);
                    ++arrayIndex;
                }
            
                if (((_bits & 64L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Upgrade", _headers._Upgrade);
                    ++arrayIndex;
                }
            
                if (((_bits & 128L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Length", _headers._ContentLength);
                    ++arrayIndex;
                }
            
                if (((_bits & 256L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Type", _headers._ContentType);
                    ++arrayIndex;
                }
            
                if (((_bits & 512L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Encoding", _headers._ContentEncoding);
                    ++arrayIndex;
                }
            
                if (((_bits & 1024L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Accept", _headers._Accept);
                    ++arrayIndex;
                }
            
                if (((_bits & 2048L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Authorization", _headers._Authorization);
                    ++arrayIndex;
                }
            
                if (((_bits & 4096L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Cookie", _headers._Cookie);
                    ++arrayIndex;
                }
            
                if (((_bits & 8192L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Expect", _headers._Expect);
                    ++arrayIndex;
                }
            
                if (((_bits & 16384L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Host", _headers._Host);
                    ++arrayIndex;
                }
            
                if (((_bits & 32768L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("User-Agent", _headers._UserAgent);
                    ++arrayIndex;
                }
            
                if (((_bits & 65536L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Origin", _headers._Origin);
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
                    case 13:
                        {
                            if ((((pUL[0] & 16131893727263186911uL) == 5711458528024281411uL) && ((pUI[2] & 3755991007u) == 1330795598u) && ((pUB[12] & 223u) == 76u))) 
                            {
                                if (((_bits & 1L) != 0))
                                {
                                    _headers._CacheControl = AppendValue(_headers._CacheControl, value);
                                }
                                else
                                {
                                    _bits |= 1L;
                                    _headers._CacheControl = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUL[0] & 16131858542891098079uL) == 6505821637182772545uL) && ((pUI[2] & 3755991007u) == 1330205761u) && ((pUB[12] & 223u) == 78u))) 
                            {
                                if (((_bits & 2048L) != 0))
                                {
                                    _headers._Authorization = AppendValue(_headers._Authorization, value);
                                }
                                else
                                {
                                    _bits |= 2048L;
                                    _headers._Authorization = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 10:
                        {
                            if ((((pUL[0] & 16131858542891098079uL) == 5283922227757993795uL) && ((pUS[4] & 57311u) == 20047u))) 
                            {
                                if (((_bits & 2L) != 0))
                                {
                                    _headers._Connection = AppendValue(_headers._Connection, value);
                                }
                                else
                                {
                                    _bits |= 2L;
                                    _headers._Connection = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUL[0] & 16131858680330051551uL) == 5281668125874799947uL) && ((pUS[4] & 57311u) == 17750u))) 
                            {
                                if (((_bits & 8L) != 0))
                                {
                                    _headers._KeepAlive = AppendValue(_headers._KeepAlive, value);
                                }
                                else
                                {
                                    _bits |= 8L;
                                    _headers._KeepAlive = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUL[0] & 16131858680330051551uL) == 4992030374873092949uL) && ((pUS[4] & 57311u) == 21582u))) 
                            {
                                if (((_bits & 32768L) != 0))
                                {
                                    _headers._UserAgent = AppendValue(_headers._UserAgent, value);
                                }
                                else
                                {
                                    _bits |= 32768L;
                                    _headers._UserAgent = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 4:
                        {
                            if ((((pUI[0] & 3755991007u) == 1163149636u))) 
                            {
                                if (((_bits & 4L) != 0))
                                {
                                    _headers._Date = AppendValue(_headers._Date, value);
                                }
                                else
                                {
                                    _bits |= 4L;
                                    _headers._Date = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUI[0] & 3755991007u) == 1414745928u))) 
                            {
                                if (((_bits & 16384L) != 0))
                                {
                                    _headers._Host = AppendValue(_headers._Host, value);
                                }
                                else
                                {
                                    _bits |= 16384L;
                                    _headers._Host = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 6:
                        {
                            if ((((pUI[0] & 3755991007u) == 1195463248u) && ((pUS[2] & 57311u) == 16717u))) 
                            {
                                if (((_bits & 16L) != 0))
                                {
                                    _headers._Pragma = AppendValue(_headers._Pragma, value);
                                }
                                else
                                {
                                    _bits |= 16L;
                                    _headers._Pragma = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUI[0] & 3755991007u) == 1162036033u) && ((pUS[2] & 57311u) == 21584u))) 
                            {
                                if (((_bits & 1024L) != 0))
                                {
                                    _headers._Accept = AppendValue(_headers._Accept, value);
                                }
                                else
                                {
                                    _bits |= 1024L;
                                    _headers._Accept = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUI[0] & 3755991007u) == 1263488835u) && ((pUS[2] & 57311u) == 17737u))) 
                            {
                                if (((_bits & 4096L) != 0))
                                {
                                    _headers._Cookie = AppendValue(_headers._Cookie, value);
                                }
                                else
                                {
                                    _bits |= 4096L;
                                    _headers._Cookie = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUI[0] & 3755991007u) == 1162893381u) && ((pUS[2] & 57311u) == 21571u))) 
                            {
                                if (((_bits & 8192L) != 0))
                                {
                                    _headers._Expect = AppendValue(_headers._Expect, value);
                                }
                                else
                                {
                                    _bits |= 8192L;
                                    _headers._Expect = new StringValues(value);
                                }
                                return;
                            }
                        
                            if ((((pUI[0] & 3755991007u) == 1195987535u) && ((pUS[2] & 57311u) == 20041u))) 
                            {
                                if (((_bits & 65536L) != 0))
                                {
                                    _headers._Origin = AppendValue(_headers._Origin, value);
                                }
                                else
                                {
                                    _bits |= 65536L;
                                    _headers._Origin = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 17:
                        {
                            if ((((pUL[0] & 16131858542891098079uL) == 5928221808112259668uL) && ((pUL[1] & 16131858542891098111uL) == 5641115115480565037uL) && ((pUB[16] & 223u) == 71u))) 
                            {
                                if (((_bits & 32L) != 0))
                                {
                                    _headers._TransferEncoding = AppendValue(_headers._TransferEncoding, value);
                                }
                                else
                                {
                                    _bits |= 32L;
                                    _headers._TransferEncoding = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 7:
                        {
                            if ((((pUI[0] & 3755991007u) == 1380405333u) && ((pUS[2] & 57311u) == 17473u) && ((pUB[6] & 223u) == 69u))) 
                            {
                                if (((_bits & 64L) != 0))
                                {
                                    _headers._Upgrade = AppendValue(_headers._Upgrade, value);
                                }
                                else
                                {
                                    _bits |= 64L;
                                    _headers._Upgrade = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 14:
                        {
                            if ((((pUL[0] & 18437701552104792031uL) == 3266321689424580419uL) && ((pUI[2] & 3755991007u) == 1196311884u) && ((pUS[6] & 57311u) == 18516u))) 
                            {
                                if (((_bits & 128L) != 0))
                                {
                                    _headers._ContentLength = AppendValue(_headers._ContentLength, value);
                                }
                                else
                                {
                                    _bits |= 128L;
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
                                if (((_bits & 256L) != 0))
                                {
                                    _headers._ContentType = AppendValue(_headers._ContentType, value);
                                }
                                else
                                {
                                    _bits |= 256L;
                                    _headers._ContentType = new StringValues(value);
                                }
                                return;
                            }
                        }
                        break;
                
                    case 16:
                        {
                            if ((((pUL[0] & 18437701552104792031uL) == 3266321689424580419uL) && ((pUL[1] & 16131858542891098079uL) == 5138124782612729413uL))) 
                            {
                                if (((_bits & 512L) != 0))
                                {
                                    _headers._ContentEncoding = AppendValue(_headers._ContentEncoding, value);
                                }
                                else
                                {
                                    _bits |= 512L;
                                    _headers._ContentEncoding = new StringValues(value);
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
            public StringValues _CacheControl;
            public StringValues _Connection;
            public StringValues _Date;
            public StringValues _KeepAlive;
            public StringValues _Pragma;
            public StringValues _TransferEncoding;
            public StringValues _Upgrade;
            public StringValues _ContentLength;
            public StringValues _ContentType;
            public StringValues _ContentEncoding;
            public StringValues _Accept;
            public StringValues _Authorization;
            public StringValues _Cookie;
            public StringValues _Expect;
            public StringValues _Host;
            public StringValues _UserAgent;
            public StringValues _Origin;
            
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
                    
                        case 4:
                            goto state4;
                    
                        case 5:
                            goto state5;
                    
                        case 6:
                            goto state6;
                    
                        case 7:
                            goto state7;
                    
                        case 8:
                            goto state8;
                    
                        case 9:
                            goto state9;
                    
                        case 10:
                            goto state10;
                    
                        case 11:
                            goto state11;
                    
                        case 12:
                            goto state12;
                    
                        case 13:
                            goto state13;
                    
                        case 14:
                            goto state14;
                    
                        case 15:
                            goto state15;
                    
                        case 16:
                            goto state16;
                    
                    default:
                        goto state_default;
                }
                
                state0:
                    if (((_bits & 1L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Cache-Control", _collection._headers._CacheControl);
                        _state = 1;
                        return true;
                    }
                
                state1:
                    if (((_bits & 2L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Connection", _collection._headers._Connection);
                        _state = 2;
                        return true;
                    }
                
                state2:
                    if (((_bits & 4L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Date", _collection._headers._Date);
                        _state = 3;
                        return true;
                    }
                
                state3:
                    if (((_bits & 8L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Keep-Alive", _collection._headers._KeepAlive);
                        _state = 4;
                        return true;
                    }
                
                state4:
                    if (((_bits & 16L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Pragma", _collection._headers._Pragma);
                        _state = 5;
                        return true;
                    }
                
                state5:
                    if (((_bits & 32L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Transfer-Encoding", _collection._headers._TransferEncoding);
                        _state = 6;
                        return true;
                    }
                
                state6:
                    if (((_bits & 64L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Upgrade", _collection._headers._Upgrade);
                        _state = 7;
                        return true;
                    }
                
                state7:
                    if (((_bits & 128L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Length", _collection._headers._ContentLength);
                        _state = 8;
                        return true;
                    }
                
                state8:
                    if (((_bits & 256L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Type", _collection._headers._ContentType);
                        _state = 9;
                        return true;
                    }
                
                state9:
                    if (((_bits & 512L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Encoding", _collection._headers._ContentEncoding);
                        _state = 10;
                        return true;
                    }
                
                state10:
                    if (((_bits & 1024L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Accept", _collection._headers._Accept);
                        _state = 11;
                        return true;
                    }
                
                state11:
                    if (((_bits & 2048L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Authorization", _collection._headers._Authorization);
                        _state = 12;
                        return true;
                    }
                
                state12:
                    if (((_bits & 4096L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Cookie", _collection._headers._Cookie);
                        _state = 13;
                        return true;
                    }
                
                state13:
                    if (((_bits & 8192L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Expect", _collection._headers._Expect);
                        _state = 14;
                        return true;
                    }
                
                state14:
                    if (((_bits & 16384L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Host", _collection._headers._Host);
                        _state = 15;
                        return true;
                    }
                
                state15:
                    if (((_bits & 32768L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("User-Agent", _collection._headers._UserAgent);
                        _state = 16;
                        return true;
                    }
                
                state16:
                    if (((_bits & 65536L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Origin", _collection._headers._Origin);
                        _state = 17;
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
            13,10,67,97,99,104,101,45,67,111,110,116,114,111,108,58,32,13,10,67,111,110,110,101,99,116,105,111,110,58,32,13,10,68,97,116,101,58,32,13,10,75,101,101,112,45,65,108,105,118,101,58,32,13,10,80,114,97,103,109,97,58,32,13,10,84,114,97,110,115,102,101,114,45,69,110,99,111,100,105,110,103,58,32,13,10,85,112,103,114,97,100,101,58,32,13,10,67,111,110,116,101,110,116,45,76,101,110,103,116,104,58,32,13,10,67,111,110,116,101,110,116,45,84,121,112,101,58,32,13,10,67,111,110,116,101,110,116,45,69,110,99,111,100,105,110,103,58,32,13,10,76,111,99,97,116,105,111,110,58,32,13,10,83,101,114,118,101,114,58,32,13,10,83,101,116,45,67,111,111,107,105,101,58,32,13,10,87,87,87,45,65,117,116,104,101,110,116,105,99,97,116,101,58,32,13,10,65,99,99,101,115,115,45,67,111,110,116,114,111,108,45,65,108,108,111,119,45,67,114,101,100,101,110,116,105,97,108,115,58,32,
        };
        
        private long _bits = 0;
        private HeaderReferences _headers;
        
        public StringValues HeaderCacheControl
        {
            get
            {
                if (((_bits & 1L) != 0))
                {
                    return _headers._CacheControl;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 1L;
                _headers._CacheControl = value; 
            }
        }
        public StringValues HeaderConnection
        {
            get
            {
                if (((_bits & 2L) != 0))
                {
                    return _headers._Connection;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 2L;
                _headers._Connection = value; 
                _headers._rawConnection = null;
            }
        }
        public StringValues HeaderDate
        {
            get
            {
                if (((_bits & 4L) != 0))
                {
                    return _headers._Date;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 4L;
                _headers._Date = value; 
                _headers._rawDate = null;
            }
        }
        public StringValues HeaderKeepAlive
        {
            get
            {
                if (((_bits & 8L) != 0))
                {
                    return _headers._KeepAlive;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 8L;
                _headers._KeepAlive = value; 
            }
        }
        public StringValues HeaderPragma
        {
            get
            {
                if (((_bits & 16L) != 0))
                {
                    return _headers._Pragma;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 16L;
                _headers._Pragma = value; 
            }
        }
        public StringValues HeaderTransferEncoding
        {
            get
            {
                if (((_bits & 32L) != 0))
                {
                    return _headers._TransferEncoding;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 32L;
                _headers._TransferEncoding = value; 
                _headers._rawTransferEncoding = null;
            }
        }
        public StringValues HeaderUpgrade
        {
            get
            {
                if (((_bits & 64L) != 0))
                {
                    return _headers._Upgrade;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 64L;
                _headers._Upgrade = value; 
            }
        }
        public StringValues HeaderContentLength
        {
            get
            {
                if (((_bits & 128L) != 0))
                {
                    return _headers._ContentLength;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 128L;
                _headers._ContentLength = value; 
                _headers._rawContentLength = null;
            }
        }
        public StringValues HeaderContentType
        {
            get
            {
                if (((_bits & 256L) != 0))
                {
                    return _headers._ContentType;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 256L;
                _headers._ContentType = value; 
            }
        }
        public StringValues HeaderContentEncoding
        {
            get
            {
                if (((_bits & 512L) != 0))
                {
                    return _headers._ContentEncoding;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 512L;
                _headers._ContentEncoding = value; 
            }
        }
        public StringValues HeaderLocation
        {
            get
            {
                if (((_bits & 1024L) != 0))
                {
                    return _headers._Location;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 1024L;
                _headers._Location = value; 
            }
        }
        public StringValues HeaderServer
        {
            get
            {
                if (((_bits & 2048L) != 0))
                {
                    return _headers._Server;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 2048L;
                _headers._Server = value; 
                _headers._rawServer = null;
            }
        }
        public StringValues HeaderSetCookie
        {
            get
            {
                if (((_bits & 4096L) != 0))
                {
                    return _headers._SetCookie;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 4096L;
                _headers._SetCookie = value; 
            }
        }
        public StringValues HeaderWWWAuthenticate
        {
            get
            {
                if (((_bits & 8192L) != 0))
                {
                    return _headers._WWWAuthenticate;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 8192L;
                _headers._WWWAuthenticate = value; 
            }
        }
        public StringValues HeaderAccessControlAllowCredentials
        {
            get
            {
                if (((_bits & 16384L) != 0))
                {
                    return _headers._AccessControlAllowCredentials;
                }
                return StringValues.Empty;
            }
            set
            {
                _bits |= 16384L;
                _headers._AccessControlAllowCredentials = value; 
            }
        }
        
        public void SetRawConnection(StringValues value, byte[] raw)
        {
            _bits |= 2L;
            _headers._Connection = value; 
            _headers._rawConnection = raw;
        }
        public void SetRawDate(StringValues value, byte[] raw)
        {
            _bits |= 4L;
            _headers._Date = value; 
            _headers._rawDate = raw;
        }
        public void SetRawTransferEncoding(StringValues value, byte[] raw)
        {
            _bits |= 32L;
            _headers._TransferEncoding = value; 
            _headers._rawTransferEncoding = raw;
        }
        public void SetRawContentLength(StringValues value, byte[] raw)
        {
            _bits |= 128L;
            _headers._ContentLength = value; 
            _headers._rawContentLength = raw;
        }
        public void SetRawServer(StringValues value, byte[] raw)
        {
            _bits |= 2048L;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                return _headers._CacheControl;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                return _headers._Connection;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                return _headers._KeepAlive;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Set-Cookie".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                return _headers._SetCookie;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
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

                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 16L) != 0))
                            {
                                return _headers._Pragma;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2048L) != 0))
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

                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 32L) != 0))
                            {
                                return _headers._TransferEncoding;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 64L) != 0))
                            {
                                return _headers._Upgrade;
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
                            if (((_bits & 128L) != 0))
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
                            if (((_bits & 256L) != 0))
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

                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 512L) != 0))
                            {
                                return _headers._ContentEncoding;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    
                        if ("WWW-Authenticate".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                return _headers._WWWAuthenticate;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 8:
                    {
                        if ("Location".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                return _headers._Location;
                            }
                            else
                            {
                                ThrowKeyNotFoundException();
                            }
                        }
                    }
                    break;

                case 32:
                    {
                        if ("Access-Control-Allow-Credentials".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 16384L) != 0))
                            {
                                return _headers._AccessControlAllowCredentials;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                value = _headers._CacheControl;
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

                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                value = _headers._Connection;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                value = _headers._KeepAlive;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Set-Cookie".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                value = _headers._SetCookie;
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

                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
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

                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 16L) != 0))
                            {
                                value = _headers._Pragma;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2048L) != 0))
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

                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 32L) != 0))
                            {
                                value = _headers._TransferEncoding;
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

                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 64L) != 0))
                            {
                                value = _headers._Upgrade;
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
                            if (((_bits & 128L) != 0))
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
                            if (((_bits & 256L) != 0))
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

                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 512L) != 0))
                            {
                                value = _headers._ContentEncoding;
                                return true;
                            }
                            else
                            {
                                value = StringValues.Empty;
                                return false;
                            }
                        }
                    
                        if ("WWW-Authenticate".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                value = _headers._WWWAuthenticate;
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

                case 8:
                    {
                        if ("Location".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                value = _headers._Location;
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

                case 32:
                    {
                        if ("Access-Control-Allow-Credentials".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 16384L) != 0))
                            {
                                value = _headers._AccessControlAllowCredentials;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 1L;
                            _headers._CacheControl = value;
                            return;
                        }
                    }
                    break;

                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 2L;
                            _headers._Connection = value;
                            _headers._rawConnection = null;
                            return;
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 8L;
                            _headers._KeepAlive = value;
                            return;
                        }
                    
                        if ("Set-Cookie".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 4096L;
                            _headers._SetCookie = value;
                            return;
                        }
                    }
                    break;

                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 4L;
                            _headers._Date = value;
                            _headers._rawDate = null;
                            return;
                        }
                    }
                    break;

                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 16L;
                            _headers._Pragma = value;
                            return;
                        }
                    
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 2048L;
                            _headers._Server = value;
                            _headers._rawServer = null;
                            return;
                        }
                    }
                    break;

                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 32L;
                            _headers._TransferEncoding = value;
                            _headers._rawTransferEncoding = null;
                            return;
                        }
                    }
                    break;

                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 64L;
                            _headers._Upgrade = value;
                            return;
                        }
                    }
                    break;

                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 128L;
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
                            _bits |= 256L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;

                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 512L;
                            _headers._ContentEncoding = value;
                            return;
                        }
                    
                        if ("WWW-Authenticate".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 8192L;
                            _headers._WWWAuthenticate = value;
                            return;
                        }
                    }
                    break;

                case 8:
                    {
                        if ("Location".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 1024L;
                            _headers._Location = value;
                            return;
                        }
                    }
                    break;

                case 32:
                    {
                        if ("Access-Control-Allow-Credentials".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            _bits |= 16384L;
                            _headers._AccessControlAllowCredentials = value;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 1L;
                            _headers._CacheControl = value;
                            return;
                        }
                    }
                    break;
            
                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 2L;
                            _headers._Connection = value;
                            _headers._rawConnection = null;
                            return;
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 8L;
                            _headers._KeepAlive = value;
                            return;
                        }
                    
                        if ("Set-Cookie".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 4096L;
                            _headers._SetCookie = value;
                            return;
                        }
                    }
                    break;
            
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 4L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 4L;
                            _headers._Date = value;
                            _headers._rawDate = null;
                            return;
                        }
                    }
                    break;
            
                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 16L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 16L;
                            _headers._Pragma = value;
                            return;
                        }
                    
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 2048L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 2048L;
                            _headers._Server = value;
                            _headers._rawServer = null;
                            return;
                        }
                    }
                    break;
            
                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 32L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 32L;
                            _headers._TransferEncoding = value;
                            _headers._rawTransferEncoding = null;
                            return;
                        }
                    }
                    break;
            
                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 64L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 64L;
                            _headers._Upgrade = value;
                            return;
                        }
                    }
                    break;
            
                case 14:
                    {
                        if ("Content-Length".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 128L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 128L;
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
                            if (((_bits & 256L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 256L;
                            _headers._ContentType = value;
                            return;
                        }
                    }
                    break;
            
                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 512L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 512L;
                            _headers._ContentEncoding = value;
                            return;
                        }
                    
                        if ("WWW-Authenticate".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 8192L;
                            _headers._WWWAuthenticate = value;
                            return;
                        }
                    }
                    break;
            
                case 8:
                    {
                        if ("Location".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 1024L;
                            _headers._Location = value;
                            return;
                        }
                    }
                    break;
            
                case 32:
                    {
                        if ("Access-Control-Allow-Credentials".Equals(key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (((_bits & 16384L) != 0))
                            {
                                ThrowDuplicateKeyException();
                            }
                            _bits |= 16384L;
                            _headers._AccessControlAllowCredentials = value;
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
                case 13:
                    {
                        if ("Cache-Control".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1L) != 0))
                            {
                                _bits &= ~1L;
                                _headers._CacheControl = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 10:
                    {
                        if ("Connection".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2L) != 0))
                            {
                                _bits &= ~2L;
                                _headers._Connection = StringValues.Empty;
                                _headers._rawConnection = null;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Keep-Alive".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8L) != 0))
                            {
                                _bits &= ~8L;
                                _headers._KeepAlive = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Set-Cookie".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4096L) != 0))
                            {
                                _bits &= ~4096L;
                                _headers._SetCookie = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 4:
                    {
                        if ("Date".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 4L) != 0))
                            {
                                _bits &= ~4L;
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
            
                case 6:
                    {
                        if ("Pragma".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 16L) != 0))
                            {
                                _bits &= ~16L;
                                _headers._Pragma = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("Server".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 2048L) != 0))
                            {
                                _bits &= ~2048L;
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
            
                case 17:
                    {
                        if ("Transfer-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 32L) != 0))
                            {
                                _bits &= ~32L;
                                _headers._TransferEncoding = StringValues.Empty;
                                _headers._rawTransferEncoding = null;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 7:
                    {
                        if ("Upgrade".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 64L) != 0))
                            {
                                _bits &= ~64L;
                                _headers._Upgrade = StringValues.Empty;
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
                            if (((_bits & 128L) != 0))
                            {
                                _bits &= ~128L;
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
                            if (((_bits & 256L) != 0))
                            {
                                _bits &= ~256L;
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
            
                case 16:
                    {
                        if ("Content-Encoding".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 512L) != 0))
                            {
                                _bits &= ~512L;
                                _headers._ContentEncoding = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    
                        if ("WWW-Authenticate".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 8192L) != 0))
                            {
                                _bits &= ~8192L;
                                _headers._WWWAuthenticate = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 8:
                    {
                        if ("Location".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 1024L) != 0))
                            {
                                _bits &= ~1024L;
                                _headers._Location = StringValues.Empty;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    break;
            
                case 32:
                    {
                        if ("Access-Control-Allow-Credentials".Equals(key, StringComparison.OrdinalIgnoreCase)) 
                        {
                            if (((_bits & 16384L) != 0))
                            {
                                _bits &= ~16384L;
                                _headers._AccessControlAllowCredentials = StringValues.Empty;
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

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Cache-Control", _headers._CacheControl);
                    ++arrayIndex;
                }
            
                if (((_bits & 2L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Connection", _headers._Connection);
                    ++arrayIndex;
                }
            
                if (((_bits & 4L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Date", _headers._Date);
                    ++arrayIndex;
                }
            
                if (((_bits & 8L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Keep-Alive", _headers._KeepAlive);
                    ++arrayIndex;
                }
            
                if (((_bits & 16L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Pragma", _headers._Pragma);
                    ++arrayIndex;
                }
            
                if (((_bits & 32L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Transfer-Encoding", _headers._TransferEncoding);
                    ++arrayIndex;
                }
            
                if (((_bits & 64L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Upgrade", _headers._Upgrade);
                    ++arrayIndex;
                }
            
                if (((_bits & 128L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Length", _headers._ContentLength);
                    ++arrayIndex;
                }
            
                if (((_bits & 256L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Type", _headers._ContentType);
                    ++arrayIndex;
                }
            
                if (((_bits & 512L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Content-Encoding", _headers._ContentEncoding);
                    ++arrayIndex;
                }
            
                if (((_bits & 1024L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Location", _headers._Location);
                    ++arrayIndex;
                }
            
                if (((_bits & 2048L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Server", _headers._Server);
                    ++arrayIndex;
                }
            
                if (((_bits & 4096L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Set-Cookie", _headers._SetCookie);
                    ++arrayIndex;
                }
            
                if (((_bits & 8192L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("WWW-Authenticate", _headers._WWWAuthenticate);
                    ++arrayIndex;
                }
            
                if (((_bits & 16384L) != 0)) 
                {
                    if (arrayIndex == array.Length)
                    {
                        ThrowArgumentException();
                    }

                    array[arrayIndex] = new KeyValuePair<string, StringValues>("Access-Control-Allow-Credentials", _headers._AccessControlAllowCredentials);
                    ++arrayIndex;
                }
            
            ((ICollection<KeyValuePair<string, StringValues>>)MaybeUnknown)?.CopyTo(array, arrayIndex);
        }
        
        protected void CopyToFast(ref MemoryPoolIterator output)
        {
            
                if (((_bits & 1L) != 0)) 
                { 
                        foreach (var value in _headers._CacheControl)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 0, 17);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 2L) != 0)) 
                { 
                    if (_headers._rawConnection != null) 
                    {
                        output.CopyFrom(_headers._rawConnection, 0, _headers._rawConnection.Length);
                    } 
                    else 
                        foreach (var value in _headers._Connection)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 17, 14);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 4L) != 0)) 
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
                                output.CopyFrom(_headerBytes, 31, 8);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 8L) != 0)) 
                { 
                        foreach (var value in _headers._KeepAlive)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 39, 14);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 16L) != 0)) 
                { 
                        foreach (var value in _headers._Pragma)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 53, 10);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 32L) != 0)) 
                { 
                    if (_headers._rawTransferEncoding != null) 
                    {
                        output.CopyFrom(_headers._rawTransferEncoding, 0, _headers._rawTransferEncoding.Length);
                    } 
                    else 
                        foreach (var value in _headers._TransferEncoding)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 63, 21);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 64L) != 0)) 
                { 
                        foreach (var value in _headers._Upgrade)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 84, 11);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 128L) != 0)) 
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
                                output.CopyFrom(_headerBytes, 95, 18);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 256L) != 0)) 
                { 
                        foreach (var value in _headers._ContentType)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 113, 16);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 512L) != 0)) 
                { 
                        foreach (var value in _headers._ContentEncoding)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 129, 20);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 1024L) != 0)) 
                { 
                        foreach (var value in _headers._Location)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 149, 12);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 2048L) != 0)) 
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
                                output.CopyFrom(_headerBytes, 161, 10);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 4096L) != 0)) 
                { 
                        foreach (var value in _headers._SetCookie)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 171, 14);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 8192L) != 0)) 
                { 
                        foreach (var value in _headers._WWWAuthenticate)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 185, 20);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
                if (((_bits & 16384L) != 0)) 
                { 
                        foreach (var value in _headers._AccessControlAllowCredentials)
                        {
                            if (value != null)
                            {
                                output.CopyFrom(_headerBytes, 205, 36);
                                output.CopyFromAscii(value);
                            }
                        }
                }
            
        }
        
        private struct HeaderReferences
        {
            public StringValues _CacheControl;
            public StringValues _Connection;
            public StringValues _Date;
            public StringValues _KeepAlive;
            public StringValues _Pragma;
            public StringValues _TransferEncoding;
            public StringValues _Upgrade;
            public StringValues _ContentLength;
            public StringValues _ContentType;
            public StringValues _ContentEncoding;
            public StringValues _Location;
            public StringValues _Server;
            public StringValues _SetCookie;
            public StringValues _WWWAuthenticate;
            public StringValues _AccessControlAllowCredentials;
            
            public byte[] _rawConnection;
            public byte[] _rawDate;
            public byte[] _rawTransferEncoding;
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
                    
                        case 4:
                            goto state4;
                    
                        case 5:
                            goto state5;
                    
                        case 6:
                            goto state6;
                    
                        case 7:
                            goto state7;
                    
                        case 8:
                            goto state8;
                    
                        case 9:
                            goto state9;
                    
                        case 10:
                            goto state10;
                    
                        case 11:
                            goto state11;
                    
                        case 12:
                            goto state12;
                    
                        case 13:
                            goto state13;
                    
                        case 14:
                            goto state14;
                    
                    default:
                        goto state_default;
                }
                
                state0:
                    if (((_bits & 1L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Cache-Control", _collection._headers._CacheControl);
                        _state = 1;
                        return true;
                    }
                
                state1:
                    if (((_bits & 2L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Connection", _collection._headers._Connection);
                        _state = 2;
                        return true;
                    }
                
                state2:
                    if (((_bits & 4L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Date", _collection._headers._Date);
                        _state = 3;
                        return true;
                    }
                
                state3:
                    if (((_bits & 8L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Keep-Alive", _collection._headers._KeepAlive);
                        _state = 4;
                        return true;
                    }
                
                state4:
                    if (((_bits & 16L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Pragma", _collection._headers._Pragma);
                        _state = 5;
                        return true;
                    }
                
                state5:
                    if (((_bits & 32L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Transfer-Encoding", _collection._headers._TransferEncoding);
                        _state = 6;
                        return true;
                    }
                
                state6:
                    if (((_bits & 64L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Upgrade", _collection._headers._Upgrade);
                        _state = 7;
                        return true;
                    }
                
                state7:
                    if (((_bits & 128L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Length", _collection._headers._ContentLength);
                        _state = 8;
                        return true;
                    }
                
                state8:
                    if (((_bits & 256L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Type", _collection._headers._ContentType);
                        _state = 9;
                        return true;
                    }
                
                state9:
                    if (((_bits & 512L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Content-Encoding", _collection._headers._ContentEncoding);
                        _state = 10;
                        return true;
                    }
                
                state10:
                    if (((_bits & 1024L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Location", _collection._headers._Location);
                        _state = 11;
                        return true;
                    }
                
                state11:
                    if (((_bits & 2048L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Server", _collection._headers._Server);
                        _state = 12;
                        return true;
                    }
                
                state12:
                    if (((_bits & 4096L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Set-Cookie", _collection._headers._SetCookie);
                        _state = 13;
                        return true;
                    }
                
                state13:
                    if (((_bits & 8192L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("WWW-Authenticate", _collection._headers._WWWAuthenticate);
                        _state = 14;
                        return true;
                    }
                
                state14:
                    if (((_bits & 16384L) != 0))
                    {
                        _current = new KeyValuePair<string, StringValues>("Access-Control-Allow-Credentials", _collection._headers._AccessControlAllowCredentials);
                        _state = 15;
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