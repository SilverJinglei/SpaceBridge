#pragma once

#include <memory>

using std::tr1::shared_ptr;

class CybertronTerminalMgr
{
public:
	~CybertronTerminalMgr();
	static CybertronTerminalMgr* SingletonPtr();
	static CybertronTerminalMgr& Singleton();

protected:
	CybertronTerminalMgr();

private:
	static CybertronTerminalMgr							*m_singleton;
	class												Impl;
	shared_ptr<Impl>									m_impl;
};