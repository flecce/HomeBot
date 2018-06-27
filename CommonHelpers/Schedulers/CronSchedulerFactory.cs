using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonHelpers.Schedulers.Tasks;

namespace CommonHelpers.Schedulers
{
    public class CronSchedulerFactory : ISchedulerFactory
    {
        private List<CronSchedulerTaskInfo> _taskList = new List<CronSchedulerTaskInfo>();
        private bool _stopAll = false;
        public void AddTask(string cronScheduler, Func<ITask> taskFactory)
        {
            _taskList.Add(new CronSchedulerTaskInfo
            {
                CronExpression = cronScheduler,
                TaskFactory = taskFactory
            });
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (!_stopAll)
                {
                    _taskList.ForEach(taskInfo =>
                    {
                        ITask taskToExecute = taskInfo.TaskFactory();
                        taskToExecute?.Init();
                        taskToExecute?.Run();
                    });

                    Thread.Sleep(5000);
                }
            });
        }

        public void Stop()
        {
            _stopAll = true;
        }
    }

    public class CronSchedulerTaskInfo
    {
        public string CronExpression { get; set; }
        public Func<ITask> TaskFactory { get; set; }
    }
}
