using System;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace ChurchPresenter.Services;

/// <summary>
/// Servicio para manejar las actualizaciones de la aplicación usando Velopack.
/// Este servicio verifica, descarga e instala actualizaciones desde GitHub Releases.
/// </summary>
public class UpdateService
{
    private readonly UpdateManager? _updateManager;
    private readonly string _currentVersion;

    /// <summary>
    /// Constructor del servicio de actualización.
    /// </summary>
    /// <param name="repoUrl">URL del repositorio de GitHub (ejemplo: "https://github.com/usuario/repo")</param>
    public UpdateService(string repoUrl)
    {
        // Obtener la versión actual del ensamblado
        var version = typeof(UpdateService).Assembly.GetName().Version;
        _currentVersion = version != null 
            ? $"{version.Major}.{version.Minor}.{version.Build}" 
            : "1.0.0";

        Console.WriteLine($"[UpdateService] Inicializando con versión actual: {_currentVersion}");
        Console.WriteLine($"[UpdateService] Repositorio: {repoUrl}");

        try
        {
            // Crear el UpdateManager con la fuente de GitHub
            // GithubSource busca releases en tu repositorio de GitHub
            var source = new GithubSource(repoUrl, null, false);
            _updateManager = new UpdateManager(source);
            Console.WriteLine($"[UpdateService] UpdateManager creado correctamente");
        }
        catch (Exception ex)
        {
            // Si no se puede crear el UpdateManager (por ejemplo, en modo desarrollo),
            // simplemente lo dejamos nulo y el servicio no hará nada
            Console.WriteLine($"[UpdateService] No se pudo inicializar UpdateManager: {ex.Message}");
            Console.WriteLine($"[UpdateService] StackTrace: {ex.StackTrace}");
            _updateManager = null;
        }
    }

    /// <summary>
    /// Obtiene la versión actual de la aplicación instalada.
    /// </summary>
    public string CurrentVersion => _currentVersion;

    /// <summary>
    /// Verifica si hay actualizaciones disponibles en GitHub Releases.
    /// </summary>
    /// <returns>
    /// Información de la actualización si hay una disponible, null si no hay actualizaciones
    /// o si no se puede verificar.
    /// </returns>
    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        Console.WriteLine($"[UpdateService] Verificando actualizaciones...");
        Console.WriteLine($"[UpdateService] UpdateManager is null: {_updateManager == null}");
        
        // Si el UpdateManager no se inicializó (desarrollo o error), retornar null
        if (_updateManager == null)
        {
            Console.WriteLine("[UpdateService] UpdateManager no está disponible. Probablemente estás en modo desarrollo.");
            return null;
        }

        try
        {
            Console.WriteLine($"[UpdateService] Llamando a CheckForUpdatesAsync...");
            
            // Verificar si hay actualizaciones disponibles
            // Esto hace una llamada a la API de GitHub para ver los releases
            var updateInfo = await _updateManager.CheckForUpdatesAsync();
            
            Console.WriteLine($"[UpdateService] UpdateInfo recibido: {updateInfo != null}");
            if (updateInfo != null)
            {
                Console.WriteLine($"[UpdateService] Nueva versión disponible: {updateInfo.TargetFullRelease?.Version}");
            }
            else
            {
                Console.WriteLine($"[UpdateService] No hay actualizaciones disponibles");
            }
            
            // Si hay una actualización disponible, updateInfo no será null
            return updateInfo;
        }
        catch (Exception ex)
        {
            // Detectar si el error es porque no está instalado con Velopack
            if (ex.Message.Contains("not installed") || ex.Message.Contains("can not be performed"))
            {
                Console.WriteLine($"[UpdateService] La aplicación no está instalada con Velopack (modo desarrollo).");
            }
            else
            {
                Console.WriteLine($"[UpdateService] Error al verificar actualizaciones: {ex.Message}");
                Console.WriteLine($"[UpdateService] StackTrace: {ex.StackTrace}");
            }
            return null;
        }
    }

    /// <summary>
    /// Descarga la actualización disponible.
    /// </summary>
    /// <param name="updateInfo">Información de la actualización a descargar</param>
    /// <param name="progress">Callback opcional para reportar el progreso (0-100)</param>
    /// <returns>True si la descarga fue exitosa, False si hubo algún error</returns>
    public async Task<bool> DownloadUpdateAsync(UpdateInfo updateInfo, Action<int>? progress = null)
    {
        if (_updateManager == null)
        {
            Console.WriteLine("UpdateManager no está disponible.");
            return false;
        }

        try
        {
            // Descargar la actualización
            // El progress permite mostrar una barra de progreso al usuario
            await _updateManager.DownloadUpdatesAsync(updateInfo, (p) => 
            {
                progress?.Invoke(p);
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al descargar actualización: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Aplica la actualización descargada y reinicia la aplicación.
    /// Esta acción cierra la aplicación actual e inicia la nueva versión.
    /// </summary>
    /// <param name="updateInfo">Información de la actualización a aplicar</param>
    public void ApplyUpdateAndRestart(UpdateInfo updateInfo)
    {
        if (_updateManager == null)
        {
            Console.WriteLine("UpdateManager no está disponible.");
            return;
        }

        try
        {
            // Aplicar la actualización y reiniciar
            // Esto cierra la aplicación actual, instala la actualización y la reinicia
            _updateManager.ApplyUpdatesAndRestart(updateInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al aplicar actualización: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si la aplicación está instalada (no ejecutándose desde el IDE).
    /// </summary>
    /// <returns>True si está instalada, False si está en desarrollo</returns>
    public bool IsInstalled()
    {
        // UpdateManager no nulo significa que la app está instalada
        // Si es null, estamos en modo desarrollo
        var isInstalled = _updateManager != null;
        Console.WriteLine($"[UpdateService] IsInstalled() llamado: {isInstalled}");
        Console.WriteLine($"[UpdateService] UpdateManager != null: {_updateManager != null}");
        return isInstalled;
    }
}
