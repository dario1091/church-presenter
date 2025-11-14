# Church Presenter

Sistema de presentación multimedia para iglesias, diseñado para proyectar canciones, versículos bíblicos e imágenes durante los servicios.

## ⚠️ LICENCIA COMERCIAL REQUERIDA

**Este software requiere una licencia comercial para su uso.**

- 📖 El código fuente está **disponible públicamente** con fines de transparencia y evaluación
- ⚖️ Ver el código **NO otorga** permiso para usarlo sin licencia
- 💼 Para adquirir una licencia, contactar:
  - 📧 Email: dario1091@gmail.com
  - 📱 WhatsApp: +57 3212113690

**Tipos de licencia disponibles:**
- 🏛️ Licencia Individual (para una iglesia)
- 🏢 Licencia Institucional (para organizaciones grandes)
- 📅 Licencia de Suscripción (mensual/anual)

---

## 📥 Instalación

### Windows

#### Instalación Completa (Recomendada para actualizaciones)

Para una instalación completa que permita actualizaciones automáticas:

1. Descarga `ChurchPresenter-1.0.1-windows.zip` desde [Releases](https://github.com/dario1091/church-presenter/releases)
2. Descomprime en `C:\Program Files\ChurchPresenter` (puede requerir permisos de administrador)
3. Crea un acceso directo al `ChurchPresenter.exe` en el escritorio o menú de inicio.

Ahora puedes ejecutar la aplicación desde el acceso directo. Las actualizaciones se aplicarán automáticamente desde el sistema.

#### Instalación Portable (Sin actualizaciones)

Si prefieres una instalación portable sin actualizaciones:

1. Descarga `ChurchPresenter-1.0.1-windows.zip` desde [Releases](https://github.com/dario1091/church-presenter/releases)
2. Descomprime el archivo en una carpeta de tu elección
3. Ejecuta `ChurchPresenter.exe`

**⚠️ Advertencia de Windows SmartScreen:**

Al ejecutar la aplicación por primera vez, Windows puede mostrar una advertencia de seguridad porque la aplicación aún no está firmada digitalmente. Esto es normal y seguro.

Para continuar:
1. Haz clic en **"Más información"**
2. Luego haz clic en **"Ejecutar de todas formas"**

Esta advertencia desaparecerá en futuras versiones cuando obtengamos un certificado de firma de código.

#### Desinstalación (Windows)

Para eliminar completamente Church Presenter de tu sistema:

1. Elimina el directorio de instalación:
   - Si instalaste en `C:\Program Files\ChurchPresenter`, elimina esa carpeta
   - Si usaste instalación portable, elimina la carpeta donde descomprimiste el archivo

2. (Opcional) Elimina los accesos directos que hayas creado en el escritorio o menú de inicio

3. (Opcional) Elimina los datos de usuario (canciones, caché, configuración):
   - Dirígete a `C:\Users\TU_USUARIO\AppData\Local\ChurchPresenter` y elimina esa carpeta
   - O presiona `Win + R`, escribe `%localappdata%\ChurchPresenter` y elimina la carpeta

### Linux

#### Instalación Completa (Recomendada para actualizaciones)

Para una instalación completa que permita actualizaciones automáticas:

1. Descarga `ChurchPresenter-1.0.1-linux-full.nupkg` desde [Releases](https://github.com/dario1091/church-presenter/releases)
2. Crea un directorio para la instalación:
   ```bash
   sudo mkdir -p /opt/ChurchPresenter
   ```
3. Descomprime el paquete en el directorio:
   ```bash
   sudo unzip ChurchPresenter-1.0.1-linux-full.nupkg -d /opt/ChurchPresenter
   ```
4. **Importante**: Cambia el propietario del directorio a tu usuario para permitir actualizaciones automáticas:
   ```bash
   sudo chown -R $USER:$USER /opt/ChurchPresenter
   ```
5. Da permisos de ejecución al AppImage:
   ```bash
   chmod +x /opt/ChurchPresenter/lib/app/ChurchPresenter.AppImage
   ```
6. Crea un enlace simbólico para ejecutar desde cualquier lugar:
   ```bash
   sudo ln -sf /opt/ChurchPresenter/lib/app/ChurchPresenter.AppImage /usr/local/bin/church-presenter
   ```
7. (Opcional) Crea un archivo .desktop para el menú de aplicaciones:
   ```bash
   sudo tee /usr/share/applications/church-presenter.desktop > /dev/null <<EOF
   [Desktop Entry]
   Name=Church Presenter
   Exec=/opt/ChurchPresenter/lib/app/ChurchPresenter.AppImage
   Icon=/opt/ChurchPresenter/lib/app/ChurchPresenter.png
   Type=Application
   Categories=Utility;
   EOF
   ```

Ahora puedes ejecutar `church-presenter` desde la terminal o buscar "Church Presenter" en el menú de aplicaciones. Las actualizaciones se aplicarán automáticamente desde el sistema.

#### Instalación Portable (Sin actualizaciones)

Si prefieres una instalación portable sin actualizaciones:

1. Descarga `ChurchPresenter.AppImage` desde [Releases](https://github.com/dario1091/church-presenter/releases)
2. Dale permisos de ejecución:
   ```bash
   chmod +x ChurchPresenter.AppImage
   ```
3. Ejecuta la aplicación:
   ```bash
   ./ChurchPresenter.AppImage
   ```

**Nota:** La instalación portable no permite actualizaciones automáticas.

#### Desinstalación (Linux)

Para eliminar completamente Church Presenter de tu sistema:

1. Elimina el enlace simbólico:
   ```bash
   sudo rm -f /usr/local/bin/church-presenter
   ```

2. Elimina el directorio de instalación:
   ```bash
   sudo rm -rf /opt/ChurchPresenter
   ```

3. (Opcional) Si creaste el archivo .desktop, elimínalo:
   ```bash
   sudo rm -f /usr/share/applications/church-presenter.desktop
   ```

4. (Opcional) Elimina los datos de usuario (canciones, caché, configuración):
   ```bash
   rm -rf ~/.local/share/ChurchPresenter
   rm -rf ~/.config/ChurchPresenter
   ```

5. (Opcional) Limpia el caché de Velopack (útil si tienes problemas con actualizaciones):
   ```bash
   rm -rf /var/tmp/velopack/ChurchPresenter/ ~/.local/share/ChurchPresenter/ && echo "Caché de Velopack limpiado"
   ```

---

## Características

### ✅ Implementadas

- **Proyección de Canciones**
  - Gestión completa de canciones (crear, editar, eliminar)
  - Importar/exportar canciones en formato JSON
  - Búsqueda de canciones por título y autor
  - Etiquetas visuales (Coro, Verso, etc.) con fondo azul en previsualización
  - Proyección a pantalla completa sin etiquetas
  - Ajuste automático de texto para múltiples líneas

- **Proyección de Biblia**
  - Múltiples versiones de la Biblia en español (RVR1960, NVI, DHH, NBLA, TLA)
  - Búsqueda semántica de versículos usando IA
  - Búsqueda tradicional por libro, capítulo y versículo
  - Proyección a pantalla completa
  - Ajuste inteligente de texto largo en múltiples líneas

- **Multimedia**
  - Gestión de imágenes organizadas por carpetas
  - Vista previa de imágenes
  - Proyección de imágenes a pantalla completa

- **Interfaz de Usuario**
  - Vista de 3 columnas: Biblioteca, Vista Previa, Proyección
  - Barra de título personalizada con botones de minimizar, maximizar y cerrar
  - Doble clic para proyectar contenido
  - Indicador visual del estado de proyección
  - Soporte para múltiples pantallas

## Requisitos

- .NET 8.0 SDK
- Linux (Fedora, Ubuntu, etc.) / Windows / macOS
- Avalonia UI
- **Licencia comercial válida** para uso en producción

## Evaluación (Solo Desarrolladores)

Si eres desarrollador y deseas **evaluar** el software:

```bash
# Clonar el repositorio
git clone https://github.com/TU_USUARIO/church-presenter.git
cd church-presenter

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar en modo desarrollo
cd ChurchPresenter
dotnet run
```

⚠️ **Nota:** Ejecutar en modo desarrollo es solo para evaluación técnica. El uso en producción requiere licencia comercial.

# Compilar
dotnet build

# Ejecutar
cd ChurchPresenter
dotnet run
```

## Estructura del Proyecto

```
church-presenter/
├── ChurchPresenter/
│   ├── Assets/
│   │   └── Bibles/          # Archivos JSON de biblias
│   ├── Models/
│   │   ├── Bible.cs
│   │   ├── Song.cs
│   │   └── ML/              # Modelos de IA para búsqueda semántica
│   ├── Services/
│   │   ├── BibleService.cs
│   │   ├── EmbeddingService.cs
│   │   ├── SemanticSearchService.cs
│   │   └── SongService.cs
│   ├── ViewModels/
│   ├── Views/
│   └── bin/Debug/net8.0/
│       ├── Songs/           # Canciones guardadas
│       ├── Cache/           # Cache de embeddings
│       └── Media/           # Imágenes multimedia
├── .gitignore
└── README.md
```

## Uso

### Proyectar Canciones
1. Selecciona una canción de la lista
2. Haz doble clic en un verso para proyectarlo
3. Navega entre versos con doble clic

### Proyectar Versículos
1. Ve a la pestaña "Biblia"
2. Busca por palabras clave o referencia específica
3. Haz doble clic en el versículo para proyectarlo

### Proyectar Imágenes
1. Ve a la pestaña "Multimedia"
2. Organiza imágenes en carpetas
3. Haz doble clic en una imagen para proyectarla

## Tecnologías

- **Framework**: .NET 8.0
- **UI**: Avalonia UI
- **MVVM**: CommunityToolkit.Mvvm
- **IA**: Microsoft.ML.OnnxRuntime (para búsqueda semántica)
- **Actualizaciones**: Velopack (sistema de actualización automática)
- **Formato de datos**: JSON

## Preguntas Frecuentes

### ¿Por qué el código es público si requiere licencia?
El código público permite transparencia, auditoría de seguridad y evaluación antes de comprar. Ver el código no otorga derechos de uso.

### ¿Puedo probarlo antes de comprar?
Sí, los desarrolladores pueden ejecutarlo en modo desarrollo para evaluación. Contacta para una demo o período de prueba.

### ¿Qué incluye la licencia?
- Derecho de uso del software
- Actualizaciones automáticas
- Soporte técnico (según tipo de licencia)
- Manual de usuario

## Licencia

Este software está bajo **Licencia Propietaria**. Ver archivo [LICENSE](LICENSE) para términos completos.

**Resumen:**
- ❌ No se permite usar sin licencia comercial
- ❌ No se permite redistribuir
- ❌ No se permite modificar o crear trabajos derivados
- ✅ El código es visible para transparencia y evaluación

## Contacto

**Para adquirir licencias o consultas:**

- 📧 Email: dario1091@gmail.com
- 📱 WhatsApp: +57 3212113690

---

**© 2025 Jose Dario Paez Perez. Todos los derechos reservados.**
