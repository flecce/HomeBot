using CommonHelpers.Schedulers.Tasks;
using System;

namespace CommonHelpers.Schedulers
{
    public interface ISchedulerFactory
    {
        void Start();

        void Stop();

        void AddTask(string cronScheduler, Func<ITask> taskFactory);
    }
}