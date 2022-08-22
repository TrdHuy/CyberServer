using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.db_manager.sql_result_handler
{
    internal enum MessageQueryResult
    {
        None = 0,

        // The task has done, but there is no result return
        OK = 1,

        // Done the task, and return the result
        Done = 2,

        // Finished the task, but return the null
        Finished = 3,

        // The task was aborted
        Aborted = 4,

        // The task was cancled
        Cancled = 5
    }
}
