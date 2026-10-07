# Election CRM - Prototipo de Modernización Arquitectónica

Repositorio oficial del prototipo web desacoplado para la gestión de información electoral,
desarrollado con Angular y .NET bajo una Arquitectura Basada en Características.

## Requisitos Previos

- Docker Desktop 4.x o superior
- Git
- Node.js y npm (para el frontend)

## Instalación y Configuración

1. Clonar el repositorio:

```bash
git clone https://github.com/AlejoConsuegra/election.git
cd election
```

2. Ingresa a la carpeta del backend (paso obligatorio):
   cd Election.CRM.API

3. Configurar las variables de entorno:
   El proyecto utiliza variables de entorno para gestionar credenciales de forma segura.
   Copie el archivo de ejemplo y ajústelo si es necesario:

    cp .env.example .env

# Ejecución del Prototipo

Levante los contenedores de la base de datos y la API:

docker-compose up -d --build

Una vez finalizado el proceso, los servicios estarán disponibles en:
API Backend (.NET): http://localhost:5070
Base de Datos (MySQL): localhost:3307

# Ejecución del Frontend (Desarrollo)

El frontend Angular se ejecuta de forma independiente.
Ingrese a la carpeta del frontend, instale las dependencias e inicie el servidor:

cd election-crm-frontend
npm install
npm start

La interfaz estará disponible en: http://localhost:4200

# Pruebas de Carga (JMeter)

Los planes de prueba, resultados brutos y reportes HTML generados con Apache JMeter se encuentran en la carpeta /evidencias_jmeter/.

# Seguridad y Credenciales

Este repositorio no contiene secretos, credenciales ni tokens.
Todas las configuraciones sensibles se gestionan a través del archivo .env, el cual está excluido del control de versiones mediante .gitignore.
