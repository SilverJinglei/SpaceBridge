#pragma once

#include <windows.h>
#include <tchar.h>
#include <memory>
#include <vector>
#include <string>
#include <map>
#include <fstream>
#include <iostream>
#include <assert.h>
#include <TlHelp32.h>
#include <DbgHelp.h>
#include "DevilLog.h"
#include "json/json.h"

using std::auto_ptr;
using std::vector;
using std::string;
using std::wstring;
using std::map;
using std::multimap;
using std::make_pair;
using std::fstream;
using std::ifstream;
using std::ofstream;
using std::tr1::shared_ptr;
using std::ios;


#ifdef _DEBUG
#pragma comment(lib, "devillog32d")
#pragma comment(lib, "lib_jsond.lib")
#else
#pragma comment(lib, "devillog32")
#pragma comment(lib, "lib_json.lib")
#endif
#pragma comment(lib, "dbghelp.lib")

#define MAX_CHAR_NUM 4096
#define SAFE_DELETE(p) if( p ) { delete p; p = nullptr; }
//====General str
#define STR_TRUE "true"
#define STR_FALSE "false"
#define STR_MILITARY_PATH "Military\\"
#define STR_TEMP_VRP_FILE "Temp.vrp"
#define STR_TEMP_PNG_FILE "Temp.png"

#define LOG_ERROR( msg )\
	TCHAR strTmp[MAX_CHAR_NUM] = {0};\
	_stprintf_s( strTmp, MAX_CHAR_NUM, _T( "##msg##[%d]" ), GetLastError() );\
	DevilLog( strTmp, Devil::LOGLEVEL_ERROR );

#define LOG_NORMAL( msg )\
	TCHAR strTmp[MAX_CHAR_NUM] = {0};\
	_stprintf_s( strTmp, MAX_CHAR_NUM, _T( "##msg##" ) );\
	DevilLog( strTmp );

#define MAKE_PROPERTY( name, type, member )\
	void Set##name( type t )\
	{\
		member = t;\
	}\
	type Get##name()\
	{\
		return member;\
	}

#define MAKE_TCHAR_PROPERTY( name, member )\
	void Set##name( TCHAR *pTC )\
	{\
		if ( nullptr != pTC )\
		{\
			_tcscpy_s( member, MAX_PATH, pTC );\
		}\
	}\
	TCHAR* Get##name()\
	{\
		return member;\
	}