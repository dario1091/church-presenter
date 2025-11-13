using Avalonia;
using System;
using Velopack;

namespace ChurchPresenter;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // ===== VELOPACK INTEGRATION =====
        // Esto DEBE ejecutarse ANTES de cualquier otra cosa en la aplicación
        // Maneja los hooks de instalación, actualización y desinstalación
        try
        {
            // VelopackApp.Build().Run() maneja:
            // - Instalación inicial
            // - Actualizaciones aplicadas
            // - Desinstalación
            // - Creación de accesos directos
            VelopackApp.Build().Run();
            
            Console.WriteLine("Velopack inicializado correctamente");
        }
        catch (Exception ex)
        {
            // En modo desarrollo, VelopackApp.Run() puede fallar porque
            // la aplicación no está instalada con el instalador de Velopack
            // Simplemente lo ignoramos y continuamos normalmente
            Console.WriteLine($"Velopack no disponible (probablemente en modo desarrollo): {ex.Message}");
        }

        // Continuar con el inicio normal de Avalonia
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
