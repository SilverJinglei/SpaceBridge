#include "BaseFunctionClass.h"
#include "Utility.h"

class BaseFunction::Impl
{
public:
	explicit Impl( void *pData );
	~Impl();

	const char* GenerateExceptionStr( const char *description );
	//this function will help you generate the constant json node automatically
	//but you need generate the Value node yourself, because it's not constant
	void GenerateResultStr( Json::Value &returnValue );
	void ParseJsonValue( char *addr );
	const void* GetValuePtr( UINT idx );
	HRESULT GenerateInvokeFuncDataStr( void *pAddr );
	HRESULT GenerateInvokeFuncDataStrInside( void *pAddr );

	//properties
	inline void SetFuncName( const TCHAR *name );
	inline const TCHAR* GetFuncName();
	inline void SetClassName( const TCHAR *name );
	inline const TCHAR* GetClassName();
	inline void SetFuncType( const TCHAR *type );
	inline const TCHAR* GetFuncType();
	inline void SetReturnStr( const string &str );
	inline const string& GetReturnStr();
	inline const string& GetExceptionStr();
	inline void SetParamNum( UINT num );
	inline UINT GetParamNum();
	inline const char* GetFuncDataStr();
	inline UINT GetFuncDataSize();
	inline const string& GetTokenStr();
	inline void SetIsOneWay( bool bFlag );
	inline bool GetIsOneWay();
	inline void SetFuncValid( bool bValid );
	inline bool GetFuncValid();
	inline void AddFuncParam( eParamType paramType, const char *paramName, bool bParseAsJson );
	inline void SetFuncBaseInfo( const TCHAR *className, const TCHAR *funcName, const TCHAR *funcType );

protected:

private:
	TCHAR							m_className[MAX_PATH];
	TCHAR							m_funcName[MAX_PATH];
	TCHAR							m_funcType[MAX_PATH];
	UINT							m_paramNum;
	int								m_iTmp;
	float							m_fTmp;
	bool							m_bTmp;
	string							m_strTmp;
	string							m_token;
	UINT							m_socketIdx;
	void							*m_data;
	string							m_strFuncData;
	string							m_strReturnValue;
	string							m_strException;
	bool							m_isOneWay;
	bool							m_invokeFuncValid;
	typedef vector<bool>			ParamJsonFlagVec;
	ParamJsonFlagVec				m_paramJsonFlagVec;
	typedef vector<eParamType>		ParamTypeVec;
	ParamTypeVec					m_paramTypeVec;
	typedef vector<string>			ParamMapNameVec;
	//this vec map to the paramTypeVec
	ParamMapNameVec					m_paramMapNameVec;
	typedef vector<string>			ParamVec;
	ParamVec						m_paramVec;
};

BaseFunction::Impl::Impl( void *pData )
	:m_iTmp( 0 )
	, m_fTmp( 0.0f )
	, m_bTmp( false )
	, m_data( pData )
	, m_isOneWay( false )
	, m_invokeFuncValid( false )
	, m_socketIdx( 0 )
{

}

BaseFunction::Impl::~Impl()
{

}

void BaseFunction::Impl::SetIsOneWay(bool bFlag)
{
	m_isOneWay = bFlag;
}

bool BaseFunction::Impl::GetIsOneWay()
{
	return m_isOneWay;
}

void BaseFunction::Impl::SetFuncName(const TCHAR *name)
{
	if ( nullptr == name )
		return;

	_tcscpy_s( m_funcName, MAX_PATH, name );
}

const TCHAR* BaseFunction::Impl::GetFuncName()
{
	return m_funcName;
}

void BaseFunction::Impl::SetClassName(const TCHAR *name)
{
	if ( nullptr == name )
		return;

	_tcscpy_s( m_className, MAX_PATH, name );
}

const TCHAR* BaseFunction::Impl::GetClassName()
{
	return m_className;
}

void BaseFunction::Impl::SetFuncType(const TCHAR *type)
{
	if ( nullptr == type )
		return;

	_tcscpy_s( m_funcType, MAX_PATH, type );
}

const TCHAR* BaseFunction::Impl::GetFuncType()
{
	return m_funcType;
}

void BaseFunction::Impl::SetReturnStr(const string &str)
{
	m_strReturnValue = str;
}

const string& BaseFunction::Impl::GetReturnStr()
{
	return m_strReturnValue;
}

const string& BaseFunction::Impl::GetExceptionStr()
{
	return m_strException;
}

void BaseFunction::Impl::SetParamNum(UINT num)
{
	m_paramNum = num;
}

UINT BaseFunction::Impl::GetParamNum()
{
	return m_paramNum;
}

const char* BaseFunction::Impl::GetFuncDataStr()
{
	return m_strFuncData.c_str();
}

UINT BaseFunction::Impl::GetFuncDataSize()
{
	return m_strFuncData.size();
}

const string& BaseFunction::Impl::GetTokenStr()
{
	return m_token;
}

void BaseFunction::Impl::SetFuncValid(bool bValid)
{
	m_invokeFuncValid = bValid;
}

bool BaseFunction::Impl::GetFuncValid()
{
	return m_invokeFuncValid;
}

void BaseFunction::Impl::AddFuncParam(eParamType paramType, const char *paramName, bool bParseAsJson)
{
	m_paramTypeVec.push_back( paramType );
	m_paramMapNameVec.push_back( paramName );
	m_paramJsonFlagVec.push_back( bParseAsJson );
}

void BaseFunction::Impl::SetFuncBaseInfo( const TCHAR *className, const TCHAR *funcName, const TCHAR *funcType )
{
	SetClassName( className );
	SetFuncName( funcName );
	SetFuncType( funcType );
}

const char* BaseFunction::Impl::GenerateExceptionStr(const char *description)
{
	Json::Value returnValue;
	returnValue[NODE_TOKEN] = m_token;
	returnValue[NODE_TYPE] = FUNC_RESULT;
	returnValue[NODE_IS_EXCEPTION] = true;
	returnValue[NODE_VALUE] = description;

	Json::FastWriter writer;
	return ( m_strException = writer.write( returnValue ) ).c_str();

	char cTmp[MAX_PATH] = {0};
	sprintf_s( cTmp, MAX_PATH, "\"%s\":%d,", NODE_SOCK_IDX, m_socketIdx );
	m_strException.insert( 1, cTmp );
}

void BaseFunction::Impl::GenerateResultStr(Json::Value &returnValue)
{
	returnValue[NODE_TOKEN] = m_token;
	returnValue[NODE_TYPE] = FUNC_RESULT;
	returnValue[NODE_IS_EXCEPTION] = false;

	Json::FastWriter writer;
	m_strReturnValue = writer.write( returnValue );

	char cTmp[MAX_PATH] = {0};
	sprintf_s( cTmp, MAX_PATH, "\"%s\":%d,", NODE_SOCK_IDX, m_socketIdx );
	m_strReturnValue.insert( 1, cTmp );
}

void BaseFunction::Impl::ParseJsonValue(char *addr)
{
	m_paramVec.clear();
	va_list param = addr;

	Json::Value root = va_arg( param, Json::Value );
	va_end( param );

	Json::FastWriter writer;
	UINT uSize = root[NODE_PARAM].size();
	//string strTmp = writer.write( root );
	for ( UINT i = 0; i < uSize; ++i )
	{
		if ( m_paramJsonFlagVec[i] )
		{
			m_paramVec.push_back( writer.write( root[NODE_PARAM][m_paramMapNameVec[i]] ) );
		}
		else
		{
			m_paramVec.push_back( root[NODE_PARAM][m_paramMapNameVec[i]].asCString() );
		}
	}

	m_token = root[NODE_TOKEN].asString();
	m_socketIdx = root[NODE_SOCK_IDX].asUInt();
}

const void* BaseFunction::Impl::GetValuePtr(UINT idx)
{
	if ( m_paramVec.size() == 0 || m_paramTypeVec.size() == 0 )
	{
		DevilLog( _T( "get value ptr failed" ), Devil::LOGLEVEL_ERROR );
		return nullptr;
	}

	eParamType type = m_paramTypeVec[idx - 1];
	string strValue = m_paramVec[idx - 1];

	switch( type )
	{
	case TYPE_INT:
		{
			m_iTmp = atoi( strValue.c_str() );
			return &m_iTmp;
		}
		break;
	case TYPE_FLOAT:
		{
			m_fTmp = ( float )atof( strValue.c_str() );
			return &m_fTmp;
		}
		break;
	case TYPE_STR:
		{
			m_strTmp = strValue;
			return m_strTmp.c_str();
		}
		break;
	case TYPE_BOOL:
		{
			m_bTmp = ( atoi( strValue.c_str() ) ) ? true : false;
			return &m_bTmp;
		}
		break;
	default:
		{
			return nullptr;
		}
	}
}

HRESULT BaseFunction::Impl::GenerateInvokeFuncDataStr( void *pAddr )
{
	GenerateInvokeFuncDataStrInside( pAddr );

	return S_OK;
}

HRESULT BaseFunction::Impl::GenerateInvokeFuncDataStrInside(void *pAddr)
{
	va_list param = static_cast<va_list>( pAddr );

	vector<string> strList;
	for ( int i = 0; i < m_paramNum; ++i )
	{
		strList.push_back( string( va_arg( param, char* ) ) );
	}

	//the sequence below must like this, because c# need this sequence to do the serialize
	char cTmp[MAX_PATH] = {0};
	Json::Value root, tmpJson, paramVal;
	root[NODE_TOKEN] = GetToken();
	root[NODE_TYPE] = ConvertWCharToMultiByte( GetFuncType(), cTmp );
	root[NODE_IS_ONE_WAY] = ( GetIsOneWay() ? STR_TRUE : STR_FALSE );
	Json::Reader reader;

	vector<string>::size_type size = strList.size();
	for ( string::size_type i = 0; i < size; ++i )
	{
		Json::Value val;
		reader.parse( strList[i].c_str(), val );
		if ( m_paramJsonFlagVec[i] )
		{
			Json::Value val;
			reader.parse( strList[i].c_str(), val );
			paramVal[m_paramMapNameVec[i]] = val;
		} 
		else
		{
			paramVal[m_paramMapNameVec[i]] = strList[i].c_str();
		}
	}

	root[NODE_NAME] = ConvertWCharToMultiByte( GetFuncName(), cTmp );
	root[NODE_CLASS] = ConvertWCharToMultiByte( GetClassName(), cTmp );
	root[NODE_PARAM] = paramVal;

	Json::FastWriter writer;
	m_strFuncData = writer.write( root );

	return S_OK;
}

BaseFunction::BaseFunction( void *pData )
	:m_Impl( new Impl( pData ) )
{
}

BaseFunction::~BaseFunction()
{

}

const char* BaseFunction::GenerateExceptionStr( const char *description )
{
	return m_Impl->GenerateExceptionStr( description );
}

void BaseFunction::GenerateResultStr( Json::Value &returnValue )
{
	return m_Impl->GenerateResultStr( returnValue );
}

void BaseFunction::ParseJsonValue( char *addr )
{
	m_Impl->ParseJsonValue( addr );
}

HRESULT BaseFunction::GenerateInvokeFuncDataStr( UINT uParamNum, ... )
{
	va_list param;
	va_start( param, uParamNum );
	return m_Impl->GenerateInvokeFuncDataStr( param );
}

HRESULT BaseFunction::GenerateInvokeFuncDataStrInside( void *pAddr )
{
	return m_Impl->GenerateInvokeFuncDataStrInside( pAddr );
}

const void* BaseFunction::GetValuePtr( UINT idx )
{
	return m_Impl->GetValuePtr( idx );
}

void BaseFunction::SetFuncName(const TCHAR *name)
{
	m_Impl->SetFuncName( name );
}

const TCHAR* BaseFunction::GetFuncName()
{
	return m_Impl->GetFuncName();
}

void BaseFunction::SetClassName(const TCHAR *name)
{
	return m_Impl->SetClassName( name );
}

const TCHAR* BaseFunction::GetClassName()
{
	return m_Impl->GetClassName();
}

void BaseFunction::SetFuncType(const TCHAR *type)
{
	m_Impl->SetFuncType( type );
}

const TCHAR* BaseFunction::GetFuncType()
{
	return m_Impl->GetFuncType();
}

void BaseFunction::SetParamNum( UINT num )
{
	m_Impl->SetParamNum( num );
}

UINT BaseFunction::GetParamNum()
{
	return m_Impl->GetParamNum();
}

const char* BaseFunction::GetFuncDataStr()
{
	return m_Impl->GetFuncDataStr();
}

UINT BaseFunction::GetFuncDataSize()
{
	return m_Impl->GetFuncDataSize();
}

const string& BaseFunction::GetTokenStr()
{
	return m_Impl->GetTokenStr();
}

const string& BaseFunction::GetReturnStr()
{
	return m_Impl->GetReturnStr();
}

bool BaseFunction::GetIsOneWay()
{
	return m_Impl->GetIsOneWay();
}

void BaseFunction::SetReturnStr(const string &str)
{
	m_Impl->SetReturnStr( str );
}

const string& BaseFunction::GetExceptionStr()
{
	return m_Impl->GetExceptionStr();
}

void BaseFunction::SetIsOneWay( bool bFlag )
{
	m_Impl->SetIsOneWay( bFlag );
}

void BaseFunction::SetFuncValid(bool bValid)
{
	m_Impl->SetFuncValid( bValid );
}

bool BaseFunction::GetFuncValid()
{
	return m_Impl->GetFuncValid();
}

void BaseFunction::AddFuncParam(eParamType paramType, const char *paramName, bool bParseAsJson)
{
	m_Impl->AddFuncParam( paramType, paramName, bParseAsJson );
}

void BaseFunction::SetFuncBaseInfo( const TCHAR *className, const TCHAR *funcName, const TCHAR *funcType )
{
	m_Impl->SetFuncBaseInfo( className, funcName, funcType );
}