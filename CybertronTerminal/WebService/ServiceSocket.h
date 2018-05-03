#pragma once

#include <WinSock2.h>
#include <Windows.h>
#include <WS2tcpip.h>
#include <process.h>
#include <vector>
#include <string>
#include <memory>
#include "DevilLog.h"

#pragma comment(lib, "ws2_32.lib")

using std::vector;
using std::tr1::shared_ptr;
using std::string;

class ServiceSocket
{
public:
	~ServiceSocket();

	static ServiceSocket* SingletonPtr();
	static ServiceSocket& Singleton();

	//functions
	HRESULT Init( const char *port, const char *ipAddr = "127.0.0.1" );
	HRESULT GetOneMsgFromQueue( string &msg );
	HRESULT SendMsg( const char *msg, UINT msgSize, UINT socketIndex );

protected:
	ServiceSocket();

private:
	static ServiceSocket							*m_singleton;
	class											Impl;
	shared_ptr<Impl>								m_impl;
};