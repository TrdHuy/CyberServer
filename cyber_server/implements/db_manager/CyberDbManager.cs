using cyber_server.@base;
using cyber_server.implements.log_manager;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cyber_server.implements.db_manager
{
    internal class CyberDbManager : IServerModule
    {
        private SemaphoreSlim _semaphore;
        private CyberDragonDbContext _appDbContext;
        public static CyberDbManager Current
        {
            get { return ServerModuleManager.DBM_Instance; }
        }

        private CyberDbManager()
        {
            _appDbContext = new CyberDragonDbContext();

            // Preload db when first boot app
            foreach (var plugin in _appDbContext.Plugins)
            {
                break;
            }
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            _appDbContext.Dispose();
        }

        public void OnModuleInit()
        {
        }

        public async Task<bool> RequestDbContextAsync(Action<CyberDragonDbContext> request)
        {
            var isSucess = false;
            await _semaphore.WaitAsync();
            try
            {
                request.Invoke(_appDbContext);
                isSucess = true;
            }
            catch (Exception ex)
            {
                isSucess = false;
                ServerLogManager.Current.E(ex.ToString());
            }
            finally
            {
                _semaphore.Release();
            }
            return isSucess;
        }

        public void RollBack()
        {
            var changedEntries = _appDbContext.ChangeTracker.Entries()
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

    }
}
