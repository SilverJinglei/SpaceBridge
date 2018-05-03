#include "ServiceSocket.h"
#include "BaseFunctionClass.h"

#define DEFAULT_BUFLEN 4096
#define DEFAULT_PORT "43472"
//the maximum should not over 64(WSA_MAXIMUM_WAIT_EVENTS)
//but in our scene, we only need 2 socket(server and client)
const int g_maxConnectNum = 2;

ServiceSocket *ServiceSocket::m_singleton = nullptr;

class ServiceSocket::Impl
{
public:
	typedef map<std::size_t, std::size_t> BraceMap;
public:
	Impl();
	~Impl();

	//functions
	HRESULT Init( const char *port, const char *ipAddr = "127.0.0.1" );
	static unsigned int _stdcall MsgThread( void *pData );
	//this func is used to parse the json data such as {....}
	HRESULT ParseRecvBufAddJsonData( string &data, UINT socketIndex );
	HRESULT GetOneMsgFromQueue( string &msg );
	HRESULT SendMsg( const char *msg, UINT msgSize, UINT socketIndex );
	HRESULT	Free();
	UINT CountCharNumFromString( string &str, char c, std::size_t &firstPos, std::size_t &lastPos );
	HRESULT GetMatchedBracePos( string &str, BraceMap &braceMap );

protected:


private:
	UINT											m_socketIndex;
	SOCKET											m_socketArr[g_maxConnectNum];
	HANDLE											m_socketEventArr[g_maxConnectNum];
	long											m_msgThreadFlag;
	long											m_msgThreadExitFlag;
	typedef vector<string> dataVec;
	dataVec											m_dataVec;
	CRITICAL_SECTION								m_csDataVec;
};

HRESULT ServiceSocket::Impl::SendMsg(const char *msg, UINT msgSize, UINT socketIndex)
{
	int iSendNum = send( m_socketArr[socketIndex], msg, msgSize, 0 );

	if (iSendNum == SOCKET_ERROR) {
		TCHAR strTmp[MAX_CHAR_NUM] = {0};
		_stprintf_s( strTmp, MAX_CHAR_NUM, _T( "send failed with error: %d" ), WSAGetLastError() );
		DevilLog( strTmp );
		closesocket( m_socketArr[0] );
		WSACloseEvent( m_socketEventArr[0] );
		WSACleanup();
		return S_FALSE;
	}

	return S_OK;
}

HRESULT ServiceSocket::Impl::ParseRecvBufAddJsonData(string &data, UINT socketIndex)
{
	//insert the socket index to json first!
	char cTmp[MAX_PATH] = {0};
	sprintf_s( cTmp, MAX_PATH, "\"%s\":%d,", NODE_SOCK_IDX, socketIndex );
	data.insert( 1, cTmp );
	string strTmp;

	string::size_type size = data.size();
	BraceMap braceMap;
	HRESULT hRes = GetMatchedBracePos( data, braceMap );

	if ( 0 == braceMap.size() )
	{
		assert( 0 );
	}

	BraceMap::iterator it = braceMap.begin();
	for (; it != braceMap.end(); ++it )
	{
		strTmp = data.substr( it->first, it->second + 1 );

		EnterCriticalSection( &m_csDataVec );
		m_dataVec.push_back( strTmp );
		LeaveCriticalSection( &m_csDataVec );
	}

	//check if we handled the whole string
	BraceMap::reverse_iterator rit = braceMap.rbegin();
	if ( rit != braceMap.rend() && ( rit->second + 1 ) != size )
	{
		data = data.substr( rit->second , size - rit->second + 1 );
	}
	else if ( rit != braceMap.rend() && ( rit->second + 1 ) == size )
	{
		data.clear();
	}

	return S_OK;
}

ServiceSocket::Impl::Impl()
	:m_msgThreadFlag( 0 )
	, m_msgThreadExitFlag( 0 )
	, m_socketIndex( 0 )
{
	InitializeCriticalSection( &m_csDataVec );
}

ServiceSocket::Impl::~Impl()
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

ServiceSocket::ServiceSocket()
	:m_impl( new Impl() )
{

}

HRESULT ServiceSocket::Impl::Init(const char *port, const char *ipAddr /*= "127.0.0.1" */)
{
	WSADATA wsaData = {0};
	int iRes = 0;

	struct addrinfo *result = NULL;
	struct addrinfo hints;

	// Initialize Winsock
	iRes = WSAStartup(MAKEWORD(2,2), &wsaData);
	if (iRes != 0) {
		printf("WSAStartup failed with error: %d\n", iRes);
		return 1;
	}

	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;

	// Resolve the server address and port
	iRes = getaddrinfo(NULL, port, &hints, &result);
	if ( iRes != 0 ) {
		printf("getaddrinfo failed with error: %d\n", iRes);
		WSACleanup();
		return 1;
	}

	// Create a SOCKET for connecting to server
	m_socketArr[m_socketIndex] = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
	if (m_socketArr[m_socketIndex] == INVALID_SOCKET) {
		printf("socket failed with error: %ld\n", WSAGetLastError());
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}

	// Setup the TCP listening socket
	iRes = bind( m_socketArr[m_socketIndex], result->ai_addr, (int)result->ai_addrlen);
	if (iRes == SOCKET_ERROR) {
		printf("bind failed with error: %d\n", WSAGetLastError());
		freeaddrinfo(result);
		closesocket(m_socketArr[m_socketIndex]);
		WSACleanup();
		return 1;
	}

	m_socketEventArr[m_socketIndex] = WSACreateEvent();

	WSAEventSelect( m_socketArr[m_socketIndex], m_socketEventArr[m_socketIndex], FD_ACCEPT | FD_CLOSE );

	freeaddrinfo(result);

	iRes = listen( m_socketArr[m_socketIndex], SOMAXCONN );
	if (iRes == SOCKET_ERROR) {
		printf("listen failed with error: %d\n", WSAGetLastError());
		closesocket( m_socketArr[m_socketIndex] );
		WSACleanup();
		return 1;
	}

	++m_socketIndex;

	InterlockedCompareExchange( &m_msgThreadFlag, 1, m_msgThreadFlag );
	HANDLE handle = (HANDLE)_beginthreadex( nullptr, 0, ServiceSocket::Impl::MsgThread, this, 0, nullptr );
	if ( handle )
	{
		CloseHandle( handle );
	}

	return S_OK;
}

HRESULT ServiceSocket::Impl::GetOneMsgFromQueue(string &msg)
{
	EnterCriticalSection( &m_csDataVec );
	if ( 0 == m_dataVec.size() )
	{
		LeaveCriticalSection( &m_csDataVec );
		return S_FALSE;
	}

	dataVec::iterator it = ( m_dataVec.begin() + m_dataVec.size() - 1 );

	if ( it != m_dataVec.end() )
	{
		msg = *it;
		m_dataVec.erase( it );
		LeaveCriticalSection( &m_csDataVec );
		return S_OK;
	}

	LeaveCriticalSection( &m_csDataVec );
	return S_FALSE;
}

HRESULT ServiceSocket::Impl::Free()
{
	InterlockedCompareExchange( &m_msgThreadFlag, 0, m_msgThreadFlag );
	UINT uTime = 0;
	while ( InterlockedCompareExchange( &m_msgThreadExitFlag, m_msgThreadExitFlag, m_msgThreadExitFlag ) == 0 )
	{
		Sleep( 10 );
		if ( uTime++ > 50 )
			break;
	}

	EnterCriticalSection( &m_csDataVec );
	m_dataVec.clear();
	LeaveCriticalSection( &m_csDataVec );

	DeleteCriticalSection( &m_csDataVec );

	for (UINT i = 0; i < m_socketIndex; ++i )
	{
		closesocket( m_socketArr[i] );
		WSACloseEvent( m_socketEventArr[i] );
	}
	m_socketIndex = 0;
	WSACleanup();

	return S_OK;
}

UINT ServiceSocket::Impl::CountCharNumFromString(string &str, char c, std::size_t &firstPos, std::size_t &lastPos)
{
	UINT uCount = 0;
	std::size_t uPos = 0;

	while ( string::npos != ( uPos = str.find( c, uPos ) ) )
	{
		if ( 0 == uCount )
			firstPos = uPos;

		lastPos = uPos;
		++uCount;
		++uPos;
	}


	return uCount;
}

HRESULT ServiceSocket::Impl::GetMatchedBracePos(string &str, BraceMap &braceMap)
{
	std::size_t leftBracePos = 0, firstLeftBracePos = 0, secondLeftBracePos = 0, firstRightBracePos = 0, secondRightBracePos = 0;
	UINT uBraceMatchedCount = 0, uLeftBraceCount = 0, uRightBraceCount = 0;
	leftBracePos = str.find( "{" );
	while ( string::npos != leftBracePos )
	{
		firstLeftBracePos = ( string::npos == firstLeftBracePos ) ? string::npos : str.find( "{", firstLeftBracePos );
		firstRightBracePos = ( string::npos == firstRightBracePos ) ? string::npos : str.find( "}", firstRightBracePos );
		++uLeftBraceCount;
		uRightBraceCount = ( firstRightBracePos != string::npos ) ? ++uRightBraceCount : uRightBraceCount;
		secondLeftBracePos = str.find( "{", firstLeftBracePos + 1 );
		secondRightBracePos = str.find( "}", firstRightBracePos + 1 );

		//match end
		if ( string::npos != secondLeftBracePos && secondLeftBracePos > firstRightBracePos )
		{
			braceMap.insert( make_pair( leftBracePos, firstRightBracePos ) );
			leftBracePos = secondLeftBracePos;
			firstLeftBracePos = secondLeftBracePos;
			firstRightBracePos = secondRightBracePos;
		}
		//match not end, we should continue
		else if ( string::npos != secondLeftBracePos && secondLeftBracePos < firstRightBracePos )
		{
			firstLeftBracePos = secondLeftBracePos;
			firstRightBracePos = secondRightBracePos;
		}
		//real match end(complete)
		else if ( string::npos == secondLeftBracePos && uLeftBraceCount == uRightBraceCount )
		{
			braceMap.insert( make_pair( leftBracePos, firstRightBracePos ) );
			leftBracePos = string::npos;
		}
		//real match end(not complete)
		else if ( string::npos == firstRightBracePos && uLeftBraceCount != uRightBraceCount )
		{
			leftBracePos = string::npos;
		}
		else
		{
			assert( 0 );
		}
	}

	return S_OK;
}

unsigned int _stdcall ServiceSocket::Impl::MsgThread(void *pData)
{
	TCHAR strTmp[MAX_CHAR_NUM] = {0};
	ServiceSocket::Impl *pSocket = ( ServiceSocket::Impl* )pData;

	if ( nullptr == pSocket )
		return 0;

	string strData = "";
	int iResult = 0;
	int iSendResult = 0;
	char recvbuf[DEFAULT_BUFLEN];
	int recvbuflen = DEFAULT_BUFLEN;
	DWORD index = 0;
	WSANETWORKEVENTS networkEvents;

	DevilLog( _T( "socket connected succeed" ) );

	// Receive until the peer shuts down the connection
	while ( InterlockedCompareExchange( &pSocket->m_msgThreadFlag, pSocket->m_msgThreadFlag, pSocket->m_msgThreadFlag ) == 1 )
	{
		index = WSAWaitForMultipleEvents( pSocket->m_socketIndex, pSocket->m_socketEventArr, FALSE, 10, FALSE );
		if ( WSA_WAIT_FAILED == index || WSA_WAIT_TIMEOUT == index )
		{
			continue;
		}

		WSAEnumNetworkEvents( pSocket->m_socketArr[index - WSA_WAIT_EVENT_0], pSocket->m_socketEventArr[index - WSA_WAIT_EVENT_0], &networkEvents );

		if ( networkEvents.lNetworkEvents & FD_ACCEPT )
		{
			if ( pSocket->m_socketIndex >= g_maxConnectNum )
			{
				DevilLog( _T( "over max socket!" ), Devil::LOGLEVEL_ERROR );
				continue;
			}
			pSocket->m_socketArr[pSocket->m_socketIndex] = accept( pSocket->m_socketArr[index - WSA_WAIT_EVENT_0], nullptr, nullptr );

			pSocket->m_socketEventArr[pSocket->m_socketIndex] = WSACreateEvent();
			WSAEventSelect( pSocket->m_socketArr[pSocket->m_socketIndex], pSocket->m_socketEventArr[pSocket->m_socketIndex], FD_READ | FD_WRITE | FD_CLOSE );
			++pSocket->m_socketIndex;

			_stprintf_s( strTmp, MAX_CHAR_NUM, _T( "new client socket connected, total socket[%d]" ), pSocket->m_socketIndex );
			DevilLog( strTmp );
		}
		else if ( networkEvents.lNetworkEvents & FD_READ )
		{
			iResult = recv( pSocket->m_socketArr[index - WSA_WAIT_EVENT_0], recvbuf, recvbuflen, 0);
			if (iResult > 0)
			{
				recvbuf[iResult] = '\0';
				strData.append( recvbuf );
				pSocket->ParseRecvBufAddJsonData( strData, index );
			}
		}
		else if ( networkEvents.lNetworkEvents & FD_CLOSE )
		{
			closesocket( pSocket->m_socketArr[index - WSA_WAIT_EVENT_0] );
			WSACloseEvent( pSocket->m_socketEventArr[index - WSA_WAIT_EVENT_0] );
			--pSocket->m_socketIndex;

			_stprintf_s( strTmp, MAX_CHAR_NUM, _T( "socket disconnected, total socket[%d]" ), pSocket->m_socketIndex );
			DevilLog( strTmp );
		}
	}


	InterlockedCompareExchange( &pSocket->m_msgThreadExitFlag, 1, pSocket->m_msgThreadExitFlag );
	return 0;
}

ServiceSocket::~ServiceSocket()
{
}

ServiceSocket* ServiceSocket::SingletonPtr()
{
	return ( nullptr == m_singleton ) ? ( m_singleton = new ServiceSocket() ) : ( m_singleton );
}

ServiceSocket& ServiceSocket::Singleton()
{
	return ( nullptr == m_singleton ) ? *( m_singleton = new ServiceSocket() ) : ( *m_singleton );
}

HRESULT ServiceSocket::Init( const char *port, const char *ipAddr /* = "127.0.0.1" */ )
{
	return m_impl->Init( port, ipAddr );
}

HRESULT ServiceSocket::GetOneMsgFromQueue( string &msg )
{
	return m_impl->GetOneMsgFromQueue( msg );
}

HRESULT ServiceSocket::SendMsg( const char *msg, UINT msgSize, UINT socketIndex )
{
	return m_impl->SendMsg( msg, msgSize, socketIndex );
}