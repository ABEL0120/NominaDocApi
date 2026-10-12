# NominaDocApi 📄💼

Microservicio independiente y de responsabilidad única (*Single Responsibility*) encargado de la lectura, indexación y descarga de comprobantes de nómina timbrados en formatos **PDF** y **XML**.

---

## 🏗️ Propósito y Arquitectura

Este microservicio desacopla las operaciones de consulta documental pesada de la API central:
- **Lectura en Base de Datos:** Consulta de tablas de nóminas timbradas en SQL Server.
- **Servidor de Archivos:** Localización de los archivos físicos en el sistema de almacenamiento (`DirectorioNominas`) y despacho mediante flujos binarios (`FileStream`).
- **Autonomía:** Corre en su propio puerto y proceso sin interferir en los servicios de asistencia y checador.

---

## 🛠️ Stack Tecnológico

- **Framework:** ASP.NET Core 8.0 Web API
- **Lenguaje:** C# 12
- **Almacenamiento:** Acceso a sistema de archivos local/SMB
- **Base de Datos:** Microsoft SQL Server (`2020_NOM_WEB`)
- **Documentación:** Swagger / OpenAPI

---

## ⚙️ Configuración del Entorno

### 1. Clonar el Repositorio
```bash
git clone https://github.com/TU_USUARIO/NominaDocApi.git
cd NominaDocApi
```

### 2. Archivo de Configuración Local
Copia la plantilla de ejemplo para generar tu archivo de configuración:
```bash
cp appsettings.example.json appsettings.json
```

Edita `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=TU_SERVIDOR;Database=2020_NOM_WEB;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;Encrypt=False;"
  },
  "DirectorioNominas": "C:\\Facturacion\\Nomina",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Nota:** La propiedad `DirectorioNominas` debe apuntar al directorio físico donde se encuentran almacenadas las carpetas de comprobantes por año y proceso.

---

## 📡 Endpoints del Microservicio

### 1. Listar Recibos Timbrados
- **Método:** `GET`
- **Ruta:** `/api/nomina/recibos/{compania}/{m103}`
- **Query Params (Opcionales):** `fechaInicial` (yyyy/MM/dd), `fechaFinal` (yyyy/MM/dd)
- **Respuesta (`200 OK`):**
```json
{
  "success": true,
  "message": "Recibos obtenidos correctamente.",
  "data": [
    {
      "anio": 2024,
      "proceso": 12,
      "fechaInicio": "01/06/2024",
      "fechaTermino": "15/06/2024",
      "fechaPago": "15/06/2024",
      "nombreArchivo": "NOM_2024_12_1054",
      "tienePdf": true,
      "tieneXml": true
    }
  ]
}
```

### 2. Descargar Archivo (PDF o XML)
- **Método:** `GET`
- **Ruta:** `/api/nomina/descargar/{anio}/{proceso}/{tipo}/{archivo}`
- **Parámetros:**
  - `anio` *(int)*: Año fiscal.
  - `proceso` *(int)*: Periodo de nómina.
  - `tipo` *(string)*: `pdf` o `xml`.
  - `archivo` *(string)*: Nombre base del archivo sin extensión.
- **Respuesta:** Archivo binario con cabecera `Content-Disposition: attachment`.

---

## 🏃‍♂️ Ejecución

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Iniciar servicio (por defecto en http://localhost:5054)
dotnet run
```