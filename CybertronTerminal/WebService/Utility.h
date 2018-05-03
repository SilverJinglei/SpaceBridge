#pragma once

#include "PreDefine.h"

const char* ConvertWCharToMultiByte( const WCHAR *wBuf, __out char *outBuf );
const WCHAR* ConverMultiByteToWChar( const char *cBuf, __out WCHAR *outBuf );
const char* RemoveGuidBrace( char *strGuid, UINT uSize );
const char* ReplaceSpecificChar( char *str, const char cFindChar, const char cReplaceChar );
const char* ReplaceSpecificChar( string &str, const char cFindChar, const char cReplaceChar );
const char* ReplaceSpecificStr( string &srcStr, string &newVal, string &oldVal );
const char* GetToken();