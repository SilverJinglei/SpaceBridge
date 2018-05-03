#include "Cooperation.h"
#include "BaseFunctionClass.h"

#define CHECK_FUNC_VALID\
	if ( GetParamNum() == uParamNum )\
		SetFuncValid( true );\
	else\
		SetFuncValid( false );

//====C# Collection func class
#define FUNC_CLASS_MODEL "Model"
#define FUNC_CLASS_GROUP "Group"

namespace CSharpFunc
{
	enum eNotifyCollectionChangedAction
	{
		Collection_Add = 0,
		Collection_Remove = 1,
		Collection_Replace = 2,
		Collection_Move = 3,
		Collection_Reset = 4
	};

	class NotifyChanged : public BaseFunction
	{
	public:
		NotifyChanged( UINT uParamNum )
			:BaseFunction( nullptr )
		{
			SetFuncBaseInfo( _T( "ICollectionService" ), _T( "NotifyChanged" ), FUNC_TYPE_OPERATION_T );
			SetIsOneWay( true );
			SetParamNum( 4 );
			CHECK_FUNC_VALID;
			AddFuncParam( TYPE_STR, "action", false );
			AddFuncParam( TYPE_STR, "className", false );
			AddFuncParam( TYPE_STR, "newItems", true );
			AddFuncParam( TYPE_STR, "oldItems", true );

			m_actionMap[Collection_Add] = "Add";
			m_actionMap[Collection_Remove] = "Remove";
			m_actionMap[Collection_Replace] = "Replace";
			m_actionMap[Collection_Move] = "Move";
			m_actionMap[Collection_Reset] = "Reset";
		}

		~NotifyChanged(){}

		virtual const char* operator()( char *addr ){
			return nullptr;
		};

		string GetActionFromMap( eNotifyCollectionChangedAction act );

	protected:

	private:
		map<int, string> m_actionMap;
	};
}