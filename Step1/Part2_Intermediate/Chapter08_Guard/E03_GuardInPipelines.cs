using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter08_Guard;

/// <summary>
/// 파이프라인에서 guard 활용을 학습합니다.
///
/// 학습 목표:
/// - 데이터 파이프라인에서 조건부 필터링
/// - 에러 처리와 guard 결합
/// - 복잡한 비즈니스 로직 표현
///
/// 핵심 개념:
/// guard는 파이프라인 중간에 조건 검사를 삽입합니다.
/// 조건이 맞지 않으면 해당 요소가 걸러지거나 흐름이 중단됩니다.
/// </summary>
public static class E03_GuardInPipelines
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 08-E03: 파이프라인에서 guard");

        // ============================================================
        // 1. 데이터 필터링 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("1. 데이터 필터링 파이프라인");

        var products = Seq(
            new Product("P001", "노트북", 1500000m, 10, "전자제품"),
            new Product("P002", "마우스", 50000m, 100, "전자제품"),
            new Product("P003", "책상", 300000m, 0, "가구"),  // 품절
            new Product("P004", "의자", 200000m, 5, "가구"),
            new Product("P005", "모니터", 500000m, 3, "전자제품")
        );

        MenuHelper.PrintExplanation("재고가 있고 50만원 이하인 상품만 필터링");
        MenuHelper.PrintBlankLines();

        var affordable = products
            .Filter(p => p.Stock > 0)           // 재고 있음
            .Filter(p => p.Price <= 500000m);   // 50만원 이하

        foreach (var p in affordable)
        {
            Console.WriteLine($"  - {p.Name}: {p.Price:N0}원 (재고: {p.Stock})");
        }

        // ============================================================
        // 2. Option 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("2. Option 파이프라인");

        MenuHelper.PrintExplanation("여러 조건을 거치는 Option 파이프라인");
        MenuHelper.PrintBlankLines();

        var result1 = ProcessOrder(1, 3);  // 유효한 주문
        var result2 = ProcessOrder(1, 100);  // 재고 초과
        var result3 = ProcessOrder(999, 1);  // 상품 없음

        MenuHelper.PrintResult("상품1, 수량3", result1);
        MenuHelper.PrintResult("상품1, 수량100", result2);
        MenuHelper.PrintResult("상품999, 수량1", result3);

        // ============================================================
        // 3. Either와 조건 검사
        // ============================================================
        MenuHelper.PrintSubHeader("3. Either와 조건 검사");

        MenuHelper.PrintExplanation("조건 실패 시 명확한 에러 메시지 제공");
        MenuHelper.PrintBlankLines();

        var order1 = CreateOrder("P001", 5, 10);
        var order2 = CreateOrder("P001", 5, 3);  // 재고 부족
        var order3 = CreateOrder("P999", 1, 10);  // 상품 없음

        order1.Match(
            Right: o => MenuHelper.PrintSuccess($"주문 생성: {o}"),
            Left: e => MenuHelper.PrintError(e)
        );

        order2.Match(
            Right: o => MenuHelper.PrintSuccess($"주문 생성: {o}"),
            Left: e => MenuHelper.PrintError(e)
        );

        order3.Match(
            Right: o => MenuHelper.PrintSuccess($"주문 생성: {o}"),
            Left: e => MenuHelper.PrintError(e)
        );

        // ============================================================
        // 4. 복합 비즈니스 규칙
        // ============================================================
        MenuHelper.PrintSubHeader("4. 복합 비즈니스 규칙");

        var customers = Seq(
            new Customer("C001", "김철수", "VIP", 1500000m, true),
            new Customer("C002", "이영희", "일반", 50000m, true),
            new Customer("C003", "박민수", "VIP", 2000000m, false),  // 비활성
            new Customer("C004", "최지영", "일반", 500000m, true)
        );

        MenuHelper.PrintExplanation("VIP이면서 활성 상태인 고객의 총 구매액");
        var vipTotal = customers
            .Filter(c => c.IsActive)
            .Filter(c => c.Tier == "VIP")
            .Map(c => c.TotalPurchase)
            .Fold(0m, (acc, x) => acc + x);

        MenuHelper.PrintResult("VIP 활성 고객 총 구매액", $"{vipTotal:N0}원");

        // ============================================================
        // 5. 단계별 검증 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("5. 단계별 검증 파이프라인");

        var registration1 = ValidateRegistration("alice", "alice@example.com", 25);
        var registration2 = ValidateRegistration("a", "bad", 15);

        registration1.Match(
            Right: r => MenuHelper.PrintSuccess($"등록 완료: {r}"),
            Left: e => MenuHelper.PrintError($"등록 실패: {e}")
        );

        registration2.Match(
            Right: r => MenuHelper.PrintSuccess($"등록 완료: {r}"),
            Left: e => MenuHelper.PrintError($"등록 실패: {e}")
        );

        // ============================================================
        // 6. Choose 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("6. Choose 패턴");

        MenuHelper.PrintExplanation("Choose는 Some인 것만 선택하고 값을 추출합니다.");
        MenuHelper.PrintBlankLines();

        var ids = Seq("1", "abc", "2", "def", "3");

        var parsed = ids.Choose(s =>
            int.TryParse(s, out var n) ? Some(n) : None
        );

        MenuHelper.PrintResult("파싱 가능한 숫자만", parsed);

        // ============================================================
        // 7. 실전 예제: 주문 처리 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("7. 실전 예제: 주문 처리 파이프라인");

        var orders = Seq(
            new OrderRequest("C001", "P001", 2),
            new OrderRequest("C001", "P003", 1),  // 품절 상품
            new OrderRequest("C003", "P002", 5),  // 비활성 고객
            new OrderRequest("C002", "P004", 10)  // 재고 부족
        );

        var processedOrders = orders
            .Map(ProcessOrderRequest)
            .ToSeq();

        foreach (var (req, result) in orders.Zip(processedOrders))
        {
            result.Match(
                Succ: msg => Console.WriteLine($"  [성공] {req.CustomerId}: {msg}"),
                Fail: err => Console.WriteLine($"  [실패] {req.CustomerId}: {err.Message}")
            );
        }

        MenuHelper.PrintSuccess("파이프라인에서 guard 학습 완료!");
    }

    // ============================================================
    // 데이터 타입
    // ============================================================

    private record Product(string Id, string Name, decimal Price, int Stock, string Category);
    private record Customer(string Id, string Name, string Tier, decimal TotalPurchase, bool IsActive);
    private record OrderRequest(string CustomerId, string ProductId, int Quantity);

    // 시뮬레이션 데이터
    private static readonly Map<string, Product> ProductDb = Map(
        ("P001", new Product("P001", "노트북", 1500000m, 10, "전자제품")),
        ("P002", new Product("P002", "마우스", 50000m, 100, "전자제품")),
        ("P003", new Product("P003", "책상", 300000m, 0, "가구")),
        ("P004", new Product("P004", "의자", 200000m, 5, "가구"))
    );

    private static readonly Map<string, Customer> CustomerDb = Map(
        ("C001", new Customer("C001", "김철수", "VIP", 1500000m, true)),
        ("C002", new Customer("C002", "이영희", "일반", 50000m, true)),
        ("C003", new Customer("C003", "박민수", "VIP", 2000000m, false))
    );

    // ============================================================
    // 파이프라인 함수들
    // ============================================================

    private static Option<string> ProcessOrder(int productId, int quantity)
    {
        var productKey = $"P00{productId}";
        return
            from product in ProductDb.Find(productKey)
            where product.Stock > 0
            where product.Stock >= quantity
            select $"주문 완료: {product.Name} x {quantity}";
    }

    private static Either<string, string> CreateOrder(string productId, int quantity, int availableStock)
    {
        return ProductDb.Find(productId).Match(
            Some: product =>
            {
                if (product.Stock == 0)
                    return Left<string, string>("상품이 품절되었습니다");
                if (product.Stock < quantity)
                    return Left<string, string>($"재고가 부족합니다 (현재: {product.Stock})");
                return Right<string, string>($"{product.Name} x {quantity} 주문");
            },
            None: () => Left<string, string>("상품을 찾을 수 없습니다")
        );
    }

    private static Either<string, string> ValidateRegistration(string username, string email, int age)
    {
        if (username.Length < 2)
            return Left("사용자명은 2자 이상이어야 합니다");
        if (!email.Contains('@'))
            return Left("올바른 이메일 형식이 아닙니다");
        if (age < 18)
            return Left("18세 이상만 가입할 수 있습니다");

        return Right($"{username} ({email})");
    }

    private static Fin<string> ProcessOrderRequest(OrderRequest request)
    {
        // 고객 확인
        var customer = CustomerDb.Find(request.CustomerId);
        if (customer.IsNone)
            return FinFail<string>(Error.New("고객을 찾을 수 없습니다"));

        var cust = customer.IfNone(() => throw new InvalidOperationException());
        if (!cust.IsActive)
            return FinFail<string>(Error.New("비활성 고객입니다"));

        // 상품 확인
        var product = ProductDb.Find(request.ProductId);
        if (product.IsNone)
            return FinFail<string>(Error.New("상품을 찾을 수 없습니다"));

        var prod = product.IfNone(() => throw new InvalidOperationException());
        if (prod.Stock == 0)
            return FinFail<string>(Error.New("품절된 상품입니다"));
        if (prod.Stock < request.Quantity)
            return FinFail<string>(Error.New($"재고 부족 (현재: {prod.Stock})"));

        return FinSucc($"{prod.Name} x {request.Quantity}");
    }
}
