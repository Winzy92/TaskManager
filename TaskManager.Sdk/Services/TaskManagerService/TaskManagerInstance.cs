using System;
using CommonServiceLocator;
using Prism.Events;
using TaskManager.Sdk.Interfaces;

namespace TaskManager.Sdk.Services.TaskManagerService
{
    public class TaskManagerInstance : ITaskManagerInstance
    {
        public IEventAggregator EventAggregator => GetInstance<IEventAggregator>();
        
        public T GetInstance<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }

        
        public bool IsRegistered<T>()
        {
            Boolean result;

            try
            {
                GetInstance<T>();

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}