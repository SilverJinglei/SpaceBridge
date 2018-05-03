#pragma once

#include "BaseFunctionClass.h"

using std::map;

class FuncMgr
{
public:
	~FuncMgr();

	static FuncMgr* SingletonPtr();
	static FuncMgr& Singleton();

	//when we add func we will distinguish them(in case we have same name method, but not same class name) with class name automatically
	HRESULT AddFunc( BaseFunction *pFunction );
	HRESULT RemoveFunc( TCHAR *className, TCHAR *funcName );
	
	//************************************
	// Method:    ExecuteFunc
	// FullName:  FuncMgr::ExecuteFunc
	// Access:    public 
	// Returns:   HRESULT
	// Qualifier:
	// Parameter: int i -- just set this int to any value, it's used for va_list
	// Parameter: TCHAR * funcName
	// Parameter: ...
	//************************************
	HRESULT ExecuteFunc( int i, const char *className, const char *funcName, ... );
	HRESULT ParseJson( Json::Value &root );
	HRESULT GetOneReturnValueFromList( string &val );

protected:
	FuncMgr();

private:
	static FuncMgr												*m_singleton;
	class														Impl;
	shared_ptr<Impl>											m_impl;
};