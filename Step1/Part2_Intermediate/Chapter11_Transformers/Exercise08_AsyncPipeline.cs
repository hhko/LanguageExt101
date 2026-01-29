using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter11_Transformers;

/// <summary>
/// 모나드 트랜스포머를 활용한 실전 비동기 파이프라인을 학습합니다.
///
/// 학습 목표:
/// - OptionT/EitherT/FinT 통합 사용
/// - 복합 에러 처리와 폴백
/// - 트랜스포머 간 변환
/// - 실전 애플리케이션 패턴
///
/// 시나리오:
/// 온라인 쇼핑몰의 주문 처리 파이프라인을 구현합니다.
/// - 사용자 조회 (있음/없음)
/// - 장바구니 조회 (에러 가능)
/// - 재고 검증 (상세 에러 정보)
/// - 주문 생성 (복합 처리)
/// </summary>
public static class Exercise08_AsyncPipeline
{
    // ============================================================
    // 2. 데이터 모델
    // ============================================================
    private record User(int Id, string Name, string Email);
    private record Product(int Id, string Name, decimal Price, int Stock);
    private record CartItem(int ProductId, int Quantity);
    private record Order(int Id, int UserId, Seq<CartItem> Items, decimal TotalAmount, string Status);

    // 시뮬레이션 데이터
    private static readonly HashMap<int, User> Users = HashMap(
        (1, new User(1, "Alice", "alice@example.com")),
        (2, new User(2, "Bob", "bob@example.com"))
    );

    private static readonly HashMap<int, Product> Products = HashMap(
        (101, new Product(101, "노트북", 1500000, 5)),
        (102, new Product(102, "마우스", 50000, 100)),
        (103, new Product(103, "키보드", 150000, 0))  // 재고 없음
    );

    private static readonly HashMap<int, Seq<CartItem>> Carts = HashMap(
        (1, Seq(new CartItem(101, 1), new CartItem(102, 2))),  // Alice의 장바구니
        (2, Seq(new CartItem(103, 1)))  // Bob의 장바구니 (재고 없는 상품)
    );

    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 08: 비동기 파이프라인");

        // ============================================================
        // 1. 시나리오 소개
        // ============================================================
        MenuHelper.PrintSubHeader("1. 시나리오 소개");

        MenuHelper.PrintExplanation("온라인 쇼핑몰 주문 처리 파이프라인:");
        MenuHelper.PrintExplanation("  1. 사용자 조회 → 없으면 게스트 처리");
        MenuHelper.PrintExplanation("  2. 장바구니 조회 → 에러 시 복구");
        MenuHelper.PrintExplanation("  3. 재고 검증 → 상세 에러 반환");
        MenuHelper.PrintExplanation("  4. 주문 생성 → 최종 결과");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 파이프라인 구조
User → Cart → ValidatedItems → Order
  ↓      ↓          ↓            ↓
OptionT  EitherT    FinT      최종결과");

        // ============================================================
        // 2. 데이터 모델 소개
        // ============================================================
        MenuHelper.PrintSubHeader("2. 데이터 모델");

        MenuHelper.PrintExplanation("사용되는 도메인 모델:");
        MenuHelper.PrintCode(@"record User(int Id, string Name, string Email);
record Product(int Id, string Name, decimal Price, int Stock);
record CartItem(int ProductId, int Quantity);
record Order(int Id, int UserId, Seq<CartItem> Items, decimal TotalAmount, string Status);");

        Console.WriteLine("    등록된 사용자: Alice(1), Bob(2)");
        Console.WriteLine("    등록된 상품: 노트북(101), 마우스(102), 키보드(103, 재고없음)");
        Console.WriteLine("    장바구니: Alice→[노트북,마우스], Bob→[키보드]");

        // ============================================================
        // 3. OptionT 파이프라인 - 있음/없음 처리
        // ============================================================
        MenuHelper.PrintSubHeader("3. OptionT 파이프라인 - 있음/없음 처리");

        MenuHelper.PrintExplanation("OptionT: 사용자 조회 (있음/없음만 처리)");
        MenuHelper.PrintBlankLines();

        OptionT<IO, User> FindUserOption(int id) =>
            Users.Find(id).Match(
                Some: user => OptionT.Some<IO, User>(user),
                None: () => OptionT.None<IO, User>());

        MenuHelper.PrintCode(@"// OptionT로 사용자 조회
OptionT<IO, User> FindUserOption(int id) =>
    Users.Find(id).Match(
        Some: user => OptionT.Some<IO, User>(user),
        None: () => OptionT.None<IO, User>());");

        var optionUser1 = FindUserOption(1).Run().Run();
        MenuHelper.PrintResult("FindUserOption(1)", optionUser1);

        var optionUser999 = FindUserOption(999).Run().Run();
        MenuHelper.PrintResult("FindUserOption(999)", optionUser999);

        // OptionT 폴백
        var optionWithFallback = FindUserOption(999)
            .BindNone(() => OptionT.Some<IO, User>(new User(0, "Guest", "guest@example.com")));
        MenuHelper.PrintResult("FindUserOption(999).BindNone(Guest)", optionWithFallback.Run().Run());

        // ============================================================
        // 4. EitherT 파이프라인 - 에러 메시지 처리
        // ============================================================
        MenuHelper.PrintSubHeader("4. EitherT 파이프라인 - 에러 메시지 처리");

        MenuHelper.PrintExplanation("EitherT: 장바구니 조회 (에러 메시지 포함)");
        MenuHelper.PrintBlankLines();

        EitherT<string, IO, Seq<CartItem>> GetCartEither(int userId) =>
            Carts.Find(userId).Match(
                Some: items => EitherT.Right<string, IO, Seq<CartItem>>(items),
                None: () => EitherT.Left<string, IO, Seq<CartItem>>($"사용자 {userId}의 장바구니가 비어있습니다"));

        MenuHelper.PrintCode(@"// EitherT로 장바구니 조회
EitherT<string, IO, Seq<CartItem>> GetCartEither(int userId) =>
    Carts.Find(userId).Match(
        Some: items => EitherT.Right<string, IO, Seq<CartItem>>(items),
        None: () => EitherT.Left<string, IO, Seq<CartItem>>($""사용자 {userId}의 장바구니가 비어있습니다""));");

        var eitherCart1 = GetCartEither(1).Run().Run();
        MenuHelper.PrintResult("GetCartEither(1)", eitherCart1);

        var eitherCart999 = GetCartEither(999).Run().Run();
        MenuHelper.PrintResult("GetCartEither(999)", eitherCart999);

        // EitherT 에러 복구
        var eitherWithFallback = GetCartEither(999)
            | EitherT.Right<string, IO, Seq<CartItem>>(Seq<CartItem>());
        MenuHelper.PrintResult("GetCartEither(999) | 빈 장바구니", eitherWithFallback.Run().Run());

        // ============================================================
        // 5. FinT 파이프라인 - Error 객체 처리
        // ============================================================
        MenuHelper.PrintSubHeader("5. FinT 파이프라인 - Error 객체 처리");

        MenuHelper.PrintExplanation("FinT: 재고 검증 (상세 에러 정보 포함)");
        MenuHelper.PrintBlankLines();

        FinT<IO, Seq<CartItem>> ValidateStock(Seq<CartItem> items)
        {
            FinT<IO, Seq<CartItem>> ValidateItem(FinT<IO, Seq<CartItem>> acc, CartItem item) =>
                acc.Bind(validated =>
                    Products.Find(item.ProductId).Match(
                        Some: product => product.Stock >= item.Quantity
                            ? FinT.Succ<IO, Seq<CartItem>>(validated.Add(item))
                            : FinT.Fail<IO, Seq<CartItem>>(Error.New(400,
                                $"재고 부족: {product.Name} (요청: {item.Quantity}, 재고: {product.Stock})")),
                        None: () => FinT.Fail<IO, Seq<CartItem>>(Error.New(404,
                            $"상품을 찾을 수 없습니다: {item.ProductId}"))));

            return items.Fold(
                FinT.Succ<IO, Seq<CartItem>>(Seq<CartItem>()),
                ValidateItem);
        }

        MenuHelper.PrintCode(@"// FinT로 재고 검증
FinT<IO, Seq<CartItem>> ValidateStock(Seq<CartItem> items) =>
    // 각 아이템의 재고 확인
    // 재고 부족 시 Error.New(400, ""재고 부족: ..."")
    // 상품 없음 시 Error.New(404, ""상품을 찾을 수 없습니다"")");

        var aliceCart = Carts[1];  // 노트북, 마우스
        var validatedAlice = ValidateStock(aliceCart).Run().Run();
        MenuHelper.PrintResult("ValidateStock(Alice 장바구니)", validatedAlice);

        var bobCart = Carts[2];  // 키보드 (재고 없음)
        var validatedBob = ValidateStock(bobCart).Run().Run();
        MenuHelper.PrintResult("ValidateStock(Bob 장바구니)", validatedBob);
        validatedBob.IfFail(err => Console.WriteLine($"    에러 코드: {err.Code}"));

        // ============================================================
        // 6. 폴백 체인 - 주 소스 → 백업 → 캐시 → 기본값
        // ============================================================
        MenuHelper.PrintSubHeader("6. 폴백 체인");

        MenuHelper.PrintExplanation("여러 단계의 폴백을 체인으로 연결:");
        MenuHelper.PrintExplanation("  주 소스 실패 → 백업 소스 → 캐시 → 기본값");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션: 외부 API 호출
        FinT<IO, User> FetchFromMainApi(int id) =>
            FinT.Fail<IO, User>(Error.New(503, "주 서버 점검 중"));

        FinT<IO, User> FetchFromBackupApi(int id) =>
            FinT.Fail<IO, User>(Error.New(503, "백업 서버 점검 중"));

        FinT<IO, User> FetchFromCache(int id) =>
            id == 1
                ? FinT.Succ<IO, User>(new User(1, "Alice (캐시)", "alice@cache.com"))
                : FinT.Fail<IO, User>(Error.New(404, "캐시에 없음"));

        User GetDefaultUser() => new User(0, "Guest", "guest@example.com");

        MenuHelper.PrintCode(@"// 폴백 체인
var user = FetchFromMainApi(userId)
    .BindFail(_ => FetchFromBackupApi(userId))  // 주 서버 실패 시 백업
    .BindFail(_ => FetchFromCache(userId))      // 백업 실패 시 캐시
    .IfFail(GetDefaultUser());                  // 캐시도 실패 시 기본값");

        var fallbackChain = FetchFromMainApi(1)
            .BindFail(_ => FetchFromBackupApi(1))
            .BindFail(_ => FetchFromCache(1))
            .IfFail(GetDefaultUser());
        MenuHelper.PrintResult("폴백 체인 (userId=1, 캐시 히트)", fallbackChain.Run());

        var fallbackChain2 = FetchFromMainApi(999)
            .BindFail(_ => FetchFromBackupApi(999))
            .BindFail(_ => FetchFromCache(999))
            .IfFail(GetDefaultUser());
        MenuHelper.PrintResult("폴백 체인 (userId=999, 기본값)", fallbackChain2.Run());

        // ============================================================
        // 7. 트랜스포머 변환
        // ============================================================
        MenuHelper.PrintSubHeader("7. 트랜스포머 변환");

        MenuHelper.PrintExplanation("트랜스포머 간 변환:");
        MenuHelper.PrintExplanation("  - OptionT → EitherT: None을 에러 메시지로 변환");
        MenuHelper.PrintExplanation("  - EitherT → FinT: 문자열 에러를 Error로 변환");
        MenuHelper.PrintExplanation("  - FinT → OptionT: Error 정보 손실");
        MenuHelper.PrintBlankLines();

        // OptionT → EitherT
        var optionToEither = FindUserOption(999)
            .ToEither("사용자를 찾을 수 없습니다");
        MenuHelper.PrintResult("OptionT.ToEither(\"에러메시지\")", optionToEither.Run().Run());

        // EitherT → OptionT (에러 정보 손실)
        var eitherToOption = GetCartEither(999).ToOption();
        MenuHelper.PrintResult("EitherT.ToOption()", eitherToOption.Run().Run());

        // FinT → OptionT (Error 정보 손실)
        var finToOption = ValidateStock(bobCart).ToOption();
        MenuHelper.PrintResult("FinT.ToOption()", finToOption.Run().Run());

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("변환 시 정보 손실 주의:");
        Console.WriteLine("    OptionT → EitherT/FinT: 에러 정보 추가 필요");
        Console.WriteLine("    EitherT/FinT → OptionT: 에러 정보 손실됨");

        // ============================================================
        // 8. 통합 파이프라인 - 실전 주문 처리
        // ============================================================
        MenuHelper.PrintSubHeader("8. 통합 파이프라인 - 실전 주문 처리");

        MenuHelper.PrintExplanation("전체 주문 처리 파이프라인을 FinT로 통합합니다.");
        MenuHelper.PrintBlankLines();

        // FinT 기반 통합 함수들
        FinT<IO, User> FindUserFin(int id) =>
            Users.Find(id).Match(
                Some: user => FinT.Succ<IO, User>(user),
                None: () => FinT.Fail<IO, User>(Error.New(404, $"사용자 ID {id}를 찾을 수 없습니다")));

        FinT<IO, Seq<CartItem>> GetCartFin(int userId) =>
            Carts.Find(userId).Match(
                Some: items => FinT.Succ<IO, Seq<CartItem>>(items),
                None: () => FinT.Fail<IO, Seq<CartItem>>(Error.New(404, $"사용자 {userId}의 장바구니가 비어있습니다")));

        FinT<IO, decimal> CalculateTotal(Seq<CartItem> items) =>
            FinT.Succ<IO, decimal>(items.Sum(item =>
                Products.Find(item.ProductId)
                    .Map(p => p.Price * item.Quantity)
                    .IfNone(0)));

        FinT<IO, Order> CreateOrder(User user, Seq<CartItem> items, decimal total) =>
            FinT.Succ<IO, Order>(new Order(
                Id: Random.Shared.Next(1000, 9999),
                UserId: user.Id,
                Items: items,
                TotalAmount: total,
                Status: "Created"));

        MenuHelper.PrintCode(@"// 통합 파이프라인
var orderPipeline =
    from user in FindUserFin(userId)
        .BindFail(err => err.Code == 404
            ? FinT.Succ<IO, User>(guestUser)
            : FinT.Fail<IO, User>(err))
    from cart in GetCartFin(user.Id)
    from validated in ValidateStock(cart)
    from total in CalculateTotal(validated)
    from order in CreateOrder(user, validated, total)
    select order;");

        // 성공 케이스: Alice
        var alicePipeline =
            from user in FindUserFin(1)
            from cart in GetCartFin(user.Id)
            from validated in ValidateStock(cart)
            from total in CalculateTotal(validated)
            from order in CreateOrder(user, validated, total)
            select order;

        var aliceResult = alicePipeline.Run().Run();
        MenuHelper.PrintResult("Alice 주문 처리", aliceResult);
        aliceResult.IfSucc(order =>
        {
            Console.WriteLine($"    주문번호: {order.Id}");
            Console.WriteLine($"    고객: {order.UserId}");
            Console.WriteLine($"    총액: {order.TotalAmount:N0}원");
        });

        // 실패 케이스: Bob (재고 부족)
        var bobPipeline =
            from user in FindUserFin(2)
            from cart in GetCartFin(user.Id)
            from validated in ValidateStock(cart)
            from total in CalculateTotal(validated)
            from order in CreateOrder(user, validated, total)
            select order;

        var bobResult = bobPipeline.Run().Run();
        MenuHelper.PrintResult("Bob 주문 처리", bobResult);
        bobResult.IfFail(err =>
        {
            Console.WriteLine($"    에러 코드: {err.Code}");
            Console.WriteLine($"    에러 메시지: {err.Message}");
        });

        // 에러 복구 포함 파이프라인
        var guestUser = new User(0, "Guest", "guest@example.com");

        var recoveredPipeline =
            from user in FindUserFin(999)
                .BindFail(err => err.Code == 404
                    ? FinT.Succ<IO, User>(guestUser)
                    : FinT.Fail<IO, User>(err))
            from cart in GetCartFin(user.Id)
                .BindFail(_ => FinT.Succ<IO, Seq<CartItem>>(Seq<CartItem>()))
            select $"{user.Name}의 장바구니: {cart.Count}개 상품";

        var recoveredResult = recoveredPipeline.Run().Run();
        MenuHelper.PrintResult("복구된 파이프라인 (Guest)", recoveredResult);

        // 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== 트랜스포머 선택 가이드 ===");
        Console.WriteLine("    ┌──────────┬──────────────────────────────────┐");
        Console.WriteLine("    │ 트랜스포머│ 사용 시나리오                    │");
        Console.WriteLine("    ├──────────┼──────────────────────────────────┤");
        Console.WriteLine("    │ OptionT  │ 단순 있음/없음 (에러 정보 불필요)│");
        Console.WriteLine("    │ EitherT  │ 커스텀 에러 타입 필요            │");
        Console.WriteLine("    │ FinT     │ 표준 Error + 에러 코드/메시지    │");
        Console.WriteLine("    └──────────┴──────────────────────────────────┘");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== 파이프라인 패턴 요약 ===");
        Console.WriteLine("    1. 첫 번째 from의 타입이 전체 컨텍스트 결정");
        Console.WriteLine("    2. BindFail/BindNone으로 단계별 에러 복구");
        Console.WriteLine("    3. 최종 Match/IfFail/IfSucc로 결과 처리");
        Console.WriteLine("    4. Run().Run()으로 IO 실행 및 결과 추출");

        MenuHelper.PrintSuccess("실습 08 완료!");
    }
}
