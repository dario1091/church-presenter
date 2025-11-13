using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChurchPresenter.Services;
using Velopack;

namespace ChurchPresenter.ViewModels;

/// <summary>
/// ViewModel para manejar la interfaz de actualizaciones.
/// Controla el estado de verificación, descarga e instalación de actualizaciones.
/// </summary>
public partial class UpdateViewModel : ViewModelBase
{
    private readonly UpdateService _updateService;
    private UpdateInfo? _currentUpdateInfo;

    // ===== PROPIEDADES OBSERVABLES =====
    
    /// <summary>
    /// Versión actual de la aplicación instalada.
    /// </summary>
    [ObservableProperty]
    private string currentVersion = "1.0.0";

    /// <summary>
    /// Versión nueva disponible para descargar.
    /// </summary>
    [ObservableProperty]
    private string? newVersion;

    /// <summary>
    /// Mensaje de estado actual (ej: "Verificando actualizaciones...", "Descargando...").
    /// </summary>
    [ObservableProperty]
    private string statusMessage = "Versión actual";

    /// <summary>
    /// Indica si hay una actualización disponible.
    /// </summary>
    [ObservableProperty]
    private bool updateAvailable;

    /// <summary>
    /// Indica si se está verificando actualizaciones.
    /// </summary>
    [ObservableProperty]
    private bool isChecking;

    /// <summary>
    /// Indica si se está descargando una actualización.
    /// </summary>
    [ObservableProperty]
    private bool isDownloading;

    /// <summary>
    /// Indica si la actualización está lista para instalar.
    /// </summary>
    [ObservableProperty]
    private bool updateReadyToInstall;

    /// <summary>
    /// Progreso de la descarga (0-100).
    /// </summary>
    [ObservableProperty]
    private int downloadProgress;

    /// <summary>
    /// Mensaje de error si algo sale mal.
    /// </summary>
    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Constructor del ViewModel.
    /// </summary>
    /// <param name="updateService">Servicio de actualización inyectado</param>
    public UpdateViewModel(UpdateService updateService)
    {
        _updateService = updateService;
        CurrentVersion = _updateService.CurrentVersion;
    }

    // ===== COMANDOS =====

    /// <summary>
    /// Comando para verificar si hay actualizaciones disponibles.
    /// </summary>
    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        // Limpiar estados anteriores
        ErrorMessage = null;
        UpdateAvailable = false;
        UpdateReadyToInstall = false;
        IsChecking = true;
        StatusMessage = "Verificando actualizaciones...";

        try
        {
            // Llamar al servicio para verificar actualizaciones
            _currentUpdateInfo = await _updateService.CheckForUpdatesAsync();

            if (_currentUpdateInfo != null)
            {
                // Hay una actualización disponible
                NewVersion = _currentUpdateInfo.TargetFullRelease.Version.ToString();
                UpdateAvailable = true;
                StatusMessage = $"Nueva versión {NewVersion} disponible";
                
                // Notificar al comando de descarga que revise si puede ejecutarse
                DownloadUpdateCommand.NotifyCanExecuteChanged();
            }
            else
            {
                // No hay actualizaciones o no se pudo verificar
                // Si no está instalado, el servicio ya devolvió null
                if (!_updateService.IsInstalled())
                {
                    StatusMessage = "Modo desarrollo - Actualizaciones no disponibles";
                    ErrorMessage = "Las actualizaciones automáticas solo funcionan cuando la aplicación está instalada. Estás ejecutando desde el IDE.";
                }
                else
                {
                    StatusMessage = "Estás usando la última versión";
                }
            }
        }
        catch (Exception ex)
        {
            // Manejo de errores
            ErrorMessage = $"Error al verificar actualizaciones: {ex.Message}";
            StatusMessage = "Error al verificar actualizaciones";
        }
        finally
        {
            IsChecking = false;
        }
    }

    /// <summary>
    /// Comando para descargar la actualización disponible.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDownload))]
    private async Task DownloadUpdateAsync()
    {
        if (_currentUpdateInfo == null)
            return;

        ErrorMessage = null;
        IsDownloading = true;
        DownloadProgress = 0;
        StatusMessage = "Descargando actualización...";

        try
        {
            // Descargar la actualización con callback de progreso
            var success = await _updateService.DownloadUpdateAsync(_currentUpdateInfo, (progress) =>
            {
                // Actualizar el progreso en el hilo de la UI
                DownloadProgress = progress;
                StatusMessage = $"Descargando... {progress}%";
            });

            if (success)
            {
                // Descarga completada exitosamente
                UpdateReadyToInstall = true;
                UpdateAvailable = false;
                StatusMessage = "Actualización descargada. Lista para instalar.";
                
                // Notificar al comando de instalación que revise si puede ejecutarse
                InstallAndRestartCommand.NotifyCanExecuteChanged();
            }
            else
            {
                ErrorMessage = "No se pudo descargar la actualización";
                StatusMessage = "Error en la descarga";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al descargar: {ex.Message}";
            StatusMessage = "Error al descargar actualización";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    /// <summary>
    /// Determina si se puede ejecutar el comando de descarga.
    /// </summary>
    private bool CanDownload()
    {
        // Solo permitir descarga si está instalado con Velopack
        return UpdateAvailable && !IsDownloading && !UpdateReadyToInstall && _updateService.IsInstalled();
    }

    /// <summary>
    /// Comando para instalar la actualización y reiniciar la aplicación.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanInstall))]
    private void InstallAndRestart()
    {
        if (_currentUpdateInfo == null)
            return;

        try
        {
            StatusMessage = "Instalando actualización...";
            
            // IMPORTANTE: Este método CIERRA la aplicación y la reinicia con la nueva versión
            _updateService.ApplyUpdateAndRestart(_currentUpdateInfo);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al instalar: {ex.Message}";
            StatusMessage = "Error al instalar actualización";
        }
    }

    /// <summary>
    /// Determina si se puede ejecutar el comando de instalación.
    /// </summary>
    private bool CanInstall()
    {
        return UpdateReadyToInstall && !IsDownloading;
    }

    /// <summary>
    /// Indica si la aplicación está instalada (no en modo desarrollo).
    /// </summary>
    public bool IsInstalled => _updateService.IsInstalled();
}
