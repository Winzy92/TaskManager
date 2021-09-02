using System;
using Prism.Events;

namespace TaskManager.Sdk.Interfaces
{
    public interface ITaskManagerInstance
    {
        /// <summary>
        /// Метод возвращает экземпляр указанного сервиса.
        /// </summary>
        T GetInstance<T>();

        /// <summary>
        /// Агрегатор событий.
        /// </summary>
        IEventAggregator EventAggregator { get; }

        /// <summary>
        /// Метод проверяет, зарегистрирован ли в контейнере указанный тип.
        /// </summary>
        Boolean IsRegistered<T>();
    }
}