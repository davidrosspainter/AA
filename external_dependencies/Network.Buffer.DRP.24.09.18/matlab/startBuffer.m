function hdr = startBuffer( direct, host, port, nChannels, nScans, fs, dataType )

% !taskkill /F /IM buffer.exe /T
% !taskkill /F /IM cmd.exe /T


% message.h
% #define DATATYPE_CHAR    (UINT32_T)0
% #define DATATYPE_UINT8   (UINT32_T)1
% #define DATATYPE_UINT16  (UINT32_T)2
% #define DATATYPE_UINT32  (UINT32_T)3
% #define DATATYPE_UINT64  (UINT32_T)4
% #define DATATYPE_INT8    (UINT32_T)5
% #define DATATYPE_INT16   (UINT32_T)6
% #define DATATYPE_INT32   (UINT32_T)7
% #define DATATYPE_INT64   (UINT32_T)8
% #define DATATYPE_FLOAT32 (UINT32_T)9
% #define DATATYPE_FLOAT64 (UINT32_T)10

switch dataType
    case 9 % single
        dataBytes = 4;
    case 10 % double
        dataBytes = 8;
    otherwise
        error('undefined dataType')
end

% https://golang.org/ref/spec#Size_and_alignment_guarantees
% type                                 size in bytes
% byte, uint8, int8                     1
% uint16, int16                         2
% uint32, int32, float32                4
% uint64, int64, float64, complex64     8
% complex128                           16

system( ['cd "' direct.realtimeHack '" & buffer.exe ' num2str( host ) ' ' num2str( port ) ' -&'] ); % start buffer - necessary!

hdr.nchans = uint32( nChannels );
hdr.nsamples = uint32( nScans );
hdr.nevents = 0;
hdr.fsample = single( fs );
hdr.data_type = uint32( dataType );
hdr.bufsize = uint32( hdr.nchans * hdr.nsamples * dataBytes );

buffer( 'put_hdr', hdr, host, port )