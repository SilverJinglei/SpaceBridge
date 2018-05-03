#pragma once
#include "ServiceSocket.h"
#include "FuncMgr.h"

//====init file json
#define NODE_PORT "Port"
#define NODE_EARTH_TERMINAL_NAME "EarthTerminalName"
#define DEFAULT_EARTH_TERMINAL_NAME "TEST.EXE"

//this class is used to cooperate the ServiceSocket class and FuncMgr class
class Cooperation
{
public:
	~Cooperation();
	static Cooperation& Singleton();
	static Cooperation* SingletonPtr();

	HRESULT StartCooperate();
	HRESULT StopCooperate();

	string& InvokeRemoteFunc( BaseFunction *pFunc );

protected:
	Cooperation();
	HRESULT GetAndCheckMsgType();
	HRESULT CheckEarthTerminalAlive();
	UINT GetSocketIndex( string &str );

private:
	static Cooperation											*m_singleton;
	class														Impl;
	shared_ptr<Impl>											m_impl;
};