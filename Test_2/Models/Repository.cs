using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Test_2.Models
{
    public class Repository<C, T> : IDisposable
        where T : class, IModel where C : DbContext, new()
    {
        public C db = null;
        public Repository(C db = null)
        {
            if (db == null)
                db = new C();
            this.db = db;
        }

        public List<T> GetList()
        {
            return db.Set<T>().ToList();
        }

        public T Get(Guid? id)
        {
            return db.Set<T>().SingleOrDefault(a => a.Id == id);
        }

        public void Add(T record)
        {
            record.Id = Guid.NewGuid();
            db.Set<T>().Add(record);
        }

        public void Remove(Guid Id)
        {
            db.Set<T>().Remove(Get(Id));

        }

        public void Commit()
        {
            db.SaveChanges();
        }
        public void Dispose()
        {
            db.Dispose();
        }
    }
}

  