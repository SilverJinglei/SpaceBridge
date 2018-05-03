#include "CSharpFunctions.h"

namespace CSharpFunc
{
	string NotifyChanged::GetActionFromMap(eNotifyCollectionChangedAction act)
	{
		return m_actionMap[act];
	}


}