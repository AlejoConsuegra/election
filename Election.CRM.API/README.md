# Plataforma de Gestión Electoral - CRM Político

## Descripción

Sistema de gestión de información electoral desarrollado con arquitectura desacoplada (.NET 8), organizado bajo una Arquitectura Basada en Características (Feature-Based Architecture). Este prototipo moderniza un sistema heredado en PHP monolítico, mejorando rendimiento, mantenibilidad y seguridad.

## 🏗️ Stack Tecnológico

- **Backend**: .NET 8.0.421 (API RESTful)
- **Base de Datos**: MySQL 8.0
- **ORM**: Entity Framework Core 8.0.x con Pomelo 8.0.2
- **Autenticación**: JWT (JSON Web Tokens)
- **Seguridad**: BCrypt para hash de contraseñas

## 🚀 Ejecución con Docker (Recomendado)

### Paso 1: Construir y levantar contenedores

```bash
docker-compose up --build -d
```

1. docker system prune -a --volumes -f
   docker-compose down

2. docker exec election-crm-api env | grep JwtSettings
3. docker exec -it election-crm-db mysql -u root -pElectoral2026!

- USE db_crm_campana;
- SHOW TABLES;
- SELECT \* FROM roles;
- SELECT id_user, nombre, cedula, id_rol FROM users;

# Detener y eliminar completamente Docker

docker-compose down -v

# Limpiar volúmenes huérfanos (por seguridad)

docker volume prune -f

# Levantar los contenedores nuevamente

# Crear la migración inicial en Entity Framework Core

dotnet ef migrations add InitialCreate
dotnet ef migrations add InitialSchema

# Aplicar la migración a la base de datos limpia

dotnet ef database update

# Respaldo DB

mysqldump -u root -p db_crm_campana > backup_antes_migracion.sql

# Levantar Docker

1. cd Election.CRM.API
2. docker-compose up --build

# Verificar contenedores

docker ps

# Iniciar Docker con la base de datos limpia

docker-compose up -d

# Verificar que la base de datos esté 100% vacía

docker exec -it election-crm-db mysql -uroot -pElectoral2026! -e "USE db_crm_campana; SHOW TABLES;"
docker exec -it election-crm-db mysql -u root -pElectoral2026!

# Forzar la eliminacion

Detiene los contenedores y elimina forzosamente el volumen (-v) donde se guardan los datos de MySQL.
Limpia cualquier volumen huérfano que haya quedado (prune -f).
Levanta un contenedor MySQL completamente nuevo y vacío.

docker-compose down -v --rmi local && docker volume prune -f && docker-compose up -d

# Mostrar

USE db_crm_campana;

SELECT \* FROM roles;

# eliminar migracion

rm -rf Migrations

# Ejecuta este único comando en tu terminal para ver los últimos registros de la API:

docker logs election-crm-api --tail 50
dotnet ef migrations remove
