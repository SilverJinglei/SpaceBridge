#pragma once

#include "PreDefine.h"

//====General node
#define NODE_CLASS "Class"
#define NODE_NAME "Name"
#define NODE_HAS_ANIMATION "HasAnimation"
#define NODE_PARAM "Parameters"
#define NODE_TOKEN "Token"
#define NODE_IS_ONE_WAY "IsOneWay"
#define NODE_IS_EXCEPTION "IsException"
#define NODE_TYPE "Type"//3 kinds: 'Operation' 'Result'
#define NODE_VALUE "Value"
#define FUNC_TYPE_OPERATION "Operation"
#define FUNC_TYPE_OPERATION_T (_T( "Operation" ))
#define FUNC_RESULT "Result"
//====VRP data node
#define NODE_MODELS "Models"
//We named it special, because of the name was used in VRP header
#define NODE_GUID_STR "Guid"
#define NODE_GUIDS_STR "Guids"
#define NODE_GROUPS "Groups"
//====socket
#define NODE_SOCK_IDX "SocketIdx"

enum eParamType
{
	TYPE_INT,
	TYPE_FLOAT,
	TYPE_DOUBLE,
	TYPE_UINT,
	TYPE_BYTE,
	TYPE_BOOL,
	TYPE_STR
};

class BaseFunction
{
public:
	BaseFunction( void *pData );
	virtual ~BaseFunction();

	const char* GenerateExceptionStr( const char *description );
	//this function will help you generate the constant json node automatically
	//but you need generate the Value node yourself, because it's not constant
	void GenerateResultStr( Json::Value &returnValue );
	void ParseJsonValue( char *addr );
	const void* GetValuePtr( UINT idx );
	HRESULT GenerateInvokeFuncDataStr( UINT uParamNum, ... );

	//properties
	void SetFuncName( const TCHAR *name );
	const TCHAR* GetFuncName();
	void SetClassName( const TCHAR *name );
	const TCHAR* GetClassName();
	void SetFuncType( const TCHAR *type );
	const TCHAR* GetFuncType();
	void SetReturnStr( const string &str );
	const string& GetReturnStr();
	const string& GetExceptionStr();
	void SetParamNum( UINT num );
	UINT GetParamNum();
	const char* GetFuncDataStr();
	UINT GetFuncDataSize();
	const string& GetTokenStr();
	void SetIsOneWay( bool bFlag );
	bool GetIsOneWay();
	void SetFuncValid( bool bValid );
	bool GetFuncValid();
	void AddFuncParam( eParamType paramType, const char *paramName, bool bParseAsJson );
	void SetFuncBaseInfo( const TCHAR *className, const TCHAR *funcName, const TCHAR *funcType );

	virtual const char* operator()( char *addr ) = 0;

protected:
	HRESULT GenerateInvokeFuncDataStrInside( void *pAddr );

private:
	class Impl;
	shared_ptr<Impl>				m_Impl;
};