using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.db_manager.sql_result_handler
{
    internal class SQLResultHandler
    {
        private CyberDragonDbContext _appDBContext;
        private SQLQueryResult _result;

        public SQLResultHandler(CyberDragonDbContext dBContext)
        {
            _appDBContext = dBContext;
        }

        public void RollBack()
        {
            var changedEntries = _appDBContext.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }

        internal void Dispose()
        {
            _appDBContext.Dispose();
        }

        public SQLQueryResult ExecuteQuery(string SQLCmdKey, params object[] paramaters)
        {
            _result = null;
            switch (SQLCmdKey)
            {
                default:
                    break;
            }

            if (_result.MesResult == MessageQueryResult.Aborted ||
                _result.MesResult == MessageQueryResult.Cancled)
            {
                RollBack();
            }

            return _result;
        }

        private void HandleDbEntityValidationException(DbEntityValidationException e)
        {
            //Should implement log writer here for debug purpose
            foreach (var eve in e.EntityValidationErrors)
            {
                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                foreach (var ve in eve.ValidationErrors)
                {
                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                        ve.PropertyName, ve.ErrorMessage);
                }
            }
        }
    }

}
