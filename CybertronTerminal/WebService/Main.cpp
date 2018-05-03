
#include "ServiceSocket.h"
#include "FuncMgr.h"
#include "Cooperation.h"
#include "VRPFunctions.h"
#include "TerminalMgr.h"

int main()
{
	shared_ptr<CybertronTerminalMgr> autoptr;
	DevilLogInit( _T( "WebService.log" ) );
	ServiceSocket::Singleton().Init( "12345" );
	Cooperation::Singleton().StartCooperate();

	system( "pause" );
	return 0;
}