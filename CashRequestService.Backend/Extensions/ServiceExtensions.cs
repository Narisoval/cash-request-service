using Exception = System.Exception;

namespace CashRequestService.Backend.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterSingletonProvider(
        this IServiceCollection services,
        Type serviceType,
        IConfiguration configuration,
        string configurationName,
        string subsection = null,
        params Type[] types)
    {
        return RegisterProvider(services, serviceType, configuration, configurationName, ServiceLifetime.Singleton, subsection, types);
    }

    public static IServiceCollection RegisterScopedProvider(
        this IServiceCollection services,
        Type serviceType,
        IConfiguration configuration,
        string configurationName,
        string subsection = null,
        params Type[] types)
    {
        return RegisterProvider(services, serviceType, configuration, configurationName, ServiceLifetime.Scoped, subsection, types);
    }

    public static IServiceCollection RegisterTransientProvider(
        this IServiceCollection services,
        Type serviceType,
        IConfiguration configuration,
        string configurationName,
        string subsection = null,
        params Type[] types)
    {
        return RegisterProvider(services, serviceType, configuration, configurationName, ServiceLifetime.Transient, subsection, types);
    }

    private static IServiceCollection RegisterProvider(
        IServiceCollection services,
        Type serviceType,
        IConfiguration configuration,
        string configurationName,
        ServiceLifetime lifetime,
        string subsection = null,
        params Type[] types)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(configurationName);
        ArgumentNullException.ThrowIfNull(types);
        if (!Enum.IsDefined(lifetime))
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime));
        }

        string providerKey = string.IsNullOrEmpty(subsection)
            ? configurationName
            : $"{subsection}:{configurationName}";

        string providerName = configuration.GetValue<string>(providerKey)
            ?? throw new Exception($"No provider name found in configuration for key '{providerKey}'.");

        Type implementationType = types.FirstOrDefault(x => x.FullName.Contains(providerName))
            ?? throw new Exception($"Provider '{providerName}' does not exist in the provided types array.");

        return lifetime switch
        {
            ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
            ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
            _ => services
        };
    }
} 