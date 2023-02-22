﻿/**************************************************************************
*                           MIT License
*
* Copyright (C) 2014 Morten Kvistgaard <mk@pch-engineering.dk>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/

/*
 * Ported from project at http://bacnet.sourceforge.net/
 */

using System.Collections.Generic;
using System.Text;
using System.IO.BACnet.Serialize;
using System.Net;
using System.Globalization;

namespace System.IO.BACnet
{
    /* note: these are not the real values, */
    /* but are shifted left for easy encoding */
    [Flags]
    public enum BacnetPduTypes : byte
    {
        PDU_TYPE_CONFIRMED_SERVICE_REQUEST = 0,
        SERVER = 1,
        NEGATIVE_ACK = 2,
        SEGMENTED_RESPONSE_ACCEPTED = 2,
        MORE_FOLLOWS = 4,
        SEGMENTED_MESSAGE = 8,
        PDU_TYPE_UNCONFIRMED_SERVICE_REQUEST = 0x10,
        PDU_TYPE_SIMPLE_ACK = 0x20,
        PDU_TYPE_COMPLEX_ACK = 0x30,
        PDU_TYPE_SEGMENT_ACK = 0x40,
        PDU_TYPE_ERROR = 0x50,
        PDU_TYPE_REJECT = 0x60,
        PDU_TYPE_ABORT = 0x70,
        PDU_TYPE_MASK = 0xF0,
    };

    public enum BacnetSegmentations
    {
        SEGMENTATION_BOTH = 0,
        SEGMENTATION_TRANSMIT = 1,
        SEGMENTATION_RECEIVE = 2,
        SEGMENTATION_NONE = 3,
    };

    public enum BacnetDeviceStatus : byte
    {
        OPERATIONAL = 0,
        OPERATIONAL_READONLY = 1,
        DOWNLOAD_REQUIRED = 2,
        DOWNLOAD_IN_PROGRESS = 3,
        NON_OPERATIONAL = 4,
        BACKUP_IN_PROGRESS = 5
    }

    public enum BACnetBackupState
    {
        IDLE = 0,
        PREPARING_FOR_BACKUP = 1,
        PREPARING_FOR_RESTORE = 2,
        PERFORMING_A_BACKUP = 3,
        PERFORMING_A_RESTORE = 4,
    }

    // From Loren Van Spronsen csharp-bacnet
    public enum BacnetRestartReason
    {
        UNKNOWN = 0,
        COLD_START = 1,
        WARM_START = 2,
        DETECTED_POWER_LOST = 3,
        DETECTED_POWER_OFF = 4,
        HARDWARE_WATCHDOG = 5,
        SOFTWARE_WATCHDOG = 6,
        SUSPENDED = 7
    }

    public enum BacnetProgramChange
    {
        READY = 0,
        LOAD = 1,
        RUN = 2,
        HALT = 3,
        RESTART = 4,
        UNLOAD = 5
    }
    public enum BacnetProgramState
    {
        IDLE = 0,
        LOADING = 1,
        RUNNING = 2,
        WAITING = 3,
        HALTED = 4,
        UNLOADING = 5
    }
    public enum BacnetReasonForHalt
    {
        NORMAL = 0,
        LOAD_FAILED = 1,
        INTERNAL = 2,
        PROGRAM = 3,
        OTHER = 4
    }

    [Flags]
    public enum BacnetResultFlags
    {
        NONE = 0,
        FIRST_ITEM = 1,
        LAST_ITEM = 2,
        MORE_ITEMS = 4,
    }

    public enum BacnetRejectReasons
    {
        REJECT_REASON_OTHER = 0,
        REJECT_REASON_BUFFER_OVERFLOW = 1,
        REJECT_REASON_INCONSISTENT_PARAMETERS = 2,
        REJECT_REASON_INVALID_PARAMETER_DATA_TYPE = 3,
        REJECT_REASON_INVALID_TAG = 4,
        REJECT_REASON_MISSING_REQUIRED_PARAMETER = 5,
        REJECT_REASON_PARAMETER_OUT_OF_RANGE = 6,
        REJECT_REASON_TOO_MANY_ARGUMENTS = 7,
        REJECT_REASON_UNDEFINED_ENUMERATION = 8,
        REJECT_REASON_UNRECOGNIZED_SERVICE = 9,
        /* Enumerated values 0-63 are reserved for definition by ASHRAE. */
        /* Enumerated values 64-255 may be used by others subject to */
        /* the procedures and constraints described in Clause 23. */
        MAX_BACNET_REJECT_REASON = 10,
        REJECT_REASON_PROPRIETARY_FIRST = 64,
        REJECT_REASON_PROPRIETARY_LAST = 255
    };

    public enum BacnetErrorClasses
    {
        ERROR_CLASS_DEVICE = 0,
        ERROR_CLASS_OBJECT = 1,
        ERROR_CLASS_PROPERTY = 2,
        ERROR_CLASS_RESOURCES = 3,
        ERROR_CLASS_SECURITY = 4,
        ERROR_CLASS_SERVICES = 5,
        ERROR_CLASS_VT = 6,
        ERROR_CLASS_COMMUNICATION = 7,
        /* Enumerated values 0-63 are reserved for definition by ASHRAE. */
        /* Enumerated values 64-65535 may be used by others subject to */
        /* the procedures and constraints described in Clause 23. */
        MAX_BACNET_ERROR_CLASS = 8,
        /* do the MAX here instead of outside of enum so that
           compilers will allocate adequate sized datatype for enum */
        ERROR_CLASS_PROPRIETARY_FIRST = 64,
        ERROR_CLASS_PROPRIETARY_LAST = 65535
    };

    /* These are sorted in the order given in
       Clause 18. ERROR, REJECT AND ABORT CODES
       The Class and Code pairings are required
       to be used in accordance with Clause 18. */
    public enum BacnetErrorCodes
    {
        /* valid for all classes */
        ERROR_CODE_OTHER = 0,

        /* Error Class - Device */
        ERROR_CODE_DEVICE_BUSY = 3,
        ERROR_CODE_CONFIGURATION_IN_PROGRESS = 2,
        ERROR_CODE_OPERATIONAL_PROBLEM = 25,

        /* Error Class - Object */
        ERROR_CODE_DYNAMIC_CREATION_NOT_SUPPORTED = 4,
        ERROR_CODE_NO_OBJECTS_OF_SPECIFIED_TYPE = 17,
        ERROR_CODE_OBJECT_DELETION_NOT_PERMITTED = 23,
        ERROR_CODE_OBJECT_IDENTIFIER_ALREADY_EXISTS = 24,
        ERROR_CODE_READ_ACCESS_DENIED = 27,
        ERROR_CODE_UNKNOWN_OBJECT = 31,
        ERROR_CODE_UNSUPPORTED_OBJECT_TYPE = 36,

        /* Error Class - Property */
        ERROR_CODE_CHARACTER_SET_NOT_SUPPORTED = 41,
        ERROR_CODE_DATATYPE_NOT_SUPPORTED = 47,
        ERROR_CODE_INCONSISTENT_SELECTION_CRITERION = 8,
        ERROR_CODE_INVALID_ARRAY_INDEX = 42,
        ERROR_CODE_INVALID_DATA_TYPE = 9,
        ERROR_CODE_NOT_COV_PROPERTY = 44,
        ERROR_CODE_OPTIONAL_FUNCTIONALITY_NOT_SUPPORTED = 45,
        ERROR_CODE_PROPERTY_IS_NOT_AN_ARRAY = 50,
        /* ERROR_CODE_READ_ACCESS_DENIED = 27, */
        ERROR_CODE_UNKNOWN_PROPERTY = 32,
        ERROR_CODE_VALUE_OUT_OF_RANGE = 37,
        ERROR_CODE_WRITE_ACCESS_DENIED = 40,

        /* Error Class - Resources */
        ERROR_CODE_NO_SPACE_FOR_OBJECT = 18,
        ERROR_CODE_NO_SPACE_TO_ADD_LIST_ELEMENT = 19,
        ERROR_CODE_NO_SPACE_TO_WRITE_PROPERTY = 20,

        /* Error Class - Security */
        ERROR_CODE_AUTHENTICATION_FAILED = 1,
        /* ERROR_CODE_CHARACTER_SET_NOT_SUPPORTED = 41, */
        ERROR_CODE_INCOMPATIBLE_SECURITY_LEVELS = 6,
        ERROR_CODE_INVALID_OPERATOR_NAME = 12,
        ERROR_CODE_KEY_GENERATION_ERROR = 15,
        ERROR_CODE_PASSWORD_FAILURE = 26,
        ERROR_CODE_SECURITY_NOT_SUPPORTED = 28,
        ERROR_CODE_TIMEOUT = 30,

        /* Error Class - Services */
        /* ERROR_CODE_CHARACTER_SET_NOT_SUPPORTED = 41, */
        ERROR_CODE_COV_SUBSCRIPTION_FAILED = 43,
        ERROR_CODE_DUPLICATE_NAME = 48,
        ERROR_CODE_DUPLICATE_OBJECT_ID = 49,
        ERROR_CODE_FILE_ACCESS_DENIED = 5,
        ERROR_CODE_INCONSISTENT_PARAMETERS = 7,
        ERROR_CODE_INVALID_CONFIGURATION_DATA = 46,
        ERROR_CODE_INVALID_FILE_ACCESS_METHOD = 10,
        ERROR_CODE_INVALID_FILE_START_POSITION = 11,
        ERROR_CODE_INVALID_PARAMETER_DATA_TYPE = 13,
        ERROR_CODE_INVALID_TIME_STAMP = 14,
        ERROR_CODE_MISSING_REQUIRED_PARAMETER = 16,
        /* ERROR_CODE_OPTIONAL_FUNCTIONALITY_NOT_SUPPORTED = 45, */
        ERROR_CODE_PROPERTY_IS_NOT_A_LIST = 22,
        ERROR_CODE_SERVICE_REQUEST_DENIED = 29,

        /* Error Class - VT */
        ERROR_CODE_UNKNOWN_VT_CLASS = 34,
        ERROR_CODE_UNKNOWN_VT_SESSION = 35,
        ERROR_CODE_NO_VT_SESSIONS_AVAILABLE = 21,
        ERROR_CODE_VT_SESSION_ALREADY_CLOSED = 38,
        ERROR_CODE_VT_SESSION_TERMINATION_FAILURE = 39,

        /* unused */
        ERROR_CODE_RESERVED1 = 33,
        /* new error codes from new addenda */
        ERROR_CODE_ABORT_BUFFER_OVERFLOW = 51,
        ERROR_CODE_ABORT_INVALID_APDU_IN_THIS_STATE = 52,
        ERROR_CODE_ABORT_PREEMPTED_BY_HIGHER_PRIORITY_TASK = 53,
        ERROR_CODE_ABORT_SEGMENTATION_NOT_SUPPORTED = 54,
        ERROR_CODE_ABORT_PROPRIETARY = 55,
        ERROR_CODE_ABORT_OTHER = 56,
        ERROR_CODE_INVALID_TAG = 57,
        ERROR_CODE_NETWORK_DOWN = 58,
        ERROR_CODE_REJECT_BUFFER_OVERFLOW = 59,
        ERROR_CODE_REJECT_INCONSISTENT_PARAMETERS = 60,
        ERROR_CODE_REJECT_INVALID_PARAMETER_DATA_TYPE = 61,
        ERROR_CODE_REJECT_INVALID_TAG = 62,
        ERROR_CODE_REJECT_MISSING_REQUIRED_PARAMETER = 63,
        ERROR_CODE_REJECT_PARAMETER_OUT_OF_RANGE = 64,
        ERROR_CODE_REJECT_TOO_MANY_ARGUMENTS = 65,
        ERROR_CODE_REJECT_UNDEFINED_ENUMERATION = 66,
        ERROR_CODE_REJECT_UNRECOGNIZED_SERVICE = 67,
        ERROR_CODE_REJECT_PROPRIETARY = 68,
        ERROR_CODE_REJECT_OTHER = 69,
        ERROR_CODE_UNKNOWN_DEVICE = 70,
        ERROR_CODE_UNKNOWN_ROUTE = 71,
        ERROR_CODE_VALUE_NOT_INITIALIZED = 72,
        ERROR_CODE_INVALID_EVENT_STATE = 73,
        ERROR_CODE_NO_ALARM_CONFIGURED = 74,
        ERROR_CODE_LOG_BUFFER_FULL = 75,
        ERROR_CODE_LOGGED_VALUE_PURGED = 76,
        ERROR_CODE_NO_PROPERTY_SPECIFIED = 77,
        ERROR_CODE_NOT_CONFIGURED_FOR_TRIGGERED_LOGGING = 78,
        ERROR_CODE_UNKNOWN_SUBSCRIPTION = 79,
        ERROR_CODE_PARAMETER_OUT_OF_RANGE = 80,
        ERROR_CODE_LIST_ELEMENT_NOT_FOUND = 81,
        ERROR_CODE_BUSY = 82,
        ERROR_CODE_COMMUNICATION_DISABLED = 83,
        ERROR_CODE_SUCCESS = 84,
        ERROR_CODE_ACCESS_DENIED = 85,
        ERROR_CODE_BAD_DESTINATION_ADDRESS = 86,
        ERROR_CODE_BAD_DESTINATION_DEVICE_ID = 87,
        ERROR_CODE_BAD_SIGNATURE = 88,
        ERROR_CODE_BAD_SOURCE_ADDRESS = 89,
        ERROR_CODE_BAD_TIMESTAMP = 90,
        ERROR_CODE_CANNOT_USE_KEY = 91,
        ERROR_CODE_CANNOT_VERIFY_MESSAGE_ID = 92,
        ERROR_CODE_CORRECT_KEY_REVISION = 93,
        ERROR_CODE_DESTINATION_DEVICE_ID_REQUIRED = 94,
        ERROR_CODE_DUPLICATE_MESSAGE = 95,
        ERROR_CODE_ENCRYPTION_NOT_CONFIGURED = 96,
        ERROR_CODE_ENCRYPTION_REQUIRED = 97,
        ERROR_CODE_INCORRECT_KEY = 98,
        ERROR_CODE_INVALID_KEY_DATA = 99,
        ERROR_CODE_KEY_UPDATE_IN_PROGRESS = 100,
        ERROR_CODE_MALFORMED_MESSAGE = 101,
        ERROR_CODE_NOT_KEY_SERVER = 102,
        ERROR_CODE_SECURITY_NOT_CONFIGURED = 103,
        ERROR_CODE_SOURCE_SECURITY_REQUIRED = 104,
        ERROR_CODE_TOO_MANY_KEYS = 105,
        ERROR_CODE_UNKNOWN_AUTHENTICATION_TYPE = 106,
        ERROR_CODE_UNKNOWN_KEY = 107,
        ERROR_CODE_UNKNOWN_KEY_REVISION = 108,
        ERROR_CODE_UNKNOWN_SOURCE_MESSAGE = 109,
        ERROR_CODE_NOT_ROUTER_TO_DNET = 110,
        ERROR_CODE_ROUTER_BUSY = 111,
        ERROR_CODE_UNKNOWN_NETWORK_MESSAGE = 112,
        ERROR_CODE_MESSAGE_TOO_LONG = 113,
        ERROR_CODE_SECURITY_ERROR = 114,
        ERROR_CODE_ADDRESSING_ERROR = 115,
        ERROR_CODE_WRITE_BDT_FAILED = 116,
        ERROR_CODE_READ_BDT_FAILED = 117,
        ERROR_CODE_REGISTER_FOREIGN_DEVICE_FAILED = 118,
        ERROR_CODE_READ_FDT_FAILED = 119,
        ERROR_CODE_DELETE_FDT_ENTRY_FAILED = 120,
        ERROR_CODE_DISTRIBUTE_BROADCAST_FAILED = 121,
        ERROR_CODE_UNKNOWN_FILE_SIZE = 122,
        ERROR_CODE_ABORT_APDU_TOO_LONG = 123,
        ERROR_CODE_ABORT_APPLICATION_EXCEEDED_REPLY_TIME = 124,
        ERROR_CODE_ABORT_OUT_OF_RESOURCES = 125,
        ERROR_CODE_ABORT_TSM_TIMEOUT = 126,
        ERROR_CODE_ABORT_WINDOW_SIZE_OUT_OF_RANGE = 127,
        ERROR_CODE_FILE_FULL = 128,
        ERROR_CODE_INCONSISTENT_CONFIGURATION = 129,
        ERROR_CODE_INCONSISTENT_OBJECT_TYPE = 130,
        ERROR_CODE_INTERNAL_ERROR = 131,
        ERROR_CODE_NOT_CONFIGURED = 132,
        ERROR_CODE_OUT_OF_MEMORY = 133,
        ERROR_CODE_VALUE_TOO_LONG = 134,
        ERROR_CODE_ABORT_INSUFFICIENT_SECURITY = 135,
        ERROR_CODE_ABORT_SECURITY_ERROR = 136,
        ERROR_CODE_DUPLICATE_ENTRY = 137,
        ERROR_CODE_INVALID_VALUE_IN_THIS_STATE = 138,
        ERROR_CODE_INVALID_OPERATION_IN_THIS_STATE = 139,
        ERROR_CODE_LIST_ITEM_NOT_NUMBERED = 140,
        ERROR_CODE_LIST_ITEM_NOT_TIMESTAMPED = 141,
        ERROR_CODE_INVALID_DATA_ENCODING = 142,
        ERROR_CODE_BVLC_FUNCTION_UNKNOWN = 143,
        ERROR_CODE_BVLC_PROPRIETARY_FUNCTION_UNKNOWN = 144,
        ERROR_CODE_HEADER_ENCODING_ERROR = 145,
        ERROR_CODE_HEADER_NOT_UNDERSTOOD = 146,
        ERROR_CODE_MESSAGE_INCOMPLETE = 147,
        ERROR_CODE_NOT_A_BACNET_SC_HUB = 148,
        ERROR_CODE_PAYLOAD_EXPECTED = 149,
        ERROR_CODE_UNEXPECTED_DATA = 150,
        ERROR_CODE_NODE_DUPLICATE_VMAC = 151,
        ERROR_CODE_HTTP_UNEXPECTED_RESPONSE_CODE = 152,
        ERROR_CODE_HTTP_NO_UPGRADE = 153,
        ERROR_CODE_HTTP_RESOURCE_NOT_LOCAL = 154,
        ERROR_CODE_HTTP_PROXY_AUTHENTICATION_FAILED = 155,
        ERROR_CODE_HTTP_RESPONSE_TIMEOUT = 156,
        ERROR_CODE_HTTP_RESPONSE_SYNTAX_ERROR = 157,
        ERROR_CODE_HTTP_RESPONSE_VALUE_ERROR = 158,
        ERROR_CODE_HTTP_RESPONSE_MISSING_HEADER = 159,
        ERROR_CODE_HTTP_WEBSOCKET_HEADER_ERROR = 160,
        ERROR_CODE_HTTP_UPGRADE_REQUIRED = 161,
        ERROR_CODE_HTTP_UPGRADE_ERROR = 162,
        ERROR_CODE_HTTP_TEMPORARY_UNAVAILABLE = 163,
        ERROR_CODE_HTTP_NOT_A_SERVER = 164,
        ERROR_CODE_HTTP_ERROR = 165,
        ERROR_CODE_WEBSOCKET_SCHEME_NOT_SUPPORTED = 166,
        ERROR_CODE_WEBSOCKET_UNKNOWN_CONTROL_MESSAGE = 167,
        ERROR_CODE_WEBSOCKET_CLOSE_ERROR = 168,
        ERROR_CODE_WEBSOCKET_CLOSED_BY_PEER = 169,
        ERROR_CODE_WEBSOCKET_ENDPOINT_LEAVES = 170,
        ERROR_CODE_WEBSOCKET_PROTOCOL_ERROR = 171,
        ERROR_CODE_WEBSOCKET_DATA_NOT_ACCEPTED = 172,
        ERROR_CODE_WEBSOCKET_CLOSED_ABNORMALLY = 173,
        ERROR_CODE_WEBSOCKET_DATA_INCONSISTENT = 174,
        ERROR_CODE_WEBSOCKET_DATA_AGAINST_POLICY = 175,
        ERROR_CODE_WEBSOCKET_FRAME_TOO_LONG = 176,
        ERROR_CODE_WEBSOCKET_EXTENSION_MISSING = 177,
        ERROR_CODE_WEBSOCKET_REQUEST_UNAVAILABLE = 178,
        ERROR_CODE_WEBSOCKET_ERROR = 179,
        ERROR_CODE_TLS_CLIENT_CERTIFICATE_ERROR = 180,
        ERROR_CODE_TLS_SERVER_CERTIFICATE_ERROR = 181,
        ERROR_CODE_TLS_CLIENT_AUTHENTICATION_FAILED = 182,
        ERROR_CODE_TLS_SERVER_AUTHENTICATION_FAILED = 183,
        ERROR_CODE_TLS_CLIENT_CERTIFICATE_EXPIRED = 184,
        ERROR_CODE_TLS_SERVER_CERTIFICATE_EXPIRED = 185,
        ERROR_CODE_TLS_CLIENT_CERTIFICATE_REVOKED = 186,
        ERROR_CODE_TLS_SERVER_CERTIFICATE_REVOKED = 187,
        ERROR_CODE_TLS_ERROR = 188,
        ERROR_CODE_DNS_UNAVAILABLE = 189,
        ERROR_CODE_DNS_NAME_RESOLUTION_FAILED = 190,
        ERROR_CODE_DNS_RESOLVER_FAILURE = 191,
        ERROR_CODE_DNS_ERROR = 192,
        ERROR_CODE_TCP_CONNECT_TIMEOUT = 193,
        ERROR_CODE_TCP_CONNECTION_REFUSED = 194,
        ERROR_CODE_TCP_CLOSED_BY_LOCAL = 195,
        ERROR_CODE_TCP_CLOSED_OTHER = 196,
        ERROR_CODE_TCP_ERROR = 197,
        ERROR_CODE_IP_ADDRESS_NOT_REACHABLE = 198,
        ERROR_CODE_IP_ERROR = 199,
        ERROR_CODE_CERTIFICATE_EXPIRED = 200,
        ERROR_CODE_CERTIFICATE_INVALID = 201,
        ERROR_CODE_CERTIFICATE_MALFORMED = 202,
        ERROR_CODE_CERTIFICATE_REVOKED = 203,
        ERROR_CODE_UNKNOWN_SECURITY_KEY = 204,
        ERROR_CODE_REFERENCED_PORT_IN_ERROR = 205,
        MAX_BACNET_ERROR_CODE = 206,

        /* Enumerated values 0-255 are reserved for definition by ASHRAE. */
        /* Enumerated values 256-65535 may be used by others subject to */
        /* the procedures and constraints described in Clause 23. */
        /* do the max range inside of enum so that
           compilers will allocate adequate sized datatype for enum
           which is used to store decoding */
        ERROR_CODE_PROPRIETARY_FIRST = 256,
        ERROR_CODE_PROPRIETARY_LAST = 65535
    };

    [Flags]
    public enum BacnetStatusFlags
    {
        STATUS_FLAG_IN_ALARM = 1,
        STATUS_FLAG_FAULT = 2,
        STATUS_FLAG_OVERRIDDEN = 4,
        STATUS_FLAG_OUT_OF_SERVICE = 8,
    };

    public enum BacnetServicesSupported
    {
        /* Alarm and Event Services */
        SERVICE_SUPPORTED_ACKNOWLEDGE_ALARM = 0,
        SERVICE_SUPPORTED_CONFIRMED_COV_NOTIFICATION = 1,
        //SERVICE_SUPPORTED_CONFIRMED_COV_NOTIFICATION_MULTIPLE = 42,
        SERVICE_SUPPORTED_CONFIRMED_EVENT_NOTIFICATION = 2,
        SERVICE_SUPPORTED_GET_ALARM_SUMMARY = 3,
        SERVICE_SUPPORTED_GET_ENROLLMENT_SUMMARY = 4,
        SERVICE_SUPPORTED_GET_EVENT_INFORMATION = 39,
        SERVICE_SUPPORTED_LIFE_SAFETY_OPERATION = 37,
        SERVICE_SUPPORTED_SUBSCRIBE_COV = 5,
        SERVICE_SUPPORTED_SUBSCRIBE_COV_PROPERTY = 38,
        //SERVICE_SUPPORTED_SUBSCRIBE_COV_PROPERTY_MULTIPLE = 41,
        /* File Access Services */
        SERVICE_SUPPORTED_ATOMIC_READ_FILE = 6,
        SERVICE_SUPPORTED_ATOMIC_WRITE_FILE = 7,
        /* Object Access Services */
        SERVICE_SUPPORTED_ADD_LIST_ELEMENT = 8,
        SERVICE_SUPPORTED_REMOVE_LIST_ELEMENT = 9,
        SERVICE_SUPPORTED_CREATE_OBJECT = 10,
        SERVICE_SUPPORTED_DELETE_OBJECT = 11,
        SERVICE_SUPPORTED_READ_PROPERTY = 12,
        SERVICE_SUPPORTED_READ_PROP_CONDITIONAL = 13,//removed in version 1 revision 12
        SERVICE_SUPPORTED_READ_PROP_MULTIPLE = 14,
        SERVICE_SUPPORTED_READ_RANGE = 35,
        SERVICE_SUPPORTED_WRITE_GROUP = 40,
        SERVICE_SUPPORTED_WRITE_PROPERTY = 15,
        SERVICE_SUPPORTED_WRITE_PROP_MULTIPLE = 16,
        /* Remote Device Management Services */
        SERVICE_SUPPORTED_DEVICE_COMMUNICATION_CONTROL = 17,
        SERVICE_SUPPORTED_PRIVATE_TRANSFER = 18,
        SERVICE_SUPPORTED_TEXT_MESSAGE = 19,
        SERVICE_SUPPORTED_REINITIALIZE_DEVICE = 20,
        /* Virtual Terminal Services */
        SERVICE_SUPPORTED_VT_OPEN = 21,
        SERVICE_SUPPORTED_VT_CLOSE = 22,
        SERVICE_SUPPORTED_VT_DATA = 23,
        /* Security Services */
        SERVICE_SUPPORTED_AUTHENTICATE = 24, //removed in version 1 revision 11
        SERVICE_SUPPORTED_REQUEST_KEY = 25, //removed in version 1 revision 11
        SERVICE_SUPPORTED_I_AM = 26,
        SERVICE_SUPPORTED_I_HAVE = 27,
        SERVICE_SUPPORTED_UNCONFIRMED_COV_NOTIFICATION = 28,
        //SERVICE_SUPPORTED_UNCONFIRMED_COV_NOTIFICATION_MULTIPLE = 43,
        SERVICE_SUPPORTED_UNCONFIRMED_EVENT_NOTIFICATION = 29,
        SERVICE_SUPPORTED_UNCONFIRMED_PRIVATE_TRANSFER = 30,
        SERVICE_SUPPORTED_UNCONFIRMED_TEXT_MESSAGE = 31,
        SERVICE_SUPPORTED_TIME_SYNCHRONIZATION = 32,
        SERVICE_SUPPORTED_UTC_TIME_SYNCHRONIZATION = 36,
        SERVICE_SUPPORTED_WHO_HAS = 33,
        SERVICE_SUPPORTED_WHO_IS = 34,
        /* Other services to be added as they are defined. */
        /* All values in this production are reserved */
        /* for definition by ASHRAE. */
        MAX_BACNET_SERVICES_SUPPORTED = 41,
    };

    public enum BacnetUnconfirmedServices : byte
    {
        SERVICE_UNCONFIRMED_I_AM = 0,
        SERVICE_UNCONFIRMED_I_HAVE = 1,
        SERVICE_UNCONFIRMED_COV_NOTIFICATION = 2,
        SERVICE_UNCONFIRMED_EVENT_NOTIFICATION = 3,
        SERVICE_UNCONFIRMED_PRIVATE_TRANSFER = 4,
        SERVICE_UNCONFIRMED_TEXT_MESSAGE = 5,
        SERVICE_UNCONFIRMED_TIME_SYNCHRONIZATION = 6,
        SERVICE_UNCONFIRMED_WHO_HAS = 7,
        SERVICE_UNCONFIRMED_WHO_IS = 8,
        SERVICE_UNCONFIRMED_UTC_TIME_SYNCHRONIZATION = 9,
        /* addendum 2010-aa */
        SERVICE_UNCONFIRMED_WRITE_GROUP = 10,
        /* Other services to be added as they are defined. */
        /* All choice values in this production are reserved */
        /* for definition by ASHRAE. */
        /* Proprietary extensions are made by using the */
        /* UnconfirmedPrivateTransfer service. See Clause 23. */
        UNCONFIRMED_COV_NOTIFICATION_MULTIPLE = 11,
        MAX_BACNET_UNCONFIRMED_SERVICE = 12,
    };

    public enum BacnetConfirmedServices : byte
    {
        /* Alarm and Event Services */
        SERVICE_CONFIRMED_ACKNOWLEDGE_ALARM = 0,
        SERVICE_CONFIRMED_COV_NOTIFICATION = 1,
        SERVICE_CONFIRMED_EVENT_NOTIFICATION = 2,
        SERVICE_CONFIRMED_GET_ALARM_SUMMARY = 3,
        SERVICE_CONFIRMED_GET_ENROLLMENT_SUMMARY = 4,
        SERVICE_CONFIRMED_GET_EVENT_INFORMATION = 29,
        SERVICE_CONFIRMED_SUBSCRIBE_COV = 5,
        SERVICE_CONFIRMED_SUBSCRIBE_COV_PROPERTY = 28,
        SERVICE_CONFIRMED_LIFE_SAFETY_OPERATION = 27,
        SERVICE_SUBSCRIBE_COV_PROPERTY_MULTIPLE = 30,
        SERVICE_CONFIRMED_COV_NOTIFICATION_MULTIPLE = 31,
        /* File Access Services */
        SERVICE_CONFIRMED_ATOMIC_READ_FILE = 6,
        SERVICE_CONFIRMED_ATOMIC_WRITE_FILE = 7,
        /* Object Access Services */
        SERVICE_CONFIRMED_ADD_LIST_ELEMENT = 8,
        SERVICE_CONFIRMED_REMOVE_LIST_ELEMENT = 9,
        SERVICE_CONFIRMED_CREATE_OBJECT = 10,
        SERVICE_CONFIRMED_DELETE_OBJECT = 11,
        SERVICE_CONFIRMED_READ_PROPERTY = 12,
        SERVICE_CONFIRMED_READ_PROP_CONDITIONAL = 13, //removed in version 1 revision 12
        SERVICE_CONFIRMED_READ_PROP_MULTIPLE = 14,
        SERVICE_CONFIRMED_READ_RANGE = 26,
        SERVICE_CONFIRMED_WRITE_PROPERTY = 15,
        SERVICE_CONFIRMED_WRITE_PROP_MULTIPLE = 16,
        /* Remote Device Management Services */
        SERVICE_CONFIRMED_DEVICE_COMMUNICATION_CONTROL = 17,
        SERVICE_CONFIRMED_PRIVATE_TRANSFER = 18,
        SERVICE_CONFIRMED_TEXT_MESSAGE = 19,
        SERVICE_CONFIRMED_REINITIALIZE_DEVICE = 20,
        /* Virtual Terminal Services */
        SERVICE_CONFIRMED_VT_OPEN = 21,
        SERVICE_CONFIRMED_VT_CLOSE = 22,
        SERVICE_CONFIRMED_VT_DATA = 23,
        /* Security Services */
        SERVICE_CONFIRMED_AUTHENTICATE = 24, //removed in version 1 revision 11
        SERVICE_CONFIRMED_REQUEST_KEY = 25,  //removed in version 1 revision 11
        /* Services added after 1995 */
        /* readRange (26) see Object Access Services */
        /* lifeSafetyOperation (27) see Alarm and Event Services */
        /* subscribeCOVProperty (28) see Alarm and Event Services */
        /* getEventInformation (29) see Alarm and Event Services */

        /* Services added after 2012
         * -- subscribe-cov-property-multiple [30] see Alarm and Event Services
           -- confirmed-cov-notification-multiple [31] see Alarm and Event Services*/

        MAX_BACNET_CONFIRMED_SERVICE = 32
    };

    // Add FC : from Karg's Stack
    public enum BacnetUnitsId
    {
        UNITS_METERS_PER_SECOND_PER_SECOND = 166,
        /* Area */
        UNITS_SQUARE_METERS = 0,
        UNITS_SQUARE_CENTIMETERS = 116,
        UNITS_SQUARE_FEET = 1,
        UNITS_SQUARE_INCHES = 115,
        /* Currency */
        UNITS_CURRENCY1 = 105,
        UNITS_CURRENCY2 = 106,
        UNITS_CURRENCY3 = 107,
        UNITS_CURRENCY4 = 108,
        UNITS_CURRENCY5 = 109,
        UNITS_CURRENCY6 = 110,
        UNITS_CURRENCY7 = 111,
        UNITS_CURRENCY8 = 112,
        UNITS_CURRENCY9 = 113,
        UNITS_CURRENCY10 = 114,
        /* Electrical */
        UNITS_MILLIAMPERES = 2,
        UNITS_AMPERES = 3,
        UNITS_AMPERES_PER_METER = 167,
        UNITS_AMPERES_PER_SQUARE_METER = 168,
        UNITS_AMPERE_SQUARE_METERS = 169,
        UNITS_DECIBELS = 199,
        UNITS_DECIBELS_MILLIVOLT = 200,
        UNITS_DECIBELS_VOLT = 201,
        UNITS_FARADS = 170,
        UNITS_HENRYS = 171,
        UNITS_OHMS = 4,
        UNITS_OHM_METERS = 172,
        UNITS_MILLIOHMS = 145,
        UNITS_KILOHMS = 122,
        UNITS_MEGOHMS = 123,
        UNITS_MICROSIEMENS = 190,
        UNITS_MILLISIEMENS = 202,
        UNITS_SIEMENS = 173,        /* 1 mho equals 1 siemens */
        UNITS_SIEMENS_PER_METER = 174,
        UNITS_TESLAS = 175,
        UNITS_VOLTS = 5,
        UNITS_MILLIVOLTS = 124,
        UNITS_KILOVOLTS = 6,
        UNITS_MEGAVOLTS = 7,
        UNITS_VOLT_AMPERES = 8,
        UNITS_KILOVOLT_AMPERES = 9,
        UNITS_MEGAVOLT_AMPERES = 10,
        UNITS_VOLT_AMPERES_REACTIVE = 11,
        UNITS_KILOVOLT_AMPERES_REACTIVE = 12,
        UNITS_MEGAVOLT_AMPERES_REACTIVE = 13,
        UNITS_VOLTS_PER_DEGREE_KELVIN = 176,
        UNITS_VOLTS_PER_METER = 177,
        UNITS_DEGREES_PHASE = 14,
        UNITS_POWER_FACTOR = 15,
        UNITS_WEBERS = 178,
        /* Energy */
        UNITS_JOULES = 16,
        UNITS_KILOJOULES = 17,
        UNITS_KILOJOULES_PER_KILOGRAM = 125,
        UNITS_MEGAJOULES = 126,
        UNITS_WATT_HOURS = 18,
        UNITS_KILOWATT_HOURS = 19,
        UNITS_MEGAWATT_HOURS = 146,
        UNITS_WATT_HOURS_REACTIVE = 203,
        UNITS_KILOWATT_HOURS_REACTIVE = 204,
        UNITS_MEGAWATT_HOURS_REACTIVE = 205,
        UNITS_BTUS = 20,
        UNITS_KILO_BTUS = 147,
        UNITS_MEGA_BTUS = 148,
        UNITS_THERMS = 21,
        UNITS_TON_HOURS = 22,
        /* Enthalpy */
        UNITS_JOULES_PER_KILOGRAM_DRY_AIR = 23,
        UNITS_KILOJOULES_PER_KILOGRAM_DRY_AIR = 149,
        UNITS_MEGAJOULES_PER_KILOGRAM_DRY_AIR = 150,
        UNITS_BTUS_PER_POUND_DRY_AIR = 24,
        UNITS_BTUS_PER_POUND = 117,
        /* Entropy */
        UNITS_JOULES_PER_DEGREE_KELVIN = 127,
        UNITS_KILOJOULES_PER_DEGREE_KELVIN = 151,
        UNITS_MEGAJOULES_PER_DEGREE_KELVIN = 152,
        UNITS_JOULES_PER_KILOGRAM_DEGREE_KELVIN = 128,
        /* Force */
        UNITS_NEWTON = 153,
        /* Frequency */
        UNITS_CYCLES_PER_HOUR = 25,
        UNITS_CYCLES_PER_MINUTE = 26,
        UNITS_HERTZ = 27,
        UNITS_KILOHERTZ = 129,
        UNITS_MEGAHERTZ = 130,
        UNITS_PER_HOUR = 131,
        /* Humidity */
        UNITS_GRAMS_OF_WATER_PER_KILOGRAM_DRY_AIR = 28,
        UNITS_PERCENT_RELATIVE_HUMIDITY = 29,
        /* Length */
        UNITS_MICROMETERS = 194,
        UNITS_MILLIMETERS = 30,
        UNITS_CENTIMETERS = 118,
        UNITS_KILOMETERS = 193,
        UNITS_METERS = 31,
        UNITS_INCHES = 32,
        UNITS_FEET = 33,
        /* Light */
        UNITS_CANDELAS = 179,
        UNITS_CANDELAS_PER_SQUARE_METER = 180,
        UNITS_WATTS_PER_SQUARE_FOOT = 34,
        UNITS_WATTS_PER_SQUARE_METER = 35,
        UNITS_LUMENS = 36,
        UNITS_LUXES = 37,
        UNITS_FOOT_CANDLES = 38,
        /* Mass */
        UNITS_MILLIGRAMS = 196,
        UNITS_GRAMS = 195,
        UNITS_KILOGRAMS = 39,
        UNITS_POUNDS_MASS = 40,
        UNITS_TONS = 41,
        /* Mass Flow */
        UNITS_GRAMS_PER_SECOND = 154,
        UNITS_GRAMS_PER_MINUTE = 155,
        UNITS_KILOGRAMS_PER_SECOND = 42,
        UNITS_KILOGRAMS_PER_MINUTE = 43,
        UNITS_KILOGRAMS_PER_HOUR = 44,
        UNITS_POUNDS_MASS_PER_SECOND = 119,
        UNITS_POUNDS_MASS_PER_MINUTE = 45,
        UNITS_POUNDS_MASS_PER_HOUR = 46,
        UNITS_TONS_PER_HOUR = 156,
        /* Power */
        UNITS_MILLIWATTS = 132,
        UNITS_WATTS = 47,
        UNITS_KILOWATTS = 48,
        UNITS_MEGAWATTS = 49,
        UNITS_BTUS_PER_HOUR = 50,
        UNITS_KILO_BTUS_PER_HOUR = 157,
        UNITS_HORSEPOWER = 51,
        UNITS_TONS_REFRIGERATION = 52,
        /* Pressure */
        UNITS_PASCALS = 53,
        UNITS_HECTOPASCALS = 133,
        UNITS_KILOPASCALS = 54,
        UNITS_MILLIBARS = 134,
        UNITS_BARS = 55,
        UNITS_POUNDS_FORCE_PER_SQUARE_INCH = 56,
        UNITS_MILLIMETERS_OF_WATER = 206,
        UNITS_CENTIMETERS_OF_WATER = 57,
        UNITS_INCHES_OF_WATER = 58,
        UNITS_MILLIMETERS_OF_MERCURY = 59,
        UNITS_CENTIMETERS_OF_MERCURY = 60,
        UNITS_INCHES_OF_MERCURY = 61,
        /* Temperature */
        UNITS_DEGREES_CELSIUS = 62,
        UNITS_DEGREES_KELVIN = 63,
        UNITS_DEGREES_KELVIN_PER_HOUR = 181,
        UNITS_DEGREES_KELVIN_PER_MINUTE = 182,
        UNITS_DEGREES_FAHRENHEIT = 64,
        UNITS_DEGREE_DAYS_CELSIUS = 65,
        UNITS_DEGREE_DAYS_FAHRENHEIT = 66,
        UNITS_DELTA_DEGREES_FAHRENHEIT = 120,
        UNITS_DELTA_DEGREES_KELVIN = 121,
        /* Time */
        UNITS_YEARS = 67,
        UNITS_MONTHS = 68,
        UNITS_WEEKS = 69,
        UNITS_DAYS = 70,
        UNITS_HOURS = 71,
        UNITS_MINUTES = 72,
        UNITS_SECONDS = 73,
        UNITS_HUNDREDTHS_SECONDS = 158,
        UNITS_MILLISECONDS = 159,
        /* Torque */
        UNITS_NEWTON_METERS = 160,
        /* Velocity */
        UNITS_MILLIMETERS_PER_SECOND = 161,
        UNITS_MILLIMETERS_PER_MINUTE = 162,
        UNITS_METERS_PER_SECOND = 74,
        UNITS_METERS_PER_MINUTE = 163,
        UNITS_METERS_PER_HOUR = 164,
        UNITS_KILOMETERS_PER_HOUR = 75,
        UNITS_FEET_PER_SECOND = 76,
        UNITS_FEET_PER_MINUTE = 77,
        UNITS_MILES_PER_HOUR = 78,
        /* Volume */
        UNITS_CUBIC_FEET = 79,
        UNITS_CUBIC_METERS = 80,
        UNITS_IMPERIAL_GALLONS = 81,
        UNITS_MILLILITERS = 197,
        UNITS_LITERS = 82,
        UNITS_US_GALLONS = 83,
        /* Volumetric Flow */
        UNITS_CUBIC_FEET_PER_SECOND = 142,
        UNITS_CUBIC_FEET_PER_MINUTE = 84,
        // One unit in Addendum 135-2012bg
        UNITS_MILLION_CUBIC_FEET_PER_MINUTE = 254,
        UNITS_CUBIC_FEET_PER_HOUR = 191,
        // five units in Addendum 135-2012bg
        UNITS_STANDARD_CUBIC_FEET_PER_DAY = 47808,
        UNITS_MILLION_STANDARD_CUBIC_FEET_PER_DAY = 47809,
        UNITS_THOUSAND_CUBIC_FEET_PER_DAY = 47810,
        UNITS_THOUSAND_STANDARD_CUBIC_FEET_PER_DAY = 47811,
        UNITS_POUNDS_MASS_PER_DAY = 47812,
        UNITS_CUBIC_METERS_PER_SECOND = 85,
        UNITS_CUBIC_METERS_PER_MINUTE = 165,
        UNITS_CUBIC_METERS_PER_HOUR = 135,
        UNITS_IMPERIAL_GALLONS_PER_MINUTE = 86,
        UNITS_MILLILITERS_PER_SECOND = 198,
        UNITS_LITERS_PER_SECOND = 87,
        UNITS_LITERS_PER_MINUTE = 88,
        UNITS_LITERS_PER_HOUR = 136,
        UNITS_US_GALLONS_PER_MINUTE = 89,
        UNITS_US_GALLONS_PER_HOUR = 192,
        /* Other */
        UNITS_DEGREES_ANGULAR = 90,
        UNITS_DEGREES_CELSIUS_PER_HOUR = 91,
        UNITS_DEGREES_CELSIUS_PER_MINUTE = 92,
        UNITS_DEGREES_FAHRENHEIT_PER_HOUR = 93,
        UNITS_DEGREES_FAHRENHEIT_PER_MINUTE = 94,
        UNITS_JOULE_SECONDS = 183,
        UNITS_KILOGRAMS_PER_CUBIC_METER = 186,
        UNITS_KW_HOURS_PER_SQUARE_METER = 137,
        UNITS_KW_HOURS_PER_SQUARE_FOOT = 138,
        UNITS_MEGAJOULES_PER_SQUARE_METER = 139,
        UNITS_MEGAJOULES_PER_SQUARE_FOOT = 140,
        UNITS_NO_UNITS = 95,
        UNITS_NEWTON_SECONDS = 187,
        UNITS_NEWTONS_PER_METER = 188,
        UNITS_PARTS_PER_MILLION = 96,
        UNITS_PARTS_PER_BILLION = 97,
        UNITS_PERCENT = 98,
        UNITS_PERCENT_OBSCURATION_PER_FOOT = 143,
        UNITS_PERCENT_OBSCURATION_PER_METER = 144,
        UNITS_PERCENT_PER_SECOND = 99,
        UNITS_PER_MINUTE = 100,
        UNITS_PER_SECOND = 101,
        UNITS_PSI_PER_DEGREE_FAHRENHEIT = 102,
        UNITS_RADIANS = 103,
        UNITS_RADIANS_PER_SECOND = 184,
        UNITS_REVOLUTIONS_PER_MINUTE = 104,
        UNITS_SQUARE_METERS_PER_NEWTON = 185,
        UNITS_WATTS_PER_METER_PER_DEGREE_KELVIN = 189,
        UNITS_WATTS_PER_SQUARE_METER_DEGREE_KELVIN = 141,
        UNITS_PER_MILLE = 207,
        UNITS_GRAMS_PER_GRAM = 208,
        UNITS_KILOGRAMS_PER_KILOGRAM = 209,
        UNITS_GRAMS_PER_KILOGRAM = 210,
        UNITS_MILLIGRAMS_PER_GRAM = 211,
        UNITS_MILLIGRAMS_PER_KILOGRAM = 212,
        UNITS_GRAMS_PER_MILLILITER = 213,
        UNITS_GRAMS_PER_LITER = 214,
        UNITS_MILLIGRAMS_PER_LITER = 215,
        UNITS_MICROGRAMS_PER_LITER = 216,
        UNITS_GRAMS_PER_CUBIC_METER = 217,
        UNITS_MILLIGRAMS_PER_CUBIC_METER = 218,
        UNITS_MICROGRAMS_PER_CUBIC_METER = 219,
        UNITS_NANOGRAMS_PER_CUBIC_METER = 220,
        UNITS_GRAMS_PER_CUBIC_CENTIMETER = 221,
        UNITS_BECQUERELS = 222,
        UNITS_KILOBECQUERELS = 223,
        UNITS_MEGABECQUERELS = 224,
        UNITS_GRAY = 225,
        UNITS_MILLIGRAY = 226,
        UNITS_MICROGRAY = 227,
        UNITS_SIEVERTS = 228,
        UNITS_MILLISIEVERTS = 229,
        UNITS_MICROSIEVERTS = 230,
        UNITS_MICROSIEVERTS_PER_HOUR = 231,
        // two units in Addendum 135-2012bg
        UNITS_MILLIREMS = 47814,
        UNITS_MILLIREMS_PER_HOUR = 47815,
        UNITS_DECIBELS_A = 232,
        UNITS_NEPHELOMETRIC_TURBIDITY_UNIT = 233,
        UNITS_PH = 234,
        UNITS_GRAMS_PER_SQUARE_METER = 235,
        // Since Addendum 135-2012ar
        UNITS_MINUTES_PER_DEGREE_KELVIN = 236,
        UNITS_METER_SQUARED_PER_METER = 237,
        UNITS_AMPERE_SECONDS = 238,
        UNITS_VOLT_AMPERE_HOURS = 239,
        UNITS_KILOVOLT_AMPERE_HOURS = 240,
        UNITS_MEGAVOLT_AMPERE_HOURS = 241,
        UNITS_VOLT_AMPERE_HOURS_REACTIVE = 242,
        UNITS_KILOVOLT_AMPERE_HOURS_REACTIVE = 243,
        UNITS_MEGAVOLT_AMPERE_HOURS_REACTIVE = 244,
        UNITS_VOLT_SQUARE_HOURS = 245,
        UNITS_AMPERE_SQUARE_HOURS = 246,
        UNITS_JOULE_PER_HOURS = 247,
        UNITS_CUBIC_FEET_PER_DAY = 248,
        UNITS_CUBIC_METERS_PER_DAY = 249,
        UNITS_WATT_HOURS_PER_CUBIC_METER = 250,
        UNITS_JOULES_PER_CUBIC_METER = 251,
        UNITS_MOLE_PERCENT = 252,
        UNITS_PASCAL_SECONDS = 253,
        UNITS_MILLION_STANDARD_CUBIC_FEET_PER_MINUTE = 254
        /*-- Enumerated values 0-255 and 47808-49999 are reserved for definition by ASHRAE. Enumerated values
          -- 256-47807 and 50000-65535 may be used by others subject to the procedures and constraints described
          -- in Clause 23.*/

    }

    public enum BacnetPolarity : byte
    {
        POLARITY_NORMAL = 0,
        POLARITY_REVERSE = 1
    }

    public enum BacnetReliability : uint
    {
        RELIABILITY_NO_FAULT_DETECTED = 0,
        RELIABILITY_NO_SENSOR = 1,
        RELIABILITY_OVER_RANGE = 2,
        RELIABILITY_UNDER_RANGE = 3,
        RELIABILITY_OPEN_LOOP = 4,
        RELIABILITY_SHORTED_LOOP = 5,
        RELIABILITY_NO_OUTPUT = 6,
        RELIABILITY_UNRELIABLE_OTHER = 7,
        RELIABILITY_PROCESS_ERROR = 8,
        RELIABILITY_MULTI_STATE_FAULT = 9,
        RELIABILITY_CONFIGURATION_ERROR = 10,
        //-- enumeration value 11 is reserved for a future addendum
        RELIABILITY_COMMUNICATION_FAILURE = 12,
        RELIABILITY_MEMBER_FAULT = 13,
        RELIABILITY_MONITORED_OBJECT_FAULT = 14,
        RELIABILITY_TRIPPED = 15,
        RELIABILITY_LAMP_FAILURE = 16,
        RELIABILITY_ACTIVATION_FAILURE = 17,
        RELIABILITY_RENEW_DHCP_FAILURE = 18,
        RELIABILITY_RENEW_FD_REGISTRATION_FAILURE = 19,
        RELIABILITY_RESTART_AUTO_NEGOTIATION_FAILURE = 20,
        RELIABILITY_RESTART_FAILURE = 21,
        RELIABILITY_PROPRIETARY_COMMAND_FAILURE = 22,
        RELIABILITY_FAULTS_LISTED = 23,
        RELIABILITY_REFERENCED_OBJECT_FAULT = 24
        /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
        /* Enumerated values 64-65535 may be used by others subject to  */
        /* the procedures and constraints described in Clause 23. */
        /* do the max range inside of enum so that
           compilers will allocate adequate sized datatype for enum
           which is used to store decoding */
    };

    public enum BacnetMaxSegments : byte
    {
        MAX_SEG0 = 0,
        MAX_SEG2 = 0x10,
        MAX_SEG4 = 0x20,
        MAX_SEG8 = 0x30,
        MAX_SEG16 = 0x40,
        MAX_SEG32 = 0x50,
        MAX_SEG64 = 0x60,
        MAX_SEG65 = 0x70,
    };

    public enum BacnetMaxAdpu : byte
    {
        MAX_APDU50 = 0,
        MAX_APDU128 = 1,
        MAX_APDU206 = 2,
        MAX_APDU480 = 3,
        MAX_APDU1024 = 4,
        MAX_APDU1476 = 5,
    };

    public enum BacnetObjectTypes
    {
        OBJECT_ANALOG_INPUT = 0,
        OBJECT_ANALOG_OUTPUT = 1,
        OBJECT_ANALOG_VALUE = 2,
        OBJECT_BINARY_INPUT = 3,
        OBJECT_BINARY_OUTPUT = 4,
        OBJECT_BINARY_VALUE = 5,
        OBJECT_CALENDAR = 6,
        OBJECT_COMMAND = 7,
        OBJECT_DEVICE = 8,
        OBJECT_EVENT_ENROLLMENT = 9,
        OBJECT_FILE = 10,
        OBJECT_GROUP = 11,
        OBJECT_LOOP = 12,
        OBJECT_MULTI_STATE_INPUT = 13,
        OBJECT_MULTI_STATE_OUTPUT = 14,
        OBJECT_NOTIFICATION_CLASS = 15,
        OBJECT_PROGRAM = 16,
        OBJECT_SCHEDULE = 17,
        OBJECT_AVERAGING = 18,
        OBJECT_MULTI_STATE_VALUE = 19,
        OBJECT_TRENDLOG = 20,
        OBJECT_LIFE_SAFETY_POINT = 21,
        OBJECT_LIFE_SAFETY_ZONE = 22,
        OBJECT_ACCUMULATOR = 23,
        OBJECT_PULSE_CONVERTER = 24,
        OBJECT_EVENT_LOG = 25,
        OBJECT_GLOBAL_GROUP = 26,
        OBJECT_TREND_LOG_MULTIPLE = 27,
        OBJECT_LOAD_CONTROL = 28,
        OBJECT_STRUCTURED_VIEW = 29,
        OBJECT_ACCESS_DOOR = 30,
        OBJECT_TIMER = 31,                  /* Addendum 135-2012ay */
        OBJECT_ACCESS_CREDENTIAL = 32,      /* Addendum 2008-j */
        OBJECT_ACCESS_POINT = 33,
        OBJECT_ACCESS_RIGHTS = 34,
        OBJECT_ACCESS_USER = 35,
        OBJECT_ACCESS_ZONE = 36,
        OBJECT_CREDENTIAL_DATA_INPUT = 37,  /* authentication-factor-input */
        OBJECT_NETWORK_SECURITY = 38,       /* Addendum 2008-g */
        OBJECT_BITSTRING_VALUE = 39,        /* Addendum 2008-w */
        OBJECT_CHARACTERSTRING_VALUE = 40,  /* Addendum 2008-w */
        OBJECT_DATE_PATTERN_VALUE = 41,     /* Addendum 2008-w */
        OBJECT_DATE_VALUE = 42,     /* Addendum 2008-w */
        OBJECT_DATETIME_PATTERN_VALUE = 43, /* Addendum 2008-w */
        OBJECT_DATETIME_VALUE = 44, /* Addendum 2008-w */
        OBJECT_INTEGER_VALUE = 45,  /* Addendum 2008-w */
        OBJECT_LARGE_ANALOG_VALUE = 46,     /* Addendum 2008-w */
        OBJECT_OCTETSTRING_VALUE = 47,      /* Addendum 2008-w */
        OBJECT_POSITIVE_INTEGER_VALUE = 48, /* Addendum 2008-w */
        OBJECT_TIME_PATTERN_VALUE = 49,     /* Addendum 2008-w */
        OBJECT_TIME_VALUE = 50,     /* Addendum 2008-w */
        OBJECT_NOTIFICATION_FORWARDER = 51, /* Addendum 2010-af */
        OBJECT_ALERT_ENROLLMENT = 52,       /* Addendum 2010-af */
        OBJECT_CHANNEL = 53,        /* Addendum 2010-aa */
        OBJECT_LIGHTING_OUTPUT = 54,        /* Addendum 2010-i */
        OBJECT_BINARY_LIGHTING_OUTPUT = 55,        /* Addendum 135-2012az */
        OBJECT_NETWORK_PORT = 56,
        OBJECT_ELEVATOR_GROUP = 57,
        OBJECT_ESCALATOR = 58,
        OBJECT_LIFT = 59,
        OBJECT_STAGING = 60, //Addendum 135-2016bd
        OBJECT_STANDARD_MAX,
        /* Enumerated values 0-127 are reserved for definition by ASHRAE. */
        /* Enumerated values 128-1023 may be used by others subject to  */
        /* the procedures and constraints described in Clause 23. */
        /* do the max range inside of enum so that
           compilers will allocate adequate sized datatype for enum
           which is used to store decoding */
        OBJECT_PROPRIETARY_MIN = 128,
        OBJECT_PROPRIETARY_MAX = 1023,
        MAX_BACNET_OBJECT_TYPE = 1024,
        MAX_ASHRAE_OBJECT_TYPE = 60,
    };

    public enum BacnetApplicationTags
    {
        BACNET_APPLICATION_TAG_NULL = 0,
        BACNET_APPLICATION_TAG_BOOLEAN = 1,
        BACNET_APPLICATION_TAG_UNSIGNED_INT = 2,
        BACNET_APPLICATION_TAG_SIGNED_INT = 3,
        BACNET_APPLICATION_TAG_REAL = 4,
        BACNET_APPLICATION_TAG_DOUBLE = 5,
        BACNET_APPLICATION_TAG_OCTET_STRING = 6,
        BACNET_APPLICATION_TAG_CHARACTER_STRING = 7,
        BACNET_APPLICATION_TAG_BIT_STRING = 8,
        BACNET_APPLICATION_TAG_ENUMERATED = 9,
        BACNET_APPLICATION_TAG_DATE = 10,
        BACNET_APPLICATION_TAG_TIME = 11,
        BACNET_APPLICATION_TAG_OBJECT_ID = 12,
        BACNET_APPLICATION_TAG_RESERVE1 = 13,
        BACNET_APPLICATION_TAG_RESERVE2 = 14,
        BACNET_APPLICATION_TAG_RESERVE3 = 15,
        MAX_BACNET_APPLICATION_TAG = 16,

        /* Extra stuff - complex tagged data - not specifically enumerated */

        /* Means : "nothing", an empty list, not even a null character */
        BACNET_APPLICATION_TAG_EMPTYLIST,
        /* BACnetWeeknday */
        BACNET_APPLICATION_TAG_WEEKNDAY,
        /* BACnetDateRange */
        BACNET_APPLICATION_TAG_DATERANGE,
        /* BACnetDateTime */
        BACNET_APPLICATION_TAG_DATETIME,
        /* BACnetTimeStamp */
        BACNET_APPLICATION_TAG_TIMESTAMP,
        /* Error Class, Error Code */
        BACNET_APPLICATION_TAG_ERROR,
        /* BACnetDeviceObjectPropertyReference */
        BACNET_APPLICATION_TAG_DEVICE_OBJECT_PROPERTY_REFERENCE,
        /* BACnetDeviceObjectReference */
        BACNET_APPLICATION_TAG_DEVICE_OBJECT_REFERENCE,
        /* BACnetObjectPropertyReference */
        BACNET_APPLICATION_TAG_OBJECT_PROPERTY_REFERENCE,
        /* BACnetDestination (Recipient_List) */
        BACNET_APPLICATION_TAG_DESTINATION,
        /* BACnetRecipient */
        BACNET_APPLICATION_TAG_RECIPIENT,
        /* BACnetCOVSubscription */
        BACNET_APPLICATION_TAG_COV_SUBSCRIPTION,
        /* BACnetCalendarEntry */
        BACNET_APPLICATION_TAG_CALENDAR_ENTRY,
        /* BACnetWeeklySchedule */
        BACNET_APPLICATION_TAG_WEEKLY_SCHEDULE,
        /* BACnetSpecialEvent */
        BACNET_APPLICATION_TAG_SPECIAL_EVENT,
        /* BACnetReadAccessSpecification */
        BACNET_APPLICATION_TAG_READ_ACCESS_SPECIFICATION,
        /* BACnetReadAccessResult */
        BACNET_APPLICATION_TAG_READ_ACCESS_RESULT,
        /* BACnetLightingCommand */
        BACNET_APPLICATION_TAG_LIGHTING_COMMAND,
        BACNET_APPLICATION_TAG_CONTEXT_SPECIFIC_DECODED,
        BACNET_APPLICATION_TAG_CONTEXT_SPECIFIC_ENCODED,
        /* BACnetLogRecord */
        BACNET_APPLICATION_TAG_LOG_RECORD,
        BACNET_APPLICATION_CONTEXT_SPECIFIC



    };

    public enum BacnetWritePriority
    {
        NO_PRIORITY = 0,
        MANUAL_LIFE_SAFETY = 1,
        AUTOMATIC_LIFE_SAFETY = 2,
        UNSPECIFIED_LEVEL_3 = 3,
        UNSPECIFIED_LEVEL_4 = 4,
        CRITICAL_EQUIPMENT_CONTROL = 5,
        MINIMUM_ON_OFF = 6,
        UNSPECIFIED_LEVEL_7 = 7,
        MANUAL_OPERATOR = 8,
        UNSPECIFIED_LEVEL_9 = 9,
        UNSPECIFIED_LEVEL_10 = 10,
        UNSPECIFIED_LEVEL_11 = 11,
        UNSPECIFIED_LEVEL_12 = 12,
        UNSPECIFIED_LEVEL_13 = 13,
        UNSPECIFIED_LEVEL_14 = 14,
        UNSPECIFIED_LEVEL_15 = 15,
        LOWEST_AND_DEFAULT = 16
    }


    public enum BacnetFileAccessMethod
    {
        RECORD_ACCESS = 0,
        STREAM_ACCESS = 1
    }

    public enum BacnetCharacterStringEncodings
    {
        CHARACTER_ANSI_X34 = 0,  /* deprecated : Addendum 135-2008k  */
        CHARACTER_UTF8 = 0,
        CHARACTER_MS_DBCS = 1,
        CHARACTER_JISC_6226 = 2, /* deprecated : Addendum 135-2008k  */
        CHARACTER_JISX_0208 = 2,
        CHARACTER_UCS4 = 3,
        CHARACTER_UCS2 = 4,
        CHARACTER_ISO8859 = 5,
    };

    public struct BacnetPropetyState
    {
        public enum BacnetPropertyStateTypes
        {
            BOOLEAN_VALUE,
            BINARY_VALUE,
            EVENT_TYPE,
            POLARITY,
            PROGRAM_CHANGE,
            PROGRAM_STATE,
            REASON_FOR_HALT,
            RELIABILITY,
            STATE,
            SYSTEM_STATUS,
            UNITS,
            UNSIGNED_VALUE,
            LIFE_SAFETY_MODE,
            LIFE_SAFETY_STATE
        };

        public BacnetPropertyStateTypes tag;
        public uint state;
    };

    public struct BacnetObjectDescription
    {
        public BacnetObjectTypes typeId;
        public List<BacnetPropertyIds> propsId;
    }

    [Serializable]
    public struct BacnetDeviceObjectPropertyReference : ASN1.IASN1encode
    {
        public BacnetObjectId objectIdentifier;
        public BacnetPropertyIds propertyIdentifier;
        public UInt32 arrayIndex;
        public BacnetObjectId deviceIndentifier;

        public BacnetDeviceObjectPropertyReference(BacnetObjectId objectIdentifier, BacnetPropertyIds propertyIdentifier, BacnetObjectId? deviceIndentifier = null, UInt32 arrayIndex = ASN1.BACNET_ARRAY_ALL)
        {
            this.objectIdentifier = objectIdentifier;
            this.propertyIdentifier = propertyIdentifier;
            this.arrayIndex = arrayIndex;
            if (deviceIndentifier != null)
                this.deviceIndentifier = deviceIndentifier.Value;
            else
                this.deviceIndentifier = new BacnetObjectId(BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE, 0);

        }
        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.bacapp_encode_device_obj_property_ref(buffer, this);
        }

        public BacnetObjectId ObjectId
        {
            get { return objectIdentifier; }
            set { objectIdentifier = value; }
        }
        public Int32 ArrayIndex // shows -1 when it's ASN1.BACNET_ARRAY_ALL
        {
            get
            {
                if (arrayIndex == ASN1.BACNET_ARRAY_ALL)
                    return -1;
                else return (Int32)arrayIndex;
            }
            set
            {
                if (value < 0)
                    arrayIndex = ASN1.BACNET_ARRAY_ALL;
                else
                    arrayIndex = (UInt32)value;
            }
        }
        public BacnetObjectId? DeviceId  // shows null when it's not OBJECT_DEVICE
        {
            get
            {
                if (deviceIndentifier.type == BacnetObjectTypes.OBJECT_DEVICE)
                    return deviceIndentifier;
                else
                    return null;
            }
            set
            {
                if (value == null)
                    deviceIndentifier = new BacnetObjectId();
                else
                    deviceIndentifier = value.Value;
            }
        }
        public BacnetPropertyIds PropertyId
        {
            get { return propertyIdentifier; }
            set { propertyIdentifier = value; }
        }

    };

    [Serializable]
    public struct BacnetDeviceObjectReference : ASN1.IASN1encode
    {
        public BacnetObjectId objectIdentifier;
        public BacnetObjectId deviceIndentifier;

        public BacnetDeviceObjectReference(BacnetObjectId objectIdentifier, BacnetObjectId? deviceIndentifier = null)
        {
            this.objectIdentifier = objectIdentifier;

            if (deviceIndentifier != null)
                this.deviceIndentifier = deviceIndentifier.Value;
            else
                this.deviceIndentifier = new BacnetObjectId(BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE, 0);
        }


        public override string ToString()
        {
            if (deviceIndentifier.type == BacnetObjectTypes.OBJECT_DEVICE)
                return objectIdentifier.ToString() + " on Device:" + deviceIndentifier.Instance.ToString();
            else
                return objectIdentifier.ToString();
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            if (deviceIndentifier.type == BacnetObjectTypes.OBJECT_DEVICE)
            {
                ASN1.encode_context_object_id(buffer, 0, deviceIndentifier.type, deviceIndentifier.instance);
            }
            ASN1.encode_context_object_id(buffer, 1, objectIdentifier.type, objectIdentifier.instance);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;

            byte TagNum;
            len += ASN1.decode_tag_number(buffer, offset, out TagNum);

            if (TagNum == 0)
            {
                len += ASN1.decode_object_id(buffer, offset + len, out deviceIndentifier.type, out deviceIndentifier.instance);
                len += ASN1.decode_tag_number(buffer, offset, out TagNum); // shall be 1
            }
            else
                deviceIndentifier = new BacnetObjectId(BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE, 0);

            len += ASN1.decode_object_id(buffer, offset + len, out objectIdentifier.type, out objectIdentifier.instance);

            return len;

        }

        public BacnetObjectId ObjectId
        {
            get { return objectIdentifier; }
            set { objectIdentifier = value; }
        }

        public BacnetObjectId? DeviceId  // shows null when it's not OBJECT_DEVICE
        {
            get
            {
                if (deviceIndentifier.type == BacnetObjectTypes.OBJECT_DEVICE)
                    return deviceIndentifier;
                else
                    return null;
            }
            set
            {
                if (value == null)
                    deviceIndentifier = new BacnetObjectId(BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE, 0);
                else
                    deviceIndentifier = value.Value;
            }
        }

    };
    public enum BacnetLoggingTypes
    {
        LOGGING_POLLED = 0,
        LOGGING_COV = 1,
        LOGGING_TRIGGERED = 2
    };

    public struct BacnetEventNotificationData
    {
        public UInt32 processIdentifier;
        public BacnetObjectId initiatingObjectIdentifier;
        public BacnetObjectId eventObjectIdentifier;
        public BacnetGenericTime timeStamp;
        public UInt32 notificationClass;
        public byte priority;
        public BacnetEventTypes eventType;
        public string messageText;       /* OPTIONAL - Set to NULL if not being used */
        public BacnetNotifyTypes notifyType;
        public bool ackRequired;
        public BacnetEventStates fromState;
        public BacnetEventStates toState;

        public enum BacnetEventStates
        {
            EVENT_STATE_NORMAL = 0,
            EVENT_STATE_FAULT = 1,
            EVENT_STATE_OFFNORMAL = 2,
            EVENT_STATE_HIGH_LIMIT = 3,
            EVENT_STATE_LOW_LIMIT = 4,
            EVENT_STATE_LIFE_SAFETY_ALARM = 5
        };

        public enum BacnetEventEnable
        {
            EVENT_ENABLE_TO_OFFNORMAL = 1,
            EVENT_ENABLE_TO_FAULT = 2,
            EVENT_ENABLE_TO_NORMAL = 4
        };

        public enum BacnetLimitEnable
        {
            EVENT_LOW_LIMIT_ENABLE = 1,
            EVENT_HIGH_LIMIT_ENABLE = 2
        };

        public enum BacnetNotifyTypes
        {
            NOTIFY_ALARM = 0,
            NOTIFY_EVENT = 1,
            NOTIFY_ACK_NOTIFICATION = 2
        };

        public enum BacnetEventTypes
        {
            EVENT_CHANGE_OF_BITSTRING = 0,
            EVENT_CHANGE_OF_STATE = 1,
            EVENT_CHANGE_OF_VALUE = 2,
            EVENT_COMMAND_FAILURE = 3,
            EVENT_FLOATING_LIMIT = 4,
            EVENT_OUT_OF_RANGE = 5,
            /*  complex-event-type        (6), -- see comment below */
            /*  event-buffer-ready   (7), -- context tag 7 is deprecated */
            EVENT_CHANGE_OF_LIFE_SAFETY = 8,
            EVENT_EXTENDED = 9,
            EVENT_BUFFER_READY = 10,
            EVENT_UNSIGNED_RANGE = 11,
            // enumeration 12 is reserved for future
            EVENT_ACCESS_EVENT = 13,
            EVENT_DOUBLE_OUT_OF_RANGE = 14,
            EVENT_SIGNED_OUT_OF_RANGE = 15,
            EVENT_UNSIGNED_OUT_OF_RANGE = 16,
            EVENT_CHANGE_OF_CHARACTERSTRING = 17,
            EVENT_CHANGE_OF_STATUS_FLAG = 18,
            EVENT_CHANGE_OF_RELIABILITY = 19,
            EVENT_NONE = 20,
            EVENT_CHANGE_OF_DISCRETE_VALUE = 21,
            EVENT_CHANGE_OF_TIMER = 22
            /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
            /* Enumerated values 64-65535 may be used by others subject to  */
            /* the procedures and constraints described in Clause 23.  */
            /* It is expected that these enumerated values will correspond to  */
            /* the use of the complex-event-type CHOICE [6] of the  */
            /* BACnetNotificationParameters production. */
            /* The last enumeration used in this version is 11. */
            /* do the max range inside of enum so that
               compilers will allocate adequate sized datatype for enum
               which is used to store decoding */
        };

        public enum BacnetCOVTypes
        {
            CHANGE_OF_VALUE_BITS,
            CHANGE_OF_VALUE_REAL
        };

        public enum BacnetLifeSafetyStates
        {
            MIN_LIFE_SAFETY_STATE = 0,
            LIFE_SAFETY_STATE_QUIET = 0,
            LIFE_SAFETY_STATE_PRE_ALARM = 1,
            LIFE_SAFETY_STATE_ALARM = 2,
            LIFE_SAFETY_STATE_FAULT = 3,
            LIFE_SAFETY_STATE_FAULT_PRE_ALARM = 4,
            LIFE_SAFETY_STATE_FAULT_ALARM = 5,
            LIFE_SAFETY_STATE_NOT_READY = 6,
            LIFE_SAFETY_STATE_ACTIVE = 7,
            LIFE_SAFETY_STATE_TAMPER = 8,
            LIFE_SAFETY_STATE_TEST_ALARM = 9,
            LIFE_SAFETY_STATE_TEST_ACTIVE = 10,
            LIFE_SAFETY_STATE_TEST_FAULT = 11,
            LIFE_SAFETY_STATE_TEST_FAULT_ALARM = 12,
            LIFE_SAFETY_STATE_HOLDUP = 13,
            LIFE_SAFETY_STATE_DURESS = 14,
            LIFE_SAFETY_STATE_TAMPER_ALARM = 15,
            LIFE_SAFETY_STATE_ABNORMAL = 16,
            LIFE_SAFETY_STATE_EMERGENCY_POWER = 17,
            LIFE_SAFETY_STATE_DELAYED = 18,
            LIFE_SAFETY_STATE_BLOCKED = 19,
            LIFE_SAFETY_STATE_LOCAL_ALARM = 20,
            LIFE_SAFETY_STATE_GENERAL_ALARM = 21,
            LIFE_SAFETY_STATE_SUPERVISORY = 22,
            LIFE_SAFETY_STATE_TEST_SUPERVISORY = 23,
            MAX_LIFE_SAFETY_STATE = 24,
            /* Enumerated values 0-255 are reserved for definition by ASHRAE.  */
            /* Enumerated values 256-65535 may be used by others subject to  */
            /* procedures and constraints described in Clause 23. */
            /* do the max range inside of enum so that
               compilers will allocate adequate sized datatype for enum
               which is used to store decoding */
            LIFE_SAFETY_STATE_PROPRIETARY_MIN = 256,
            LIFE_SAFETY_STATE_PROPRIETARY_MAX = 65535
        };

        public enum BacnetLifeSafetyModes
        {
            MIN_LIFE_SAFETY_MODE = 0,
            LIFE_SAFETY_MODE_OFF = 0,
            LIFE_SAFETY_MODE_ON = 1,
            LIFE_SAFETY_MODE_TEST = 2,
            LIFE_SAFETY_MODE_MANNED = 3,
            LIFE_SAFETY_MODE_UNMANNED = 4,
            LIFE_SAFETY_MODE_ARMED = 5,
            LIFE_SAFETY_MODE_DISARMED = 6,
            LIFE_SAFETY_MODE_PREARMED = 7,
            LIFE_SAFETY_MODE_SLOW = 8,
            LIFE_SAFETY_MODE_FAST = 9,
            LIFE_SAFETY_MODE_DISCONNECTED = 10,
            LIFE_SAFETY_MODE_ENABLED = 11,
            LIFE_SAFETY_MODE_DISABLED = 12,
            LIFE_SAFETY_MODE_AUTOMATIC_RELEASE_DISABLED = 13,
            LIFE_SAFETY_MODE_DEFAULT = 14,
            MAX_LIFE_SAFETY_MODE = 15,
            /* Enumerated values 0-255 are reserved for definition by ASHRAE.  */
            /* Enumerated values 256-65535 may be used by others subject to  */
            /* procedures and constraints described in Clause 23. */
            /* do the max range inside of enum so that
               compilers will allocate adequate sized datatype for enum
               which is used to store decoding */
            LIFE_SAFETY_MODE_PROPRIETARY_MIN = 256,
            LIFE_SAFETY_MODE_PROPRIETARY_MAX = 65535
        };

        public enum BacnetLifeSafetyOperations
        {
            LIFE_SAFETY_OP_NONE = 0,
            LIFE_SAFETY_OP_SILENCE = 1,
            LIFE_SAFETY_OP_SILENCE_AUDIBLE = 2,
            LIFE_SAFETY_OP_SILENCE_VISUAL = 3,
            LIFE_SAFETY_OP_RESET = 4,
            LIFE_SAFETY_OP_RESET_ALARM = 5,
            LIFE_SAFETY_OP_RESET_FAULT = 6,
            LIFE_SAFETY_OP_UNSILENCE = 7,
            LIFE_SAFETY_OP_UNSILENCE_AUDIBLE = 8,
            LIFE_SAFETY_OP_UNSILENCE_VISUAL = 9,
            /* Enumerated values 0-63 are reserved for definition by ASHRAE.  */
            /* Enumerated values 64-65535 may be used by others subject to  */
            /* procedures and constraints described in Clause 23. */
            /* do the max range inside of enum so that
               compilers will allocate adequate sized datatype for enum
               which is used to store decoding */
            LIFE_SAFETY_OP_PROPRIETARY_MIN = 64,
            LIFE_SAFETY_OP_PROPRIETARY_MAX = 65535
        };

        /*
         ** Each of these structures in the union maps to a particular eventtype
         ** Based on BACnetNotificationParameters
         */

        /*
         ** EVENT_CHANGE_OF_BITSTRING
         */
        public BacnetBitString changeOfBitstring_referencedBitString;
        public BacnetBitString changeOfBitstring_statusFlags;
        /*
         ** EVENT_CHANGE_OF_STATE
         */
        public BacnetPropetyState changeOfState_newState;
        public BacnetBitString changeOfState_statusFlags;
        /*
         ** EVENT_CHANGE_OF_VALUE
         */
        public BacnetBitString changeOfValue_changedBits;
        public float changeOfValue_changeValue;
        public BacnetCOVTypes changeOfValue_tag;
        public BacnetBitString changeOfValue_statusFlags;
        /*
         ** EVENT_COMMAND_FAILURE
         **
         ** Not Supported!
         */
        /*
         ** EVENT_FLOATING_LIMIT
         */
        public float floatingLimit_referenceValue;
        public BacnetBitString floatingLimit_statusFlags;
        public float floatingLimit_setPointValue;
        public float floatingLimit_errorLimit;
        /*
         ** EVENT_OUT_OF_RANGE
         */
        public float outOfRange_exceedingValue;
        public BacnetBitString outOfRange_statusFlags;
        public float outOfRange_deadband;
        public float outOfRange_exceededLimit;
        /*
         ** EVENT_CHANGE_OF_LIFE_SAFETY
         */
        public BacnetLifeSafetyStates changeOfLifeSafety_newState;
        public BacnetLifeSafetyModes changeOfLifeSafety_newMode;
        public BacnetBitString changeOfLifeSafety_statusFlags;
        public BacnetLifeSafetyOperations changeOfLifeSafety_operationExpected;
        /*
         ** EVENT_EXTENDED
         **
         ** Not Supported!
         */
        /*
         ** EVENT_BUFFER_READY
         */
        public BacnetDeviceObjectPropertyReference bufferReady_bufferProperty;
        public uint bufferReady_previousNotification;
        public uint bufferReady_currentNotification;
        /*
         ** EVENT_UNSIGNED_RANGE
         */
        public uint unsignedRange_exceedingValue;
        public BacnetBitString unsignedRange_statusFlags;
        public uint unsignedRange_exceededLimit;
    };

    public struct BacnetValue
    {
        public BacnetApplicationTags Tag;
        public object Value;
        public BacnetValue(BacnetApplicationTags tag, object value)
        {
            this.Tag = tag;
            this.Value = value;
        }
        public BacnetValue(object value)
        {
            this.Value = value;
            Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL;

            //guess at the tag
            if (value != null)
                Tag = TagFromType(value.GetType());
        }

        public BacnetApplicationTags TagFromType(Type t)
        {
            if (t == typeof(string))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING;
            else if (t == typeof(long) || t == typeof(int) || t == typeof(short) || t == typeof(sbyte))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT;
            else if (t == typeof(ulong) || t == typeof(uint) || t == typeof(ushort) || t == typeof(byte))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT;
            else if (t == typeof(bool))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN;
            else if (t == typeof(float))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL;
            else if (t == typeof(double))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_DOUBLE;
            else if (t == typeof(BacnetBitString))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING;
            else if (t == typeof(BacnetObjectId))
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID;
            else
                return BacnetApplicationTags.BACNET_APPLICATION_TAG_CONTEXT_SPECIFIC_ENCODED;
        }

        public override string ToString()
        {
            if (Value == null)
                return "";
            else if (Value.GetType() == typeof(byte[]))
            {
                string ret = "";
                byte[] tmp = (byte[])Value;
                foreach (byte b in tmp)
                    ret += b.ToString("X2");
                return ret;
            }
            else
                return Value.ToString();
        }
    }
    [Serializable]
    public struct BacnetObjectId : IComparable<BacnetObjectId>
    {
        public BacnetObjectTypes type;
        public UInt32 instance;
        public BacnetObjectId(BacnetObjectTypes type, UInt32 instance)
        {
            this.type = type;
            this.instance = instance;
        }
        public BacnetObjectTypes Type
        {
            get { return type; }
            set { type = value; }
        }
        public UInt32 Instance
        {
            get { return instance; }
            set { instance = value; }
        }
        public override string ToString()
        {
            return type.ToString() + ":" + instance;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            else return obj.ToString().Equals(this.ToString());
        }
        public int CompareTo(BacnetObjectId other)
        {
            if (this.type == other.type)
                return this.instance.CompareTo(other.instance);
            else
            {
                if (this.type == BacnetObjectTypes.OBJECT_DEVICE) return -1;
                if (other.type == BacnetObjectTypes.OBJECT_DEVICE) return 1;
                // cast to int for comparison otherwise unpredictable behaviour with outbound enum (proprietary type)
                return ((int)(this.type)).CompareTo((int)other.type);
            }
        }
        public static BacnetObjectId Parse(string value)
        {
            BacnetObjectId ret = new BacnetObjectId();
            if (string.IsNullOrEmpty(value)) return ret;
            int p = value.IndexOf(":");
            if (p < 0) return ret;
            string str_type = value.Substring(0, p);
            string str_instance = value.Substring(p + 1);
            ret.type = (BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), str_type);
            ret.instance = uint.Parse(str_instance);
            return ret;
        }

    };

    public struct BacnetPropertyReference
    {
        public UInt32 propertyIdentifier;
        public UInt32 propertyArrayIndex;        /* optional */
        public BacnetPropertyReference(uint id, uint array_index)
        {
            propertyIdentifier = id;
            propertyArrayIndex = array_index;
        }
        public override string ToString()
        {
            return ((BacnetPropertyIds)propertyIdentifier).ToString();
        }
    };

    public struct BacnetPropertyValue
    {
        public BacnetPropertyReference property;
        public IList<BacnetValue> value;
        public byte priority;

        public override string ToString()
        {
            return property.ToString();
        }
    }

    public struct BacnetGenericTime
    {
        public BacnetTimestampTags Tag;
        public DateTime Time;
        public UInt16 Sequence;

        public BacnetGenericTime(DateTime Time, BacnetTimestampTags Tag, UInt16 Sequence = 0)
        {
            this.Time = Time;
            this.Tag = Tag;
            this.Sequence = Sequence;
        }
    }

    public enum BacnetTimestampTags
    {
        TIME_STAMP_NONE = -1,
        TIME_STAMP_TIME = 0,
        TIME_STAMP_SEQUENCE = 1,
        TIME_STAMP_DATETIME = 2
    };

    public struct BacnetDate : ASN1.IASN1encode
    {
        public byte year;     /* 255 any */
        public byte month;      /* 1=Jan; 255 any, 13 Odd, 14 Even */
        public byte day;        /* 1..31; 32 last day of the month; 255 any */
        public byte wday;       /* 1=Monday-7=Sunday, 255 any */

        public BacnetDate(byte year, byte month, byte day, byte wday = 255)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.wday = wday;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {

            buffer.Add((byte)year);
            buffer.Add((byte)month);
            buffer.Add((byte)day);
            buffer.Add((byte)wday);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            year = buffer[offset];
            month = buffer[offset + 1];
            day = buffer[offset + 2];
            wday = buffer[offset + 3];

            return 4;
        }
        public bool IsPeriodic
        {
            get { return (year == 255) || (month > 12) || (day == 255); }
        }

        public bool IsAFittingDate(DateTime date)
        {
            if ((date.Year != (year + 1900)) && (year != 255))
                return false;

            if ((date.Month != month) && (month != 255) && (month != 13) && (month != 14))
                return false;
            if ((month == 13) && ((date.Month & 1) != 1))
                return false;
            if ((month == 14) && ((date.Month & 1) == 1))
                return false;

            if ((date.Day != day) && (day != 255))
                return false;
            // day 32 todo

            if (wday == 255)
                return true;
            if ((wday == 7) && (date.DayOfWeek == 0))  // Sunday 7 for Bacnet, 0 for .NET
                return true;
            if (wday == (int)date.DayOfWeek)
                return true;

            return false;
        }

        public DateTime toDateTime() // Not every time possible, too much complex (any month, any year ...)
        {
            try
            {
                if (IsPeriodic == true)
                    return new DateTime(1, 1, 1);
                else
                    return new DateTime(year + 1900, month, day);
            }
            catch { }

            return DateTime.Now; // or anything else why not !
        }

        string GetDayName(int day)
        {
            if (day == 7) day = 0;
            return CultureInfo.CurrentCulture.DateTimeFormat.DayNames[day];
        }

        public override string ToString()
        {
            String ret;

            if (year != 255)
                ret = (year + 1900).ToString() + "-";
            else
                ret = "****-";


            switch (month)
            {
                case 13:
                    ret = ret + "odd-";
                    break;
                case 14:
                    ret = ret + "even-";
                    break;
                case 255:
                    ret = ret + "**-";
                    break;
                default:
                    ret = ret + month.ToString() + "-";
                    break;
            }
            if (day != 255)
                ret = ret + day.ToString();
            else
                ret = ret + "**";



            if (wday != 255)
                ret = ret + " " + GetDayName(wday);
            return ret;
        }
    }

    public struct BacnetDateRange : ASN1.IASN1encode
    {
        public BacnetDate startDate;
        public BacnetDate endDate;

        public BacnetDateRange(BacnetDate start, BacnetDate end)
        {
            startDate = start;
            endDate = end;
        }
        public void ASN1encode(EncodeBuffer buffer)
        {

            ASN1.encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE, false, 4);
            startDate.ASN1encode(buffer);
            ASN1.encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE, false, 4);
            endDate.ASN1encode(buffer);

        }
        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 1; // opening tag
            len += startDate.ASN1decode(buffer, offset + len, len_value);
            len++;
            len += endDate.ASN1decode(buffer, offset + len, len_value);
            return len;
        }

        public bool IsAFittingDate(DateTime date)
        {
            date = new DateTime(date.Year, date.Month, date.Day);
            if ((date >= startDate.toDateTime()) && (date <= endDate.toDateTime()))
                return true;
            else
                return false;

        }

        public override string ToString()
        {
            string ret;

            if (startDate.day != 255)
                ret = "From " + startDate.ToString();
            else
                ret = "From ****-**-**";

            if (endDate.day != 255)
                ret = ret + " to " + endDate.ToString();
            else
                ret = ret + " to ****-**-**";

            return ret;
        }
    };

    public struct BacnetweekNDay : ASN1.IASN1encode
    {
        public byte month;  /* 1 January, 13 Odd, 14 Even, 255 Any */
        public byte week;   /* 255 any, 1 for day 1 to 7, 2 for day 8 to 14, ... 6 last 7 days of this month */
        public byte wday;   /* 1=Monday-7=Sunday, 255 any */

        public BacnetweekNDay(byte day, byte month, byte week = 255)
        {
            wday = day;
            this.month = month;
            this.week = week;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            buffer.Add(month);
            buffer.Add(week);
            buffer.Add(wday);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            month = buffer[offset++];
            week = buffer[offset++];
            wday = buffer[offset];

            return 3;
        }

        string GetDayName(int day)
        {
            if (day == 7) day = 0;
            return CultureInfo.CurrentCulture.DateTimeFormat.DayNames[day];
        }

        public override string ToString()
        {
            string ret;

            if (wday != 255)
                ret = GetDayName(wday);
            else
                ret = "Every days";

            if ((week >= 1) && (week <= 5))
                ret = ret + " (days " + (((week - 1) * 7) + 1).ToString() + "-" + (week * 7).ToString() + ")";
            if (week == 6)
                ret = ret + " (last 7 days of month)";

            if (month <= 12)
                ret = ret + " on " + CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[month - 1];
            else
                if (month == 255)
                ret = ret + " on every months";
            else if (month == 13)
                ret = ret + " on odd months";
            else
                ret = ret + " on even months";

            return ret;
        }

        public bool IsAFittingDate(DateTime date)
        {

            if ((date.Month != month) && (month != 255) && (month != 13) && (month != 14))
                return false;
            if ((month == 13) && ((date.Month & 1) != 1))
                return false;
            if ((month == 14) && ((date.Month & 1) == 1))
                return false;

            //  week, wrong values not tested
            if ((week >= 1) && (week <= 5))
            {
                if (date.Day <= (week - 1) * 7)
                    return false;
                if (date.Day > week * 7)
                    return false;
            }
            if (week == 6) // last 7 days of the month
            {
                if (date.AddDays(7).Month == date.Month)
                    return false;
            }

            if (wday == 255)
                return true;
            if ((wday == 7) && (date.DayOfWeek == 0))  // Sunday 7 for Bacnet, 0 for .NET
                return true;
            if (wday == (int)date.DayOfWeek)
                return true;

            return false;
        }
    }

    public enum BACnetDoorStatus
    {
        CLOSED = 0,
        OPENED = 1,
        UNKNOWN = 2,
        DOOR_FAULT = 3,
        UNUSED = 4,
        NONE = 5,
        CLOSING = 6,
        OPENING = 7,
        SAFETY_LOCKED = 8,
        LIMITED_OPENED = 9
    }

    public enum BACnetDoorValue
    {
        LOCK = 0,
        UNLOCK = 1,
        PULSE_UNLOCK = 2,
        EXTENDED_PULSE_UNLOCK = 3

    }

    public enum BACnetAccessCredentialDisableReason
    {
        DISABLED = 0,
        DISABLED_NEEDS_PROVISIONING = 1,
        DISABLED_UNASSIGNED = 2,
        DISABLED_NOT_YET_ACTIVE = 3,
        DISABLED_EXPIRED = 4,
        DISABLED_LOCKOUT = 5,
        DISABLED_MAX_DAYS = 6,
        DISABLED_MAX_USES = 7,
        DISABLED_INACTIVITY = 8,
        DISABLED_MANUAL = 9
    }

    public enum BACnetAccessCredentialDisable
    {
        NONE = 0,
        DISABLE = 1,
        DISABLE_MANUAL = 2,
        DISABLE_LOCKOUT = 3
    }

    public enum BACnetAccessPassbackMode
    {
        PASSBACK_OFF = 0,
        HARD_PASSBACK = 1,
        SOFT_PASSBACK = 2
    }

    public enum BACnetAccessUserType
    {
        ASSET =0,
        GROUP =1,
        PERSON =2
    }

    public enum BACnetAccessZoneOccupancyState
    {
        NORMAL = 0,
        BELOW_LOWER_LIMIT = 1,
        AT_LOWER_LIMIT = 2,
        AT_UPPER_LIMIT = 3,
        ABOVE_UPPER_LIMIT = 4,
        DISABLED = 5,
        NOT_SUPPORTED = 6
    }

    public enum BACnetAuthenticationFactorType
    {
        UNDEFINED = 0,
        ERROR = 1,
        CUSTOM = 2,
        SIMPLE_NUMBER16 = 3,
        SIMPLE_NUMBER32 = 4,
        SIMPLE_NUMBER56 = 5,
        SIMPLE_ALPHA_NUMERIC = 6,
        ABA_TRACK2 = 7,
        WIEGAND26 = 8,
        WIEGAND37 = 9,
        WIEGAND37_FACILITY = 10,
        FACILITY16_CARD32 = 11,
        FACILITY32_CARD32 = 12,
        FASC_N = 13,
        FASC_N_BCD = 14,
        FASC_N_LARGE = 15,
        FASC_N_LARGE_BCD = 16,
        GSA75 = 17,
        CHUID = 18,
        CHUID_FULL = 19,
        GUID = 20,
        CBEFF_A = 21,
        CBEFF_B = 22,
        CBEFF_C = 23,
        USER_PASSWORD = 24
    }

    public enum BACnetAuthorizationExemption
    {
        PASSBACK = 0,
        OCCUPANCY_CHECK = 1,
        ACCESS_RIGHTS = 2,
        LOCKOUT = 3,
        DENY = 4,
        VERIFICATION = 5,
        AUTHORIZATION_DELAY = 6
    }

    public enum BACnetAuthorizationMode
    {
        AUTHORIZE = 0,
        GRANT_ACTIVE = 1,
        DENY_ALL = 2,
        VERIFICATION_REQUIRED = 3,
        AUTHORIZATION_DELAYED = 4,
        NONE = 5
    }

    public enum BACnetAuthenticationStatus
    {
        NOT_READY = 0,
        READY = 1,
        DISABLED = 2,
        WAITING_FOR_AUTHENTICATION_FACTOR = 3,
        WAITING_FOR_ACCOMPANIMENT = 4,
        WAITING_FOR_VERIFICATION = 5,
        IN_PROGRESS = 6
    }

    public enum BACnetBinaryPV
    {
        inactive = 0,
        active = 1
    }

    public enum BACnetBinaryLightingPV
    {
        OFF = 0,
        ON = 1,
        WARN = 2,
        WARN_OFF = 3,
        WARN_RELINQUISH = 4,
        STOP = 5

    }

    public enum BACnetEscalatorFault
    {
        CONTROLLER_FAULT = 0,
        DRIVE_AND_MOTOR_FAULT = 1,
        MECHANICAL_COMPONENT_FAULT = 2,
        OVERSPEED_FAULT = 3,
        POWER_SUPPLY_FAULT = 4,
        SAFETY_DEVICE_FAULT = 5,
        CONTROLLER_SUPPLY_FAULT = 6,
        DRIVE_TEMPERATURE_EXCEEDED = 7,
        COMB_PLATE_FAULT = 8
    }

    public enum BACnetEscalatorMode
    {
        UNKNOWN = 0,
        STOP = 1,
        UP = 2,
        DOWN = 3,
        INSPECTION = 4,
        OUT_OF_SERVICE = 5

    }

    public enum BACnetEscalatorOperationDirection
    {
        UNKNOWN = 0,
        STOPPED = 1,
        UP_RATED_SPEED = 2,
        UP_REDUCED_SPEED = 3,
        DOWN_RATED_SPEED = 4,
        DOWN_REDUCED_SPEED = 5
    }

    public enum BACnetIPMode
    {
        NORMAL = 0,
        FOREIGN = 1,
        BBMD = 2
    }

    public enum BACnetLiftCarDirection
    {
        UNKNOWN = 0,
        NONE = 1,
        STOPPED = 2,
        UP = 3,
        DOWN = 4,
        UP_AND_DOWN = 5
    }

    public enum BACnetLiftCarDoorCommand
    {
        NONE = 0,
        OPEN = 1,
        CLOSE = 2
    }

    public enum BACnetLiftCarDriveStatus
    {
        UNKNOWN = 0,
        STATIONARY = 1,
        BRAKING = 2,
        ACCELERATE = 3,
        DECELERATE = 4,
        RATED_SPEED = 5,
        SINGLE_FLOOR_JUMP = 6,
        TWO_FLOOR_JUMP = 7,
        THREE_FLOOR_JUMP = 8,
        MULTI_FLOOR_JUMP = 9
    }

    public enum BACnetLiftCarMode
    {
        UNKNOWN = 0,
        NORMAL = 1, //-- IN SERVICE
        VIP = 2,
        HOMING = 3,
        PARKING = 4,
        ATTENDANT_CONTROL = 5,
        FIREFIGHTER_CONTROL = 6,
        EMERGENCY_POWER = 7,
        INSPECTION = 8,
        CABINET_RECALL = 9,
        EARTHQUAKE_OPERATION = 10,
        FIRE_OPERATION = 11,
        OUT_OF_SERVICE = 12,
        OCCUPANT_EVACUATION = 13
    }

    public enum BACnetLiftFault
    {
        CONTROLLER_FAULT = 0,
        DRIVE_AND_MOTOR_FAULT = 1,
        GOVERNOR_AND_SAFETY_GEAR_FAULT = 2,
        LIFT_SHAFT_DEVICE_FAULT = 3,
        POWER_SUPPLY_FAULT = 4,
        SAFETY_INTERLOCK_FAULT = 5,
        DOOR_CLOSING_FAULT = 6,
        DOOR_OPENING_FAULT = 7,
        CAR_STOPPED_OUTSIDE_LANDING_ZONE = 8,
        CALL_BUTTON_STUCK = 9,
        START_FAILURE = 10,
        CONTROLLER_SUPPLY_FAULT = 11,
        SELF_TEST_FAILURE = 12,
        RUNTIME_LIMIT_EXCEEDED = 13,
        POSITION_LOST = 14,
        DRIVE_TEMPERATURE_EXCEEDED = 15,
        LOAD_MEASUREMENT_FAULT = 16
    }

    public enum BACnetLiftGroupMode
    {
        UNKNOWN = 0,
        NORMAL = 1,
        DOWN_PEAK = 2,
        TWO_WAY = 3,
        FOUR_WAY = 4,
        EMERGENCY_POWER = 5,
        UP_PEAK = 6
    }

    public enum BACnetLightingInProgress
    {
        IDLE = 0,
        FADE_ACTIVE = 1,
        RAMP_ACTIVE = 2,
        NOT_CONTROLLED = 3,
        OTHER = 4
    }

    public enum BACnetLightingOperation
    {
        NONE = 0,
        FADE_TO = 1,
        RAMP_TO = 2,
        STEP_UP = 3,
        STEP_DOWN = 4,
        STEP_ON = 5,
        STEP_OFF = 6,
        WARN = 7,
        WARN_OFF = 8,
        WARN_RELINQUISH = 9,
        STOP = 10
    }

    public enum BACnetLightingTransition
    {
        NONE = 0,
        FADE = 1,
        RAMP = 2
    }

    public enum BACnetLoggingType
    {
        POLLED = 0,
        COV = 1,
        TRIGGERED = 2
    }

    public enum BACnetLogStatus
    {
        LOG_DISABLED = 0,
        BUFFER_PURGED = 1,
        LOG_INTERRUPTED = 2
    }

    public enum BACnetLockStatus
    {
        LOCKED = 0,
        UNLOCKED = 1,
        LOCK_FAULT = 2,
        UNUSED = 3,
        UNKNOWN = 4
    }

    public enum BACnetMaintenance
    {
        NONE = 0,
        PERIODIC_TEST = 1,
        NEED_SERVICE_OPERATIONAL = 2,
        NEED_SERVICE_INOPERATIVE = 3
    }

    public enum BACnetNetworkNumberQuality
    {
        UNKNOWN = 0,
        LEARNED = 1,
        LEARNED_CONFIGURED = 2,
        CONFIGURED = 3
    }

    public enum BACnetNetworkPortCommand
    {
        IDLE = 0,
        DISCARD_CHANGES = 1,
        RENEW_FD_REGISTRATION = 2,
        RESTART_SLAVE_DISCOVERY = 3,
        RENEW_DHCP = 4,
        RESTART_AUTONEGOTIATION = 5,
        DISCONNECT = 6,
        RESTART_PORT = 7
    }

    public enum BACnetNetworkType
    {
        ETHERNET = 0,
        ARCNET = 1,
        MSTP = 2,
        PTP = 3,
        LONTALK = 4,
        IPV4 = 5,
        ZIGBEE = 6,
        VIRTUAL = 7,
        NON_BACNET = 8,
        IPV6 = 9,
        SERIAL = 10,
        SECURE_CONNECT = 11 // Addentum 135-2020cc
    }

    public enum BACnetSecurityLevel
    {
        INCAPABLE = 0, // INDICATES THAT THE DEVICE IS CONFIGURED TO NOT USE SECURITY
        PLAIN = 1,
        SIGNED = 2,
        ENCRYPTED = 3,
        SIGNED_END_TO_END = 4,
        ENCRYPTED_END_TO_END = 5
    }

    public enum BACnetProtocolLevel
    {
        PHYSICAL = 0,
        PROTOCOL = 1,
        BACNET_APPLICATION = 2,
        NON_BACNET_APPLICATION = 3
    }

    public enum BACnetSecurityPolicy
    {
        PLAIN_NON_TRUSTED = 0,
        PLAIN_TRUSTED = 1,
        SIGNED_TRUSTED = 2,
        ENCRYPTED_TRUSTED = 3
    }

    public enum BACnetRelationship
    {
        UNKNOWN = 0,
        DEFAULT = 1,
        CONTAINS = 2,
        CONTAINED_BY = 3,
        USES = 4,
        USED_BY = 5,
        COMMANDS = 6,
        COMMANDED_BY = 7,
        ADJUSTS = 8,
        ADJUSTED_BY = 9,
        INGRESS = 10,
        EGRESS = 11,
        SUPPLIES_AIR = 12,
        RECEIVES_AIR = 13,
        SUPPLIES_HOT_AIR = 14,
        RECEIVES_HOT_AIR = 15,
        SUPPLIES_COOL_AIR = 16,
        RECEIVES_COOL_AIR = 17,
        SUPPLIES_POWER = 18,
        RECEIVES_POWER = 19,
        SUPPLIES_GAS = 20,
        RECEIVES_GAS = 21,
        SUPPLIES_WATER = 22,
        RECEIVES_WATER = 23,
        SUPPLIES_HOT_WATER = 24,
        RECEIVES_HOT_WATER = 25,
        SUPPLIES_COOL_WATER = 26,
        RECEIVES_COOL_WATER = 27,
        SUPPLIES_STEAM = 28,
        RECEIVES_STEAM = 29,
    }
    public enum BACnetShedState
    {
        SHED_INACTIVE = 0,
        SHED_REQUEST_PENDING = 1,
        SHED_COMPLIANT = 2,
        SHED_NON_COMPLIANT = 3
    }

    public enum BACnetSilencedState
    {
        UNSILENCED = 0,
        AUDIBLE_SILENCED = 1,
        VISIBLE_SILENCED = 2,
        ALL_SILENCED = 3
    }

    public enum BACnetTimerState
    {
        IDLE = 0,
        RUNNING = 1,
        EXPIRED = 2
    }

    public enum BACnetTimerTransition
    {
        NONE = 0,
        IDLE_TO_RUNNING = 1,
        RUNNING_TO_IDLE = 2,
        RUNNING_TO_RUNNING = 3,
        RUNNING_TO_EXPIRED = 4,
        FORCED_TO_EXPIRED = 5,
        EXPIRED_TO_IDLE = 6,
        EXPIRED_TO_RUNNING = 7
    }

    public enum BACnetVTClass
    {
        DEFAULT_TERMINAL = 0,
        ANSI_X3_64 = 1,
        DEC_VT52 = 2,
        DEC_VT100 = 3,
        DEC_VT220 = 4,
        HP_700_94 = 5,
        IBM_3130 = 6
    }

    public enum BACnetWriteStatus
    {
        IDLE = 0,
        IN_PROGRESS = 1,
        SUCCESSFUL = 2,
        FAILED = 3
    }

    public struct BACnetPropertyState : ASN1.IASN1encode //C. Günther
    {
        public enum BACnetPropertyStates
        {
            BOOLEAN_VALUE = 0,
            BINARY_VALUE = 1,
            EVENT_TYPE = 2,
            POLARITY = 3,
            PROGRAM_CHANGE = 4,
            PROGRAM_STATE = 5,
            REASON_FOR_HALT = 6,
            RELIABILITY = 7,
            STATE = 8,
            SYSTEM_STATUS = 9,
            UNITS = 10,
            UNSIGNED_VALUE = 11,
            LIFE_SAFETY_MODE = 12,
            LIFE_SAFETY_STATE = 13,
            RESTART_REASON = 14,
            DOOR_ALARM_STATE = 15,
            ACTION = 16,
            DOOR_SECURED_STATUS = 17,
            DOOR_STATUS = 18,
            DOOR_VALUE = 19,
            FILE_ACCESS_METHOD = 20,
            LOCK_STATUS = 21,
            LIFE_SAFETY_OPERATION = 22,
            MAINTENANCE = 23,
            NODE_TYPE = 24,
            NOTIFY_TYPE = 25,
            SECURITY_LEVEL = 26,
            SHED_STATE = 27,
            SILENCED_STATE = 28,
            //-- context tag 29 reserved for future addenda
            ACCESS_EVENT = 30,
            ZONE_OCCUPANCY_STATE = 31,
            ACCESS_CREDENTIAL_DISABLE_REASON = 32,
            ACCESS_CREDENTIAL_DISABLE = 33,
            AUTHENTICATION_STATUS = 34,
            BACKUP_STATE = 36,
            WRITE_STATUS = 37,
            LIGHTING_IN_PROGRESS = 38,
            LIGHTING_OPERATION = 39,
            LIGHTING_TRANSITION = 40,
            INTEGER_VALUE = 41,
            BINARY_LIGHTING_VALUE = 42,
            TIMER_STATE = 43,
            TIMER_TRANSITION = 44,
            BACNET_IP_MODE = 45,
            NETWORK_PORT_COMMAND = 46,
            NETWORK_TYPE = 47,
            NETWORK_NUMBER_QUALITY = 48,
            ESCALATOR_OPERATION_DIRECTION = 49,
            ESCALATOR_FAULT = 50,
            ESCALATOR_MODE = 51,
            LIFT_CAR_DIRECTION = 52,
            LIFT_CAR_DOOR_COMMAND = 53,
            LIFT_CAR_DRIVE_STATUS = 54,
            LIFT_CAR_MODE = 55,
            LIFT_GROUP_MODE = 56,
            LIFT_FAULT = 57,
            PROTOCOL_LEVEL = 58,
            EXTENDED_VALUE = 63

        };

        public BACnetPropertyStates type;
        public object state;

        public void ASN1encode(EncodeBuffer buffer)
        {
            //maybe get type form state!!!!
            //only BOOLEAN_VALUE and INTEGER_VALUE differnt?
            if (type == BACnetPropertyStates.BOOLEAN_VALUE)
            {
                if ((bool)state == true)
                {
                    ASN1.encode_context_enumerated(buffer, (byte)type, (uint)1);
                }
                else
                    ASN1.encode_context_enumerated(buffer, (byte)type, (uint)0);

            } else if (type == BACnetPropertyStates.INTEGER_VALUE)
            {
                ASN1.encode_context_signed(buffer, (byte)type, (int)state);

            } else
                ASN1.encode_context_enumerated(buffer, (byte)type, Convert.ToUInt32(state));

        }

        public int ASN1decode(byte[] buffer, int offset)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            type = (BACnetPropertyState.BACnetPropertyStates)tag;
            uint val;
            switch (type)
            {
                case BACnetPropertyState.BACnetPropertyStates.BINARY_VALUE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.BOOLEAN_VALUE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    bool bval = val > 0 ? true : false;
                    state = bval;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.EVENT_TYPE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetEventNotificationData.BacnetEventTypes)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.POLARITY:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetPolarity)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.PROGRAM_CHANGE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetProgramRequest)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.PROGRAM_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetProgramState)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.REASON_FOR_HALT:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetProgramError)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.RELIABILITY:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetReliability)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetEventNotificationData.BacnetEventStates)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.SYSTEM_STATUS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetDeviceStatus)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.UNITS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetUnitsId)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.UNSIGNED_VALUE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFE_SAFETY_MODE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetEventNotificationData.BacnetLifeSafetyModes)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFE_SAFETY_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetEventNotificationData.BacnetLifeSafetyStates)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.RESTART_REASON:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetRestartReason)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.DOOR_ALARM_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetDoorAlarmState)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ACTION:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetAction)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.DOOR_SECURED_STATUS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetDoorSecuredStatus)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.DOOR_STATUS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetDoorStatus)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.DOOR_VALUE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetDoorValue)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.FILE_ACCESS_METHOD:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetFileAccessMethod)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LOCK_STATUS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLockStatus)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFE_SAFETY_OPERATION:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetEventNotificationData.BacnetLifeSafetyOperations)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.MAINTENANCE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetMaintenance)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.NODE_TYPE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetNodeTypes)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.NOTIFY_TYPE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BacnetEventNotificationData.BacnetNotifyTypes)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.SECURITY_LEVEL:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetSecurityLevel)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.SHED_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetShedState)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.SILENCED_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetSilencedState)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ACCESS_EVENT:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetAccessEvents)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ZONE_OCCUPANCY_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetAccessZoneOccupancyState)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ACCESS_CREDENTIAL_DISABLE_REASON:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetAccessCredentialDisableReason)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ACCESS_CREDENTIAL_DISABLE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetAccessCredentialDisable)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.AUTHENTICATION_STATUS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetAuthenticationStatus)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.BACKUP_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetBackupState)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.WRITE_STATUS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetWriteStatus)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIGHTING_IN_PROGRESS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLightingInProgress)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIGHTING_OPERATION:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLightingOperation)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIGHTING_TRANSITION:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLightingTransition)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.INTEGER_VALUE:
                    int v;
                    len += ASN1.decode_signed(buffer, offset + len, len_value_type, out v);
                    state = v;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.BINARY_LIGHTING_VALUE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetBinaryLightingPV)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.TIMER_STATE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetTimerState)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.TIMER_TRANSITION:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetTimerTransition)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.BACNET_IP_MODE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetIPMode)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.NETWORK_PORT_COMMAND:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetNetworkPortCommand)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.NETWORK_TYPE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetNetworkType)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.NETWORK_NUMBER_QUALITY:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetNetworkNumberQuality)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ESCALATOR_OPERATION_DIRECTION:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetEscalatorOperationDirection)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ESCALATOR_FAULT:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetEscalatorFault)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.ESCALATOR_MODE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetEscalatorMode)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFT_CAR_DIRECTION:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLiftCarDirection)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFT_CAR_DOOR_COMMAND:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLiftCarDoorCommand)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFT_CAR_DRIVE_STATUS:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLiftCarDriveStatus)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFT_CAR_MODE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLiftCarMode)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFT_GROUP_MODE:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLiftGroupMode)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.LIFT_FAULT:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetLiftFault)val;
                    break;
                case BACnetPropertyState.BACnetPropertyStates.PROTOCOL_LEVEL:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = (BACnetProtocolLevel)val;
                    break;
                default:
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                    state = val;

                    break;


            }
            return len;

        }


        public override string ToString()
        {
            if (state != null)
                return state.ToString();
            else
                return "";
        }
    }

    public struct BACnetScale : ASN1.IASN1encode
    {
        private object value;
        private choicetype type;

        public enum choicetype
        {
            REAL = 0,
            INTEGER = 1
        }

        public BACnetScale(choicetype Type, object Value)
        {
            type = Type;
            value = Value;
        }

        public choicetype Type
        {
            get
            {
                return type;
            }
            set
            {

                switch(value)
                {
                    case choicetype.REAL:
                        if (this.value == null)
                            this.value = (float)0;
                        if (this.value.GetType() != typeof(float))
                            this.value = (float)Convert.ChangeType(this.value, typeof(float));
                        break;
                    case choicetype.INTEGER:
                        if (this.value == null)
                            this.value = (int)0;
                        if (this.value.GetType() != typeof(int))
                            this.value = (int)Convert.ChangeType(this.value, typeof(int));
                        break;
                    default:
                        throw new NotSupportedException();
                }
                this.type = value;
            }

        }

        public object Value
        {
            get
            {
                if (type == choicetype.REAL)
                    return (float)value;
                if (type == choicetype.INTEGER)
                    return (int)value;
                else
                    throw new NotSupportedException();
            }
            set
            {
                this.value = value;
            }
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            if (value is float)
                ASN1.encode_context_real(buffer, 0, (float)value);
            if(value is int)
                ASN1.encode_context_signed(buffer, 1, (int)value);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

            if (tag == 0)
            {
                //real
                float v;
                len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out v);
                this.value = v;
                type = choicetype.REAL;


            }
            else if (tag == 1)
            {
                //integer
                int v;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out v);
                this.value = v;
                type = choicetype.INTEGER;

            }
            else return len - 1;

            return len;



        }

    }



    public struct BACnetPrescale : ASN1.IASN1encode //C. Günther
    {
        private uint multiplier;
        private uint moduloDivide;

        public uint Multiplier
        {
            get { return multiplier; }
            set { multiplier = value; }

        }

        public uint Modulo_Divide
        {
            get { return moduloDivide; }
            set { moduloDivide = value; }
        }

        public BACnetPrescale(uint Multiplier,uint Modul0_Divide)
        {
            multiplier = Multiplier;
            moduloDivide = Modul0_Divide;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            //not done yet! CG
            ASN1.encode_context_unsigned(buffer, 0, multiplier);
            ASN1.encode_context_unsigned(buffer, 1, moduloDivide);

        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            //CG
            int len = 0;
            byte tag;
            uint len_value_type;

            //multplier
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out multiplier);
            else return len - 1;

            //moduleDivide
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out moduloDivide);

            return len;
        }

        public override string ToString()
        {
            return "multplier :" + multiplier.ToString() + " moduleDivide :" + moduloDivide.ToString();
        }
    }

    public enum BACnetProgramRequest
    {
        READY = 0,
        LOAD = 1,
        RUN = 2,
        HALT = 3,
        RESTART = 4,
        UNLOAD = 5

    }

    public enum BACnetProgramError
    {
        NORMAL = 0,
        LOAD_FAILED = 1,
        INTERNAL = 2,
        PROGRAM = 3,
        OTHER = 4,
    }

    public enum BACnetDoorAlarmState
    {
        NORMAL = 0,
        ALARM = 1,
        DOOR_OPEN_TOO_LONG = 2,
        FORCED_OPEN = 3,
        TAMPER = 4,
        DOOR_FAULT = 5,
        LOCK_DOWN = 6,
        FREE_ACCESS = 7,
        EGRESS_OPEN = 8
    }

    public enum BACnetAction
    {
        DIRECT = 0,
        REVERSE = 1

    }

    public enum BACnetDoorSecuredStatus
    {
        SECURED = 0,
        UNSECURED = 1,
        UNKNOWN = 2
    }

    public enum BACnetAccessEvents
    {
        NONE = 0,
        GRANTED = 1,
        MUSTER = 2,
        PASSBACK_DETECTED = 3,
        DURESS = 4,
        TRACE = 5,
        LOCKOUT_MAX_ATTEMPTS = 6,
        LOCKOUT_OTHER = 7,
        LOCKOUT_RELINQUISHED = 8,
        LOCKED_BY_HIGHER_PRIORITY = 9,
        OUT_OF_SERVICE = 10,
        OUT_OF_SERVICE_RELINQUISHED = 11,
        ACCOMPANIMENT_BY = 12,
        AUTHENTICATION_FACTOR_READ = 13,
        AUTHORIZATION_DELAYED = 14,
        VERIFICATION_REQUIRED = 15,
        NO_ENTRY_AFTER_GRANT = 16,
        //-- Enumerated values 128-511 are used for events which indicate that access has been denied.
        DENIED_DENY_ALL = 128,
        DENIED_UNKNOWN_CREDENTIAL = 129,
        DENIED_AUTHENTICATION_UNAVAILABLE = 130,
        DENIED_AUTHENTICATION_FACTOR_TIMEOUT = 131,
        DENIED_INCORRECT_AUTHENTICATION_FACTOR = 132,
        DENIED_ZONE_NO_ACCESS_RIGHTS = 133,
        DENIED_POINT_NO_ACCESS_RIGHTS = 134,
        DENIED_NO_ACCESS_RIGHTS = 135,
        DENIED_OUT_OF_TIME_RANGE = 136,
        DENIED_THREAT_LEVEL = 137,
        DENIED_PASSBACK = 138,
        DENIED_UNEXPECTED_LOCATION_USAGE = 139,
        DENIED_MAX_ATTEMPTS = 140,
        DENIED_LOWER_OCCUPANCY_LIMIT = 141,
        DENIED_UPPER_OCCUPANCY_LIMIT = 142,
        DENIED_AUTHENTICATION_FACTOR_LOST = 143,
        DENIED_AUTHENTICATION_FACTOR_STOLEN = 144,
        DENIED_AUTHENTICATION_FACTOR_DAMAGED = 145,
        DENIED_AUTHENTICATION_FACTOR_DESTROYED = 146,
        DENIED_AUTHENTICATION_FACTOR_DISABLED = 147,
        DENIED_AUTHENTICATION_FACTOR_ERROR = 148,
        DENIED_CREDENTIAL_UNASSIGNED = 149,
        DENIED_CREDENTIAL_NOT_PROVISIONED = 150,
        DENIED_CREDENTIAL_NOT_YET_ACTIVE = 151,
        DENIED_CREDENTIAL_EXPIRED = 152,
        DENIED_CREDENTIAL_MANUAL_DISABLE = 153,
        DENIED_CREDENTIAL_LOCKOUT = 154,
        DENIED_CREDENTIAL_MAX_DAYS = 155,
        DENIED_CREDENTIAL_MAX_USES = 156,
        DENIED_CREDENTIAL_INACTIVITY = 157,
        DENIED_CREDENTIAL_DISABLED = 158,
        DENIED_NO_ACCOMPANIMENT = 159,
        DENIED_INCORRECT_ACCOMPANIMENT = 160,
        DENIED_LOCKOUT = 161,
        DENIED_VERIFICATION_FAILED = 162,
        DENIED_VERIFICATION_TIMEOUT = 163,
        DENIED_OTHER = 164

        /*-- Enumerated values 0-511 are reserved for definition by ASHRAE.Enumerated values
        -- 512-65535 may be used by others subject to the procedures and constraints described
        -- in Clause 23.*/

    }

    public struct BACnetFaultParameter : ASN1.IASN1encode //C. Günther
    {
        public BACnetFaultType type;
        public object Value;


        public struct FaultExtended
        {
            public ushort vendorid;
            public uint extended_fault_type;
            public List<object> parameters;

            public override string ToString()
            {
                string ret = "vendorid :" + vendorid.ToString() + "fault type :" + extended_fault_type;

                int i = 1;

                foreach (object obj in parameters)
                {
                    ret = ret + "parameter " + i.ToString() + ":" + obj.ToString() + " ";
                    i++;
                }

                return ret;
            }
        }

        public enum BACnetFaultType
        {
            NONE = 0,
            FAULT_CHARACTERSTRING = 1,
            FAULT_EXTENDED = 2,
            FAULT_LIFE_SAFETY = 3,
            FAULT_STATE = 4,
            FAULT_STATUS_FLAGS = 5,
            FAULT_OUT_OF_RANGE = 6,
            FAULT_LISTED = 7
        }

        public struct FaultCharacterstring
        {
            public List<string> values;

            public override string ToString()
            {
                string ret = "";

                int i = 1;

                foreach (string str in values)
                {
                    ret = ret + "value " + i.ToString() + ":" + str + " ";
                    i++;
                }

                return ret;
            }
        }

        public struct FaultLiveSafety
        {
            public List<BacnetEventNotificationData.BacnetLifeSafetyStates> fault_values;
            public BacnetDeviceObjectPropertyReference reference;

            public override string ToString()
            {
                string ret = "";
                int i = 1;
                foreach (BacnetEventNotificationData.BacnetLifeSafetyStates ls in fault_values)
                {
                    ret = ret + "faultvalue " + i.ToString() + ":" + ls.ToString() + " ";
                    i++;
                }
                return ret + " Reference :" + reference.objectIdentifier + ":" + reference.propertyIdentifier;

            }
        }

        public struct FaultOutOfRange
        {
            public object MaxNormalValue;
            public object MinimalNormalValue;

            public override string ToString()
            {
                return "MaxNormalValue :" + MaxNormalValue.ToString() + " MinimalNormalValue :" + MinimalNormalValue.ToString();
            }
        }

        public struct FaultState
        {
            public List<BACnetPropertyState> values;

            public override string ToString()
            {
                string ret = "";
                int i = 1;
                foreach (BACnetPropertyState ps in (List<BACnetPropertyState>)values)
                {
                    ret = ret + i.ToString() + ". " + ps.type.ToString() + " " + ps.state.ToString() + " ";
                    i++;
                }

                return ret;
            }
        }

        public struct FaultStatusFlags
        {
            public BacnetDeviceObjectPropertyReference reference;

            public override string ToString()
            {
                return reference.objectIdentifier.ToString() + "-" + reference.propertyIdentifier.ToString();
            }

        }

        public struct FaultListed
        {
            public BacnetDeviceObjectPropertyReference reference;

            public override string ToString()
            {
                return reference.objectIdentifier.ToString() + "-" + reference.propertyIdentifier.ToString();
            }

        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            //not done yet!  CG
            ASN1.encode_opening_tag(buffer, (byte)type);
            switch (type)
            {
                case BACnetFaultType.FAULT_STATE:
                    FaultState fs = (FaultState)Value;
                    ASN1.encode_opening_tag(buffer, 0);
                    foreach (BACnetPropertyState ps in (List<BACnetPropertyState>)fs.values)
                    {
                        ps.ASN1encode(buffer);
                    }
                    ASN1.encode_closing_tag(buffer, 0);
                    break;
                case BACnetFaultType.FAULT_STATUS_FLAGS:
                    FaultStatusFlags sf = (FaultStatusFlags)Value;
                    BacnetDeviceObjectPropertyReference dopr1 = (BacnetDeviceObjectPropertyReference)sf.reference;
                    ASN1.encode_opening_tag(buffer, 0);
                    ASN1.bacapp_encode_device_obj_property_ref(buffer, dopr1);
                    ASN1.encode_closing_tag(buffer, 0);
                    break;
                case BACnetFaultType.FAULT_OUT_OF_RANGE:
                    //wireshark no help again?!!! or SCADA Device Sim Wrong?
                    FaultOutOfRange foor = (FaultOutOfRange)Value;
                    ASN1.encode_opening_tag(buffer, 0);
                    if (foor.MinimalNormalValue.GetType() == typeof(int))
                    {
                        ASN1.encode_application_signed(buffer, (int)foor.MinimalNormalValue);
                    }
                    else if (foor.MinimalNormalValue.GetType() == typeof(uint))
                    {
                        ASN1.encode_application_unsigned(buffer, (uint)foor.MinimalNormalValue);
                    }
                    else if (foor.MinimalNormalValue.GetType() == typeof(double))
                    {
                        ASN1.encode_application_double(buffer, (double)foor.MinimalNormalValue);
                    }
                    else if (foor.MinimalNormalValue.GetType() == typeof(float))
                    {
                        ASN1.encode_application_real(buffer, (float)foor.MinimalNormalValue);
                    }
                    ASN1.encode_closing_tag(buffer, 0);
                    ASN1.encode_opening_tag(buffer, 1);
                    if (foor.MaxNormalValue.GetType() == typeof(int))
                    {
                        ASN1.encode_application_signed(buffer, (int)foor.MaxNormalValue);
                    }
                    else if (foor.MaxNormalValue.GetType() == typeof(uint))
                    {
                        ASN1.encode_application_unsigned(buffer, (uint)foor.MaxNormalValue);
                    }
                    else if (foor.MaxNormalValue.GetType() == typeof(double))
                    {
                        ASN1.encode_application_double(buffer, (double)foor.MaxNormalValue);
                    }
                    else if (foor.MaxNormalValue.GetType() == typeof(float))
                    {
                        ASN1.encode_application_real(buffer, (float)foor.MaxNormalValue);
                    }
                    ASN1.encode_closing_tag(buffer, 1);
                    break;
                case BACnetFaultType.FAULT_EXTENDED:
                    FaultExtended fe = (FaultExtended)Value;
                    ASN1.encode_opening_tag(buffer, 0);
                    ASN1.encode_application_unsigned(buffer, fe.vendorid);
                    ASN1.encode_closing_tag(buffer, 0);
                    ASN1.encode_opening_tag(buffer, 1);
                    ASN1.encode_application_unsigned(buffer, fe.extended_fault_type);
                    ASN1.encode_closing_tag(buffer, 1);
                    ASN1.encode_opening_tag(buffer, 2);
                    foreach (object obj in fe.parameters)
                    {
                        if (obj.GetType() == typeof(uint))
                        {
                            ASN1.encode_application_unsigned(buffer, (uint)obj);
                        } else if (obj.GetType() == typeof(int))
                        {
                            ASN1.encode_application_signed(buffer, (int)obj);
                        } else if (obj.GetType() == typeof(double))
                        {
                            ASN1.encode_application_double(buffer, (double)obj);
                        } else if (obj.GetType() == typeof(float))
                        {
                            ASN1.encode_application_real(buffer, (float)obj);
                        } else if (obj.GetType() == typeof(BacnetBitString))
                        {
                            ASN1.encode_application_bitstring(buffer, (BacnetBitString)obj);
                        } else if (obj.GetType() == typeof(DateTime)) //how to get if is time or date???
                        {
                            ASN1.bacapp_encode_datetime(buffer, (DateTime)obj);
                        } else if (obj.GetType() == typeof(BacnetObjectId))
                        {
                            BacnetObjectId objid = (BacnetObjectId)obj;
                            ASN1.encode_application_object_id(buffer, objid.type, objid.instance);
                        }
                        /*  not finished
                            null NULL,
                            real REAL,
                            unsigned Unsigned,
                            boolean BOOLEAN,
                            integer INTEGER,
                            double Double,
                            octetstring OCTET STRING,
                            characterstring CharacterString,
                            bitstring BIT STRING,
                            enumerated ENUMERATED,
                            date Date,
                            time Time,
                            objectidentifier BACnetObjectIdentifier,
                            reference [0] BACnetDeviceObjectPropertyReference*/
                    }
                    ASN1.encode_closing_tag(buffer, 2);
                    break;
                case BACnetFaultType.FAULT_LISTED:
                    FaultListed fl = (FaultListed)Value;
                    BacnetDeviceObjectPropertyReference dopr = (BacnetDeviceObjectPropertyReference)fl.reference;
                    ASN1.encode_opening_tag(buffer, 0);
                    ASN1.bacapp_encode_device_obj_property_ref(buffer, dopr);
                    ASN1.encode_closing_tag(buffer, 0);
                    break;
                case BACnetFaultType.NONE:
                    break;
                case BACnetFaultType.FAULT_CHARACTERSTRING:
                    ASN1.encode_opening_tag(buffer, 0);
                    FaultCharacterstring cs = (FaultCharacterstring)Value;
                    foreach (string st in (List<string>)cs.values)
                    {
                        ASN1.encode_application_character_string(buffer, st);
                    }
                    ASN1.encode_closing_tag(buffer, 0);
                    break;
            }
            ASN1.encode_closing_tag(buffer, (byte)type);



        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            //CG
            int len = 0;

            byte tag;
            uint len_value_type;
            int tmp = 0;
            byte faultType;

            len += ASN1.decode_tag_number(buffer, offset + len, out faultType);

            type = (BACnetFaultType)faultType;
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

            switch (type)
            {
                case BACnetFaultType.FAULT_CHARACTERSTRING:
                    FaultCharacterstring fcs = new FaultCharacterstring();
                    List<string> string_values = new List<string>();
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {
                        len++;
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                        {

                            tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                            if (tag == 7)
                            {

                                len += tmp;
                                string string_value;
                                len += ASN1.decode_character_string(buffer, offset + len, (int)len_value_type, len_value_type, out string_value);

                                string_values.Add(string_value);

                            }


                        }
                        len++;


                    }
                    else
                        return len - 1;
                    fcs.values = string_values;
                    Value = fcs;

                    break;
                case BACnetFaultType.FAULT_EXTENDED:
                    //guessed


                    FaultExtended fe = new FaultExtended();
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {
                        len++;
                        len += ASN1.decode_unsigned16(buffer, offset + len, out fe.vendorid);

                    }
                    //guessed
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out fe.extended_fault_type);

                    }
                    //CG guessed
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 2))
                    {
                        len++;
                        List<object> objlist = new List<object>();
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 2))
                        {
                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            switch ((BacnetApplicationTags)tag)
                            {
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL: //null
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL: //real
                                    float float_value;
                                    len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out float_value);
                                    objlist.Add(float_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT: //unsigned
                                    uint uint_value;
                                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uint_value);
                                    objlist.Add(uint_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN: //boolean
                                    uint val;
                                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                                    bool bval = val > 0 ? true : false;
                                    objlist.Add(bval);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT: //integer
                                    int int_value;
                                    len += ASN1.decode_signed(buffer, offset + len, len_value_type, out int_value);
                                    objlist.Add(int_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DOUBLE: //double
                                    double double_value;
                                    len = ASN1.decode_double_safe(buffer, offset + len, len_value_type, out double_value);
                                    objlist.Add(double_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING: //octetstring
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING: //characterstring
                                    string string_value;
                                    len += ASN1.decode_character_string(buffer, offset + len, (int)len_value_type, len_value_type, out string_value);
                                    objlist.Add(string_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING: //bitstring
                                    BacnetBitString bit_value;
                                    len += ASN1.decode_bitstring(buffer, offset + len, len_value_type, out bit_value);
                                    objlist.Add(bit_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED: //enumerated
                                    uint e_value;
                                    len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out e_value);
                                    objlist.Add(e_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE: //date
                                    DateTime date_v;
                                    len += ASN1.decode_date_safe(buffer, offset + len, len_value_type, out date_v);
                                    objlist.Add(date_v);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME: //time
                                    DateTime time_value;
                                    len += ASN1.decode_bacnet_time_safe(buffer, offset + len, len_value_type, out time_value);
                                    objlist.Add(time_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID: //BACnetObjectIdentifier,
                                    ushort object_type = 0;
                                    uint instance = 0;
                                    len += ASN1.decode_object_id_safe(buffer, offset + len, len_value_type, out object_type, out instance);
                                    objlist.Add(new BacnetObjectId((BacnetObjectTypes)object_type, instance));
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DEVICE_OBJECT_PROPERTY_REFERENCE: //BACnetDeviceObjectPropertyReference
                                    BacnetDeviceObjectPropertyReference dopr;
                                    len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -2000, out dopr);
                                    objlist.Add(dopr);
                                    break;

                            }

                        }
                        fe.parameters = objlist;
                        len++;

                    }
                    len++;
                    Value = fe;


                    break;
                case BACnetFaultType.FAULT_LIFE_SAFETY:
                    FaultLiveSafety fls = new FaultLiveSafety();

                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {
                        len++;
                        List<BacnetEventNotificationData.BacnetLifeSafetyStates> fv = new List<BacnetEventNotificationData.BacnetLifeSafetyStates>();
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                        {
                            uint val;
                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);

                            fv.Add((BacnetEventNotificationData.BacnetLifeSafetyStates)val);
                        }
                        len++;
                        fls.fault_values = fv;
                    }
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;
                        len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -100, out fls.reference);
                        len++;

                    }
                    //len++;

                    Value = fls;

                    break;
                case BACnetFaultType.FAULT_STATE:

                    FaultState ft = new FaultState();
                    List<BACnetPropertyState> psl = new List<BACnetPropertyState>(); //wrong written propetystate???
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {
                        len++;


                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                        {
                            BACnetPropertyState ps = new BACnetPropertyState();
                            len += ps.ASN1decode(buffer, offset + len);
                            psl.Add(ps);
                        }
                    }
                    else return len - 1;
                    ft.values = psl;
                    len++;
                    Value = ft;

                    break;
                case BACnetFaultType.FAULT_STATUS_FLAGS:
                    FaultStatusFlags fsf = new FaultStatusFlags();
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {
                        len++;
                        len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -100, out fsf.reference); //why i need ADPU_LEN????

                    }
                    else return len - 1;
                    len++;
                    Value = fsf;
                    break;
                case BACnetFaultType.FAULT_OUT_OF_RANGE:
                    //Wireshark no help with 135-2016??
                    FaultOutOfRange foor = new FaultOutOfRange();
                    //min
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {

                        len++;
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                        if ((BacnetApplicationTags)tag == BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL)
                        {
                            //not tested!
                            float v;
                            len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out v);

                            foor.MinimalNormalValue = v;

                        }
                        else if ((BacnetApplicationTags)tag == BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                        {
                            uint v;
                            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out v);

                            foor.MinimalNormalValue = v;

                        }
                        else if ((BacnetApplicationTags)tag == BacnetApplicationTags.BACNET_APPLICATION_TAG_DOUBLE)
                        {
                            //not tested!
                            double v;
                            len += ASN1.decode_double_safe(buffer, offset + len, len_value_type, out v);

                            foor.MinimalNormalValue = v;
                        }
                        else if ((BacnetApplicationTags)tag == BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT)
                        {
                            //not tested!
                            int v;
                            len += ASN1.decode_signed(buffer, offset + len, len_value_type, out v);

                            foor.MinimalNormalValue = v;
                        }
                        len++; //close
                    }
                    //max
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                        if (tag == 1)
                        {
                            //not tested!
                            float v;
                            len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out v);

                            foor.MaxNormalValue = v;

                        }
                        else if (tag == 2)
                        {
                            uint v;
                            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out v);

                            foor.MaxNormalValue = v;

                        }
                        else if (tag == 3)
                        {
                            //not tested!
                            double v;
                            len += ASN1.decode_double_safe(buffer, offset + len, len_value_type, out v);

                            foor.MaxNormalValue = v;
                        }
                        else if (tag == 4)
                        {
                            //not tested!
                            int v;
                            len += ASN1.decode_signed(buffer, offset + len, len_value_type, out v);

                            foor.MaxNormalValue = v;
                        }
                        len++; //close
                    }

                    Value = foor;

                    break;
                case BACnetFaultType.FAULT_LISTED:
                    FaultListed fl = new FaultListed();
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {
                        len++;
                        len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -100, out fl.reference); //why i need ADPU_LEN????

                    }
                    else return len - 1;
                    len++;
                    Value = fl;

                    break;

                default:
                    return len - 1;
            }
            len++;
            return len;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public struct BACnetAccessRule : ASN1.IASN1encode //C. Günther
    {
        private timerangerspecifier time_range_specifier;
        private BacnetDeviceObjectPropertyReference time_range;
        private locationspecifier location_specifier;
        private BacnetDeviceObjectReference location;
        private bool enable;

        public enum timerangerspecifier
        {
            specified =0,
            always = 1
        }

        public enum locationspecifier
        {
            specified =0,
            all =1
        }

        public timerangerspecifier Time_Range_Specifier
        {
            get { return time_range_specifier; }
            set { time_range_specifier = value; }
        }

        public BacnetDeviceObjectPropertyReference Time_Range
        {
            get { return time_range; }
            set { time_range = value; }
        }

        public locationspecifier Location_Specifier
        {
            get { return location_specifier; }
            set { location_specifier = value; }
        }

        public BacnetDeviceObjectReference Location
        {
            get { return location; }
            set { location = value; }
        }

        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_enumerated(buffer, 0, Convert.ToUInt32(time_range_specifier));
            if(time_range_specifier == timerangerspecifier.specified)
            {
                ASN1.encode_opening_tag(buffer, 1);
                ASN1.bacapp_encode_device_obj_property_ref(buffer, time_range);
                ASN1.encode_closing_tag(buffer, 1);
            }
            ASN1.encode_context_enumerated(buffer, 2, Convert.ToUInt32(location_specifier));
            if (time_range_specifier == timerangerspecifier.specified)
            {
                ASN1.encode_opening_tag(buffer, 3);
                location.ASN1encode(buffer);
                ASN1.encode_closing_tag(buffer, 3);
            }
            ASN1.encode_context_boolean(buffer, 4, enable);
        }


        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                uint u_val;
                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out u_val);
                time_range_specifier = (timerangerspecifier)u_val;

            }
            else return len - 1;

            if (time_range_specifier == 0) //specified (1 = always)
            {
                if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                {
                    len++;
                    len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -5000, out this.time_range);
                    len++; //closing tag

                }

            }
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

            if (tag == 2)
            {
                uint u_val;
                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out u_val);
                location_specifier = (locationspecifier)u_val;
            }
            else return len - 1;
            if (location_specifier == 0) //specified (1 = all)
            {
                if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 3))
                {
                    len++;
                    BacnetDeviceObjectReference v = new BacnetDeviceObjectReference();
                    len += v.ASN1decode(buffer, offset + len, (uint)len_value - (uint)len);
                    len++; //closing tag
                    this.location = v;

                }

            }
            //boolean enable
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            uint val;
            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
            enable = val > 0 ? true : false;

            //len++;
            return len;
        }

    }

    public struct BACnetLightingCommand : ASN1.IASN1encode //C. Günther
    {
        private BACnetLightingOperation operation;
        private float target_level;
        private float ramp_rate;
        private float step_increment;
        private uint fade_time;
        private uint priority;

        public BACnetLightingOperation Operation
        {
            get { return operation; }
            set { operation = value; }

        }

        public float Target_Level
        {
            get { return target_level; }
            set { target_level = value; }

        }

        public float Ramp_Rate
        {
            get { return ramp_rate; }
            set { ramp_rate = value; }
        }

        public float Step_Increment
        {
            get { return step_increment; }
            set { step_increment = value; }
        }

        public uint Fade_Time
        {
            get { return fade_time; }
            set { fade_time = value; }
        }

        public uint Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public BACnetLightingCommand(BACnetLightingOperation Operation)
        {
            operation = Operation;
            target_level = -1;
            ramp_rate = -1;
            step_increment = -1;
            fade_time = 0;
            priority = 0;
        }

        public BACnetLightingCommand(BACnetLightingOperation Operation, float Target_Level, float Ramp_Rate, float Step_Increment, uint Fade_Time, uint Priority)
        {

            operation = Operation;
            target_level = Target_Level;

            ramp_rate = Ramp_Rate;
            step_increment = Step_Increment;
            fade_time = Fade_Time;
            priority = Priority;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, Convert.ToUInt32(operation));
            if (target_level >= 0 && target_level <= 100)
                ASN1.encode_context_real(buffer, 1, target_level);
            if (ramp_rate >= 0.1 && ramp_rate <= 100.0)
                ASN1.encode_context_real(buffer, 2, ramp_rate);
            if (step_increment >= 0.1 && step_increment <= 100.0)
                ASN1.encode_context_real(buffer, 3, step_increment);
            if (fade_time >= 100 && fade_time <= 86400000)
                ASN1.encode_context_unsigned(buffer, 4, fade_time);
            if (priority >= 1 && priority <= 16)
                ASN1.encode_context_unsigned(buffer, 5, priority);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;

            uint len_value_type;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

            if (tag == 0) //operation
            {
                uint op;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out op);
                operation = (BACnetLightingOperation)op;
            }

            //set values
            target_level = -1;
            ramp_rate = -1;
            step_increment = -1;
            fade_time = 0;
            priority = 0;

            if (len < (len_value - offset - 1)) //this works somehow
            {
                //optional Target Level
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 1)
                {
                    len++;
                    len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out target_level);
                }
            }
            if (len < (len_value - offset - 1))
            {
                //optional ramp rate
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 2)
                {
                    len++;
                    len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out ramp_rate);
                }
            }
            if (len < (len_value - offset - 1))
            {
                //optional step increment
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 3)
                {
                    len++;
                    len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out step_increment);
                }
            }
            if (len < (len_value - offset - 1))
            {
                //optional fade time
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 4)
                {
                    len++;
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out fade_time);
                }
            }

            if (len < (len_value - offset - 1))
            {
                //optional priority
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 5)
                {
                    len++;
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out priority);
                }
            }

            return len;
        }

    }

    public struct BACnetChannelValue : ASN1.IASN1encode //C. Günther
    {

        private ApplicationTag tag;
        private object value;

        public ApplicationTag Tag
        {
            get { return tag; }
            set {
                this.Value = null;
                tag = value; }
        }

        public object Value
        {
            get
            {
                switch (tag)
                {
                    case ApplicationTag.NULL:
                        return null;
                    case ApplicationTag.BOOLEAN:
                        return (bool)Convert.ChangeType(value, typeof(bool));
                    case ApplicationTag.UNSIGNED_INT:
                        return (uint)Convert.ChangeType(value, typeof(uint));
                    case ApplicationTag.SIGNED_INT:
                        return (int)Convert.ChangeType(value, typeof(int));
                    case ApplicationTag.REAL:
                        return (float)Convert.ChangeType(value, typeof(float));
                    case ApplicationTag.DOUBLE:
                        return (double)Convert.ChangeType(value, typeof(double));
                    case ApplicationTag.OCTET_STRING:
                        return (byte[])Convert.ChangeType(value, typeof(byte[]));
                    case ApplicationTag.CHARACTER_STRING:
                        return (string)Convert.ChangeType(value, typeof(string));
                    case ApplicationTag.BIT_STRING:
                        return (BacnetBitString)Convert.ChangeType(value, typeof(BacnetBitString));
                    case ApplicationTag.ENUMERATED:
                        return (uint)Convert.ChangeType(value, typeof(uint));
                    case ApplicationTag.DATE:
                    case ApplicationTag.TIME:
                        return (DateTime)Convert.ChangeType(value, typeof(DateTime));
                    case ApplicationTag.OBJECT_ID:
                        return (BacnetObjectId)Convert.ChangeType(value, typeof(BacnetObjectId));
                    case ApplicationTag.LIGHTING_COMMAND:
                        return (BACnetLightingCommand)Convert.ChangeType(value, typeof(BACnetLightingCommand));
                    default:
                        throw new NotSupportedException();
                }
            }
            set
            {
                switch (tag)
                {
                    case ApplicationTag.NULL:
                        this.value = null;
                        break;
                    case ApplicationTag.BOOLEAN:
                        if (value == null)
                            value = (bool) false;
                        if (value.GetType() != typeof(bool))
                            value = (bool)Convert.ChangeType(value, typeof(bool));
                        this.value = value;
                        break;
                    case ApplicationTag.SIGNED_INT:
                        if (value == null)
                            value = (int)0;
                        if (value.GetType() != typeof(int))
                            value = (int)Convert.ChangeType(value, typeof(int));
                        this.value = value;
                        break;
                    case ApplicationTag.REAL:
                        if (value == null)
                            value = (float)0;
                        if (value.GetType() != typeof(double))
                            value = (float)Convert.ChangeType(value, typeof(float));
                        this.value = value;
                        break;
                    case ApplicationTag.DOUBLE:
                        if (value == null)
                            value = (double)0;
                        if (value.GetType() != typeof(double))
                            value = (double)Convert.ChangeType(value, typeof(double));
                        this.value = value;
                        break;
                    case ApplicationTag.OCTET_STRING:
                        if (value == null)
                            value = new byte[100];
                        if (value.GetType() != typeof(byte[]))
                            value = (byte[])Convert.ChangeType(value, typeof(byte[]));
                        this.value = value;
                        break;
                    case ApplicationTag.CHARACTER_STRING:
                        if (value == null)
                            value = (string)"";
                        if (value.GetType() != typeof(string))
                            value = (string)Convert.ChangeType(value, typeof(string));
                        this.value = value;
                        break;
                    case ApplicationTag.BIT_STRING:
                        if (value == null)
                            value = new BacnetBitString();
                        if (value.GetType() != typeof(BacnetBitString))
                            value = (BacnetBitString)Convert.ChangeType(value, typeof(BacnetBitString));
                        this.value = value;
                        break;
                    case ApplicationTag.LIGHTING_COMMAND:
                        if (value == null)
                            value = new BACnetLightingCommand();
                        if (value.GetType() != typeof(BACnetLightingCommand))
                            value = (BACnetLightingCommand)Convert.ChangeType(value, typeof(BACnetLightingCommand));
                        this.value = value;
                        break;
                    case ApplicationTag.OBJECT_ID:
                        if (value == null)
                            value = new BacnetObjectId();
                        if (value.GetType() != typeof(BacnetObjectId))
                            value = (BacnetObjectId)Convert.ChangeType(value, typeof(BacnetObjectId));
                        this.value = value;
                        break;
                    case ApplicationTag.UNSIGNED_INT:
                    case ApplicationTag.ENUMERATED:
                        if (value == null)
                            value = (uint)0;
                        if (value.GetType() != typeof(uint))
                            value = (uint)Convert.ChangeType(value, typeof(uint));
                        this.value = value;
                        break;
                    case ApplicationTag.DATE:
                    case ApplicationTag.TIME:
                        if (value == null)
                            value = new DateTime();
                        if (value.GetType() != typeof(DateTime))
                            value = (DateTime)Convert.ChangeType(value, typeof(DateTime));
                        this.value = value;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

        }

        public enum ApplicationTag
        {
            NULL = 0,
            BOOLEAN = 1,
            UNSIGNED_INT = 2,
            SIGNED_INT = 3,
            REAL = 4,
            DOUBLE = 5,
            OCTET_STRING = 6,
            CHARACTER_STRING = 7,
            BIT_STRING = 8,
            ENUMERATED = 9,
            DATE = 10,
            TIME = 11,
            OBJECT_ID = 12,
            LIGHTING_COMMAND = 13,
        }

        public BACnetChannelValue(ApplicationTag Tag, object Value)
        {
            this.tag = Tag;
            this.value = Value;
        }

        public override string ToString()
        {
            return "tag :" + tag + " Value :" + value;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            if (value.GetType() == typeof(BACnetLightingCommand))
            {
                ASN1.encode_opening_tag(buffer, 0);
                BACnetLightingCommand lc = (BACnetLightingCommand)value;
                lc.ASN1encode(buffer);
                ASN1.encode_closing_tag(buffer, 0);
            }
            else
            {
                switch (tag)
                {
                    case ApplicationTag.NULL:
                        break;
                    case ApplicationTag.BOOLEAN:
                        ASN1.encode_application_boolean(buffer, Convert.ToBoolean(value));
                        break;
                    case ApplicationTag.UNSIGNED_INT:
                        ASN1.encode_application_unsigned(buffer, Convert.ToUInt64(value));
                        break;
                    case ApplicationTag.SIGNED_INT:
                        ASN1.encode_application_signed(buffer, Convert.ToInt64(value));
                        break;
                    case ApplicationTag.REAL:
                        ASN1.encode_application_real(buffer, Convert.ToSingle(value));
                        break;
                    case ApplicationTag.DOUBLE:
                        ASN1.encode_application_double(buffer, Convert.ToDouble(value));
                        break;
                    case ApplicationTag.OCTET_STRING:
                        ASN1.encode_application_octet_string(buffer, (byte[])value, 0, ((byte[])value).Length);
                        break;
                    case ApplicationTag.CHARACTER_STRING:
                        ASN1.encode_application_character_string(buffer, value.ToString());
                        break;
                    case ApplicationTag.BIT_STRING:
                        ASN1.encode_application_bitstring(buffer, (BacnetBitString)value);
                        break;
                    case ApplicationTag.ENUMERATED:
                        ASN1.encode_application_enumerated(buffer, Convert.ToUInt32(value));
                        break;
                    case ApplicationTag.DATE:
                        ASN1.encode_application_date(buffer, Convert.ToDateTime(value));
                        break;
                    case ApplicationTag.TIME:
                        ASN1.encode_application_date(buffer, Convert.ToDateTime(value));
                        break;
                    case ApplicationTag.OBJECT_ID:
                        ASN1.encode_application_object_id(buffer, ((BacnetObjectId)value).type, ((BacnetObjectId)value).instance);
                        break;
                    default:
                        throw new Exception("I cannot encode this, isn't BACnetChannelValue");

                }

            }

        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte _tag;
            uint len_value_type;


            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {

                len++;
                BACnetLightingCommand lc = new BACnetLightingCommand();
                len += lc.ASN1decode(buffer, offset + len, len_value);
                value = lc;
                len++;
            }
            else
            {

                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out _tag, out len_value_type);
                tag = (ApplicationTag)_tag;
                switch (tag)
                {
                    case ApplicationTag.NULL:
                        value = null;
                        break;
                    case ApplicationTag.BOOLEAN:
                        uint val;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                        bool bval = val > 0 ? true : false;
                        value = bval;
                        break;
                    case ApplicationTag.UNSIGNED_INT:
                        uint uint_value;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uint_value);
                        value = uint_value;
                        break;
                    case ApplicationTag.SIGNED_INT:
                        int int_value;
                        len += ASN1.decode_signed(buffer, offset + len, len_value_type, out int_value);
                        value = int_value;
                        break;
                    case ApplicationTag.REAL:
                        float float_value;
                        len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out float_value);
                        value = float_value;
                        break;
                    case ApplicationTag.DOUBLE:
                        double double_value;
                        len = ASN1.decode_double_safe(buffer, offset + len, len_value_type, out double_value);
                        value = double_value;
                        break;
                    case ApplicationTag.OCTET_STRING:
                        byte[] octet_string = new byte[len_value_type];
                        len = ASN1.decode_octet_string(buffer, offset + len, (int)len_value_type, octet_string, 0, len_value_type); //maybe
                        value = octet_string;
                        break;
                    case ApplicationTag.CHARACTER_STRING:
                        string string_value;
                        len += ASN1.decode_character_string(buffer, offset + len, (int)len_value_type, len_value_type, out string_value);
                        value = string_value;
                        break;
                    case ApplicationTag.BIT_STRING:
                        BacnetBitString bit_value;
                        len += ASN1.decode_bitstring(buffer, offset + len, len_value_type, out bit_value);
                        value = bit_value;
                        break;
                    case ApplicationTag.ENUMERATED:
                        uint e_value;
                        len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out e_value);
                        value = e_value;
                        break;
                    case ApplicationTag.DATE:
                        DateTime date_v;
                        len += ASN1.decode_date_safe(buffer, offset + len, len_value_type, out date_v);
                        value = date_v;
                        break;
                    case ApplicationTag.TIME:
                        DateTime time_value;
                        len += ASN1.decode_bacnet_time_safe(buffer, offset + len, len_value_type, out time_value);
                        value = time_value;
                        break;
                    case ApplicationTag.OBJECT_ID:
                        ushort object_type = 0;
                        uint instance = 0;
                        len += ASN1.decode_object_id_safe(buffer, offset + len, len_value_type, out object_type, out instance);
                        value = new BacnetObjectId((BacnetObjectTypes)object_type, instance);
                        break;
                    default:
                        throw new Exception("I cannot decode this, isn't BACnetChannelValue");

                }
            }


            return len;

        }

    }

    public struct BACnetGroupChannelValue : ASN1.IASN1encode //C. Günther
    {
        public ushort channel;
        public uint overriding_priority;
        public BACnetChannelValue value;

        public BACnetGroupChannelValue(ushort Channel, BACnetChannelValue Value, uint Overriding_prio = 0)
        {
            channel = Channel;
            overriding_priority = Overriding_prio;
            value = Value;
        }

        public override string ToString()
        {
            return " channel :" + channel + " overrinding_prio :" + overriding_priority + " value :" + value;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_enumerated(buffer, 0, channel);
            if (overriding_priority > 0 && overriding_priority < 17) //optional
                ASN1.encode_context_unsigned(buffer, 1, overriding_priority);
            value.ASN1encode(buffer);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {

            int len = 0;

            byte tag;
            uint len_value_type;


            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                uint u_v;
                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out u_v);
                channel = (ushort)u_v;
            }
            else
                return len - 1;

            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1) //optional
            {
                len++;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out overriding_priority);
            }

            value = new BACnetChannelValue();
            len += value.ASN1decode(buffer, offset + len, len_value);

            return len;
        }


    }

    public struct BACnetActionCommand : ASN1.IASN1encode //christopher Günther
    {
        private bool opt_device_identifier;
        private BacnetObjectId device_identifier;
        private BacnetObjectId object_identifier;
        private BacnetPropertyIds property_identifier;
        private bool opt_property_array_index;
        private uint property_array_index;
        private BacnetValue property_value;
        private bool opt_priority;
        private uint priority;
        private bool opt_post_delay;
        private uint post_delay;
        private bool quit_on_failure;
        private bool write_successful;

        public BacnetObjectId Device_Identifier
        {
            get { return device_identifier; }
            set {
                opt_device_identifier = true;
                device_identifier = value; }
        }

        public BacnetObjectId Object_Identifier
        {
            get { return object_identifier; }
            set { object_identifier = value; }

        }
        public BacnetPropertyIds Property_Identifier
        {
            get { return property_identifier; }
            set { property_identifier = value; }
        }

        public uint Property_Array_Index
        {
            get { return property_array_index; }
            set {
                if((uint) value != ASN1.BACNET_ARRAY_ALL)
                {
                    opt_property_array_index = true;
                }else
                    opt_property_array_index = false;
                property_array_index = value; }
        }

        public BacnetValue Property_Value
        {
            get { return property_value; }
            set { property_value = value; }
        }
        public uint Priority
        {
            get { return priority; }
            set {
                if ((uint)value > 0 && (uint)value < 17)
                    opt_priority = true;
                else
                    opt_priority = false;
                priority = value; }
        }
        public uint Post_Delay
        {
            get { return post_delay; }
            set {
                opt_post_delay = true;
                post_delay = value; }
        }
        public bool Quit_On_Failure
        {
            get { return quit_on_failure; }
            set { quit_on_failure = value; }
        }
        public bool Write_Successful
        {
            get { return write_successful; }
            set { write_successful = value; }
        }

        public BACnetActionCommand(BacnetObjectId Object_Identifier, BacnetPropertyIds Property_Identifier, BacnetValue Property_Value, bool Quit_On_Failure, bool Write_Successful, bool Opt_Device_Identifier = false, bool Opt_Property_Array_Index = false, bool Opt_Priority = false, bool Opt_Post_Delay = false)
        {
            opt_device_identifier = Opt_Device_Identifier;
            device_identifier = new BacnetObjectId(); ;
            object_identifier = Object_Identifier;
            property_identifier = Property_Identifier;
            opt_property_array_index = Opt_Property_Array_Index;
            property_array_index = ASN1.BACNET_ARRAY_ALL;
            property_value = Property_Value;
            opt_priority = Opt_Priority;
            priority = 0;
            opt_post_delay = Opt_Post_Delay;
            post_delay = 0;
            quit_on_failure = Quit_On_Failure;
            write_successful = Write_Successful;
        }


        public void ASN1encode(EncodeBuffer buffer)
        {

            if (opt_device_identifier)
                ASN1.encode_context_object_id(buffer, 0, device_identifier.type, device_identifier.instance);
            ASN1.encode_context_object_id(buffer, 1, object_identifier.type, object_identifier.instance);
            ASN1.encode_context_unsigned(buffer, 2, (uint)property_identifier);
            if (opt_property_array_index)
                ASN1.encode_context_unsigned(buffer, 3, (uint)property_array_index);
            ASN1.encode_opening_tag(buffer, 4);
            ASN1.bacapp_encode_application_data(buffer, property_value);
            ASN1.encode_closing_tag(buffer, 4);
            if (opt_priority)
                ASN1.encode_context_unsigned(buffer, 5, priority);
            if (opt_post_delay)
                ASN1.encode_context_unsigned(buffer, 6, post_delay);
            ASN1.encode_context_boolean(buffer, 7, quit_on_failure);
            ASN1.encode_context_boolean(buffer, 8, write_successful);

        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                len++;
                opt_device_identifier = true;
                ushort obj_type;
                uint instance;
                len += ASN1.decode_object_id(buffer, offset + len, out obj_type, out instance);
                device_identifier = new BacnetObjectId((BacnetObjectTypes)obj_type, instance);
            }
            else
                opt_device_identifier = false;
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                len++;
                ushort obj_type;
                uint instance;
                len += ASN1.decode_object_id(buffer, offset + len, out obj_type, out instance);
                object_identifier = new BacnetObjectId((BacnetObjectTypes)obj_type, instance);

            }
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
            {
                len++;
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                property_identifier = (BacnetPropertyIds)u_val;
            }
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 3)
            {
                len++;
                opt_property_array_index = true;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out property_array_index);

            }
            else
            {
                opt_property_array_index = false;
                property_array_index = ASN1.BACNET_ARRAY_ALL;
            }
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 4))
            {
                len++;
                len += ASN1.bacapp_decode_application_data(buffer, offset + len, (int)len_value, object_identifier.type, property_identifier, out property_value);
                len++;
            }
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 5)
            {
                len++;
                opt_priority = true;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out priority);

            }
            else
                opt_priority = false;
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 6)
            {
                len++;
                opt_post_delay = true;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out post_delay);

            }
            else
                opt_post_delay = false;

            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 7)
            {
                len++;
                uint u_val;

                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                quit_on_failure = u_val > 0 ? true : false;


            }

            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 8)
            {
                len++;
                uint u_val;

                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                write_successful = u_val > 0 ? true : false;


            }

            return len;
        }


    }

    public struct BACnetActionList : ASN1.IASN1encode //christopher Günther
    {
        public List<BACnetActionCommand> actions { get; set; }

        public override string ToString()
        {
            string ret = "";
            int i = 0;
            foreach (BACnetActionCommand ac in actions)
            {
                ret = ret + "Index" + i + " " + ac;
            }
            return ret;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_opening_tag(buffer, 0);
            foreach (BACnetActionCommand ac in actions)
            {
                ac.ASN1encode(buffer);
            }
            ASN1.encode_closing_tag(buffer, 0);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;


            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {
                len++;
                actions = new List<BACnetActionCommand>();
                while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                {

                    BACnetActionCommand ac = new BACnetActionCommand();
                    len += ac.ASN1decode(buffer, offset + len, len_value);
                    actions.Add(ac);
                }
                len++;//closing tag

            }
            else
                return len - 1;

            return len;
        }
    }

    public struct BACnetBDTEntry //C. Günther
    {
        public BACnetHostNPort bbmd_address;
        public bool opt_broadcast_mask;
        public byte[] broadcast_mask; //shall be present if BACnet/IP, and absent for BACnet/IPv6



        public override string ToString()
        {
            string ret = bbmd_address.ToString();
            if (opt_broadcast_mask)
            {
                ret += " broadcast_mask :" + BitConverter.ToString(broadcast_mask);
            }
            return ret;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_opening_tag(buffer, 0);
            bbmd_address.ASN1encode(buffer);
            ASN1.encode_closing_tag(buffer, 0);
            if (opt_broadcast_mask)
            {
                ASN1.encode_tag(buffer, 1, true, (uint)broadcast_mask.Length);
                foreach (byte b in broadcast_mask)
                    buffer.Add(b);

            }

        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {
                len++;
                bbmd_address = new BACnetHostNPort();
                len += bbmd_address.ASN1decode(buffer, offset + len, len_value);
                len++;
            }
            else
                return len - 1;
            //optional broadcast-mask
            if (len < len_value)
            {
                opt_broadcast_mask = true;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                byte[] b_val = new byte[len_value_type];
                len += ASN1.decode_octet_string(buffer, offset + len, (int)len_value, b_val, 0, len_value_type);
                broadcast_mask = b_val;

            }
            else
                opt_broadcast_mask = false;


            return len;
        }

    }

    public struct BACnetHostAddress //C. Günther
    {
        public BACnetHostAddressType type_tag;
        public object value;

        public BACnetHostAddressType Type
        {
            get { return type_tag; }
            set { type_tag = value; }
        }

        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public enum BACnetHostAddressType
        {
            none = 0,
            ip_address = 1,
            name = 2
        }

        public override string ToString()
        {
            if (type_tag == BACnetHostAddressType.ip_address)
            {
                byte[] b_val = (byte[])value;
                if (b_val.Length == 4) //ipv4
                    return type_tag + " " + b_val[0] + "." + b_val[1] + "." + b_val[2] + "." + b_val[3];
                return type_tag + " " + BitConverter.ToString(b_val); ; //needs to be fixed for ipv6
            }
            return type_tag + " " + value;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            switch (type_tag)
            {
                case BACnetHostAddressType.none:
                    ASN1.encode_tag(buffer, 0, true, 0);
                    break;
                case BACnetHostAddressType.ip_address:
                    if (value is byte[])
                    {
                        byte[] b_val = (byte[])value;
                        ASN1.encode_tag(buffer, 1, true, (uint)b_val.Length);
                        foreach (byte b in b_val)
                            buffer.Add(b);
                    }
                    break;
                case BACnetHostAddressType.name:
                    if (value is string)
                    {
                        ASN1.encode_context_character_string(buffer, 2, (string)value);
                    }
                    break;

            }
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

            type_tag = (BACnetHostAddressType)tag;
            switch (type_tag)
            {
                case BACnetHostAddressType.none:
                    value = null;
                    break;
                case BACnetHostAddressType.ip_address:
                    byte[] b_val = new byte[len_value_type];
                    len += ASN1.decode_octet_string(buffer, offset + len, (int)len_value, b_val, 0, len_value_type);
                    value = b_val;
                    break;
                case BACnetHostAddressType.name:
                    string st_v;
                    len += ASN1.decode_character_string(buffer, offset + len, (int)len_value, len_value_type, out st_v);
                    value = st_v;
                    break;

            }
            return len;
        }

    }

    public struct BACnetHostNPort //C. Günther
    {
        private BACnetHostAddress host;
        private ushort port;

        public BACnetHostAddress Host
        {
            get { return host; }
            set { host = value; }
        }

        public ushort Port
        {
            get { return port; }
            set { port = value; }
        }

        public override string ToString()
        {
            return host + " Port :" + port;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_opening_tag(buffer, 0);
            host.ASN1encode(buffer);
            ASN1.encode_closing_tag(buffer, 0);
            ASN1.encode_context_unsigned(buffer, 1, port);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {
                len++;
                host = new BACnetHostAddress();
                len += host.ASN1decode(buffer, offset + len, len_value);
                len++;
            }
            else
                return len - 1;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                port = (ushort)u_val;
            }

            return len;

        }
    }

    public struct BACnetFDTEntry //C. Günther
    {
        public byte[] bacnetip_address;
        public ushort time_to_live;
        public ushort remaining_time_to_live;

        public override string ToString()
        {
            string ret = "";
            if (bacnetip_address.Length == 6) //ipv4
            {
                ret += "IPV4 Address :" + bacnetip_address[0] + "." + bacnetip_address[1] + "." + bacnetip_address[2] + "." + bacnetip_address[3] + ":" + (ushort)((bacnetip_address[4] << 8) | (bacnetip_address[5] << 0));
            }
            else if (bacnetip_address.Length == 18) //ipv6 no idea
                ret += "IPV6 Address :" + bacnetip_address.ToString();
            ret += " time to live :" + time_to_live + " remaining time to live :" + remaining_time_to_live;
            return ret;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_tag(buffer, 0, true, (uint)bacnetip_address.Length);
            foreach (byte b in bacnetip_address)
                buffer.Add(b);
            ASN1.encode_context_unsigned(buffer, 1, time_to_live);
            ASN1.encode_context_unsigned(buffer, 2, remaining_time_to_live);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            //ip address
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                bacnetip_address = new byte[len_value_type];
                len += ASN1.decode_octet_string(buffer, offset + len, (int)len_value, bacnetip_address, 0, len_value_type);

            }
            else
                return len - 1;
            //time to live
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {

                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                time_to_live = (ushort)u_val;
            }
            //remaining_time_to_live
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
            {

                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                remaining_time_to_live = (ushort)u_val;
            }


            return len;
        }
    }

    public struct BACnetAddressBinding //C. Günther
    {
        private BacnetObjectId device_identifier;
        private BacnetAddress device_address;

        public BacnetObjectId Device_Identifier
        {
            get { return device_identifier; }
            set { device_identifier = value; }
        }
        public BacnetAddress Device_Address
        {
            get { return device_address; }
            set { device_address = value; }
        }

        public override string ToString()
        {
            try
            {
                return "Device_Identifier:" + device_identifier + "Network_Number:" + device_address.net + "mac_address:" + BitConverter.ToString(device_address.adr);
            }
            catch
            {
                return "";
            }
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_application_object_id(buffer, device_identifier.type, device_identifier.instance);
            ASN1.encode_application_unsigned(buffer, device_address.net);
            ASN1.encode_application_octet_string(buffer, device_address.adr, 0, device_address.adr.Length);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            // device_identifier
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID)
            {
                ushort obj_typ;
                uint obj_instance;
                len += ASN1.decode_object_id_safe(buffer, offset + len, len_value_type, out obj_typ, out obj_instance);
                device_identifier = new BacnetObjectId((BacnetObjectTypes)obj_typ, obj_instance);
            }
            else
                return len;

            //device_address
            device_address = new BacnetAddress();
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

            if (tag == (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                device_address.net = (ushort)u_val;
            }

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

            if (tag == (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING)
            {
                byte[] b_val = new byte[len_value_type];
                len += ASN1.decode_octet_string(buffer, offset + len, (int)len_value, b_val, 0, len_value_type);
                device_address.adr = b_val;
            }
            return len;
        }

    }

    public struct BACnetVMACEntry : ASN1.IASN1encode  //C. Günther
    {
        public byte[] virtual_mac_address;
        public byte[] native_mac_address;

        public override string ToString()
        {
            return "Virtual MAC :" + BitConverter.ToString(virtual_mac_address) + " Native MAC :" + BitConverter.ToString(native_mac_address);
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_tag(buffer, 0, true, (uint)virtual_mac_address.Length);
            foreach (byte b in virtual_mac_address)
                buffer.Add(b);
            ASN1.encode_tag(buffer, 1, true, (uint)native_mac_address.Length);
            foreach (byte b in native_mac_address)
                buffer.Add(b);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                virtual_mac_address = new byte[len_value_type];
                len += ASN1.decode_octet_string(buffer, offset + len, (int)len_value, virtual_mac_address, 0, len_value_type);

            }
            else
                return len - 1;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                native_mac_address = new byte[len_value_type];
                len += ASN1.decode_octet_string(buffer, offset + len, (int)len_value, native_mac_address, 0, len_value_type);

            }

            return len;
        }


    }

    public struct BACnetAccumulatorRecord : ASN1.IASN1encode //C. Günther
    {
        private DateTime timestamp;
        private uint present_value;
        private uint accumulated_value;
        private status accumulator_status;

        public DateTime Timestamp
        {
            get { return timestamp; }
            set { timestamp = value; }
        }

        public uint Present_Value
        {
            get { return present_value; }
            set { present_value = value; }
        }

        public uint Accumulated_Value
        {
            get { return accumulated_value; }
            set { accumulated_value = value; }
        }

        public status Accumulator_Status
        {
            get { return accumulator_status; }
            set { accumulator_status = value; }
        }

        public enum status
        {
            NORMAL = 0,
            STARTING = 1,
            RECOVERED = 2,
            ABNORMAL = 3,
            FAILED = 4
        }

        public BACnetAccumulatorRecord(DateTime Timestamp, uint Present_Value, uint Accumulated_Value, status Accumulator_Status)
        {
            timestamp = Timestamp;
            present_value = Present_Value;
            accumulated_value = Accumulated_Value;
            accumulator_status = Accumulator_Status;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.bacapp_encode_context_datetime(buffer, 0, timestamp);
            ASN1.encode_context_unsigned(buffer, 1, present_value);
            ASN1.encode_context_unsigned(buffer, 2, accumulated_value);
            ASN1.encode_context_enumerated(buffer, 3, Convert.ToUInt32(accumulator_status));
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {

                len += ASN1.decode_bacnet_datetime(buffer, offset + len, out timestamp);

            }
            else
                return len - 1;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {

                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out present_value);

            }
            else
                return len - 1;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
            {

                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out accumulated_value);

            }
            else
                return len - 1;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 3)
            {
                uint u_val;
                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out u_val);
                accumulator_status = (status)u_val;

            }
            else
                return len - 1;

            return len;


        }



    }

    public struct BACnetRouterEntry : ASN1.IASN1encode //christopher Günther
    {
        public ushort network_number;
        public byte[] mac_address;
        public BACnetNetworkReachability status;
        public bool opt_performance_index;
        public byte performance_index;


        public enum BACnetNetworkReachability //name removed again from 135-2016??
        {
            available = 0,
            busy = 1,
            disconnected = 2
        }

        public override string ToString()
        {

            string ret = "network number :" + network_number + " MAC Address :" + BitConverter.ToString(mac_address) + " Status :" + status;
            if (opt_performance_index)
                ret += " performance Index :" + performance_index;
            return ret;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, network_number);
            ASN1.encode_tag(buffer, 1, true, (uint)mac_address.Length);
            foreach (byte b in mac_address)
                buffer.Add(b);
            ASN1.encode_context_unsigned(buffer, 2, Convert.ToUInt32(status));
            if (opt_performance_index)
                ASN1.encode_context_unsigned(buffer, 3, performance_index);

        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                network_number = (ushort)u_val;
            }
            else
                return len - 1;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                mac_address = new byte[len_value_type];
                len += ASN1.decode_octet_string(buffer, offset + len, (int)len_value, mac_address, 0, len_value_type);

            }
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                status = (BACnetNetworkReachability)u_val;

            }

            if (len < len_value)
            {

                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 3)
                {
                    opt_performance_index = true;
                    uint u_val;
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                    performance_index = (byte)u_val;

                }
                else
                    opt_performance_index = false;
            }

            return len;


        }


    }

    public struct BACnetWriteGroupRequest : ASN1.IASN1encode //C. Günther
    {
        public uint group_number;
        public uint write_priority;
        public List<BACnetGroupChannelValue> change_list;
        public bool inhibit_delay;

        public BACnetWriteGroupRequest(uint Group_Number, uint write_prio, List<BACnetGroupChannelValue> Channel_List, bool Inhibit_Delay = false)
        {
            group_number = Group_Number;
            write_priority = write_prio;
            change_list = Channel_List;
            inhibit_delay = Inhibit_Delay;
        }

        public override string ToString()
        {
            string ret = "group number :" + group_number + " write prio :" + write_priority + " inhibit delay :" + inhibit_delay;
            foreach (BACnetGroupChannelValue gcv in change_list)
                ret = ret + gcv.ToString();
            return ret;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, group_number);
            ASN1.encode_context_unsigned(buffer, 1, write_priority);
            ASN1.encode_opening_tag(buffer, 2);

            foreach (BACnetGroupChannelValue gcv in change_list)
            {
                gcv.ASN1encode(buffer);

            }
            ASN1.encode_closing_tag(buffer, 2);
            if (inhibit_delay == true)
                ASN1.encode_context_enumerated(buffer, 3, 1); //is optional

        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            //group number
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out group_number);
            else
                return len - 1;

            //write priority
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out write_priority);

            //Group Channel List
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 2))
            {

                len++;
                change_list = new List<BACnetGroupChannelValue>();
                while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 2))
                {

                    BACnetGroupChannelValue gcv = new BACnetGroupChannelValue();
                    len += gcv.ASN1decode(buffer, offset + len, len_value);
                    change_list.Add(gcv);
                }


                len++;//closing tag

            }


            if (len < len_value)
            {
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 3)//optional
                {
                    len++;
                    uint tmp;
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out tmp);
                    if (tmp != 0)
                        inhibit_delay = true;
                    else
                        inhibit_delay = false;
                }
            }

            return len;

        }

    }

    public struct BACnetSubscribeCOVPropertyMultiple : ASN1.IASN1encode //C. Günther
    {
        //could also be used for BACnetCOVMultipleSubscription but than has
        //BACnetRecipientProcess recipient instead of uint subscriber_process_identifier
        public uint subscriber_process_identifier;
        public bool issue_confirmed_notifications;
        public bool option_lifetime;
        public uint lifetime; //time-remaining
        public bool option_max_notification_delay;
        public uint max_notification_delay;

        public List<cov_subscription_specification> list_of_cov_subscription_specifications;

        public override string ToString()
        {
            string ret = "subscriber_process_identifier :" + subscriber_process_identifier + " issue_confirmed_notifications :" + issue_confirmed_notifications;
            if (option_lifetime)
                ret = ret + " lifetime :" + lifetime;
            if (option_max_notification_delay)
                ret = ret + " max_notification_delay :" + max_notification_delay;
            foreach (cov_subscription_specification css in list_of_cov_subscription_specifications)
                ret = ret + css;

            return ret;
        }

        public BACnetSubscribeCOVPropertyMultiple(uint subsc_id, bool issue_confirmed, List<cov_subscription_specification> covsl, bool opt_lifet = false, bool opt_max_notify_delay = false)
        {
            subscriber_process_identifier = subsc_id;
            issue_confirmed_notifications = issue_confirmed;
            option_lifetime = opt_lifet;
            lifetime = 0;
            option_max_notification_delay = opt_max_notify_delay;
            max_notification_delay = 0;
            list_of_cov_subscription_specifications = covsl;

        }

        public struct cov_reference
        {
            public BacnetPropertyReference monitored_property;
            public bool opt_cov_increment;
            public float cov_increment;
            public bool timestamped;

            public override string ToString()
            {
                string ret = " monitored_property :" + ((BacnetPropertyIds)monitored_property.propertyIdentifier).ToString();
                if (opt_cov_increment)
                    ret = ret + " " + cov_increment;
                ret = ret + " timestamped :" + timestamped;
                return ret;


            }

            public cov_reference(BacnetPropertyReference mp, bool timesta, bool opt_cov_i = false)
            {
                monitored_property = mp;
                timestamped = timesta;
                opt_cov_increment = opt_cov_i;
                cov_increment = 0;
            }
        }

        public struct cov_subscription_specification
        {
            public BacnetObjectId monitored_object_identifier;
            public List<cov_reference> list_of_cov_references;

            public override string ToString()
            {
                string ret = monitored_object_identifier.ToString();

                foreach (cov_reference cr in list_of_cov_references)
                    ret = ret + " " + cr;

                return ret;

            }

            public cov_subscription_specification(BacnetObjectId m_o_i, List<cov_reference> list_of_cov_ref)
            {
                monitored_object_identifier = m_o_i;
                list_of_cov_references = list_of_cov_ref;
            }

        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, subscriber_process_identifier);
            if (issue_confirmed_notifications)
                ASN1.encode_context_boolean(buffer, 1, issue_confirmed_notifications);
            if (option_lifetime)
                ASN1.encode_context_unsigned(buffer, 2, lifetime);
            if (option_max_notification_delay)
                ASN1.encode_context_unsigned(buffer, 3, max_notification_delay);
            ASN1.encode_opening_tag(buffer, 4);
            foreach (cov_subscription_specification css in list_of_cov_subscription_specifications)
            {

                ASN1.encode_context_object_id(buffer, 0, css.monitored_object_identifier.type, (uint)css.monitored_object_identifier.instance);
                ASN1.encode_opening_tag(buffer, 1);
                foreach (cov_reference cr in css.list_of_cov_references)
                {


                    ASN1.encode_opening_tag(buffer, 0);
                    ASN1.encode_context_enumerated(buffer, 0, cr.monitored_property.propertyIdentifier);
                    ASN1.encode_closing_tag(buffer, 0);

                    if (cr.opt_cov_increment)
                        ASN1.encode_context_real(buffer, 1, cr.cov_increment);
                    ASN1.encode_context_boolean(buffer, 2, cr.timestamped);

                }
                ASN1.encode_closing_tag(buffer, 1);
            }
            ASN1.encode_closing_tag(buffer, 4);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;

            byte tag;
            uint len_value_type;
            //subscriber-process-identifier
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out subscriber_process_identifier);
            else
                return len - 1;

            //issue-confirmed-notifications
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                uint val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                bool bval = val > 0 ? true : false;
                issue_confirmed_notifications = bval;
            }
            //intialize
            option_lifetime = false;
            lifetime = 0;
            option_max_notification_delay = false;
            max_notification_delay = 0;
            //lifetime
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
            {
                len++;
                option_lifetime = true;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out lifetime);

            }
            //max notification delay
            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 3)
            {
                len++;
                option_max_notification_delay = true;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out max_notification_delay);
            }
            //list-of-cov-subscription-specifications
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 4))
            {
                List<cov_subscription_specification> css = new List<cov_subscription_specification>();
                len++;
                while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 4))
                {

                    cov_subscription_specification cs = new cov_subscription_specification();

                    ushort object_type = 0;

                    len += ASN1.decode_context_object_id(buffer, offset + len, 0, out object_type, out cs.monitored_object_identifier.instance);
                    cs.monitored_object_identifier.type = (BacnetObjectTypes)object_type;

                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;
                        List<cov_reference> crl = new List<cov_reference>();
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                        {
                            cov_reference cr = new cov_reference();
                            //initiliase
                            cr.opt_cov_increment = false;
                            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                            {
                                len++;
                                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out cr.monitored_property.propertyIdentifier);
                                len++;
                            }
                            //cov-increment
                            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            if (tag == 1)
                            {
                                len++;
                                cr.opt_cov_increment = true;
                                len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out cr.cov_increment);
                            }
                            //timestamped
                            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            if (tag == 2)
                            {
                                len++;
                                uint val;
                                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                                bool bval = val > 0 ? true : false;
                                cr.timestamped = bval;

                            }

                            crl.Add(cr);

                        }
                        len++;
                        cs.list_of_cov_references = crl;
                    }



                    css.Add(cs);
                }
                len++;//closing tag
                list_of_cov_subscription_specifications = css;
            }




            return len;

        }
    }

    public struct BACnetCOVNotificationMultiple : ASN1.IASN1encode //C. Günther for UnconfirmedCOVNotificationMultiple and ConfirmedCOVNotificationMultiple
    {
        //cg
        public uint subscriber_process_identifier;
        public BacnetObjectId initiating_device_identifier;
        public uint time_remaining;
        public DateTime timestamp;
        public List<cov_notification> list_of_cov_notifications;

        public override string ToString()
        {
            string ret = "processid :" + subscriber_process_identifier + " initiating_device " + initiating_device_identifier.type.ToString() + " time remaining:" + time_remaining + " timestamp :" + timestamp;

            foreach (cov_notification cn in list_of_cov_notifications)
            {
                ret = ret + cn.ToString();
            }


            return ret;
        }

        public struct value
        {
            public BacnetPropertyIds property_identifier;
            public uint property_array_index;
            public BacnetValue property_value;
            public bool opt_time_of_change;
            public DateTime time_of_change;

            public override string ToString()
            {
                return "propid :" + property_identifier + " array :" + property_array_index + "value" + property_value.Value.ToString();
            }

        }

        public struct cov_notification
        {
            public BacnetObjectId monitored_object_identifier;
            public List<value> list_of_values;

            public override string ToString()
            {
                string ret = "monitored object :" + monitored_object_identifier.type;
                foreach (value v in list_of_values)
                {
                    ret = ret + v.ToString();
                }
                return ret;
            }

        }


        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, subscriber_process_identifier);
            ASN1.encode_context_object_id(buffer, 1, initiating_device_identifier.type, (uint)initiating_device_identifier.instance);
            ASN1.encode_context_unsigned(buffer, 2, time_remaining);
            ASN1.encode_opening_tag(buffer, 3);
            ASN1.bacapp_encode_datetime(buffer, timestamp);
            ASN1.encode_closing_tag(buffer, 3);
            ASN1.encode_opening_tag(buffer, 4);
            foreach (cov_notification cn in list_of_cov_notifications)
            {

                ASN1.encode_context_object_id(buffer, 0, cn.monitored_object_identifier.type, (uint)cn.monitored_object_identifier.instance);
                ASN1.encode_opening_tag(buffer, 1);
                foreach (value v in cn.list_of_values)
                {

                    ASN1.encode_context_enumerated(buffer, 0, (uint)v.property_identifier);
                    if (v.property_array_index != ASN1.BACNET_ARRAY_ALL)
                        ASN1.encode_context_unsigned(buffer, 1, v.property_array_index);
                    ASN1.encode_opening_tag(buffer, 2);
                    ASN1.bacapp_encode_application_data(buffer, v.property_value);
                    ASN1.encode_closing_tag(buffer, 2);
                    if (v.opt_time_of_change)
                        ASN1.encode_context_time(buffer, 3, v.time_of_change);

                }
                ASN1.encode_closing_tag(buffer, 1);
            }
            ASN1.encode_closing_tag(buffer, 4);

        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {

            int len = 0;

            byte tag;
            uint len_value_type;
            ushort obj_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out subscriber_process_identifier);
            }
            else
                return len - 1;

            len += ASN1.decode_context_object_id(buffer, offset + len, 1, out obj_type, out initiating_device_identifier.instance);
            initiating_device_identifier.type = (BacnetObjectTypes)obj_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out time_remaining);


            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 3))
            {

                len++;
                len += ASN1.decode_bacnet_datetime(buffer, offset + len, out timestamp);
                len++;
            }

            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 4))
            {

                len++;
                List<cov_notification> cnl = new List<cov_notification>();
                while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 4))
                {


                    cov_notification cn = new cov_notification();
                    len += ASN1.decode_context_object_id(buffer, offset + len, 0, out obj_type, out cn.monitored_object_identifier.instance);
                    cn.monitored_object_identifier.type = (BacnetObjectTypes)obj_type;
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;
                        List<value> vl = new List<value>();

                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                        {
                            value v = new value();

                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            if (tag == 0)
                            {
                                uint u_val;
                                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out u_val);
                                v.property_identifier = (BacnetPropertyIds)u_val;


                            }
                            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                            if (tag == 1)
                            {
                                len++;
                                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out v.property_array_index);

                            }
                            else
                                v.property_array_index = ASN1.BACNET_ARRAY_ALL;
                            //decode timestamp opt.

                            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 2))
                            {
                                len++;
                                BacnetValue bv = new BacnetValue();
                                len += ASN1.bacapp_decode_application_data(buffer, offset + len, (int)len_value, cn.monitored_object_identifier.type, v.property_identifier, out bv);
                                v.property_value = bv;

                                len++;

                            }
                            //decode timestamp opt.

                            ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            if (tag == 3)
                            {

                                v.opt_time_of_change = true;
                                len++;
                                len += ASN1.decode_bacnet_time_safe(buffer, offset + len, len_value_type, out v.time_of_change);


                            }
                            else
                                v.opt_time_of_change = false;

                            vl.Add(v);


                        }
                        len++;

                        cn.list_of_values = vl;


                    }

                    cnl.Add(cn);

                }
                len++;
                list_of_cov_notifications = cnl;



            }

            return len;
        }

    }

	public struct BACnetKeyIdentifier
    {
        byte algorithm;
        byte key_id;

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, algorithm);
            ASN1.encode_context_unsigned(buffer, 1, key_id);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0 && len_value_type == 1)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                algorithm = (byte)u_val;
            }
            else
                return len - 1;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1 && len_value_type == 1)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                key_id = (byte)u_val;
            }
            else
                return len - 1;

            return len;

        }
    }

    public struct BACnetSecurityKeySet
    {
        byte key_revision;
        DateTime activation_time;
        DateTime expiration_time;
        List<BACnetKeyIdentifier> key_ids;

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, key_revision);
            ASN1.bacapp_encode_context_datetime(buffer, 1, activation_time);
            ASN1.bacapp_encode_context_datetime(buffer, 2, expiration_time);
            ASN1.encode_opening_tag(buffer, 3);
            foreach (BACnetKeyIdentifier ki in key_ids)
                ki.ASN1encode(buffer);
            ASN1.encode_closing_tag(buffer, 3);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            //key revision
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0 && len_value_type == 1)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                key_revision = (byte)u_val;
            }
            else
                return len - 1;
            //activation time
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                len += ASN1.decode_bacnet_datetime(buffer, offset + len, out activation_time);
            }
            else
                return len - 1;
            //expiration_time
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
            {
                len += ASN1.decode_bacnet_datetime(buffer, offset + len, out expiration_time);
            }
            else
                return len - 1;

            //key_ids
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 3))
            {

                len++;
                key_ids = new List<BACnetKeyIdentifier>();
                while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 3))
                {

                    BACnetKeyIdentifier ki = new BACnetKeyIdentifier();
                    len += ki.ASN1decode(buffer, offset + len, len_value);
                    key_ids.Add(ki);
                }


                len++;//closing tag

            }

            return len;



        }
    }

    public struct BACnetNetworkSecurityPolicy
    {
        byte port_id;
        BACnetSecurityPolicy security_level;

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_unsigned(buffer, 0, port_id);
            ASN1.encode_context_enumerated(buffer, 1, Convert.ToUInt32(security_level));
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0 && len_value_type == 1)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                port_id = (byte)u_val;
            }
            else
                return len - 1;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                uint u_val;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                security_level = (BACnetSecurityPolicy)u_val;
            }
            else
                return len - 1;


            return len;


        }
    }

    public struct BACnetObjectPropertyReference : ASN1.IASN1encode //C. Günther
    {
        private BacnetObjectId object_identifier;
        private BacnetPropertyIds property_identifier;
        private bool option_property_array_index;
        private uint array_index;

        public BACnetObjectPropertyReference(BacnetObjectId Objectid, BacnetPropertyIds Property_id, uint Array_Index = ASN1.BACNET_ARRAY_ALL)
        {
            object_identifier = Objectid;
            property_identifier = Property_id;
            option_property_array_index = false;
            array_index = Array_Index;
            if (array_index != ASN1.BACNET_ARRAY_ALL)
            {
                option_property_array_index = true;
            }
        }

        public BacnetObjectId Object_Identifier
        {
            get { return object_identifier; }
            set { object_identifier = value; }
        }

        public BacnetPropertyIds Property_Identifier
        {
            get { return property_identifier; }
            set { property_identifier = value; }
        }

        public bool Option_Property_Array_Index
        {
            get { return option_property_array_index; }
        }

        public uint Array_Index
        {
            get { return array_index; }
            set {
                if ((uint)value == ASN1.BACNET_ARRAY_ALL)
                    option_property_array_index = false;
                else
                    option_property_array_index = true;
                array_index = value;
            }
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_context_object_id(buffer, 0, object_identifier.type, object_identifier.instance);
            ASN1.encode_context_enumerated(buffer, 1, Convert.ToUInt32(property_identifier));
            if (option_property_array_index)
                ASN1.encode_context_unsigned(buffer, 2, array_index);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;
            int tmp = 0;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 0)
            {
                ushort u_val;
                len += ASN1.decode_object_id(buffer, offset + len, out u_val, out object_identifier.instance);
                object_identifier.type = (BacnetObjectTypes)u_val;
            }
            else
                return len - 1;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 1)
            {
                uint u_val;
                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out u_val);
                property_identifier = (BacnetPropertyIds)u_val;
            }
            else
                return len - 1;
            option_property_array_index = false;

            tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            if (tag == 2)
            {
                option_property_array_index = true;
                len += tmp;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out array_index);
            }
            else
                array_index = ASN1.BACNET_ARRAY_ALL;

            return len;


        }
    }

    public struct BACnetSetpointReference : ASN1.IASN1encode //C. Günther
    {
        public BACnetObjectPropertyReference setpoint_reference;

        public BACnetObjectPropertyReference Setpoint_Reference
        {
            get { return setpoint_reference; }
            set { setpoint_reference = value; }

        }

        public BACnetSetpointReference(BACnetObjectPropertyReference Setpoint_Reference)
        {
            setpoint_reference = Setpoint_Reference;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_opening_tag(buffer, 0);
            setpoint_reference.ASN1encode(buffer);
            ASN1.encode_closing_tag(buffer, 0);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            if(ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {
                len++;
                BACnetObjectPropertyReference opr = new BACnetObjectPropertyReference();
                len += opr.ASN1decode(buffer, offset + len, len_value);
                setpoint_reference = opr;
                len++;

            }else
                return len;

            return len;

        }
    }

    public struct BACnetShedLevel : ASN1.IASN1encode //C. Günther
    {
        private object value;
        private ShedLevelType type;

        public ShedLevelType Type
        {
            get { return type; }
            set {
                type = value;
                switch (type)
                {
                    case ShedLevelType.LEVEL:
                    case ShedLevelType.PERCENT:
                        this.value = (uint)Convert.ChangeType(this.value, typeof(uint));
                        break;
                    case ShedLevelType.AMOUNT:
                        this.value = (float)Convert.ChangeType(this.value, typeof(float));
                        break;
                    default:
                        throw new NotSupportedException();
                }

                 }
        }

        public object Value
        {
            get {
                switch (type)
                {
                    case ShedLevelType.LEVEL:
                    case ShedLevelType.PERCENT:
                        return (uint)Convert.ChangeType(this.value, typeof(uint));
                    case ShedLevelType.AMOUNT:
                        return (float)Convert.ChangeType(this.value, typeof(float));
                    default:
                        throw new NotSupportedException();
                }
            }
            set {
                switch (type)
                {
                    case ShedLevelType.LEVEL:
                    case ShedLevelType.PERCENT:
                        if (this.value == null)
                            this.value = (uint)0;
                        else if(this.value.GetType() != typeof(uint))
                            this.value = (uint)Convert.ChangeType(this.value, typeof(uint));
                        else this.value = value;
                        break;
                    case ShedLevelType.AMOUNT:
                        if (this.value == null)
                            this.value = (float)0;
                        if (this.value.GetType() != typeof(float))
                            this.value = (float)Convert.ChangeType(this.value, typeof(float));
                        this.value = value;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public enum ShedLevelType
        {
            PERCENT = 0,
            LEVEL = 1,
            AMOUNT = 2
        }

        public BACnetShedLevel(ShedLevelType Type, object Value)
        {
            type = Type;
            value = Value;

        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            switch(type)
            {
                case ShedLevelType.PERCENT:
                    ASN1.encode_context_unsigned(buffer, 0, (uint)value);
                    break;
                case ShedLevelType.LEVEL:
                    ASN1.encode_context_unsigned(buffer, 1, (uint)value);
                    break;
                case ShedLevelType.AMOUNT:
                    ASN1.encode_context_real(buffer, 2, (float)value);
                    break;

            }
        }
        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;
            byte tag;
            uint len_value_type;

            //percent
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
            type = (ShedLevelType)tag;
            switch (type)
            {
                case ShedLevelType.PERCENT:
                case ShedLevelType.LEVEL:
                    uint u_val;
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out u_val);
                    value = u_val;
                    break;
                case ShedLevelType.AMOUNT:
                    float f_val;
                    len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out f_val);
                    value = f_val;
                    break;
                default:
                    return len;
            }

            return len;
        }


    }

    public struct BACnetEventParameter : ASN1.IASN1encode //C. Günther
    {
        /*BACnetNotificationParameters
          -- These choices have a one-to-one correspondence with the Event_Type enumeration with the exception of the
          -- complex-event-type, which is used for proprietary event types.*/
        public BacnetEventNotificationData.BacnetEventTypes type;

        public object Value;

        public struct ChangeOfState
        {
            public uint time_delay;
            public List<BACnetPropertyState> list_of_values;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                int i = 1;
                foreach (BACnetPropertyState ps in list_of_values)
                {
                    ret = ret + " alarmvalue " + i.ToString() + " :" + ps.ToString();
                    i++;
                }
                return ret;
            }

        }

        public struct ChangeOfBitstring
        {
            public uint time_delay;
            public BacnetBitString bitmask;
            public List<BacnetBitString> list_of_bitstring_values;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                int i = 1;

                ret = ret + " bitmask :" + bitmask.ToString();

                foreach (BacnetBitString bs in list_of_bitstring_values)
                {
                    ret = ret + " alarmvalue " + i.ToString() + " :" + bs.ToString();
                    i++;
                }

                return ret;

            }

        }

        public struct ChangeOfValue
        {
            public enum covtype
            {
                bitmask = 0,
                referenced_property_increment = 1

            };

            public uint time_delay;

            public covtype type;

            public object covcriteria;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                return ret + " " + type.ToString() + ": " + covcriteria.ToString();
            }
        }

        public struct CommandFailure
        {
            public uint time_delay;
            public BacnetDeviceObjectPropertyReference feedback_property_reference;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                return ret + " feedback reference :" + feedback_property_reference.deviceIndentifier.ToString() + ":" + feedback_property_reference.objectIdentifier.ToString() + ":" + feedback_property_reference.propertyIdentifier.ToString();

            }
        }

        public struct FloatingLimit
        {
            public uint time_delay;
            public BacnetDeviceObjectPropertyReference setpoint_reference;
            public float low_diff_limit;
            public float high_diff_limit;
            public float deadband;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                return ret + " setpointref :" + setpoint_reference.deviceIndentifier.ToString() + ":" + setpoint_reference.objectIdentifier.ToString() + ":" + setpoint_reference.propertyIdentifier.ToString() + " low diff :" + low_diff_limit.ToString() + " high diff :" + high_diff_limit.ToString() + " deadband :" + deadband.ToString();

            }
        }

        public struct OutOfRange
        {
            public uint time_delay;
            public float low_limit;
            public float high_limit;
            public float deadband;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                return ret + " low limit :" + low_limit.ToString() + " high limit:" + high_limit.ToString() + " deadband :" + deadband.ToString();

            }

        }

        public struct ChangeOfLifeSafety
        {
            public uint time_delay;
            public List<BacnetEventNotificationData.BacnetLifeSafetyStates> list_of_life_safety_alarm_values;
            public List<BacnetEventNotificationData.BacnetLifeSafetyStates> list_of_alarm_values;
            public BacnetDeviceObjectPropertyReference mode_property_reference;
        }

        public struct Extended
        {
            public ushort vendor_id;
            public uint extended_event_type;
            public List<object> parameters;

            public override string ToString()
            {
                string ret = "vendorid:" + vendor_id.ToString() + ":extendedventtype:" + extended_event_type.ToString();
                int i = 1;
                foreach (object obj in parameters)
                {
                    ret = ret + "parameter" + i.ToString() + ":" + obj.GetType() + ":" + obj.ToString() + " ";
                    i++;
                }
                return ret;

            }
        }

        public struct BufferReady
        {
            public uint notification_threshold;
            public UInt32 previous_notification_count;

            public override string ToString()
            {
                return "treshold :" + notification_threshold.ToString() + " count :" + previous_notification_count.ToString();
            }

        }

        public struct UnsignedRange
        {
            public uint time_delay;
            public uint low_limit;
            public uint high_limit;

            public override string ToString()
            {
                return "timedelay :" + time_delay.ToString() + " lowlimit :" + low_limit.ToString() + " highlimit :" + high_limit.ToString();
            }
        }

        public struct AccessEvent
        {
            public List<BACnetAccessEvents> list_of_access_events;
            public BacnetDeviceObjectPropertyReference access_event_time_reference;

            public override string ToString()
            {
                string ret = "";
                int i = 1;
                foreach (BACnetAccessEvents ls in list_of_access_events)
                {
                    ret = ret + "accessevent " + i.ToString() + ":" + ls.ToString() + " ";
                    i++;
                }
                return ret + " Reference :" + access_event_time_reference.objectIdentifier + ":" + access_event_time_reference.propertyIdentifier;

            }


        }

        public struct DoubleOutOfRange
        {
            public uint time_delay;
            public double low_limit;
            public double high_limit;
            public double deadband;

            public override string ToString()
            {
                return "timedelay :" + time_delay.ToString() + " lowlimit :" + low_limit.ToString() + " high_limit :" + high_limit.ToString() + " deadband :" + deadband.ToString();
            }

        }

        public struct SignedOutOfRange
        {
            public uint time_delay;
            public int low_limit;
            public int high_limit;
            public uint deadband;

            public override string ToString()
            {
                return "timedelay :" + time_delay.ToString() + " lowlimit :" + low_limit.ToString() + " highlimit :" + high_limit.ToString() + " deadband :" + deadband.ToString();
            }
        }

        public struct UnsignedOutOfRange
        {
            public uint time_delay;
            public uint low_limit;
            public uint high_limit;
            public uint deadband;



            public override string ToString()
            {
                return "timedelay :" + time_delay.ToString() + " lowlimit :" + low_limit.ToString() + " highlimit :" + high_limit.ToString() + " deadband :" + deadband.ToString();
            }
        }

        public struct ChangeOfCharacterString
        {
            public uint time_delay;
            public List<string> list_of_alarm_values;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                int i = 1;
                foreach (string string_value in list_of_alarm_values)
                {
                    ret = ret + " alarmvalue " + i.ToString() + " :" + string_value;
                    i++;
                }
                return ret;

            }
        }

        public struct ChangeOfStatusFlag
        {
            public uint time_delay;
            public List<BacnetStatusFlags> selected_flags; //??

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString() + "flags :" + selected_flags;
                int i = 1;
                foreach (BacnetStatusFlags flag in selected_flags)
                {
                    ret = ret + "flag " + i.ToString() + ":" + flag.ToString() + " ";
                    i++;
                }
                return ret;

            }

        }

        public struct ChangeOfDiscreteValue
        {
            public uint time_delay;

            public override string ToString()
            {
                return "timedelay :" + time_delay.ToString();
            }
        }

        public struct ChangeOfTimer
        {
            public uint time_delay;
            public List<BACnetTimerState> alarm_values;
            public BacnetDeviceObjectPropertyReference update_time_reference;

            public override string ToString()
            {
                string ret = "timedelay :" + time_delay.ToString();
                int i = 1;
                foreach (BACnetTimerState value in alarm_values)
                {
                    ret = ret + " alarmvalue " + i.ToString() + " :" + value;
                    i++;
                }
                return ret + update_time_reference.objectIdentifier.ToString() + "." + update_time_reference.propertyIdentifier.ToString();
            }

        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            //not all CG

            ASN1.encode_opening_tag(buffer, (byte)type);
            switch (type)
            {
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_STATE:
                    ChangeOfState cos = (ChangeOfState)Value;
                    ASN1.encode_context_unsigned(buffer, 0, cos.time_delay);
                    ASN1.encode_opening_tag(buffer, 1);
                    foreach (BACnetPropertyState ps in (List<BACnetPropertyState>)cos.list_of_values)
                    {
                        ps.ASN1encode(buffer);
                    }
                    ASN1.encode_closing_tag(buffer, 1);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_OUT_OF_RANGE:
                    OutOfRange oor = (OutOfRange)Value;
                    ASN1.encode_context_unsigned(buffer, 0, oor.time_delay);
                    ASN1.encode_context_real(buffer, 1, oor.low_limit);
                    ASN1.encode_context_real(buffer, 2, oor.high_limit);
                    ASN1.encode_context_real(buffer, 3, oor.deadband);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_UNSIGNED_RANGE:
                    UnsignedRange ur = (UnsignedRange)Value;
                    ASN1.encode_context_unsigned(buffer, 0, ur.time_delay);
                    ASN1.encode_context_unsigned(buffer, 1, ur.low_limit);
                    ASN1.encode_context_unsigned(buffer, 2, ur.high_limit);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_UNSIGNED_OUT_OF_RANGE:
                    UnsignedOutOfRange uoor = (UnsignedOutOfRange)Value;
                    ASN1.encode_context_unsigned(buffer, 0, uoor.time_delay);
                    ASN1.encode_context_unsigned(buffer, 1, uoor.low_limit);
                    ASN1.encode_context_unsigned(buffer, 2, uoor.high_limit);
                    ASN1.encode_context_unsigned(buffer, 3, uoor.deadband);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_SIGNED_OUT_OF_RANGE:
                    SignedOutOfRange soor = (SignedOutOfRange)Value;
                    ASN1.encode_context_unsigned(buffer, 0, soor.time_delay);
                    ASN1.encode_context_signed(buffer, 1, soor.low_limit);
                    ASN1.encode_context_signed(buffer, 2, soor.high_limit);
                    ASN1.encode_context_unsigned(buffer, 3, soor.deadband);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_DOUBLE_OUT_OF_RANGE:
                    DoubleOutOfRange door = (DoubleOutOfRange)Value;
                    ASN1.encode_context_unsigned(buffer, 0, door.time_delay);
                    ASN1.encode_context_double(buffer, 1, door.low_limit);
                    ASN1.encode_context_double(buffer, 2, door.high_limit);
                    ASN1.encode_context_double(buffer, 3, door.deadband);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_FLOATING_LIMIT:
                    FloatingLimit fl = (FloatingLimit)Value;
                    ASN1.encode_context_unsigned(buffer, 0, fl.time_delay);
                    ASN1.encode_opening_tag(buffer, 1);
                    ASN1.bacapp_encode_device_obj_property_ref(buffer, fl.setpoint_reference);
                    ASN1.encode_closing_tag(buffer, 1);
                    ASN1.encode_context_real(buffer, 2, fl.low_diff_limit);
                    ASN1.encode_context_real(buffer, 3, fl.high_diff_limit);
                    ASN1.encode_context_real(buffer, 4, fl.deadband);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_COMMAND_FAILURE:
                    CommandFailure cf = (CommandFailure)Value;
                    ASN1.encode_context_unsigned(buffer, 0, cf.time_delay);
                    ASN1.encode_opening_tag(buffer, 1);
                    ASN1.bacapp_encode_device_obj_property_ref(buffer, cf.feedback_property_reference);
                    ASN1.encode_closing_tag(buffer, 1);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_VALUE:
                    ChangeOfValue cov = (ChangeOfValue)Value;
                    ASN1.encode_context_unsigned(buffer, 0, cov.time_delay);
                    if (cov.covcriteria.GetType() == typeof(BacnetBitString))
                        ASN1.encode_context_bitstring(buffer, 0, (BacnetBitString)cov.covcriteria);
                    if (cov.covcriteria.GetType() == typeof(float))
                        ASN1.encode_context_real(buffer, 1, (float)cov.covcriteria);
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_BITSTRING:
                    ChangeOfBitstring cob = (ChangeOfBitstring)Value;
                    ASN1.encode_context_unsigned(buffer, 0, cob.time_delay);
                    ASN1.encode_context_bitstring(buffer, 1, (BacnetBitString)cob.bitmask);
                    ASN1.encode_opening_tag(buffer, 2);
                    foreach (BacnetBitString bs in cob.list_of_bitstring_values)
                        ASN1.encode_application_bitstring(buffer, (BacnetBitString)cob.bitmask);
                    ASN1.encode_closing_tag(buffer, 2);
                    break;
                /*
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_EXTENDED:
                    Extended e = (Extended)Value;
                    ASN1.encode_context_unsigned(buffer, 0, e.vendor_id);
                    ASN1.encode_context_unsigned(buffer, 1, e.extended_event_type);
                    ASN1.encode_opening_tag(buffer, 2);
                    //missing
                    ASN1.encode_closing_tag(buffer, 2);
                    break;
                    //some missing!*/
                default:
                    //someone add missing!!
                    throw new NotImplementedException();
            }
            ASN1.encode_closing_tag(buffer, (byte)type);
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            //CG
            int len = 0;

            byte tag;
            uint len_value_type;
            int tmp = 0;
            Value = new object();

            byte eventType;
            float v;
            len += ASN1.decode_tag_number(buffer, offset + len, out eventType);

            type = (BacnetEventNotificationData.BacnetEventTypes)eventType;

            switch (type)
            {
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_BITSTRING:
                    //EVENT_CHANGE_OF_BITSTRING
                    //time delay
                    ChangeOfBitstring cob = new ChangeOfBitstring();

                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cob.time_delay);

                    }



                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    BacnetBitString bs;
                    len += ASN1.decode_bitstring(buffer, offset + len, len_value_type, out cob.bitmask);

                    len++;

                    List<BacnetBitString> bsl = new List<BacnetBitString>();
                    while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 2))
                    {

                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                        len += ASN1.decode_bitstring(buffer, offset + len, len_value_type, out bs);

                        bsl.Add(bs);

                    }
                    cob.list_of_bitstring_values = bsl;

                    len++;


                    Value = cob;

                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_STATE:
                    //EVENT_CHANGE_OF_STATE
                    //time delay
                    ChangeOfState cos = new ChangeOfState();
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cos.time_delay);

                    }

                    len++;

                    //propertystate list

                    List<BACnetPropertyState> psl = new List<BACnetPropertyState>(); //wrong written propetystate???

                    //decode Property state
                    while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                    {

                        BACnetPropertyState ps = new BACnetPropertyState();
                        len += ps.ASN1decode(buffer, offset + len);
                        psl.Add(ps);

                    }
                    cos.list_of_values = psl;
                    Value = cos;
                    len++;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_VALUE:
                    //change of value
                    //time delay
                    ChangeOfValue cov = new ChangeOfValue();
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cov.time_delay);

                    }

                    len++;

                    //decode cov-criteria
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 1)
                    {
                        cov.type = ChangeOfValue.covtype.referenced_property_increment;
                        len += ASN1.decode_real(buffer, offset + len, out v);
                        cov.covcriteria = v;

                    }

                    if (tag == 0)
                    {
                        cov.type = ChangeOfValue.covtype.bitmask;
                        BacnetBitString bst;
                        len += ASN1.decode_bitstring(buffer, offset + len, len_value_type, out bst);
                        cov.covcriteria = bst;

                    }
                    len++;
                    Value = cov;

                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_COMMAND_FAILURE:
                    //command-failure
                    //time delay
                    CommandFailure cf = new CommandFailure();
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cf.time_delay);

                    }

                    len++;

                    len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -100, out cf.feedback_property_reference); //why i need ADPU_LEN????
                    len++;

                    Value = cf;

                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_FLOATING_LIMIT:
                    //floating-limit
                    //time delay
                    FloatingLimit fl = new FloatingLimit();
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out fl.time_delay);

                    }

                    len++;

                    len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -100, out fl.setpoint_reference); //why i need ADPU_LEN????

                    len++;

                    //low-diff-limit real
                    len += ASN1.decode_tag_number(buffer, offset + len, out tag);

                    if (tag == 2)
                    {
                        len += ASN1.decode_real(buffer, offset + len, out fl.low_diff_limit);

                    }

                    //high-diff-limit real
                    len += ASN1.decode_tag_number(buffer, offset + len, out tag);

                    if (tag == 3)
                    {
                        len += ASN1.decode_real(buffer, offset + len, out fl.high_diff_limit);

                    }

                    //deadband real
                    len += ASN1.decode_tag_number(buffer, offset + len, out tag);

                    if (tag == 4)
                    {
                        len += ASN1.decode_real(buffer, offset + len, out fl.deadband);

                    }

                    Value = fl;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_OUT_OF_RANGE:
                    //EVENT_OUT_OF_RANGE
                    OutOfRange oor = new OutOfRange();
                    //timedelay
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out oor.time_delay);

                    }

                    //len++;
                    //low-limit real
                    len += ASN1.decode_tag_number(buffer, offset + len, out tag);

                    if (tag == 1)
                    {
                        len += ASN1.decode_real(buffer, offset + len, out oor.low_limit);

                    }

                    //high-limit real
                    len += ASN1.decode_tag_number(buffer, offset + len, out tag);

                    if (tag == 2)
                    {
                        len += ASN1.decode_real(buffer, offset + len, out oor.high_limit);

                    }

                    //deadband real
                    len += ASN1.decode_tag_number(buffer, offset + len, out tag);

                    if (tag == 3)
                    {
                        len += ASN1.decode_real(buffer, offset + len, out oor.deadband);

                    }
                    Value = oor;


                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_LIFE_SAFETY:
                    //change-of-life-safety guessed!!!!
                    ChangeOfLifeSafety cols = new ChangeOfLifeSafety();
                    //timedelay
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cols.time_delay);

                    } else return len - 1;
                    len++;
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;
                        List<BacnetEventNotificationData.BacnetLifeSafetyStates> fvlsav = new List<BacnetEventNotificationData.BacnetLifeSafetyStates>();
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                        {
                            uint val;
                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);

                            fvlsav.Add((BacnetEventNotificationData.BacnetLifeSafetyStates)val);
                        }
                        len++;
                        cols.list_of_life_safety_alarm_values = fvlsav;
                    }
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 2))
                    {
                        len++;
                        List<BacnetEventNotificationData.BacnetLifeSafetyStates> fvav = new List<BacnetEventNotificationData.BacnetLifeSafetyStates>();
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                        {
                            uint val;
                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);

                            fvav.Add((BacnetEventNotificationData.BacnetLifeSafetyStates)val);
                        }
                        len++;
                        cols.list_of_alarm_values = fvav;

                    }

                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 3))
                    {
                        len++;
                        len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -100, out cols.mode_property_reference);
                        len++;

                    }

                    len++;
                    Value = cols;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_EXTENDED:
                    //extended guessed

                    Extended ext = new Extended();
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        uint t = 0;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out t);// ext.vendorid);
                        ext.vendor_id = (ushort)t;
                    }
                    else return len - 1;

                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 1)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out ext.extended_event_type);

                    }
                    // CG guessed
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 2))
                    {
                        len++;
                        List<object> objlist = new List<object>();
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 2))
                        {
                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            switch ((BacnetApplicationTags)tag)
                            {
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL: //null
                                    objlist.Add(null);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL: //real
                                    float float_value;
                                    len += ASN1.decode_real_safe(buffer, offset + len, len_value_type, out float_value);
                                    objlist.Add(float_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT: //unsigned
                                    uint uint_value;
                                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uint_value);
                                    objlist.Add(uint_value);

                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN: //boolean
                                    uint val;
                                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out val);
                                    bool bval = val > 0 ? true : false;
                                    objlist.Add(bval);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT: //integer
                                    int int_value;
                                    len += ASN1.decode_signed(buffer, offset + len, len_value_type, out int_value);
                                    objlist.Add(int_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DOUBLE: //double
                                    double double_value;
                                    len = ASN1.decode_double_safe(buffer, offset + len, len_value_type, out double_value);
                                    objlist.Add(double_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING: //octetstring
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING: //characterstring
                                    string string_value;
                                    len += ASN1.decode_character_string(buffer, offset + len, (int)len_value_type, len_value_type, out string_value);
                                    objlist.Add(string_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING: //bitstring
                                    BacnetBitString bit_value;
                                    len += ASN1.decode_bitstring(buffer, offset + len, len_value_type, out bit_value);
                                    objlist.Add(bit_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED: //enumerated
                                    uint e_value;
                                    len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out e_value);
                                    objlist.Add(e_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE: //date
                                    DateTime date_v;
                                    len += ASN1.decode_date_safe(buffer, offset + len, len_value_type, out date_v);
                                    objlist.Add(date_v);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME: //time
                                    DateTime time_value;
                                    len += ASN1.decode_bacnet_time_safe(buffer, offset + len, len_value_type, out time_value);
                                    objlist.Add(time_value);
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID: //BACnetObjectIdentifier,
                                    ushort object_type = 0;
                                    uint instance = 0;
                                    len += ASN1.decode_object_id_safe(buffer, offset + len, len_value_type, out object_type, out instance);
                                    objlist.Add(new BacnetObjectId((BacnetObjectTypes)object_type, instance));
                                    break;
                                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DEVICE_OBJECT_PROPERTY_REFERENCE: //BACnetDeviceObjectPropertyReference
                                    BacnetDeviceObjectPropertyReference dopr;
                                    len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -2000, out dopr);
                                    objlist.Add(dopr);
                                    break;
                            }

                        }
                        ext.parameters = objlist;
                        len++;

                    }

                    Value = ext;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_BUFFER_READY:
                    //buffer-ready guessed
                    BufferReady br = new BufferReady();
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out br.notification_threshold);
                    }
                    else return len - 1;
                    len++;
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 1)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out br.previous_notification_count);
                    }
                    len++;
                    Value = br;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_UNSIGNED_RANGE:
                    //unsigned-range
                    //time delay
                    UnsignedRange ur = new UnsignedRange();

                    //timedelay
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out ur.time_delay);

                    }

                    //len++;
                    //low-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 1)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out ur.low_limit);

                    }


                    //high-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 2)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out ur.high_limit);

                    }

                    Value = ur;

                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_ACCESS_EVENT:
                    //access-event guessed
                    AccessEvent ea = new AccessEvent();
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
                    {
                        List<BACnetAccessEvents> ael = new List<BACnetAccessEvents>();
                        len++;
                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                        {
                            uint uv;
                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uv);
                            ael.Add((BACnetAccessEvents)uv);
                        }
                        ea.list_of_access_events = ael;
                    }
                    len++;
                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;
                        len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -100, out ea.access_event_time_reference);

                        len++;

                    }



                    Value = ea;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_DOUBLE_OUT_OF_RANGE:
                    //double-out-of-range
                    DoubleOutOfRange door = new DoubleOutOfRange();
                    //time delay
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out door.time_delay);

                    }

                    //low-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 1)
                    {
                        len += ASN1.decode_double_safe(buffer, offset + len, len_value_type, out door.low_limit);

                    }

                    //high-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 2)
                    {
                        len += ASN1.decode_double_safe(buffer, offset + len, len_value_type, out door.high_limit);

                    }

                    //deadband
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 3)
                    {
                        len += ASN1.decode_double_safe(buffer, offset + len, len_value_type, out door.deadband);

                    }

                    Value = door;
                    len++;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_SIGNED_OUT_OF_RANGE:
                    //signed-out-of-range
                    SignedOutOfRange soor = new SignedOutOfRange();

                    //timedelay
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out soor.time_delay);

                    }

                    //low-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 1)
                    {
                        len += ASN1.decode_signed(buffer, offset + len, len_value_type, out soor.low_limit);

                    }

                    //high-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 2)
                    {
                        len += ASN1.decode_signed(buffer, offset + len, len_value_type, out soor.high_limit);

                    }

                    //deadband
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 3)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out soor.deadband);

                    }
                    len++;
                    Value = soor;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_UNSIGNED_OUT_OF_RANGE:
                    //unsigned-out-of-range
                    UnsignedOutOfRange uoor = new UnsignedOutOfRange();
                    //timedelay
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uoor.time_delay);

                    }

                    //len++;
                    //low-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 1)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uoor.low_limit);

                    }

                    //high-limit
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 2)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uoor.high_limit);

                    }

                    //deadband
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                    if (tag == 3)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out uoor.deadband);

                    }
                    len++;
                    Value = uoor;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_CHARACTERSTRING:
                    //change-of-character-string
                    ChangeOfCharacterString cocs = new ChangeOfCharacterString();
                    //timedelay
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cocs.time_delay);

                    }
                    len++;


                    List<string> string_values = new List<string>();
                    while (!ASN1.decode_is_closing_tag_number(buffer, offset + len + 1, 17))
                    {

                        tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                        if (tag == 7)
                        {

                            len += tmp;
                            string string_value;
                            len += ASN1.decode_character_string(buffer, offset + len, (int)len_value_type, len_value_type, out string_value);

                            string_values.Add(string_value);

                        }


                    }

                    cocs.list_of_alarm_values = string_values;
                    len++;
                    len++;

                    Value = cocs;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_STATUS_FLAG:
                    //change-of-status-flags not finished
                    return len - 1;
                /*
                ChangeOfStatusFlag cosf = new ChangeOfStatusFlag();
                //timedelay
                tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                if (tag == 0)
                {
                    len += tmp;
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cosf.time_delay);

                }
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);

                BacnetBitString flag_bitstring;
                len += ASN1.decode_bitstring(buffer, offset + len, len_value_type, out flag_bitstring);
                //somehow convert flag_bitstring to flags?????

                Value = cosf;
                break;*/
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_NONE:
                    //none 20
                    len++;
                    Value = null;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_DISCRETE_VALUE:
                    //EVENT_CHANGE_OF_DISCRETE_VALUE
                    ChangeOfDiscreteValue codv = new ChangeOfDiscreteValue();

                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out codv.time_delay);

                    }
                    len++;
                    Value = codv;
                    break;
                case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_TIMER:
                    ChangeOfTimer cot = new ChangeOfTimer();
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                    if (tag == 0)
                    {
                        len += tmp;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out cot.time_delay);

                    }

                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    {
                        len++;

                        List<BACnetTimerState> tsl = new List<BACnetTimerState>();

                        while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                        {
                            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag, out len_value_type);
                            uint ts;
                            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out ts);

                            tsl.Add((BACnetTimerState)ts);
                        }
                        len++;
                        cot.alarm_values = tsl;
                    }

                    if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 2))
                    {
                        len++;
                        len += ASN1.decode_device_obj_property_ref(buffer, offset + len, -10000, out cot.update_time_reference); //adpu len is a joke???

                    }
                    len++;
                    Value = cot;
                    break;
                default:
                    return len - 1;
            }
            len++;
            return len;

        }

        public override string ToString()
        {
            return type.ToString() + ":" + Value.ToString();
        }


    }

    public struct BACnetTimeValue : ASN1.IASN1encode
    {
        public DateTime dt;
        public BacnetValue value;


        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_application_time(buffer, dt);
            ASN1.bacapp_encode_application_data(buffer, value);
        }

        public int ASN1decode(byte[] buffer, int offset)
        {
            var len = ASN1.decode_application_time(buffer, offset, out dt);
            if (len < 0)
                return len;
            byte app_tag;
            uint len_value;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out app_tag, out len_value);
            len += ASN1.bacapp_decode_data(buffer, offset + len, 100, (BacnetApplicationTags)(app_tag), len_value, out value);
            return len;
        }
    }

    public struct BACnetSpecialEvent: ASN1.IASN1encode
    {
        public object period; // BacnetDate or BacnetDateRange or BacnetweekNDay or ObjectIdentifier
        public List<BACnetTimeValue> Values;
        public byte eventPriority;
        public void ASN1encode(EncodeBuffer buffer)
        {
            if (Values != null)
            {
                if (period is BacnetObjectId ob)
                {
                    ASN1.encode_context_object_id(buffer, 1, ob.type, ob.instance);
                }
                else
                {
                    ASN1.encode_opening_tag(buffer, 0);
                    ((BACnetCalendarEntry)period).ASN1encode(buffer);
                    ASN1.encode_closing_tag(buffer, 0);
                }
                ASN1.encode_opening_tag(buffer, 2);
                foreach (ASN1.IASN1encode entry in Values)
                {
                    entry.ASN1encode(buffer);
                }
                ASN1.encode_closing_tag(buffer, 2);
                
                ASN1.encode_context_unsigned(buffer, 3, eventPriority);
            }
        }

        public override string ToString()
        {
            return period.ToString();
            /*
            if (period is BacnetObjectId ob)
            {
                return ob.ToString();
            }

            if (period is BacnetDate bd)
            {
                return String.Format("date: {0}", bd.ToString());
            }
            if (period is BacnetDateRange bdr)
            {
                return String.Format("date: {0}", bdr.ToString());
            }
            */
        }

        public int ASN1decode(byte[] buffer, int offset)
        {
            int len = 0;
            byte tag_number;

            Values = new List<BACnetTimeValue>();

            len += ASN1.decode_tag_number(buffer, offset + len, out tag_number);
            if (tag_number == 0)
            {
                var p = new BACnetCalendarEntry();
                len += p.decode_one(buffer, offset + len) + 1;
                period = p;
            }
            else
            {
                ushort type;
                uint instance;
                len += ASN1.decode_context_object_id(buffer, offset + len, 1, out type, out instance);
                period = new BacnetObjectId((BacnetObjectTypes)type, instance);
            }

            len += 1; // time-values opening tag;
            while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 2))
            {
                var v = new BACnetTimeValue();
                len += v.ASN1decode(buffer, offset + len);
                Values.Add(v);
            }
            len += 1; // time-values closing tag;
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 3))
                return -1;
            len += 1;
            ASN1.decode_unsigned8(buffer, offset + len, out eventPriority);
            return len + 1;
        }

    }

    public struct BACnetCalendarEntry : ASN1.IASN1encode
    {
        public List<object> Entries; // BacnetDate or BacnetDateRange or BacnetweekNDay

        public void ASN1encode(EncodeBuffer buffer)
        {
            if (Entries != null)
                foreach (ASN1.IASN1encode entry in Entries)
                {
                    if (entry is BacnetDate)
                    {
                        ASN1.encode_tag(buffer, 0, true, 4);
                        entry.ASN1encode(buffer);
                    }

                    if (entry is BacnetDateRange)
                    {
                        ASN1.encode_opening_tag(buffer, 1);
                        entry.ASN1encode(buffer);
                        ASN1.encode_closing_tag(buffer, 1);
                    }

                    if (entry is BacnetweekNDay)
                    {
                        ASN1.encode_tag(buffer, 2, true, 3);
                        entry.ASN1encode(buffer);
                    }
                }
        }

        public int ASN1decode(byte[] buffer, int offset, uint len_value)
        {
            int len = 0;

            Entries = new List<object>();

            for (;;)
            {
                var len1 = decode_one(buffer, offset + len);
                if (len1 <= 0)
                    return len;
                len += len1;
            }

        }

        public int decode_one(byte[] buffer, int offset)
        {
            if( Entries == null)
                Entries = new List<object>();

            if (!ASN1.IS_CONTEXT_SPECIFIC(buffer[offset]) || ASN1.IS_CLOSING_TAG(buffer[offset]))
                return -1;
            byte tag_number;
            var len = ASN1.decode_tag_number(buffer, offset, out tag_number);

            switch (tag_number)
            {
                case 0:
                    BacnetDate bdt = new BacnetDate();
                    len += bdt.ASN1decode(buffer, offset + len, 0);
                    Entries.Add(bdt);
                    return len;
                case 1:
                    BacnetDateRange bdr = new BacnetDateRange();
                    len += bdr.ASN1decode(buffer, offset + len, 0);
                    Entries.Add(bdr);
                    len++; // closing tag
                    return len;
                case 2:
                    BacnetweekNDay bwd = new BacnetweekNDay();
                    len += bwd.ASN1decode(buffer, offset + len, 0);
                    Entries.Add(bwd);
                    return len;
                default:
                    return len - 1; // closing Tag
            }
        }
        public override string ToString()
        {
            return Entries[0].ToString();
        }
    }

    public struct BacnetGetEventInformationData
    {
        public BacnetObjectId objectIdentifier;
        public BacnetEventNotificationData.BacnetEventStates eventState;
        public BacnetBitString acknowledgedTransitions;
        public BacnetGenericTime[] eventTimeStamps;    //3
        public BacnetEventNotificationData.BacnetNotifyTypes notifyType;
        public BacnetBitString eventEnable;
        public uint[] eventPriorities;     //3
    };

    public struct BacnetWriteAccessSpecification
    {
        public BacnetObjectId object_id;
        public ICollection<BacnetPropertyValue> values_refs;
        public BacnetWriteAccessSpecification(BacnetObjectId object_id, ICollection<BacnetPropertyValue> values_refs)
        {
            this.object_id = object_id;
            this.values_refs = values_refs;
        }
    }

    public struct BacnetReadAccessSpecification
    {
        public BacnetObjectId objectIdentifier;
        public IList<BacnetPropertyReference> propertyReferences;
        public BacnetReadAccessSpecification(BacnetObjectId objectIdentifier, IList<BacnetPropertyReference> propertyReferences)
        {
            this.objectIdentifier = objectIdentifier;
            this.propertyReferences = propertyReferences;
        }
        public static object Parse(string value)
        {
            BacnetReadAccessSpecification ret = new BacnetReadAccessSpecification();
            if (string.IsNullOrEmpty(value)) return ret;
            string[] tmp = value.Split(':');
            if (tmp == null || tmp.Length < 2) return ret;
            ret.objectIdentifier.type = (BacnetObjectTypes)Enum.Parse(typeof(BacnetObjectTypes), tmp[0]);
            ret.objectIdentifier.instance = uint.Parse(tmp[1]);
            List<BacnetPropertyReference> refs = new List<BacnetPropertyReference>();
            for (int i = 2; i < tmp.Length; i++)
            {
                BacnetPropertyReference n = new BacnetPropertyReference();
                n.propertyArrayIndex = System.IO.BACnet.Serialize.ASN1.BACNET_ARRAY_ALL;
                n.propertyIdentifier = (uint)(BacnetPropertyIds)Enum.Parse(typeof(BacnetPropertyIds), tmp[i]);
                refs.Add(n);
            }
            ret.propertyReferences = refs;
            return ret;
        }
        public override string ToString()
        {
            string ret = objectIdentifier.ToString();
            foreach (BacnetPropertyReference r in propertyReferences)
            {
                ret += ":" + ((BacnetPropertyIds)r.propertyIdentifier).ToString();
            }
            return ret;
        }
    }

    public struct BacnetReadAccessResult
    {
        public BacnetObjectId objectIdentifier;
        public IList<BacnetPropertyValue> values;
        public BacnetReadAccessResult(BacnetObjectId objectIdentifier, IList<BacnetPropertyValue> values)
        {
            this.objectIdentifier = objectIdentifier;
            this.values = values;
        }
    }

    public struct BacnetCOVSubscription
    {
        /* BACnetRecipientProcess */
        public BacnetAddress Recipient;
        public uint subscriptionProcessIdentifier;
        /* BACnetObjectPropertyReference */
        public BacnetObjectId monitoredObjectIdentifier;
        public BacnetPropertyReference monitoredProperty;
        /* BACnetCOVSubscription */
        public bool IssueConfirmedNotifications;
        public uint TimeRemaining;
        public float COVIncrement;

        // For Yabe PropertyGrid
        public override string ToString()
        {
            string name = monitoredObjectIdentifier.ToString();
            if (name.StartsWith("OBJECT_")) name = name.Substring(7);

            return name + " by " + Recipient.ToString() + ", remain " + TimeRemaining + "s";
        }
    }

    public struct BacnetError
    {
        public BacnetErrorClasses error_class;
        public BacnetErrorCodes error_code;
        public BacnetError(BacnetErrorClasses error_class, BacnetErrorCodes error_code)
        {
            this.error_class = error_class;
            this.error_code = error_code;
        }
        public BacnetError(uint error_class, uint error_code)
        {
            this.error_class = (BacnetErrorClasses)error_class;
            this.error_code = (BacnetErrorCodes)error_code;
        }
        public override string ToString()
        {
            return error_class.ToString() + ": " + error_code.ToString();
        }
    }

    public struct BacnetBitString
    {
        public byte bits_used;
        public byte[] value;

        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < bits_used; i++)
            {
                ret = ret + ((value[i / 8] & (1 << (i % 8))) > 0 ? "1" : "0");
            }
            return ret;
        }

        public void SetBit(byte bit_number, bool v)
        {
            byte byte_number = (byte)(bit_number / 8);
            byte bit_mask = 1;

            if (value == null) value = new byte[System.IO.BACnet.Serialize.ASN1.MAX_BITSTRING_BYTES];

            if (byte_number < System.IO.BACnet.Serialize.ASN1.MAX_BITSTRING_BYTES)
            {
                /* set max bits used */
                if (bits_used < (bit_number + 1))
                    bits_used = (byte)(bit_number + 1);
                bit_mask = (byte)(bit_mask << (bit_number - (byte_number * 8)));
                if (v)
                    value[byte_number] |= bit_mask;
                else
                    value[byte_number] &= (byte)(~(bit_mask));
            }
        }

        public static BacnetBitString Parse(string str)
        {
            BacnetBitString ret = new BacnetBitString();
            ret.value = new byte[System.IO.BACnet.Serialize.ASN1.MAX_BITSTRING_BYTES];

            if (!string.IsNullOrEmpty(str))
            {
                ret.bits_used = (byte)str.Length;
                for (int i = 0; i < ret.bits_used; i++)
                {
                    bool is_set = str[i] == '1';
                    if (is_set) ret.value[i / 8] |= (byte)(1 << (i % 8));
                }
            }

            return ret;
        }

        public uint ConvertToInt()
        {
            return value == null ? 0 : BitConverter.ToUInt32(value, 0);
        }

        public static BacnetBitString ConvertFromInt(uint value)
        {
            BacnetBitString ret = new BacnetBitString();
            ret.value = BitConverter.GetBytes(value);
            ret.bits_used = (byte)Math.Ceiling(Math.Log(value, 2));
            return ret;
        }
    };

    public enum BacnetTrendLogValueType : byte
    {
        // Copyright (C) 2009 Peter Mc Shane in Steve Karg Stack, trendlog.h
        // Thank's to it's encoding sample, very usefull for this decoding work
        TL_TYPE_STATUS = 0,
        TL_TYPE_BOOL = 1,
        TL_TYPE_REAL = 2,
        TL_TYPE_ENUM = 3,
        TL_TYPE_UNSIGN = 4,
        TL_TYPE_SIGN = 5,
        TL_TYPE_BITS = 6,
        TL_TYPE_NULL = 7,
        TL_TYPE_ERROR = 8,
        TL_TYPE_DELTA = 9,
        TL_TYPE_ANY = 10
    }

    public enum BACnetAccessAuthenticationFactorDisable
    {
        NONE = 0,
        DISABLED = 1,
        DISABLED_LOST = 2,
        DISABLED_STOLEN =3,
        DISABLED_DAMAGED =4,
        DISABLED_DESTROYED= 5
    }

    public struct BacnetLogRecord
    {
        public DateTime timestamp;

        /* logDatum: CHOICE { */
        public BacnetTrendLogValueType type;
        //private BacnetBitString log_status;
        //private bool boolean_value;
        //private float real_value;
        //private uint enum_value;
        //private uint unsigned_value;
        //private int signed_value;
        //private BacnetBitString bitstring_value;
        //private bool null_value;
        //private BacnetError failure;
        //private float time_change;
        private object any_value;
        /* } */

        public BacnetBitString statusFlags;

        public BacnetLogRecord(BacnetTrendLogValueType type, object value, DateTime stamp, uint status)
        {
            this.type = type;
            timestamp = stamp;
            statusFlags = BacnetBitString.ConvertFromInt(status);
            any_value = null;
            this.Value = value;
        }

        public object Value
        {
            get
            {
                switch (type)
                {
                    case BacnetTrendLogValueType.TL_TYPE_ANY:
                        return any_value;
                    case BacnetTrendLogValueType.TL_TYPE_BITS:
                        return (BacnetBitString)Convert.ChangeType(any_value, typeof(BacnetBitString));
                    case BacnetTrendLogValueType.TL_TYPE_BOOL:
                        return (bool)Convert.ChangeType(any_value, typeof(bool));
                    case BacnetTrendLogValueType.TL_TYPE_DELTA:
                        return (float)Convert.ChangeType(any_value, typeof(float));
                    case BacnetTrendLogValueType.TL_TYPE_ENUM:
                        return (uint)Convert.ChangeType(any_value, typeof(uint));
                    case BacnetTrendLogValueType.TL_TYPE_ERROR:
                        if (any_value != null)
                            return (BacnetError)Convert.ChangeType(any_value, typeof(BacnetError));
                        else
                            return new BacnetError(BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_ABORT_OTHER);
                    case BacnetTrendLogValueType.TL_TYPE_NULL:
                        return null;
                    case BacnetTrendLogValueType.TL_TYPE_REAL:
                        return (float)Convert.ChangeType(any_value, typeof(float));
                    case BacnetTrendLogValueType.TL_TYPE_SIGN:
                        return (int)Convert.ChangeType(any_value, typeof(int));
                    case BacnetTrendLogValueType.TL_TYPE_STATUS:
                        return (BacnetBitString)Convert.ChangeType(any_value, typeof(BacnetBitString));
                    case BacnetTrendLogValueType.TL_TYPE_UNSIGN:
                        return (uint)Convert.ChangeType(any_value, typeof(uint));
                    default:
                        throw new NotSupportedException();
                }
            }
            set
            {
                switch (type)
                {
                    case BacnetTrendLogValueType.TL_TYPE_ANY:
                        any_value = value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_BITS:
                        if(value == null) value = new BacnetBitString();
                        if (value.GetType() != typeof(BacnetBitString))
                            value = BacnetBitString.ConvertFromInt((uint)Convert.ChangeType(value, typeof(uint)));
                        any_value = (BacnetBitString)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_BOOL:
                        if(value == null) value = false;
                        if (value.GetType() != typeof(bool))
                            value = (bool)Convert.ChangeType(value, typeof(bool));
                        any_value = (bool)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_DELTA:
                        if(value == null) value = (float)0;
                        if (value.GetType() != typeof(float))
                            value = (float)Convert.ChangeType(value, typeof(float));
                        any_value = (float)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_ENUM:
                        if(value == null) value = (uint)0;
                        if (value.GetType() != typeof(uint))
                            value = (uint)Convert.ChangeType(value, typeof(uint));
                        any_value = (uint)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_ERROR:
                        if (value == null) value = new BacnetError();
                        if (value.GetType() != typeof(BacnetError))
                            throw new ArgumentException();
                        any_value = (BacnetError)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_NULL:
                        if (value != null) throw new ArgumentException();
                        any_value = value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_REAL:
                        if(value == null) value = (float)0;
                        if (value.GetType() != typeof(float))
                            value = (float)Convert.ChangeType(value, typeof(float));
                        any_value = (float)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_SIGN:
                        if(value == null) value = (Int32)0;
                        if (value.GetType() != typeof(Int32))
                            value = (Int32)Convert.ChangeType(value, typeof(Int32));
                        any_value = (Int32)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_STATUS:
                        if(value == null) value = new BacnetBitString();
                        if (value.GetType() != typeof(BacnetBitString))
                            value = BacnetBitString.ConvertFromInt((uint)Convert.ChangeType(value, typeof(uint)));
                        any_value = (BacnetBitString)value;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_UNSIGN:
                        if(value == null) value = (UInt32)0;
                        if (value.GetType() != typeof(UInt32))
                            value = (UInt32)Convert.ChangeType(value, typeof(UInt32));
                        any_value = (UInt32)value;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public T GetValue<T>()
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
    }

    // FC
    public struct DeviceReportingRecipient : ASN1.IASN1encode
    {
        public BacnetBitString WeekofDay;
        public DateTime toTime, fromTime;

        public BacnetObjectId Id;
        public BacnetAddress adr;

        public uint processIdentifier;
        public bool Ack_Required;
        public BacnetBitString evenType;

        public DeviceReportingRecipient(BacnetValue v0, BacnetValue v1, BacnetValue v2, BacnetValue v3, BacnetValue v4, BacnetValue v5, BacnetValue v6)
        {
            Id = new BacnetObjectId();
            adr = null;

            WeekofDay = (BacnetBitString)v0.Value;
            fromTime = (DateTime)v1.Value;
            toTime = (DateTime)v2.Value;
            if (v3.Value is BacnetObjectId)
                Id = (BacnetObjectId)v3.Value;
            else
            {
                BacnetValue[] netdescr = (BacnetValue[])v3.Value;
                ushort s = Convert.ToUInt16(netdescr[0].Value);
                byte[] b = (byte[])netdescr[1].Value;
                adr = new BacnetAddress(BacnetAddressTypes.IP, s, b);
            }
            processIdentifier = Convert.ToUInt32(v4.Value);
            Ack_Required = (bool)v5.Value;
            evenType = (BacnetBitString)v6.Value;
        }

        public DeviceReportingRecipient(BacnetBitString WeekofDay, DateTime fromTime, DateTime toTime, BacnetObjectId Id, uint processIdentifier, bool Ack_Required, BacnetBitString evenType)
        {
            adr = null;

            this.WeekofDay = WeekofDay;
            this.toTime = toTime;
            this.fromTime = fromTime;
            this.Id = Id;
            this.processIdentifier = processIdentifier;
            this.Ack_Required = Ack_Required;
            this.evenType = evenType;
        }
        public DeviceReportingRecipient(BacnetBitString WeekofDay, DateTime fromTime, DateTime toTime, BacnetAddress adr, uint processIdentifier, bool Ack_Required, BacnetBitString evenType)
        {
            this.Id = new BacnetObjectId();

            this.WeekofDay = WeekofDay;
            this.toTime = toTime;
            this.fromTime = fromTime;
            this.adr = adr;
            this.processIdentifier = processIdentifier;
            this.Ack_Required = Ack_Required;
            this.evenType = evenType;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.bacapp_encode_application_data(buffer, new BacnetValue(WeekofDay));
            ASN1.bacapp_encode_application_data(buffer, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME, fromTime));
            ASN1.bacapp_encode_application_data(buffer, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME, toTime));
            if (adr != null)
                adr.ASN1encode(buffer);
            else
                ASN1.encode_context_object_id(buffer, 0, Id.type, Id.instance);         // BacnetObjectId is context specific encoded
            ASN1.bacapp_encode_application_data(buffer, new BacnetValue(processIdentifier));
            ASN1.bacapp_encode_application_data(buffer, new BacnetValue(Ack_Required));
            ASN1.bacapp_encode_application_data(buffer, new BacnetValue(evenType));
        }
    }

    public enum BacnetAddressTypes
    {
        None,
        IP,
        MSTP,
        Ethernet,
        ArcNet,
        LonTalk,
        PTP,
        IPV6,
        SC
    }

    public class BacnetAddress : ASN1.IASN1encode
    {
        public UInt16 net;
        public byte[] adr;              // Variable size. Used MSTP, Ethernet, UDP and also for BACnet/SC VMAC (duplicated)
        public byte[] VMac=new byte[6]; // 3 bytes for IP V6, 6 for BACnetSC
        public BacnetAddressTypes type;

        // Modif FC
        public BacnetAddress RoutedSource=null;

        public BacnetAddress(BacnetAddressTypes type, UInt16 net, byte[] adr)
        {
            this.type = type;
            this.net = net;
            this.adr = adr;
            if (this.adr == null) this.adr = new byte[0];
        }

        public BacnetAddress(BacnetAddressTypes type, String s, UInt16 net=0)
        {
            this.type = type;
            this.net = net;
            switch (type)
            {
                case BacnetAddressTypes.IP:
                    try
                    {
                        String[] IpStrCut = s.Split(':');
                        IPAddress ip;
                        bool IsIP = IPAddress.TryParse(IpStrCut[0], out ip);
                        uint Port = Convert.ToUInt16(IpStrCut[1]);
                        if (IsIP==true)
                        {
                            String[] Cut = IpStrCut[0].Split('.');
                            adr=new byte[6];
                                for (int i=0;i<4;i++)
                                    adr[i]=Convert.ToByte(Cut[i]);
                            adr[4] = (byte)((Port & 0xff00) >> 8);
                            adr[5] = (byte)(Port & 0xff);
                        }
                        else
                            throw new Exception();
                    }
                    catch { throw new Exception(); }
                   break;
                case BacnetAddressTypes.Ethernet:
                   try
                   {
                       String[] EthStrCut = s.Split('-');
                       adr=new byte[6];
                       for (int i = 0; i < 6; i++)
                           adr[i] = Convert.ToByte(EthStrCut[i], 16);
                   }
                   catch { throw new Exception(); }
                   break;
            }
        }
        public BacnetAddress()
        {
            type = BacnetAddressTypes.None;
        }

        public override int GetHashCode()
        {
            return adr.GetHashCode();
        }
        public override string ToString()
        {
            return ToString(this.type);
        }
        public string ToString(BacnetAddressTypes type)
        {
            switch (type)
            {
                case BacnetAddressTypes.IP:
                    if(adr == null || adr.Length < 6) return "0.0.0.0";
                    return adr[0] + "." + adr[1] + "." + adr[2] + "." + adr[3] + ":" + ((adr[4] << 8) | (adr[5] << 0));
                case BacnetAddressTypes.MSTP:
                    if(adr == null || adr.Length < 1) return "-1";
                    return adr[0].ToString();
                case BacnetAddressTypes.PTP:
                    return "x";
                case BacnetAddressTypes.Ethernet:
                    StringBuilder sb1 = new StringBuilder();
                    for (int i = 0; i < 6; i++)
                    {
                        sb1.Append(adr[i].ToString("X2"));
                        if (i != 5) sb1.Append('-');
                    }

                    return sb1.ToString();
                case BacnetAddressTypes.IPV6:
                    if (adr == null || adr.Length != 18) return "[::]";
                    ushort port = (ushort)((adr[16] << 8) | (adr[17] << 0));
                    byte[] Ipv6 = new byte[16];
                    Array.Copy(adr, Ipv6, 16);
                    IPEndPoint ep = new System.Net.IPEndPoint(new IPAddress(Ipv6), (int)port);
                    return ep.ToString();

                case BacnetAddressTypes.SC:
                    StringBuilder sb = new StringBuilder("Vmac ");
                    for (int i = 0; i < 6; i++)
                        sb.Append(VMac[i].ToString("X2"));
                    return sb.ToString();

                 default: // Routed @ are always like this, NPDU do not contains the MAC type, only the lenght
                    if (adr == null) return "?";

                    if (adr.Length == 6) // certainly IP, but not sure (Newron System send it for internal usage with 4*0 bytes)
                        return ToString(BacnetAddressTypes.IP);

                    if (adr.Length == 18)   // Not sure it could appears, since NPDU may contains Vmac ?
                        return ToString(BacnetAddressTypes.IPV6);

                    if (adr.Length==3)
                          return "IPv6 VMac : "+((int)(adr[0] << 16) | (adr[1] << 8) | adr[2]).ToString();

                    StringBuilder sb2 = new StringBuilder();
                    for (int i = 0; i < adr.Length; i++)
                        sb2.Append(adr[i] + " ");
                    return sb2.ToString();
            }
        }

        public String ToString(bool SourceOnly)
        {
            if (this.RoutedSource == null)
                return ToString();
            if (SourceOnly)
                return this.RoutedSource.ToString();
            else
                return this.RoutedSource.ToString() + " via " + ToString();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BacnetAddress)) return false;
            BacnetAddress d = (BacnetAddress)obj;
            if (adr == null && d.adr == null) return true;
            else if (adr == null || d.adr == null) return false;
            else if (adr.Length != d.adr.Length) return false;
            else
            {
                for (int i = 0; i < adr.Length; i++)
                    if (adr[i] != d.adr[i]) return false;

                // Modif FC
                if ((RoutedSource == null) && (d.RoutedSource != null))
                    return false;
                if ((d.RoutedSource==null)&&(RoutedSource == null)) return true;
                return RoutedSource.Equals(d.RoutedSource);

            }

        }

        // checked if device is routed by curent equipement
        public bool IsMyRouter(BacnetAddress device)
        {
            if ((device.RoutedSource == null)||(RoutedSource!=null))
                return false;
            if (adr.Length != device.adr.Length) return false;

            for (int i = 0; i < adr.Length; i++)
                if (adr[i] != device.adr[i]) return false;

            return true;
        }

        public void ASN1encode(EncodeBuffer buffer)
        {
            ASN1.encode_opening_tag(buffer, 1);
            ASN1.encode_application_unsigned(buffer, net);
            ASN1.encode_application_octet_string(buffer, adr, 0, adr.Length);
            ASN1.encode_closing_tag(buffer, 1);
        }

        public string FullHashString()
        {
            StringBuilder s = new StringBuilder(((uint)type).ToString()+"." + net.ToString()+".");
            for (int i=0;i<adr.Length;i++)
                s.Append(adr[i].ToString("X2"));
            if (RoutedSource != null)
                s.Append(":"+RoutedSource.FullHashString());

            return s.ToString();

        }
    }

    public enum BacnetPtpFrameTypes : byte
    {
        FRAME_TYPE_HEARTBEAT_XOFF = 0,
        FRAME_TYPE_HEARTBEAT_XON = 1,
        FRAME_TYPE_DATA0 = 2,
        FRAME_TYPE_DATA1 = 3,
        FRAME_TYPE_DATA_ACK0_XOFF = 4,
        FRAME_TYPE_DATA_ACK1_XOFF = 5,
        FRAME_TYPE_DATA_ACK0_XON = 6,
        FRAME_TYPE_DATA_ACK1_XON = 7,
        FRAME_TYPE_DATA_NAK0_XOFF = 8,
        FRAME_TYPE_DATA_NAK1_XOFF = 9,
        FRAME_TYPE_DATA_NAK0_XON = 0x0A,
        FRAME_TYPE_DATA_NAK1_XON = 0x0B,
        FRAME_TYPE_CONNECT_REQUEST = 0x0C,
        FRAME_TYPE_CONNECT_RESPONSE = 0x0D,
        FRAME_TYPE_DISCONNECT_REQUEST = 0x0E,
        FRAME_TYPE_DISCONNECT_RESPONSE = 0x0F,
        FRAME_TYPE_TEST_REQUEST = 0x14,
        FRAME_TYPE_TEST_RESPONSE = 0x15,
        FRAME_TYPE_GREETING = 0xFF,     //special invention
    }

    public enum BacnetPtpDisconnectReasons : byte
    {
        PTP_DISCONNECT_NO_MORE_DATA = 0,
        PTP_DISCONNECT_PREEMPTED = 1,
        PTP_DISCONNECT_INVALID_PASSWORD = 2,
        PTP_DISCONNECT_OTHER = 3,
    }

    /* MS/TP Frame Type */
    public enum BacnetMstpFrameTypes : byte
    {
        /* Frame Types 8 through 127 are reserved by ASHRAE. */
        FRAME_TYPE_TOKEN = 0,
        FRAME_TYPE_POLL_FOR_MASTER = 1,
        FRAME_TYPE_REPLY_TO_POLL_FOR_MASTER = 2,
        FRAME_TYPE_TEST_REQUEST = 3,
        FRAME_TYPE_TEST_RESPONSE = 4,
        FRAME_TYPE_BACNET_DATA_EXPECTING_REPLY = 5,
        FRAME_TYPE_BACNET_DATA_NOT_EXPECTING_REPLY = 6,
        FRAME_TYPE_REPLY_POSTPONED = 7,
        /* Frame Types 128 through 255: Proprietary Frames */
        /* These frames are available to vendors as proprietary (non-BACnet) frames. */
        /* The first two octets of the Data field shall specify the unique vendor */
        /* identification code, most significant octet first, for the type of */
        /* vendor-proprietary frame to be conveyed. The length of the data portion */
        /* of a Proprietary frame shall be in the range of 2 to 501 octets. */
        FRAME_TYPE_PROPRIETARY_MIN = 128,
        FRAME_TYPE_PROPRIETARY_MAX = 255,
    };

    public enum BacnetPropertyIds
    {
        PROP_ACKED_TRANSITIONS = 0,
        PROP_ACK_REQUIRED = 1,
        PROP_ACTION = 2,
        PROP_ACTION_TEXT = 3,
        PROP_ACTIVE_TEXT = 4,
        PROP_ACTIVE_VT_SESSIONS = 5,
        PROP_ALARM_VALUE = 6,
        PROP_ALARM_VALUES = 7,
        PROP_ALL = 8,
        PROP_ALL_WRITES_SUCCESSFUL = 9,
        PROP_APDU_SEGMENT_TIMEOUT = 10,
        PROP_APDU_TIMEOUT = 11,
        PROP_APPLICATION_SOFTWARE_VERSION = 12,
        PROP_ARCHIVE = 13,
        PROP_BIAS = 14,
        PROP_CHANGE_OF_STATE_COUNT = 15,
        PROP_CHANGE_OF_STATE_TIME = 16,
        PROP_NOTIFICATION_CLASS = 17,
        PROP_BLANK_1 = 18,
        PROP_CONTROLLED_VARIABLE_REFERENCE = 19,
        PROP_CONTROLLED_VARIABLE_UNITS = 20,
        PROP_CONTROLLED_VARIABLE_VALUE = 21,
        PROP_COV_INCREMENT = 22,
        PROP_DATE_LIST = 23,
        PROP_DAYLIGHT_SAVINGS_STATUS = 24,
        PROP_DEADBAND = 25,
        PROP_DERIVATIVE_CONSTANT = 26,
        PROP_DERIVATIVE_CONSTANT_UNITS = 27,
        PROP_DESCRIPTION = 28,
        PROP_DESCRIPTION_OF_HALT = 29,
        PROP_DEVICE_ADDRESS_BINDING = 30,
        PROP_DEVICE_TYPE = 31,
        PROP_EFFECTIVE_PERIOD = 32,
        PROP_ELAPSED_ACTIVE_TIME = 33,
        PROP_ERROR_LIMIT = 34,
        PROP_EVENT_ENABLE = 35,
        PROP_EVENT_STATE = 36,
        PROP_EVENT_TYPE = 37,
        PROP_EXCEPTION_SCHEDULE = 38,
        PROP_FAULT_VALUES = 39,
        PROP_FEEDBACK_VALUE = 40,
        PROP_FILE_ACCESS_METHOD = 41,
        PROP_FILE_SIZE = 42,
        PROP_FILE_TYPE = 43,
        PROP_FIRMWARE_REVISION = 44,
        PROP_HIGH_LIMIT = 45,
        PROP_INACTIVE_TEXT = 46,
        PROP_IN_PROCESS = 47,
        PROP_INSTANCE_OF = 48,
        PROP_INTEGRAL_CONSTANT = 49,
        PROP_INTEGRAL_CONSTANT_UNITS = 50,
        PROP_ISSUE_CONFIRMED_NOTIFICATIONS = 51,
        PROP_LIMIT_ENABLE = 52,
        PROP_LIST_OF_GROUP_MEMBERS = 53,
        PROP_LIST_OF_OBJECT_PROPERTY_REFERENCES = 54,
        PROP_LIST_OF_SESSION_KEYS = 55,
        PROP_LOCAL_DATE = 56,
        PROP_LOCAL_TIME = 57,
        PROP_LOCATION = 58,
        PROP_LOW_LIMIT = 59,
        PROP_MANIPULATED_VARIABLE_REFERENCE = 60,
        PROP_MAXIMUM_OUTPUT = 61,
        PROP_MAX_APDU_LENGTH_ACCEPTED = 62,
        PROP_MAX_INFO_FRAMES = 63,
        PROP_MAX_MASTER = 64,
        PROP_MAX_PRES_VALUE = 65,
        PROP_MINIMUM_OFF_TIME = 66,
        PROP_MINIMUM_ON_TIME = 67,
        PROP_MINIMUM_OUTPUT = 68,
        PROP_MIN_PRES_VALUE = 69,
        PROP_MODEL_NAME = 70,
        PROP_MODIFICATION_DATE = 71,
        PROP_NOTIFY_TYPE = 72,
        PROP_NUMBER_OF_APDU_RETRIES = 73,
        PROP_NUMBER_OF_STATES = 74,
        PROP_OBJECT_IDENTIFIER = 75,
        PROP_OBJECT_LIST = 76,
        PROP_OBJECT_NAME = 77,
        PROP_OBJECT_PROPERTY_REFERENCE = 78,
        PROP_OBJECT_TYPE = 79,
        PROP_OPTIONAL = 80,
        PROP_OUT_OF_SERVICE = 81,
        PROP_OUTPUT_UNITS = 82,
        PROP_EVENT_PARAMETERS = 83,
        PROP_POLARITY = 84,
        PROP_PRESENT_VALUE = 85,
        PROP_PRIORITY = 86,
        PROP_PRIORITY_ARRAY = 87,
        PROP_PRIORITY_FOR_WRITING = 88,
        PROP_PROCESS_IDENTIFIER = 89,
        PROP_PROGRAM_CHANGE = 90,
        PROP_PROGRAM_LOCATION = 91,
        PROP_PROGRAM_STATE = 92,
        PROP_PROPORTIONAL_CONSTANT = 93,
        PROP_PROPORTIONAL_CONSTANT_UNITS = 94,
        PROP_PROTOCOL_CONFORMANCE_CLASS = 95,       /* deleted in version 1 revision 2 */
        PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED = 96,
        PROP_PROTOCOL_SERVICES_SUPPORTED = 97,
        PROP_PROTOCOL_VERSION = 98,
        PROP_READ_ONLY = 99,
        PROP_REASON_FOR_HALT = 100,
        PROP_RECIPIENT = 101,
        PROP_RECIPIENT_LIST = 102,
        PROP_RELIABILITY = 103,
        PROP_RELINQUISH_DEFAULT = 104,
        PROP_REQUIRED = 105,
        PROP_RESOLUTION = 106,
        PROP_SEGMENTATION_SUPPORTED = 107,
        PROP_SETPOINT = 108,
        PROP_SETPOINT_REFERENCE = 109,
        PROP_STATE_TEXT = 110,
        PROP_STATUS_FLAGS = 111,
        PROP_SYSTEM_STATUS = 112,
        PROP_TIME_DELAY = 113,
        PROP_TIME_OF_ACTIVE_TIME_RESET = 114,
        PROP_TIME_OF_STATE_COUNT_RESET = 115,
        PROP_TIME_SYNCHRONIZATION_RECIPIENTS = 116,
        PROP_UNITS = 117,
        PROP_UPDATE_INTERVAL = 118,
        PROP_UTC_OFFSET = 119,
        PROP_VENDOR_IDENTIFIER = 120,
        PROP_VENDOR_NAME = 121,
        PROP_VT_CLASSES_SUPPORTED = 122,
        PROP_WEEKLY_SCHEDULE = 123,
        PROP_ATTEMPTED_SAMPLES = 124,
        PROP_AVERAGE_VALUE = 125,
        PROP_BUFFER_SIZE = 126,
        PROP_CLIENT_COV_INCREMENT = 127,
        PROP_COV_RESUBSCRIPTION_INTERVAL = 128,
        PROP_CURRENT_NOTIFY_TIME = 129,
        PROP_EVENT_TIME_STAMPS = 130,
        PROP_LOG_BUFFER = 131,
        PROP_LOG_DEVICE_OBJECT_PROPERTY = 132,
        /* The enable property is renamed from log-enable in
           Addendum b to ANSI/ASHRAE 135-2004(135b-2) */
        PROP_ENABLE = 133,
        PROP_LOG_INTERVAL = 134,
        PROP_MAXIMUM_VALUE = 135,
        PROP_MINIMUM_VALUE = 136,
        PROP_NOTIFICATION_THRESHOLD = 137,
        PROP_PREVIOUS_NOTIFY_TIME = 138,
        PROP_PROTOCOL_REVISION = 139,
        PROP_RECORDS_SINCE_NOTIFICATION = 140,
        PROP_RECORD_COUNT = 141,
        PROP_START_TIME = 142,
        PROP_STOP_TIME = 143,
        PROP_STOP_WHEN_FULL = 144,
        PROP_TOTAL_RECORD_COUNT = 145,
        PROP_VALID_SAMPLES = 146,
        PROP_WINDOW_INTERVAL = 147,
        PROP_WINDOW_SAMPLES = 148,
        PROP_MAXIMUM_VALUE_TIMESTAMP = 149,
        PROP_MINIMUM_VALUE_TIMESTAMP = 150,
        PROP_VARIANCE_VALUE = 151,
        PROP_ACTIVE_COV_SUBSCRIPTIONS = 152,
        PROP_BACKUP_FAILURE_TIMEOUT = 153,
        PROP_CONFIGURATION_FILES = 154,
        PROP_DATABASE_REVISION = 155,
        PROP_DIRECT_READING = 156,
        PROP_LAST_RESTORE_TIME = 157,
        PROP_MAINTENANCE_REQUIRED = 158,
        PROP_MEMBER_OF = 159,
        PROP_MODE = 160,
        PROP_OPERATION_EXPECTED = 161,
        PROP_SETTING = 162,
        PROP_SILENCED = 163,
        PROP_TRACKING_VALUE = 164,
        PROP_ZONE_MEMBERS = 165,
        PROP_LIFE_SAFETY_ALARM_VALUES = 166,
        PROP_MAX_SEGMENTS_ACCEPTED = 167,
        PROP_PROFILE_NAME = 168,
        PROP_AUTO_SLAVE_DISCOVERY = 169,
        PROP_MANUAL_SLAVE_ADDRESS_BINDING = 170,
        PROP_SLAVE_ADDRESS_BINDING = 171,
        PROP_SLAVE_PROXY_ENABLE = 172,
        PROP_LAST_NOTIFY_RECORD = 173,
        PROP_SCHEDULE_DEFAULT = 174,
        PROP_ACCEPTED_MODES = 175,
        PROP_ADJUST_VALUE = 176,
        PROP_COUNT = 177,
        PROP_COUNT_BEFORE_CHANGE = 178,
        PROP_COUNT_CHANGE_TIME = 179,
        PROP_COV_PERIOD = 180,
        PROP_INPUT_REFERENCE = 181,
        PROP_LIMIT_MONITORING_INTERVAL = 182,
        PROP_LOGGING_OBJECT = 183,
        PROP_LOGGING_RECORD = 184,
        PROP_PRESCALE = 185,
        PROP_PULSE_RATE = 186,
        PROP_SCALE = 187,
        PROP_SCALE_FACTOR = 188,
        PROP_UPDATE_TIME = 189,
        PROP_VALUE_BEFORE_CHANGE = 190,
        PROP_VALUE_SET = 191,
        PROP_VALUE_CHANGE_TIME = 192,
        /* enumerations 193-206 are new */
        PROP_ALIGN_INTERVALS = 193,
        /* enumeration 194 is unassigned */
        PROP_INTERVAL_OFFSET = 195,
        PROP_LAST_RESTART_REASON = 196,
        PROP_LOGGING_TYPE = 197,
        /* enumeration 198-201 is unassigned */
        PROP_RESTART_NOTIFICATION_RECIPIENTS = 202,
        PROP_TIME_OF_DEVICE_RESTART = 203,
        PROP_TIME_SYNCHRONIZATION_INTERVAL = 204,
        PROP_TRIGGER = 205,
        PROP_UTC_TIME_SYNCHRONIZATION_RECIPIENTS = 206,
        /* enumerations 207-211 are used in Addendum d to ANSI/ASHRAE 135-2004 */
        PROP_NODE_SUBTYPE = 207,
        PROP_NODE_TYPE = 208,
        PROP_STRUCTURED_OBJECT_LIST = 209,
        PROP_SUBORDINATE_ANNOTATIONS = 210,
        PROP_SUBORDINATE_LIST = 211,
        /* enumerations 212-225 are used in Addendum e to ANSI/ASHRAE 135-2004 */
        PROP_ACTUAL_SHED_LEVEL = 212,
        PROP_DUTY_WINDOW = 213,
        PROP_EXPECTED_SHED_LEVEL = 214,
        PROP_FULL_DUTY_BASELINE = 215,
        /* enumerations 216-217 are unassigned */
        /* enumerations 212-225 are used in Addendum e to ANSI/ASHRAE 135-2004 */
        PROP_REQUESTED_SHED_LEVEL = 218,
        PROP_SHED_DURATION = 219,
        PROP_SHED_LEVEL_DESCRIPTIONS = 220,
        PROP_SHED_LEVELS = 221,
        PROP_STATE_DESCRIPTION = 222,
        /* enumerations 223-225 are unassigned  */
        /* enumerations 226-235 are used in Addendum f to ANSI/ASHRAE 135-2004 */
        PROP_DOOR_ALARM_STATE = 226,
        PROP_DOOR_EXTENDED_PULSE_TIME = 227,
        PROP_DOOR_MEMBERS = 228,
        PROP_DOOR_OPEN_TOO_LONG_TIME = 229,
        PROP_DOOR_PULSE_TIME = 230,
        PROP_DOOR_STATUS = 231,
        PROP_DOOR_UNLOCK_DELAY_TIME = 232,
        PROP_LOCK_STATUS = 233,
        PROP_MASKED_ALARM_VALUES = 234,
        PROP_SECURED_STATUS = 235,
        /* enumerations 236-243 are unassigned  */
        /* enumerations 244-311 are used in Addendum j to ANSI/ASHRAE 135-2004 */
        PROP_ABSENTEE_LIMIT = 244,
        PROP_ACCESS_ALARM_EVENTS = 245,
        PROP_ACCESS_DOORS = 246,
        PROP_ACCESS_EVENT = 247,
        PROP_ACCESS_EVENT_AUTHENTICATION_FACTOR = 248,
        PROP_ACCESS_EVENT_CREDENTIAL = 249,
        PROP_ACCESS_EVENT_TIME = 250,
        PROP_ACCESS_TRANSACTION_EVENTS = 251,
        PROP_ACCOMPANIMENT = 252,
        PROP_ACCOMPANIMENT_TIME = 253,
        PROP_ACTIVATION_TIME = 254,
        PROP_ACTIVE_AUTHENTICATION_POLICY = 255,
        PROP_ASSIGNED_ACCESS_RIGHTS = 256,
        PROP_AUTHENTICATION_FACTORS = 257,
        PROP_AUTHENTICATION_POLICY_LIST = 258,
        PROP_AUTHENTICATION_POLICY_NAMES = 259,
        PROP_AUTHENTICATION_STATUS = 260,
        PROP_AUTHORIZATION_MODE = 261,
        PROP_BELONGS_TO = 262,
        PROP_CREDENTIAL_DISABLE = 263,
        PROP_CREDENTIAL_STATUS = 264,
        PROP_CREDENTIALS = 265,
        PROP_CREDENTIALS_IN_ZONE = 266,
        PROP_DAYS_REMAINING = 267,
        PROP_ENTRY_POINTS = 268,
        PROP_EXIT_POINTS = 269,
        PROP_EXPIRY_TIME = 270,
        PROP_EXTENDED_TIME_ENABLE = 271,
        PROP_FAILED_ATTEMPT_EVENTS = 272,
        PROP_FAILED_ATTEMPTS = 273,
        PROP_FAILED_ATTEMPTS_TIME = 274,
        PROP_LAST_ACCESS_EVENT = 275,
        PROP_LAST_ACCESS_POINT = 276,
        PROP_LAST_CREDENTIAL_ADDED = 277,
        PROP_LAST_CREDENTIAL_ADDED_TIME = 278,
        PROP_LAST_CREDENTIAL_REMOVED = 279,
        PROP_LAST_CREDENTIAL_REMOVED_TIME = 280,
        PROP_LAST_USE_TIME = 281,
        PROP_LOCKOUT = 282,
        PROP_LOCKOUT_RELINQUISH_TIME = 283,
        PROP_MASTER_EXEMPTION = 284,
        PROP_MAX_FAILED_ATTEMPTS = 285,
        PROP_MEMBERS = 286,
        PROP_MUSTER_POINT = 287,
        PROP_NEGATIVE_ACCESS_RULES = 288,
        PROP_NUMBER_OF_AUTHENTICATION_POLICIES = 289,
        PROP_OCCUPANCY_COUNT = 290,
        PROP_OCCUPANCY_COUNT_ADJUST = 291,
        PROP_OCCUPANCY_COUNT_ENABLE = 292,
        PROP_OCCUPANCY_EXEMPTION = 293,
        PROP_OCCUPANCY_LOWER_LIMIT = 294,
        PROP_OCCUPANCY_LOWER_LIMIT_ENFORCED = 295,
        PROP_OCCUPANCY_STATE = 296,
        PROP_OCCUPANCY_UPPER_LIMIT = 297,
        PROP_OCCUPANCY_UPPER_LIMIT_ENFORCED = 298,
        PROP_PASSBACK_EXEMPTION = 299,
        PROP_PASSBACK_MODE = 300,
        PROP_PASSBACK_TIMEOUT = 301,
        PROP_POSITIVE_ACCESS_RULES = 302,
        PROP_REASON_FOR_DISABLE = 303,
        PROP_SUPPORTED_FORMATS = 304,
        PROP_SUPPORTED_FORMAT_CLASSES = 305,
        PROP_THREAT_AUTHORITY = 306,
        PROP_THREAT_LEVEL = 307,
        PROP_TRACE_FLAG = 308,
        PROP_TRANSACTION_NOTIFICATION_CLASS = 309,
        PROP_USER_EXTERNAL_IDENTIFIER = 310,
        PROP_USER_INFORMATION_REFERENCE = 311,
        /* enumerations 312-316 are unassigned */
        PROP_USER_NAME = 317,
        PROP_USER_TYPE = 318,
        PROP_USES_REMAINING = 319,
        PROP_ZONE_FROM = 320,
        PROP_ZONE_TO = 321,
        PROP_ACCESS_EVENT_TAG = 322,
        PROP_GLOBAL_IDENTIFIER = 323,
        /* enumerations 324-325 are unassigned */
        PROP_VERIFICATION_TIME = 326,
        PROP_BASE_DEVICE_SECURITY_POLICY = 327,
        PROP_DISTRIBUTION_KEY_REVISION = 328,
        PROP_DO_NOT_HIDE = 329,
        PROP_KEY_SETS = 330,
        PROP_LAST_KEY_SERVER = 331,
        PROP_NETWORK_ACCESS_SECURITY_POLICIES = 332,
        PROP_PACKET_REORDER_TIME = 333,
        PROP_SECURITY_PDU_TIMEOUT = 334,
        PROP_SECURITY_TIME_WINDOW = 335,
        PROP_SUPPORTED_SECURITY_ALGORITHM = 336,
        PROP_UPDATE_KEY_SET_TIMEOUT = 337,
        PROP_BACKUP_AND_RESTORE_STATE = 338,
        PROP_BACKUP_PREPARATION_TIME = 339,
        PROP_RESTORE_COMPLETION_TIME = 340,
        PROP_RESTORE_PREPARATION_TIME = 341,
        /* enumerations 342-344 are defined in Addendum 2008-w */
        PROP_BIT_MASK = 342,
        PROP_BIT_TEXT = 343,
        PROP_IS_UTC = 344,
        PROP_GROUP_MEMBERS = 345,
        PROP_GROUP_MEMBER_NAMES = 346,
        PROP_MEMBER_STATUS_FLAGS = 347,
        PROP_REQUESTED_UPDATE_INTERVAL = 348,
        PROP_COVU_PERIOD = 349,
        PROP_COVU_RECIPIENTS = 350,
        PROP_EVENT_MESSAGE_TEXTS = 351,
        /* enumerations 352-363 are defined in Addendum 2010-af */
        PROP_EVENT_MESSAGE_TEXTS_CONFIG = 352,
        PROP_EVENT_DETECTION_ENABLE = 353,
        PROP_EVENT_ALGORITHM_INHIBIT = 354,
        PROP_EVENT_ALGORITHM_INHIBIT_REF = 355,
        PROP_TIME_DELAY_NORMAL = 356,
        PROP_RELIABILITY_EVALUATION_INHIBIT = 357,
        PROP_FAULT_PARAMETERS = 358,
        PROP_FAULT_TYPE = 359,
        PROP_LOCAL_FORWARDING_ONLY = 360,
        PROP_PROCESS_IDENTIFIER_FILTER = 361,
        PROP_SUBSCRIBED_RECIPIENTS = 362,
        PROP_PORT_FILTER = 363,
        /* enumeration 364 is defined in Addendum 2010-ae */
        PROP_AUTHORIZATION_EXEMPTIONS = 364,
        /* enumerations 365-370 are defined in Addendum 2010-aa */
        PROP_ALLOW_GROUP_DELAY_INHIBIT = 365,
        PROP_CHANNEL_NUMBER = 366,
        PROP_CONTROL_GROUPS = 367,
        PROP_EXECUTION_DELAY = 368,
        PROP_LAST_PRIORITY = 369,
        PROP_WRITE_STATUS = 370,
        /* enumeration 371 is defined in Addendum 2010-ao */
        PROP_PROPERTY_LIST = 371,
        /* enumeration 372 is defined in Addendum 2010-ak */
        PROP_SERIAL_NUMBER = 372,
        /* enumerations 373-386 are defined in Addendum 2010-i */
        PROP_BLINK_WARN_ENABLE = 373,
        PROP_DEFAULT_FADE_TIME = 374,
        PROP_DEFAULT_RAMP_RATE = 375,
        PROP_DEFAULT_STEP_INCREMENT = 376,
        PROP_EGRESS_TIME = 377,
        PROP_IN_PROGRESS = 378,
        PROP_INSTANTANEOUS_POWER = 379,
        PROP_LIGHTING_COMMAND = 380,
        PROP_LIGHTING_COMMAND_DEFAULT_PRIORITY = 381,
        PROP_MAX_ACTUAL_VALUE = 382,
        PROP_MIN_ACTUAL_VALUE = 383,
        PROP_POWER = 384,
        PROP_TRANSITION = 385,
        PROP_EGRESS_ACTIVE = 386,

        PROP_INTERFACE_VALUE = 387,
        PROP_FAULT_HIGH_LIMIT = 388,
        PROP_FAULT_LOW_LIMIT = 389,
        PROP_LOW_DIFF_LIMIT = 390,
        /* enumerations 391-392 are defined in Addendum 135-2012az */
        PROP_STRIKE_COUNT = 391,
        PROP_TIME_OF_STRIKE_COUNT_RESET = 392,
        /* enumerations 393-398 are defined in Addendum 135-2012ay */
        PROP_DEFAULT_TIMEOUT = 393,
        PROP_INITIAL_TIMEOUT = 394,
        PROP_LAST_STATE_CHANGE = 395,
        PROP_STATE_CHANGE_VALUES = 396,
        PROP_TIMER_RUNNING = 397,
        PROP_TIMER_STATE = 398,
        PROP_APDU_LENGTH = 399,
        PROP_IP_ADDRESS = 400,
        PROP_IP_DEFAULT_GATEWAY = 401,
        PROP_IP_DHCP_ENABLE = 402,
        PROP_IP_DHCP_LEASE_TIME = 403,
        PROP_IP_DHCP_LEASE_TIME_REMAINING = 404,
        PROP_IP_DHCP_SERVER = 405,
        PROP_IP_DNS_SERVER = 406,
        PROP_BACNET_IP_GLOBAL_ADDRESS = 407,
        PROP_BACNET_IP_MODE = 408,
        PROP_BACNET_IP_MULTICAST_ADDRESS = 409,
        PROP_BACNET_IP_NAT_TRAVERSAL = 410,
        PROP_IP_SUBNET_MASK = 411,
        PROP_BACNET_IP_UDP_PORT = 412,

        PROP_BBMD_ACCEPT_FD_REGISTRATIONS = 413,
        PROP_BBMD_BROADCAST_DISTRIBUTION_TABLE = 414,
        PROP_BBMD_FOREIGN_DEVICE_TABLE = 415,
        PROP_CHANGES_PENDING = 416,
        PROP_COMMAND = 417,
        PROP_FD_BBMD_ADDRESS = 418,
        PROP_FD_SUBSCRIPTION_LIFETIME = 419,
        PROP_LINK_SPEED = 420,
        PROP_LINK_SPEEDS = 421,
        PROP_LINK_SPEED_AUTONEGOTIATE = 422,
        PROP_MAC_ADDRESS = 423,
        PROP_NETWORK_INTERFACE_NAME = 424,
        PROP_NETWORK_NUMBER = 425,
        PROP_NETWORK_NUMBER_QUALITY = 426,
        PROP_NETWORK_TYPE = 427,
        PROP_ROUTING_TABLE = 428,
        PROP_VIRTUAL_MAC_ADDRESS_TABLE = 429,
        // ADDENDUM_135_2012AS
        PROP_COMMAND_TIME_ARRAY = 430,
        PROP_CURRENT_COMMAND_PRIORITY = 431,
        PROP_LAST_COMMAND_TIME = 432,
        PROP_VALUE_SOURCE = 433,
        PROP_VALUE_SOURCE_ARRAY = 434,
        PROP_BACNET_IPV6_MODE = 435,
        PROP_IPV6_ADDRESS = 436,
        PROP_IPV6_PREFIX_LENGTH = 437,
        PROP_BACNET_IPV6_UDP_PORT = 438,
        PROP_IPV6_DEFAULT_GATEWAY = 439,
        PROP_BACNET_IPV6_MULTICAST_ADDRESS = 440,
        PROP_IPV6_DNS_SERVER = 441,
        PROP_IPV6_AUTO_ADDRESSING_ENABLE = 442,
        PROP_IPV6_DHCP_LEASE_TIME = 443,
        PROP_IPV6_DHCP_LEASE_TIME_REMAINING = 444,
        PROP_IPV6_DHCP_SERVER = 445,
        PROP_IPV6_ZONE_INDEX = 446,
        PROP_CAR_ASSIGNED_DIRECTION = 448,
        PROP_CAR_DOOR_COMMAND = 449,
        PROP_CAR_DOOR_STATUS = 450,
        PROP_CAR_DOOR_TEXT = 451,
        PROP_CAR_DOOR_ZONE = 452,
        PROP_CAR_DRIVE_STATUS = 453,
        PROP_CAR_LOAD = 454,
        PROP_CAR_LOAD_UNITS = 455,
        PROP_CAR_MODE = 456,
        PROP_CAR_MOVING_DIRECTION = 457,
        PROP_CAR_POSITION = 458,
        PROP_ELEVATOR_GROUP = 459,
        PROP_ENERGY_METER = 460,
        PROP_ENERGY_METER_REF = 461,
        PROP_ESCALATOR_MODE = 462,
        PROP_FLOOR_TEXT = 464,
        PROP_GROUP_ID = 465,
        PROP_GROUP_MODE = 467,
        PROP_HIGHER_DECK = 468,
        PROP_INSTALLATION_ID = 469,

        PROP_LANDING_CALLS = 470,
        PROP_LANDING_CALL_CONTROL = 471,
        PROP_LANDING_DOOR_STATUS = 472,
        PROP_LOWER_DECK = 473,
        PROP_MACHINE_ROOM_ID = 474,
        PROP_MAKING_CAR_CALL = 475,
        PROP_NEXT_STOPPING_FLOOR = 476,
        PROP_OPERATION_DIRECTION = 477,
        PROP_PASSENGER_ALARM = 478,
        PROP_POWER_MODE = 479,
        PROP_REGISTERED_CAR_CALL = 480,
        PROP_ACTIVE_COV_MULTIPLE_SUBSCRIPTIONS = 481,
        PROP_PROTOCOL_LEVEL = 482,
        PROP_REFERENCE_PORT = 483,
        PROP_DEPLOYED_PROFILE_LOCATION = 484,
        PROP_PROFILE_LOCATION = 485,
        PROP_TAGS = 486,
        PROP_SUBORDINATE_NODE_TYPES = 487,
        PROP_SUBORDINATE_RELATIONSHIPS = 489,
        PROP_SUBORDINATE_TAGS = 488,
        PROP_DEFAULT_SUBORDINATE_RELATIONSHIP = 490,
        PROP_REPRESENTS = 491,
        PROP_DEFAULT_PRESENT_VALUE = 492, //Addendum 135-2016bd
        PROP_PRESENT_STAGE = 493, //Addendum 135-2016bd
        PROP_STAGES = 494, //Addendum 135-2016bd
        PROP_STAGE_NAMES = 495, //Addendum 135-2016bd
        PROP_TARGET_REFERENCES = 496, //Addendum 135-2016bd

        PROP_DEVICE_UUID = 507, // Addendum 135-2016bj

        /* The special property identifiers all, optional, and required  */
        /* are reserved for use in the ReadPropertyConditional and */
        /* ReadPropertyMultiple services or services not defined in this standard. */
        /* Enumerated values 0-511 are reserved for definition by ASHRAE.  */
        /* Enumerated values 512-4194303 may be used by others subject to the  */
        /* procedures and constraints described in Clause 23.  */
        /* do the max range inside of enum so that
           compilers will allocate adequate sized datatype for enum
           which is used to store decoding */
        MAX_BACNET_PROPERTY_ID = 4194303,
    };

    public enum BacnetNodeTypes
    {
        NT_UNKNOWN = 0,
        NT_SYSTEM = 1,
        NT_NETWORK = 2,
        NT_DEVICE = 3,
        NT_ORGANIZATIONAL = 4,
        NT_AREA = 5,
        NT_EQUIPMENT = 6,
        NT_POINT = 7,
        NT_COLLECTION= 8,
        NT_PROPERTY = 9,
        NT_FUNCTIONAL = 10,
        NT_OTHER = 11,
        NT_SUBSYSTEM = 12,
        NT_BUILDING = 13,
        NT_FLOOR = 14,
        NT_SECTION = 15,
        NT_MODULE = 16,
        NT_TREE = 17,
        NT_MEMBER = 18,
        NT_PROTOCOL = 19,
        NT_ROOM = 20,
        NT_ZONE = 21

    }



    [Flags]
    public enum BacnetNpduControls : byte
    {
        PriorityNormalMessage = 0,
        PriorityUrgentMessage = 1,
        PriorityCriticalMessage = 2,
        PriorityLifeSafetyMessage = 3,
        ExpectingReply = 4,
        SourceSpecified = 8,
        DestinationSpecified = 32,
        NetworkLayerMessage = 128,
    };

    /*Network Layer Message Type */
    /*If Bit 7 of the control octet described in 6.2.2 is 1, */
    /* a message type octet shall be present as shown in Figure 6-1. */
    /* The following message types are indicated: */
    public enum BacnetNetworkMessageTypes : byte
    {
        NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK = 0,
        NETWORK_MESSAGE_I_AM_ROUTER_TO_NETWORK = 1,
        NETWORK_MESSAGE_I_COULD_BE_ROUTER_TO_NETWORK = 2,
        NETWORK_MESSAGE_REJECT_MESSAGE_TO_NETWORK = 3,
        NETWORK_MESSAGE_ROUTER_BUSY_TO_NETWORK = 4,
        NETWORK_MESSAGE_ROUTER_AVAILABLE_TO_NETWORK = 5,
        NETWORK_MESSAGE_INIT_RT_TABLE = 6,
        NETWORK_MESSAGE_INIT_RT_TABLE_ACK = 7,
        NETWORK_MESSAGE_ESTABLISH_CONNECTION_TO_NETWORK = 8,
        NETWORK_MESSAGE_DISCONNECT_CONNECTION_TO_NETWORK = 9,
        /* X'0A' to X'7F': Reserved for use by ASHRAE, */
        /* X'80' to X'FF': Available for vendor proprietary messages */
    } ;

    public enum BacnetReinitializedStates
    {
        BACNET_REINIT_COLDSTART = 0,
        BACNET_REINIT_WARMSTART = 1,
        BACNET_REINIT_STARTBACKUP = 2,
        BACNET_REINIT_ENDBACKUP = 3,
        BACNET_REINIT_STARTRESTORE = 4,
        BACNET_REINIT_ENDRESTORE = 5,
        BACNET_REINIT_ABORTRESTORE = 6,
        BACNET_REINIT_ACTIVATE_CHANGES = 7,
        BACNET_REINIT_IDLE = 255
    };
}

namespace System.IO.BACnet.Serialize
{
    public class MSTP
    {
        public const byte MSTP_PREAMBLE1 = 0x55;
        public const byte MSTP_PREAMBLE2 = 0xFF;
        public const BacnetMaxAdpu MSTP_MAX_APDU = BacnetMaxAdpu.MAX_APDU480;
        public const byte MSTP_HEADER_LENGTH = 8;

        public static byte CRC_Calc_Header(byte dataValue, byte crcValue)
        {
            ushort crc;

            crc = (ushort)(crcValue ^ dataValue); /* XOR C7..C0 with D7..D0 */

            /* Exclusive OR the terms in the table (top down) */
            crc = (ushort)(crc ^ (crc << 1) ^ (crc << 2) ^ (crc << 3) ^ (crc << 4) ^ (crc << 5) ^ (crc << 6) ^ (crc << 7));

            /* Combine bits shifted out left hand end */
            return (byte)((crc & 0xfe) ^ ((crc >> 8) & 1));
        }

        public static byte CRC_Calc_Header(byte[] buffer, int offset, int length)
        {
            byte crc = 0xff;
            for (int i = offset; i < (offset + length); i++)
                crc = CRC_Calc_Header(buffer[i], crc);
            return (byte)~crc;
        }

        public static ushort CRC_Calc_Data(byte dataValue, ushort crcValue)
        {
            ushort crcLow;

            crcLow = (ushort)((crcValue & 0xff) ^ dataValue);     /* XOR C7..C0 with D7..D0 */

            /* Exclusive OR the terms in the table (top down) */
            return (ushort)((crcValue >> 8) ^ (crcLow << 8) ^ (crcLow << 3)
                ^ (crcLow << 12) ^ (crcLow >> 4)
                ^ (crcLow & 0x0f) ^ ((crcLow & 0x0f) << 7));
        }

        public static ushort CRC_Calc_Data(byte[] buffer, int offset, int length)
        {
            ushort crc = 0xffff;
            for (int i = offset; i < (offset + length); i++)
                crc = CRC_Calc_Data(buffer[i], crc);
            return (ushort)~crc;
        }

        public static int Encode(byte[] buffer, int offset, BacnetMstpFrameTypes frame_type, byte destination_address, byte source_address, int msg_length)
        {
            buffer[offset + 0] = MSTP_PREAMBLE1;
            buffer[offset + 1] = MSTP_PREAMBLE2;
            buffer[offset + 2] = (byte)frame_type;
            buffer[offset + 3] = destination_address;
            buffer[offset + 4] = source_address;
            buffer[offset + 5] = (byte)((msg_length & 0xFF00) >> 8);
            buffer[offset + 6] = (byte)((msg_length & 0x00FF) >> 0);
            buffer[offset + 7] = CRC_Calc_Header(buffer, offset + 2, 5);
            if (msg_length > 0)
            {
                //calculate data crc
                ushort data_crc = CRC_Calc_Data(buffer, offset + 8, msg_length);
                buffer[offset + 8 + msg_length + 0] = (byte)(data_crc & 0xFF);  //LSB first!
                buffer[offset + 8 + msg_length + 1] = (byte)(data_crc >> 8);
            }
            //optional pad (0xFF)
            return MSTP_HEADER_LENGTH + (msg_length) + (msg_length > 0 ? 2 : 0);
        }

        public static int Decode(byte[] buffer, int offset, int max_length, out BacnetMstpFrameTypes frame_type, out byte destination_address, out byte source_address, out int msg_length)
        {
            if (max_length < MSTP_HEADER_LENGTH)  //not enough data
            {
                frame_type = BacnetMstpFrameTypes.FRAME_TYPE_BACNET_DATA_EXPECTING_REPLY; // don't care
                destination_address = source_address = 0;   // don't care
                msg_length = 0;
                return -1;
            }
            frame_type = (BacnetMstpFrameTypes)buffer[offset + 2];
            destination_address = buffer[offset + 3];
            source_address = buffer[offset + 4];
            msg_length = (buffer[offset + 5] << 8) | (buffer[offset + 6] << 0);
            byte crc_header = buffer[offset + 7];
            ushort crc_data = 0;

            if (msg_length > 0)
            {
                if ((offset + 8 + msg_length + 1) >= buffer.Length)
                    return -1;
                crc_data = (ushort)((buffer[offset + 8 + msg_length + 1] << 8) | (buffer[offset + 8 + msg_length + 0] << 0));
            }
            if (buffer[offset + 0] != MSTP_PREAMBLE1) return -1;
            if (buffer[offset + 1] != MSTP_PREAMBLE2) return -1;
            if (CRC_Calc_Header(buffer, offset + 2, 5) != crc_header) return -1;
            if (msg_length > 0 && max_length >= (MSTP_HEADER_LENGTH + msg_length + 2) && CRC_Calc_Data(buffer, offset + 8, msg_length) != crc_data) return -1;
            return 8 + (msg_length) + (msg_length > 0 ? 2 : 0);
        }
    }

    public class PTP
    {
        public const byte PTP_PREAMBLE1 = 0x55;
        public const byte PTP_PREAMBLE2 = 0xFF;
        public const byte PTP_GREETING_PREAMBLE1 = 0x42;
        public const byte PTP_GREETING_PREAMBLE2 = 0x41;
        public const BacnetMaxAdpu PTP_MAX_APDU = BacnetMaxAdpu.MAX_APDU480;
        public const byte PTP_HEADER_LENGTH = 6;

        public static int Encode(byte[] buffer, int offset, BacnetPtpFrameTypes frame_type, int msg_length)
        {
            buffer[offset + 0] = PTP_PREAMBLE1;
            buffer[offset + 1] = PTP_PREAMBLE2;
            buffer[offset + 2] = (byte)frame_type;
            buffer[offset + 3] = (byte)((msg_length & 0xFF00) >> 8);
            buffer[offset + 4] = (byte)((msg_length & 0x00FF) >> 0);
            buffer[offset + 5] = MSTP.CRC_Calc_Header(buffer, offset + 2, 3);
            if (msg_length > 0)
            {
                //calculate data crc
                ushort data_crc = MSTP.CRC_Calc_Data(buffer, offset + 6, msg_length);
                buffer[offset + 6 + msg_length + 0] = (byte)(data_crc & 0xFF);  //LSB first!
                buffer[offset + 6 + msg_length + 1] = (byte)(data_crc >> 8);
            }
            return PTP_HEADER_LENGTH + (msg_length) + (msg_length > 0 ? 2 : 0);
        }

        public static int Decode(byte[] buffer, int offset, int max_length, out BacnetPtpFrameTypes frame_type, out int msg_length)
        {
            if (max_length < PTP_HEADER_LENGTH) // not enough data
            {
                frame_type = BacnetPtpFrameTypes.FRAME_TYPE_CONNECT_REQUEST; // don't care
                msg_length = 0;
                return -1;     //not enough data
            }
            frame_type = (BacnetPtpFrameTypes)buffer[offset + 2];
            msg_length = (buffer[offset + 3] << 8) | (buffer[offset + 4] << 0);
            byte crc_header = buffer[offset + 5];
            ushort crc_data = 0;
            if (msg_length > 0)
            {
                if ((offset + 6 + msg_length + 1) >= buffer.Length)
                    return -1;
                crc_data = (ushort)((buffer[offset + 6 + msg_length + 1] << 8) | (buffer[offset + 6 + msg_length + 0] << 0));
            }
            if (buffer[offset + 0] != PTP_PREAMBLE1) return -1;
            if (buffer[offset + 1] != PTP_PREAMBLE2) return -1;
            if (MSTP.CRC_Calc_Header(buffer, offset + 2, 3) != crc_header) return -1;
            if (msg_length > 0 && max_length >= (PTP_HEADER_LENGTH + msg_length + 2) && MSTP.CRC_Calc_Data(buffer, offset + 6, msg_length) != crc_data) return -1;
            return 8 + (msg_length) + (msg_length > 0 ? 2 : 0);
        }

    }

    public class NPDU
    {
        public const byte BACNET_PROTOCOL_VERSION = 1;

        public static BacnetNpduControls DecodeFunction(byte[] buffer, int offset)
        {
            if (buffer[offset + 0] != BACNET_PROTOCOL_VERSION) return 0;
            return (BacnetNpduControls)buffer[offset + 1];
        }

        public static int Decode(byte[] buffer, int offset, out BacnetNpduControls function, out BacnetAddress destination, out BacnetAddress source, out byte hop_count, out BacnetNetworkMessageTypes network_msg_type, out ushort vendor_id)
        {
            int org_offset = offset;

            offset++;
            function = (BacnetNpduControls)buffer[offset++];

            destination = null;
            if ((function & BacnetNpduControls.DestinationSpecified) == BacnetNpduControls.DestinationSpecified)
            {
                destination = new BacnetAddress(BacnetAddressTypes.None, (ushort)((buffer[offset++] << 8) | (buffer[offset++] << 0)), null);
                int adr_len = buffer[offset++];
                if (adr_len > 0)
                {
                    destination.adr = new byte[adr_len];
                    for (int i = 0; i < destination.adr.Length; i++)
                        destination.adr[i] = buffer[offset++];
                }
            }

            source = null;
            if ((function & BacnetNpduControls.SourceSpecified) == BacnetNpduControls.SourceSpecified)
            {
                source = new BacnetAddress(BacnetAddressTypes.None, (ushort)((buffer[offset++] << 8) | (buffer[offset++] << 0)), null);
                int adr_len = buffer[offset++];
                if (adr_len > 0)
                {
                    source.adr = new byte[adr_len];
                    for (int i = 0; i < source.adr.Length; i++)
                        source.adr[i] = buffer[offset++];
                }
            }

            hop_count = 0;
            if ((function & BacnetNpduControls.DestinationSpecified) == BacnetNpduControls.DestinationSpecified)
            {
                hop_count = buffer[offset++];
            }

            network_msg_type = BacnetNetworkMessageTypes.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK;
            vendor_id = 0;
            if ((function & BacnetNpduControls.NetworkLayerMessage) == BacnetNpduControls.NetworkLayerMessage)
            {
                network_msg_type = (BacnetNetworkMessageTypes)buffer[offset++];
                if (((byte)network_msg_type) >= 0x80)
                {
                    vendor_id = (ushort)((buffer[offset++] << 8) | (buffer[offset++] << 0));
                }
                else if (network_msg_type == BacnetNetworkMessageTypes.NETWORK_MESSAGE_WHO_IS_ROUTER_TO_NETWORK)
                    offset += 2;  // Don't care about destination network adress
            }

            if (buffer[org_offset + 0] != BACNET_PROTOCOL_VERSION) return -1;
            return offset - org_offset;
        }

        public static void Encode(EncodeBuffer buffer, BacnetNpduControls function, BacnetAddress destination, BacnetAddress source, byte hop_count, BacnetNetworkMessageTypes network_msg_type, ushort vendor_id)
        {
            Encode(buffer, function, destination, source, hop_count);

            if ((function & BacnetNpduControls.NetworkLayerMessage) > 0) // sure it is, otherwise the other Encode is used
            {
                buffer.buffer[buffer.offset++] = (byte)network_msg_type;
                if (((byte)network_msg_type) >= 0x80) // who used this ??? sure nobody !
                {
                    buffer.buffer[buffer.offset++] = (byte)((vendor_id & 0xFF00) >> 8);
                    buffer.buffer[buffer.offset++] = (byte)((vendor_id & 0x00FF) >> 0);
                }
            }
        }

        public static void Encode(EncodeBuffer buffer, BacnetNpduControls function, BacnetAddress destination, BacnetAddress source=null, byte hop_count=0xFF)
        {
            // Modif FC
            bool has_destination = destination != null && destination.net > 0; // && destination.net != 0xFFFF;
            bool has_source = source != null && source.net > 0 && source.net != 0xFFFF;

            buffer.buffer[buffer.offset++] = BACNET_PROTOCOL_VERSION;
            buffer.buffer[buffer.offset++] = (byte)(function | (has_destination ? BacnetNpduControls.DestinationSpecified : 0) | (has_source ? BacnetNpduControls.SourceSpecified : 0));

            if (has_destination)
            {
                buffer.buffer[buffer.offset++] =(byte)((destination.net & 0xFF00) >> 8);
                buffer.buffer[buffer.offset++] =(byte)((destination.net & 0x00FF) >> 0);

                if (destination.net == 0xFFFF)                  //patch by F. Chaxel
                    buffer.buffer[buffer.offset++] = 0;
                else
                {
                    buffer.buffer[buffer.offset++] = (byte)destination.adr.Length;
                    if (destination.adr.Length > 0)
                    {
                        for (int i = 0; i < destination.adr.Length; i++)
                            buffer.buffer[buffer.offset++] = destination.adr[i];
                    }
                }
            }

            if (has_source)
            {
                buffer.buffer[buffer.offset++] =(byte)((source.net & 0xFF00) >> 8);
                buffer.buffer[buffer.offset++] = (byte)((source.net & 0x00FF) >> 0);

                buffer.buffer[buffer.offset++] = (byte)source.adr.Length;
                if (source.adr.Length > 0)
                {
                    for (int i = 0; i < source.adr.Length; i++)
                        buffer.buffer[buffer.offset++] = source.adr[i];
                }

            }

            if (has_destination)
            {
                buffer.buffer[buffer.offset++] = hop_count;
            }

        }
    }

    public class APDU
    {
        public static BacnetPduTypes GetDecodedType(byte[] buffer, int offset)
        {
            return (BacnetPduTypes)buffer[offset];
        }

        public static void SetDecodedType(byte[] buffer, int offset, BacnetPduTypes type)
        {
            buffer[offset] = (byte)type;
        }

        public static int GetDecodedInvokeId(byte[] buffer, int offset)
        {
            BacnetPduTypes type = GetDecodedType(buffer, offset);
            switch (type & BacnetPduTypes.PDU_TYPE_MASK)
            {
                case BacnetPduTypes.PDU_TYPE_SIMPLE_ACK:
                case BacnetPduTypes.PDU_TYPE_COMPLEX_ACK:
                case BacnetPduTypes.PDU_TYPE_ERROR:
                case BacnetPduTypes.PDU_TYPE_REJECT:
                case BacnetPduTypes.PDU_TYPE_ABORT:
                    return buffer[offset + 1];
                case BacnetPduTypes.PDU_TYPE_CONFIRMED_SERVICE_REQUEST:
                    return buffer[offset + 2];
                default:
                    return -1;
            }
        }

        public static void EncodeConfirmedServiceRequest(EncodeBuffer buffer, BacnetPduTypes type, BacnetConfirmedServices service, BacnetMaxSegments max_segments, BacnetMaxAdpu max_adpu, byte invoke_id, byte sequence_number=0, byte proposed_window_size=0)
        {
            buffer.buffer[buffer.offset++] =(byte)type;
            buffer.buffer[buffer.offset++] =(byte)((byte)max_segments | (byte)max_adpu);
            buffer.buffer[buffer.offset++] =invoke_id;

            if((type & BacnetPduTypes.SEGMENTED_MESSAGE) > 0)
            {
                buffer.buffer[buffer.offset++] =sequence_number;
                buffer.buffer[buffer.offset++] =proposed_window_size;
            }
            buffer.buffer[buffer.offset++] =(byte)service;
        }

        public static int DecodeConfirmedServiceRequest(byte[] buffer, int offset, out BacnetPduTypes type, out BacnetConfirmedServices service, out BacnetMaxSegments max_segments, out BacnetMaxAdpu max_adpu, out byte invoke_id, out byte sequence_number, out byte proposed_window_number)
        {
            int org_offset = offset;

            type = (BacnetPduTypes)buffer[offset++];
            max_segments = (BacnetMaxSegments)(buffer[offset] & 0xF0);
            max_adpu = (BacnetMaxAdpu)(buffer[offset++] & 0x0F);
            invoke_id = buffer[offset++];

            sequence_number = 0;
            proposed_window_number = 0;
            if ((type & BacnetPduTypes.SEGMENTED_MESSAGE) > 0)
            {
                sequence_number = buffer[offset++];
                proposed_window_number = buffer[offset++];
            }
            service = (BacnetConfirmedServices)buffer[offset++];

            return offset - org_offset;
        }

        public static void EncodeUnconfirmedServiceRequest(EncodeBuffer buffer, BacnetPduTypes type, BacnetUnconfirmedServices service)
        {
            buffer.buffer[buffer.offset++] =(byte)type;
            buffer.buffer[buffer.offset++] =(byte)service;
        }

        public static int DecodeUnconfirmedServiceRequest(byte[] buffer, int offset, out BacnetPduTypes type, out BacnetUnconfirmedServices service)
        {
            int org_offset = offset;

            type = (BacnetPduTypes)buffer[offset++];
            service = (BacnetUnconfirmedServices)buffer[offset++];

            return offset - org_offset;
        }

        public static void EncodeSimpleAck(EncodeBuffer buffer, BacnetPduTypes type, BacnetConfirmedServices service, byte invoke_id)
        {
            buffer.buffer[buffer.offset++] =(byte)type;
            buffer.buffer[buffer.offset++] =invoke_id;
            buffer.buffer[buffer.offset++] =(byte)service;
        }

        public static int DecodeSimpleAck(byte[] buffer, int offset, out BacnetPduTypes type, out BacnetConfirmedServices service, out byte invoke_id)
        {
            int org_offset = offset;

            type = (BacnetPduTypes)buffer[offset++];
            invoke_id = buffer[offset++];
            service = (BacnetConfirmedServices)buffer[offset++];

            return offset - org_offset;
        }

        public static int EncodeComplexAck(EncodeBuffer buffer, BacnetPduTypes type, BacnetConfirmedServices service, byte invoke_id, byte sequence_number, byte proposed_window_number)
        {
            int len = 3;
            buffer.buffer[buffer.offset++] =(byte)type;
            buffer.buffer[buffer.offset++] =invoke_id;
            if ((type & BacnetPduTypes.SEGMENTED_MESSAGE) > 0)
            {
                buffer.buffer[buffer.offset++] =sequence_number;
                buffer.buffer[buffer.offset++] =proposed_window_number;
                len += 2;
            }
            buffer.buffer[buffer.offset++] =(byte)service;
            return len;
        }

        public static int DecodeComplexAck(byte[] buffer, int offset, out BacnetPduTypes type, out BacnetConfirmedServices service, out byte invoke_id, out byte sequence_number, out byte proposed_window_number)
        {
            int org_offset = offset;

            type = (BacnetPduTypes)buffer[offset++];
            invoke_id = buffer[offset++];

            sequence_number = 0;
            proposed_window_number = 0;
            if ((type & BacnetPduTypes.SEGMENTED_MESSAGE) > 0)
            {
                sequence_number = buffer[offset++];
                proposed_window_number = buffer[offset++];
            }
            service = (BacnetConfirmedServices)buffer[offset++];

            return offset - org_offset;
        }

        public static void EncodeSegmentAck(EncodeBuffer buffer, BacnetPduTypes type, byte original_invoke_id, byte sequence_number, byte actual_window_size)
        {
            buffer.buffer[buffer.offset++] =(byte)type;
            buffer.buffer[buffer.offset++] =original_invoke_id;
            buffer.buffer[buffer.offset++] =sequence_number;
            buffer.buffer[buffer.offset++] =actual_window_size;
        }

        public static int DecodeSegmentAck(byte[] buffer, int offset, out BacnetPduTypes type, out byte original_invoke_id, out byte sequence_number, out byte actual_window_size)
        {
            int org_offset = offset;

            type = (BacnetPduTypes)buffer[offset++];
            original_invoke_id = buffer[offset++];
            sequence_number = buffer[offset++];
            actual_window_size = buffer[offset++];

            return offset - org_offset;
        }

        public static void EncodeError(EncodeBuffer buffer, BacnetPduTypes type, BacnetConfirmedServices service, byte invoke_id)
        {
            buffer.buffer[buffer.offset++] =(byte)type;
            buffer.buffer[buffer.offset++] =invoke_id;
            buffer.buffer[buffer.offset++] =(byte)service;
        }

        public static int DecodeError(byte[] buffer, int offset, out BacnetPduTypes type, out BacnetConfirmedServices service, out byte invoke_id)
        {
            int org_offset = offset;

            type = (BacnetPduTypes)buffer[offset++];
            invoke_id = buffer[offset++];
            service = (BacnetConfirmedServices)buffer[offset++];

            return offset - org_offset;
        }

        /// <summary>
        /// Also EncodeReject
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        /// <param name="invoke_id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static void EncodeAbort(EncodeBuffer buffer, BacnetPduTypes type, byte invoke_id, byte reason)
        {
            buffer.buffer[buffer.offset++] =(byte)type;
            buffer.buffer[buffer.offset++] =invoke_id;
            buffer.buffer[buffer.offset++] =reason;
        }

        public static int DecodeAbort(byte[] buffer, int offset, out BacnetPduTypes type, out byte invoke_id, out byte reason)
        {
            int org_offset = offset;

            type = (BacnetPduTypes)buffer[offset++];
            invoke_id = buffer[offset++];
            reason = buffer[offset++];

            return offset - org_offset;
        }
    }

    public class ASN1
    {
        public const int BACNET_MAX_OBJECT = 0x3FF;
        public const int BACNET_INSTANCE_BITS = 22;
        public const int BACNET_MAX_INSTANCE = 0x3FFFFF;
        public const int MAX_BITSTRING_BYTES = 15;
        public const uint BACNET_ARRAY_ALL = 0xFFFFFFFFU;
        public const uint BACNET_NO_PRIORITY = 0;
        public const uint BACNET_MIN_PRIORITY = 1;
        public const uint BACNET_MAX_PRIORITY = 16;

        public interface IASN1encode
        {
            void ASN1encode(EncodeBuffer buffer);
        }

        public static void encode_bacnet_object_id(EncodeBuffer buffer, BacnetObjectTypes object_type, UInt32 instance)
        {
            UInt32 value = 0;
            UInt32 type = 0;

            type = (UInt32)object_type;
            value = ((type & BACNET_MAX_OBJECT) << BACNET_INSTANCE_BITS) | (instance & BACNET_MAX_INSTANCE);
            encode_unsigned32(buffer, value);
        }

        public static void encode_tag(EncodeBuffer buffer, byte tag_number, bool context_specific, UInt32 len_value_type)
        {
            int len = 1;
            byte[] tmp = new byte[3];

            tmp[0] = 0;
            if (context_specific) tmp[0] |= 0x8;

            /* additional tag byte after this byte */
            /* for extended tag byte */
            if (tag_number <= 14)
            {
                tmp[0] |= (byte)(tag_number << 4);
            }
            else
            {
                tmp[0] |= 0xF0;
                tmp[1] = tag_number;
                len++;
            }

            /* NOTE: additional len byte(s) after extended tag byte */
            /* if larger than 4 */
            if (len_value_type <= 4)
            {
                tmp[0] |= (byte)len_value_type;
                buffer.Add(tmp, len);
            }
            else
            {
                tmp[0] |= 5;
                if (len_value_type <= 253)
                {
                    tmp[len++] = (byte)len_value_type;
                    buffer.Add(tmp, len);
                }
                else if (len_value_type <= 65535)
                {
                    tmp[len++] = 254;
                    buffer.Add(tmp, len);
                    encode_unsigned16(buffer, (UInt16)len_value_type);
                }
                else
                {
                    tmp[len++] = 255;
                    buffer.Add(tmp, len);
                    encode_unsigned32(buffer, len_value_type);
                }
            }
        }

        public static void encode_bacnet_enumerated(EncodeBuffer buffer, UInt32 value)
        {
            encode_bacnet_unsigned(buffer, value);
        }

        public static void encode_application_object_id(EncodeBuffer buffer, BacnetObjectTypes object_type, UInt32 instance)
        {
            EncodeBuffer tmp1 = new EncodeBuffer();
            encode_bacnet_object_id(tmp1, object_type, instance);
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID, false, (uint)tmp1.offset);
            buffer.Add(tmp1.buffer, tmp1.offset);
        }

        public static void encode_application_unsigned(EncodeBuffer buffer, UInt32 value)
        {
            EncodeBuffer tmp1 = new EncodeBuffer();
            encode_bacnet_unsigned(tmp1, value);
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT, false, (uint)tmp1.offset);
            buffer.Add(tmp1.buffer, tmp1.offset);
        }

        public static void encode_application_unsigned(EncodeBuffer buffer, UInt64 value)
        {
            EncodeBuffer tmp1 = new EncodeBuffer();
            encode_bacnet_unsigned(tmp1, value);
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT, false, (uint)tmp1.offset);
            buffer.Add(tmp1.buffer, tmp1.offset);
        }

        public static void encode_application_enumerated(EncodeBuffer buffer, UInt32 value)
        {
            EncodeBuffer tmp1 = new EncodeBuffer();
            encode_bacnet_enumerated(tmp1, value);
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, false, (uint)tmp1.offset);
            buffer.Add(tmp1.buffer, tmp1.offset);
        }

        public static void encode_application_signed(EncodeBuffer buffer, Int64 value)
        {
            EncodeBuffer tmp1 = new EncodeBuffer();
            encode_bacnet_signed(tmp1, value);
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT, false, (uint)tmp1.offset);
            buffer.Add(tmp1.buffer, tmp1.offset);
        }

        public static void encode_bacnet_unsigned(EncodeBuffer buffer, UInt64 value)
        {
            if (value < 0x100)
            {
                buffer.Add((byte)value);
            }
            else if (value < 0x10000)
            {
                encode_unsigned16(buffer, (UInt16)value);
            }
            else if (value < 0x1000000)
            {
                encode_unsigned24(buffer, (UInt32)value);
            }
            else if (value < 0x100000000)
            {
                encode_unsigned32(buffer, (UInt32)value);
            }
            else
            {
                encode_unsigned64(buffer, value);
            }
        }

        public static void encode_context_boolean(EncodeBuffer buffer, byte tag_number, bool boolean_value)
        {
            encode_tag(buffer, (byte)tag_number, true, 1);
            buffer.Add((boolean_value ? (byte)1 : (byte)0));
        }

        public static void encode_context_real(EncodeBuffer buffer, byte tag_number, float value)
        {
            encode_tag(buffer, tag_number, true, 4);
            encode_bacnet_real(buffer, value);
        }

        public static void encode_context_double(EncodeBuffer buffer, byte tag_number, double value)
        {
            encode_tag(buffer, tag_number, true, 8);
            encode_bacnet_double(buffer, value);
        }

        public static void encode_context_unsigned(EncodeBuffer buffer, byte tag_number, UInt32 value)
        {
            int len;

            /* length of unsigned is variable, as per 20.2.4 */
            if (value < 0x100)
                len = 1;
            else if (value < 0x10000)
                len = 2;
            else if (value < 0x1000000)
                len = 3;
            else
                len = 4;

            encode_tag(buffer, tag_number, true, (UInt32)len);
            encode_bacnet_unsigned(buffer, value);
        }

        public static void encode_context_character_string(EncodeBuffer buffer, byte tag_number, string value)
        {

            EncodeBuffer tmp = new EncodeBuffer();
            encode_bacnet_character_string(tmp, value);

            encode_tag(buffer, tag_number, true, (UInt32)tmp.offset);
            buffer.Add(tmp.buffer, tmp.offset);

        }

        public static void encode_context_enumerated(EncodeBuffer buffer, byte tag_number, UInt32 value)
        {
            int len = 0;        /* return value */

            if (value < 0x100)
                len = 1;
            else if (value < 0x10000)
                len = 2;
            else if (value < 0x1000000)
                len = 3;
            else
                len = 4;

            encode_tag(buffer, tag_number, true, (uint)len);
            encode_bacnet_enumerated(buffer, value);
        }

        public static void encode_bacnet_signed(EncodeBuffer buffer, Int64 value)
        {
            /* don't encode the leading X'FF' or X'00' of the two's compliment.
               That is, the first octet of any multi-octet encoded value shall
               not be X'00' if the most significant bit (bit 7) of the second
               octet is 0, and the first octet shall not be X'FF' if the most
               significant bit of the second octet is 1. */
            if ((value >= -128) && (value < 128))
                buffer.Add((byte)(sbyte)value);
            else if ((value >= -32768) && (value < 32768))
                encode_signed16(buffer, (Int16)value);
            else if ((value > -8388607) && (value < 8388608))
                encode_signed24(buffer, (int)value);
            else if ((value > -2147483648) && (value < 2147483648))
                encode_signed32(buffer, (int)value);
            else
                encode_signed64(buffer, value);
        }

        public static void encode_octet_string(EncodeBuffer buffer, byte[] octet_string, int octet_offset, int octet_count)
        {
            if (octet_string != null)
            {
                for (int i = octet_offset; i < (octet_offset+octet_count); i++)
                    buffer.Add(octet_string[i]);
            }
        }

        public static void encode_application_octet_string(EncodeBuffer buffer, byte[] octet_string, int octet_offset, int octet_count)
        {
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING, false, (uint)octet_count);
            encode_octet_string(buffer, octet_string, octet_offset, octet_count);
        }

        public static void encode_application_boolean(EncodeBuffer buffer, bool boolean_value)
        {
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN, false, boolean_value ? (uint)1 : (uint)0);
        }

        public static void encode_bacnet_real(EncodeBuffer buffer, float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            buffer.Add(data[3]);
            buffer.Add(data[2]);
            buffer.Add(data[1]);
            buffer.Add(data[0]);
        }

        public static void encode_bacnet_double(EncodeBuffer buffer, double value)
        {
            byte[] data = BitConverter.GetBytes(value);
            buffer.Add(data[7]);
            buffer.Add(data[6]);
            buffer.Add(data[5]);
            buffer.Add(data[4]);
            buffer.Add(data[3]);
            buffer.Add(data[2]);
            buffer.Add(data[1]);
            buffer.Add(data[0]);
        }

        public static void encode_application_real(EncodeBuffer buffer, float value)
        {
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL, false, 4);
            encode_bacnet_real(buffer, value);
        }

        public static void encode_application_double(EncodeBuffer buffer, double value)
        {
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_DOUBLE, false, 8);
            encode_bacnet_double(buffer, value);
        }

        private static byte bitstring_bytes_used(BacnetBitString bit_string)
        {
            byte len = 0;    /* return value */
            byte used_bytes = 0;
            byte last_bit = 0;

            if (bit_string.bits_used > 0)
            {
                last_bit = (byte)(bit_string.bits_used - 1);
                used_bytes = (byte)(last_bit / 8);
                /* add one for the first byte */
                used_bytes++;
                len = used_bytes;
            }

            return len;
        }


        private static byte byte_reverse_bits(byte in_byte)
        {
            byte out_byte = 0;

            if ((in_byte & 1) > 0)
            {
                out_byte |= 0x80;
            }
            if ((in_byte & 2) > 0)
            {
                out_byte |= 0x40;
            }
            if ((in_byte & 4) > 0)
            {
                out_byte |= 0x20;
            }
            if ((in_byte & 8) > 0)
            {
                out_byte |= 0x10;
            }
            if ((in_byte & 16)> 0)
            {
                out_byte |= 0x8;
            }
            if ((in_byte & 32) > 0)
            {
                out_byte |= 0x4;
            }
            if ((in_byte & 64) > 0)
            {
                out_byte |= 0x2;
            }
            if ((in_byte & 128) > 0)
            {
                out_byte |= 1;
            }

            return out_byte;
        }

        private static byte bitstring_octet(BacnetBitString bit_string, byte octet_index)
        {
            byte octet = 0;

            if (bit_string.value != null)
            {
                if (octet_index < MAX_BITSTRING_BYTES)
                {
                    octet = bit_string.value[octet_index];
                }
            }

            return octet;
        }

        public static void encode_bitstring(EncodeBuffer buffer, BacnetBitString bit_string)
        {
            byte remaining_used_bits = 0;
            byte used_bytes = 0;
            byte i = 0;

            /* if the bit string is empty, then the first octet shall be zero */
            if (bit_string.bits_used == 0)
            {
                buffer.Add(0);
            }
            else
            {
                used_bytes = bitstring_bytes_used(bit_string);
                remaining_used_bits = (byte)(bit_string.bits_used - ((used_bytes - 1) * 8));
                /* number of unused bits in the subsequent final octet */
                buffer.Add((byte)(8 - remaining_used_bits));
                for (i = 0; i < used_bytes; i++)
                    buffer.Add(byte_reverse_bits(bitstring_octet(bit_string, i)));
            }
        }

        public static void encode_application_bitstring(EncodeBuffer buffer, BacnetBitString bit_string)
        {
            uint bit_string_encoded_length = 1;     /* 1 for the bits remaining octet */

            /* bit string may use more than 1 octet for the tag, so find out how many */
            bit_string_encoded_length += bitstring_bytes_used(bit_string);
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING, false, bit_string_encoded_length);
            encode_bitstring(buffer, bit_string);
        }

        public static void bacapp_encode_application_data(EncodeBuffer buffer, BacnetValue value)
        {


            if (value.Value == null)
            {
                buffer.Add((byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL);
                return;
            }

            switch (value.Tag)
            {
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL:
                    /* don't encode anything */
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN:
                    encode_application_boolean(buffer, Convert.ToBoolean(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT:
                    encode_application_unsigned(buffer, Convert.ToUInt64(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT:
                    encode_application_signed(buffer, Convert.ToInt64(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL:
                    encode_application_real(buffer, Convert.ToSingle(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DOUBLE:
                    encode_application_double(buffer, Convert.ToDouble(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING:
                    encode_application_octet_string(buffer, (byte[])value.Value, 0, ((byte[])value.Value).Length);
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING:
                    encode_application_character_string(buffer, value.Value.ToString());
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING:
                    encode_application_bitstring(buffer, (BacnetBitString)value.Value);
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED:
                    encode_application_enumerated(buffer, Convert.ToUInt32(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE:
                    encode_application_date(buffer, Convert.ToDateTime(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME:
                    encode_application_time(buffer, Convert.ToDateTime(value.Value));
                    break;
                // Added for EventTimeStamp
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_TIMESTAMP:
                    bacapp_encode_timestamp(buffer, (BacnetGenericTime)value.Value);
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DATETIME:
                    bacapp_encode_datetime(buffer, Convert.ToDateTime(value.Value));
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID:
                    encode_application_object_id(buffer, ((BacnetObjectId)value.Value).type, ((BacnetObjectId)value.Value).instance);
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_COV_SUBSCRIPTION:
                    encode_cov_subscription(buffer, ((BacnetCOVSubscription)value.Value));       //is this the right way to do it, I wonder?
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_READ_ACCESS_RESULT:
                    encode_read_access_result(buffer, ((BacnetReadAccessResult)value.Value));       //is this the right way to do it, I wonder?
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_READ_ACCESS_SPECIFICATION:
                    encode_read_access_specification(buffer, ((BacnetReadAccessSpecification)value.Value));     //is this the right way to do it, I wonder?
                    break;
                default:
                    Type with_type = value.Value.GetType();
                    if (value.Value is Enum)
                    {
                        encode_application_enumerated(buffer, Convert.ToUInt32(value.Value));
                    }else
                    if (with_type == typeof(BACnetEventParameter))
                    {
                        BACnetEventParameter v = (BACnetEventParameter)value.Value;
                        v.ASN1encode(buffer);

                    }
                    else if (with_type == typeof(BACnetFaultParameter))
                    {
                        BACnetFaultParameter v = (BACnetFaultParameter)value.Value;
                        v.ASN1encode(buffer);

                    }
                    else if (with_type == typeof(BACnetLightingCommand))
                    {
                        BACnetLightingCommand v = (BACnetLightingCommand)value.Value;
                        v.ASN1encode(buffer);


                    }
                    else if (with_type == typeof(BACnetActionList))
                    {
                        BACnetActionList v = (BACnetActionList)value.Value;
                        v.ASN1encode(buffer);

                    }
                    else if (with_type == typeof(BACnetChannelValue))
                    {
                        BACnetChannelValue v = (BACnetChannelValue)value.Value;
                        v.ASN1encode(buffer);
                    }
                    else if (with_type == typeof(BACnetRouterEntry))
                    {
                        BACnetRouterEntry v = (BACnetRouterEntry)value.Value;
                        v.ASN1encode(buffer);
                    }
                    else if (with_type == typeof(BACnetVMACEntry))
                    {
                        BACnetVMACEntry v = (BACnetVMACEntry)value.Value;
                        v.ASN1encode(buffer);
                    }
                    else if (with_type == typeof(BACnetAddressBinding))
                    {
                        BACnetAddressBinding v = (BACnetAddressBinding)value.Value;
                        v.ASN1encode(buffer);
                    }
                    else if (with_type == typeof(BACnetFDTEntry))
                    {
                        BACnetFDTEntry v = (BACnetFDTEntry)value.Value;
                        v.ASN1encode(buffer);

                    }
                    else if (with_type == typeof(BACnetHostNPort))
                    {
                        BACnetHostNPort v = (BACnetHostNPort)value.Value;
                        v.ASN1encode(buffer);
                    }
                    else if (with_type == typeof(BACnetBDTEntry))
                    {
                        BACnetBDTEntry v = (BACnetBDTEntry)value.Value;
                        v.ASN1encode(buffer);

                    }
                    else if (with_type == typeof(BACnetShedLevel))
                    {
                        BACnetShedLevel v = (BACnetShedLevel)value.Value;
                        v.ASN1encode(buffer);
                    }
                    else if(with_type == typeof(BACnetObjectPropertyReference))
                    {
                        BACnetObjectPropertyReference v = (BACnetObjectPropertyReference)value.Value;
                        v.ASN1encode(buffer);
                    }
                    else  if (with_type == typeof(BACnetSetpointReference))
                    {
                        BACnetSetpointReference v = (BACnetSetpointReference)value.Value;
                        v.ASN1encode(buffer);

                    }else if(with_type == typeof(BACnetAccumulatorRecord))
                    {
                        BACnetAccumulatorRecord v = (BACnetAccumulatorRecord)value.Value;
                        v.ASN1encode(buffer);

                    }else

                    if (value.Value is byte[])
                    {
                        byte[] arr = (byte[])value.Value;
                        if (buffer != null) buffer.Add(arr, arr.Length);
                    }
                    else
                    {
                        try
                        {
                            Type oType = value.Value.GetType();
                            if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)))
                            {
                                // last chance to encode a List<object>
                                List<object> t=(List<object>)value.Value;
                                foreach (object o in t)
                                {
                                    IASN1encode d = (IASN1encode)o;
                                    d.ASN1encode(buffer);
                                }
                            }
                            else
                            {
                                // last chance to encode a value
                                IASN1encode d = (IASN1encode)value.Value;
                                d.ASN1encode(buffer);
                            }
                        }
                        catch { throw new Exception("I cannot encode this"); }
                    }
                    break;
            }
        }

        public static void bacapp_encode_device_obj_property_ref(EncodeBuffer buffer, BacnetDeviceObjectPropertyReference value)
        {
            encode_context_object_id(buffer, 0, value.objectIdentifier.type, value.objectIdentifier.instance);
            encode_context_enumerated(buffer, 1, (uint)value.propertyIdentifier);

            /* Array index is optional so check if needed before inserting */
            if (value.arrayIndex != ASN1.BACNET_ARRAY_ALL)
                encode_context_unsigned(buffer, 2, value.arrayIndex);

            /* Likewise, device id is optional so see if needed
             * (set type to non device to omit */
            if (value.deviceIndentifier.type == BacnetObjectTypes.OBJECT_DEVICE)
                encode_context_object_id(buffer, 3, value.deviceIndentifier.type, value.deviceIndentifier.instance);
        }

        public static void bacapp_encode_context_device_obj_property_ref(EncodeBuffer buffer, byte tag_number, BacnetDeviceObjectPropertyReference value)
        {
            encode_opening_tag(buffer, tag_number);
            bacapp_encode_device_obj_property_ref(buffer, value);
            encode_closing_tag(buffer, tag_number);
        }

        public static void bacapp_encode_property_state(EncodeBuffer buffer, BacnetPropetyState value)
        {
            switch (value.tag)
            {
                case BacnetPropetyState.BacnetPropertyStateTypes.BOOLEAN_VALUE:
                    encode_context_boolean(buffer, 0, value.state == 1 ? true : false);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.BINARY_VALUE:
                    encode_context_enumerated(buffer, 1, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.EVENT_TYPE:
                    encode_context_enumerated(buffer, 2, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.POLARITY:
                    encode_context_enumerated(buffer, 3, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.PROGRAM_CHANGE:
                    encode_context_enumerated(buffer, 4, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.PROGRAM_STATE:
                    encode_context_enumerated(buffer, 5, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.REASON_FOR_HALT:
                    encode_context_enumerated(buffer, 6, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.RELIABILITY:
                    encode_context_enumerated(buffer, 7, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.STATE:
                    encode_context_enumerated(buffer, 8, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.SYSTEM_STATUS:
                    encode_context_enumerated(buffer, 9, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.UNITS:
                    encode_context_enumerated(buffer, 10, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.UNSIGNED_VALUE:
                    encode_context_unsigned(buffer, 11, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.LIFE_SAFETY_MODE:
                    encode_context_enumerated(buffer, 12, value.state);
                    break;
                case BacnetPropetyState.BacnetPropertyStateTypes.LIFE_SAFETY_STATE:
                    encode_context_enumerated(buffer, 13, value.state);
                    break;
                default:
                    /* FIXME: assert(0); - return a negative len? */
                    break;
            }
        }

        public static void encode_context_bitstring(EncodeBuffer buffer, byte tag_number, BacnetBitString bit_string)
        {
            uint bit_string_encoded_length = 1;     /* 1 for the bits remaining octet */

            /* bit string may use more than 1 octet for the tag, so find out how many */
            bit_string_encoded_length += bitstring_bytes_used(bit_string);
            encode_tag(buffer, tag_number, true, bit_string_encoded_length);
            encode_bitstring(buffer, bit_string);
        }

        public static void encode_opening_tag(EncodeBuffer buffer, byte tag_number)
        {
            int len = 1;
            byte[] tmp = new byte[2];

            /* set class field to context specific */
            tmp[0] = 0x8;
            /* additional tag byte after this byte for extended tag byte */
            if (tag_number <= 14)
            {
                tmp[0] |= (byte)(tag_number << 4);
            }
            else
            {
                tmp[0] |= 0xF0;
                tmp[1] = tag_number;
                len++;
            }
            /* set type field to opening tag */
            tmp[0] |= 6;

            buffer.Add(tmp, len);
        }

        public static void encode_context_signed(EncodeBuffer buffer, byte tag_number, Int32 value)
        {
            int len = 0;        /* return value */

            /* length of signed int is variable, as per 20.2.11 */
            if ((value >= -128) && (value < 128))
                len = 1;
            else if ((value >= -32768) && (value < 32768))
                len = 2;
            else if ((value > -8388608) && (value < 8388608))
                len = 3;
            else
                len = 4;

            encode_tag(buffer, tag_number, true, (uint)len);
            encode_bacnet_signed(buffer, value);
        }

        public static void encode_context_object_id(EncodeBuffer buffer, byte tag_number, BacnetObjectTypes object_type, uint instance)
        {
            encode_tag(buffer, tag_number, true, 4);
            encode_bacnet_object_id(buffer, object_type, instance);
        }

        public static void encode_closing_tag(EncodeBuffer buffer,byte tag_number)
        {
            int len = 1;
            byte[] tmp = new byte[2];

            /* set class field to context specific */
            tmp[0] = 0x8;
            /* additional tag byte after this byte for extended tag byte */
            if (tag_number <= 14)
            {
                tmp[0] |= (byte)(tag_number << 4);
            }
            else
            {
                tmp[0] |= 0xF0;
                tmp[1] = tag_number;
                len++;
            }
            /* set type field to closing tag */
            tmp[0] |= 7;

            buffer.Add(tmp, len);
        }

        public static void encode_bacnet_time(EncodeBuffer buffer, DateTime value)
        {
            buffer.Add((byte)value.Hour);
            buffer.Add((byte)value.Minute);
            buffer.Add((byte)value.Second);
            buffer.Add((byte)(value.Millisecond / 10));
        }

        public static void encode_context_time(EncodeBuffer buffer, byte tag_number, DateTime value)
        {
            encode_tag(buffer, tag_number, true, 4);
            encode_bacnet_time(buffer, value);
        }

        public static void encode_bacnet_date(EncodeBuffer buffer, DateTime value)
        {
            if (value == new DateTime(1, 1, 1)) // this is the way decode do for 'Date any' = DateTime(0)
            {
                buffer.Add(0xFF); buffer.Add(0xFF); buffer.Add(0xFF); buffer.Add(0xFF);
                return;
            }

            /* allow 2 digit years */
            if (value.Year >= 1900)
                buffer.Add((byte)(value.Year - 1900));
            else if (value.Year < 0x100)
                buffer.Add((byte)value.Year);
            else
                throw new Exception("Date is rubbish");

            buffer.Add((byte)value.Month);
            buffer.Add((byte)value.Day);
            if (value.DayOfWeek==DayOfWeek.Sunday)
                buffer.Add(7);
            else
                buffer.Add((byte)value.DayOfWeek);
        }

        public static void encode_application_date(EncodeBuffer buffer, DateTime value)
        {
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE, false, 4);
            encode_bacnet_date(buffer, value);
        }

        public static void encode_application_time(EncodeBuffer buffer, DateTime value)
        {
            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME, false, 4);
            encode_bacnet_time(buffer, value);
        }

        public static void bacapp_encode_datetime(EncodeBuffer buffer, DateTime value)
        {
            if (value != new DateTime(1, 1, 1))
            {
                encode_application_date(buffer, value);
                encode_application_time(buffer, value);
            }
        }

        public static void bacapp_encode_context_datetime(EncodeBuffer buffer, byte tag_number, DateTime value)
        {
            if (value != new DateTime(1,1,1))
            {
                encode_opening_tag(buffer, tag_number);
                bacapp_encode_datetime(buffer, value);
                encode_closing_tag(buffer, tag_number);
            }
        }

        public static void bacapp_encode_timestamp(EncodeBuffer buffer, BacnetGenericTime value)
        {
            switch (value.Tag)
            {
                case BacnetTimestampTags.TIME_STAMP_TIME:
                    encode_context_time(buffer, 0, value.Time);
                    break;
                case BacnetTimestampTags.TIME_STAMP_SEQUENCE:
                    encode_context_unsigned(buffer, 1, value.Sequence);
                    break;
                case BacnetTimestampTags.TIME_STAMP_DATETIME:
                    bacapp_encode_context_datetime(buffer, 2, value.Time);
                    break;
                case BacnetTimestampTags.TIME_STAMP_NONE:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void bacapp_encode_context_timestamp(EncodeBuffer buffer, byte tag_number, BacnetGenericTime value)
        {
            if (value.Tag != BacnetTimestampTags.TIME_STAMP_NONE)
            {
                encode_opening_tag(buffer, tag_number);
                bacapp_encode_timestamp(buffer, value);
                encode_closing_tag(buffer, tag_number);
            }
        }

        public static void encode_application_character_string(EncodeBuffer buffer, string value)
        {

            EncodeBuffer tmp = new EncodeBuffer();
            encode_bacnet_character_string(tmp, value);

            encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING, false, (UInt32)tmp.offset);
            buffer.Add(tmp.buffer, tmp.offset);

        }

        public static void encode_bacnet_character_string(EncodeBuffer buffer, string value)
        {
            buffer.Add((byte)BacnetCharacterStringEncodings.CHARACTER_UTF8);
            byte[] bufUTF8 = Encoding.UTF8.GetBytes(value); // Encoding.ASCII depreciated : Addendum 135-2008k
            buffer.Add(bufUTF8, bufUTF8.Length);
         }

        public static void encode_unsigned16(EncodeBuffer buffer, UInt16 value)
        {
            buffer.Add((byte)((value & 0xff00) >> 8));
            buffer.Add((byte)((value & 0x00ff) >> 0));
        }

        public static void encode_unsigned24(EncodeBuffer buffer, UInt32 value)
        {
            buffer.Add((byte)((value & 0xff0000) >> 16));
            buffer.Add((byte)((value & 0x00ff00) >> 8));
            buffer.Add((byte)((value & 0x0000ff) >> 0));
        }

        public static void encode_unsigned32(EncodeBuffer buffer, UInt32 value)
        {
            buffer.Add((byte)((value & 0xff000000) >> 24));
            buffer.Add((byte)((value & 0x00ff0000) >> 16));
            buffer.Add((byte)((value & 0x0000ff00) >> 8));
            buffer.Add((byte)((value & 0x000000ff) >> 0));
        }

        public static void encode_unsigned64(EncodeBuffer buffer, UInt64 value)
        {
            buffer.Add((byte)(value >> 56));
            buffer.Add((byte)((value & 0xff000000000000) >> 48));
            buffer.Add((byte)((value & 0xff0000000000) >> 40));
            buffer.Add((byte)((value & 0xff00000000) >> 32));
            buffer.Add((byte)((value & 0xff000000) >> 24));
            buffer.Add((byte)((value & 0x00ff0000) >> 16));
            buffer.Add((byte)((value & 0x0000ff00) >> 8));
            buffer.Add((byte)((value & 0x000000ff) >> 0));
        }

        public static void encode_signed16(EncodeBuffer buffer, Int16 value)
        {
            buffer.Add((byte)((value & 0xff00) >> 8));
            buffer.Add((byte)((value & 0x00ff) >> 0));
        }

        public static void encode_signed24(EncodeBuffer buffer, Int32 value)
        {
            buffer.Add((byte)((value & 0xff0000) >> 16));
            buffer.Add((byte)((value & 0x00ff00) >> 8));
            buffer.Add((byte)((value & 0x0000ff) >> 0));
        }

        public static void encode_signed32(EncodeBuffer buffer, Int32 value)
        {
            buffer.Add((byte)((value & 0xff000000) >> 24));
            buffer.Add((byte)((value & 0x00ff0000) >> 16));
            buffer.Add((byte)((value & 0x0000ff00) >> 8));
            buffer.Add((byte)((value & 0x000000ff) >> 0));
        }

        public static void encode_signed64(EncodeBuffer buffer, Int64 value)
        {
            buffer.Add((byte)(value>> 56));
            buffer.Add((byte)((value & 0xff000000000000) >> 48));
            buffer.Add((byte)((value & 0xff0000000000) >> 40));
            buffer.Add((byte)((value & 0xff00000000) >> 32));

            buffer.Add((byte)((value & 0xff000000) >> 24));
            buffer.Add((byte)((value & 0x00ff0000) >> 16));
            buffer.Add((byte)((value & 0x0000ff00) >> 8));
            buffer.Add((byte)((value & 0x000000ff) >> 0));
        }

        public static void encode_read_access_specification(EncodeBuffer buffer, BacnetReadAccessSpecification value)
        {
            /* Tag 0: BACnetObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 0, value.objectIdentifier.type, value.objectIdentifier.instance);

            /* Tag 1: sequence of BACnetPropertyReference */
            ASN1.encode_opening_tag(buffer, 1);
            foreach (BacnetPropertyReference p in value.propertyReferences)
            {
                ASN1.encode_context_enumerated(buffer, 0, p.propertyIdentifier);

                /* optional array index */
                if (p.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
                    ASN1.encode_context_unsigned(buffer, 1, p.propertyArrayIndex);
            }
            ASN1.encode_closing_tag(buffer, 1);
        }

        public static void encode_read_access_result(EncodeBuffer buffer, BacnetReadAccessResult value)
        {
            /* Tag 0: BACnetObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 0, value.objectIdentifier.type, value.objectIdentifier.instance);

            /* Tag 1: listOfResults */
            ASN1.encode_opening_tag(buffer, 1);
            foreach (BacnetPropertyValue p_value in value.values)
            {
                /* Tag 2: propertyIdentifier */
                ASN1.encode_context_enumerated(buffer, 2, p_value.property.propertyIdentifier);
                /* Tag 3: optional propertyArrayIndex */
                if (p_value.property.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
                    ASN1.encode_context_unsigned(buffer, 3, p_value.property.propertyArrayIndex);
                
                if (p_value.value != null && p_value.value.Count > 0 && p_value.value[0].Value is BacnetError)
                {
                    /* Tag 5: Error */
                    ASN1.encode_opening_tag(buffer, 5);
                    ASN1.encode_application_enumerated(buffer, (uint)((BacnetError)p_value.value[0].Value).error_class);
                    ASN1.encode_application_enumerated(buffer, (uint)((BacnetError)p_value.value[0].Value).error_code);
                    ASN1.encode_closing_tag(buffer, 5);
                }
                else
                {
                    /* Tag 4: Value */
                    ASN1.encode_opening_tag(buffer, 4);
                    foreach (BacnetValue v in p_value.value)
                    {
                        ASN1.bacapp_encode_application_data(buffer, v);
                    }
                    ASN1.encode_closing_tag(buffer, 4);
                }
            }
            ASN1.encode_closing_tag(buffer, 1);
        }

        public static int decode_read_access_result(byte[] buffer, int offset, int apdu_len, out BacnetReadAccessResult value)
        {
            int len = 0;
            byte tag_number;
            uint len_value_type;
            int tag_len;

            value = new BacnetReadAccessResult();

            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return -1;
            len = 1;
            len += ASN1.decode_object_id(buffer, offset + len, out value.objectIdentifier.type, out value.objectIdentifier.instance);

            /* Tag 1: listOfResults */
            if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                return -1;
            len++;

            List<BacnetPropertyValue> _value_list = new List<BacnetPropertyValue>();
            while ((apdu_len - len) > 0)
            {
                BacnetPropertyValue new_entry = new BacnetPropertyValue();

                /* end */
                if (ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                {
                    len++;
                    break;
                }

                /* Tag 2: propertyIdentifier */
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                if (tag_number != 2)
                    return -1;
                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out new_entry.property.propertyIdentifier);
                /* Tag 3: Optional Array Index */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                if (tag_number == 3)
                {
                    len += tag_len;
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out new_entry.property.propertyArrayIndex);
                }
                else
                    new_entry.property.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;

                /* Tag 4: Value */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number == 4)
                {
                    BacnetValue v;
                    List<BacnetValue> local_value_list = new List<BacnetValue>();
                    while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 4))
                    {
                        tag_len = ASN1.bacapp_decode_application_data(buffer, offset + len, apdu_len + offset - 1, value.objectIdentifier.type, (BacnetPropertyIds)new_entry.property.propertyIdentifier, out v);
                        if (tag_len < 0) return -1;
                        len += tag_len;
                        local_value_list.Add(v);
                    }
                    // FC : two values one Date & one Time => change to one datetime
                    if ((local_value_list.Count == 2) && (local_value_list[0].Tag == BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE) && (local_value_list[1].Tag == BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME))
                    {
                        DateTime date, time;
                        date = (DateTime)local_value_list[0].Value;
                        time = (DateTime)local_value_list[1].Value;
                        DateTime bdatetime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
                        local_value_list.Clear();
                        local_value_list.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_DATETIME,bdatetime));
                        new_entry.value = local_value_list;
                    }
                    else
                        new_entry.value = local_value_list;
                    len++;
                }
                else if (tag_number == 5)
                {
                    /* Tag 5: Error */
                    BacnetError err = new BacnetError();
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                    len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out len_value_type);      //error_class
                    err.error_class = (BacnetErrorClasses)len_value_type;
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                    len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out len_value_type);       //error_code
                    err.error_code = (BacnetErrorCodes)len_value_type;
                    if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 5))
                        return -1;
                    len++;

                    new_entry.value = new BacnetValue[] { new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ERROR, err) };
                }

                _value_list.Add(new_entry);

            }
            value.values = _value_list;

            return len;
        }

        public static int decode_read_access_specification(byte[] buffer, int offset, int apdu_len, out BacnetReadAccessSpecification value)
        {
            int len = 0;
            byte tag_number;
            uint len_value_type;
            int tmp;

            value = new BacnetReadAccessSpecification();

            /* Tag 0: Object ID */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return -1;
            len++;
            len += ASN1.decode_object_id(buffer, offset + len, out value.objectIdentifier.type, out value.objectIdentifier.instance);

            /* Tag 1: sequence of ReadAccessSpecification */
            if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                return -1;
            len++;  /* opening tag is only one octet */

            /* properties */
            List<BacnetPropertyReference> __property_id_and_array_index = new List<BacnetPropertyReference>();
            while ((apdu_len - len) > 1 && !ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
            {
                BacnetPropertyReference p_ref = new BacnetPropertyReference();

                /* Tag 0: propertyIdentifier */
                if (!ASN1.IS_CONTEXT_SPECIFIC(buffer[offset + len]))
                    return -1;

                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                if (tag_number != 0)
                    return -1;

                /* Should be at least the unsigned value + 1 tag left */
                if ((len + len_value_type) >= apdu_len)
                    return -1;
                len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out p_ref.propertyIdentifier);
                /* Assume most probable outcome */
                p_ref.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;
                /* Tag 1: Optional propertyArrayIndex */
                if (ASN1.IS_CONTEXT_SPECIFIC(buffer[offset + len]) && !ASN1.IS_CLOSING_TAG(buffer[offset + len]))
                {
                    tmp = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                    if (tag_number == 1)
                    {
                        len += tmp;
                        /* Should be at least the unsigned array index + 1 tag left */
                        if ((len + len_value_type) >= apdu_len)
                            return -1;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out p_ref.propertyArrayIndex);
                    }
                }
                __property_id_and_array_index.Add(p_ref);
            }

            /* closing tag */
            if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                return -1;
            len++;

            value.propertyReferences = __property_id_and_array_index;
            return len;
        }

        // FC
        public static int decode_device_obj_property_ref(byte[] buffer, int offset, int apdu_len,  out BacnetDeviceObjectPropertyReference value)
        {
            int len = 0;
            byte tag_number;
            uint len_value_type;
            int tag_len;

            value = new BacnetDeviceObjectPropertyReference();
            value.arrayIndex = ASN1.BACNET_ARRAY_ALL;

            /* Tag 0: Object ID */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return -1;
            len++;
            len += ASN1.decode_object_id(buffer, offset + len, out value.objectIdentifier.type, out value.objectIdentifier.instance);


            /* Tag 1 : Property identifier */
            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -1;
            uint propertyIdentifier;
            len += decode_enumerated(buffer, offset + len, len_value_type, out propertyIdentifier);
            value.propertyIdentifier = (BacnetPropertyIds)propertyIdentifier;

            /* Tag 2: Optional Array Index */
            tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number == 2)
            {
                len += tag_len;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out value.arrayIndex);
            }

            /* Tag 3 : Optional Device Identifier */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 3))
                return len;
            if (IS_CLOSING_TAG(buffer[offset+len])) return len;

            len++;

            len += ASN1.decode_object_id(buffer, offset + len, out value.deviceIndentifier.type, out value.deviceIndentifier.instance);

            return len;
        }

        public static int decode_unsigned(byte[] buffer, int offset, uint len_value, out ulong value)
        {
            uint unsigned_value = 0;

            switch (len_value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    decode_unsigned(buffer, offset, len_value, out unsigned_value);
                    value=unsigned_value;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    decode_unsigned64(buffer, offset, out value, len_value);
                    break;
                default:
                    value = 0;
                    break;
            }

            return (int)len_value;
        }

        public static int decode_unsigned(byte[] buffer, int offset, uint len_value, out uint value)
        {
            ushort unsigned16_value = 0;

            switch (len_value)
            {
                case 1:
                    value = buffer[offset];
                    break;
                case 2:
                    decode_unsigned16(buffer, offset, out unsigned16_value);
                    value = unsigned16_value;
                    break;
                case 3:
                    decode_unsigned24(buffer, offset, out value);
                    break;
                case 4:
                    decode_unsigned32(buffer, offset, out value);
                    break;
                default:
                    value = 0;
                    break;
            }

            return (int)len_value;
        }

        public static int decode_unsigned64(byte[] buffer, int offset, out ulong value, uint len=8)
        {
            value = 0;
            int dec = 8 * ((int)len - 1);
            for (int i = 0; i < len; i++)
            {
                value |=((ulong)(((ulong)buffer[offset + i]) << dec));
                dec = dec - 8;
            }
            return (int)len;
        }

        public static int decode_unsigned32(byte[] buffer, int offset, out uint value)
        {
            value = ((uint)((((uint)buffer[offset+0]) << 24) & 0xff000000));
            value |= ((uint)((((uint)buffer[offset + 1]) << 16) & 0x00ff0000));
            value |= ((uint)((((uint)buffer[offset + 2]) << 8) & 0x0000ff00));
            value |= ((uint)(((uint)buffer[offset + 3]) & 0x000000ff));
            return 4;
        }

        public static int decode_unsigned24(byte[] buffer, int offset, out uint value)
        {
            value = ((uint)((((uint)buffer[offset + 0]) << 16) & 0x00ff0000));
            value |= ((uint)((((uint)buffer[offset + 1]) << 8) & 0x0000ff00));
            value |= ((uint)(((uint)buffer[offset + 2]) & 0x000000ff));
            return 3;
        }

        public static int decode_unsigned16(byte[] buffer, int offset, out ushort value)
        {
            value = ((ushort)((((uint)buffer[offset + 0]) << 8) & 0x0000ff00));
            value |= ((ushort)(((uint)buffer[offset + 1]) & 0x000000ff));
            return 2;
        }

        public static int decode_unsigned8(byte[] buffer, int offset, out byte value)
        {
            value = buffer[offset + 0];
            return 1;
        }

        // variable lenght decoding of signed int 64
        public static int decode_signed64(byte[] buffer, int offset, out long value, uint len=8)
        {

            value = 0;

            int dec=56;

            if ((buffer[offset + 0] & 0x80) != 0) /* negative - bit 7 is set */
                for (int i = (int)len; i < 8; i++)
                {
                    value |= ((long)((long)0xFF << dec));
                    dec = dec - 8;
                }

            dec = 8*((int)len-1);
            for (int i = 0; i < len; i++)
            {
                value |= ((long)(((long)buffer[offset + i]) << dec));
                dec = dec - 8;
            }
            return (int)len;
        }

        public static int decode_signed32(byte[] buffer, int offset, out int value)
        {
            value = ((int)((((int)buffer[offset + 0]) << 24) & 0xff000000));
            value |= ((int)((((int)buffer[offset + 1]) << 16) & 0x00ff0000));
            value |= ((int)((((int)buffer[offset + 2]) << 8) & 0x0000ff00));
            value |= ((int)(((int)buffer[offset + 3]) & 0x000000ff));
            return 4;
        }

        public static int decode_signed24(byte[] buffer, int offset, out int value)
        {
            /* negative - bit 7 is set */
            if ((buffer[offset + 0] & 0x80) != 0)
                value = ~0x00FFFFFF;
            else
                value = 0;
            value |= buffer[offset + 0] << 16;
            value |= buffer[offset + 1] << 8;
            value |= buffer[offset + 2];
            return 3;
        }

        public static int decode_signed16(byte[] buffer, int offset, out short value)
        {
            value = ((short)((((int)buffer[offset + 0]) << 8) & 0x0000ff00));
            value |= ((short)(((int)buffer[offset + 1]) & 0x000000ff));
            return 2;
        }

        public static int decode_signed8(byte[] buffer, int offset, out sbyte value)
        {
            value = (sbyte)buffer[offset + 0];
            return 1;
        }

        public static bool IS_EXTENDED_TAG_NUMBER(byte x)
        {
            return ((x & 0xF0) == 0xF0);
        }

        public static bool IS_EXTENDED_VALUE(byte x)
        {
            return ((x & 0x07) == 5);
        }

        public static bool IS_CONTEXT_SPECIFIC(byte x)
        {
            return ((x & 0x8) == 0x8);
        }

        public static bool IS_OPENING_TAG(byte x)
        {
            return ((x & 0x07) == 6);
        }

        public static bool IS_CLOSING_TAG(byte x)
        {
            return ((x & 0x07) == 7);
        }

        public static int decode_tag_number(byte[] buffer, int offset, out byte tag_number)
        {
            int len = 1;        /* return value */

            /* decode the tag number first */
            if (IS_EXTENDED_TAG_NUMBER(buffer[offset]))
            {
                /* extended tag */
                tag_number = buffer[offset+1];
                len++;
            }
            else
            {
                tag_number = (byte)(buffer[offset] >> 4);
            }

            return len;
        }

        public static int decode_signed(byte[] buffer, int offset, uint len_value, out long value)
        {
            switch (len_value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    int int_value;;
                    decode_signed(buffer, offset, len_value, out int_value);
                    value=int_value;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    decode_signed64(buffer, offset, out value, len_value);
                    break;
                default:
                    value = 0;
                    break;
            }

            return (int)len_value;
        }

        public static int decode_signed(byte[] buffer, int offset, uint len_value, out int value)
        {
                switch (len_value)
                {
                    case 1:
                        sbyte sbyte_value;
                        decode_signed8(buffer, offset, out sbyte_value);
                        value = sbyte_value;
                        break;
                    case 2:
                        short short_value;
                        decode_signed16(buffer, offset, out short_value);
                        value = short_value;
                        break;
                    case 3:
                        decode_signed24(buffer, offset, out value);
                        break;
                    case 4:
                        decode_signed32(buffer, offset, out value);
                        break;
                    default:
                        value = 0;
                        break;
                }

            return (int)len_value;
        }

        public static int decode_real(byte[] buffer, int offset, out float value)
        {
            byte[] tmp = new byte[] { buffer[offset + 3], buffer[offset + 2], buffer[offset + 1], buffer[offset + 0] };
            value = BitConverter.ToSingle(tmp, 0);
            return 4;
        }

        public static int decode_real_safe(byte[] buffer, int offset, uint len_value, out float value)
        {
            if (len_value != 4)
            {
                value = 0.0f;
                return (int)len_value;
            }
            else
            {
                return decode_real(buffer, offset, out value);
            }
        }

        public static int decode_double(byte[] buffer, int offset, out double value)
        {
            byte[] tmp = new byte[] { buffer[offset + 7], buffer[offset + 6], buffer[offset + 5], buffer[offset + 4], buffer[offset + 3], buffer[offset + 2], buffer[offset + 1], buffer[offset + 0] };
            value = BitConverter.ToDouble(tmp, 0);
            return 8;
        }

        public static int decode_double_safe(byte[] buffer, int offset, uint len_value, out double value)
        {
            if (len_value != 8)
            {
                value = 0.0f;
                return (int)len_value;
            }
            else
            {
                return decode_double(buffer, offset, out value);
            }
        }

        private static bool octetstring_copy(byte[] buffer, int offset, int max_offset, byte[] octet_string, int octet_string_offset, uint octet_string_length)
        {
            bool status = false;        /* return value */

            if (octet_string_length <= (max_offset + offset))
            {
                if (octet_string != null) Array.Copy(buffer, offset, octet_string, octet_string_offset, Math.Min(octet_string.Length, buffer.Length - offset));
                status = true;
            }

            return status;
        }

        public static int decode_octet_string(byte[] buffer, int offset, int max_length, byte[] octet_string, int octet_string_offset, uint octet_string_length)
        {
            int len = 0;        /* return value */

            octetstring_copy(buffer, offset, max_length, octet_string, octet_string_offset, octet_string_length);
            len = (int)octet_string_length;

            return len;
        }

        public static int decode_context_octet_string(byte[] buffer, int offset, int max_length, byte tag_number, byte[] octet_string, int octet_string_offset)
        {
            int len = 0;        /* return value */
            uint len_value = 0;

            octet_string = null;
            if (decode_is_context_tag(buffer, offset, tag_number))
            {
                len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);

                if (octetstring_copy(buffer, offset + len, max_length, octet_string, octet_string_offset, len_value))
                {
                    len += (int)len_value;
                }
            }
            else
                len = -1;

            return len;
        }

        private static bool multi_charset_characterstring_decode(byte[] buffer, int offset, int max_length, byte encoding, uint length, out string char_string)
        {
            char_string = "";
            try
            {
                Encoding e;

                switch ((BacnetCharacterStringEncodings)encoding)
                {
                    // 'normal' encoding, backward compatible ANSI_X34 (for decoding only)
                    case BacnetCharacterStringEncodings.CHARACTER_UTF8:
                        e = Encoding.UTF8;
                        break;

                    // UCS2 is backward compatible UTF16 (for decoding only)
                    // http://hackipedia.org/Character%20sets/Unicode,%20UTF%20and%20UCS%20encodings/UCS-2.htm
                    // https://en.wikipedia.org/wiki/Byte_order_mark
                    case BacnetCharacterStringEncodings.CHARACTER_UCS2:
                        if ((buffer[offset] == 0xFF) && (buffer[offset + 1] == 0xFE)) // Byte Order Mark
                            e = Encoding.Unicode; // little endian encoding
                        else
                            e = Encoding.BigEndianUnicode; // big endian encoding if BOM is not set, or 0xFE-0xFF
                        break;

                    // eq. UTF32. In usage somewhere for transmission ? A bad idea !
                    case BacnetCharacterStringEncodings.CHARACTER_UCS4:
                        if ((buffer[offset] == 0xFF) && (buffer[offset + 1] == 0xFE) && (buffer[offset + 2] == 0) && (buffer[offset + 3] == 0))
                            e = Encoding.UTF32; // UTF32 little endian encoding
                        else
                            e = Encoding.GetEncoding(12001); // UTF32 big endian encoding if BOM is not set, or 0-0-0xFE-0xFF
                        break;

                    case BacnetCharacterStringEncodings.CHARACTER_ISO8859:
                        e = Encoding.GetEncoding(28591); // "iso-8859-1"
                        break;

                    // FIXME: somebody in Japan (or elsewhere) could help,test&validate if such devices exist ?
                    // http://cgproducts.johnsoncontrols.com/met_pdf/1201531.pdf?ref=binfind.com/web page 18
                    case BacnetCharacterStringEncodings.CHARACTER_MS_DBCS:
                        e = Encoding.GetEncoding("shift_jis");
                        break;

                    // FIXME: somebody in Japan (or elsewhere) could help,test&validate if such devices exist ?
                    // http://www.sljfaq.org/afaq/encodings.html
                    case BacnetCharacterStringEncodings.CHARACTER_JISX_0208:
                        e = Encoding.GetEncoding("shift_jis"); // maybe "iso-2022-jp" ?
                        break;

                    // unknown code (wrong code, experimental, ...)
                    // decoded as ISO-8859-1 (removing controls) : displays certainly a strange content !
                    default:
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < length; i++)
                        {
                            char oneChar = (char)buffer[offset + i]; // byte to char on .NET : ISO-8859-1
                            if (char.IsSymbol(oneChar)) sb.Append(oneChar);
                        }
                        char_string = sb.ToString();
                        return true;
                }

                char_string = e.GetString(buffer, offset, (int)length);
            }
            catch
            {
                char_string = "string decoding error !";
            }

            return true; // always OK
        }

        public static int decode_character_string(byte[] buffer, int offset, int max_length, uint len_value, out string char_string)
        {
            int len = 0;        /* return value */
            bool status = false;

            status = multi_charset_characterstring_decode(buffer, offset + 1, max_length, buffer[offset], len_value - 1, out char_string);
            if (status)
            {
                len = (int)len_value;
            }

            return len;
        }

        private static bool bitstring_set_octet(ref BacnetBitString bit_string, byte index, byte octet)
        {
            bool status = false;

            if (index < MAX_BITSTRING_BYTES)
            {
                bit_string.value[index] = octet;
                status = true;
            }

            return status;
        }

        private static bool bitstring_set_bits_used(ref BacnetBitString bit_string, byte bytes_used, byte unused_bits)
        {
            bool status = false;

            /* FIXME: check that bytes_used is at least one? */
            bit_string.bits_used = (byte)(bytes_used * 8);
            bit_string.bits_used -= unused_bits;
            status = true;

            return status;
        }

        public static int decode_bitstring(byte[] buffer, int offset, uint len_value, out BacnetBitString bit_string)
        {
            int len = 0;
            byte unused_bits = 0;
            uint i = 0;
            uint bytes_used = 0;

            bit_string = new BacnetBitString();
            bit_string.value = new byte[MAX_BITSTRING_BYTES];
            if (len_value > 0)
            {
                /* the first octet contains the unused bits */
                bytes_used = len_value - 1;
                if (bytes_used <= MAX_BITSTRING_BYTES)
                {
                    len = 1;
                    for (i = 0; i < bytes_used; i++)
                    {
                        bitstring_set_octet(ref bit_string, (byte)i, byte_reverse_bits(buffer[offset+len++]));
                    }
                    unused_bits = (byte)(buffer[offset] & 0x07);
                    bitstring_set_bits_used(ref bit_string, (byte)bytes_used, unused_bits);
                }
            }

            return len;
        }

        public static int decode_context_character_string(byte[] buffer, int offset, int max_length, byte tag_number, out string char_string)
        {
            int len = 0;        /* return value */
            bool status = false;
            uint len_value = 0;

            char_string = null;
            if (decode_is_context_tag(buffer, offset + len, tag_number))
            {
                len +=
                    decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);

                status =
                    multi_charset_characterstring_decode(buffer, offset + 1 + len, max_length, buffer[offset + len], len_value - 1, out char_string);
                if (status)
                {
                    len += (int)len_value;
                }
            }
            else
                len = -1;

            return len;
        }

        public static int decode_date(byte[] buffer, int offset, out DateTime bdate)
        {
            int year = (ushort)(buffer[offset] + 1900);
            int month = buffer[offset + 1];
            int day = buffer[offset + 2];
            int wday = buffer[offset + 3];

            if(month == 0xFF && day == 0xFF && wday == 0xFF && (year-1900) == 0xFF)
                bdate = new DateTime(1, 1, 1);
            else
                bdate = new DateTime(year, month, day);

            return 4;
        }

        public static int decode_date_safe(byte[] buffer, int offset, uint len_value, out DateTime bdate)
        {
            if (len_value != 4)
            {
                bdate = new DateTime(1, 1, 1);
                return (int)len_value;
            }
            else
            {
                return decode_date(buffer, offset, out bdate);
            }
        }

        public static int decode_bacnet_time(byte[] buffer, int offset, out DateTime btime)
        {
            int hour = buffer[offset + 0];
            int min = buffer[offset + 1];
            int sec = buffer[offset + 2];
            int hundredths = buffer[offset + 3];
            if (hour > 24) hour = 0;
            if (min > 60) min = 0;
            if (sec > 60) sec = 0;
            if (hundredths > 99) hundredths = 0;   // sometimes set to 255
            btime = new DateTime(1, 1, 1, hour, min, sec, hundredths * 10);
            return 4;
        }

        public static int decode_bacnet_time_safe(byte[] buffer, int offset, uint len_value, out DateTime btime)
        {
            if (len_value != 4)
            {
                btime = new DateTime(1, 1, 1);
                return (int)len_value;
            }
            else
            {
                return decode_bacnet_time(buffer, offset, out btime);
            }
        }

        public static int decode_bacnet_datetime(byte[] buffer, int offset, out DateTime bdatetime)
        {
            int len = 0;
            DateTime date;
            len += ASN1.decode_application_date(buffer, offset + len, out date); // Date
            DateTime time;
            len += ASN1.decode_application_time(buffer, offset + len, out time); // Time
            bdatetime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
            return len;
        }

        public static int decode_object_id(byte[] buffer, int offset, out ushort object_type, out uint instance)
        {
            uint value = 0;
            int len = 0;

            len = decode_unsigned32(buffer, offset, out value);
            object_type =
                (ushort)(((value >> BACNET_INSTANCE_BITS) & BACNET_MAX_OBJECT));
            instance = (value & BACNET_MAX_INSTANCE);

            return len;
        }

        public static int decode_object_id_safe(byte[] buffer, int offset, uint len_value, out ushort object_type, out uint instance)
        {
            if (len_value != 4)
            {
                object_type = 0;
                instance = 0;
                return 0;
            }
            else
            {
                return decode_object_id(buffer, offset, out object_type, out instance);
            }
        }

        public static int decode_context_object_id(byte[] buffer, int offset, byte tag_number, out ushort object_type, out uint instance)
        {
            int len = 0;

            if (decode_is_context_tag_with_length(buffer, offset + len, tag_number, out len))
            {
                len += decode_object_id(buffer, offset + len, out object_type, out instance);
            }
            else
            {
                object_type = 0;
                instance = 0;
                len = -1;
            }
            return len;
        }

        public static int decode_application_time(byte[] buffer, int offset, out DateTime btime)
        {
            int len = 0;
            byte tag_number;
            decode_tag_number(buffer, offset + len, out tag_number);

            if (tag_number == (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME)
            {
                len++;
                len += decode_bacnet_time(buffer, offset + len, out btime);
            }
            else
            {
                btime = new DateTime(1, 1, 1);
                len = -1;
            }
            return len;
        }


        public static int decode_context_bacnet_time(byte[] buffer, int offset, byte tag_number, out DateTime btime)
        {
            int len = 0;

            if (decode_is_context_tag_with_length(buffer, offset + len, tag_number, out len))
            {
                len += decode_bacnet_time(buffer, offset + len, out btime);
            }
            else
            {
                btime = new DateTime(1, 1, 1);
                len = -1;
            }
            return len;
        }

        public static int decode_application_date(byte[] buffer, int offset, out DateTime bdate)
        {
            int len = 0;
            byte tag_number;
            decode_tag_number(buffer, offset + len, out tag_number);

            if (tag_number == (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE)
            {
                len++;
                len += decode_date(buffer, offset + len, out bdate);
            }
            else
            {
                bdate = new DateTime(1, 1, 1);
                len = -1;
            }
            return len;
        }

        public static bool decode_is_context_tag_with_length(byte[] buffer, int offset, byte tag_number, out int tag_length)
        {
            byte my_tag_number = 0;

            tag_length = decode_tag_number(buffer, offset, out my_tag_number);

            return (bool)(IS_CONTEXT_SPECIFIC(buffer[offset]) &&
                (my_tag_number == tag_number));
        }

        public static int decode_context_date(byte[] buffer, int offset, byte tag_number, out DateTime bdate)
        {
            int len = 0;

            if (decode_is_context_tag_with_length(buffer, offset + len, tag_number, out len))
            {
                len += decode_date(buffer, offset + len, out bdate);
            }
            else
            {
                bdate = new DateTime(1, 1, 1);
                len = -1;
            }
            return len;
        }

        public static int bacapp_decode_data(byte[] buffer, int offset, int max_length, BacnetApplicationTags tag_data_type, uint len_value_type, out BacnetValue value)
        {
            int len = 0;
            uint uint_value;

            ulong ulong_value;
            long long_value;

            value = new BacnetValue();
            value.Tag = tag_data_type;

            switch (tag_data_type)
            {
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL:
                    /* nothing else to do */
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN:
                    value.Value = len_value_type > 0 ? true : false;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT:
                    len = decode_unsigned(buffer, offset, len_value_type, out ulong_value);
                    value.Value = ulong_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT:
                    len = decode_signed(buffer, offset, len_value_type, out long_value);
                    value.Value = long_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL:
                    float float_value;
                    len = decode_real_safe(buffer, offset, len_value_type, out float_value);
                    value.Value = float_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DOUBLE:
                    double double_value;
                    len = decode_double_safe(buffer, offset, len_value_type, out double_value);
                    value.Value = double_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING:
                    byte[] octet_string = new byte[len_value_type];
                    len = decode_octet_string(buffer, offset, max_length, octet_string, 0, len_value_type);
                    value.Value = octet_string;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING:
                    string string_value;
                    len = decode_character_string(buffer, offset, max_length, len_value_type, out string_value);
                    value.Value = string_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING:
                    BacnetBitString bit_value;
                    len = decode_bitstring(buffer, offset, len_value_type, out bit_value);
                    value.Value = bit_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED:
                    len = decode_enumerated(buffer, offset, len_value_type, out uint_value);
                    value.Value = uint_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE:
                    DateTime date_value;
                    len = decode_date_safe(buffer, offset, len_value_type, out date_value);
                    value.Value = date_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME:
                    DateTime time_value;
                    len = decode_bacnet_time_safe(buffer, offset, len_value_type, out time_value);
                    value.Value = time_value;
                    break;
                case BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID:
                    {
                        ushort object_type = 0;
                        uint instance = 0;
                        len = decode_object_id_safe(buffer, offset, len_value_type, out object_type, out instance);
                        value.Value = new BacnetObjectId((BacnetObjectTypes)object_type, instance);
                    }
                    break;
                default:
                    break;
            }

            return len;
        }

        /* returns the fixed tag type for certain context tagged properties */
        private static BacnetApplicationTags bacapp_context_tag_type(BacnetPropertyIds property, byte tag_number)
        {
            BacnetApplicationTags tag = BacnetApplicationTags.MAX_BACNET_APPLICATION_TAG;

            switch (property)
            {
                case BacnetPropertyIds.PROP_ACTUAL_SHED_LEVEL:
                case BacnetPropertyIds.PROP_REQUESTED_SHED_LEVEL:
                case BacnetPropertyIds.PROP_EXPECTED_SHED_LEVEL:
                    switch (tag_number)
                    {
                        case 0:
                        case 1:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                            break;
                        case 2:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL;
                            break;
                        default:
                            break;
                    }
                    break;
                case BacnetPropertyIds.PROP_ACTION:
                    switch (tag_number)
                    {
                        case 0:
                        case 1:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID;
                            break;
                        case 2:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED;
                            break;
                        case 3:
                        case 5:
                        case 6:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                            break;
                        case 7:
                        case 8:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN;
                            break;
                        case 4:        /* propertyValue: abstract syntax */
                        default:
                            break;
                    }
                    break;
                case BacnetPropertyIds.PROP_LIST_OF_GROUP_MEMBERS:
                    /* Sequence of ReadAccessSpecification */
                    switch (tag_number)
                    {
                        case 0:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID;
                            break;
                        default:
                            break;
                    }
                    break;
                case BacnetPropertyIds.PROP_EXCEPTION_SCHEDULE:
                    switch (tag_number)
                    {
                        case 1:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID;
                            break;
                        case 3:
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                            break;
                        case 0:        /* calendarEntry: abstract syntax + context */
                        case 2:        /* list of BACnetTimeValue: abstract syntax */
                        default:
                            break;
                    }
                    break;
                case BacnetPropertyIds.PROP_LOG_DEVICE_OBJECT_PROPERTY:
                    switch (tag_number)
                    {
                        case 0:        /* Object ID */
                        case 3:        /* Device ID */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID;
                            break;
                        case 1:        /* Property ID */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED;
                            break;
                        case 2:        /* Array index */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                            break;
                        default:
                            break;
                    }
                    break;
                case BacnetPropertyIds.PROP_SUBORDINATE_LIST:
                    /* BACnetARRAY[N] of BACnetDeviceObjectReference */
                    switch (tag_number)
                    {
                        case 0:        /* Optional Device ID */
                        case 1:        /* Object ID */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID;
                            break;
                        default:
                            break;
                    }
                    break;

                case BacnetPropertyIds.PROP_RECIPIENT_LIST:
                    /* List of BACnetDestination */
                    switch (tag_number)
                    {
                        case 0:        /* Device Object ID */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID;
                            break;
                        default:
                            break;
                    }
                    break;
                case BacnetPropertyIds.PROP_ACTIVE_COV_SUBSCRIPTIONS:
                    /* BACnetCOVSubscription */
                    switch (tag_number)
                    {
                        case 0:        /* BACnetRecipientProcess */
                        case 1:        /* BACnetObjectPropertyReference */
                            break;
                        case 2:        /* issueConfirmedNotifications */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN;
                            break;
                        case 3:        /* timeRemaining */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                            break;
                        case 4:        /* covIncrement */
                            tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return tag;
        }

        public static int bacapp_decode_context_data(byte[] buffer, int offset, uint max_apdu_len, BacnetApplicationTags property_tag, out BacnetValue value)
        {
            int apdu_len = 0, len = 0;
            int tag_len = 0;
            byte tag_number = 0;
            uint len_value_type = 0;

            value = new BacnetValue();

            if (IS_CONTEXT_SPECIFIC(buffer[offset]))
            {
                //value->context_specific = true;
                tag_len = decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                apdu_len = tag_len;
                /* Empty construct : (closing tag) => returns NULL value */
                if (tag_len > 0 && (tag_len <= max_apdu_len) && !decode_is_closing_tag_number(buffer, offset + len, tag_number))
                {
                    //value->context_tag = tag_number;
                    if (property_tag < BacnetApplicationTags.MAX_BACNET_APPLICATION_TAG)
                    {
                        len = bacapp_decode_data(buffer, offset + apdu_len, (int)max_apdu_len, property_tag, len_value_type, out value);
                        apdu_len += len;
                    }
                    else if (len_value_type > 0)
                    {
                        /* Unknown value : non null size (elementary type) */
                        apdu_len += (int)len_value_type;
                        /* SHOULD NOT HAPPEN, EXCEPTED WHEN READING UNKNOWN CONTEXTUAL PROPERTY */
                    }
                    else
                        apdu_len = -1;
                }
                else if (tag_len == 1)        /* and is a Closing tag */
                    apdu_len = 0;       /* Don't advance over that closing tag. */
            }

            return apdu_len;
        }

        public static int bacapp_decode_application_data(byte[] buffer, int offset, int max_offset, BacnetObjectTypes object_type, BacnetPropertyIds property_id, out BacnetValue value)
        {
            int len = 0;
            int tag_len = 0;
            int decode_len = 0;
            byte tag_number = 0;
            uint len_value_type = 0;

            value = new BacnetValue();
            /*C. Günther*/
            //put before because has otherwise would decode as not context specific!
            if (property_id == BacnetPropertyIds.PROP_REQUESTED_SHED_LEVEL || property_id == BacnetPropertyIds.PROP_EXPECTED_SHED_LEVEL || property_id == BacnetPropertyIds.PROP_ACTUAL_SHED_LEVEL)
            {
                BACnetShedLevel v = new BACnetShedLevel();
                tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                if (tag_len < 0) return -1;
                value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                value.Value = v;

                return tag_len;
            }
            if (property_id == BacnetPropertyIds.PROP_LAST_KEY_SERVER)
            {
                BACnetAddressBinding v = new BACnetAddressBinding();
                tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                if (tag_len < 0) return -1;
                value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                value.Value = v;

                return tag_len;
            }

            /* FIXME: use max_apdu_len! */
            if (!IS_CONTEXT_SPECIFIC(buffer[offset]))
            {
                tag_len = decode_tag_number_and_value(buffer, offset, out tag_number, out len_value_type);
                if (tag_len > 0)
                {
                    len += tag_len;

                    decode_len = bacapp_decode_data(buffer, offset + len, max_offset, (BacnetApplicationTags)tag_number, len_value_type, out value);
                    if (decode_len < 0) return decode_len;
                    len += decode_len;
                }
            }
            else
            {
                return bacapp_decode_context_application_data(buffer, offset, max_offset, object_type, property_id, out value);
            }

            return len;
        }

        public static int bacapp_decode_context_application_data(byte[] buffer, int offset, int max_offset, BacnetObjectTypes object_type, BacnetPropertyIds property_id, out BacnetValue value)
        {
            int len = 0;
            int tag_len = 0;
            byte tag_number = 0;
            byte sub_tag_number = 0;
            uint len_value_type = 0;

            value = new BacnetValue();

            if (IS_CONTEXT_SPECIFIC(buffer[offset]))
            {
                //this seems to be a strange way to determine object encodings
                if (property_id == BacnetPropertyIds.PROP_LIST_OF_GROUP_MEMBERS)
                {
                    BacnetReadAccessSpecification v;
                    tag_len = ASN1.decode_read_access_specification(buffer, offset, max_offset, out v);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_READ_ACCESS_SPECIFICATION;
                    value.Value = v;
                    return tag_len;
                }
                else if (property_id == BacnetPropertyIds.PROP_ACTIVE_COV_SUBSCRIPTIONS)
                {
                    BacnetCOVSubscription v;
                    tag_len = ASN1.decode_cov_subscription(buffer, offset, max_offset, out v);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_COV_SUBSCRIPTION;
                    value.Value = v;
                    return tag_len;
                }
                else if (object_type == BacnetObjectTypes.OBJECT_GROUP && property_id == BacnetPropertyIds.PROP_PRESENT_VALUE)
                {
                    BacnetReadAccessResult v;
                    tag_len = ASN1.decode_read_access_result(buffer, offset, max_offset, out v);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_READ_ACCESS_RESULT;
                    value.Value = v;
                    return tag_len;
                }
                else if ( (property_id == BacnetPropertyIds.PROP_LIST_OF_OBJECT_PROPERTY_REFERENCES) || (property_id == BacnetPropertyIds.PROP_LOG_DEVICE_OBJECT_PROPERTY) || (property_id == BacnetPropertyIds.PROP_OBJECT_PROPERTY_REFERENCE))
                {
                    BacnetDeviceObjectPropertyReference v;
                    tag_len = ASN1.decode_device_obj_property_ref(buffer, offset, max_offset, out v);

                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_PROPERTY_REFERENCE;
                    value.Value = v;
                    return tag_len;
                }
                else if (property_id == BacnetPropertyIds.PROP_SETPOINT_REFERENCE)
                {
                    BACnetSetpointReference v = new BACnetSetpointReference();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }
                else if (property_id == BacnetPropertyIds.PROP_EVENT_ALGORITHM_INHIBIT_REF || property_id == BacnetPropertyIds.PROP_INPUT_REFERENCE || property_id == BacnetPropertyIds.PROP_CONTROLLED_VARIABLE_REFERENCE || property_id == BacnetPropertyIds.PROP_MANIPULATED_VARIABLE_REFERENCE)
                {
                    BACnetObjectPropertyReference v = new BACnetObjectPropertyReference();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }
                else if (property_id == BacnetPropertyIds.PROP_DATE_LIST)
                {
                    BACnetCalendarEntry v = new BACnetCalendarEntry();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_CONTEXT_SPECIFIC_DECODED;
                    value.Value = v;
                    return tag_len;
                }
                else if (property_id == BacnetPropertyIds.PROP_SUBORDINATE_LIST ||property_id == BacnetPropertyIds.PROP_LAST_ACCESS_POINT || property_id == BacnetPropertyIds.PROP_BELONGS_TO || property_id == BacnetPropertyIds.PROP_ACCESS_EVENT_CREDENTIAL || property_id == BacnetPropertyIds.PROP_ACCOMPANIMENT || property_id == BacnetPropertyIds.PROP_ZONE_FROM || property_id == BacnetPropertyIds.PROP_ZONE_TO || property_id == BacnetPropertyIds.PROP_LAST_CREDENTIAL_ADDED || property_id == BacnetPropertyIds.PROP_LAST_CREDENTIAL_REMOVED ||property_id == BacnetPropertyIds.PROP_ENTRY_POINTS || property_id == BacnetPropertyIds.PROP_EXIT_POINTS)
                {
                    BacnetDeviceObjectReference v=new BacnetDeviceObjectReference();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_DEVICE_OBJECT_REFERENCE;
                    value.Value = v;
                    return tag_len;
                }
                else if (property_id == BacnetPropertyIds.PROP_EVENT_TIME_STAMPS || property_id == BacnetPropertyIds.PROP_COMMAND_TIME_ARRAY || property_id == BacnetPropertyIds.PROP_LAST_COMMAND_TIME || property_id == BacnetPropertyIds.PROP_ACCESS_EVENT_TIME || property_id == BacnetPropertyIds.PROP_UPDATE_TIME || property_id == BacnetPropertyIds.PROP_LAST_RESTORE_TIME)
                {

                    ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                    len++; // skip Tag

                    if (tag_number == 0) // Time without date
                    {
                        DateTime dt;
                        len += decode_bacnet_time(buffer, offset + len, out dt);
                        value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_TIMESTAMP;
                        value.Value = dt;
                    }
                    else if (tag_number == 1) // sequence number
                    {
                        uint val;
                        len += decode_unsigned(buffer, offset + len, len_value_type, out val);
                        value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT;
                        value.Value = val;
                    }
                    else if (tag_number == 2) // date + time
                    {
                        DateTime dt;
                        len += decode_bacnet_datetime(buffer, offset + len, out dt);
                        value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_TIMESTAMP;
                        len++;  // closing Tag
                        value.Value = dt;
                    }
                    else
                        return -1;

                    return len;
                }else if(property_id == BacnetPropertyIds.PROP_EVENT_PARAMETERS)
                {
                    BACnetEventParameter v = new BACnetEventParameter();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;

                    value.Value = v;


                    return tag_len;

                }
                else if (property_id == BacnetPropertyIds.PROP_PRESCALE)
                {
                    BACnetPrescale v = new BACnetPrescale();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }
                else if (property_id == BacnetPropertyIds.PROP_SCALE)
                {
                    BACnetScale v = new BACnetScale();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }
                else if (property_id == BacnetPropertyIds.PROP_FAULT_PARAMETERS)
                {
                    BACnetFaultParameter v = new BACnetFaultParameter();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }
                else if(property_id == BacnetPropertyIds.PROP_NEGATIVE_ACCESS_RULES ||property_id == BacnetPropertyIds.PROP_POSITIVE_ACCESS_RULES)
                {
                    BACnetAccessRule v = new BACnetAccessRule();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }
                else if (property_id == BacnetPropertyIds.PROP_LIGHTING_COMMAND)
                {
                    BACnetLightingCommand v = new BACnetLightingCommand();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;

                    return tag_len;

                }
                else if(property_id == BacnetPropertyIds.PROP_ACTION  && object_type == BacnetObjectTypes.OBJECT_COMMAND)
                {
                    BACnetActionList v = new BACnetActionList();

                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;

                    return tag_len;
                }
                else if(property_id == BacnetPropertyIds.PROP_PRESENT_VALUE && object_type == BacnetObjectTypes.OBJECT_CHANNEL)
                {
                    BACnetChannelValue v = new BACnetChannelValue();

                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;

                    return tag_len;
                }
                else if(property_id == BacnetPropertyIds.PROP_ROUTING_TABLE)
                {
                    BACnetRouterEntry v = new BACnetRouterEntry();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;

                    return tag_len;

                }
                else if(property_id == BacnetPropertyIds.PROP_VIRTUAL_MAC_ADDRESS_TABLE)
                {
                    BACnetVMACEntry v = new BACnetVMACEntry();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;

                    return tag_len;
                }
                else if(property_id == BacnetPropertyIds.PROP_BBMD_FOREIGN_DEVICE_TABLE)
                {
                    BACnetFDTEntry v = new BACnetFDTEntry();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;
                }
                else if(property_id == BacnetPropertyIds.PROP_BACNET_IP_GLOBAL_ADDRESS || property_id == BacnetPropertyIds.PROP_FD_BBMD_ADDRESS )
                {

                    BACnetHostNPort v = new BACnetHostNPort();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }
                else if(property_id == BacnetPropertyIds.PROP_BBMD_BROADCAST_DISTRIBUTION_TABLE)
                {
                    BACnetBDTEntry v = new BACnetBDTEntry();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;
                }
                else if(property_id == BacnetPropertyIds.PROP_LOGGING_RECORD && object_type == BacnetObjectTypes.OBJECT_ACCUMULATOR)
                {
                    BACnetAccumulatorRecord v = new BACnetAccumulatorRecord();
                    tag_len = v.ASN1decode(buffer, offset, (uint)max_offset);
                    if (tag_len < 0) return -1;
                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_CONTEXT_SPECIFIC;
                    value.Value = v;
                    return tag_len;

                }


                    value.Tag = BacnetApplicationTags.BACNET_APPLICATION_TAG_CONTEXT_SPECIFIC_DECODED;
                List<BacnetValue> list = new List<BacnetValue>();

                decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                // If an opening tag is not present, no loop to get the values
                bool MultiplValue=IS_OPENING_TAG(buffer[offset + len]);

                while (((len + offset) <= max_offset) && !IS_CLOSING_TAG(buffer[offset + len]))
                {
                    tag_len = decode_tag_number_and_value(buffer, offset + len, out sub_tag_number, out len_value_type);
                    if (tag_len < 0) return -1;
                    
                    if (sub_tag_number==0 && len_value_type==0)
                    {
                        list.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL, null));
                        len += tag_len;
                    }
                    else if (len_value_type == 0)
                    {
                        BacnetValue sub_value;
                        len += tag_len;
                        tag_len = bacapp_decode_application_data(buffer, offset + len, max_offset, BacnetObjectTypes.MAX_BACNET_OBJECT_TYPE, BacnetPropertyIds.MAX_BACNET_PROPERTY_ID, out sub_value);
                        if (tag_len < 0) return -1;
                        list.Add(sub_value);
                        len += tag_len;
                    }
                    else
                    {
                        BacnetValue sub_value = new BacnetValue();

                        //override tag_number
                        BacnetApplicationTags override_tag_number = bacapp_context_tag_type(property_id, sub_tag_number);
                        if (override_tag_number != BacnetApplicationTags.MAX_BACNET_APPLICATION_TAG) sub_tag_number = (byte)override_tag_number;

                        //try app decode
                        int sub_tag_len = bacapp_decode_data(buffer, offset + len + tag_len, max_offset, (BacnetApplicationTags)sub_tag_number, len_value_type, out sub_value);
                        if (sub_tag_len == (int)len_value_type)
                        {
                            list.Add(sub_value);
                            len += tag_len + (int)len_value_type;
                        }
                        else
                        {
                            //fallback to copy byte array
                            byte[] context_specific = new byte[(int)len_value_type];
                            Array.Copy(buffer, offset + len + tag_len, context_specific, 0, (int)len_value_type);
                            sub_value = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_CONTEXT_SPECIFIC_ENCODED, context_specific);

                            list.Add(sub_value);
                            len += tag_len + (int)len_value_type;
                        }
                    }

                    if (MultiplValue == false)
                    {
                        value = list[0];
                        return len;
                    }
                }
                if ((len + offset) > max_offset) return -1;

                //end tag
                if (decode_is_closing_tag_number(buffer, offset + len, tag_number))
                    len++;

                //context specifique is array of BACNET_VALUE
                value.Value = list.ToArray();
            }
            else
            {
                return -1;
            }

            return len;
        }

        public static int decode_object_id(byte[] buffer, int offset, out BacnetObjectTypes object_type, out uint instance)
        {
            uint value = 0;
            int len = 0;

            len = decode_unsigned32(buffer, offset, out value);
            object_type = (BacnetObjectTypes)(((value >> BACNET_INSTANCE_BITS) & BACNET_MAX_OBJECT));
            instance = (value & BACNET_MAX_INSTANCE);

            return len;
        }

        public static int decode_enumerated(byte[] buffer, int offset, uint len_value, out uint value)
        {
            int len;
            len = decode_unsigned(buffer, offset, len_value, out value);
            return len;
        }

        public static bool decode_is_context_tag(byte[] buffer, int offset, byte tag_number)
        {
            byte my_tag_number = 0;

            decode_tag_number(buffer, offset, out my_tag_number);
            return (bool)(IS_CONTEXT_SPECIFIC(buffer[offset]) && (my_tag_number == tag_number));
        }

        public static bool decode_is_opening_tag_number(byte[] buffer, int offset, byte tag_number)
        {
            byte my_tag_number = 0;

            decode_tag_number(buffer, offset, out my_tag_number);
            return (bool)(IS_OPENING_TAG(buffer[offset]) && (my_tag_number == tag_number));
        }

        public static bool decode_is_closing_tag_number(byte[] buffer, int offset, byte tag_number)
        {
            byte my_tag_number = 0;

            decode_tag_number(buffer, offset, out my_tag_number);
            return (bool)(IS_CLOSING_TAG(buffer[offset]) && (my_tag_number == tag_number));
        }

        public static bool decode_is_closing_tag(byte[] buffer, int offset)
        {
            return (bool)((buffer[offset] & 0x07) == 7);
        }

        public static bool decode_is_opening_tag(byte[] buffer, int offset)
        {
            return (bool)((buffer[offset] & 0x07) == 6);
        }

        public static int decode_tag_number_and_value(byte[] buffer, int offset, out byte tag_number, out uint value)
        {
            int len = 1;
            ushort value16;
            uint value32;

            len = decode_tag_number(buffer, offset, out tag_number);
            if (IS_EXTENDED_VALUE(buffer[offset]))
            {
                /* tagged as uint32_t */
                if (buffer[offset + len] == 255)
                {
                    len++;
                    len += decode_unsigned32(buffer, offset + len, out value32);
                    value = value32;
                }
                /* tagged as uint16_t */
                else if (buffer[offset + len] == 254)
                {
                    len++;
                    len += decode_unsigned16(buffer, offset + len, out value16);
                    value = value16;
                }
                /* no tag - must be uint8_t */
                else
                {
                    value = buffer[offset + len];
                    len++;
                }
            }
            else if (IS_OPENING_TAG(buffer[offset]))
            {
                value = 0;
            }
            else if (IS_CLOSING_TAG(buffer[offset]))
            {
                /* closing tag */
                value = 0;
            }
            else
            {
                /* small value */
                value = (uint)(buffer[offset] & 0x07);
            }

            return len;
        }

        /// <summary>
        /// This is used by the Active_COV_Subscriptions property in DEVICE
        /// </summary>
        public static void encode_cov_subscription(EncodeBuffer buffer, BacnetCOVSubscription value)
        {
            /* Recipient [0] BACnetRecipientProcess - opening */
            ASN1.encode_opening_tag(buffer, 0);

            /*  recipient [0] BACnetRecipient - opening */
            ASN1.encode_opening_tag(buffer, 0);
            /* CHOICE - device [0] BACnetObjectIdentifier - opening */
            /* CHOICE - address [1] BACnetAddress - opening */
            ASN1.encode_opening_tag(buffer, 1);
            /* network-number Unsigned16, */
            /* -- A value of 0 indicates the local network */
            ASN1.encode_application_unsigned(buffer, value.Recipient.net);
            /* mac-address OCTET STRING */
            /* -- A string of length 0 indicates a broadcast */
            if (value.Recipient.net == 0xFFFF)
                ASN1.encode_application_octet_string(buffer, new byte[0], 0, 0);
            else
                ASN1.encode_application_octet_string(buffer, value.Recipient.adr, 0, value.Recipient.adr.Length);
            /* CHOICE - address [1] BACnetAddress - closing */
            ASN1.encode_closing_tag(buffer, 1);
            /*  recipient [0] BACnetRecipient - closing */
            ASN1.encode_closing_tag(buffer, 0);

            /* processIdentifier [1] Unsigned32 */
            ASN1.encode_context_unsigned(buffer, 1, value.subscriptionProcessIdentifier);
            /* Recipient [0] BACnetRecipientProcess - closing */
            ASN1.encode_closing_tag(buffer, 0);

            /*  MonitoredPropertyReference [1] BACnetObjectPropertyReference, */
            ASN1.encode_opening_tag(buffer, 1);
            /* objectIdentifier [0] */
            ASN1.encode_context_object_id(buffer, 0, value.monitoredObjectIdentifier.type, value.monitoredObjectIdentifier.instance);
            /* propertyIdentifier [1] */
            /* FIXME: we are monitoring 2 properties! How to encode? */
            ASN1.encode_context_enumerated(buffer, 1, value.monitoredProperty.propertyIdentifier);
            if (value.monitoredProperty.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
                ASN1.encode_context_unsigned(buffer, 2, value.monitoredProperty.propertyArrayIndex);
            /* MonitoredPropertyReference [1] - closing */
            ASN1.encode_closing_tag(buffer, 1);

            /* IssueConfirmedNotifications [2] BOOLEAN, */
            ASN1.encode_context_boolean(buffer, 2, value.IssueConfirmedNotifications);
            /* TimeRemaining [3] Unsigned, */
            ASN1.encode_context_unsigned(buffer, 3, value.TimeRemaining);
            /* COVIncrement [4] REAL OPTIONAL, */
            if (value.COVIncrement > 0)
                ASN1.encode_context_real(buffer, 4, value.COVIncrement);
        }

        public static int decode_cov_subscription(byte[] buffer, int offset, int apdu_len, out BacnetCOVSubscription value)
        {
            int len = 0;
            int tag_len;
            byte tag_number;
            uint len_value_type;
            uint tmp;

            value = new BacnetCOVSubscription();
            value.Recipient = new BacnetAddress(BacnetAddressTypes.None, 0, null);

            if (!decode_is_opening_tag_number(buffer, offset + len, 0))
                return -1;
            len++;
            if (!decode_is_opening_tag_number(buffer, offset + len, 0))
                return -1;
            len++;
            if (!decode_is_opening_tag_number(buffer, offset + len, 1))
                return -1;
            len++;
            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                return -1;
            len += decode_unsigned(buffer, offset + len, len_value_type, out tmp);
            value.Recipient.net = (ushort)tmp;
            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING)
                return -1;
            value.Recipient.adr = new byte[len_value_type];
            len += decode_octet_string(buffer, offset + len, apdu_len, value.Recipient.adr, 0, len_value_type);
            if (!decode_is_closing_tag_number(buffer, offset + len, 1))
                return -1;
            len++;
            if (!decode_is_closing_tag_number(buffer, offset + len, 0))
                return -1;
            len++;

            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -1;
            len += decode_unsigned(buffer, offset + len, len_value_type, out value.subscriptionProcessIdentifier);
            if (!decode_is_closing_tag_number(buffer, offset + len, 0))
                return -1;
            len++;

            if (!decode_is_opening_tag_number(buffer, offset + len, 1))
                return -1;
            len++;
            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 0)
                return -1;
            len += decode_object_id(buffer, offset + len, out value.monitoredObjectIdentifier.type, out value.monitoredObjectIdentifier.instance);
            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -1;
            len += decode_enumerated(buffer, offset + len, len_value_type, out value.monitoredProperty.propertyIdentifier);
            tag_len = decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number == 2)
            {
                len += tag_len;
                len += decode_unsigned(buffer, offset + len, len_value_type, out value.monitoredProperty.propertyArrayIndex);
            }
            else
                value.monitoredProperty.propertyArrayIndex = BACNET_ARRAY_ALL;
            if (!decode_is_closing_tag_number(buffer, offset + len, 1))
                return -1;
            len++;

            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 2)
                return -1;
            value.IssueConfirmedNotifications = buffer[offset + len] > 0 ? true : false;
            len++;

            len += decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 3)
                return -1;
            len += decode_unsigned(buffer, offset + len, len_value_type, out value.TimeRemaining);

            if ((len < apdu_len) && (!IS_CLOSING_TAG(buffer[offset + len])))
            {
                decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                if (tag_number != 4)
                    return len;
                len++;
                len += decode_real(buffer, offset + len, out value.COVIncrement);
            }

            return len;
        }
    }

    public class EncodeBuffer
    {
        public byte[] buffer;           //buffer to serialize into
        public int offset;              //offset in buffer ... will go beyond max_offset (so that you may count what's needed)
        public int max_offset;          //don't write beyond this offset
        public int serialize_counter;   //used with 'min_limit'
        public int min_limit;           //don't write before this limit (used for segmentation)
        public EncodeResult result;
        public bool expandable;
        public EncodeBuffer()
        {
            expandable = true;
            buffer = new byte[128];
            max_offset = buffer.Length - 1;
        }
        public EncodeBuffer(byte[] buffer, int offset)
        {
            if (buffer == null) buffer = new byte[0];
            this.expandable = false;
            this.buffer = buffer;
            this.offset = offset;
            this.max_offset = buffer.Length;
        }
        public void Increment()
        {
            if (offset < max_offset)
            {
                if (serialize_counter >= min_limit)
                    offset++;
                serialize_counter++;
            }
            else
            {
                if (serialize_counter >= min_limit)
                    offset++;
            }
        }
        public void Add(byte b)
        {
            if (offset < max_offset)
            {
                if (serialize_counter >= min_limit)
                    buffer[offset] = b;
            }
            else
            {
                if (expandable)
                {
                    Array.Resize<byte>(ref buffer, buffer.Length * 2);
                    max_offset = buffer.Length - 1;
                    if (serialize_counter >= min_limit)
                        buffer[offset] = b;
                }
                else
                    result |= EncodeResult.NotEnoughBuffer;
            }
            Increment();
        }
        public void Add(byte[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
                Add(buffer[i]);
        }
        public int GetDiff(EncodeBuffer buffer)
        {
            int diff = Math.Abs(buffer.offset - offset);
            diff = Math.Max(Math.Abs(buffer.serialize_counter - serialize_counter), diff);
            return diff;
        }
        public EncodeBuffer Copy()
        {
            EncodeBuffer ret = new EncodeBuffer();
            ret.buffer = buffer;
            ret.max_offset = max_offset;
            ret.min_limit = min_limit;
            ret.offset = offset;
            ret.result = result;
            ret.serialize_counter = serialize_counter;
            ret.expandable = expandable;
            return ret;
        }

        public byte[] ToArray()
        {
            byte[] ret = new byte[offset];
            Array.Copy(buffer, 0, ret, 0, ret.Length);
            return ret;
        }
        public void Reset(int offset)
        {
            this.offset = offset;
            serialize_counter = 0;
            result = EncodeResult.Good;
        }
        public override string ToString()
        {
            return offset + " - " + serialize_counter;
        }
        public int GetLength()
        {
            return Math.Min(offset, max_offset);
        }
    }

    [Flags]
    public enum EncodeResult
    {
        Good = 0,
        NotEnoughBuffer = 1,
    }

    public enum BacnetReadRangeRequestTypes
    {
        RR_BY_POSITION = 1,
        RR_BY_SEQUENCE = 2,
        RR_BY_TIME = 4,
        RR_READ_ALL = 8,
    }

    public class Services
    {
        public static void EncodeIamBroadcast(EncodeBuffer buffer, UInt32 device_id, uint max_apdu, BacnetSegmentations segmentation, UInt16 vendor_id)
        {
            ASN1.encode_application_object_id(buffer, BacnetObjectTypes.OBJECT_DEVICE, device_id);
            ASN1.encode_application_unsigned(buffer, max_apdu);
            ASN1.encode_application_enumerated(buffer, (uint)segmentation);
            ASN1.encode_application_unsigned(buffer, vendor_id);
        }

        public static int DecodeIamBroadcast(byte[] buffer, int offset, out UInt32 device_id, out UInt32 max_apdu, out BacnetSegmentations segmentation, out UInt16 vendor_id)
        {
            int len;
            int apdu_len = 0;
            int org_offset = offset;
            uint len_value;
            byte tag_number;
            BacnetObjectId object_id;
            uint decoded_value;

            device_id = 0;
            max_apdu = 0;
            segmentation = BacnetSegmentations.SEGMENTATION_NONE;
            vendor_id = 0;

            /* OBJECT ID - object id */
            len =
                ASN1.decode_tag_number_and_value(buffer, offset + apdu_len, out tag_number, out len_value);
            apdu_len += len;
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID)
                return -1;
            len = ASN1.decode_object_id(buffer, offset + apdu_len, out object_id.type, out object_id.instance);
            apdu_len += len;
            if (object_id.type != BacnetObjectTypes.OBJECT_DEVICE)
                return -1;
            device_id = object_id.instance;
            /* MAX APDU - unsigned */
            len =
                ASN1.decode_tag_number_and_value(buffer, offset + apdu_len, out tag_number, out len_value);
            apdu_len += len;
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                return -1;
            len = ASN1.decode_unsigned(buffer, offset + apdu_len, len_value, out decoded_value);
            apdu_len += len;
            max_apdu = decoded_value;
            /* Segmentation - enumerated */
            len =
                ASN1.decode_tag_number_and_value(buffer, offset + apdu_len, out tag_number, out len_value);
            apdu_len += len;
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED)
                return -1;
            len = ASN1.decode_enumerated(buffer, offset + apdu_len, len_value, out decoded_value);
            apdu_len += len;
            if (decoded_value > (uint)BacnetSegmentations.SEGMENTATION_NONE)
                return -1;
            segmentation = (BacnetSegmentations)decoded_value;
            /* Vendor ID - unsigned16 */
            len =
                ASN1.decode_tag_number_and_value(buffer, offset + apdu_len, out tag_number, out len_value);
            apdu_len += len;
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                return -1;
            len = ASN1.decode_unsigned(buffer, offset + apdu_len, len_value, out decoded_value);
            apdu_len += len;
            if (decoded_value > 0xFFFF)
                return -1;
            vendor_id = (ushort)decoded_value;

            return offset - org_offset;
        }

        public static void EncodeIhaveBroadcast(EncodeBuffer buffer, BacnetObjectId device_id, BacnetObjectId object_id, string object_name)
        {
            /* deviceIdentifier */
            ASN1.encode_application_object_id(buffer, device_id.type, device_id.instance);
            /* objectIdentifier */
            ASN1.encode_application_object_id(buffer, object_id.type, object_id.instance);
            /* objectName */
            ASN1.encode_application_character_string(buffer, object_name);
        }

        public static void EncodeWhoHasBroadcast(EncodeBuffer buffer, int low_limit, int high_limit, BacnetObjectId object_id, string object_name)
        {
            /* optional limits - must be used as a pair */
            if ((low_limit >= 0) && (low_limit <= ASN1.BACNET_MAX_INSTANCE) && (high_limit >= 0) && (high_limit <= ASN1.BACNET_MAX_INSTANCE))
            {
                ASN1.encode_context_unsigned(buffer, 0, (uint)low_limit);
                ASN1.encode_context_unsigned(buffer, 1, (uint)high_limit);
            }
            if (!string.IsNullOrEmpty(object_name))
            {
                ASN1.encode_context_character_string(buffer, 3, object_name);
            }
            else
            {
                ASN1.encode_context_object_id(buffer, 2, object_id.type, object_id.instance);
            }
        }

        public static void EncodeWhoIsBroadcast(EncodeBuffer buffer, int low_limit, int high_limit)
        {
            /* optional limits - must be used as a pair */
            if ((low_limit >= 0) && (low_limit <= ASN1.BACNET_MAX_INSTANCE) &&
                (high_limit >= 0) && (high_limit <= ASN1.BACNET_MAX_INSTANCE))
            {
                ASN1.encode_context_unsigned(buffer, 0, (uint)low_limit);
                ASN1.encode_context_unsigned(buffer, 1, (uint)high_limit);
            }
        }

        public static int DecodeWhoIsBroadcast(byte[] buffer, int offset, int apdu_len, out int low_limit, out int high_limit)
        {
            int len = 0;
            byte tag_number;
            uint len_value;
            uint decoded_value;

            low_limit = -1;
            high_limit = -1;

            if (apdu_len <= 0) return len;

            /* optional limits - must be used as a pair */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
            if (tag_number != 0)
                return -1;
            if (apdu_len > len)
            {
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out decoded_value);
                if (decoded_value <= ASN1.BACNET_MAX_INSTANCE)
                    low_limit = (int)decoded_value;
                if (apdu_len > len)
                {
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    if (tag_number != 1)
                        return -1;
                    if (apdu_len > len)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value, out decoded_value);
                        if (decoded_value <= ASN1.BACNET_MAX_INSTANCE)
                            high_limit = (int)decoded_value;
                    }
                    else
                        return -1;
                }
                else
                    return -1;
            }
            else
                return -1;

            return len;
        }
        // Added by thamersalek
        public static int DecodeWhoHasBroadcast(byte[] buffer, int offset, int apdu_len, out int low_limit, out int high_limit, out BacnetObjectId ObjId, out string ObjName)
        {

            int len = 0;
            byte tag_number;
            uint len_value;
            uint decoded_value;

            ObjName = null;
            ObjId = new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_OUTPUT, 0x3FFFFF);
            low_limit = -1;
            high_limit = -1;
            ushort ObjType;
            uint ObjInst;

            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);

            if (tag_number == 0)
            {
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out decoded_value);
                if (decoded_value <= ASN1.BACNET_MAX_INSTANCE)
                    low_limit = (int)decoded_value;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
            }

            if (tag_number == 1)
            {
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out decoded_value);
                if (decoded_value <= ASN1.BACNET_MAX_INSTANCE)
                    high_limit = (int)decoded_value;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
            }

            if (tag_number == 2)
            {
                len += ASN1.decode_object_id(buffer, offset + len, out ObjType, out ObjInst);
                ObjId = new BacnetObjectId((BacnetObjectTypes)ObjType, ObjInst);
            }

            if (tag_number == 3)
            {
                len += ASN1.decode_character_string(buffer, offset + len, apdu_len - (offset + len), len_value, out ObjName);

            }

            return len;
        }
        public static void EncodeAlarmAcknowledge(EncodeBuffer buffer, uint ackProcessIdentifier, BacnetObjectId eventObjectIdentifier, uint eventStateAcked, string ackSource, BacnetGenericTime eventTimeStamp, BacnetGenericTime ackTimeStamp)
        {
            ASN1.encode_context_unsigned(buffer, 0, ackProcessIdentifier);
            ASN1.encode_context_object_id(buffer, 1, eventObjectIdentifier.type, eventObjectIdentifier.instance);
            ASN1.encode_context_enumerated(buffer, 2, eventStateAcked);
            ASN1.bacapp_encode_context_timestamp(buffer, 3, eventTimeStamp);
            ASN1.encode_context_character_string(buffer, 4, ackSource);
            ASN1.bacapp_encode_context_timestamp(buffer, 5, ackTimeStamp);
        }

        public static void EncodeAtomicReadFile(EncodeBuffer buffer, bool is_stream, BacnetObjectId object_id, int position, uint count)
        {
            ASN1.encode_application_object_id(buffer, object_id.type, object_id.instance);
            switch (is_stream)
            {
                case true:
                    ASN1.encode_opening_tag(buffer, 0);
                    ASN1.encode_application_signed(buffer, position);
                    ASN1.encode_application_unsigned(buffer, count);
                    ASN1.encode_closing_tag(buffer, 0);
                    break;
                case false:
                    ASN1.encode_opening_tag(buffer, 1);
                    ASN1.encode_application_signed(buffer, position);
                    ASN1.encode_application_unsigned(buffer, count);
                    ASN1.encode_closing_tag(buffer, 1);
                    break;

            }
        }

        public static int DecodeAtomicReadFile(byte[] buffer, int offset, int apdu_len, out bool is_stream, out BacnetObjectId object_id, out int position, out uint count)
        {
            int len = 0;
            byte tag_number;
            uint len_value_type;
            int tag_len;

            is_stream = true;
            object_id = new BacnetObjectId();
            position = -1;
            count = 0;

            len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID)
                return -1;
            len += ASN1.decode_object_id(buffer, offset + len, out object_id.type, out object_id.instance);
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {
                is_stream = true;
                /* a tag number is not extended so only one octet */
                len++;
                /* fileStartPosition */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT)
                    return -1;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
                /* requestedOctetCount */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                    return -1;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out count);
                if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                    return -1;
                /* a tag number is not extended so only one octet */
                len++;
            }
            else if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
            {
                is_stream = false;
                /* a tag number is not extended so only one octet */
                len++;
                /* fileStartRecord */
                tag_len =
                    ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number,
                    out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT)
                    return -1;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
                /* RecordCount */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                    return -1;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out count);
                if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                    return -1;
                /* a tag number is not extended so only one octet */
                len++;
            }
            else
                return -1;

            return len;
        }

        public static void EncodeAtomicReadFileAcknowledge(EncodeBuffer buffer, bool is_stream, bool end_of_file, int position, uint block_count, byte[][] blocks, int[] counts)
        {
            ASN1.encode_application_boolean(buffer, end_of_file);
            switch (is_stream)
            {
                case true:
                    ASN1.encode_opening_tag(buffer, 0);
                    ASN1.encode_application_signed(buffer, position);
                    ASN1.encode_application_octet_string(buffer, blocks[0], 0, counts[0]);
                    ASN1.encode_closing_tag(buffer, 0);
                    break;
                case false:
                    ASN1.encode_opening_tag(buffer, 1);
                    ASN1.encode_application_signed(buffer, position);
                    ASN1.encode_application_unsigned(buffer, block_count);
                    for (int i = 0; i < block_count; i++)
                        ASN1.encode_application_octet_string(buffer, blocks[i], 0, counts[i]);
                    ASN1.encode_closing_tag(buffer, 1);
                    break;

            }
        }

        public static void EncodeAtomicWriteFile(EncodeBuffer buffer, bool is_stream, BacnetObjectId object_id, int position, uint block_count, byte[][] blocks, int[] counts)
        {
            ASN1.encode_application_object_id(buffer, object_id.type, object_id.instance);
            switch (is_stream)
            {
                case true:
                    ASN1.encode_opening_tag(buffer, 0);
                    ASN1.encode_application_signed(buffer, position);
                    ASN1.encode_application_octet_string(buffer, blocks[0], 0, counts[0]);
                    ASN1.encode_closing_tag(buffer, 0);
                    break;
                case false:
                    ASN1.encode_opening_tag(buffer, 1);
                    ASN1.encode_application_signed(buffer, position);
                    ASN1.encode_application_unsigned(buffer, block_count);
                    for (int i = 0; i < block_count; i++)
                        ASN1.encode_application_octet_string(buffer, blocks[i], 0, counts[i]);
                    ASN1.encode_closing_tag(buffer, 1);
                    break;

            }
        }

        public static int DecodeAtomicWriteFile(byte[] buffer, int offset, int apdu_len, out bool is_stream, out BacnetObjectId object_id, out int position, out uint block_count, out byte[][] blocks, out int[] counts)
        {
            int len = 0;
            byte tag_number;
            uint len_value_type;
            int i;
            int tag_len;

            object_id = new BacnetObjectId();
            is_stream = true;
            position = -1;
            block_count = 0;
            blocks = null;
            counts = null;

            len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_ID)
                return -1;
            len += ASN1.decode_object_id(buffer, offset + len, out object_id.type, out object_id.instance);
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {
                is_stream = true;
                /* a tag number of 2 is not extended so only one octet */
                len++;
                /* fileStartPosition */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT)
                    return -1;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
                /* fileData */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING)
                    return -1;
                block_count = 1;
                blocks = new byte[1][];
                blocks[0] = new byte[len_value_type];
                counts = new int[] { (int)len_value_type };
                len += ASN1.decode_octet_string(buffer, offset + len, apdu_len, blocks[0], 0, len_value_type);
                if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                    return -1;
                /* a tag number is not extended so only one octet */
                len++;
            }
            else if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
            {
                is_stream = false;
                /* a tag number is not extended so only one octet */
                len++;
                /* fileStartRecord */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT)
                    return -1;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
                /* returnedRecordCount */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                    return -1;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out block_count);
                /* fileData */
                blocks = new byte[block_count][];
                counts = new int[block_count];
                for (i = 0; i < block_count; i++)
                {
                    tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                    len += tag_len;
                    blocks[i] = new byte[len_value_type];
                    counts[i] = (int)len_value_type;
                    if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING)
                        return -1;
                    len += ASN1.decode_octet_string(buffer, offset + len, apdu_len, blocks[i], 0, len_value_type);
                }
                if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                    return -1;
                /* a tag number is not extended so only one octet */
                len++;
            }
            else
                return -1;

            return len;
        }

        //**********************************************************************************
        // by Christopher Günter
        public static void EncodeCreateProperty(EncodeBuffer buffer, BacnetObjectId object_id, ICollection<BacnetPropertyValue> value_list)
        {

            /* Tag 1: sequence of WriteAccessSpecification */
            ASN1.encode_opening_tag(buffer, 0);
            ASN1.encode_context_object_id(buffer, 1, object_id.type, object_id.instance);
            ASN1.encode_closing_tag(buffer, 0);


            if (value_list != null)
            {
                ASN1.encode_opening_tag(buffer, 1);

                foreach (BacnetPropertyValue p_value in value_list)
                {

                    ASN1.encode_context_enumerated(buffer, 0, p_value.property.propertyIdentifier);


// frankschubert: These lines need to be removed, caused an error in CreateObject service
//                    if (p_value.property.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
//                        ASN1.encode_context_unsigned(buffer, 1, p_value.property.propertyArrayIndex);


                    ASN1.encode_opening_tag(buffer, 2);
                    foreach (BacnetValue value in p_value.value)
                    {
                        ASN1.bacapp_encode_application_data(buffer, value);
                    }
                    ASN1.encode_closing_tag(buffer, 2);


                    if (p_value.priority != ASN1.BACNET_NO_PRIORITY)
                        ASN1.encode_context_unsigned(buffer, 3, p_value.priority);
                }

                ASN1.encode_closing_tag(buffer, 1);
            }

        }
        //***************************************************************
        public static void EncodeAddListElement(EncodeBuffer buffer, BacnetObjectId object_id, uint property_id, uint array_index, IList<BacnetValue> value_list)
        {
            ASN1.encode_context_object_id(buffer, 0, object_id.type, object_id.instance);
            ASN1.encode_context_enumerated(buffer, 1, property_id);


            if (array_index != ASN1.BACNET_ARRAY_ALL)
            {
                ASN1.encode_context_unsigned(buffer, 2, array_index);
            }


            ASN1.encode_opening_tag(buffer, 3);
            foreach (BacnetValue value in value_list)
            {
                ASN1.bacapp_encode_application_data(buffer, value);
            }
            ASN1.encode_closing_tag(buffer, 3);

        }

        public static void EncodeAtomicWriteFileAcknowledge(EncodeBuffer buffer, bool is_stream, int position)
        {
            switch (is_stream)
            {
                case true:
                    ASN1.encode_context_signed(buffer, 0, position);
                    break;
                case false:
                    ASN1.encode_context_signed(buffer, 1, position);
                    break;

            }
        }

        public static void EncodeCOVNotifyConfirmed(EncodeBuffer buffer, uint subscriberProcessIdentifier, uint initiatingDeviceIdentifier, BacnetObjectId monitoredObjectIdentifier, uint timeRemaining, IEnumerable<BacnetPropertyValue> values)
        {
            /* tag 0 - subscriberProcessIdentifier */
            ASN1.encode_context_unsigned(buffer, 0, subscriberProcessIdentifier);
            /* tag 1 - initiatingDeviceIdentifier */
            ASN1.encode_context_object_id(buffer, 1, BacnetObjectTypes.OBJECT_DEVICE, initiatingDeviceIdentifier);
            /* tag 2 - monitoredObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 2, monitoredObjectIdentifier.type, monitoredObjectIdentifier.instance);
            /* tag 3 - timeRemaining */
            ASN1.encode_context_unsigned(buffer, 3, timeRemaining);
            /* tag 4 - listOfValues */
            ASN1.encode_opening_tag(buffer, 4);
            foreach(BacnetPropertyValue value in values)
            {
                /* tag 0 - propertyIdentifier */
                ASN1.encode_context_enumerated(buffer, 0, value.property.propertyIdentifier);
                /* tag 1 - propertyArrayIndex OPTIONAL */
                if (value.property.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
                {
                    ASN1.encode_context_unsigned(buffer, 1, value.property.propertyArrayIndex);
                }
                /* tag 2 - value */
                /* abstract syntax gets enclosed in a context tag */
                ASN1.encode_opening_tag(buffer, 2);
                foreach (BacnetValue v in value.value)
                {
                    ASN1.bacapp_encode_application_data(buffer, v);
                }
                ASN1.encode_closing_tag(buffer, 2);
                /* tag 3 - priority OPTIONAL */
                if (value.priority != ASN1.BACNET_NO_PRIORITY)
                {
                    ASN1.encode_context_unsigned(buffer, 3, value.priority);
                }
                /* is there another one to encode? */
                /* FIXME: check to see if there is room in the APDU */
            }
            ASN1.encode_closing_tag(buffer, 4);
        }

        public static void EncodeCOVNotifyUnconfirmed(EncodeBuffer buffer, uint subscriberProcessIdentifier, uint initiatingDeviceIdentifier, BacnetObjectId monitoredObjectIdentifier, uint timeRemaining, IEnumerable<BacnetPropertyValue> values)
        {
            /* tag 0 - subscriberProcessIdentifier */
            ASN1.encode_context_unsigned(buffer, 0, subscriberProcessIdentifier);
            /* tag 1 - initiatingDeviceIdentifier */
            ASN1.encode_context_object_id(buffer, 1, BacnetObjectTypes.OBJECT_DEVICE, initiatingDeviceIdentifier);
            /* tag 2 - monitoredObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 2, monitoredObjectIdentifier.type, monitoredObjectIdentifier.instance);
            /* tag 3 - timeRemaining */
            ASN1.encode_context_unsigned(buffer, 3, timeRemaining);
            /* tag 4 - listOfValues */
            ASN1.encode_opening_tag(buffer, 4);
            foreach (BacnetPropertyValue value in values)
            {
                /* tag 0 - propertyIdentifier */
                ASN1.encode_context_enumerated(buffer, 0, value.property.propertyIdentifier);
                /* tag 1 - propertyArrayIndex OPTIONAL */
                if (value.property.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
                {
                    ASN1.encode_context_unsigned(buffer, 1, value.property.propertyArrayIndex);
                }
                /* tag 2 - value */
                /* abstract syntax gets enclosed in a context tag */
                ASN1.encode_opening_tag(buffer, 2);
                foreach (BacnetValue v in value.value)
                {
                    ASN1.bacapp_encode_application_data(buffer, v);
                }
                ASN1.encode_closing_tag(buffer, 2);
                /* tag 3 - priority OPTIONAL */
                if (value.priority != ASN1.BACNET_NO_PRIORITY)
                {
                    ASN1.encode_context_unsigned(buffer, 3, value.priority);
                }
                /* is there another one to encode? */
                /* FIXME: check to see if there is room in the APDU */
            }
            ASN1.encode_closing_tag(buffer, 4);
        }

        public static void EncodeSubscribeCOV(EncodeBuffer buffer, uint subscriberProcessIdentifier, BacnetObjectId monitoredObjectIdentifier, bool cancellationRequest, bool issueConfirmedNotifications, uint lifetime)
        {
            /* tag 0 - subscriberProcessIdentifier */
            ASN1.encode_context_unsigned(buffer, 0, subscriberProcessIdentifier);
            /* tag 1 - monitoredObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 1, monitoredObjectIdentifier.type, monitoredObjectIdentifier.instance);
            /*
               If both the 'Issue Confirmed Notifications' and
               'Lifetime' parameters are absent, then this shall
               indicate a cancellation request.
             */
            if (!cancellationRequest)
            {
                /* tag 2 - issueConfirmedNotifications */
                ASN1.encode_context_boolean(buffer, 2, issueConfirmedNotifications);
                /* tag 3 - lifetime */
                ASN1.encode_context_unsigned(buffer, 3, lifetime);
            }
        }

        public static int DecodeSubscribeCOV(byte[] buffer, int offset, int apdu_len, out uint subscriberProcessIdentifier, out BacnetObjectId monitoredObjectIdentifier, out bool cancellationRequest, out bool issueConfirmedNotifications, out uint lifetime)
        {
            int len = 0;
            byte tag_number;
            uint len_value;

            subscriberProcessIdentifier = 0;
            monitoredObjectIdentifier = new BacnetObjectId();
            cancellationRequest = false;
            issueConfirmedNotifications = false;
            lifetime = 0;

            /* tag 0 - subscriberProcessIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 0))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out subscriberProcessIdentifier);
            }
            else
                return -1;
            /* tag 1 - monitoredObjectIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 1))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_object_id(buffer, offset + len, out monitoredObjectIdentifier.type, out monitoredObjectIdentifier.instance);
            }
            else
                return -1;
            /* optional parameters - if missing, means cancellation */
            if (len < apdu_len)
            {
                /* tag 2 - issueConfirmedNotifications - optional */
                if (ASN1.decode_is_context_tag(buffer, offset + len, 2))
                {
                    cancellationRequest = false;
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    issueConfirmedNotifications = buffer[offset + len] > 0;
                    len += (int)len_value;
                }
                else
                {
                    cancellationRequest = true;
                }
                /* tag 3 - lifetime - optional */
                if (ASN1.decode_is_context_tag(buffer, offset + len, 3))
                {
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value, out lifetime);
                }
                else
                {
                    lifetime = 0;
                }
            }
            else
            {
                cancellationRequest = true;
            }

            return len;
        }

        public static void EncodeSubscribeProperty(EncodeBuffer buffer, uint subscriberProcessIdentifier, BacnetObjectId monitoredObjectIdentifier, bool cancellationRequest, bool issueConfirmedNotifications, uint lifetime, BacnetPropertyReference monitoredProperty, bool covIncrementPresent, float covIncrement)
        {
            /* tag 0 - subscriberProcessIdentifier */
            ASN1.encode_context_unsigned(buffer, 0, subscriberProcessIdentifier);
            /* tag 1 - monitoredObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 1, monitoredObjectIdentifier.type, monitoredObjectIdentifier.instance);
            if (!cancellationRequest)
            {
                /* tag 2 - issueConfirmedNotifications */
                ASN1.encode_context_boolean(buffer, 2, issueConfirmedNotifications);
                /* tag 3 - lifetime */
                ASN1.encode_context_unsigned(buffer, 3, lifetime);
            }
            /* tag 4 - monitoredPropertyIdentifier */
            ASN1.encode_opening_tag(buffer, 4);
            ASN1.encode_context_enumerated(buffer, 0, monitoredProperty.propertyIdentifier);
            if (monitoredProperty.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
            {
                ASN1.encode_context_unsigned(buffer, 1, monitoredProperty.propertyArrayIndex);

            }
            ASN1.encode_closing_tag(buffer, 4);

            /* tag 5 - covIncrement */
            if (covIncrementPresent)
            {
                ASN1.encode_context_real(buffer, 5, covIncrement);
            }
        }

        public static int DecodeSubscribeProperty(byte[] buffer, int offset, int apdu_len, out uint subscriberProcessIdentifier, out BacnetObjectId monitoredObjectIdentifier, out BacnetPropertyReference monitoredProperty, out bool cancellationRequest, out bool issueConfirmedNotifications, out uint lifetime, out float covIncrement)
        {
            int len = 0;
            byte tag_number;
            uint len_value;
            uint decoded_value;

            subscriberProcessIdentifier = 0;
            monitoredObjectIdentifier = new BacnetObjectId();
            cancellationRequest = false;
            issueConfirmedNotifications = false;
            lifetime = 0;
            covIncrement = 0;
            monitoredProperty = new BacnetPropertyReference();

            /* tag 0 - subscriberProcessIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 0))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out subscriberProcessIdentifier);
            }
            else
                return -1;

            /* tag 1 - monitoredObjectIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 1))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_object_id(buffer, offset + len, out monitoredObjectIdentifier.type, out monitoredObjectIdentifier.instance);
            }
            else
                return -1;

            /* tag 2 - issueConfirmedNotifications - optional */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 2))
            {
                cancellationRequest = false;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                issueConfirmedNotifications = buffer[offset + len] > 0;
                len++;
            }
            else
            {
                cancellationRequest = true;
            }

            /* tag 3 - lifetime - optional */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 3))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out lifetime);
            }
            else
            {
                lifetime = 0;
            }

            /* tag 4 - monitoredPropertyIdentifier */
            if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 4))
                return -1;

            /* a tag number of 4 is not extended so only one octet */
            len++;
            /* the propertyIdentifier is tag 0 */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 0))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_enumerated(buffer, offset + len, len_value, out decoded_value);
                monitoredProperty.propertyIdentifier = decoded_value;
            }
            else
                return -1;

            /* the optional array index is tag 1 */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 1))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number,out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out decoded_value);
                monitoredProperty.propertyArrayIndex = decoded_value;
            }
            else
            {
                monitoredProperty.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;
            }

            if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 4))
                return -1;

            /* a tag number of 4 is not extended so only one octet */
            len++;
            /* tag 5 - covIncrement - optional */
            if ((len < apdu_len)&&(ASN1.decode_is_context_tag(buffer, offset + len, 5)))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_real(buffer, offset + len, out covIncrement);
            }

            return len;
        }

        // F Chaxel
        public static int DecodeEventNotifyData(byte[] buffer, int offset, int apdu_len, out BacnetEventNotificationData EventData)
        {
            int len = 0;
            uint len_value;
            byte tag_number;

            EventData = new BacnetEventNotificationData();

            /* tag 0 - processIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 0))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out EventData.processIdentifier);
            }
            else
                return -1;

            /*  tag 1 - initiatingObjectIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 1))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_object_id(buffer, offset + len, out EventData.initiatingObjectIdentifier.type, out EventData.initiatingObjectIdentifier.instance);
            }
            else
                return -1;

            /*  tag 2 - eventObjectIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 2))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_object_id(buffer, offset + len, out EventData.eventObjectIdentifier.type, out EventData.eventObjectIdentifier.instance);
            }
            else
                return -1;

            /*  tag 3 - timeStamp */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 3))
            {   DateTime date;
                DateTime time;

                len+=2; // opening Tag 3 then 2

                len += ASN1.decode_application_date(buffer, offset+len, out date);
                len += ASN1.decode_application_time(buffer, offset+len, out time);
                EventData.timeStamp.Time= new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
                EventData.timeStamp.Tag = BacnetTimestampTags.TIME_STAMP_DATETIME;
                len+=2; // closing tag 2 then 3
            }
            else
                return -1;

            /* tag 4 - noticicationClass */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 4))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out EventData.notificationClass);
            }
            else
                return -1;

            /* tag 5 - priority */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 5))
            {
                uint priority;

                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out priority);
                if (priority>0xFF) return -1;
                EventData.priority = (byte)priority;
            }
            else
                return -1;

            /* tag 6 - eventType */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 6))
            {
                uint eventType;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_enumerated(buffer, offset + len, len_value, out eventType);
                EventData.eventType = (BacnetEventNotificationData.BacnetEventTypes)eventType;
            }
            else
                return -1;

            /* optional tag 7 - messageText  : never tested */
            if (ASN1.decode_is_context_tag(buffer,offset+len,7))
            {
                // max_lenght 20000 sound like a joke
                len += ASN1.decode_context_character_string(buffer, offset + len, 20000, 7, out EventData.messageText);
            }

            /* tag 8 - notifyType */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 8))
            {
                uint notifyType;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_enumerated(buffer, offset + len, len_value, out notifyType);
                EventData.notifyType = (BacnetEventNotificationData.BacnetNotifyTypes)notifyType;
            }
            else
                return -1;

            switch (EventData.notifyType)
            {
                case BacnetEventNotificationData.BacnetNotifyTypes.NOTIFY_ALARM:
                case BacnetEventNotificationData.BacnetNotifyTypes.NOTIFY_EVENT:
                    /* tag 9 - ackRequired */
                    byte val;
                    uint fromstate;

                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_unsigned8(buffer, offset + len, out val);
                    EventData.ackRequired = Convert.ToBoolean(val);

                    /* tag 10 - fromState */
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_enumerated(buffer, offset + len, len_value, out fromstate);
                    EventData.fromState = (BacnetEventNotificationData.BacnetEventStates)fromstate;
                    break;
                default:
                    break;
            }

            /* tag 11 - toState */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 11))
            {
                uint toState;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_enumerated(buffer, offset + len, len_value, out toState);
                EventData.toState = (BacnetEventNotificationData.BacnetEventStates)toState;
            }
            else
                return -1;

            // some work to do for Tag 12
            // somebody want to do it ?

            return len;

        }
        private static void EncodeEventNotifyData(EncodeBuffer buffer, BacnetEventNotificationData data)
        {
            /* tag 0 - processIdentifier */
            ASN1.encode_context_unsigned(buffer, 0, data.processIdentifier);
            /* tag 1 - initiatingObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 1, data.initiatingObjectIdentifier.type, data.initiatingObjectIdentifier.instance);

            /* tag 2 - eventObjectIdentifier */
            ASN1.encode_context_object_id(buffer, 2, data.eventObjectIdentifier.type, data.eventObjectIdentifier.instance);

            /* tag 3 - timeStamp */
            ASN1.bacapp_encode_context_timestamp(buffer, 3, data.timeStamp);

            /* tag 4 - noticicationClass */
            ASN1.encode_context_unsigned(buffer, 4, data.notificationClass);

            /* tag 5 - priority */
            ASN1.encode_context_unsigned(buffer, 5, data.priority);

            /* tag 6 - eventType */
            ASN1.encode_context_enumerated(buffer, 6, (uint)data.eventType);

            /* tag 7 - messageText */
            if (!string.IsNullOrEmpty(data.messageText))
                ASN1.encode_context_character_string(buffer, 7, data.messageText);

            /* tag 8 - notifyType */
            ASN1.encode_context_enumerated(buffer, 8, (uint)data.notifyType);

            switch (data.notifyType)
            {
                case BacnetEventNotificationData.BacnetNotifyTypes.NOTIFY_ALARM:
                case BacnetEventNotificationData.BacnetNotifyTypes.NOTIFY_EVENT:
                    /* tag 9 - ackRequired */
                    ASN1.encode_context_boolean(buffer, 9, data.ackRequired);

                    /* tag 10 - fromState */
                    ASN1.encode_context_enumerated(buffer, 10, (uint)data.fromState);
                    break;
                default:
                    break;
            }

            /* tag 11 - toState */
            ASN1.encode_context_enumerated(buffer, 11, (uint)data.toState);

            switch (data.notifyType)
            {
                case BacnetEventNotificationData.BacnetNotifyTypes.NOTIFY_ALARM:
                case BacnetEventNotificationData.BacnetNotifyTypes.NOTIFY_EVENT:
                    /* tag 12 - event values */
                    ASN1.encode_opening_tag(buffer, 12);

                    switch (data.eventType)
                    {
                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_BITSTRING:
                            ASN1.encode_opening_tag(buffer, 0);
                            ASN1.encode_context_bitstring(buffer, 0, data.changeOfBitstring_referencedBitString);
                            ASN1.encode_context_bitstring(buffer, 1, data.changeOfBitstring_statusFlags);
                            ASN1.encode_closing_tag(buffer, 0);
                            break;

                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_STATE:
                            ASN1.encode_opening_tag(buffer, 1);
                            ASN1.encode_opening_tag(buffer, 0);
                            ASN1.bacapp_encode_property_state(buffer, data.changeOfState_newState);
                            ASN1.encode_closing_tag(buffer, 0);
                            ASN1.encode_context_bitstring(buffer, 1, data.changeOfState_statusFlags);
                            ASN1.encode_closing_tag(buffer, 1);
                            break;

                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_VALUE:
                            ASN1.encode_opening_tag(buffer, 2);
                            ASN1.encode_opening_tag(buffer, 0);

                            switch (data.changeOfValue_tag)
                            {
                                case BacnetEventNotificationData.BacnetCOVTypes.CHANGE_OF_VALUE_REAL:
                                    ASN1.encode_context_real(buffer, 1, data.changeOfValue_changeValue);
                                    break;
                                case BacnetEventNotificationData.BacnetCOVTypes.CHANGE_OF_VALUE_BITS:
                                    ASN1.encode_context_bitstring(buffer, 0, data.changeOfValue_changedBits);
                                    break;
                                default:
                                    throw new Exception("Hmm?");
                            }

                            ASN1.encode_closing_tag(buffer, 0);
                            ASN1.encode_context_bitstring(buffer, 1, data.changeOfValue_statusFlags);
                            ASN1.encode_closing_tag(buffer, 2);
                            break;

                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_FLOATING_LIMIT:
                            ASN1.encode_opening_tag(buffer, 4);
                            ASN1.encode_context_real(buffer, 0, data.floatingLimit_referenceValue);
                            ASN1.encode_context_bitstring(buffer, 1, data.floatingLimit_statusFlags);
                            ASN1.encode_context_real(buffer, 2, data.floatingLimit_setPointValue);
                            ASN1.encode_context_real(buffer, 3, data.floatingLimit_errorLimit);
                            ASN1.encode_closing_tag(buffer, 4);
                            break;

                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_OUT_OF_RANGE:
                            ASN1.encode_opening_tag(buffer, 5);
                            ASN1.encode_context_real(buffer, 0, data.outOfRange_exceedingValue);
                            ASN1.encode_context_bitstring(buffer, 1, data.outOfRange_statusFlags);
                            ASN1.encode_context_real(buffer, 2, data.outOfRange_deadband);
                            ASN1.encode_context_real(buffer, 3, data.outOfRange_exceededLimit);
                            ASN1.encode_closing_tag(buffer, 5);
                            break;

                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_CHANGE_OF_LIFE_SAFETY:
                            ASN1.encode_opening_tag(buffer, 8);
                            ASN1.encode_context_enumerated(buffer, 0, (uint)data.changeOfLifeSafety_newState);
                            ASN1.encode_context_enumerated(buffer, 1, (uint)data.changeOfLifeSafety_newMode);
                            ASN1.encode_context_bitstring(buffer, 2, data.changeOfLifeSafety_statusFlags);
                            ASN1.encode_context_enumerated(buffer, 3, (uint)data.changeOfLifeSafety_operationExpected);
                            ASN1.encode_closing_tag(buffer, 8);
                            break;

                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_BUFFER_READY:
                            ASN1.encode_opening_tag(buffer, 10);
                            ASN1.bacapp_encode_context_device_obj_property_ref(buffer, 0, data.bufferReady_bufferProperty);
                            ASN1.encode_context_unsigned(buffer, 1, data.bufferReady_previousNotification);
                            ASN1.encode_context_unsigned(buffer, 2, data.bufferReady_currentNotification);
                            ASN1.encode_closing_tag(buffer, 10);

                            break;
                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_UNSIGNED_RANGE:
                            ASN1.encode_opening_tag(buffer, 11);
                            ASN1.encode_context_unsigned(buffer, 0, data.unsignedRange_exceedingValue);
                            ASN1.encode_context_bitstring(buffer, 1, data.unsignedRange_statusFlags);
                            ASN1.encode_context_unsigned(buffer, 2, data.unsignedRange_exceededLimit);
                            ASN1.encode_closing_tag(buffer, 11);
                            break;
                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_EXTENDED:
                        case BacnetEventNotificationData.BacnetEventTypes.EVENT_COMMAND_FAILURE:
                        default:
                            throw new NotImplementedException();
                    }
                    ASN1.encode_closing_tag(buffer, 12);
                    break;
                case BacnetEventNotificationData.BacnetNotifyTypes.NOTIFY_ACK_NOTIFICATION:
                /* FIXME: handle this case */
                default:
                    break;
            }
        }

        public static void EncodeEventNotifyConfirmed(EncodeBuffer buffer, BacnetEventNotificationData data)
        {
            EncodeEventNotifyData(buffer, data);
        }

        public static void EncodeEventNotifyUnconfirmed(EncodeBuffer buffer, BacnetEventNotificationData data)
        {
            EncodeEventNotifyData(buffer, data);
        }

        public static void EncodeAlarmSummary(EncodeBuffer buffer, BacnetObjectId objectIdentifier, uint alarmState, BacnetBitString acknowledgedTransitions)
        {
            /* tag 0 - Object Identifier */
            ASN1.encode_application_object_id(buffer, objectIdentifier.type, objectIdentifier.instance);
            /* tag 1 - Alarm State */
            ASN1.encode_application_enumerated(buffer, alarmState);
            /* tag 2 - Acknowledged Transitions */
            ASN1.encode_application_bitstring(buffer, acknowledgedTransitions);
        }

        // FChaxel
        public static int DecodeAlarmSummaryOrEvent(byte[] buffer, int offset, int apdu_len, bool GetEvent, ref IList<BacnetGetEventInformationData> Alarms, out bool MoreEvent)
        {
            int len = 0;;

            if (GetEvent) len++;  // peut être tag 0

            while ((apdu_len - 3 - len) > 0)
            {
                byte tag_number = 0;
                uint len_value = 0;
                uint tmp;

                BacnetGetEventInformationData value=new BacnetGetEventInformationData();

                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_object_id(buffer, offset + len, out value.objectIdentifier.type, out value.objectIdentifier.instance);
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_enumerated(buffer, offset + len, len_value, out tmp);
                value.eventState = (BacnetEventNotificationData.BacnetEventStates)tmp;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_bitstring(buffer, offset + len, len_value, out value.acknowledgedTransitions);

                if (GetEvent)
                {
                    len++;  // opening Tag 3
                    value.eventTimeStamps = new BacnetGenericTime[3];

                    for (int i = 0; i < 3; i++)
                    {
                        DateTime dt1, dt2;

                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value); // opening tag

                        if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL)
                        {
                            len += ASN1.decode_application_date(buffer, offset + len, out dt1);
                            len += ASN1.decode_application_time(buffer, offset + len, out dt2);
                            DateTime dt = new DateTime(dt1.Year, dt1.Month, dt1.Day, dt2.Hour, dt2.Minute, dt2.Second, dt2.Millisecond);
                            value.eventTimeStamps[i] = new BacnetGenericTime(dt, BacnetTimestampTags.TIME_STAMP_DATETIME);
                            len++; // closing tag
                        }
                        else
                            len += (int)len_value;


                    }
                    len++;  // closing Tag 3

                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_enumerated(buffer, offset + len, len_value, out tmp);
                    value.notifyType = (BacnetEventNotificationData.BacnetNotifyTypes)tmp;

                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_bitstring(buffer, offset + len, len_value, out value.eventEnable);

                    len++; // opening tag 6;
                    value.eventPriorities = new uint[3];
                    for (int i = 0; i < 3; i++)
                    {
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value, out value.eventPriorities[i]);
                    }
                    len++;  // closing Tag 6
                }

                Alarms.Add(value);

            }

            if (GetEvent)
                MoreEvent = (buffer[apdu_len - 1] == 1);
            else
                MoreEvent = false;

            return len;
        }

        public static void EncodeGetEventInformation(EncodeBuffer buffer, bool send_last, BacnetObjectId lastReceivedObjectIdentifier)
        {
            /* encode optional parameter */
            if (send_last)
                ASN1.encode_context_object_id(buffer, 0, lastReceivedObjectIdentifier.type, lastReceivedObjectIdentifier.instance);
        }

        public static void EncodeGetEventInformationAcknowledge(EncodeBuffer buffer, BacnetGetEventInformationData[] events, bool moreEvents)
        {
            /* service ack follows */
            /* Tag 0: listOfEventSummaries */
            ASN1.encode_opening_tag(buffer, 0);
            foreach(BacnetGetEventInformationData event_data in events)
            {
                /* Tag 0: objectIdentifier */
                ASN1.encode_context_object_id(buffer, 0, event_data.objectIdentifier.type, event_data.objectIdentifier.instance);
                /* Tag 1: eventState */
                ASN1.encode_context_enumerated(buffer, 1, (uint)event_data.eventState);
                /* Tag 2: acknowledgedTransitions */
                ASN1.encode_context_bitstring(buffer, 2, event_data.acknowledgedTransitions);
                /* Tag 3: eventTimeStamps */
                ASN1.encode_opening_tag(buffer, 3);
                for (int i = 0; i < 3; i++)
                {
                    ASN1.bacapp_encode_timestamp(buffer, event_data.eventTimeStamps[i]);
                }
                ASN1.encode_closing_tag(buffer, 3);
                /* Tag 4: notifyType */
                ASN1.encode_context_enumerated(buffer, 4, (uint)event_data.notifyType);
                /* Tag 5: eventEnable */
                ASN1.encode_context_bitstring(buffer, 5, event_data.eventEnable);
                /* Tag 6: eventPriorities */
                ASN1.encode_opening_tag(buffer, 6);
                for (int i = 0; i < 3; i++)
                {
                    ASN1.encode_application_unsigned(buffer, event_data.eventPriorities[i]);
                }
                ASN1.encode_closing_tag(buffer, 6);
            }
            ASN1.encode_closing_tag(buffer, 0);
            ASN1.encode_context_boolean(buffer, 1, moreEvents);
        }

        public static void EncodeLifeSafetyOperation(EncodeBuffer buffer, uint processId, string requestingSrc, uint operation, BacnetObjectId targetObject)
        {
            /* tag 0 - requestingProcessId */
            ASN1.encode_context_unsigned(buffer, 0, processId);
            /* tag 1 - requestingSource */
            ASN1.encode_context_character_string(buffer, 1, requestingSrc);
            /* Operation */
            ASN1.encode_context_enumerated(buffer, 2, operation);
            /* Object ID */
            ASN1.encode_context_object_id(buffer, 3, targetObject.type, targetObject.instance);
        }

        public static void EncodePrivateTransferConfirmed(EncodeBuffer buffer, uint vendorID, uint serviceNumber, byte[] data)
        {
            ASN1.encode_context_unsigned(buffer, 0, vendorID);
            ASN1.encode_context_unsigned(buffer, 1, serviceNumber);
            ASN1.encode_opening_tag(buffer, 2);
            buffer.Add(data, data.Length);
            ASN1.encode_closing_tag(buffer, 2);
        }

        public static void EncodePrivateTransferUnconfirmed(EncodeBuffer buffer, uint vendorID, uint serviceNumber, byte[] data)
        {
            ASN1.encode_context_unsigned(buffer, 0, vendorID);
            ASN1.encode_context_unsigned(buffer, 1, serviceNumber);
            ASN1.encode_opening_tag(buffer, 2);
            buffer.Add(data, data.Length);
            ASN1.encode_closing_tag(buffer, 2);
        }

        public static void EncodePrivateTransferAcknowledge(EncodeBuffer buffer, uint vendorID, uint serviceNumber, byte[] data)
        {
            ASN1.encode_context_unsigned(buffer, 0, vendorID);
            ASN1.encode_context_unsigned(buffer, 1, serviceNumber);
            ASN1.encode_opening_tag(buffer, 2);
            buffer.Add(data, data.Length);
            ASN1.encode_closing_tag(buffer, 2);
        }

        public static void EncodeDeviceCommunicationControl(EncodeBuffer buffer, uint timeDuration, uint enable_disable, string password)
        {
            /* optional timeDuration */
            if (timeDuration > 0)
                ASN1.encode_context_unsigned(buffer, 0, timeDuration);

            /* enable disable */
            ASN1.encode_context_enumerated(buffer, 1, enable_disable);

            /* optional password */
            if (!string.IsNullOrEmpty(password))
            {
                /* FIXME: must be at least 1 character, limited to 20 characters */
                ASN1.encode_context_character_string(buffer, 2, password);
            }
        }

        public static int DecodeDeviceCommunicationControl(byte[] buffer, int offset, int apdu_len, out uint timeDuration, out uint enable_disable, out string password)
        {
            int len = 0;
            byte tag_number = 0;
            uint len_value_type = 0;

            timeDuration = 0;
            enable_disable = 0;
            password = "";

            /* Tag 0: timeDuration, in minutes --optional--
             * But if not included, take it as indefinite,
             * which we return as "very large" */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 0))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out timeDuration);
            }

            /* Tag 1: enable_disable */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 1))
                return -1;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out enable_disable);

            /* Tag 2: password --optional-- */
            if (len < apdu_len)
            {
                if (!ASN1.decode_is_context_tag(buffer, offset + len, 2))
                    return -1;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += ASN1.decode_character_string(buffer, offset + len, apdu_len - (offset + len), len_value_type, out password);
            }

            return len;
        }

        public static void EncodeReinitializeDevice(EncodeBuffer buffer, BacnetReinitializedStates state, string password)
        {
            ASN1.encode_context_enumerated(buffer, 0, (uint)state);

            /* optional password */
            if (!string.IsNullOrEmpty(password))
            {
                /* FIXME: must be at least 1 character, limited to 20 characters */
                ASN1.encode_context_character_string(buffer, 1, password);
            }
        }

        public static int DecodeReinitializeDevice(byte[] buffer, int offset, int apdu_len, out BacnetReinitializedStates state, out string password)
        {
            int len = 0;
            byte tag_number = 0;
            uint len_value_type = 0;
            uint value;

            state = BacnetReinitializedStates.BACNET_REINIT_IDLE;
            password = "";

            /* Tag 0: reinitializedStateOfDevice */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return -1;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out value);
            state = (BacnetReinitializedStates)value;
            /* Tag 1: password - optional */
            if (len < apdu_len)
            {
                if (!ASN1.decode_is_context_tag(buffer, offset + len, 1))
                    return -1;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += ASN1.decode_character_string(buffer, offset + len, apdu_len - (offset + len), len_value_type, out password);
            }

            return len;
        }

        public static void EncodeReadRange(EncodeBuffer buffer, BacnetObjectId object_id, uint property_id, uint arrayIndex, BacnetReadRangeRequestTypes requestType, uint position, DateTime time, int count)
        {
            ASN1.encode_context_object_id(buffer, 0, object_id.type, object_id.instance);
            ASN1.encode_context_enumerated(buffer, 1, property_id);

            /* optional array index */
            if (arrayIndex != ASN1.BACNET_ARRAY_ALL)
            {
                ASN1.encode_context_unsigned(buffer, 2, arrayIndex);
            }

            /* Build the appropriate (optional) range parameter based on the request type */
            switch (requestType)
            {
                case BacnetReadRangeRequestTypes.RR_BY_POSITION:
                    ASN1.encode_opening_tag(buffer, 3);
                    ASN1.encode_application_unsigned(buffer, position);
                    ASN1.encode_application_signed(buffer, count);
                    ASN1.encode_closing_tag(buffer, 3);
                    break;

                case BacnetReadRangeRequestTypes.RR_BY_SEQUENCE:
                    ASN1.encode_opening_tag(buffer, 6);
                    ASN1.encode_application_unsigned(buffer, position);
                    ASN1.encode_application_signed(buffer, count);
                    ASN1.encode_closing_tag(buffer, 6);
                    break;

                case BacnetReadRangeRequestTypes.RR_BY_TIME:
                    ASN1.encode_opening_tag(buffer, 7);
                    ASN1.encode_application_date(buffer, time);
                    ASN1.encode_application_time(buffer, time);
                    ASN1.encode_application_signed(buffer, count);
                    ASN1.encode_closing_tag(buffer, 7);
                    break;

                case BacnetReadRangeRequestTypes.RR_READ_ALL:  /* to attempt a read of the whole array or list, omit the range parameter */
                    break;

                default:
                    break;
            }
        }

        public static int DecodeReadRange(byte[] buffer, int offset, int apdu_len, out BacnetObjectId object_id, out BacnetPropertyReference property, out BacnetReadRangeRequestTypes requestType, out uint position, out DateTime time, out int count)
        {
            int len = 0;
            ushort type = 0;
            byte tag_number = 0;
            uint len_value_type = 0;

            object_id = new BacnetObjectId();
            property = new BacnetPropertyReference();
            requestType = BacnetReadRangeRequestTypes.RR_READ_ALL;
            position = 0;
            time = new DateTime(1, 1, 1);
            count = -1;

            /* Tag 0: Object ID          */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return -1;
            len++;
            len += ASN1.decode_object_id(buffer, offset + len, out type, out object_id.instance);
            object_id.type = (BacnetObjectTypes)type;
            /* Tag 1: Property ID */
            len +=
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -1;
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out property.propertyIdentifier);

            /* Tag 2: Optional Array Index */
            if (len < apdu_len && ASN1.decode_is_context_tag(buffer, offset + len, 0))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out property.propertyArrayIndex);
            }
            else
                property.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;

            /* optional request type */
            if (len < apdu_len)
            {
                len += ASN1.decode_tag_number(buffer, offset + len, out tag_number);    //opening tag
                switch (tag_number)
                {
                    case 3:
                        requestType = BacnetReadRangeRequestTypes.RR_BY_POSITION;
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out position);
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                        len += ASN1.decode_signed(buffer, offset + len, len_value_type, out count);
                        break;
                    case 6:
                        requestType = BacnetReadRangeRequestTypes.RR_BY_SEQUENCE;
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out position);
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                        len += ASN1.decode_signed(buffer, offset + len, len_value_type, out count);
                        break;
                    case 7:
                        requestType = BacnetReadRangeRequestTypes.RR_BY_TIME;
                        DateTime date;
                        len += ASN1.decode_application_date(buffer, offset + len, out date);
                        len += ASN1.decode_application_time(buffer, offset + len, out time);
                        time = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                        len += ASN1.decode_signed(buffer, offset + len, len_value_type, out count);
                        break;
                    default:
                        return -1;  //don't know this type yet
                }
                len += ASN1.decode_tag_number(buffer, offset + len, out tag_number);    //closing tag
            }
            return len;
        }

        public static void EncodeReadRangeAcknowledge(EncodeBuffer buffer, BacnetObjectId object_id, uint property_id, uint arrayIndex, BacnetBitString ResultFlags, uint ItemCount, byte[] application_data, BacnetReadRangeRequestTypes requestType, uint FirstSequence)
        {
            /* service ack follows */
            ASN1.encode_context_object_id(buffer, 0, object_id.type, object_id.instance);
            ASN1.encode_context_enumerated(buffer, 1, property_id);
            /* context 2 array index is optional */
            if (arrayIndex != ASN1.BACNET_ARRAY_ALL)
            {
                ASN1.encode_context_unsigned(buffer, 2, arrayIndex);
            }
            /* Context 3 BACnet Result Flags */
            ASN1.encode_context_bitstring(buffer, 3, ResultFlags);
            /* Context 4 Item Count */
            ASN1.encode_context_unsigned(buffer, 4, ItemCount);
            /* Context 5 Property list - reading the standard it looks like an empty list still
             * requires an opening and closing tag as the tagged parameter is not optional
             */
            ASN1.encode_opening_tag(buffer, 5);
            if (ItemCount != 0)
            {
                buffer.Add(application_data, application_data.Length);
            }
            ASN1.encode_closing_tag(buffer, 5);

            if ((ItemCount != 0) && (requestType != BacnetReadRangeRequestTypes.RR_BY_POSITION) && (requestType != BacnetReadRangeRequestTypes.RR_READ_ALL))
            {
                /* Context 6 Sequence number of first item */
                ASN1.encode_context_unsigned(buffer, 6, FirstSequence);
            }
        }

        // FC
        public static uint DecodeReadRangeAcknowledge(byte[] buffer, int offset, int apdu_len, out byte[] RangeBuffer)
        {
            int len = 0;
            ushort type = 0;
            byte tag_number;
            uint len_value_type = 0;

            BacnetObjectId object_id;
            BacnetPropertyReference property;
            BacnetBitString ResultFlag;
            uint ItemCount;

            RangeBuffer = null;

            /* Tag 0: Object ID          */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return 0;
            len++;
            len += ASN1.decode_object_id(buffer, offset + len, out type, out object_id.instance);
            object_id.type = (BacnetObjectTypes)type;

            /* Tag 1: Property ID */
            len +=ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return 0;
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out property.propertyIdentifier);

            /* Tag 2: Optional Array Index or Tag 3:  BACnet Result Flags */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if ((tag_number == 2) && (len < apdu_len))
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out property.propertyArrayIndex);
            else
                /* Tag 3:  BACnet Result Flags */
                len += ASN1.decode_bitstring(buffer, offset + len, (uint)2, out ResultFlag);

            /* Tag 4 Item Count */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out ItemCount);

            if (!(ASN1.decode_is_opening_tag(buffer, offset + len)))
                return 0;
            len += 1;

            RangeBuffer = new byte[buffer.Length - offset - len - 1];

            Array.Copy(buffer, offset + len, RangeBuffer, 0, RangeBuffer.Length);

            return ItemCount;
        }

        public static void EncodeReadProperty(EncodeBuffer buffer, BacnetObjectId object_id, uint property_id, uint array_index = ASN1.BACNET_ARRAY_ALL)
        {
            if ((int)object_id.type <= ASN1.BACNET_MAX_OBJECT)
            {
                /* check bounds so that we could create malformed
                   messages for testing */
                ASN1.encode_context_object_id(buffer, 0, object_id.type, object_id.instance);
            }
            if (property_id <= (uint)BacnetPropertyIds.MAX_BACNET_PROPERTY_ID)
            {
                /* check bounds so that we could create malformed
                   messages for testing */
                ASN1.encode_context_enumerated(buffer, 1, property_id);
            }
            /* optional array index */
            if (array_index != ASN1.BACNET_ARRAY_ALL)
            {
                ASN1.encode_context_unsigned(buffer, 2, array_index);
            }
        }

        public static int DecodeAtomicWriteFileAcknowledge(byte[] buffer, int offset, int apdu_len, out bool is_stream, out int position)
        {
            int len = 0;
            byte tag_number = 0;
            uint len_value_type = 0;

            is_stream = false;
            position = 0;

            len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number == 0)
            {
                is_stream = true;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
            }
            else if (tag_number == 1)
            {
                is_stream = false;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
            }
            else
                return -1;

            return len;
        }

        public static int DecodeAtomicReadFileAcknowledge(byte[] buffer, int offset, int apdu_len, out bool end_of_file, out bool is_stream, out int position, out uint count, out byte[] target_buffer, out int target_offset)
        {
            int len = 0;
            byte tag_number = 0;
            uint len_value_type = 0;
            int tag_len = 0;

            end_of_file = false;
            is_stream = false;
            position = -1;
            count = 0;
            target_buffer = null;
            target_offset = -1;

            len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN)
                return -1;
            end_of_file = len_value_type > 0;
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 0))
            {
                is_stream = true;
                /* a tag number is not extended so only one octet */
                len++;
                /* fileStartPosition */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT)
                    return -1;
                len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
                /* fileData */
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                len += tag_len;
                if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING)
                    return -1;
                //len += ASN1.decode_octet_string(buffer, offset + len, buffer.Length, target_buffer, target_offset, len_value_type);
                target_buffer = buffer;
                target_offset = offset + len;
                count = len_value_type;
                len += (int)count;
                if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 0))
                    return -1;
                /* a tag number is not extended so only one octet */
                len++;
            }
            else if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
            {
                is_stream = false;
                throw new NotImplementedException("Non stream File transfers are not supported");
                ///* a tag number is not extended so only one octet */
                //len++;
                ///* fileStartRecord */
                //tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                //len += tag_len;
                //if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_SIGNED_INT)
                //    return -1;
                //len += ASN1.decode_signed(buffer, offset + len, len_value_type, out position);
                ///* returnedRecordCount */
                //tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                //len += tag_len;
                //if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT)
                //    return -1;
                //len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out count);
                //for (i = 0; i < count; i++)
                //{
                //    /* fileData */
                //    tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                //    len += tag_len;
                //    if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_OCTET_STRING)
                //        return -1;
                //    len += ASN1.decode_octet_string(buffer, offset + len, buffer.Length, target_buffer, target_offset, len_value_type);
                //}
                //if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                //    return -1;
                ///* a tag number is not extended so only one octet */
                //len++;
            }
            else
                return -1;

            return len;
        }

        /* Added (by Thamer Alsalek) BTL conformance for : BC 135.1: 13.4.3 - Bad Tag Diagnostic & BC 135.1: 13.4.4-A - Missing Required Parameter */
        public static int DecodeReadProperty(byte[] buffer, int offset, int apdu_len, out BacnetObjectId object_id, out BacnetPropertyReference property)
        {
            int len = 0;
            ushort type = 0;
            byte tag_number = 0;
            uint len_value_type = 0;

            object_id = new BacnetObjectId();
            property = new BacnetPropertyReference();

            // must have at least 2 tags , otherwise return reject code: Missing required parameter
            if (apdu_len < 7)
                return -1;

            /* Tag 0: Object ID          */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return -2;

            len++;
            len += ASN1.decode_object_id(buffer, offset + len, out type, out object_id.instance);
            object_id.type = (BacnetObjectTypes)type;
            /* Tag 1: Property ID */

            len +=
                ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -2;

            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out property.propertyIdentifier);

            /* Tag 2: Optional Array Index */
            if (len < apdu_len)
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                if ((tag_number == 2) && (len < apdu_len))
                {
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out property.propertyArrayIndex);
                }
                else
                    return -2;
            }
            else
                property.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;

            if (len < apdu_len)
                /* If something left over now, we have an invalid request */
                return -3;

            return len;
        }

        public static void EncodeReadPropertyAcknowledge(EncodeBuffer buffer, BacnetObjectId object_id, uint property_id, uint array_index, IEnumerable<BacnetValue> value_list)
        {
            /* service ack follows */
            ASN1.encode_context_object_id(buffer, 0, object_id.type, object_id.instance);
            ASN1.encode_context_enumerated(buffer, 1, property_id);
            /* context 2 array index is optional */
            if (array_index != ASN1.BACNET_ARRAY_ALL)
            {
                ASN1.encode_context_unsigned(buffer, 2, array_index);
            }

            /* Value */
            ASN1.encode_opening_tag(buffer, 3);
            foreach (BacnetValue value in value_list)
            {
                ASN1.bacapp_encode_application_data(buffer, value);
            }
            ASN1.encode_closing_tag(buffer, 3);
        }

        public static int DecodeSubscribeCOVPropertyMultipleAcknowledge(byte[] buffer, int offset, int apdu_len, out BacnetObjectId object_id, out BacnetPropertyReference property, out uint error)
        {
            byte tag_number = 0;
            uint len_value_type = 0;
            int tag_len = 0;    /* length of tag decode */
            int len = 0;        /* total length of decodes */

            object_id = new BacnetObjectId();
            property = new BacnetPropertyReference();
            error = 0;

            /* FIXME: check apdu_len against the len during decode   */
            /* Tag 0: Object ID */
            if (!ASN1.decode_is_context_tag(buffer, offset, 0))
                return -1;
            len = 1;
            len += ASN1.decode_object_id(buffer, offset + len, out object_id.type, out object_id.instance);
            /* Tag 1: Property ID */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -1;
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out property.propertyIdentifier);
            /* Tag 2: Optional Array Index */
            tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number == 2)
            {
                len += tag_len;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out property.propertyArrayIndex);
            }
            else
                property.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;
            //error-type
            if (!ASN1.decode_is_context_tag(buffer, offset, 3))
                return -1;
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out error);

            return len;
        }



        public static int DecodeReadPropertyAcknowledge(byte[] buffer, int offset, int apdu_len, out BacnetObjectId object_id, out BacnetPropertyReference property, out IList<BacnetValue> value_list)
        {
            byte tag_number = 0;
            uint len_value_type = 0;
            int tag_len = 0;    /* length of tag decode */
            int len = 0;        /* total length of decodes */

            object_id = new BacnetObjectId();
            property = new BacnetPropertyReference();
            value_list = new List<BacnetValue>();

            /* FIXME: check apdu_len against the len during decode   */
            /* Tag 0: Object ID */
            if (!ASN1.decode_is_context_tag(buffer, offset, 0))
                return -1;
            len = 1;
            len += ASN1.decode_object_id(buffer, offset + len, out object_id.type, out object_id.instance);
            /* Tag 1: Property ID */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -1;
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out property.propertyIdentifier);
            /* Tag 2: Optional Array Index */
            tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number == 2)
            {
                len += tag_len;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out property.propertyArrayIndex);
            }
            else
                property.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;

            /* Tag 3: opening context tag */
            if (ASN1.decode_is_opening_tag_number(buffer, offset + len, 3))
            {
                /* a tag number of 3 is not extended so only one octet */
                len++;

                BacnetValue value;
                while ((apdu_len - len) > 1)
                {
                    tag_len = ASN1.bacapp_decode_application_data(buffer, offset + len, apdu_len + offset, object_id.type, (BacnetPropertyIds)property.propertyIdentifier, out value);
                    if (tag_len < 0) return -1;
                    len += tag_len;
                    value_list.Add(value);
                }
            }
            else
                return -1;

            if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 3))
                return -1;
            len++;

            return len;
        }

        public static void EncodeReadPropertyMultiple(EncodeBuffer buffer, IList<BacnetReadAccessSpecification> properties)
        {
            foreach(BacnetReadAccessSpecification value in properties)
                ASN1.encode_read_access_specification(buffer, value);
        }

        public static void EncodeReadPropertyMultiple(EncodeBuffer buffer, BacnetObjectId object_id, IList<BacnetPropertyReference> properties)
        {
            EncodeReadPropertyMultiple(buffer, new BacnetReadAccessSpecification[] { new BacnetReadAccessSpecification(object_id, properties) });
        }

        public static int DecodeReadPropertyMultiple(byte[] buffer, int offset, int apdu_len, out IList<BacnetReadAccessSpecification> properties)
        {
            int len = 0;
            int tmp;

            List<BacnetReadAccessSpecification> values = new List<BacnetReadAccessSpecification>();
            properties = null;

            while ((apdu_len - len) > 0)
            {
                BacnetReadAccessSpecification value;
                tmp = ASN1.decode_read_access_specification(buffer, offset + len, apdu_len - len, out value);
                if (tmp < 0) return -1;
                len += tmp;
                values.Add(value);
            }

            properties = values;
            return len;
        }

        public static void EncodeReadPropertyMultipleAcknowledge(EncodeBuffer buffer, IList<BacnetReadAccessResult> values)
        {
            foreach (BacnetReadAccessResult value in values)
                ASN1.encode_read_access_result(buffer, value);
        }

        public static int DecodeReadPropertyMultipleAcknowledge(byte[] buffer, int offset, int apdu_len, out IList<BacnetReadAccessResult> values)
        {
            int len = 0;
            int tmp;

            List<BacnetReadAccessResult> _values = new List<BacnetReadAccessResult>();
            values = null;

            while ((apdu_len - len) > 0)
            {
                BacnetReadAccessResult value;
                tmp = ASN1.decode_read_access_result(buffer, offset + len, apdu_len - len, out value);
                if (tmp < 0) return -1;
                len += tmp;
                _values.Add(value);
            }

            values = _values;
            return len;
        }

        public static void EncodeWriteProperty(EncodeBuffer buffer, BacnetObjectId object_id, uint property_id, uint array_index, uint priority, IEnumerable<BacnetValue> value_list)
        {
            ASN1.encode_context_object_id(buffer, 0, object_id.type, object_id.instance);
            ASN1.encode_context_enumerated(buffer, 1, property_id);

            /* optional array index; ALL is -1 which is assumed when missing */
            if (array_index != ASN1.BACNET_ARRAY_ALL)
            {
                ASN1.encode_context_unsigned(buffer, 2, array_index);
            }

            /* propertyValue */
            ASN1.encode_opening_tag(buffer, 3);
            foreach (BacnetValue value in value_list)
            {
                ASN1.bacapp_encode_application_data(buffer, value);
            }
            ASN1.encode_closing_tag(buffer, 3);

            /* optional priority - 0 if not set, 1..16 if set */
            if (priority != ASN1.BACNET_NO_PRIORITY)
            {
                ASN1.encode_context_unsigned(buffer, 4, priority);
            }
        }

        public static int DecodeCOVNotifyUnconfirmed(byte[] buffer, int offset, int apdu_len, out uint subscriberProcessIdentifier, out BacnetObjectId initiatingDeviceIdentifier, out BacnetObjectId monitoredObjectIdentifier, out uint timeRemaining, out ICollection<BacnetPropertyValue> values)
        {
            int len = 0;
            byte tag_number = 0;
            uint len_value = 0;
            uint decoded_value;

            subscriberProcessIdentifier = 0;
            initiatingDeviceIdentifier = new BacnetObjectId();
            monitoredObjectIdentifier = new BacnetObjectId();
            timeRemaining = 0;
            values = null;

            /* tag 0 - subscriberProcessIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 0))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out subscriberProcessIdentifier);
            }
            else
                return -1;

            /* tag 1 - initiatingDeviceIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 1))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_object_id(buffer, offset + len, out initiatingDeviceIdentifier.type, out initiatingDeviceIdentifier.instance);
            }
            else
                return -1;

            /* tag 2 - monitoredObjectIdentifier */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 2))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_object_id(buffer, offset + len, out monitoredObjectIdentifier.type, out monitoredObjectIdentifier.instance);
            }
            else
                return -1;

            /* tag 3 - timeRemaining */
            if (ASN1.decode_is_context_tag(buffer, offset + len, 3))
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                len += ASN1.decode_unsigned(buffer, offset + len, len_value, out timeRemaining);
            }
            else
                return -1;

            /* tag 4: opening context tag - listOfValues */
            if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 4))
                return -1;

            /* a tag number of 4 is not extended so only one octet */
            len++;
            LinkedList<BacnetPropertyValue> _values = new LinkedList<BacnetPropertyValue>();
            while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 4))
            {
                BacnetPropertyValue new_entry = new BacnetPropertyValue();

                /* tag 0 - propertyIdentifier */
                if (ASN1.decode_is_context_tag(buffer, offset + len, 0))
                {
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_enumerated(buffer, offset + len, len_value, out new_entry.property.propertyIdentifier);
                }
                else
                    return -1;

                /* tag 1 - propertyArrayIndex OPTIONAL */
                if (ASN1.decode_is_context_tag(buffer, offset + len, 1))
                {
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value, out new_entry.property.propertyArrayIndex);
                }
                else
                    new_entry.property.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;

                /* tag 2: opening context tag - value */
                if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 2))
                    return -1;

                /* a tag number of 2 is not extended so only one octet */
                len++;
                List<BacnetValue> b_values = new List<BacnetValue>();
                while (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 2))
                {
                    BacnetValue b_value;
                    int tmp = ASN1.bacapp_decode_application_data(buffer, offset + len, apdu_len + offset,monitoredObjectIdentifier.type, (BacnetPropertyIds)new_entry.property.propertyIdentifier, out b_value);
                    if (tmp < 0) return -1;
                    len += tmp;
                    b_values.Add(b_value);
                }
                new_entry.value = b_values;

                /* a tag number of 2 is not extended so only one octet */
                len++;
                /* tag 3 - priority OPTIONAL */
                if (ASN1.decode_is_context_tag(buffer, offset + len, 3))
                {
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value, out decoded_value);
                    new_entry.priority = (byte)decoded_value;
                }
                else
                    new_entry.priority = (byte)ASN1.BACNET_NO_PRIORITY;

                _values.AddLast(new_entry);
            }

            values = _values;

            return len;
        }

        public static int DecodeWriteProperty(byte[] buffer, int offset, int apdu_len, out BacnetObjectId object_id, out BacnetPropertyValue value)
        {
            int len = 0;
            int tag_len = 0;
            byte tag_number = 0;
            uint len_value_type = 0;
            uint unsigned_value = 0;

            object_id = new BacnetObjectId();
            value = new BacnetPropertyValue();

            /* Tag 0: Object ID          */
            if (!ASN1.decode_is_context_tag(buffer, offset + len, 0))
                return -1;
            len++;
            len += ASN1.decode_object_id(buffer, offset + len, out object_id.type, out object_id.instance);
            /* Tag 1: Property ID */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number != 1)
                return -1;
            len += ASN1.decode_enumerated(buffer, offset + len, len_value_type, out value.property.propertyIdentifier);
            /* Tag 2: Optional Array Index */
            /* note: decode without incrementing len so we can check for opening tag */
            tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
            if (tag_number == 2)
            {
                len += tag_len;
                len += ASN1.decode_unsigned(buffer, offset + len, len_value_type, out value.property.propertyArrayIndex);
            }
            else
                value.property.propertyArrayIndex = ASN1.BACNET_ARRAY_ALL;
            /* Tag 3: opening context tag */
            if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 3))
                return -1;
            len++;

            //data
            List<BacnetValue> _value_list = new List<BacnetValue>();
            while ((apdu_len - len) > 1 && !ASN1.decode_is_closing_tag_number(buffer, offset + len, 3))
            {
                BacnetValue b_value;
                int l = ASN1.bacapp_decode_application_data(buffer, offset + len, apdu_len + offset, object_id.type, (BacnetPropertyIds)value.property.propertyIdentifier, out b_value);
                if (l <= 0) return -1;
                len += l;
                _value_list.Add(b_value);
            }
            value.value = _value_list;

            if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 3))
                return -2;
            /* a tag number of 3 is not extended so only one octet */
            len++;
            /* Tag 4: optional Priority - assumed MAX if not explicitly set */
            value.priority = (byte)ASN1.BACNET_MAX_PRIORITY;
            if (len < apdu_len)
            {
                tag_len = ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value_type);
                if (tag_number == 4)
                {
                    len += tag_len;
                    len = ASN1.decode_unsigned(buffer, offset + len, len_value_type, out unsigned_value);
                    if ((unsigned_value >= ASN1.BACNET_MIN_PRIORITY) && (unsigned_value <= ASN1.BACNET_MAX_PRIORITY))
                        value.priority = (byte)unsigned_value;
                    else
                        return -1;
                }
            }

            return len;
        }

        public static void EncodeWritePropertyMultiple(EncodeBuffer buffer, BacnetObjectId object_id, ICollection<BacnetPropertyValue> value_list)
        {
            ASN1.encode_context_object_id(buffer, 0, object_id.type, object_id.instance);
            /* Tag 1: sequence of WriteAccessSpecification */
            ASN1.encode_opening_tag(buffer, 1);

            foreach(BacnetPropertyValue p_value in value_list)
            {
                /* Tag 0: Property */
                ASN1.encode_context_enumerated(buffer, 0, p_value.property.propertyIdentifier);

                /* Tag 1: array index */
                if (p_value.property.propertyArrayIndex != ASN1.BACNET_ARRAY_ALL)
                    ASN1.encode_context_unsigned(buffer, 1, p_value.property.propertyArrayIndex);

                /* Tag 2: Value */
                ASN1.encode_opening_tag(buffer, 2);
                foreach (BacnetValue value in p_value.value)
                {
                    ASN1.bacapp_encode_application_data(buffer, value);
                }
                ASN1.encode_closing_tag(buffer, 2);

                /* Tag 3: Priority */
                if (p_value.priority != ASN1.BACNET_NO_PRIORITY)
                    ASN1.encode_context_unsigned(buffer, 3, p_value.priority);
            }

            ASN1.encode_closing_tag(buffer, 1);
        }

        public static void EncodeWriteObjectMultiple(EncodeBuffer buffer, ICollection<BacnetReadAccessResult> value_list)
        {
            foreach (BacnetReadAccessResult r_value in value_list)
                EncodeWritePropertyMultiple(buffer, r_value.objectIdentifier, r_value.values);
        }
        // By C. Gunter
        // quite the same as DecodeWritePropertyMultiple
        public static int DecodeCreateObject(byte[] buffer, int offset, int apdu_len, out BacnetObjectId object_id, out ICollection<BacnetPropertyValue> values_refs)
        {
            int len = 0;
            byte tag_number;
            uint len_value;
            uint ulVal;
            uint property_id;

            object_id = new BacnetObjectId();
            values_refs = null;

            //object id
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);

            if ((tag_number == 0) && (apdu_len > len))
            {
                apdu_len -= len;
                if (apdu_len >= 4)
                {
                    ushort typenr;

                    len += ASN1.decode_context_object_id(buffer, offset + len, 1, out typenr, out object_id.instance);
                    object_id.type = (BacnetObjectTypes)typenr;
                }
                else
                    return -1;
            }
            else
                return -1;
            if (ASN1.decode_is_closing_tag(buffer, offset + len))
                len++;
            //end objectid

            // No initial values ?
            if (buffer.Length == offset + len)
                return len;

            /* Tag 1: sequence of WriteAccessSpecification */
            if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                return -1;
            len++;

            LinkedList<BacnetPropertyValue> _values = new LinkedList<BacnetPropertyValue>();
            while ((apdu_len - len) > 1)
            {
                BacnetPropertyValue new_entry = new BacnetPropertyValue();

                /* tag 0 - Property Identifier */
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                if (tag_number == 0)
                    len += ASN1.decode_enumerated(buffer, offset + len, len_value, out property_id);
                else
                    return -1;

                /* tag 1 - Property Array Index - optional */
                ulVal = ASN1.BACNET_ARRAY_ALL;
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                if (tag_number == 1)
                {
                    len += ASN1.decode_unsigned(buffer, offset + len, len_value, out ulVal);
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                }
                new_entry.property = new BacnetPropertyReference(property_id, ulVal);

                /* tag 2 - Property Value */
                if ((tag_number == 2) && (ASN1.decode_is_opening_tag(buffer, offset + len - 1)))
                {
                    List<BacnetValue> values = new List<BacnetValue>();
                    while (!ASN1.decode_is_closing_tag(buffer, offset + len))
                    {
                        BacnetValue value;
                        int l = ASN1.bacapp_decode_application_data(buffer, offset + len, apdu_len + offset, object_id.type, (BacnetPropertyIds)property_id, out value);
                        if (l <= 0) return -1;
                        len += l;
                        values.Add(value);
                    }
                    len++;
                    new_entry.value = values;
                }
                else
                    return -1;

                _values.AddLast(new_entry);
            }

            /* Closing tag 1 - List of Properties */
            if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                return -1;
            len++;

            values_refs = _values;

            return len;
        }
        public static int DecodeDeleteObject(byte[] buffer, int offset, int apdu_len, out BacnetObjectId object_id)
        {
            int len = 0;
            byte tag_number;
            uint lenght;
            object_id = new BacnetObjectId();
            ASN1.decode_tag_number_and_value(buffer, offset, out tag_number, out lenght);

            if (tag_number != 12)
                return -1;

            len = 1;
            len += ASN1.decode_object_id(buffer, offset + len, out object_id.type, out object_id.instance);

            if (len == apdu_len) //check if packet was correct!
                return len;
            else
                return -1;
        }

        public static void EncodeCreateObjectAcknowledge(EncodeBuffer buffer, BacnetObjectId object_id)
        {
            ASN1.encode_application_object_id(buffer, object_id.type, object_id.instance);
        }

        public static int DecodeWritePropertyMultiple(byte[] buffer, int offset, int apdu_len,
                    out List<BacnetWriteAccessSpecification> properties)
        {
            int len = 0;
            byte tag_number;
            uint len_value;
            uint ulVal;
            uint property_id;

            properties = new List<BacnetWriteAccessSpecification>();

            while ((apdu_len - len) > 1)
            {
                BacnetObjectId object_id = new BacnetObjectId();

                /* Context tag 0 - Object ID */
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                if ((tag_number == 0) && (apdu_len > len))
                {
                    if (apdu_len - len >= 4)
                    {
                        len += ASN1.decode_object_id(buffer, offset + len, out object_id.type, out object_id.instance);
                    }
                    else
                        return -1;
                }
                else
                    return -1;

                /* Tag 1: sequence of WriteAccessSpecification */
                if (!ASN1.decode_is_opening_tag_number(buffer, offset + len, 1))
                    return -1;
                len++;

                LinkedList<BacnetPropertyValue> _values = new LinkedList<BacnetPropertyValue>();
                while (tag_number != 1)
                {
                    BacnetPropertyValue new_entry = new BacnetPropertyValue();

                    /* tag 0 - Property Identifier */
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    if (tag_number == 0)
                        len += ASN1.decode_enumerated(buffer, offset + len, len_value, out property_id);
                    else
                        return -1;

                    /* tag 1 - Property Array Index - optional */
                    ulVal = ASN1.BACNET_ARRAY_ALL;
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    if (tag_number == 1)
                    {
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value, out ulVal);
                        len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    }

                    new_entry.property = new BacnetPropertyReference(property_id, ulVal);

                    /* tag 2 - Property Value */
                    if ((tag_number == 2) && (ASN1.decode_is_opening_tag(buffer, offset + len - 1)))
                    {
                        List<BacnetValue> values = new List<BacnetValue>();
                        while (!ASN1.decode_is_closing_tag(buffer, offset + len))
                        {
                            BacnetValue value;
                            int l = ASN1.bacapp_decode_application_data(buffer, offset + len, apdu_len + offset,
                                object_id.type, (BacnetPropertyIds)property_id, out value);
                            if (l <= 0) return -1;
                            len += l;
                            values.Add(value);
                        }

                        len++;
                        new_entry.value = values;
                    }
                    else
                        return -1;

                    /* tag 3 - Priority - optional */
                    ulVal = ASN1.BACNET_NO_PRIORITY;
                    len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
                    if (tag_number == 3)
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value, out ulVal);
                    else
                        len--;
                    new_entry.priority = (byte)ulVal;

                    _values.AddLast(new_entry);

                    /* Closing tag 1 - List of Properties */
                    if (!ASN1.decode_is_closing_tag_number(buffer, offset + len, 1))
                        return -1;
                    else
                        tag_number = 1;

                    len++;
                }

                properties.Add(new BacnetWriteAccessSpecification(object_id, _values));

            }

            return len;
        }

        public static void EncodeTimeSync(EncodeBuffer buffer, DateTime time)
        {
            ASN1.encode_application_date(buffer, time);
            ASN1.encode_application_time(buffer, time);
        }

        public static int DecodeTimeSync(byte[] buffer, int offset, int length, out DateTime dateTime)
        {
            int len = 0;
            byte tag_number;
            uint len_value;
            DateTime d_date, t_date;

            dateTime = new DateTime(1, 1, 1);

            /* date */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE)
                return -1;
            len += ASN1.decode_date(buffer, offset + len, out d_date);
            /* time */
            len += ASN1.decode_tag_number_and_value(buffer, offset + len, out tag_number, out len_value);
            if (tag_number != (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME)
                return -1;
            len += ASN1.decode_bacnet_time(buffer, offset + len, out t_date);

            //merge
            dateTime = new DateTime(d_date.Year, d_date.Month, d_date.Day, t_date.Hour, t_date.Minute, t_date.Second, t_date.Millisecond);

            return len;
        }

        public static void EncodeError(EncodeBuffer buffer, BacnetErrorClasses error_class, BacnetErrorCodes error_code)
        {
            ASN1.encode_application_enumerated(buffer, (uint)error_class);
            ASN1.encode_application_enumerated(buffer, (uint)error_code);
        }

        public static int DecodeError(byte[] buffer, int offset, int length, out BacnetErrorClasses error_class, out BacnetErrorCodes error_code)
        {
            int org_offset = offset;
            uint tmp;

            byte tag_number;
            uint len_value_type;
            offset += ASN1.decode_tag_number_and_value(buffer, offset, out tag_number, out len_value_type);
            /* FIXME: we could validate that the tag is enumerated... */
            offset += ASN1.decode_enumerated(buffer, offset, len_value_type, out tmp);
            error_class = (BacnetErrorClasses)tmp;
            offset += ASN1.decode_tag_number_and_value(buffer, offset, out tag_number, out len_value_type);
            /* FIXME: we could validate that the tag is enumerated... */
            offset += ASN1.decode_enumerated(buffer, offset, len_value_type, out tmp);
            error_code = (BacnetErrorCodes)tmp;

            return offset - org_offset;
        }

        public static void EncodeLogRecord(EncodeBuffer buffer, BacnetLogRecord record)
        {
            /* Tag 0: timestamp */
            ASN1.encode_opening_tag(buffer, 0);
            ASN1.encode_application_date(buffer, record.timestamp);
            ASN1.encode_application_time(buffer, record.timestamp);
            ASN1.encode_closing_tag(buffer, 0);

            /* Tag 1: logDatum */
            if (record.type != BacnetTrendLogValueType.TL_TYPE_NULL)
            {

                if (record.type == BacnetTrendLogValueType.TL_TYPE_ERROR)
                {
                    ASN1.encode_opening_tag(buffer, 1);
                    ASN1.encode_opening_tag(buffer, 8);
                    BacnetError err = record.GetValue<BacnetError>();
                    Services.EncodeError(buffer, err.error_class, err.error_code);
                    ASN1.encode_closing_tag(buffer, 8);
                    ASN1.encode_closing_tag(buffer, 1);
                    return;
                }

                ASN1.encode_opening_tag(buffer, 1);
                EncodeBuffer tmp1 = new System.IO.BACnet.Serialize.EncodeBuffer();
                switch (record.type)
                {
                    case BacnetTrendLogValueType.TL_TYPE_ANY:
                        throw new NotImplementedException();
                    case BacnetTrendLogValueType.TL_TYPE_BITS:
                        ASN1.encode_bitstring(tmp1, record.GetValue<BacnetBitString>());
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_BOOL:
                        tmp1.Add(record.GetValue<bool>() ? (byte)1 : (byte)0);
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_DELTA:
                        ASN1.encode_bacnet_real(tmp1, record.GetValue<float>());
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_ENUM:
                        ASN1.encode_application_enumerated(tmp1, record.GetValue<uint>());
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_REAL:
                        ASN1.encode_bacnet_real(tmp1, record.GetValue<float>());
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_SIGN:
                        ASN1.encode_bacnet_signed(tmp1, record.GetValue<int>());
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_STATUS:
                        ASN1.encode_bitstring(tmp1, record.GetValue<BacnetBitString>());
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_UNSIGN:
                        ASN1.encode_bacnet_unsigned(tmp1, record.GetValue<uint>());
                        break;
                }
                ASN1.encode_tag(buffer, (byte)record.type, false, (uint)tmp1.offset);
                buffer.Add(tmp1.buffer, tmp1.offset);
                ASN1.encode_closing_tag(buffer, 1);
            }

            /* Tag 2: status */
            if (record.statusFlags.bits_used > 0)
            {
                ASN1.encode_opening_tag(buffer, 2);
                ASN1.encode_application_bitstring(buffer, record.statusFlags);
                ASN1.encode_closing_tag(buffer, 2);
            }
        }

        public static int DecodeLogRecord(byte[] buffer, int offset, int length, int n_curves, out BacnetLogRecord[] records)
        {
            int len = 0;
            byte tag_number;
            uint len_value;
            records = new BacnetLogRecord[n_curves];

            DateTime date;
            DateTime time;

            len += ASN1.decode_tag_number(buffer, offset + len, out tag_number);
            if (tag_number != 0) return -1;

            // Date and Time in Tag 0
            len += ASN1.decode_application_date(buffer, offset+len, out date);
            len += ASN1.decode_application_time(buffer, offset + len, out time);

            DateTime dt = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond);

            if (!(ASN1.decode_is_closing_tag(buffer, offset + len))) return -1;
            len++;

            // Value or error in Tag 1
            len += ASN1.decode_tag_number(buffer, offset+len, out tag_number);
            if (tag_number != 1) return -1;

            byte ContextTagType = 0;

            // Not test for TrendLogMultiple
            // Seems to be encoded like this somewhere in an Ashrae document
            for (int CurveNumber = 0; CurveNumber < n_curves; CurveNumber++)
            {
                len += ASN1.decode_tag_number_and_value(buffer, offset + len, out ContextTagType, out len_value);
                records[CurveNumber] = new BacnetLogRecord();
                records[CurveNumber].timestamp = dt;
                records[CurveNumber].type = (BacnetTrendLogValueType)ContextTagType;

                switch ((BacnetTrendLogValueType)ContextTagType)
                {
                    case BacnetTrendLogValueType.TL_TYPE_STATUS:
                        BacnetBitString sval;
                        len += ASN1.decode_bitstring(buffer, offset + len, len_value, out sval);
                        records[CurveNumber].Value = sval;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_BOOL:
                        records[CurveNumber].Value = buffer[offset + len] > 0 ? true : false;
                        len++;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_REAL:
                        float rval;
                        len += ASN1.decode_real(buffer, offset + len, out rval);
                        records[CurveNumber].Value = rval;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_ENUM:
                        uint eval;
                        len += ASN1.decode_enumerated(buffer, offset + len, len_value, out eval);
                        records[CurveNumber].Value = eval;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_SIGN:
                        int ival;
                        len += ASN1.decode_signed(buffer, offset + len, len_value, out ival);
                        records[CurveNumber].Value = ival;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_UNSIGN:
                        uint uinval;
                        len += ASN1.decode_unsigned(buffer, offset + len, len_value, out uinval);
                        records[CurveNumber].Value = uinval;
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_ERROR:
                        BacnetErrorClasses Errclass;
                        BacnetErrorCodes Errcode;
                        len += DecodeError(buffer, offset + len, length, out Errclass, out Errcode);
                        records[CurveNumber].Value = new BacnetError(Errclass, Errcode);
                        len++; // Closing Tag 8
                        break;
                    case BacnetTrendLogValueType.TL_TYPE_NULL:
                        len++;
                        records[CurveNumber].Value = null;
                        break;
                    // Time change (Automatic or Synch time) Delta in seconds
                    case BacnetTrendLogValueType.TL_TYPE_DELTA:
                        float dval;
                        len += ASN1.decode_real(buffer, offset + len, out dval);
                        records[CurveNumber].Value = dval;
                        break;
                    // No way to handle these data types, sure it's the end of this download !
                    case BacnetTrendLogValueType.TL_TYPE_ANY:
                        throw new NotImplementedException();
                    case BacnetTrendLogValueType.TL_TYPE_BITS:
                        BacnetBitString bval;
                        len += ASN1.decode_bitstring(buffer, offset + len, len_value, out bval);
                        records[CurveNumber].Value = bval;
                        break;
                    default:
                        return 0;
                }
            }

            if (!(ASN1.decode_is_closing_tag(buffer, offset+len))) return -1;
            len++;

            // Optional Tag 2
            if (len < length)
            {
                int l = ASN1.decode_tag_number(buffer, offset+len, out tag_number);
                if (tag_number == 2)
                {
                    len += l;
                    BacnetBitString StatusFlags;
                    len += ASN1.decode_bitstring(buffer, offset+len, 2, out StatusFlags);

                    //set status to all returns
                    for (int CurveNumber = 0; CurveNumber < n_curves; CurveNumber++)
                        records[CurveNumber].statusFlags = StatusFlags;
                }
            }

            return len;
        }
    }
}
