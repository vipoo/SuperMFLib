#region license

/*
MediaFoundationLib - Provide access to MediaFoundation interfaces via .NET
Copyright (C) 2007
http://sourceforge.net/projects/directshownet/

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

#endregion

using System;
using System.Text;
using System.Runtime.InteropServices;

using MediaFoundation.Misc;

namespace MediaFoundation.Utils
{
    #region Utility Classes

    sealed public class MFError
    {
        #region Errors

        public const int MF_E_PLATFORM_NOT_INITIALIZED = unchecked((int)0xC00D36B0);
        public const int MF_E_BUFFERTOOSMALL = unchecked((int)0xC00D36B1);
        public const int MF_E_INVALIDREQUEST = unchecked((int)0xC00D36B2);
        public const int MF_E_INVALIDSTREAMNUMBER = unchecked((int)0xC00D36B3);
        public const int MF_E_INVALIDMEDIATYPE = unchecked((int)0xC00D36B4);
        public const int MF_E_NOTACCEPTING = unchecked((int)0xC00D36B5);
        public const int MF_E_NOT_INITIALIZED = unchecked((int)0xC00D36B6);
        public const int MF_E_UNSUPPORTED_REPRESENTATION = unchecked((int)0xC00D36B7);
        public const int MF_E_NO_MORE_TYPES = unchecked((int)0xC00D36B9);
        public const int MF_E_UNSUPPORTED_SERVICE = unchecked((int)0xC00D36BA);
        public const int MF_E_UNEXPECTED = unchecked((int)0xC00D36BB);
        public const int MF_E_INVALIDNAME = unchecked((int)0xC00D36BC);
        public const int MF_E_INVALIDTYPE = unchecked((int)0xC00D36BD);
        public const int MF_E_INVALID_FILE_FORMAT = unchecked((int)0xC00D36BE);
        public const int MF_E_INVALIDINDEX = unchecked((int)0xC00D36BF);
        public const int MF_E_INVALID_TIMESTAMP = unchecked((int)0xC00D36C0);
        public const int MF_E_UNSUPPORTED_SCHEME = unchecked((int)0xC00D36C3);
        public const int MF_E_UNSUPPORTED_BYTESTREAM_TYPE = unchecked((int)0xC00D36C4);
        public const int MF_E_UNSUPPORTED_TIME_FORMAT = unchecked((int)0xC00D36C5);
        public const int MF_E_NO_SAMPLE_TIMESTAMP = unchecked((int)0xC00D36C8);
        public const int MF_E_NO_SAMPLE_DURATION = unchecked((int)0xC00D36C9);
        public const int MF_E_INVALID_STREAM_DATA = unchecked((int)0xC00D36CB);
        public const int MF_E_RT_UNAVAILABLE = unchecked((int)0xC00D36CF);
        public const int MF_E_UNSUPPORTED_RATE = unchecked((int)0xC00D36D0);
        public const int MF_E_THINNING_UNSUPPORTED = unchecked((int)0xC00D36D1);
        public const int MF_E_REVERSE_UNSUPPORTED = unchecked((int)0xC00D36D2);
        public const int MF_E_UNSUPPORTED_RATE_TRANSITION = unchecked((int)0xC00D36D3);
        public const int MF_E_RATE_CHANGE_PREEMPTED = unchecked((int)0xC00D36D4);
        public const int MF_E_NOT_FOUND = unchecked((int)0xC00D36D5);
        public const int MF_E_NOT_AVAILABLE = unchecked((int)0xC00D36D6);
        public const int MF_E_NO_CLOCK = unchecked((int)0xC00D36D7);
        public const int MF_S_MULTIPLE_BEGIN = unchecked((int)0x000D36D8);
        public const int MF_E_MULTIPLE_BEGIN = unchecked((int)0xC00D36D9);
        public const int MF_E_MULTIPLE_SUBSCRIBERS = unchecked((int)0xC00D36DA);
        public const int MF_E_TIMER_ORPHANED = unchecked((int)0xC00D36DB);
        public const int MF_E_STATE_TRANSITION_PENDING = unchecked((int)0xC00D36DC);
        public const int MF_E_UNSUPPORTED_STATE_TRANSITION = unchecked((int)0xC00D36DD);
        public const int MF_E_UNRECOVERABLE_ERROR_OCCURRED = unchecked((int)0xC00D36DE);
        public const int MF_E_SAMPLE_HAS_TOO_MANY_BUFFERS = unchecked((int)0xC00D36DF);
        public const int MF_E_SAMPLE_NOT_WRITABLE = unchecked((int)0xC00D36E0);
        public const int MF_E_INVALID_KEY = unchecked((int)0xC00D36E2);
        public const int MF_E_BAD_STARTUP_VERSION = unchecked((int)0xC00D36E3);
        public const int MF_E_UNSUPPORTED_CAPTION = unchecked((int)0xC00D36E4);
        public const int MF_E_INVALID_POSITION = unchecked((int)0xC00D36E5);
        public const int MF_E_ATTRIBUTENOTFOUND = unchecked((int)0xC00D36E6);
        public const int MF_E_PROPERTY_TYPE_NOT_ALLOWED = unchecked((int)0xC00D36E7);
        public const int MF_E_PROPERTY_TYPE_NOT_SUPPORTED = unchecked((int)0xC00D36E8);
        public const int MF_E_PROPERTY_EMPTY = unchecked((int)0xC00D36E9);
        public const int MF_E_PROPERTY_NOT_EMPTY = unchecked((int)0xC00D36EA);
        public const int MF_E_PROPERTY_VECTOR_NOT_ALLOWED = unchecked((int)0xC00D36EB);
        public const int MF_E_PROPERTY_VECTOR_REQUIRED = unchecked((int)0xC00D36EC);
        public const int MF_E_OPERATION_CANCELLED = unchecked((int)0xC00D36ED);
        public const int MF_E_BYTESTREAM_NOT_SEEKABLE = unchecked((int)0xC00D36EE);
        public const int MF_E_DISABLED_IN_SAFEMODE = unchecked((int)0xC00D36EF);
        public const int MF_E_CANNOT_PARSE_BYTESTREAM = unchecked((int)0xC00D36F0);
        public const int MF_E_SOURCERESOLVER_MUTUALLY_EXCLUSIVE_FLAGS = unchecked((int)0xC00D36F1);
        public const int MF_E_MEDIAPROC_WRONGSTATE = unchecked((int)0xC00D36F2);
        public const int MF_E_RT_THROUGHPUT_NOT_AVAILABLE = unchecked((int)0xC00D36F3);
        public const int MF_E_RT_TOO_MANY_CLASSES = unchecked((int)0xC00D36F4);
        public const int MF_E_RT_WOULDBLOCK = unchecked((int)0xC00D36F5);
        public const int MF_E_NO_BITPUMP = unchecked((int)0xC00D36F6);
        public const int MF_E_RT_OUTOFMEMORY = unchecked((int)0xC00D36F7);
        public const int MF_E_RT_WORKQUEUE_CLASS_NOT_SPECIFIED = unchecked((int)0xC00D36F8);
        public const int MF_E_INSUFFICIENT_BUFFER = unchecked((int)0xC00D7170);
        public const int MF_E_CANNOT_CREATE_SINK = unchecked((int)0xC00D36FA);
        public const int MF_E_BYTESTREAM_UNKNOWN_LENGTH = unchecked((int)0xC00D36FB);
        public const int MF_E_SESSION_PAUSEWHILESTOPPED = unchecked((int)0xC00D36FC);
        public const int MF_S_ACTIVATE_REPLACED = unchecked((int)0x000D36FD);
        public const int MF_E_FORMAT_CHANGE_NOT_SUPPORTED = unchecked((int)0xC00D36FE);
        public const int MF_S_ASF_PARSEINPROGRESS = unchecked((int)0x400D3A98);
        public const int MF_E_ASF_PARSINGINCOMPLETE = unchecked((int)0xC00D3A98);
        public const int MF_E_ASF_MISSINGDATA = unchecked((int)0xC00D3A99);
        public const int MF_E_ASF_INVALIDDATA = unchecked((int)0xC00D3A9A);
        public const int MF_E_ASF_OPAQUEPACKET = unchecked((int)0xC00D3A9B);
        public const int MF_E_ASF_NOINDEX = unchecked((int)0xC00D3A9C);
        public const int MF_E_ASF_OUTOFRANGE = unchecked((int)0xC00D3A9D);
        public const int MF_E_ASF_INDEXNOTLOADED = unchecked((int)0xC00D3A9E);
        public const int MF_E_ASF_TOO_MANY_PAYLOADS = unchecked((int)0xC00D3A9F);
        public const int MF_E_ASF_UNSUPPORTED_STREAM_TYPE = unchecked((int)0xC00D3AA0);
        public const int MF_E_NO_EVENTS_AVAILABLE = unchecked((int)0xC00D3E80);
        public const int MF_E_INVALID_STATE_TRANSITION = unchecked((int)0xC00D3E82);
        public const int MF_E_END_OF_STREAM = unchecked((int)0xC00D3E84);
        public const int MF_E_SHUTDOWN = unchecked((int)0xC00D3E85);
        public const int MF_E_MP3_NOTFOUND = unchecked((int)0xC00D3E86);
        public const int MF_E_MP3_OUTOFDATA = unchecked((int)0xC00D3E87);
        public const int MF_E_MP3_NOTMP3 = unchecked((int)0xC00D3E88);
        public const int MF_E_MP3_NOTSUPPORTED = unchecked((int)0xC00D3E89);
        public const int MF_E_NO_DURATION = unchecked((int)0xC00D3E8A);
        public const int MF_E_INVALID_FORMAT = unchecked((int)0xC00D3E8C);
        public const int MF_E_PROPERTY_NOT_FOUND = unchecked((int)0xC00D3E8D);
        public const int MF_E_PROPERTY_READ_ONLY = unchecked((int)0xC00D3E8E);
        public const int MF_E_PROPERTY_NOT_ALLOWED = unchecked((int)0xC00D3E8F);
        public const int MF_E_MEDIA_SOURCE_NOT_STARTED = unchecked((int)0xC00D3E91);
        public const int MF_E_UNSUPPORTED_FORMAT = unchecked((int)0xC00D3E98);
        public const int MF_E_MP3_BAD_CRC = unchecked((int)0xC00D3E99);
        public const int MF_E_NOT_PROTECTED = unchecked((int)0xC00D3E9A);
        public const int MF_E_MEDIA_SOURCE_WRONGSTATE = unchecked((int)0xC00D3E9B);
        public const int MF_E_NETWORK_RESOURCE_FAILURE = unchecked((int)0xC00D4268);
        public const int MF_E_NET_WRITE = unchecked((int)0xC00D4269);
        public const int MF_E_NET_READ = unchecked((int)0xC00D426A);
        public const int MF_E_NET_REQUIRE_NETWORK = unchecked((int)0xC00D426B);
        public const int MF_E_NET_REQUIRE_ASYNC = unchecked((int)0xC00D426C);
        public const int MF_E_NET_BWLEVEL_NOT_SUPPORTED = unchecked((int)0xC00D426D);
        public const int MF_E_NET_STREAMGROUPS_NOT_SUPPORTED = unchecked((int)0xC00D426E);
        public const int MF_E_NET_MANUALSS_NOT_SUPPORTED = unchecked((int)0xC00D426F);
        public const int MF_E_NET_INVALID_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D4270);
        public const int MF_E_NET_CACHESTREAM_NOT_FOUND = unchecked((int)0xC00D4271);
        public const int MF_I_MANUAL_PROXY = unchecked((int)0x400D4272);
        public const int MF_E_NET_REQUIRE_INPUT = unchecked((int)0xC00D4274);
        public const int MF_E_NET_REDIRECT = unchecked((int)0xC00D4275);
        public const int MF_E_NET_REDIRECT_TO_PROXY = unchecked((int)0xC00D4276);
        public const int MF_E_NET_TOO_MANY_REDIRECTS = unchecked((int)0xC00D4277);
        public const int MF_E_NET_TIMEOUT = unchecked((int)0xC00D4278);
        public const int MF_E_NET_CLIENT_CLOSE = unchecked((int)0xC00D4279);
        public const int MF_E_NET_BAD_CONTROL_DATA = unchecked((int)0xC00D427A);
        public const int MF_E_NET_INCOMPATIBLE_SERVER = unchecked((int)0xC00D427B);
        public const int MF_E_NET_UNSAFE_URL = unchecked((int)0xC00D427C);
        public const int MF_E_NET_CACHE_NO_DATA = unchecked((int)0xC00D427D);
        public const int MF_E_NET_EOL = unchecked((int)0xC00D427E);
        public const int MF_E_NET_BAD_REQUEST = unchecked((int)0xC00D427F);
        public const int MF_E_NET_INTERNAL_SERVER_ERROR = unchecked((int)0xC00D4280);
        public const int MF_E_NET_SESSION_NOT_FOUND = unchecked((int)0xC00D4281);
        public const int MF_E_NET_NOCONNECTION = unchecked((int)0xC00D4282);
        public const int MF_E_NET_CONNECTION_FAILURE = unchecked((int)0xC00D4283);
        public const int MF_E_NET_INCOMPATIBLE_PUSHSERVER = unchecked((int)0xC00D4284);
        public const int MF_E_NET_SERVER_ACCESSDENIED = unchecked((int)0xC00D4285);
        public const int MF_E_NET_PROXY_ACCESSDENIED = unchecked((int)0xC00D4286);
        public const int MF_E_NET_CANNOTCONNECT = unchecked((int)0xC00D4287);
        public const int MF_E_NET_INVALID_PUSH_TEMPLATE = unchecked((int)0xC00D4288);
        public const int MF_E_NET_INVALID_PUSH_PUBLISHING_POINT = unchecked((int)0xC00D4289);
        public const int MF_E_NET_BUSY = unchecked((int)0xC00D428A);
        public const int MF_E_NET_RESOURCE_GONE = unchecked((int)0xC00D428B);
        public const int MF_E_NET_ERROR_FROM_PROXY = unchecked((int)0xC00D428C);
        public const int MF_E_NET_PROXY_TIMEOUT = unchecked((int)0xC00D428D);
        public const int MF_E_NET_SERVER_UNAVAILABLE = unchecked((int)0xC00D428E);
        public const int MF_E_NET_TOO_MUCH_DATA = unchecked((int)0xC00D428F);
        public const int MF_E_NET_SESSION_INVALID = unchecked((int)0xC00D4290);
        public const int MF_E_OFFLINE_MODE = unchecked((int)0xC00D4291);
        public const int MF_E_NET_UDP_BLOCKED = unchecked((int)0xC00D4292);
        public const int MF_E_NET_UNSUPPORTED_CONFIGURATION = unchecked((int)0xC00D4293);
        public const int MF_E_NET_PROTOCOL_DISABLED = unchecked((int)0xC00D4294);
        public const int MF_E_ALREADY_INITIALIZED = unchecked((int)0xC00D4650);
        public const int MF_E_BANDWIDTH_OVERRUN = unchecked((int)0xC00D4651);
        public const int MF_E_LATE_SAMPLE = unchecked((int)0xC00D4652);
        public const int MF_E_FLUSH_NEEDED = unchecked((int)0xC00D4653);
        public const int MF_E_INVALID_PROFILE = unchecked((int)0xC00D4654);
        public const int MF_E_INDEX_NOT_COMMITTED = unchecked((int)0xC00D4655);
        public const int MF_E_NO_INDEX = unchecked((int)0xC00D4656);
        public const int MF_E_CANNOT_INDEX_IN_PLACE = unchecked((int)0xC00D4657);
        public const int MF_E_MISSING_ASF_LEAKYBUCKET = unchecked((int)0xC00D4658);
        public const int MF_E_INVALID_ASF_STREAMID = unchecked((int)0xC00D4659);
        public const int MF_E_STREAMSINK_REMOVED = unchecked((int)0xC00D4A38);
        public const int MF_E_STREAMSINKS_OUT_OF_SYNC = unchecked((int)0xC00D4A3A);
        public const int MF_E_STREAMSINKS_FIXED = unchecked((int)0xC00D4A3B);
        public const int MF_E_STREAMSINK_EXISTS = unchecked((int)0xC00D4A3C);
        public const int MF_E_SAMPLEALLOCATOR_CANCELED = unchecked((int)0xC00D4A3D);
        public const int MF_E_SAMPLEALLOCATOR_EMPTY = unchecked((int)0xC00D4A3E);
        public const int MF_E_SINK_ALREADYSTOPPED = unchecked((int)0xC00D4A3F);
        public const int MF_E_ASF_FILESINK_BITRATE_UNKNOWN = unchecked((int)0xC00D4A40);
        public const int MF_E_SINK_NO_STREAMS = unchecked((int)0xC00D4A41);
        public const int MF_S_SINK_NOT_FINALIZED = unchecked((int)0x000D4A42);
        public const int MF_E_VIDEO_REN_NO_PROCAMP_HW = unchecked((int)0xC00D4E20);
        public const int MF_E_VIDEO_REN_NO_DEINTERLACE_HW = unchecked((int)0xC00D4E21);
        public const int MF_E_VIDEO_REN_COPYPROT_FAILED = unchecked((int)0xC00D4E22);
        public const int MF_E_VIDEO_REN_SURFACE_NOT_SHARED = unchecked((int)0xC00D4E23);
        public const int MF_E_VIDEO_DEVICE_LOCKED = unchecked((int)0xC00D4E24);
        public const int MF_E_NEW_VIDEO_DEVICE = unchecked((int)0xC00D4E25);
        public const int MF_E_NO_VIDEO_SAMPLE_AVAILABLE = unchecked((int)0xC00D4E26);
        public const int MF_E_NO_AUDIO_PLAYBACK_DEVICE = unchecked((int)0xC00D4E84);
        public const int MF_E_AUDIO_PLAYBACK_DEVICE_IN_USE = unchecked((int)0xC00D4E85);
        public const int MF_E_AUDIO_PLAYBACK_DEVICE_INVALIDATED = unchecked((int)0xC00D4E86);
        public const int MF_E_AUDIO_SERVICE_NOT_RUNNING = unchecked((int)0xC00D4E87);
        public const int MF_E_TOPO_INVALID_OPTIONAL_NODE = unchecked((int)0xC00D520E);
        public const int MF_E_TOPO_CANNOT_FIND_DECRYPTOR = unchecked((int)0xC00D5211);
        public const int MF_E_TOPO_CODEC_NOT_FOUND = unchecked((int)0xC00D5212);
        public const int MF_E_TOPO_CANNOT_CONNECT = unchecked((int)0xC00D5213);
        public const int MF_E_TOPO_UNSUPPORTED = unchecked((int)0xC00D5214);
        public const int MF_E_TOPO_INVALID_TIME_ATTRIBUTES = unchecked((int)0xC00D5215);
        public const int MF_E_SEQUENCER_UNKNOWN_SEGMENT_ID = unchecked((int)0xC00D61AC);
        public const int MF_S_SEQUENCER_CONTEXT_CANCELED = unchecked((int)0x000D61AD);
        public const int MF_E_NO_SOURCE_IN_CACHE = unchecked((int)0xC00D61AE);
        public const int MF_S_SEQUENCER_SEGMENT_AT_END_OF_STREAM = unchecked((int)0x000D61AF);
        public const int MF_E_TRANSFORM_TYPE_NOT_SET = unchecked((int)0xC00D6D60);
        public const int MF_E_TRANSFORM_STREAM_CHANGE = unchecked((int)0xC00D6D61);
        public const int MF_E_TRANSFORM_INPUT_REMAINING = unchecked((int)0xC00D6D62);
        public const int MF_E_TRANSFORM_PROFILE_MISSING = unchecked((int)0xC00D6D63);
        public const int MF_E_TRANSFORM_PROFILE_INVALID_OR_CORRUPT = unchecked((int)0xC00D6D64);
        public const int MF_E_TRANSFORM_PROFILE_TRUNCATED = unchecked((int)0xC00D6D65);
        public const int MF_E_TRANSFORM_PROPERTY_PID_NOT_RECOGNIZED = unchecked((int)0xC00D6D66);
        public const int MF_E_TRANSFORM_PROPERTY_VARIANT_TYPE_WRONG = unchecked((int)0xC00D6D67);
        public const int MF_E_TRANSFORM_PROPERTY_NOT_WRITEABLE = unchecked((int)0xC00D6D68);
        public const int MF_E_TRANSFORM_PROPERTY_ARRAY_VALUE_WRONG_NUM_DIM = unchecked((int)0xC00D6D69);
        public const int MF_E_TRANSFORM_PROPERTY_VALUE_SIZE_WRONG = unchecked((int)0xC00D6D6A);
        public const int MF_E_TRANSFORM_PROPERTY_VALUE_OUT_OF_RANGE = unchecked((int)0xC00D6D6B);
        public const int MF_E_TRANSFORM_PROPERTY_VALUE_INCOMPATIBLE = unchecked((int)0xC00D6D6C);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_OUTPUT_MEDIATYPE = unchecked((int)0xC00D6D6D);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_INPUT_MEDIATYPE = unchecked((int)0xC00D6D6E);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_MEDIATYPE_COMBINATION = unchecked((int)0xC00D6D6F);
        public const int MF_E_TRANSFORM_CONFLICTS_WITH_OTHER_CURRENTLY_ENABLED_FEATURES = unchecked((int)0xC00D6D70);
        public const int MF_E_TRANSFORM_NEED_MORE_INPUT = unchecked((int)0xC00D6D72);
        public const int MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_SPKR_CONFIG = unchecked((int)0xC00D6D73);
        public const int MF_E_TRANSFORM_CANNOT_CHANGE_MEDIATYPE_WHILE_PROCESSING = unchecked((int)0xC00D6D74);
        public const int MF_S_TRANSFORM_DO_NOT_PROPAGATE_EVENT = unchecked((int)0x000D6D75);
        public const int MF_E_UNSUPPORTED_D3D_TYPE = unchecked((int)0xC00D6D76);
        public const int MF_E_LICENSE_INCORRECT_RIGHTS = unchecked((int)0xC00D7148);
        public const int MF_E_LICENSE_OUTOFDATE = unchecked((int)0xC00D7149);
        public const int MF_E_LICENSE_REQUIRED = unchecked((int)0xC00D714A);
        public const int MF_E_DRM_HARDWARE_INCONSISTENT = unchecked((int)0xC00D714B);
        public const int MF_E_NO_CONTENT_PROTECTION_MANAGER = unchecked((int)0xC00D714C);
        public const int MF_E_LICENSE_RESTORE_NO_RIGHTS = unchecked((int)0xC00D714D);
        public const int MF_E_BACKUP_RESTRICTED_LICENSE = unchecked((int)0xC00D714E);
        public const int MF_E_LICENSE_RESTORE_NEEDS_INDIVIDUALIZATION = unchecked((int)0xC00D714F);
        public const int MF_S_PROTECTION_NOT_REQUIRED = unchecked((int)0x000D7150);
        public const int MF_E_COMPONENT_REVOKED = unchecked((int)0xC00D7151);
        public const int MF_E_TRUST_DISABLED = unchecked((int)0xC00D7152);
        public const int MF_E_WMDRMOTA_NO_ACTION = unchecked((int)0xC00D7153);
        public const int MF_E_WMDRMOTA_ACTION_ALREADY_SET = unchecked((int)0xC00D7154);
        public const int MF_E_WMDRMOTA_DRM_HEADER_NOT_AVAILABLE = unchecked((int)0xC00D7155);
        public const int MF_E_WMDRMOTA_DRM_ENCRYPTION_SCHEME_NOT_SUPPORTED = unchecked((int)0xC00D7156);
        public const int MF_E_WMDRMOTA_ACTION_MISMATCH = unchecked((int)0xC00D7157);
        public const int MF_E_WMDRMOTA_INVALID_POLICY = unchecked((int)0xC00D7158);
        public const int MF_E_POLICY_UNSUPPORTED = unchecked((int)0xC00D7159);
        public const int MF_E_OPL_NOT_SUPPORTED = unchecked((int)0xC00D715A);
        public const int MF_E_TOPOLOGY_VERIFICATION_FAILED = unchecked((int)0xC00D715B);
        public const int MF_E_SIGNATURE_VERIFICATION_FAILED = unchecked((int)0xC00D715C);
        public const int MF_E_DEBUGGING_NOT_ALLOWED = unchecked((int)0xC00D715D);
        public const int MF_E_CODE_EXPIRED = unchecked((int)0xC00D715E);
        public const int MF_E_GRL_VERSION_TOO_LOW = unchecked((int)0xC00D715F);
        public const int MF_E_GRL_RENEWAL_NOT_FOUND = unchecked((int)0xC00D7160);
        public const int MF_E_GRL_EXTENSIBLE_ENTRY_NOT_FOUND = unchecked((int)0xC00D7161);
        public const int MF_E_KERNEL_UNTRUSTED = unchecked((int)0xC00D7162);
        public const int MF_E_PEAUTH_UNTRUSTED = unchecked((int)0xC00D7163);
        public const int MF_E_NON_PE_PROCESS = unchecked((int)0xC00D7165);
        public const int MF_E_REBOOT_REQUIRED = unchecked((int)0xC00D7167);
        public const int MF_S_WAIT_FOR_POLICY_SET = unchecked((int)0x000D7168);
        public const int MF_S_VIDEO_DISABLED_WITH_UNKNOWN_SOFTWARE_OUTPUT = unchecked((int)0x000D7169);
        public const int MF_E_GRL_INVALID_FORMAT = unchecked((int)0xC00D716A);
        public const int MF_E_GRL_UNRECOGNIZED_FORMAT = unchecked((int)0xC00D716B);
        public const int MF_E_ALL_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716C);
        public const int MF_E_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716D);
        public const int MF_E_USERMODE_UNTRUSTED = unchecked((int)0xC00D716E);
        public const int MF_E_PEAUTH_SESSION_NOT_STARTED = unchecked((int)0xC00D716F);
        public const int MF_E_PEAUTH_PUBLICKEY_REVOKED = unchecked((int)0xC00D7171);
        public const int MF_E_GRL_ABSENT = unchecked((int)0xC00D7172);
        public const int MF_S_PE_TRUSTED = unchecked((int)0x000D7173);
        public const int MF_E_PE_UNTRUSTED = unchecked((int)0xC00D7174);
        public const int MF_E_PEAUTH_NOT_STARTED = unchecked((int)0xC00D7175);
        public const int MF_E_INCOMPATIBLE_SAMPLE_PROTECTION = unchecked((int)0xC00D7176);
        public const int MF_E_PE_SESSIONS_MAXED = unchecked((int)0xC00D7177);
        public const int MF_E_HIGH_SECURITY_LEVEL_CONTENT_NOT_ALLOWED = unchecked((int)0xC00D7178);
        public const int MF_E_TEST_SIGNED_COMPONENTS_NOT_ALLOWED = unchecked((int)0xC00D7179);
        public const int MF_E_ITA_UNSUPPORTED_ACTION = unchecked((int)0xC00D717A);
        public const int MF_E_ITA_ERROR_PARSING_SAP_PARAMETERS = unchecked((int)0xC00D717B);
        public const int MF_E_POLICY_MGR_ACTION_OUTOFBOUNDS = unchecked((int)0xC00D717C);
        public const int MF_E_BAD_OPL_STRUCTURE_FORMAT = unchecked((int)0xC00D717D);
        public const int MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_PROTECTION_GUID = unchecked((int)0xC00D717E);
        public const int MF_E_NO_PMP_HOST = unchecked((int)0xC00D717F);
        public const int MF_E_ITA_OPL_DATA_NOT_INITIALIZED = unchecked((int)0xC00D7180);
        public const int MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_OUTPUT = unchecked((int)0xC00D7181);
        public const int MF_E_ITA_UNRECOGNIZED_DIGITAL_VIDEO_OUTPUT = unchecked((int)0xC00D7182);
        public const int MF_E_CLOCK_INVALID_CONTINUITY_KEY = unchecked((int)0xC00D9C40);
        public const int MF_E_CLOCK_NO_TIME_SOURCE = unchecked((int)0xC00D9C41);
        public const int MF_E_CLOCK_STATE_ALREADY_SET = unchecked((int)0xC00D9C42);
        public const int MF_E_CLOCK_NOT_SIMPLE = unchecked((int)0xC00D9C43);
        public const int MF_S_CLOCK_STOPPED = unchecked((int)0x000D9C44);
        public const int MF_E_NO_MORE_DROP_MODES = unchecked((int)0xC00DA028);
        public const int MF_E_NO_MORE_QUALITY_LEVELS = unchecked((int)0xC00DA029);
        public const int MF_E_DROPTIME_NOT_SUPPORTED = unchecked((int)0xC00DA02A);
        public const int MF_E_QUALITYKNOB_WAIT_LONGER = unchecked((int)0xC00DA02B);
        public const int MF_E_QM_INVALIDSTATE = unchecked((int)0xC00DA02C);

        #endregion

        #region externs

        [DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int FormatMessage(FormatMessageFlags dwFlags, IntPtr lpSource,
            int dwMessageId, int dwLanguageId, ref IntPtr lpBuffer, int nSize, IntPtr Arguments);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryExFlags dwFlags);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        #endregion

        #region Declarations

        [Flags, UnmanagedName("#defines in WinBase.h")]
        private enum LoadLibraryExFlags
        {
            DontResolveDllReferences = 0x00000001,
            LoadLibraryAsDataFile = 0x00000002,
            LoadWithAlteredSearchPath = 0x00000008,
            LoadIgnoreCodeAuthzLevel = 0x00000010
        }

        [Flags, UnmanagedName("FORMAT_MESSAGE_* defines")]
        private enum FormatMessageFlags
        {
            AllocateBuffer = 0x00000100,
            IgnoreInserts = 0x00000200,
            FromString = 0x00000400,
            FromHmodule = 0x00000800,
            FromSystem = 0x00001000,
            ArgumentArray = 0x00002000,
            MaxWidthMask = 0x000000FF
        }

        #endregion

        private static IntPtr s_hModule = IntPtr.Zero;
        private const string MESSAGEFILE = "mferror.dll";

        /// <summary>
        /// Prevent people from trying to instantiate this class
        /// </summary>
        private MFError()
        {
            // Prevent people from trying to instantiate this class
        }

        /// <summary>
        /// Returns a string describing a MF error.  Works for both error codes
        /// (values < 0) and Status codes (values >= 0)
        /// </summary>
        /// <param name="hr">HRESULT for which to get description</param>
        /// <returns>The string, or null if no error text can be found</returns>
        public static string GetErrorText(int hr)
        {
            string sRet = null;
            int dwBufferLength;
            IntPtr ip = IntPtr.Zero;

            FormatMessageFlags dwFormatFlags =
                FormatMessageFlags.AllocateBuffer |
                FormatMessageFlags.IgnoreInserts |
                FormatMessageFlags.FromSystem |
                FormatMessageFlags.MaxWidthMask;

            // Scan both the Windows Media library, and the system library looking for the message
            dwBufferLength = FormatMessage(
                dwFormatFlags,
                s_hModule, // module to get message from (NULL == system)
                hr, // error number to get message for
                0, // default language
                ref ip,
                0,
                IntPtr.Zero
                );

            // Not a system message.  In theory, you should be able to get both with one call.  In practice (at
            // least on my 64bit box), you need to make 2 calls.
            if (dwBufferLength == 0)
            {
                if (s_hModule == IntPtr.Zero)
                {
                    // Load the Media Foundation error message dll
                    s_hModule = LoadLibraryEx(MESSAGEFILE, IntPtr.Zero, LoadLibraryExFlags.LoadLibraryAsDataFile);
                }

                if (s_hModule != IntPtr.Zero)
                {
                    // If the load succeeds, make sure we look in it
                    dwFormatFlags |= FormatMessageFlags.FromHmodule;

                    // Scan both the Windows Media library, and the system library looking for the message
                    dwBufferLength = FormatMessage(
                        dwFormatFlags,
                        s_hModule, // module to get message from (NULL == system)
                        hr, // error number to get message for
                        0, // default language
                        ref ip,
                        0,
                        IntPtr.Zero
                        );
                }
            }

            try
            {
                // Convert the returned buffer to a string.  If ip is null (due to not finding
                // the message), no exception is thrown.  sRet just stays null.  The
                // try/finally is for the (remote) possibility that we run out of memory
                // creating the string.
                sRet = Marshal.PtrToStringUni(ip);
            }
            finally
            {
                // Cleanup
                if (ip != IntPtr.Zero)
                {
                    LocalFree(ip);
                }
            }

            return sRet;
        }

        /// <summary>
        /// If hr has a "failed" status code (E_*), throw an exception.  Note that status
        /// messages (S_*) are not considered failure codes.  If MediaFoundation error text
        /// is available, it is used to build the exception, otherwise a generic com error
        /// is thrown.
        /// </summary>
        /// <param name="hr">The HRESULT to check</param>
        public static void ThrowExceptionForHR(int hr)
        {
            // If a severe error has occurred
            if (hr < 0)
            {
                string s = GetErrorText(hr);

                // If a string is returned, build a com error from it
                if (s != null)
                {
                    throw new COMException(s, hr);
                }
                else
                {
                    // No string, just use standard com error
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }
    }

    abstract public class COMBase
    {
        public const int S_Ok = 0;
        public const int S_False = 0;

        public const int E_NotImplemented = unchecked((int)0x80004001);
        public const int E_NoInterface = unchecked((int)0x80004002);
        public const int E_Pointer = unchecked((int)0x80004003);
        public const int E_Abort = unchecked((int)0x80004004);
        public const int E_Fail = unchecked((int)0x80004005);
        public const int E_Unexpected = unchecked((int)0x8000FFFF);
        public const int E_OutOfMemory = unchecked((int)0x8007000E);
        public const int E_InvalidArgument = unchecked((int)0x80070057);
        public const int E_BufferTooSmall = unchecked((int)0x8007007a);

        public static bool Succeeded(int hr)
        {
            return hr >= 0;
        }

        public static bool Failed(int hr)
        {
            return hr < 0;
        }

        public static void SafeRelease(object o)
        {
            if (o != null)
            {
                IDisposable id = o as IDisposable;
                if (id != null)
                {
                    id.Dispose();
                }
                else
                {
                    try
                    {
                        Marshal.ReleaseComObject(o);
                    }
                    catch { }
                }
            }
        }

        public static int ParseError(Exception e)
        {
            int hr;

            if (e is COMException)
            {
                COMException ce = e as COMException;
                hr = ce.ErrorCode;
            }
            else
            {
                const string TEXT = "(Exception from HRESULT: 0x";
                // 		Message	"The system cannot find the file specified. (Exception from HRESULT: 0x80070002)"
                int iPos = e.Message.LastIndexOf(TEXT);
                if (iPos < 0)
                {
                    hr = E_Fail;
                }
                else
                {
                    hr = int.Parse(e.Message.Substring(iPos + TEXT.Length, 8), System.Globalization.NumberStyles.AllowHexSpecifier);
                }
            }

            return hr;
        }
    }

    #endregion
}
