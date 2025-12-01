using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter03_Either;

/// <summary>
/// 실습 02: Either를 사용한 에러 처리
///
/// 학습 목표:
/// - Either를 사용한 단계별 검증
/// - 에러 전파 패턴 이해
/// - 실무적인 에러 처리 구현
///
/// 과제:
/// 1. 주문 정보 검증 파이프라인 구현
/// 2. 결제 처리 시뮬레이션
/// 3. 에러 복구 패턴 구현
/// </summary>
public static class Exercise02_ErrorHandling
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 02: 에러 처리");

        // ============================================================
        // 과제 1: 주문 검증 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("과제 1: 주문 검증 파이프라인");

        MenuHelper.PrintExplanation("주문 정보를 단계별로 검증하는 파이프라인을 구현하세요.");
        MenuHelper.PrintExplanation("각 검증 단계에서 실패하면 적절한 에러 메시지를 반환합니다.");
        MenuHelper.PrintBlankLines();

        // 테스트 케이스
        var validOrder = CreateOrder("P001", 3, "서울시 강남구");
        var invalidProduct = CreateOrder("P999", 3, "서울시 강남구");
        var invalidQuantity = CreateOrder("P001", 100, "서울시 강남구");
        var invalidAddress = CreateOrder("P001", 2, "");

        MenuHelper.PrintResult("유효한 주문", validOrder);
        MenuHelper.PrintResult("잘못된 상품", invalidProduct);
        MenuHelper.PrintResult("재고 초과", invalidQuantity);
        MenuHelper.PrintResult("주소 누락", invalidAddress);

        // ============================================================
        // 과제 2: 결제 처리
        // ============================================================
        MenuHelper.PrintSubHeader("과제 2: 결제 처리");

        MenuHelper.PrintExplanation("주문 → 결제 → 확인의 전체 흐름을 구현하세요.");
        MenuHelper.PrintExplanation("각 단계에서 실패 가능성이 있습니다.");
        MenuHelper.PrintBlankLines();

        var paymentResult1 = ProcessPayment("P001", 2, "서울시 강남구", "card_valid");
        var paymentResult2 = ProcessPayment("P001", 2, "서울시 강남구", "card_expired");
        var paymentResult3 = ProcessPayment("P001", 2, "서울시 강남구", "card_insufficient");

        paymentResult1.Match(
            Right: r => MenuHelper.PrintSuccess($"결제 성공: {r}"),
            Left: e => MenuHelper.PrintError($"결제 실패: {e}")
        );

        paymentResult2.Match(
            Right: r => MenuHelper.PrintSuccess($"결제 성공: {r}"),
            Left: e => MenuHelper.PrintError($"결제 실패: {e}")
        );

        paymentResult3.Match(
            Right: r => MenuHelper.PrintSuccess($"결제 성공: {r}"),
            Left: e => MenuHelper.PrintError($"결제 실패: {e}")
        );

        // ============================================================
        // 과제 3: 에러 복구 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("과제 3: 에러 복구 패턴");

        MenuHelper.PrintExplanation("실패한 경우 대체 로직을 실행하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        // 주 결제 수단 실패 시 대체 결제 수단 시도
        var result = TryPaymentWithFallback("card_expired");
        result.Match(
            Right: r => MenuHelper.PrintSuccess($"최종 결제 성공: {r}"),
            Left: e => MenuHelper.PrintError($"모든 결제 실패: {e}")
        );

        MenuHelper.PrintSuccess("실습 02 완료!");
    }

    // ============================================================
    // 구현할 함수들
    // ============================================================

    // 상품 데이터
    private static readonly Dictionary<string, (string Name, decimal Price, int Stock)> Products = new()
    {
        { "P001", ("노트북", 1500000m, 10) },
        { "P002", ("마우스", 50000m, 50) },
        { "P003", ("키보드", 100000m, 0) }  // 품절
    };

    /// <summary>
    /// 상품 검증
    /// </summary>
    private static Either<string, (string Name, decimal Price, int Stock)> ValidateProduct(string productId)
    {
        // TODO: 상품이 존재하는지 확인
        // 없으면 Left("상품을 찾을 수 없습니다: {productId}")
        return Products.TryGetValue(productId, out var product)
            ? Right(product)
            : Left($"상품을 찾을 수 없습니다: {productId}");
    }

    /// <summary>
    /// 수량 검증
    /// </summary>
    private static Either<string, int> ValidateQuantity(int quantity, int stock)
    {
        // TODO: 수량이 1 이상이고, 재고 이하인지 확인
        if (quantity <= 0)
            return Left("수량은 1 이상이어야 합니다");
        if (quantity > stock)
            return Left($"재고가 부족합니다. 현재 재고: {stock}");
        return Right(quantity);
    }

    /// <summary>
    /// 주소 검증
    /// </summary>
    private static Either<string, string> ValidateAddress(string address)
    {
        // TODO: 주소가 비어있지 않은지 확인
        if (string.IsNullOrWhiteSpace(address))
            return Left("배송 주소는 필수입니다");
        return Right(address);
    }

    /// <summary>
    /// 주문 생성 파이프라인
    /// </summary>
    private static Either<string, Order> CreateOrder(string productId, int quantity, string address)
    {
        // TODO: ValidateProduct, ValidateQuantity, ValidateAddress를 체이닝하여
        // Order 객체를 생성하세요
        return from product in ValidateProduct(productId)
               from qty in ValidateQuantity(quantity, product.Stock)
               from addr in ValidateAddress(address)
               select new Order(
                   ProductId: productId,
                   ProductName: product.Name,
                   Quantity: qty,
                   TotalPrice: product.Price * qty,
                   Address: addr
               );
    }

    /// <summary>
    /// 결제 처리
    /// </summary>
    private static Either<string, string> ChargePayment(string cardToken, decimal amount)
    {
        // TODO: cardToken에 따라 결제 성공/실패 시뮬레이션
        // "card_valid" → 성공
        // "card_expired" → Left("카드가 만료되었습니다")
        // "card_insufficient" → Left("잔액이 부족합니다")
        return cardToken switch
        {
            "card_valid" => Right($"결제완료 (금액: {amount:N0}원)"),
            "card_expired" => Left("카드가 만료되었습니다"),
            "card_insufficient" => Left("잔액이 부족합니다"),
            _ => Left("알 수 없는 결제 오류")
        };
    }

    /// <summary>
    /// 전체 결제 프로세스
    /// </summary>
    private static Either<string, string> ProcessPayment(
        string productId,
        int quantity,
        string address,
        string cardToken)
    {
        // TODO: CreateOrder와 ChargePayment를 체이닝
        return from order in CreateOrder(productId, quantity, address)
               from payment in ChargePayment(cardToken, order.TotalPrice)
               select $"주문 완료 - {order.ProductName} x {order.Quantity}, {payment}";
    }

    /// <summary>
    /// 대체 결제 수단으로 시도
    /// </summary>
    private static Either<string, string> TryPaymentWithFallback(string primaryCard)
    {
        // TODO: 주 결제 실패 시 대체 결제 시도
        // 힌트: Match를 사용하여 Left인 경우 다른 결제 시도
        var primaryResult = ChargePayment(primaryCard, 100000m);

        return primaryResult.Match(
            Right: r => Right(r),
            Left: _ =>
            {
                Console.WriteLine("  주 결제 실패, 대체 결제 시도...");
                return ChargePayment("card_valid", 100000m)
                    .MapLeft(e => $"주 결제 및 대체 결제 모두 실패: {e}");
            }
        );
    }

    // 주문 레코드
    private record Order(
        string ProductId,
        string ProductName,
        int Quantity,
        decimal TotalPrice,
        string Address);
}
