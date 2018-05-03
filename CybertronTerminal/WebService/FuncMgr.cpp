#include "FuncMgr.h"
#include "Utility.h"

FuncMgr *FuncMgr::m_singleton = nullptr;

class FuncMgr::Impl
{
public:
	Impl();
	~Impl();

	//when we add func we will distinguish them(in case we have same name method, but not same class name) with class name automatically
	HRESULT AddFunc( BaseFunction *pFunction );
	HRESULT RemoveFunc( TCHAR *className, TCHAR *funcName );
	HRESULT ExecuteFunc( int i, const char *className, const char *funcName, ... );
	HRESULT ParseJson( Json::Value &root );
	HRESULT GetOneReturnValueFromList( string &val );
	void Free();

protected:


private:
	typedef map<string, BaseFunction*> FunctionMap;
	typedef map<string, FunctionMap*> FunctionMapPtrVec;
	FunctionMapPtrVec											m_functionMapPtrVec;
	typedef map<string, FunctionMap*> FunctionClassMap;
	FunctionClassMap											m_functionClassMap;
	typedef vector<string> ReturnValueVec;
	ReturnValueVec												m_returnValueVec;
	CRITICAL_SECTION											m_csReturnValueVec;

};

FuncMgr::Impl::Impl()
{
	InitializeCriticalSection( &m_csReturnValueVec );
}

FuncMgr::Impl::~Impl()
{
	try
	{
		Free();
	}
	catch (...)
	{
		assert( 0 );
	}
}

HRESULT FuncMgr::Impl::AddFunc( BaseFunction *pFunction )
{
	if ( nullptr == pFunction )
	{
		return S_FALSE;
	}

	char cTmp[MAX_CHAR_NUM] = {0};
	string funcName = ConvertWCharToMultiByte( pFunction->GetFuncName(), cTmp );
	string className = ConvertWCharToMultiByte( pFunction->GetClassName(), cTmp );

	FunctionMapPtrVec::iterator it = m_functionMapPtrVec.find( className );
	if ( it != m_functionMapPtrVec.end() )
	{
		it->second->insert( make_pair( funcName, pFunction ) );
	}
	else
	{
		FunctionMap *pFuncMap = new FunctionMap;
		m_functionMapPtrVec.insert( make_pair( className, pFuncMap ) );
		pFuncMap->insert( make_pair( funcName, pFunction ) );
	}

	return S_OK;
}

HRESULT FuncMgr::Impl::RemoveFunc(TCHAR *className, TCHAR *funcName)
{
	char cTmp[MAX_CHAR_NUM] = {0};
	string strFuncName = ConvertWCharToMultiByte( funcName, cTmp );
	string strClassName = ConvertWCharToMultiByte( className, cTmp );

	FunctionMapPtrVec::iterator it = m_functionMapPtrVec.find( strClassName );
	if ( it != m_functionMapPtrVec.end() )
	{
		FunctionMap::iterator it2 = it->second->find( strFuncName );
		if ( it2 != it->second->end() )
		{
			it->second->erase( it2 );
			return S_OK;
		}
	}

	return S_FALSE;
}

HRESULT FuncMgr::Impl::ExecuteFunc( int i, const char *className, const char *funcName, ... )
{
	va_list param;
	va_start( param, i );
	va_arg( param, TCHAR* );
	va_arg( param, TCHAR* );

	FunctionMapPtrVec::iterator it = m_functionMapPtrVec.find( className );
	if ( it != m_functionMapPtrVec.end() )
	{
		FunctionMap::iterator it2 = it->second->find( funcName );
		if ( it2 != it->second->end() )
		{
			string strReturnValue = (*it2->second)( param );
			if ( "" == strReturnValue )
				return S_OK;

			EnterCriticalSection( &m_csReturnValueVec );
			m_returnValueVec.push_back( strReturnValue );
			LeaveCriticalSection( &m_csReturnValueVec );
			return S_OK;
		}
	}

	return S_FALSE;
}

HRESULT FuncMgr::Impl::ParseJson( Json::Value &root )
{
	const char *className = root[NODE_CLASS].asCString();
	const char *funcName = root[NODE_NAME].asCString();
	const char *type = root[NODE_TYPE].asCString();

	if ( nullptr != funcName && nullptr != className )
	{
		HRESULT res = 0;
		res = ExecuteFunc( 0, className, funcName, root );
		return ( res == S_OK ) ? S_OK : S_FALSE;
	}

	return S_FALSE;
}

HRESULT FuncMgr::Impl::GetOneReturnValueFromList(string &val)
{
	EnterCriticalSection( &m_csReturnValueVec );
	if ( m_returnValueVec.size() )
	{
		ReturnValueVec::iterator it = m_returnValueVec.begin();
		if ( "" == *it )
		{
			LeaveCriticalSection( &m_csReturnValueVec );
			return S_FALSE;
		}
		val = *it;
		m_returnValueVec.erase( it );
		LeaveCriticalSection( &m_csReturnValueVec );
		return S_OK;
	}
	else
	{
		LeaveCriticalSection( &m_csReturnValueVec );
		return S_FALSE;
	}
}

void FuncMgr::Impl::Free()
{
	FunctionMapPtrVec::iterator it = m_functionMapPtrVec.begin();
	for (; it != m_functionMapPtrVec.end(); ++it )
	{
		FunctionMap::iterator it2 = (*(it->second)).begin();
		for (; it2 != (*(it->second)).end(); ++it2 )
		{
			SAFE_DELETE( it2->second );
		}
		(*(it->second)).clear();
		SAFE_DELETE((it->second));
	}
	m_functionMapPtrVec.clear();

	EnterCriticalSection( &m_csReturnValueVec );
	m_returnValueVec.clear();
	LeaveCriticalSection( &m_csReturnValueVec );
	DeleteCriticalSection( &m_csReturnValueVec );
}

FuncMgr::FuncMgr()
	:m_impl( new Impl )
{
}

FuncMgr::~FuncMgr()
{
}

FuncMgr* FuncMgr::SingletonPtr()
{
	return ( nullptr == m_singleton ) ? ( m_singleton = new FuncMgr ) : ( m_singleton );
}

FuncMgr& FuncMgr::Singleton()
{
	return ( nullptr == m_singleton ) ? *( m_singleton = new FuncMgr ) : *( m_singleton );
}

HRESULT FuncMgr::AddFunc(BaseFunction *pFunction)
{
	return m_impl->AddFunc( pFunction );
}

HRESULT FuncMgr::RemoveFunc( TCHAR *className, TCHAR *funcName )
{
	return m_impl->RemoveFunc( className, funcName );
}

HRESULT FuncMgr::ParseJson( Json::Value &root )
{
	return m_impl->ParseJson( root );
}

HRESULT FuncMgr::GetOneReturnValueFromList( string &val )
{
	return m_impl->GetOneReturnValueFromList( val );
}
