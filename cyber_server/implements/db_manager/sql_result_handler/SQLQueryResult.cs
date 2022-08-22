using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.db_manager.sql_result_handler
{
    internal class SQLQueryResult
    {
        private object _result;
        private MessageQueryResult _mesResult;
        private string _messageToString;

        public SQLQueryResult(object result, MessageQueryResult mesResult, string messageToString = "")
        {
            _result = result;
            _mesResult = mesResult;
            _messageToString = messageToString;
        }

        public object Result
        {
            get { return _result; }
        }

        public MessageQueryResult MesResult
        {
            get { return _mesResult; }
        }

        public string Messsage
        {
            get { return _messageToString; }
        }
    }

}
