#pragma once

/************************************************************************
This is my own log system,simple feature and simple usage
If i need more features,just modify it myself

Modify count:2

Support features:
non-exclusive file open
thread safe
unicode&ascii
************************************************************************/

#include <Windows.h>
#include <iostream>
#include <tchar.h>
#include <memory>

namespace Devil
{
#define MAX_CHAR_BUF 2048

	enum eLogLevel
	{
		LOGLEVEL_NORMAL,
		LOGLEVEL_WARNING,
		LOGLEVEL_ERROR,
		LOGLEVEL_CRITICAL
	};

	class LogSystem
	{
	public:		
		~LogSystem();

		static LogSystem&					Singleton();
		static LogSystem*					SingletonPtr();

		HRESULT									Init( const TCHAR *fileName = NULL, bool bEnable = true );
		HRESULT									Log( const TCHAR *logMessage, 
																	eLogLevel logLevel = LOGLEVEL_NORMAL, 
																	bool bTimeFlag = true, 
																	bool bFullTimeFlag = false, 
																	bool bLvFlag = true );

	protected:
		LogSystem();
		HRESULT									MakeSureFilePathExists( const TCHAR *strPath );

	private:
		static LogSystem*								m_Singleton;
		bool														m_bInitFlag;
		bool														m_bEnable;
		FILE*														m_pFile;
		TCHAR													m_FileName[MAX_PATH];
		TCHAR													m_LogMessage[MAX_CHAR_BUF];
		CRITICAL_SECTION								m_CsWrite;
	};
}

_declspec( dllexport ) HRESULT _stdcall DevilLogInit( const TCHAR *fileName, bool bEnable = true );
__declspec( dllexport ) HRESULT _stdcall DevilLog( const TCHAR *logMessage, Devil::eLogLevel lv = Devil::LOGLEVEL_NORMAL );