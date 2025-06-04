# Дипломна бакалаврська робота Кузьмичова І.І. 643п 2025

В рамках цієї роботи було розроблено систему управління стоматологічною клінікою "DentalClinic".

## Структура проєкту

Проєкт складається с 3 компонентів:
1. Backend: ASP.NET Core 9
2. Frontend: Angular 17
3. Database: PostgreSQL 16

## Як запустити?

1. Запустіть Docker Desktop
2. Запустіть скрипт `./scripts/launch-databse.sh`
3. Відкрийте солюшн `./DentalClinic.sln` у Visual Studio 2022
4. Запустіть проєкт `./src/DentalClinic.WebApi/DentalClinic.WebApi.csproj`
5. Вікрийте папку `./src/DentalClinic.Frontend` у Visual Studio Code як робочу директорію
6. Відкрийте консоль і запустіть проєкт за допомогою команди `npm start`

## Дані для авторизації

При першому запуску у базі даних створюється юзер, який володіє правами адміністратора, із наступними даними для авторизації:
- Логін: `admin@dental.clinic`
- Пароль: `passW0RD!`