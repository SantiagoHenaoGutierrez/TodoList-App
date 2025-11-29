\# TodoList - Aplicación de Gestión de Tareas



Aplicación full-stack para gestión de tareas con autenticación JWT, desarrollada con Angular 19 y .NET 9.



\## Características



\- Autenticación con JWT

\- CRUD completo de tareas

\- Filtros (Todas, Pendientes, Completadas)

\- Estadísticas de tareas

\- Diseño responsive con Angular Material

\- Lazy Loading de módulos

\- Estado global con RxJS Observables

\- Optimización con trackBy



\## Tecnologías



\### Frontend



\- Angular 19

\- Angular Material

\- RxJS

\- TypeScript



\### Backend



\- .NET 9 / ASP.NET Core

\- Entity Framework Core (In-Memory Database)

\- JWT Authentication

\- BCrypt para hash de contraseñas



\## Estructura del Proyecto



```

TodoList/

├── TodoListAPI/          # Backend .NET

│   ├── Controllers/

│   ├── Services/

│   ├── Models/

│   ├── Data/

│   └── Configuration/

└── TodoListApp/          # Frontend Angular

&nbsp;   ├── src/

&nbsp;   │   ├── app/

&nbsp;   │   │   ├── core/

&nbsp;   │   │   ├── features/

&nbsp;   │   │   └── shared/

&nbsp;   │   └── environments/

&nbsp;   └── angular.json

```



\## Instalación y Ejecución



\### Requisitos Previos

\- Node.js 18+ y npm

\- .NET 9 SDK

\- Angular CLI (`npm install -g @angular/cli`)



\### Backend (.NET API)

```bash

\# Navegar a la carpeta del backend

cd TodoListAPI



\# Restaurar dependencias

dotnet restore



\# Ejecutar la API

dotnet run



\# La API estará disponible en:

\# http://localhost:5227

\# https://localhost:7133

```



\### Frontend (Angular)



```bash

\# Navegar a la carpeta del frontend

cd TodoListApp



\# Instalar dependencias

npm install



\# Ejecutar la aplicación

ng serve



\# La app estará disponible en:

\# http://localhost:4200

```



\## 👤 Usuario de Prueba

```

Email: admin@todolist.com

Contraseña: Admin123

```



\##  Pruebas

```bash

\# Frontend

cd TodoListApp

ng test



\# Backend

cd TodoListAPI

dotnet test

```



\## Autor



Santiago Henao - \[Tu GitHub/LinkedIn]

