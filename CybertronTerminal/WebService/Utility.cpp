#include "Utility.h"

const char* ConvertWCharToMultiByte( const WCHAR *wBuf, __out char *outBuf )
{
	int iSize = 0;
	iSize = WideCharToMultiByte( CP_UTF8, 0, wBuf, -1, NULL, 0, NULL, NULL );
	if ( !iSize )
		return nullptr;
	WideCharToMultiByte( CP_UTF8, 0, wBuf, -1, outBuf, iSize, NULL, NULL );

	return outBuf;
}

const WCHAR* ConverMultiByteToWChar( const char *cBuf, __out WCHAR *outBuf )
{
	int iSize = 0;
	iSize = MultiByteToWideChar( CP_ACP, 0, cBuf, -1, NULL, 0 );
	if ( !iSize )
		return nullptr;
	MultiByteToWideChar( CP_ACP, 0, cBuf, -1, outBuf, iSize );

	return outBuf;
}

const char* GetToken()
{
	static string strToken;
	SYSTEMTIME st = {0};
	GetLocalTime( &st );

	char strTmp[MAX_CHAR_NUM] = {0};
	sprintf_s( strTmp, MAX_CHAR_NUM, "%d-%d-%d %d:%d:%d:%d", st.wMonth, st.wDay, st.wYear, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds );

	strToken = strTmp;
	return strToken.c_str();
}

const char* ReplaceSpecificChar( char *str, const char cFindChar, const char cReplaceChar )
{
	if ( nullptr == str )
		return nullptr;

	UINT uCount = 0;
	while ( str[uCount] )
	{
		if ( str[uCount] == cFindChar )
			str[uCount] = cReplaceChar;

		++uCount;
	}

	return str;
}

const char* ReplaceSpecificChar( string &str, char cFindChar, char cReplaceChar )
{
	std::size_t size = str.length();
	if ( 0 == size )
		return nullptr;

	for ( std::size_t i = 0; i < size; ++i )
	{
		if ( cFindChar == str[i] )
			str[i] = cReplaceChar;
	}

	return str.c_str();
}

const char* RemoveGuidBrace( char *strGuid, UINT uSize )
{
	char cTmp[MAX_PATH] = {0};
	memcpy_s( cTmp, MAX_PATH, strGuid + 1, strlen( strGuid ) - 2 );
	strcpy_s( strGuid, uSize, cTmp );

	return strGuid;
}

const char* ReplaceSpecificStr( string &srcStr, string &newVal, string &oldVal )
{
	std::size_t pos = string::npos;
	while ( ( pos = srcStr.find( oldVal ) ) != string::npos )
	{
		srcStr.replace( pos, oldVal.length(), newVal );
	}

	return srcStr.c_str();
}
