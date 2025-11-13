# 📘 DOCUMENTACIÓN COMPLETA - ChurchPresenter

## 📋 Índice
1. [¿Qué es ChurchPresenter?](#qué-es-churchpresenter)
2. [Tecnologías Utilizadas](#tecnologías-utilizadas)
3. [Entendiendo los Archivos](#entendiendo-los-archivos)
4. [Estructura del Proyecto](#estructura-del-proyecto)
5. [Guía de Modificaciones Comunes](#guía-de-modificaciones-comunes)
6. [Cómo Funciona la Aplicación](#cómo-funciona-la-aplicación)

---

## 🎯 ¿Qué es ChurchPresenter?

ChurchPresenter es una aplicación de escritorio para presentaciones en iglesias. Permite proyectar:
- **Canciones** (con versos, coros, puentes)
- **Versículos bíblicos** (con búsqueda inteligente)
- **Imágenes multimedia**

La aplicación tiene 3 columnas principales:
1. **Columna 1**: Biblioteca de contenido (lista de canciones, búsqueda de biblia, carpetas de imágenes)
2. **Columna 2**: Vista previa del contenido seleccionado
3. **Columna 3**: Vista de lo que se está proyectando actualmente

---

## 🛠️ Tecnologías Utilizadas

### **Lenguaje: C# (C Sharp)**
- **¿Qué es?**: Un lenguaje de programación moderno creado por Microsoft
- **Extensión de archivo**: `.cs`
- **Similar a**: Java, JavaScript (pero con sintaxis diferente)
- **Características**:
  - Fuertemente tipado (debes declarar tipos de datos)
  - Orientado a objetos (todo son clases y objetos)
  - Usa punto y coma `;` al final de cada instrucción

### **Framework: .NET 8.0**
- **¿Qué es?**: Plataforma de desarrollo de Microsoft para crear aplicaciones
- **Versión**: 8.0 (moderna y multiplataforma)
- **Compatibilidad**: Windows, Linux, macOS

### **UI Framework: Avalonia UI**
- **¿Qué es?**: Framework para crear interfaces gráficas multiplataforma
- **Similar a**: WPF (Windows), React Native (móvil)
- **Extensión de archivos**: `.axaml` (Avalonia XAML)
- **Características**:
  - Separa la interfaz (XAML) de la lógica (C#)
  - Usa patrón MVVM (Model-View-ViewModel)

### **Patrón de Arquitectura: MVVM**
```
┌─────────────┐     ┌──────────────┐     ┌───────────┐
│   View      │────▶│  ViewModel   │────▶│   Model   │
│ (Interfaz)  │◀────│   (Lógica)   │◀────│  (Datos)  │
└─────────────┘     └──────────────┘     └───────────┘
```

---

## 📁 Entendiendo los Archivos

### 1. **Archivos `.cs` (C Sharp)**
**Propósito**: Contienen el código de programación (lógica)

**Tipos de archivos .cs en el proyecto**:

#### **Models/** (Modelos de datos)
```csharp
// Models/Song.cs - Define cómo es una canción
public class Song
{
    public string Title { get; set; }      // Título de la canción
    public string Author { get; set; }     // Autor
    public List<Verse> Verses { get; set; } // Lista de versos
}
```
- **Qué hacen**: Definen la estructura de los datos
- **Ejemplo**: Una canción tiene título, autor, y lista de versos

#### **ViewModels/** (Lógica de la interfaz)
```csharp
// ViewModels/SongsViewModel.cs
public class SongsViewModel : ViewModelBase
{
    [ObservableProperty]
    private Song? selectedSong; // Canción seleccionada
    
    [RelayCommand]
    private void AddSong() { ... } // Comando para agregar canción
}
```
- **Qué hacen**: Manejan la lógica de cada pantalla
- **Contienen**: Comandos (botones), propiedades (datos visibles), métodos

#### **Views/** (Archivo de código detrás de la interfaz)
```csharp
// Views/MainWindow.axaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent(); // Inicializa la interfaz
    }
}
```
- **Qué hacen**: Conectan la interfaz con eventos especiales
- **Normalmente**: Solo tienen inicialización, la lógica está en ViewModels

#### **Services/** (Servicios de negocio)
```csharp
// Services/SongService.cs
public class SongService
{
    public async Task<List<Song>> LoadAllSongsAsync() { ... }
    public async Task SaveSongAsync(Song song) { ... }
}
```
- **Qué hacen**: Manejan operaciones complejas (guardar, cargar archivos)
- **Ejemplo**: SongService guarda y carga canciones desde JSON

### 2. **Archivos `.axaml` (Avalonia XAML)**
**Propósito**: Definen la interfaz visual (como HTML para web)

**Estructura básica**:
```xml
<!-- SongsView.axaml - Vista de canciones -->
<UserControl>
  <Grid ColumnDefinitions="1*,1*,1*">  <!-- 3 columnas iguales -->
    
    <!-- COLUMNA 1: Lista de canciones -->
    <Border Grid.Column="0">
      <ListBox ItemsSource="{Binding FilteredSongs}">
        <ListBox.ItemTemplate>
          <DataTemplate>
            <StackPanel>
              <TextBlock Text="{Binding Title}"/>      <!-- Título -->
              <TextBlock Text="{Binding Author}"/>     <!-- Autor -->
            </StackPanel>
          </DataTemplate>
        </ListBox.ItemTemplate>
      </ListBox>
    </Border>
    
    <!-- COLUMNA 2: Vista previa -->
    <Border Grid.Column="1">
      <TextBlock Text="{Binding PreviewText}"/>
    </Border>
    
    <!-- COLUMNA 3: Proyección -->
    <Border Grid.Column="2">
      <TextBlock Text="{Binding ProjectionText}"/>
    </Border>
    
  </Grid>
</UserControl>
```

**Elementos comunes en XAML**:
- `<Grid>`: Contenedor con filas y columnas
- `<StackPanel>`: Apila elementos vertical u horizontalmente
- `<Border>`: Caja con borde, fondo y esquinas redondeadas
- `<TextBlock>`: Texto no editable
- `<TextBox>`: Cuadro de texto editable
- `<Button>`: Botón clickeable
- `<ListBox>`: Lista de elementos

**Binding (enlace de datos)**:
```xml
<!-- Conecta la interfaz con el ViewModel -->
<TextBox Text="{Binding SearchText}"/>
<!-- SearchText es una propiedad en el ViewModel -->
```

### 3. **Archivos `.axaml.cs`**
**Propósito**: Código detrás del archivo `.axaml`

```csharp
// EditSongDialog.axaml.cs
public partial class EditSongDialog : Window
{
    public EditSongDialog()
    {
        InitializeComponent(); // Carga el XAML
    }
    
    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        // Cuando se hace clic en guardar
        this.Close(true); // Cierra el diálogo
    }
}
```
- **Relación**: `EditSongDialog.axaml.cs` es el código de `EditSongDialog.axaml`
- **Uso común**: Manejar eventos de clic, abrir/cerrar ventanas

### 4. **Archivo `.csproj`**
**Propósito**: Archivo de configuración del proyecto

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>  <!-- Versión de .NET -->
  </PropertyGroup>
  
  <ItemGroup>
    <!-- Paquetes/Librerías que usa el proyecto -->
    <PackageReference Include="Avalonia" Version="11.3.8" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.1" />
  </ItemGroup>
</Project>
```

---

## 🏗️ Estructura del Proyecto

```
ChurchPresenter/
│
├── 📄 App.axaml                    # Configuración global de la app + COLORES
├── 📄 App.axaml.cs                 # Código de inicialización
├── 📄 Program.cs                   # Punto de entrada de la aplicación
├── 📄 ChurchPresenter.csproj       # Configuración del proyecto
│
├── 📁 Models/                      # DATOS: Define estructura de datos
│   ├── Song.cs                     # Modelo de canción (título, autor, versos)
│   └── Bible.cs                    # Modelo de biblia (libros, capítulos, versículos)
│
├── 📁 ViewModels/                  # LÓGICA: Maneja el comportamiento
│   ├── MainWindowViewModel.cs     # Lógica de ventana principal
│   ├── SongsViewModel.cs          # Lógica de gestión de canciones
│   ├── BibleViewModel.cs          # Lógica de búsqueda bíblica
│   ├── MultimediaViewModel.cs     # Lógica de imágenes
│   └── PresentationViewModel.cs   # Lógica de proyección
│
├── 📁 Views/                       # INTERFAZ: Define cómo se ve
│   ├── MainWindow.axaml           # Ventana principal (3 columnas)
│   ├── MainWindow.axaml.cs
│   ├── SongsView.axaml            # Vista de canciones
│   ├── SongsView.axaml.cs
│   ├── BibleView.axaml            # Vista de biblia
│   ├── EditSongDialog.axaml       # Diálogo de edición de canciones
│   └── PresentationWindow.axaml   # Ventana de proyección
│
├── 📁 Services/                    # SERVICIOS: Operaciones complejas
│   ├── SongService.cs             # Guardar/cargar canciones
│   ├── BibleService.cs            # Cargar biblias
│   └── SemanticSearchService.cs   # Búsqueda inteligente
│
├── 📁 Converters/                  # CONVERTIDORES: Transforman datos para UI
│   └── BoolConverters.cs
│
└── 📁 Assets/                      # RECURSOS: Archivos estáticos
    └── Bibles/                     # Archivos JSON de biblias
```

---

## 🎨 Guía de Modificaciones Comunes

### ✏️ 1. Cambiar los Colores de la Aplicación

**Archivo a modificar**: `App.axaml`

**Ubicación en el archivo**: Líneas 14-105 (aproximadamente)

**Cómo funciona**:
```xml
<!-- App.axaml -->
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.ThemeDictionaries>
      
      <!-- TEMA OSCURO -->
      <ResourceDictionary x:Key="Dark">
        <!-- Color principal (botones, selecciones) -->
        <Color x:Key="PrimaryColor">#6366F1</Color>  <!-- 👈 CAMBIA ESTE -->
        
        <!-- Color de acento (resaltados) -->
        <Color x:Key="AccentColor">#F59E0B</Color>   <!-- 👈 CAMBIA ESTE -->
        
        <!-- Colores de fondo -->
        <Color x:Key="BackgroundColor">#0F172A</Color>  <!-- 👈 Fondo principal -->
        <Color x:Key="SurfaceColor">#1E293B</Color>     <!-- 👈 Fondo de tarjetas -->
        <Color x:Key="CardColor">#334155</Color>        <!-- 👈 Fondo de elementos -->
        
        <!-- Colores de texto -->
        <Color x:Key="TextPrimaryColor">#F1F5F9</Color>    <!-- 👈 Texto principal -->
        <Color x:Key="TextSecondaryColor">#94A3B8</Color>  <!-- 👈 Texto secundario -->
      </ResourceDictionary>
      
      <!-- TEMA CLARO -->
      <ResourceDictionary x:Key="Light">
        <!-- (Mismas propiedades pero para tema claro) -->
      </ResourceDictionary>
      
    </ResourceDictionary.ThemeDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

**Ejemplo: Cambiar a colores verdes**
```xml
<Color x:Key="PrimaryColor">#10B981</Color>    <!-- Verde -->
<Color x:Key="AccentColor">#F59E0B</Color>     <!-- Naranja (mantener) -->
<Color x:Key="BackgroundColor">#001a0f</Color> <!-- Verde muy oscuro -->
```

**Formato de colores**:
- `#RRGGBB` donde:
  - `RR` = Rojo (00-FF en hexadecimal)
  - `GG` = Verde (00-FF en hexadecimal)
  - `BB` = Azul (00-FF en hexadecimal)
- Ejemplo: `#FF0000` = Rojo puro, `#00FF00` = Verde puro, `#0000FF` = Azul puro

**Herramientas útiles**:
- https://coolors.co/ (generador de paletas)
- https://htmlcolorcodes.com/ (selector de colores)

---

### 📊 2. Agregar una Nueva Columna a la Interfaz

**Escenario**: Quieres agregar una 4ta columna para mostrar notas adicionales

**Archivos a modificar**:

#### **Paso 1: Modificar el XAML (Interfaz)**
**Archivo**: `Views/SongsView.axaml`

```xml
<!-- ANTES: 3 columnas -->
<Grid ColumnDefinitions="1*,1*,1*">
  <Border Grid.Column="0"><!-- Columna 1 --></Border>
  <Border Grid.Column="1"><!-- Columna 2 --></Border>
  <Border Grid.Column="2"><!-- Columna 3 --></Border>
</Grid>

<!-- DESPUÉS: 4 columnas -->
<Grid ColumnDefinitions="1*,1*,1*,1*">  <!-- 👈 Agregamos 1* -->
  <Border Grid.Column="0"><!-- Columna 1 --></Border>
  <Border Grid.Column="1"><!-- Columna 2 --></Border>
  <Border Grid.Column="2"><!-- Columna 3 --></Border>
  
  <!-- 👇 NUEVA COLUMNA 4 -->
  <Border Grid.Column="3" 
          Background="{DynamicResource SurfaceBrush}"
          BorderBrush="{DynamicResource BorderBrush}"
          BorderThickness="1"
          CornerRadius="12"
          Margin="8">
    <StackPanel Padding="20">
      <TextBlock Text="Notas" FontSize="20" FontWeight="Bold"/>
      <TextBlock Text="{Binding CurrentNotes}" 
                 TextWrapping="Wrap"
                 Margin="0,10,0,0"/>
    </StackPanel>
  </Border>
</Grid>
```

**Explicación**:
- `ColumnDefinitions="1*,1*,1*,1*"`: 4 columnas de igual ancho
  - `*` significa "proporcional"
  - `1*,2*,1*` = columna del medio es el doble de ancha
- `Grid.Column="3"`: Coloca el elemento en la columna 4 (se cuenta desde 0)

#### **Paso 2: Agregar la Propiedad en el ViewModel**
**Archivo**: `ViewModels/SongsViewModel.cs`

```csharp
// ViewModels/SongsViewModel.cs

public partial class SongsViewModel : ViewModelBase
{
    // ... propiedades existentes ...
    
    // 👇 AGREGAR ESTA NUEVA PROPIEDAD
    [ObservableProperty]
    private string currentNotes = string.Empty;
    
    // Actualizar las notas cuando se selecciona una canción
    partial void OnSelectedSongChanged(Song? value)
    {
        if (value != null)
        {
            // 👇 AGREGAR ESTA LÍNEA
            CurrentNotes = value.Notes ?? "Sin notas";
        }
        else
        {
            CurrentNotes = string.Empty;
        }
    }
}
```

**Explicación**:
- `[ObservableProperty]`: Genera automáticamente notificaciones cuando el valor cambia
- `partial void OnSelectedSongChanged`: Se ejecuta cuando cambia la canción seleccionada

---

### 🎵 3. Cambiar el Formato de las Canciones

**Escenario**: Actualmente las canciones usan formato:
```
VERSE I
Contenido del verso 1

CHORUS
Contenido del coro
```

Quieres cambiarlo a:
```
[Verso 1]
Contenido del verso 1

[Coro]
Contenido del coro
```

**Archivos a modificar**:

#### **Paso 1: Cambiar el Modelo (opcional)**
**Archivo**: `Models/Song.cs`

```csharp
// El modelo actual está bien, pero podrías agregar más tipos:
public enum VerseType
{
    Verse,
    Chorus,
    Bridge,
    PreChorus,
    Intro,      // 👈 NUEVO
    Outro       // 👈 NUEVO
}
```

#### **Paso 2: Modificar el Parser (Análisis de Texto)**
**Archivo**: `ViewModels/EditSongViewModel.cs`

Busca el método `ParseLyrics` (probablemente no está visible, pero está ahí). Deberías encontrar algo como:

```csharp
// ANTES: Detecta "VERSE I", "CHORUS", etc.
private List<Verse> ParseLyrics(string lyrics)
{
    var verses = new List<Verse>();
    var lines = lyrics.Split('\n');
    
    foreach (var line in lines)
    {
        if (line.StartsWith("VERSE"))
        {
            // Crea un nuevo verso
        }
        else if (line.StartsWith("CHORUS"))
        {
            // Crea un coro
        }
    }
    return verses;
}
```

**Cámbialo a**:

```csharp
// DESPUÉS: Detecta "[Verso 1]", "[Coro]", etc.
private List<Verse> ParseLyrics(string lyrics)
{
    var verses = new List<Verse>();
    var lines = lyrics.Split('\n');
    VerseType currentType = VerseType.Verse;
    var currentContent = new List<string>();
    
    foreach (var line in lines)
    {
        // 👇 NUEVO: Detectar formato [Verso 1]
        if (line.StartsWith("[") && line.Contains("]"))
        {
            // Guardar verso anterior si existe
            if (currentContent.Any())
            {
                verses.Add(new Verse
                {
                    Type = currentType,
                    Content = string.Join("\n", currentContent),
                    Label = GetLabelFromType(currentType)
                });
                currentContent.Clear();
            }
            
            // Detectar tipo del nuevo verso
            var label = line.Trim('[', ']').ToLower();
            if (label.Contains("verso"))
                currentType = VerseType.Verse;
            else if (label.Contains("coro"))
                currentType = VerseType.Chorus;
            else if (label.Contains("puente"))
                currentType = VerseType.Bridge;
        }
        else if (!string.IsNullOrWhiteSpace(line))
        {
            currentContent.Add(line);
        }
    }
    
    // Guardar último verso
    if (currentContent.Any())
    {
        verses.Add(new Verse
        {
            Type = currentType,
            Content = string.Join("\n", currentContent)
        });
    }
    
    return verses;
}
```

#### **Paso 3: Actualizar el Placeholder en la Interfaz**
**Archivo**: `Views/EditSongDialog.axaml`

```xml
<!-- ANTES -->
<TextBlock Text="Use VERSE I, VERSE II, etc. para versos y CHORUS para el coro" 
           FontStyle="Italic"/>

<!-- DESPUÉS -->
<TextBlock Text="Use [Verso 1], [Verso 2] para versos y [Coro] para el coro" 
           FontStyle="Italic"/>
```

---

### 🔤 4. Cambiar el Tamaño de la Fuente en la Proyección

**Archivo a modificar**: `Views/PresentationWindow.axaml`

```xml
<!-- PresentationWindow.axaml -->
<TextBlock Text="{Binding CurrentText}"
           FontSize="48"           <!-- 👈 CAMBIA ESTE NÚMERO -->
           FontWeight="Bold"
           Foreground="White"
           TextAlignment="Center"
           TextWrapping="Wrap"/>
```

**Valores recomendados**:
- **Pequeño**: `36`
- **Mediano**: `48` (actual)
- **Grande**: `64`
- **Muy grande**: `80`

**Hacer que sea configurable** (más avanzado):

1. Agregar propiedad en `PresentationViewModel.cs`:
```csharp
[ObservableProperty]
private double fontSize = 48;
```

2. Cambiar el XAML:
```xml
<TextBlock FontSize="{Binding FontSize}"/>
```

3. Agregar controles en la interfaz para cambiarlo:
```xml
<Slider Minimum="24" 
        Maximum="120" 
        Value="{Binding FontSize}"
        Width="200"/>
```

---

### 📝 5. Agregar un Nuevo Campo a las Canciones

**Escenario**: Quieres agregar el año de la canción

#### **Paso 1: Modificar el Modelo**
**Archivo**: `Models/Song.cs`

```csharp
public class Song
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; } = 0;  // 👈 NUEVO CAMPO
    public List<Verse> Verses { get; set; } = new();
    public string? Notes { get; set; }
    public string FilePath { get; set; } = string.Empty;
}
```

#### **Paso 2: Actualizar el Formulario de Edición**
**Archivo**: `Views/EditSongDialog.axaml`

```xml
<StackPanel Margin="20" Spacing="10">
  <TextBlock Text="Título:"/>
  <TextBox Text="{Binding Title}"/>
  
  <TextBlock Text="Autor:"/>
  <TextBox Text="{Binding Author}"/>
  
  <!-- 👇 NUEVO CAMPO -->
  <TextBlock Text="Año:"/>
  <TextBox Text="{Binding Year}"/>
  
  <TextBlock Text="Letra:" Margin="0,10,0,0"/>
  <!-- ... resto del formulario ... -->
</StackPanel>
```

#### **Paso 3: Actualizar el ViewModel**
**Archivo**: `ViewModels/EditSongViewModel.cs`

```csharp
public partial class EditSongViewModel : ViewModelBase
{
    [ObservableProperty]
    private string title = string.Empty;
    
    [ObservableProperty]
    private string author = string.Empty;
    
    [ObservableProperty]
    private int year = 0;  // 👈 NUEVA PROPIEDAD
    
    public void LoadSong(Song song)
    {
        Title = song.Title;
        Author = song.Author;
        Year = song.Year;  // 👈 CARGAR NUEVO CAMPO
        // ...
    }
    
    public Song ToSong()
    {
        return new Song
        {
            Title = Title,
            Author = Author,
            Year = Year,  // 👈 GUARDAR NUEVO CAMPO
            // ...
        };
    }
}
```

#### **Paso 4: Mostrar en la Lista**
**Archivo**: `Views/SongsView.axaml`

```xml
<ListBox.ItemTemplate>
  <DataTemplate>
    <StackPanel>
      <TextBlock Text="{Binding Title}" FontWeight="Bold"/>
      <TextBlock Text="{Binding Author}" FontSize="12" Opacity="0.8"/>
      <!-- 👇 MOSTRAR AÑO -->
      <TextBlock Text="{Binding Year, StringFormat='{}Año: {0}'}" 
                 FontSize="10" 
                 Opacity="0.6"/>
    </StackPanel>
  </DataTemplate>
</ListBox.ItemTemplate>
```

---

### 🎨 6. Cambiar el Estilo de los Botones

**Archivo a modificar**: `App.axaml`

**Agregar después de las definiciones de colores**:

```xml
<!-- App.axaml -->
<Application.Styles>
  <themes:FluentTheme/>
  
  <!-- 👇 AGREGAR ESTILOS PERSONALIZADOS -->
  <Style Selector="Button">
    <Setter Property="Background" Value="{DynamicResource PrimaryBrush}"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="Padding" Value="16,8"/>
    <Setter Property="CornerRadius" Value="8"/>       <!-- Esquinas redondeadas -->
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
  </Style>
  
  <!-- Efecto hover (al pasar el mouse) -->
  <Style Selector="Button:pointerover">
    <Setter Property="Background" Value="{DynamicResource PrimaryLightBrush}"/>
  </Style>
  
  <!-- Efecto al presionar -->
  <Style Selector="Button:pressed">
    <Setter Property="Background" Value="{DynamicResource PrimaryDarkBrush}"/>
  </Style>
</Application.Styles>
```

**Crear botones con colores específicos**:

```xml
<!-- Botón de peligro (rojo) -->
<Button Classes="danger" Content="Eliminar"/>

<!-- Agregar estilo en App.axaml -->
<Style Selector="Button.danger">
  <Setter Property="Background" Value="#EF4444"/>
</Style>
<Style Selector="Button.danger:pointerover">
  <Setter Property="Background" Value="#DC2626"/>
</Style>
```

---

### 🔍 7. Modificar la Búsqueda de Canciones

**Archivo actual**: `ViewModels/SongsViewModel.cs`

**Búsqueda actual**: Solo busca por título y autor

```csharp
private void UpdateFilteredSongs()
{
    if (string.IsNullOrWhiteSpace(SearchText))
    {
        FilteredSongs = new ObservableCollection<Song>(_allSongs);
    }
    else
    {
        var filtered = _allSongs.Where(s =>
            s.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            s.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
        ).ToList();
        
        FilteredSongs = new ObservableCollection<Song>(filtered);
    }
}
```

**Mejorar para buscar también en la letra**:

```csharp
private void UpdateFilteredSongs()
{
    if (string.IsNullOrWhiteSpace(SearchText))
    {
        FilteredSongs = new ObservableCollection<Song>(_allSongs);
    }
    else
    {
        var searchLower = SearchText.ToLower();
        var filtered = _allSongs.Where(s =>
            s.Title.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
            s.Author.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
            // 👇 AGREGAR BÚSQUEDA EN LETRA
            s.Verses.Any(v => v.Content.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
        ).ToList();
        
        FilteredSongs = new ObservableCollection<Song>(filtered);
    }
}
```

---

## 🔄 Cómo Funciona la Aplicación (Flujo de Datos)

### Flujo: Seleccionar y Proyectar una Canción

```
1. Usuario hace clic en una canción
   ↓
2. SongsView.axaml detecta el clic
   ↓
3. Se actualiza SelectedSong en SongsViewModel
   ↓
4. OnSelectedSongChanged() se ejecuta
   ↓
5. Se llena la columna 2 (Preview) con los versos
   ↓
6. Usuario hace doble clic en un verso
   ↓
7. ProjectVerse() se ejecuta en SongsViewModel
   ↓
8. Se actualiza PresentationViewModel.CurrentText
   ↓
9. PresentationWindow muestra el texto
   ↓
10. Se actualiza la columna 3 (Projection) con el mismo texto
```

### Flujo: Agregar una Nueva Canción

```
1. Usuario hace clic en botón "+"
   ↓
2. AddSongCommand se ejecuta en SongsViewModel
   ↓
3. Se abre EditSongDialog (ventana emergente)
   ↓
4. Usuario llena el formulario (título, autor, letra)
   ↓
5. Usuario hace clic en "Guardar"
   ↓
6. OnSaveClick() en EditSongDialog.axaml.cs
   ↓
7. ToSong() convierte los datos a un objeto Song
   ↓
8. SongService.SaveSongAsync() guarda la canción en JSON
   ↓
9. Se recarga la lista de canciones
   ↓
10. La nueva canción aparece en la lista
```

### Flujo: Cambiar el Color de un Botón

```
1. Modificas App.axaml (PrimaryColor)
   ↓
2. La aplicación se reinicia o recarga
   ↓
3. Todos los elementos con Background="{DynamicResource PrimaryBrush}"
   toman el nuevo color automáticamente
```

---

## 🧪 Cómo Probar tus Cambios

### Compilar y Ejecutar

```bash
# Desde la terminal en la carpeta del proyecto
cd ChurchPresenter

# Compilar (busca errores)
dotnet build

# Si no hay errores, ejecutar
dotnet run
```

### Errores Comunes

1. **Error: Property not found**
   - Olvidaste declarar una propiedad en el ViewModel
   - Solución: Agregar `[ObservableProperty]` antes de la propiedad

2. **Error: Cannot resolve symbol**
   - Falta un `using` al inicio del archivo
   - Solución: Agregar `using ChurchPresenter.Models;` (o el namespace necesario)

3. **La interfaz no se actualiza**
   - La propiedad no es `ObservableProperty`
   - Solución: Cambiar `private string myProp;` a `[ObservableProperty] private string myProp;`

4. **Binding not found**
   - El nombre en XAML no coincide con el ViewModel
   - Solución: Asegurar que `Text="{Binding MyProperty}"` coincida con `[ObservableProperty] private string myProperty;`

---

## 📚 Recursos de Aprendizaje

### Para C#
- **Microsoft Learn**: https://learn.microsoft.com/dotnet/csharp/
- **Tutorial básico**: https://www.w3schools.com/cs/

### Para Avalonia UI
- **Documentación oficial**: https://docs.avaloniaui.net/
- **Ejemplos**: https://github.com/AvaloniaUI/Avalonia.Samples

### Para MVVM
- **Guía de CommunityToolkit**: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/

---

## 🎓 Conceptos Clave para Recordar

### 1. **Separación de Responsabilidades**
- **Models**: Solo datos (como una base de datos en memoria)
- **ViewModels**: Solo lógica (cómo se comporta)
- **Views**: Solo interfaz (cómo se ve)

### 2. **Data Binding (Enlace de Datos)**
```xml
<!-- La interfaz se conecta automáticamente con el ViewModel -->
<TextBox Text="{Binding SearchText}"/>
```
- Cuando cambias `SearchText` en el ViewModel, el `TextBox` se actualiza solo
- Cuando el usuario escribe en el `TextBox`, `SearchText` se actualiza solo

### 3. **Comandos (Commands)**
```csharp
[RelayCommand]
private void AddSong() { ... }
```
- Se genera automáticamente `AddSongCommand`
- Se conecta en XAML: `<Button Command="{Binding AddSongCommand}"/>`

### 4. **Propiedades Observables**
```csharp
[ObservableProperty]
private string title = "";
```
- Se genera automáticamente `Title` (con mayúscula)
- Notifica automáticamente a la interfaz cuando cambia

---

## 🚀 Ejercicios Prácticos

### Ejercicio 1: Cambiar Color de Acento
1. Abre `App.axaml`
2. Encuentra `<Color x:Key="AccentColor">#F59E0B</Color>`
3. Cámbialo a `#10B981` (verde)
4. Ejecuta con `dotnet run`
5. Observa los cambios en botones y resaltados

### Ejercicio 2: Agregar Campo "Categoría" a Canciones
1. Modifica `Models/Song.cs` → Agrega `public string Category { get; set; } = "";`
2. Modifica `Views/EditSongDialog.axaml` → Agrega `<TextBox Text="{Binding Category}"/>`
3. Modifica `ViewModels/EditSongViewModel.cs` → Agrega `[ObservableProperty] private string category = "";`
4. Actualiza `LoadSong()` y `ToSong()` para incluir `Category`
5. Compila y prueba

### Ejercicio 3: Cambiar Tamaño de Fuente
1. Abre `Views/PresentationWindow.axaml`
2. Encuentra la etiqueta `<TextBlock>` con el texto de proyección
3. Cambia `FontSize="48"` a `FontSize="64"`
4. Ejecuta y proyecta un texto para ver la diferencia

---

## 📞 Solución de Problemas

### "No se ven mis cambios"
- Asegúrate de guardar todos los archivos (Ctrl+S)
- Cierra la aplicación completamente
- Ejecuta `dotnet clean` y luego `dotnet build`

### "Error de compilación"
- Lee el mensaje de error (generalmente dice qué falta)
- Verifica que los nombres coincidan entre XAML y C#
- Asegúrate de tener todos los `using` necesarios

### "La interfaz se ve rara"
- Verifica que el Grid tenga las columnas correctas
- Asegúrate de que `Grid.Column` sea el número correcto (empieza en 0)
- Revisa que los `Binding` estén bien escritos

---

## ✨ Consejos Finales

1. **Haz cambios pequeños**: Cambia una cosa a la vez y prueba
2. **Usa los colores**: Los comentarios con 👈 indican cambios importantes
3. **Copia y pega con cuidado**: Respeta la indentación y los nombres
4. **Pregunta cuando tengas dudas**: Es mejor preguntar que romper algo
5. **Guarda versiones anteriores**: Antes de cambios grandes, haz una copia de seguridad

---

**¡Ahora estás listo para hacer cambios en ChurchPresenter! 🎉**

Si necesitas ayuda específica con algún cambio, vuelve a esta documentación y busca la sección correspondiente.
