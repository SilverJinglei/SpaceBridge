namespace SpaceStation.Service
{
    public interface IGenericInstanceService
    {
        void Update(string className, int id, string property, object value);
    }
}