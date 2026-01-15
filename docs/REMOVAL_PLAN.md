# План удаления демонстрационных компонентов из CleanAspire

## Обзор

Этот документ содержит подробный план удаления демонстрационных сущностей и функций из проекта CleanAspire. Проект является self-hosted решением, поэтому необходимо оставить только базовую аутентификацию по логину/паролю и удалить все демонстрационные сущности (Stock, Tenant, Product).

## ВАЖНО: Инструкции по выполнению плана

### Процесс документирования ПЕРЕД удалением

**КРИТИЧЕСКИ ВАЖНО:** Перед удалением каждой сущности или функции необходимо:

1. **Задокументировать реализацию** - Скопировать код в [IMPLEMENTATION_REFERENCE.md](./IMPLEMENTATION_REFERENCE.md) с комментариями о том, как это было реализовано
2. **Зафиксировать связи** - Записать все зависимости и связи с другими компонентами
3. **Сохранить примеры использования** - Добавить примеры API endpoints, UI компонентов, конфигурации

### Работа с чекбоксами

По мере выполнения плана **ОБЯЗАТЕЛЬНО** отмечайте выполненные пункты, изменяя:
- `- [ ]` на `- [x]` для выполненных задач
- Пример:
  ```markdown
  - [ ] Невыполненная задача
  - [x] Выполненная задача
  ```

### Порядок работы с каждой сущностью

Для каждой удаляемой сущности (Product, Stock, Tenant):

1. ✍️ **ДОКУМЕНТИРОВАНИЕ** (ОБЯЗАТЕЛЬНО ПЕРВЫМ ШАГОМ)
   - [ ] Создать раздел в IMPLEMENTATION_REFERENCE.md
   - [ ] Скопировать код Domain Entity
   - [ ] Скопировать EF Configuration
   - [ ] Скопировать примеры Commands/Queries
   - [ ] Скопировать API endpoints
   - [ ] Сохранить примеры UI компонентов
   - [ ] Зафиксировать все связи и зависимости

2. 🔍 **АНАЛИЗ СВЯЗЕЙ**
   - [ ] Проверить Foreign Keys в базе данных
   - [ ] Найти все Navigation Properties
   - [ ] Проверить использование в Interceptors
   - [ ] Найти все ссылки в тестах

3. 🗑️ **УДАЛЕНИЕ** (только после п.1 и п.2)
   - [ ] Frontend (UI Pages)
   - [ ] Frontend (Generated API Client)
   - [ ] API Endpoints
   - [ ] Application Layer (Commands/Queries/DTOs)
   - [ ] Infrastructure (Configurations)
   - [ ] Domain (Entities)
   - [ ] Database Migration

4. ✅ **ПРОВЕРКА**
   - [ ] Компиляция проекта
   - [ ] Запуск тестов
   - [ ] Проверка миграций

---

## Граф зависимостей сущностей

```
ApplicationUser
    └── TenantId (FK) → Tenant
    └── SuperiorId (FK) → ApplicationUser

Tenant (независимая сущность)
    ↑ используется в ApplicationUser
    ↑ используется в SignUp.razor

Product (независимая сущность)
    ↑ используется в Stock

Stock
    └── ProductId (FK) → Product
```

**Порядок удаления по зависимостям:**
1. Stock (зависит от Product)
2. Product (независимая)
3. Tenant (используется в ApplicationUser - нужно сначала удалить поле TenantId)

---

## Связи интерфейсов мультитенанси

- `IMustHaveTenant` - интерфейс для обязательного TenantId (не используется сущностями)
- `IMayHaveTenant` - интерфейс для опционального TenantId (не используется сущностями)
- **Важно:** Проверить использование в `AuditableEntityInterceptor.cs`

---

## 1. Удаление регистрации и альтернативных методов входа

### 1.1 Регистрация пользователей (Sign Up)

**Описание:** Полное удаление функционала регистрации новых пользователей. В self-hosted системе пользователи создаются администратором.

#### Backend компоненты
- [x] `src/CleanAspire.Api/IdentityApiAdditionalEndpointsExtensions.cs`
  - Удалить endpoint `POST /account/signup`
  - Удалить endpoint `GET /account/confirmEmail` (изменен для работы только с email change)
  - Удалить класс `SignupRequest`
  - Удалить всю логику email подтверждения для регистрации

- [ ] `src/CleanAspire.Api/Identity/EmailSender.cs`
  - Удалить сервис отправки email подтверждений
  - Удалить SendGrid интеграцию для регистрации

#### Frontend компоненты (Razor Pages)
- [x] `src/CleanAspire.ClientApp/Pages/Account/SignUp.razor`
  - Удалить страницу регистрации

- [x] `src/CleanAspire.ClientApp/Pages/Account/SignupConfirmation.razor`
  - Удалить страницу подтверждения регистрации

- [x] `src/CleanAspire.ClientApp/Pages/Account/SignupSuccessful.razor`
  - Удалить страницу успешной регистрации

- [x] `src/CleanAspire.ClientApp/Pages/Account/SignIn.razor`
  - Удалить ссылку "Create an account" / "Sign Up"

#### Generated API Client
- [x] `src/CleanAspire.ClientApp/Client/Account/Signup/SignupRequestBuilder.cs`
  - Удалить generated код для signup

- [x] `src/CleanAspire.ClientApp/Client/Register/RegisterRequestBuilder.cs`
  - Удалить generated код для registration

- [x] `src/CleanAspire.ClientApp/Client/Models/SignupRequest.cs`
  - Удалить модель SignupRequest

---

### 1.2 OAuth (Google & Microsoft)

**Описание:** Удаление всех сторонних методов аутентификации. Оставить только вход по логину и паролю.

#### Backend OAuth endpoints
- [x] `src/CleanAspire.Api/IdentityApiAdditionalEndpointsExtensions.cs`
  - Удалить `GET /account/google/loginUrl`
  - Удалить `POST /account/google/signIn`
  - Удалить `GET /account/microsoft/loginUrl`
  - Удалить `POST /account/microsoft/signIn`
  - Удалить всю логику Google OAuth
  - Удалить всю логику Microsoft OAuth

#### Frontend OAuth компоненты
- [x] `src/CleanAspire.ClientApp/Pages/Account/GoogleLoginCallback.razor`
  - Удалить страницу Google callback (`/external-login`)

- [x] `src/CleanAspire.ClientApp/Pages/Account/MicrosoftLoginCallback.razor`
  - Удалить страницу Microsoft callback (`/authentication-callback`)

- [x] `src/CleanAspire.ClientApp/Pages/Account/SignIn.razor`
  - Удалить кнопку "Login with Google"
  - Удалить кнопку "Login with Microsoft"
  - Удалить иконки `Icons.Custom.Brands.Google` и `Icons.Custom.Brands.Microsoft`

#### Generated API Client для OAuth
- [x] `src/CleanAspire.ClientApp/Client/Account/Google/GoogleRequestBuilder.cs`
  - Удалить Google API client

- [x] `src/CleanAspire.ClientApp/Client/Account/Google/LoginUrl/LoginUrlRequestBuilder.cs`
  - Удалить Google LoginUrl builder

- [x] `src/CleanAspire.ClientApp/Client/Account/Google/SignIn/SignInRequestBuilder.cs`
  - Удалить Google SignIn builder

- [x] `src/CleanAspire.ClientApp/Client/Account/Microsoft/MicrosoftRequestBuilder.cs`
  - Удалить Microsoft API client

- [x] `src/CleanAspire.ClientApp/Client/Account/Microsoft/LoginUrl/LoginUrlRequestBuilder.cs`
  - Удалить Microsoft LoginUrl builder

- [x] `src/CleanAspire.ClientApp/Client/Account/Microsoft/SignIn/SignInRequestBuilder.cs`
  - Удалить Microsoft SignIn builder

#### Конфигурация OAuth
- [ ] `src/CleanAspire.AppHost/appsettings.json` или соответствующий файл
  - Удалить `Authentication:Google:ClientId`
  - Удалить `Authentication:Google:ClientSecret`
  - Удалить `Authentication:Microsoft:ClientId`
  - Удалить `Authentication:Microsoft:ClientSecret`
  - Удалить `Authentication:Microsoft:TenantId`

---

### 1.3 Двухфакторная аутентификация (2FA)

**Описание:** (Опционально) Если не требуется в self-hosted версии, удалить функционал 2FA.

#### Backend 2FA endpoints
- [x] `src/CleanAspire.Api/IdentityApiAdditionalEndpointsExtensions.cs`
  - Удалить `POST /account/login2fa`
  - Удалить `GET /account/generateAuthenticator`
  - Удалить `POST /account/enable2fa`
  - Удалить `GET /account/disable2fa`
  - Удалить `GET /account/generateRecoveryCodes`
  - Удалить логику 2FA из основного `/login` endpoint
  - Удалить классы `AuthenticatorResponse`, `Enable2faRequest`, `RecoveryCodesResponse`

#### Frontend 2FA компоненты
- [x] `src/CleanAspire.ClientApp/Pages/Account/Profile/TwofactorSetting.razor`
  - Удалить страницу настроек 2FA
- [x] `src/CleanAspire.ClientApp/Pages/Account/Profile/Setting.razor`
  - Удалить вкладку Security с TwofactorSetting
- [x] `src/CleanAspire.ClientApp/Pages/Account/SignIn.razor`
  - Удалить логику RequiresTwoFactor и TwoFactorCode

---

### 1.4 ApplicationUser (очистка полей)

**Описание:** Удаление полей, связанных с OAuth и расширенным профилем.

- [x] `src/CleanAspire.Domain/Identities/ApplicationUser.cs`
  - Удалить поле `Provider` (хранит "Local", "Google", "Microsoft")
  - Удалить поле `RefreshToken` и `RefreshTokenExpiryTime` (оставлено для JWT refresh)
  - **Сохранено:** `Nickname`, `TimeZoneId`, `LanguageCode`, `AvatarUrl`, `SuperiorId`
  - **Удалено:** `TenantId` (связано с мультитенанси)
- [x] `src/CleanAspire.Infrastructure/Persistence/Configurations/IdentityUserConfiguration.cs`
  - Удалить конфигурацию Provider и TenantId
- [x] `src/CleanAspire.Api/IdentityApiAdditionalEndpointsExtensions.cs`
  - Удалить Provider и TenantId из ProfileRequest/ProfileResponse
  - Удалить использование Provider и TenantId в updateProfile
- [x] `src/CleanAspire.Infrastructure/Services/ClaimsPrincipalExtensions.cs`
  - Удалить GetProvider() и GetTenantId()
  - Удалить Provider и TenantId из ApplicationClaimTypes
- [x] `src/CleanAspire.Infrastructure/Services/CurrentUserAccessor.cs`
  - Удалить TenantId property
- [x] `src/CleanAspire.Application/Common/Interfaces/ICurrentUserAccessor.cs`
  - Удалить TenantId из интерфейса
- [x] `src/CleanAspire.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs`
  - Удалить использование TenantId в SetCreationAuditInfo
- [x] `src/CleanAspire.Infrastructure/Persistence/Seed/ApplicationDbContextInitializer.cs`
  - Удалить Provider и TenantId из seed данных
- [x] `src/CleanAspire.ClientApp/Pages/Account/Profile/ProfileSetting.razor`
  - Удалить TenantId из ProfileModel

---

## 2. Удаление демонстрационных сущностей

### 2.1 Product (Продукт)

**Описание:** Полное удаление сущности Product - демонстрационная сущность для каталога товаров.

**Модель Product:**
```csharp
- SKU: string
- Name: string
- Category: ProductCategory (enum: Electronics, Furniture, Clothing, Food, Beverages, HealthCare, Sports)
- Description: string
- Price: decimal
- Currency: string
- UOM: string
```

**⚠️ ЗАВИСИМОСТИ И СВЯЗИ:**
- **Stock.ProductId** (FK) - КРИТИЧЕСКИ ВАЖНО: Stock должен быть удален ПЕРЕД Product
- **ProductAutocomplete.cs** - UI компонент для выбора продукта (используется в Stock UI)
- **ProductCacheService.cs** - сервис кэширования продуктов
- **ProductServiceProxy.cs** - прокси-сервис для API вызовов
- **StockEndpointTests.cs** - тесты используют Product

---

#### 📋 ШАГ 1: ДОКУМЕНТИРОВАНИЕ (ОБЯЗАТЕЛЬНО ПЕРЕД УДАЛЕНИЕМ)
- [ ] Задокументировать Product entity в IMPLEMENTATION_REFERENCE.md
- [ ] Сохранить все Commands (Create, Update, Delete, Import)
- [ ] Сохранить все Queries (GetAll, GetById, Pagination, Export)
- [ ] Сохранить валидаторы (CreateProductCommandValidator, UpdateProductCommandValidator, DeleteProductCommandValidator)
- [ ] Сохранить Event Handlers (ProductCreatedEvent, ProductUpdatedEvent, ProductDeletedEvent)
- [ ] Сохранить UI компоненты (Index.razor, Edit.razor, NewProductDialog.razor)
- [ ] Сохранить ProductCacheService и ProductServiceProxy
- [ ] Зафиксировать структуру CSV импорта/экспорта
- [ ] Сохранить ProductConfiguration (EF Core)

---

#### ШАГ 2: УДАЛЕНИЕ

#### Domain Layer
- [ ] `src/CleanAspire.Domain/Entities/Product.cs`
  - Удалить сущность Product

- [ ] `src/CleanAspire.Domain/Enums/ProductCategory.cs` (если существует)
  - Удалить enum ProductCategory

#### Application Layer - Commands
- [ ] `src/CleanAspire.Application/Features/Products/Commands/CreateProductCommand.cs`
  - Удалить команду создания продукта

- [ ] `src/CleanAspire.Application/Features/Products/Commands/UpdateProductCommand.cs`
  - Удалить команду обновления продукта

- [ ] `src/CleanAspire.Application/Features/Products/Commands/DeleteProductCommand.cs`
  - Удалить команду удаления продукта

- [ ] `src/CleanAspire.Application/Features/Products/Commands/ImportProductsCommand.cs`
  - Удалить команду импорта продуктов

- [ ] `src/CleanAspire.Application/Features/Products/Commands/` (папка)
  - Удалить всю папку Commands для Products

#### Application Layer - Queries
- [ ] `src/CleanAspire.Application/Features/Products/Queries/GetAllProductsQuery.cs`
  - Удалить запрос получения всех продуктов

- [ ] `src/CleanAspire.Application/Features/Products/Queries/GetProductByIdQuery.cs`
  - Удалить запрос получения продукта по ID

- [ ] `src/CleanAspire.Application/Features/Products/Queries/ProductsWithPaginationQuery.cs`
  - Удалить запрос с пагинацией

- [ ] `src/CleanAspire.Application/Features/Products/Queries/ExportProductsQuery.cs`
  - Удалить запрос экспорта

- [ ] `src/CleanAspire.Application/Features/Products/Queries/` (папка)
  - Удалить всю папку Queries для Products

#### Application Layer - DTOs
- [ ] `src/CleanAspire.Application/Features/Products/DTOs/ProductDto.cs`
  - Удалить DTO для продукта

- [ ] `src/CleanAspire.Application/Features/Products/` (папка)
  - Удалить всю папку Features/Products

#### Infrastructure Layer
- [ ] `src/CleanAspire.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
  - Удалить конфигурацию Entity Framework для Product

- [ ] Database Migration
  - Создать миграцию для удаления таблицы `Products`

#### API Layer
- [ ] `src/CleanAspire.Api/Endpoints/ProductEndpointRegistrar.cs`
  - Удалить все endpoints для Product:
    - `GET /products/`
    - `GET /products/{id}`
    - `POST /products/`
    - `PUT /products/`
    - `DELETE /products/`
    - `POST /products/pagination`
    - `POST /products/export`
    - `POST /products/import`

#### Client (Frontend) - Generated API
- [ ] `src/CleanAspire.ClientApp/Client/Models/ProductDto.cs`
  - Удалить generated модель ProductDto

- [ ] `src/CleanAspire.ClientApp/Client/Models/ProductsWithPaginationQuery.cs`
  - Удалить generated модель запроса с пагинацией

- [ ] `src/CleanAspire.ClientApp/Client/Products/` (папка, если существует)
  - Удалить все generated API клиенты для Products

#### Client (Frontend) - UI Pages
- [ ] `src/CleanAspire.ClientApp/Pages/Products/Index.razor`
  - Удалить главную страницу списка продуктов

- [ ] `src/CleanAspire.ClientApp/Pages/Products/Edit.razor`
  - Удалить страницу редактирования продукта

- [ ] `src/CleanAspire.ClientApp/Pages/Products/Components/NewProductDialog.razor`
  - Удалить диалог создания нового продукта

- [ ] `src/CleanAspire.ClientApp/Pages/Products/` (папка)
  - Удалить всю папку Products

#### Client (Frontend) - Services
- [ ] `src/CleanAspire.ClientApp/Services/Products/ProductCacheService.cs`
  - Удалить сервис кэширования продуктов

- [ ] `src/CleanAspire.ClientApp/Services/Products/ProductServiceProxy.cs`
  - Удалить прокси-сервис для API

- [ ] `src/CleanAspire.ClientApp/Services/Products/` (папка)
  - Удалить всю папку Services/Products

#### Client (Frontend) - Autocomplete
- [ ] `src/CleanAspire.ClientApp/Components/Autocompletes/ProductAutocomplete.cs`
  - Удалить компонент автокомплита для выбора продукта

#### Application Layer - Validators
- [ ] `src/CleanAspire.Application/Features/Products/Validators/CreateProductCommandValidator.cs`
  - Удалить валидатор создания

- [ ] `src/CleanAspire.Application/Features/Products/Validators/UpdateProductCommandValidator.cs`
  - Удалить валидатор обновления

- [ ] `src/CleanAspire.Application/Features/Products/Validators/DeleteProductCommandValidator.cs`
  - Удалить валидатор удаления

#### Application Layer - Event Handlers
- [ ] `src/CleanAspire.Application/Features/Products/EventHandlers/ProductCreatedEvent.cs`
  - Удалить event handler создания

- [ ] `src/CleanAspire.Application/Features/Products/EventHandlers/ProductUpdatedEvent.cs`
  - Удалить event handler обновления

- [ ] `src/CleanAspire.Application/Features/Products/EventHandlers/ProductDeletedEvent.cs`
  - Удалить event handler удаления

#### Infrastructure Layer - Seed Data
- [ ] `src/CleanAspire.Infrastructure/Persistence/Seed/ApplicationDbContextInitializer.cs`
  - Удалить seed данные для Products (строки с Product initialization)

---

### 2.2 Stock (Запас)

**Описание:** Полное удаление сущности Stock - демонстрационная сущность для управления складскими запасами.

**Модель Stock:**
```csharp
- ProductId: string (FK к Product)
- Product: Product (навигационное свойство)
- Quantity: int
- Location: string
```

**⚠️ ЗАВИСИМОСТИ И СВЯЗИ:**
- **Product.Id** (FK через ProductId) - Stock зависит от Product
- **StockConfiguration.cs** - настройка связи с Product
- **StockEndpointTests.cs** - интеграционные тесты
- **ProductAutocomplete.cs** - используется в UI для выбора продукта

**⚠️ ПОРЯДОК УДАЛЕНИЯ: Stock ДОЛЖЕН быть удален ПЕРЕД Product**

---

#### 📋 ШАГ 1: ДОКУМЕНТИРОВАНИЕ (ОБЯЗАТЕЛЬНО ПЕРЕД УДАЛЕНИЕМ)
- [x] Задокументировать Stock entity в IMPLEMENTATION_REFERENCE.md
- [x] Сохранить StockDispatchingCommand (бизнес-логика отправки со склада)
- [x] Сохранить StockReceivingCommand (бизнес-логика получения на склад)
- [x] Сохранить StocksWithPaginationQuery
- [x] Сохранить валидаторы (StockDispatchingCommandValidator, StockReceivingCommandValidator)
- [x] Сохранить UI компоненты (Index.razor, StockDialog.razor)
- [x] Сохранить StockConfiguration (EF Core с FK на Product)
- [x] Зафиксировать бизнес-операции dispatch/receive

---

#### ШАГ 2: УДАЛЕНИЕ

#### Domain Layer
- [x] `src/CleanAspire.Domain/Entities/Stock.cs`
  - Удалить сущность Stock

#### Application Layer - Commands
- [x] `src/CleanAspire.Application/Features/Stocks/Commands/StockDispatchingCommand.cs`
  - Удалить команду отправки запаса

- [x] `src/CleanAspire.Application/Features/Stocks/Commands/StockReceivingCommand.cs`
  - Удалить команду получения запаса

- [x] `src/CleanAspire.Application/Features/Stocks/Commands/` (папка)
  - Удалить всю папку Commands для Stocks

#### Application Layer - Queries
- [x] `src/CleanAspire.Application/Features/Stocks/Queries/StocksWithPaginationQuery.cs`
  - Удалить запрос с пагинацией

- [x] `src/CleanAspire.Application/Features/Stocks/Queries/` (папка)
  - Удалить всю папку Queries для Stocks

#### Application Layer - DTOs
- [x] `src/CleanAspire.Application/Features/Stocks/DTOs/StockDto.cs`
  - Удалить DTO для запаса

- [x] `src/CleanAspire.Application/Features/Stocks/` (папка)
  - Удалить всю папку Features/Stocks

#### Infrastructure Layer
- [x] `src/CleanAspire.Infrastructure/Persistence/Configurations/StockConfiguration.cs`
  - Удалить конфигурацию Entity Framework для Stock

- [x] `src/CleanAspire.Infrastructure/Persistence/ApplicationDbContext.cs`
  - Удалить DbSet<Stock> Stocks

- [x] `src/CleanAspire.Application/Common/Interfaces/IApplicationDbContext.cs`
  - Удалить DbSet<Stock> Stocks из интерфейса

- [ ] Database Migration
  - Создать миграцию для удаления таблицы `Stocks`

#### API Layer
- [x] `src/CleanAspire.Api/Endpoints/StockEndpointRegistrar.cs`
  - Удалить все endpoints для Stock:
    - `POST /stocks/dispatch`
    - `POST /stocks/receive`
    - `POST /stocks/pagination`

#### Client (Frontend) - Generated API
- [x] `src/CleanAspire.ClientApp/Client/Models/StockDto.cs`
  - Удалить generated модель StockDto (будет пересоздана при регенерации)

- [x] `src/CleanAspire.ClientApp/Client/Models/StocksWithPaginationQuery.cs`
  - Удалить generated модель запроса с пагинацией (будет пересоздана при регенерации)

- [x] `src/CleanAspire.ClientApp/Client/Stocks/` (папка, если существует)
  - Удалить все generated API клиенты для Stocks

- [x] `src/CleanAspire.ClientApp/Client/ApiClient.cs`
  - Удалить ссылку на Stocks property

#### Client (Frontend) - UI Pages
- [x] `src/CleanAspire.ClientApp/Pages/Stocks/Index.razor`
  - Удалить главную страницу управления запасами

- [x] `src/CleanAspire.ClientApp/Pages/Stocks/StockDialog.razor`
  - Удалить диалог для операций со складом

- [x] `src/CleanAspire.ClientApp/Pages/Stocks/` (папка)
  - Удалить всю папку Stocks

#### Application Layer - Validators
- [x] `src/CleanAspire.Application/Features/Stocks/Validators/StockDispatchingCommandValidator.cs`
  - Удалить валидатор отправки

- [x] `src/CleanAspire.Application/Features/Stocks/Validators/StockReceivingCommandValidator.cs`
  - Удалить валидатор получения

#### Infrastructure Layer - Seed Data
- [x] `src/CleanAspire.Infrastructure/Persistence/Seed/ApplicationDbContextInitializer.cs`
  - Удалить seed данные для Stocks

#### Tests
- [x] `tests/CleanAspire.Tests/StockEndpointTests.cs`
  - Удалить интеграционные тесты для Stock endpoints

---

### 2.3 Tenant (Организация/Мультитенанси)

**Описание:** Полное удаление сущности Tenant - демонстрационная сущность для мультитенанси (поддержки нескольких организаций).

**Модель Tenant:**
```csharp
- Id: string (GUID v7)
- Name: string
- Description: string
```

**⚠️ ЗАВИСИМОСТИ И СВЯЗИ:**
- **ApplicationUser.TenantId** (FK) - КРИТИЧЕСКИ ВАЖНО: сначала удалить поле TenantId из ApplicationUser
- **SignUp.razor** - использует MultiTenantAutocomplete для выбора организации
- **SignupRequest** - содержит TenantId
- **ProfileRequest/ProfileResponse** - могут содержать TenantId
- **IMustHaveTenant / IMayHaveTenant** - интерфейсы мультитенанси
- **AuditableEntityInterceptor** - может использовать TenantId
- **CurrentUserAccessor** - может использовать TenantId
- **TenantEndpoints** - помечены AllowAnonymous для регистрации
- **Seed данные** - инициализация тенантов

**⚠️ ПОРЯДОК УДАЛЕНИЯ:**
1. Сначала удалить поле TenantId из ApplicationUser
2. Удалить MultiTenantAutocomplete из SignUp.razor
3. Удалить TenantId из всех Request/Response models
4. Затем удалить саму сущность Tenant

---

#### 📋 ШАГ 1: ДОКУМЕНТИРОВАНИЕ (ОБЯЗАТЕЛЬНО ПЕРЕД УДАЛЕНИЕМ)
- [ ] Задокументировать Tenant entity в IMPLEMENTATION_REFERENCE.md
- [ ] Сохранить все Commands (Create, Update, Delete)
- [ ] Сохранить Queries (GetAll, GetById)
- [ ] Сохранить TenantDto
- [ ] Сохранить TenantConfiguration (EF Core, GUID v7)
- [ ] Сохранить TenantEndpointRegistrar (с AllowAnonymous)
- [ ] Сохранить MultiTenantAutocomplete компонент
- [ ] Зафиксировать использование в SignUp flow
- [ ] Сохранить связь с ApplicationUser
- [ ] Зафиксировать интерфейсы IMustHaveTenant / IMayHaveTenant

---

#### ШАГ 2: ПРЕДВАРИТЕЛЬНЫЕ ДЕЙСТВИЯ (перед удалением Tenant entity)

#### ApplicationUser - удаление TenantId
- [ ] `src/CleanAspire.Domain/Identities/ApplicationUser.cs`
  - Удалить поле `public string? TenantId { get; set; }`
  - **ВАЖНО:** Это разорвет FK связь с Tenant

#### Конфигурация EF Core
- [ ] `src/CleanAspire.Infrastructure/Persistence/Configurations/IdentityUserConfiguration.cs`
  - Удалить конфигурацию TenantId (если есть)

#### Request/Response Models
- [ ] `src/CleanAspire.ClientApp/Client/Models/SignupRequest.cs`
  - Удалить поле TenantId

- [ ] `src/CleanAspire.ClientApp/Client/Models/ProfileRequest.cs`
  - Удалить поле TenantId (если есть)

- [ ] `src/CleanAspire.ClientApp/Client/Models/ProfileResponse.cs`
  - Удалить поле TenantId (если есть)

#### UI Components
- [ ] `src/CleanAspire.ClientApp/Pages/Account/SignUp.razor`
  - Удалить MultiTenantAutocomplete компонент
  - Удалить привязку к model.Tenant

- [ ] `src/CleanAspire.ClientApp/Components/Autocompletes/MultiTenantAutocomplete.cs` (если существует)
  - Удалить компонент автокомплита для выбора организации

#### Services
- [ ] `src/CleanAspire.Infrastructure/Services/CurrentUserAccessor.cs`
  - Удалить логику работы с TenantId (если есть)

- [ ] `src/CleanAspire.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs`
  - Удалить логику обработки IMustHaveTenant / IMayHaveTenant (если есть)

#### Database Migration - удаление TenantId из ApplicationUser
- [ ] Создать миграцию для удаления колонки TenantId из таблицы AspNetUsers

---

#### ШАГ 3: УДАЛЕНИЕ TENANT ENTITY

#### Domain Layer
- [ ] `src/CleanAspire.Domain/Entities/Tenant.cs`
  - Удалить сущность Tenant

#### Application Layer - Commands
- [ ] `src/CleanAspire.Application/Features/Tenants/Commands/CreateTenantCommand.cs`
  - Удалить команду создания организации

- [ ] `src/CleanAspire.Application/Features/Tenants/Commands/UpdateTenantCommand.cs`
  - Удалить команду обновления организации

- [ ] `src/CleanAspire.Application/Features/Tenants/Commands/DeleteTenantCommand.cs`
  - Удалить команду удаления организации

- [ ] `src/CleanAspire.Application/Features/Tenants/Commands/` (папка)
  - Удалить всю папку Commands для Tenants

#### Application Layer - Queries
- [ ] `src/CleanAspire.Application/Features/Tenants/Queries/GetAllTenantsQuery.cs`
  - Удалить запрос получения всех организаций

- [ ] `src/CleanAspire.Application/Features/Tenants/Queries/GetTenantByIdQuery.cs`
  - Удалить запрос получения организации по ID

- [ ] `src/CleanAspire.Application/Features/Tenants/Queries/` (папка)
  - Удалить всю папку Queries для Tenants

#### Application Layer - DTOs
- [ ] `src/CleanAspire.Application/Features/Tenants/DTOs/TenantDto.cs`
  - Удалить DTO для организации

- [ ] `src/CleanAspire.Application/Features/Tenants/` (папка)
  - Удалить всю папку Features/Tenants

#### Infrastructure Layer
- [ ] `src/CleanAspire.Infrastructure/Persistence/Configurations/TenantConfiguration.cs`
  - Удалить конфигурацию Entity Framework для Tenant

- [ ] Database Migration
  - Создать миграцию для удаления таблицы `Tenants`

#### API Layer
- [ ] `src/CleanAspire.Api/Endpoints/TenantEndpointRegistrar.cs`
  - Удалить все endpoints для Tenant:
    - `GET /tenants/` (AllowAnonymous)
    - `GET /tenants/{id}` (AllowAnonymous)
    - `POST /tenants/`
    - `PUT /tenants/`
    - `DELETE /tenants/`

#### Client (Frontend) - Generated API
- [ ] `src/CleanAspire.ClientApp/Client/Models/TenantDto.cs`
  - Удалить generated модель TenantDto

- [ ] `src/CleanAspire.ClientApp/Client/Tenants/` (папка, если существует)
  - Удалить все generated API клиенты для Tenants

#### Client (Frontend) - UI Pages (если существуют)
- [ ] Найти и удалить Razor страницы для управления организациями:
  - TenantsList.razor
  - TenantEdit.razor
  - TenantCreate.razor

#### Domain Interfaces - удаление мультитенанси
- [ ] `src/CleanAspire.Domain/Common/IMustHaveTenant.cs`
  - Удалить интерфейс IMustHaveTenant (если больше не используется)

- [ ] Проверить использование IMayHaveTenant
  - Удалить если больше не используется

#### Infrastructure - Seed Data
- [ ] `src/CleanAspire.Infrastructure/Persistence/Seed/ApplicationDbContextInitializer.cs`
  - Удалить seed данные для Tenants

---

## 3. Удаление GitHub ссылок и иконок

### 3.1 UI компоненты с GitHub ссылками

**Описание:** Удаление всех ссылок на GitHub репозиторий из интерфейса приложения.

- [ ] `src/CleanAspire.ClientApp/Layout/Appbar.razor`
  - Удалить GitHub иконку в AppBar
  - Удалить `MudIconButton` с `Icon="@Icons.Custom.Brands.GitHub"`
  - Удалить ссылку `https://github.com/neozhu/cleanaspire.git`

- [ ] `src/CleanAspire.ClientApp/Pages/Home.razor`
  - Удалить кнопку "GitHub Repo" в Hero разделе
  - Удалить `MudButton` с `Icon="@Icons.Custom.Brands.GitHub"`
  - Удалить ссылку `https://github.com/neozhu/cleanaspire`
  - Удалить GitHub ссылку в footer (если есть)

---

### 3.2 README и документация

**Описание:** Обновление README для self-hosted версии.

- [ ] `README.md`
  - Удалить GitHub badge в начале документа
  - Удалить инструкции клонирования из GitHub
  - Удалить ссылку на Demo site (`https://cleanaspire.blazorserver.com/`)
  - Удалить ссылку на OpenAPI documentation (`https://apiservice.blazorserver.com/scalar/v1`)
  - Удалить ссылку на CleanAspire Code Generator (ChatGPT)
  - **Сохранить:** Локальные инструкции по установке и запуску

---

### 3.3 CI/CD и GitHub Actions

**Описание:** Удаление GitHub-специфичных workflow (опционально, если проект переносится на другую платформу).

- [ ] `.github/workflows/docker.yml`
  - Удалить GitHub Actions workflow для Docker build
  - **Альтернатива:** Заменить на локальный CI/CD (GitLab CI, Jenkins, etc.)

- [ ] `README.md`
  - Удалить Docker build badge

---

### 3.4 Иконки (очистка неиспользуемых)

**Описание:** После удаления OAuth и GitHub ссылок, следующие иконки больше не нужны.

- [ ] Проверить использование иконок в проекте:
  - `Icons.Custom.Brands.GitHub` - должна быть удалена
  - `Icons.Custom.Brands.Google` - должна быть удалена
  - `Icons.Custom.Brands.Microsoft` - должна быть удалена

- [ ] Если используется custom icons package - удалить неиспользуемые зависимости

---

## 4. Дополнительные задачи после удаления

### 4.1 Database Migrations

- [ ] Создать EF Core миграцию для удаления таблиц:
  - `Products`
  - `Stocks`
  - `Tenants`
  - Обновить таблицу `AspNetUsers` (удалить поля `TenantId`, `Provider`, etc.)

- [ ] Применить миграцию к базе данных

---

### 4.2 Navigation и Menu

- [ ] Проверить и обновить навигационное меню:
  - Удалить пункты меню для Products, Stocks, Tenants
  - Удалить ссылки на страницы Sign Up

- [ ] Обновить маршрутизацию (если используется централизованная конфигурация)

---

### 4.3 Permissions и Authorization

- [ ] Проверить политики авторизации:
  - Удалить permission policies для Products, Stocks, Tenants
  - Удалить роли, связанные с управлением демо-сущностями

---

### 4.4 Seed Data

- [ ] `src/CleanAspire.Infrastructure/Persistence/SeedData/` (если существует)
  - Удалить seed данные для Products, Stocks, Tenants

---

### 4.5 Unit & Integration Tests

- [ ] Найти и удалить тесты для удаленных компонентов:
  - ProductTests
  - StockTests
  - TenantTests
  - OAuth tests
  - SignUp tests

---

### 4.6 Dependencies и NuGet Packages

- [ ] Проверить зависимости и удалить неиспользуемые:
  - SendGrid (если использовался только для email confirmation)
  - Google OAuth libraries
  - Microsoft OAuth libraries

---

## 5. Контрольный список финальной проверки

- [ ] Проект успешно компилируется без ошибок
- [ ] Все юнит-тесты проходят
- [ ] База данных обновлена (применены миграции)
- [ ] Навигация и меню обновлены
- [ ] Нет сломанных ссылок в UI
- [ ] appsettings.json очищен от неиспользуемых настроек
- [ ] README.md обновлен для self-hosted версии
- [ ] Документация обновлена

---

## 6. Рекомендуемый порядок удаления (Step-by-Step)

### Этап 0: Подготовка
- [ ] **Создать резервную копию проекта и базы данных**
- [ ] **Создать отдельную Git ветку** для удаления (например, `feature/remove-demo-entities`)
- [ ] **Закоммитить текущее состояние** проекта
- [ ] **Убедиться что проект компилируется и тесты проходят**

---

### Этап 1: Документирование (КРИТИЧЕСКИ ВАЖНО)

**Задачи документирования перед удалением:**
- [x] Документировать OAuth (Google & Microsoft) в IMPLEMENTATION_REFERENCE.md
- [x] Документировать Sign Up flow в IMPLEMENTATION_REFERENCE.md
- [x] Документировать 2FA в IMPLEMENTATION_REFERENCE.md
- [ ] Документировать Stock entity и бизнес-логику
- [ ] Документировать Product entity и импорт/экспорт
- [ ] Документировать Tenant entity и мультитенанси
- [ ] Документировать все интерфейсы и абстракции

**Commit:** `git commit -m "docs: document demo entities before removal"`

---

### Этап 2: Удаление регистрации и OAuth (раздел 1)

**2.1 Регистрация пользователей**
- [x] Удалить Backend endpoints (SignUp, ConfirmEmail изменен)
- [x] Удалить Frontend pages (SignUp.razor, SignupConfirmation.razor, SignupSuccessful.razor)
- [x] Удалить Generated API clients
- [ ] Удалить EmailSender для регистрации

**Commit:** `git commit -m "feat: remove user registration functionality"` ✅

**2.2 OAuth (Google & Microsoft)**
- [x] Удалить Backend OAuth endpoints
- [x] Удалить Frontend callback pages
- [x] Удалить OAuth buttons из SignIn.razor
- [x] Удалить Generated API clients для OAuth
- [ ] Удалить конфигурацию OAuth из appsettings

**Commit:** `git commit -m "feat: remove OAuth authentication (Google & Microsoft)"`

**2.3 2FA (опционально)**
- [x] Удалить Backend 2FA endpoints
- [x] Удалить Frontend TwofactorSetting.razor
- [x] Удалить Generated API clients для 2FA
- [x] Удалить логику 2FA из основного login endpoint
- [x] Удалить логику 2FA из SignIn.razor

**Commit:** `git commit -m "feat: remove 2FA functionality"` ✅

**2.4 Очистка ApplicationUser**
- [x] Удалить поле Provider
- [x] Оставить RefreshToken/RefreshTokenExpiryTime (используется для JWT refresh)
- [x] Удалить поле TenantId
- [x] Обновить все использования Provider и TenantId

**Commit:** `git commit -m "refactor: clean up ApplicationUser fields (remove Provider and TenantId)"` ✅

**Проверка:**
- [ ] Компиляция проекта
- [ ] Запуск тестов
- [ ] Проверка работы входа по логину/паролю

---

### Этап 3: Удаление Stock entity (раздел 2.2)

**⚠️ ВАЖНО: Stock ДОЛЖЕН быть удален ПЕРЕД Product из-за FK зависимости**

**3.1 Документирование**
- [ ] Завершить документирование Stock в IMPLEMENTATION_REFERENCE.md

**3.2 Удаление (следовать чекбоксам из раздела 2.2)**
- [x] Frontend UI Pages
- [x] Frontend Generated API clients
- [x] Application Layer (Commands, Queries, DTOs, Validators)
- [x] API Endpoints
- [x] Infrastructure (Configuration, Seed data, DbSet)
- [x] Domain Entity
- [x] Tests
- [ ] Создать и применить Database Migration

**Commit:** `git commit -m "feat: remove Stock entity and related functionality"` ✅

**Проверка:**
- [ ] Компиляция проекта
- [ ] Запуск тестов
- [ ] Проверка миграции

---

### Этап 4: Удаление Product entity (раздел 2.1)

**4.1 Документирование**
- [ ] Завершить документирование Product в IMPLEMENTATION_REFERENCE.md

**4.2 Удаление (следовать чекбоксам из раздела 2.1)**
- [ ] Frontend UI Pages, Services, Autocomplete
- [ ] Frontend Generated API clients
- [ ] Application Layer (Commands, Queries, DTOs, Validators, Event Handlers)
- [ ] API Endpoints
- [ ] Infrastructure (Configuration, Seed data)
- [ ] Domain Entity
- [ ] Создать и применить Database Migration

**Commit:** `git commit -m "feat: remove Product entity and related functionality"` ✅

**Проверка:**
- [ ] Компиляция проекта
- [ ] Запуск тестов
- [ ] Проверка миграции

---

### Этап 5: Удаление Tenant entity (раздел 2.3)

**⚠️ ВАЖНО: Двухэтапное удаление - сначала разорвать связи, потом удалить entity**

**5.1 Документирование**
- [ ] Завершить документирование Tenant в IMPLEMENTATION_REFERENCE.md

**5.2 Удаление связей с ApplicationUser (ШАГ 2 из раздела 2.3)**
- [ ] Удалить TenantId из ApplicationUser
- [ ] Удалить TenantId из Request/Response models
- [ ] Удалить MultiTenantAutocomplete из SignUp.razor
- [ ] Обновить Services (CurrentUserAccessor, AuditableEntityInterceptor)
- [ ] Создать миграцию для удаления TenantId из AspNetUsers

**Commit:** `git commit -m "refactor: remove TenantId from ApplicationUser"`

**5.3 Удаление Tenant entity (ШАГ 3 из раздела 2.3)**
- [ ] Frontend Generated API clients
- [ ] Application Layer (Commands, Queries, DTOs)
- [ ] API Endpoints
- [ ] Infrastructure (Configuration, Seed data)
- [ ] Domain Entity
- [ ] Domain Interfaces (IMustHaveTenant, IMayHaveTenant)
- [ ] Создать и применить Database Migration

**Commit:** `git commit -m "feat: remove Tenant entity and multi-tenancy support"`

**Проверка:**
- [ ] Компиляция проекта
- [ ] Запуск тестов
- [ ] Проверка миграций

---

### Этап 6: Удаление GitHub ссылок (раздел 3)

**6.1 UI компоненты**
- [ ] Удалить GitHub иконку из Appbar.razor
- [ ] Удалить GitHub кнопку из Home.razor
- [ ] Удалить GitHub ссылки из footer

**6.2 README и документация**
- [ ] Обновить README.md для self-hosted версии
- [ ] Удалить ссылки на Demo site
- [ ] Удалить GitHub badges
- [ ] Обновить инструкции по установке

**6.3 CI/CD (опционально)**
- [ ] Удалить `.github/workflows/docker.yml` (если переходите на другую платформу)

**Commit:** `git commit -m "docs: update for self-hosted deployment, remove GitHub links"`

**Проверка:**
- [ ] Проверить UI на наличие сломанных ссылок

---

### Этап 7: Финальная очистка (раздел 4)

**7.1 Navigation и Menu**
- [ ] Проверить навигационное меню
- [ ] Удалить пункты для Products, Stocks, Tenants
- [ ] Удалить ссылки на SignUp

**7.2 Permissions и Authorization**
- [ ] Проверить политики авторизации
- [ ] Удалить permissions для демо-сущностей

**7.3 Dependencies**
- [ ] Проверить и удалить неиспользуемые NuGet packages
  - SendGrid (если использовался только для регистрации)
  - Google OAuth libraries
  - Microsoft OAuth libraries

**7.4 Тесты**
- [ ] Удалить тесты для удаленных компонентов
- [ ] Обновить существующие тесты

**Commit:** `git commit -m "chore: final cleanup - remove unused dependencies and tests"`

**Проверка:**
- [ ] Компиляция проекта
- [ ] Все юнит-тесты проходят
- [ ] Все интеграционные тесты проходят
- [ ] База данных обновлена (применены все миграции)

---

### Этап 8: Финальная проверка и документация

**8.1 Контрольный список**
- [ ] Проект успешно компилируется без ошибок
- [ ] Все тесты проходят
- [ ] База данных обновлена (применены миграции)
- [ ] Навигация и меню обновлены
- [ ] Нет сломанных ссылок в UI
- [ ] appsettings.json очищен от неиспользуемых настроек
- [ ] README.md обновлен для self-hosted версии
- [ ] IMPLEMENTATION_REFERENCE.md содержит всю документацию

**8.2 Тестирование**
- [ ] Вход по логину/паролю работает
- [ ] Управление профилем работает
- [ ] Базовый функционал приложения работает
- [ ] Нет ошибок в консоли браузера
- [ ] Нет ошибок в логах сервера

**8.3 Финальный commit**
- [ ] `git commit -m "feat: complete removal of demo entities and public registration"`

**8.4 Создание Pull Request**
- [ ] Создать PR из ветки `feature/remove-demo-entities` в `main`
- [ ] Добавить описание всех изменений
- [ ] Запросить code review (если применимо)

---

**Общее количество задач:** ~200+

**Прогресс выполнения:** По мере выполнения отмечайте чекбоксы `- [x]`

**Примерное время выполнения:** 2-4 рабочих дня (в зависимости от опыта)

---

## Примечания

- Все изменения необходимо делать в отдельной ветке Git
- После каждого крупного этапа создавать commit
- Тестировать приложение после каждого этапа удаления
- Сохранять документацию по удаленному функционалу для возможного восстановления

---

**Дата создания плана:** 2026-01-15

**Автор:** Claude Code Assistant
