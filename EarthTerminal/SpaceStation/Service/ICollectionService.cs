using System.Collections;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SpaceStation.Service
{
    public interface ICollectionService
    {
        void NotifyChanged(NotifyCollectionChangedAction action, string className, IList newItems, IList oldItems);

        Task RemoveAsync(string className, IList oldItems);
    }

    public abstract class CollectionServiceBase : ICollectionService
    {
        public void NotifyChanged(NotifyCollectionChangedAction action, string className, IList newItems, IList oldItems)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        OnAddItems(className, newItems);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        OnRemoveItems(className, oldItems);
                        break;
                    }
            }
        }

        public Task RemoveAsync(string className, IList oldItems)
        {
            throw new System.NotImplementedException();
        }

        protected virtual void OnRemoveItems(string className, IList oldItems)
        {
            
        }

        protected virtual void OnAddItems(string className, IList newItems)
        {
            
        }
    }
}