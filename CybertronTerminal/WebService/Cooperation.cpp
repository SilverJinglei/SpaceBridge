#include "Cooperation.h"
#include "PreDefine.h"
#include "Utility.h"

Cooperation *Cooperation::m_singleton = nullptr;

class Cooperation::Impl
{
public:
	Impl();
	~Impl();

	HRESULT StartCooperate();
	HRESULT StopCooperate();

	static unsigned int _stdcall CooperationThread( void *pData );
	string& InvokeRemoteFunc( BaseFunction *pFunc );
	HRESULT GetAndCheckMsgType();
	HRESULT CheckEarthTerminalAlive();
	UINT GetSocketIndex( string &str );


private:
	string														m_strPort;
	string														m_strExeName;
	long														m_coopThreadFlag;
	long														m_coopThreadExitFlag;
	typedef map<string, BaseFunction*> FuncMap;
	FuncMap														m_funcDataMap;
	CRITICAL_SECTION											m_csFuncDataMap;
	string														m_returnValue;
	HANDLE														m_returnEvent;
};

Cooperation::Impl::Impl()
	:m_coopThreadFlag( 0 )
	, m_coopThreadExitFlag( 0 )
{
	m_returnEvent = CreateEvent( nullptr, FALSE, FALSE, _T( "" ) );
	InitializeCriticalSection( &m_csFuncDataMap );
}

Cooperation::Impl::~Impl()
{
	CloseHandle( m_returnEvent );
	DeleteCriticalSection( &m_csFuncDataMap );
}

HRESULT Cooperation::Impl::StartCooperate()
{
	ifstream jsonFile( "Terminal.ini", ios::binary );
	Json::Value root;
	Json::Reader reader;
	if ( jsonFile )
	{
		reader.parse( jsonFile, root );
		jsonFile.close();

		//====start socket service
		m_strPort = root[NODE_PORT].asString();
		ServiceSocket::Singleton().Init( m_strPort.c_str() );

		//====start EarthTerminal exe
		m_strExeName = root[NODE_EARTH_TERMINAL_NAME].asString();
		ShellExecuteA( nullptr, nullptr, m_strExeName.c_str(), "open", nullptr, SW_SHOW );
	}
	else
	{
// 		ServiceSocket::Singleton().Init( DEFAULT_PORT );
// 		ShellExecuteA( nullptr, nullptr, DEFAULT_EARTH_TERMINAL_NAME, "open", nullptr, SW_SHOW );
		DevilLog( _T( "StartCooperate failed, file not exist" ), Devil::LOGLEVEL_ERROR );
		return S_FALSE;
	}


	InterlockedCompareExchange( &m_coopThreadFlag, 1, m_coopThreadFlag );
	HANDLE handle = ( HANDLE )_beginthreadex( nullptr, 0, Cooperation::Impl::CooperationThread, this, 0, nullptr );
	if ( handle )
	{
		CloseHandle( handle );
	}

	return S_OK;
}

HRESULT Cooperation::Impl::StopCooperate()
{
	//check whether or start the thread or not
	if ( InterlockedCompareExchange( &m_coopThreadFlag, m_coopThreadFlag, m_coopThreadFlag ) != 0 )
	{
		InterlockedCompareExchange( &m_coopThreadFlag, 0, m_coopThreadFlag );
		while ( InterlockedCompareExchange( &m_coopThreadExitFlag, m_coopThreadExitFlag, m_coopThreadExitFlag ) == 0 )
		{
			Sleep( 10 );
		}
	}

	return S_OK;
}

unsigned int _stdcall Cooperation::Impl::CooperationThread(void *pData)
{
	Cooperation::Impl *pImpl = ( Cooperation::Impl* )pData;
	if ( nullptr == pImpl )
		return 1;

	TCHAR strTmp[MAX_CHAR_NUM] = {0};
	HRESULT hRes = S_FALSE;
	string strReturnValue;
	UINT uTime = GetTickCount();

	while ( InterlockedCompareExchange( &pImpl->m_coopThreadFlag, pImpl->m_coopThreadFlag, pImpl->m_coopThreadFlag ) == 1 )
	{
		//get msg and check msg type, then choose what we should do
		pImpl->GetAndCheckMsgType();

		//check the return value list see if we need send the return value
		hRes = FuncMgr::Singleton().GetOneReturnValueFromList( strReturnValue );
		if ( S_OK == hRes )
		{
			ServiceSocket::Singleton().SendMsg( strReturnValue.c_str(), strReturnValue.size(), pImpl->GetSocketIndex( strReturnValue ) );
		}

		//check if the earth terminal still alive, if not, we should restart it
		if ( GetTickCount() - uTime > 5000 )
		{
			pImpl->CheckEarthTerminalAlive();
			uTime = GetTickCount();
		}

		Sleep( 1 );
	}

	InterlockedCompareExchange( &pImpl->m_coopThreadExitFlag, 1, pImpl->m_coopThreadExitFlag );
	return 0;
}

string& Cooperation::Impl::InvokeRemoteFunc(BaseFunction *pFunc)
{
	//we should handle return value here
	//now we have 2 methods to handle it, Async or Sync
	//for now, we used sync
	ServiceSocket::Singleton().SendMsg( pFunc->GetFuncDataStr(), pFunc->GetFuncDataSize(), 1 );
	if ( !pFunc->GetIsOneWay() )
	{
		EnterCriticalSection( &m_csFuncDataMap );
		m_funcDataMap.insert( make_pair( pFunc->GetTokenStr(), pFunc ) );
		LeaveCriticalSection( &m_csFuncDataMap );

		WaitForSingleObject( m_returnEvent, INFINITE );

		m_returnValue = pFunc->GetReturnStr();
	}
	else
	{
		m_returnValue = "";
	}

	return m_returnValue;
}

HRESULT Cooperation::Impl::GetAndCheckMsgType()
{
	TCHAR strTmp[MAX_CHAR_NUM] = {0};
	string strMsg;
	HRESULT hRes = ServiceSocket::Singleton().GetOneMsgFromQueue( strMsg );
	if ( S_OK == hRes )
	{
		Json::Reader reader;
		Json::Value root;
		bool bRes = reader.parse( ReplaceSpecificChar( strMsg, '\\', '/' ), root );
		if ( !bRes )
		{
			assert( !bRes );
			_stprintf_s( strTmp, MAX_CHAR_NUM, _T( "reader parse json failed:[%s]" ), strMsg.c_str() );
			DevilLog( strTmp, Devil::LOGLEVEL_ERROR );
			return S_FALSE;
		}

		string strType = root[NODE_TYPE].asString();

		if ( _stricmp( strType.c_str(), FUNC_TYPE_OPERATION ) == 0 )
		{
			bool bIsOneWay = root[NODE_IS_ONE_WAY].asBool();
			if ( bIsOneWay )
			{
				FuncMgr::Singleton().ParseJson( root );
			}
			else
			{
				FuncMgr::Singleton().ParseJson( root );
			}
		}
		else if ( _stricmp( strType.c_str(), FUNC_RESULT ) == 0 )
		{
			string strToken = root[NODE_TOKEN].asString();
			EnterCriticalSection( &m_csFuncDataMap );
			FuncMap::iterator it = m_funcDataMap.find( strToken );
			if ( it != m_funcDataMap.end() )
			{
				it->second->SetReturnStr( root[NODE_VALUE].asString() );
				//notify the event to finish the InvokeFuncThread
				SetEvent( m_returnEvent );
				m_funcDataMap.erase( it );
			}
			LeaveCriticalSection( &m_csFuncDataMap );
		}
	}

	return S_OK;
}

HRESULT Cooperation::Impl::CheckEarthTerminalAlive()
{
	WCHAR wTmp[MAX_PATH] = {0};
	TCHAR strTmp[MAX_CHAR_NUM] = {0};
	HANDLE hProcessSnap = nullptr;
	HANDLE hProcess = nullptr;
	PROCESSENTRY32 pe32 = {0};
	DWORD dwPriorityClass = 0;

	hProcessSnap = CreateToolhelp32Snapshot( TH32CS_SNAPPROCESS, 0 );
	if( hProcessSnap == INVALID_HANDLE_VALUE )
	{
		return S_FALSE;
	}

	pe32.dwSize = sizeof( PROCESSENTRY32 );
	if( !Process32First( hProcessSnap, &pe32 ) )
	{
		CloseHandle( hProcessSnap ); // Must clean up the snapshot object!
		return S_FALSE;
	}

	bool bFoundFlag = false;
	do
	{
		if ( wcscmp( ConverMultiByteToWChar( m_strExeName.c_str(), wTmp ), pe32.szExeFile ) == 0 )
		{
			bFoundFlag = true;
			break;
		}

	} while( Process32Next( hProcessSnap, &pe32 ) );

	CloseHandle( hProcessSnap );

	if ( !bFoundFlag )
	{
		HINSTANCE hRes = ShellExecuteA( nullptr, nullptr, m_strExeName.c_str(), "open", nullptr, SW_SHOW );
		TCHAR strTmp[1024] = {0};
		_stprintf_s( strTmp, 1024, _T( "couldn't find earth terminal, restart it[%d]" ), ( int )hRes );
		DevilLog( strTmp );
	}

	return S_OK;
}

UINT Cooperation::Impl::GetSocketIndex(string &str)
{
	std::size_t firstPos = str.find( ':' );
	std::size_t secondPos = str.find( ',' );

	string strTmp = str.substr( firstPos + 1, secondPos - firstPos - 1 );
	str = "{" + str.substr( secondPos + 1, str.size() - secondPos - 1 );
	return static_cast<UINT>( atoi( strTmp.c_str() ) );
}

Cooperation::Cooperation()
	:m_impl( new Impl )
{
}

Cooperation::~Cooperation()
{

}

Cooperation& Cooperation::Singleton()
{
	return ( nullptr == m_singleton ) ? *( m_singleton = new Cooperation ) : *( m_singleton );
}

Cooperation* Cooperation::SingletonPtr()
{
	return ( nullptr == m_singleton ) ? ( m_singleton = new Cooperation ) : ( m_singleton );
}

HRESULT Cooperation::StartCooperate()
{
	return m_impl->StartCooperate();
}

HRESULT Cooperation::StopCooperate()
{
	return m_impl->StopCooperate();
}

string& Cooperation::InvokeRemoteFunc( BaseFunction *pFunc )
{
	return m_impl->InvokeRemoteFunc( pFunc );
}

HRESULT Cooperation::CheckEarthTerminalAlive()
{
	return m_impl->CheckEarthTerminalAlive();
}

HRESULT Cooperation::GetAndCheckMsgType()
{
	return m_impl->GetAndCheckMsgType();
}

UINT Cooperation::GetSocketIndex( string &str )
{
	return m_impl->GetSocketIndex( str );
}
