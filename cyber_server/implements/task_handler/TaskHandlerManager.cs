using cyber_server.views.usercontrols.others;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cyber_server.implements.task_handler
{
    internal class TaskHandlerManager
    {
        public const string SERVER_WINDOW_HANDLER_KEY = "SERVER_WINDOW_HANDLER_KEY";
        public const string PLUGIN_MODIFY_WINDOW_HANDLER_KEY = "PLUGIN_MODIFY_WINDOW_HANDLER_KEY";
        public const string TOOL_MODIFY_WINDOW_HANDLER_KEY = "TOOL_MODIFY_WINDOW_HANDLER_KEY";

        private static TaskHandlerManager _instance;
        private Dictionary<string, TaskHandlingPanel> _handlerMapper = new Dictionary<string, TaskHandlingPanel>();

        public static TaskHandlerManager Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TaskHandlerManager();
                }
                return _instance;
            }
        }

        public void RegisterHandler(string key, TaskHandlingPanel handler)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _handlerMapper[key] = handler;
            }
        }

        public void UnregisterHandler(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _handlerMapper.Remove(key);
            }
        }

        public TaskHandlingPanel GetHandlerByKey(string key)
        {
            if (_handlerMapper.ContainsKey(key))
            {
                return _handlerMapper[key];
            }
            return null;
        }
    }
}
