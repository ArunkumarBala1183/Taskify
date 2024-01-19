namespace Task_Management.Models.DatabaseOperations
{
    public interface IDatabaseOperations
    {
        public object insertData(object  data);
        public void deleteData(object data);
        public object updateData(object data);
        public object searchData(object id);
        public object viewData(object id);
    }
}
