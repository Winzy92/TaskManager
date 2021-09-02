using Prism.Ioc;
using Prism.Modularity;
using TaskManager.Sdk.Interfaces;

namespace TaskManager.Sdk.Services
{
    public class ServiceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ISettingsService, SettingsService.SettingsService>();
            containerRegistry.RegisterSingleton<IDatabaseConnectionService, DataBaseConnectionService.DataBaseConnectionService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }
    }
}