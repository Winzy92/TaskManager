using CommonServiceLocator;
using Prism.Unity;
using TaskManager.Sdk.Interfaces;
using Unity;

namespace TaskManager.Sdk.Services.TaskManagerService
{
    public class TaskManagerServices
    {
        /// <summary>
        /// Этот класс по сути обертка для своего сервис локатора, сервис локатор можно будет вызывать не создавая
        /// при этом экземпляра
        /// </summary>

        public static ITaskManagerInstance Instance { get; private set; }

        public static void SetInstance(ITaskManagerInstance instance)
        {
            Instance = instance;
        }

        static TaskManagerServices()
        {
            SetInstance(new TaskManagerInstance());
        }

    }
}