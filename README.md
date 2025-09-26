# SS Tracker - Sistema de Seguimiento de Disponibilidad

Una aplicación web dockerizada que combina una API de ASP.NET Core con un frontend de React para mostrar la disponibilidad de asesores en formato de calendario.

## ⚡ Inicio Rápido (TL;DR)

```bash
# Desarrollo con hot-reload automático completo
./scripts/dev.sh

# Producción optimizada
./scripts/docker-build.sh
```

🌐 **App**: `http://localhost:5007/calendar` | 📊 **API**: `http://localhost:5007/swagger`

## 🏗️ Arquitectura

- **Backend**: ASP.NET Core 8.0 Web API
- **Frontend**: React 18 con TypeScript
- **Containerización**: Docker multi-stage build
- **Datos**: CSV con horarios de disponibilidad
- **API**: REST siguiendo convenciones estándar con prefijo `/api/v1`
- **Documentación**: Swagger/OpenAPI 3.0 integrado

## 📊 Endpoints Disponibles

| Endpoint                                                   | Descripción                      |
| ---------------------------------------------------------- | -------------------------------- |
| `http://localhost:5007/`                                   | Información general de la API    |
| `http://localhost:5007/swagger/index.html`                 | Documentación Swagger de la API  |
| `http://localhost:5007/calendar`                           | **Interfaz de calendario React** |
| `http://localhost:5007/api/v1/health`                      | Estado de salud del sistema      |
| `http://localhost:5007/api/v1/schedule`                    | Calendario completo              |
| `http://localhost:5007/api/v1/schedule/advisors/available` | Asesores disponibles ahora       |

## 🚀 Inicio Rápido

### 🔥 DESARROLLO (Con Hot Reload Automático)

Para desarrollo con hot reload completo donde **todos los cambios se aplican automáticamente**:

```bash
# ⚡ Comando único - maneja todo automáticamente
./scripts/dev.sh

# 📊 Ver logs en tiempo real
./scripts/dev.sh logs

# 🔄 Reiniciar servicios
./scripts/dev.sh restart

# 🛑 Detener todo
./scripts/dev.sh stop
```

**🔥 Hot Reload Automático:**

- **✅ C# changes**: Auto-recompile en ~2-3 segundos
- **✅ React changes**: **Detección automática + rebuild automático**
- **✅ CSV/Assets**: Cambios instantáneos via Docker volumes

**📍 Endpoints de desarrollo:**

- 🌐 **Aplicación**: `http://localhost:5007/calendar`
- 📊 **API Docs**: `http://localhost:5007/swagger/index.html`
- ❤️ **Health**: `http://localhost:5007/api/v1/health`
- 📅 **Calendario**: `http://localhost:5007/api/v1/schedule`
- 👥 **Disponibles**: `http://localhost:5007/api/v1/schedule/advisors/available`

### 🏭 PRODUCCIÓN (Optimizado)

Para producción con build optimizado:

```bash
# Build completo para producción
./scripts/docker-build.sh
```

**🛠️ Comandos útiles para producción:**

```bash
# Ver logs
docker-compose logs -f

# Detener servicios
docker-compose down

# Rebuild completo
docker-compose build --no-cache

# Limpiar todo
docker-compose down --volumes --rmi all
```

## 📦 Estructura del Proyecto

```
sstracker/
├── Controllers/              # Controladores de API
├── Assets/                  # Archivos CSV de datos
├── ClientApp/               # Aplicación React
│   ├── src/
│   │   ├── components/      # Componentes React
│   │   ├── api.ts          # Cliente API
│   │   └── types.ts        # Tipos TypeScript
│   └── public/
├── scripts/                 # 📂 Scripts de desarrollo y producción
│   ├── dev.sh              # 🚀 Desarrollo con hot-reload automático
│   └── docker-build.sh     # 📦 Build optimizado para producción
├── wwwroot/                # Archivos estáticos (build)
├── Dockerfile              # Configuración Docker producción
├── Dockerfile.dev          # Configuración Docker desarrollo
├── docker-compose.yml      # Orquestación producción
└── docker-compose.dev.yml  # Orquestación desarrollo
```

## 🎨 Características del Calendario

- **Layout de Tabla**: 5 columnas (días de la semana) x filas de horas
- **Vista por Bloques**: Cada hora muestra qué asesores están disponibles
- **Indicador en Tiempo Real**: Resalta la hora actual y asesores disponibles con "🕐 AHORA"
- **Responsive Design**:
  - **Desktop**: Vista completa de la semana (5 días)
  - **Mobile**: Solo día actual y siguiente (ej: Viernes → Lunes)
- **Auto-refresh**: Actualización automática cada 30 segundos
- **Interactividad**: Hover para mostrar fotos, click para mantener visible
- **Colores inteligentes**: Solo asesores disponibles AHORA se colorean en verde

## 🐳 Docker - Configuraciones

### Multi-Stage Build (Producción)

El Dockerfile principal utiliza:

1. **Stage 1**: Build del frontend React
2. **Stage 2**: Build del backend .NET
3. **Stage 3**: Imagen final optimizada

### Hot Reload (Desarrollo)

El Dockerfile.dev utiliza:

1. **Volúmenes montados**: Código fuente sincronizado
2. **dotnet watch**: Auto-recompilación de C#
3. **File watcher**: Detección de cambios automática

## 📋 API Endpoints (REST v1)

### GET /api/v1/schedule

Devuelve el calendario completo con todos los horarios y asesores.

**Respuesta:**

```json
{
  "entries": [
    {
      "day": "Mon",
      "start": "09:00",
      "end": "12:00",
      "advisors": "Angel",
      "advisorsList": ["Angel"]
    }
  ],
  "totalEntries": 15,
  "lastModified": "2025-09-26T15:30:00"
}
```

### GET /api/v1/schedule/advisors/available

Devuelve los asesores disponibles en el momento actual.

**Respuesta:**

```json
{
  "available": ["Jorge", "Angel", "Raul"]
}
```

### GET /api/v1/health

Estado de salud del sistema.

**Respuesta:**

```json
{
  "status": "Healthy",
  "serverTime": "2025-09-26T15:30:00",
  "csvExists": true,
  "csvLines": 15,
  "csvLastModified": "2025-09-26T10:00:00"
}
```

## 📊 Formato de Datos CSV

El sistema lee un archivo CSV con el formato simplificado:

```csv
day,start,end,advisors
Mon,09:00,12:00,Angel
Mon,12:00,14:00,Jorge
Mon,14:00,17:00,Raul; Angel; Jorge
Tue,11:00,13:00,chipachueco
Wed,09:00,13:00,chipachueco
```

**Campos:**

- `day`: Día de la semana (Mon, Tue, Wed, Thu, Fri)
- `start`: Hora de inicio (HH:mm)
- `end`: Hora de fin (HH:mm)
- `advisors`: Lista de asesores separados por `;` o `,`

## 🔧 Variables de Entorno

| Variable                          | Desarrollo      | Producción      | Descripción                  |
| --------------------------------- | --------------- | --------------- | ---------------------------- |
| `ASPNETCORE_ENVIRONMENT`          | `Development`   | `Production`    | Entorno de ejecución         |
| `ASPNETCORE_URLS`                 | `http://+:5007` | `http://+:5007` | URLs de escucha              |
| `DOTNET_USE_POLLING_FILE_WATCHER` | `true`          | -               | File watcher para hot reload |

## 📝 Scripts Disponibles

### 🚀 `./scripts/dev.sh` - Desarrollo Principal

Script unificado para desarrollo con hot-reload automático completo:

```bash
./scripts/dev.sh          # Iniciar desarrollo (comando principal)
./scripts/dev.sh logs     # Ver logs en tiempo real
./scripts/dev.sh restart  # Reiniciar servicios Docker
./scripts/dev.sh stop     # Detener todo el entorno
./scripts/dev.sh status   # Ver estado actual
./scripts/dev.sh build    # Build manual de React (opcional)
```

### 📦 `./scripts/docker-build.sh` - Producción

Script para builds optimizados de producción:

```bash
./scripts/docker-build.sh  # Build completo para deployment
```

**¿Por qué usar los scripts?**

- ✅ **Automatización completa**: Un comando hace todo
- ✅ **Hot-reload automático**: React se rebuill automáticamente
- ✅ **Gestión inteligente**: Verifica dependencias y cleanup automático
- ✅ **Códigos de color**: Output fácil de leer

## 🔍 Troubleshooting

### Problema: Puerto 5007 ocupado

```bash
# Encontrar proceso
lsof -i :5007

# Parar containers
docker-compose down
docker-compose -f docker-compose.dev.yml down
```

### Problema: React no se actualiza automáticamente

```bash
# Verificar que el file watcher esté funcionando
./scripts/dev.sh status

# Build manual si es necesario
./scripts/dev.sh build

# Reiniciar el entorno completo
./scripts/dev.sh restart
```

### Problema: Hot reload no funciona

```bash
# Ver logs para diagnosticar
./scripts/dev.sh logs

# Reiniciar todo el entorno
./scripts/dev.sh stop
./scripts/dev.sh

# Si persiste, verificar dependencias
docker --version
docker-compose --version
```

## 🚀 Flujo de Trabajo Recomendado

### Para Desarrollo Diario:

1. **Iniciar**: `./scripts/dev.sh` - Un comando para todo
2. **Desarrollar**:
   - ✅ Cambios en C# → **Se recompila automáticamente**
   - ✅ Cambios en React/CSS → **Se rebuill automáticamente**
   - ✅ Cambios en CSV → **Se actualizan automáticamente**
3. **Ver**: `http://localhost:5007/calendar` - Refresca browser para ver cambios
4. **Detener**: `./scripts/dev.sh stop` cuando termines

### Para Producción/Deployment:

1. **Build**: `./scripts/docker-build.sh` - Build optimizado completo
2. **Deploy**: El calendario estará en `http://localhost:5007/calendar`

### 💡 Tips de Productividad:

- 🔥 **Deja corriendo**: `./scripts/dev.sh` y solo programa
- 📊 **Logs útiles**: `./scripts/dev.sh logs` en otra terminal
- 🔄 **Si algo falla**: `./scripts/dev.sh restart` resetea todo

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama feature (`git checkout -b feature/NuevaFuncionalidad`)
3. Commit tus cambios (`git commit -m 'Agregar NuevaFuncionalidad'`)
4. Push a la rama (`git push origin feature/NuevaFuncionalidad`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.
