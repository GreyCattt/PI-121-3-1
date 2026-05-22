```mermaid
classDiagram
    direction TB

    %% Контролери API
    class ControllerBase {
        <<Framework Class>>
    }

    class LotsController {
        -ILotService _lotService
        +GetAllLots() Task~ActionResult~
        +SearchAndFilterLots(string searchQuery, int? categoryId, string status, decimal? minPrice, decimal? maxPrice) Task~ActionResult~
        +GetLotById(int id) Task~ActionResult~
        +CreateLot(CreateLotRequest request) Task~ActionResult~
        +ApproveLot(int id, int managerId) Task~IActionResult~
        +UpdateLot(int id, UpdateLotRequest request) Task~IActionResult~
        +DeleteLot(int id) Task~IActionResult~
    }

    class CategoriesController {
        -ICategoryService _categoryService
        +GetAll() Task~ActionResult~
        +Create(CategoryDto request) Task~IActionResult~
        +Delete(int id) Task~IActionResult~
    }

    class AuthController {
        -IAuthService _authService
        +Login(LoginRequest request) Task~IActionResult~
        +Register(RegisterRequest request) Task~IActionResult~
        +Me() Task~ActionResult~
    }

    class AuctionController {
        -IAuctionService _auctionService
        +PlaceBid(PlaceBidRequest request) Task~IActionResult~
    }

    %% Вхідні моделі запитів (Request Models)
    class CreateLotRequest {
        +string Title
        +string Description
        +decimal StartingPrice
        +DateTime StartTime
        +DateTime EndTime
        +int CategoryId
        +LotStatus? Status
    }

    class UpdateLotRequest {
        +string Title
        +string Description
        +decimal StartingPrice
        +DateTime StartTime
        +DateTime EndTime
        +int CategoryId
        +LotStatus Status
    }

    class LoginRequest {
        +string Email
        +string Password
    }

    class RegisterRequest {
        +string Username
        +string Email
        +string Password
    }

    class PlaceBidRequest {
        +int LotId
        +decimal Amount
    }

    %% Глобальна обробка помилок
    class ExceptionHandlingMiddleware {
        -RequestDelegate _next
        +InvokeAsync(HttpContext context) Task
        -HandleExceptionAsync(HttpContext context, Exception exception)$ Task
    }

    %% Зв'язки успадкування з ASP.NET Core
    ControllerBase <|-- LotsController
    ControllerBase <|-- CategoriesController
    ControllerBase <|-- AuthController
    ControllerBase <|-- AuctionController

    %% Залежності контролерів від моделей та сервісів BLL
    LotsController --> ILotService : викликає операції
    LotsController ..> CreateLotRequest : приймає з body
    LotsController ..> UpdateLotRequest : приймає з body

    CategoriesController --> ICategoryService : викликає операції
    
    AuthController --> IAuthService : викликає операції
    AuthController ..> LoginRequest : приймає з body
    AuthController ..> RegisterRequest : приймає з body

    AuctionController --> IAuctionService : викликає операції
    AuctionController ..> PlaceBidRequest : приймає з body

    ExceptionHandlingMiddleware ..> Exception : перехоплює та серіалізує у JSON 400/404/500
    ```