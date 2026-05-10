using System;

namespace BLL.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, object key)
            : base($"Сутність '{entityName}' з ідентифікатором ({key}) не знайдено.")
        {
        }
    }
}