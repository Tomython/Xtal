# Xtal Player

A modern, minimalist audio player built with ASP.NET Core and vanilla JavaScript. Features include audio playback with speed control, visualizations, and metadata support.

## Features

- 🎵 **Audio Playback**: Support for various audio formats (MP3, WAV, OGG)
- ⚡ **Speed Control**: Adjust playback speed from 0.5x to 1.5x
- 🎨 **Visual Effects**: Animated cover art with gradient borders and glowing effects
- 📱 **Responsive Design**: Mobile-friendly interface
- 🏷️ **Metadata Support**: Automatic extraction of track titles and album art
- 🎯 **Modern UI**: Clean, minimalist design with smooth animations

## Project Architecture

### 📁 Directory Structure

```
XtalPlayer/
├── Controllers/
│   └── HomeController.cs              # MVC Controller handling routes
├── Views/
│   ├── Shared/
│   │   └── _Layout.cshtml             # Main layout template with meta tags
│   ├── Home/
│   │   └── Index.cshtml               # Main player view (MVC version)
│   └── _ViewStart.cshtml              # View configuration
├── Properties/
│   ├── AssemblyInfo.cs                # Assembly metadata
│   └── launchSettings.json            # Development launch configuration
├── wwwroot/                           # Static assets (organized by type)
│   ├── css/
│   │   └── site.css                   # Main stylesheet with CSS variables
│   ├── js/
│   │   └── site.js                    # Main JavaScript module (ES6 class)
│   ├── images/
│   │   ├── placeholder.png             # Default album cover
│   │   ├── tarsicon.ico               # Favicon
│   │   └── tarspng.png                # Apple touch icon
│   ├── videos/
│   │   └── blackxtalvideo.mp4         # Logo video
│   ├── legacy/                        # Legacy HTML versions
│   │   ├── XtalHtml.html              # Original HTML version
│   │   └── hfd.html                   # Alternative player version
│   ├── audio/                         # Audio samples (if any)
│   └── fonts/                         # Custom fonts (if any)
├── Program.cs                         # Application entry point (ASP.NET Core)
├── XtalPlayer.csproj                  # Project file
├── appsettings.json                   # Application configuration
├── appsettings.Development.json       # Development configuration
├── .gitignore                         # Git ignore rules
└── README.md                          # Project documentation
```

### 🏗️ Architecture Overview

#### **Modern MVC Structure**
- **ASP.NET Core MVC**: Proper separation of concerns with controllers, views, and models
- **Razor Views**: Server-side rendering with client-side interactivity
- **Static File Organization**: Assets organized in logical directories by type
- **Progressive Enhancement**: Works without JavaScript, enhanced with it

#### **Frontend Architecture**
- **CSS Architecture**: 
  - CSS custom properties for theming
  - Mobile-first responsive design
  - Organized component-based styles
  - Accessibility and performance optimizations
- **JavaScript Architecture**:
  - ES6 class-based module system
  - Web Audio API integration
  - Event-driven architecture
  - Proper error handling and cleanup

#### **Asset Organization**
- **CSS**: Centralized in `/css/site.css` with organized sections
- **JavaScript**: Modular ES6 class in `/js/site.js`
- **Images**: Organized in `/images/` directory
- **Videos**: Media files in `/videos/` directory
- **Legacy Files**: Preserved in `/legacy/` for compatibility

#### **Development Features**
- **Hot Reload**: ASP.NET Core development server with hot reload
- **Source Maps**: JavaScript and CSS source mapping for debugging
- **Versioning**: Asset versioning with `asp-append-version="true"`
- **Environment Configuration**: Separate configs for development and production

## Recent Improvements

### 🐛 Bug Fixes
- **Fixed CSS Issues**: Removed duplicate CSS rules and fixed invalid gradient properties
- **Corrected File References**: Fixed placeholder image references (`.jpg` → `.png`)
- **Improved Error Handling**: Added comprehensive error handling for audio loading and playback
- **Memory Management**: Proper cleanup of audio nodes and animation frames

### 🚀 Performance Optimizations
- **Lazy Audio Context**: Audio context is only created on first user interaction
- **Efficient Animations**: Optimized animation loops with proper cleanup
- **Resource Management**: Automatic cleanup of blob URLs and audio nodes
- **Responsive Design**: Added mobile-specific optimizations

### 🏗️ Code Organization
- **MVC Architecture**: Converted from basic HTTP server to proper ASP.NET Core MVC
- **Separation of Concerns**: Separated HTML, CSS, and JavaScript into organized sections
- **Modern C# Features**: Used modern C# syntax and patterns
- **Proper Routing**: Implemented proper MVC routing with fallback for static files

### 🎨 UI/UX Improvements
- **CSS Variables**: Used CSS custom properties for consistent theming
- **Better Animations**: Smoother transitions and improved visual effects
- **Mobile Responsive**: Enhanced mobile experience with responsive breakpoints
- **Accessibility**: Better semantic HTML and ARIA support

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Modern web browser with Web Audio API support
- Git (for version control)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd XtalPlayer
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Open in browser**
   Navigate to `https://localhost:7151` or `http://localhost:5241`

### Development Workflow

#### **Project Structure Guidelines**
- **Controllers**: Handle HTTP requests and business logic
- **Views**: Razor templates for HTML generation
- **Static Assets**: Organized by type in `wwwroot/`
- **Configuration**: Environment-specific settings in `appsettings.*.json`

#### **Code Organization Best Practices**
- **CSS**: Use CSS custom properties for theming, organize by components
- **JavaScript**: ES6 classes with proper error handling and cleanup
- **HTML**: Semantic markup with proper ARIA attributes
- **Assets**: Version all static files for cache busting

#### **Development Commands**
```bash
# Run in development mode
dotnet run --environment Development

# Build for production
dotnet build --configuration Release

# Publish application
dotnet publish --configuration Release --output ./publish

# Clean build artifacts
dotnet clean
```

### Development

The application uses ASP.NET Core with MVC pattern:
- **Controllers**: Handle routing and request processing
- **Views**: Razor templates for HTML generation
- **Static Files**: Served from `wwwroot` directory
- **JavaScript**: Vanilla JS with Web Audio API for audio processing

## Browser Compatibility

- Chrome/Edge 66+
- Firefox 60+
- Safari 14+
- Mobile browsers with Web Audio API support

## Technical Details

### Audio Processing
- Uses Web Audio API for high-quality audio processing
- Supports real-time speed adjustment
- Automatic audio context management
- Proper cleanup of audio resources

### Visual Effects
- CSS animations with hardware acceleration
- Gradient borders with animated backgrounds
- Responsive design with mobile optimizations
- Smooth transitions and hover effects

### Performance
- Lazy loading of audio context
- Efficient animation loops
- Memory leak prevention
- Optimized DOM manipulation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is open source and available under the MIT License.
