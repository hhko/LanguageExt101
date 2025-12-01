using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// 실습 03: Fin과 Error를 사용한 API 결과 처리
/// </summary>
public static class Exercise03_ApiResults
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 03: API 결과 처리");

        // ============================================================
        // 과제 1: API 응답 파싱
        // ============================================================
        MenuHelper.PrintSubHeader("과제 1: API 응답 파싱");

        var successResponse = new ApiResponse(200, "{\"id\": 1, \"name\": \"Alice\"}");
        var errorResponse = new ApiResponse(404, "{\"error\": \"User not found\"}");
        var serverError = new ApiResponse(500, "Internal Server Error");

        var parsed1 = ParseApiResponse(successResponse);
        var parsed2 = ParseApiResponse(errorResponse);
        var parsed3 = ParseApiResponse(serverError);

        MenuHelper.PrintResult("200 OK", parsed1);
        MenuHelper.PrintResult("404 Not Found", parsed2);
        MenuHelper.PrintResult("500 Server Error", parsed3);

        // ============================================================
        // 과제 2: API 호출 체이닝
        // ============================================================
        MenuHelper.PrintSubHeader("과제 2: API 호출 체이닝");

        var orderDetails1 = GetOrderDetails(1);
        var orderDetails2 = GetOrderDetails(999);

        orderDetails1.Match(
            Succ: details => MenuHelper.PrintSuccess($"주문 상세: {details}"),
            Fail: err => MenuHelper.PrintError($"[{err.Code}] {err.Message}")
        );

        orderDetails2.Match(
            Succ: details => MenuHelper.PrintSuccess($"주문 상세: {details}"),
            Fail: err => MenuHelper.PrintError($"[{err.Code}] {err.Message}")
        );

        MenuHelper.PrintSuccess("실습 03 완료!");
    }

    private record ApiResponse(int StatusCode, string Body);
    private record User(int Id, string Name, string Email);
    private record Order(int Id, int UserId, int ProductId, decimal Amount);
    private record Product(int Id, string Name, decimal Price);

    private static readonly Dictionary<int, User> Users = new()
    {
        { 1, new User(1, "Alice", "alice@example.com") },
        { 2, new User(2, "Bob", "bob@example.com") }
    };

    private static readonly Dictionary<int, Product> Products = new()
    {
        { 1001, new Product(1001, "노트북", 1500000m) },
        { 1002, new Product(1002, "마우스", 50000m) }
    };

    private static Fin<string> ParseApiResponse(ApiResponse response)
    {
        return response.StatusCode switch
        {
            >= 200 and < 300 => FinSucc(response.Body),
            >= 400 and < 500 => FinFail<string>(Error.New(response.StatusCode, "클라이언트 오류: " + response.Body)),
            >= 500 => FinFail<string>(Error.New(response.StatusCode, "서버 오류: " + response.Body)),
            _ => FinFail<string>(Error.New(response.StatusCode, "알 수 없는 응답"))
        };
    }

    private static Fin<User> FetchUser(int userId)
    {
        return Users.TryGetValue(userId, out var user)
            ? FinSucc(user)
            : FinFail<User>(Error.New(404, $"사용자를 찾을 수 없습니다 (ID: {userId})"));
    }

    private static Fin<Product> FetchProduct(int productId)
    {
        return Products.TryGetValue(productId, out var product)
            ? FinSucc(product)
            : FinFail<Product>(Error.New(404, $"상품을 찾을 수 없습니다 (ID: {productId})"));
    }

    private static Fin<string> GetOrderDetails(int userId)
    {
        return from user in FetchUser(userId)
               from product in FetchProduct(1001)
               select $"{user.Name}님의 상품: {product.Name} ({product.Price:N0}원)";
    }
}
