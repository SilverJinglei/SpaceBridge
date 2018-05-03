#include "TerminalMgr.h"
#include "ServiceSocket.h"
#include "Cooperation.h"
#include "FuncMgr.h"


CybertronTerminalMgr *CybertronTerminalMgr::m_singleton = nullptr;

class CybertronTerminalMgr::Impl
{
public:
	Impl();
	~Impl();

protected:

private:
	shared_ptr<Devil::LogSystem>									m_logSystemPtr;
	shared_ptr<ServiceSocket>										m_serviceSocketPtr;
	shared_ptr<Cooperation>											m_cooperationMgrPtr;
	shared_ptr<FuncMgr>												m_funcMgrPtr;
};

CybertronTerminalMgr::Impl::Impl()
	:m_cooperationMgrPtr( Cooperation::SingletonPtr() )
	, m_funcMgrPtr( FuncMgr::SingletonPtr() )
	, m_serviceSocketPtr( ServiceSocket::SingletonPtr() )
	, m_logSystemPtr( Devil::LogSystem::SingletonPtr() )
{
	m_cooperationMgrPtr->StartCooperate();
}

CybertronTerminalMgr::Impl::~Impl()
{
	m_cooperationMgrPtr->StopCooperate();
}

CybertronTerminalMgr::CybertronTerminalMgr()
	:m_impl( new Impl )
{
}

CybertronTerminalMgr::~CybertronTerminalMgr()
{
}

CybertronTerminalMgr* CybertronTerminalMgr::SingletonPtr()
{
	return ( m_singleton == nullptr ) ? ( m_singleton = new CybertronTerminalMgr ) : ( m_singleton );
}

CybertronTerminalMgr& CybertronTerminalMgr::Singleton()
{
	return ( m_singleton == nullptr ) ? *( m_singleton = new CybertronTerminalMgr ) : *( m_singleton );
}