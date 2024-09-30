using Elevator.Data.Repository.Interface;
using System.ComponentModel;
using System.Data.Common;
using System.Dynamic;
using System.Linq.Expressions;

namespace Elevator.Data.Repository
{
    public class DataRepository<T> : IRepository<T> where T : class
    {
        protected IContext Db;

        public DataRepository(IContext datacontext)
        {
            Db = datacontext;
        }

        #region IRepository<T> Members

        public T Insert(T item)
        {
            Db.Set<T>().Add(item);
            Db.SaveChanges();
            return item;
        }

        public bool Delete(Expression<Func<T, bool>> predicate)
        {
            var list = SearchFor(predicate).ToList();
            if (list != null)
            {
                foreach (T ctr in list)
                {
                    Db.Entry<T>(ctr).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                }

                Db.SaveChanges();
                return true;
            }

            return false;
        }

        public T UpdateSpecificValuesNotNull(T item, Expression<Func<T, bool>> predicate)
        {
            var record = SearchFor(predicate).FirstOrDefault();
            if (record != null)
            {
                var init = Db.Entry<T>(record).CurrentValues.ToObject();

                Db.Entry<T>(record).CurrentValues.SetValues(CopyValues(init, item));
                Db.SaveChanges();
                return record;
            }

            return null;
        }


        public T Update(T item, Expression<Func<T, bool>> predicate)
        {
            var record = SearchFor(predicate).FirstOrDefault();
            if (record != null)
            {
                Db.Entry<T>(record).CurrentValues.SetValues(item);
                Db.SaveChanges();
                return record;
            }

            return null;
        }

        public IQueryable<T> SearchFor(Expression<Func<T, bool>> predicate)
        {

            return Db.Set<T>().Where(predicate);
        }

        public IQueryable<T> GetAll()
        {
            return Db.Set<T>();
        }

        public List<Dictionary<string, object>> Read(DbDataReader reader)
        {
            List<Dictionary<string, object>> expandolist = new List<Dictionary<string, object>>();
            foreach (var item in reader)
            {
                IDictionary<string, object> expando = new ExpandoObject();
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(item))
                {
                    var obj = propertyDescriptor.GetValue(item);
                    expando.Add(propertyDescriptor.Name, obj);
                }
                expandolist.Add(new Dictionary<string, object>(expando));
            }
            return expandolist;
        }

        #endregion

        #region static class
        private object CopyValues(object target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                {
                    prop.SetValue(target, value, null);
                }
            }

            return target;
        }
        #endregion
    }

}
