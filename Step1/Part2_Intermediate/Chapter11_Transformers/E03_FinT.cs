using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter11_Transformers;

/// <summary>
/// FinT 모나드 트랜스포머를 학습합니다.
///
/// 학습 목표:
/// - FinT&lt;M, A&gt;의 구조와 사용법
/// - Error 타입을 통한 풍부한 에러 정보
/// - EitherT와의 차이점 (Error 고정 vs 제네릭 L)
/// - IO + Fin 조합
///
/// 핵심 개념:
/// FinT는 Fin&lt;A&gt; = Either&lt;Error, A&gt;를 다른 모나드와 결합합니다.
/// EitherT&lt;L, M, R&gt;에서 L이 Error로 고정된 특수한 경우입니다.
/// Error 타입은 코드, 메시지, 내부 예외 등 풍부한 에러 정보를 제공합니다.
///
/// 구조: FinT&lt;M, A&gt; = M&lt;Fin&lt;A&gt;&gt; = M&lt;Either&lt;Error, A&gt;&gt;
/// 예시: FinT&lt;IO, int&gt; = IO&lt;Fin&lt;int&gt;&gt;
/// </summary>
public static class E03_FinT
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 11-E03: FinT 모나드 트랜스포머");

        // ============================================================
        // 1. FinT 개념 - EitherT와 비교
        // ============================================================
        MenuHelper.PrintSubHeader("1. FinT 개념 - EitherT와 비교");

        MenuHelper.PrintExplanation("EitherT vs FinT 비교:");
        MenuHelper.PrintExplanation("  - EitherT<L, M, R>: L 타입이 자유 (개발자 정의)");
        MenuHelper.PrintExplanation("  - FinT<M, A>: 실패 타입이 Error로 고정");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("FinT의 장점:");
        MenuHelper.PrintExplanation("  - Error 타입의 풍부한 정보 (코드, 메시지, 스택트레이스)");
        MenuHelper.PrintExplanation("  - 표준화된 에러 처리 패턴");
        MenuHelper.PrintExplanation("  - Eff와 자연스러운 조합");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// EitherT: 에러 타입을 직접 정의
EitherT<string, IO, int> eitherT;      // 에러가 string
EitherT<MyError, IO, int> eitherT2;    // 에러가 MyError

// FinT: 에러 타입이 Error로 고정
FinT<IO, int> finT;  // 에러가 항상 Error 타입
// Error는 코드, 메시지, Inner 등 풍부한 정보 포함");

        // ============================================================
        // 2. FinT 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. FinT 생성");

        MenuHelper.PrintExplanation("FinT.Succ<M, A>: 성공 값으로 생성");
        MenuHelper.PrintExplanation("FinT.Fail<M, A>: Error로 실패 생성");
        MenuHelper.PrintExplanation("FinT.lift: 기존 모나드나 Fin을 트랜스포머로 리프팅");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Succ 생성 (성공)
var succ = FinT.Succ<IO, int>(42);

// Fail 생성 (실패)
var fail = FinT.Fail<IO, int>(Error.New(""에러 발생""));

// 에러 코드와 함께 생성
var failWithCode = FinT.Fail<IO, int>(Error.New(404, ""리소스를 찾을 수 없습니다""));

// Fin을 리프팅 (Pure/Fail로 Fin 생성)
Fin<int> fin = Pure(100);  // 또는 Fail<int>(Error.New(...))
var liftedFin = FinT.lift<IO, int>(fin);");

        // Succ 생성
        var succT = FinT.Succ<IO, int>(42);
        var resultSucc = succT.Run().Run();
        MenuHelper.PrintResult("FinT.Succ<IO, int>(42).Run().Run()", resultSucc);

        // Fail 생성
        var failT = FinT.Fail<IO, int>(Error.New("에러 발생"));
        var resultFail = failT.Run().Run();
        MenuHelper.PrintResult("FinT.Fail<IO, int>(Error.New(\"에러 발생\")).Run().Run()", resultFail);

        // 에러 코드와 함께
        var failWithCode = FinT.Fail<IO, int>(Error.New(404, "리소스를 찾을 수 없습니다"));
        var resultFailCode = failWithCode.Run().Run();
        MenuHelper.PrintResult("Fail(Error.New(404, \"...\")).Run().Run()", resultFailCode);

        // Fin 리프팅
        Fin<int> finSuccValue = Pure(100);
        var liftedSucc = FinT.lift<IO, int>(finSuccValue);
        MenuHelper.PrintResult("lift(Fin Pure(100)).Run().Run()", liftedSucc.Run().Run());

        Fin<int> finFailValue = Error.New("리프트된 에러");
        var liftedFail = FinT.lift<IO, int>(finFailValue);
        MenuHelper.PrintResult("lift(Fin Fail(...)).Run().Run()", liftedFail.Run().Run());

        // ============================================================
        // 3. Map과 Bind
        // ============================================================
        MenuHelper.PrintSubHeader("3. Map과 Bind");

        MenuHelper.PrintExplanation("Map: Succ 값만 변환 (Fail은 그대로 통과)");
        MenuHelper.PrintExplanation("Bind: FinT를 반환하는 함수와 연결");
        MenuHelper.PrintExplanation("MapFail: Error 값을 변환");
        MenuHelper.PrintBlankLines();

        // Map
        var mapped = FinT.Succ<IO, int>(10)
            .Map(x => x * 2)
            .Map(x => $"결과: {x}");

        MenuHelper.PrintResult("Succ(10).Map(*2).Map(문자열)", mapped.Run().Run());

        // Fail에 Map (변환 안됨)
        var mappedFail = FinT.Fail<IO, int>(Error.New("원본 에러"))
            .Map(x => x * 2);
        MenuHelper.PrintResult("Fail(\"원본 에러\").Map(*2)", mappedFail.Run().Run());

        // MapFail
        var mappedFailValue = FinT.Fail<IO, int>(Error.New("에러"))
            .MapFail(err => Error.New(500, $"[CRITICAL] {err.Message}"));
        MenuHelper.PrintResult("Fail.MapFail(코드 추가)", mappedFailValue.Run().Run());

        // Bind
        FinT<IO, int> SafeDivide(int a, int b) =>
            b == 0
                ? FinT.Fail<IO, int>(Error.New("0으로 나눌 수 없습니다"))
                : FinT.Succ<IO, int>(a / b);

        var bound = FinT.Succ<IO, int>(100)
            .Bind(x => SafeDivide(x, 5));
        MenuHelper.PrintResult("Succ(100).Bind(SafeDivide(_, 5))", bound.Run().Run());

        var boundFail = FinT.Succ<IO, int>(100)
            .Bind(x => SafeDivide(x, 0));
        MenuHelper.PrintResult("Succ(100).Bind(SafeDivide(_, 0))", boundFail.Run().Run());

        // ============================================================
        // 4. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("4. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("FinT는 LINQ 쿼리 구문을 완벽 지원합니다.");
        MenuHelper.PrintExplanation("IO와 Fin 효과를 하나의 쿼리로 조합합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// LINQ로 FinT 조합
var query =
    from a in FinT.Succ<IO, int>(10)
    from b in FinT.Succ<IO, int>(20)
    from c in SafeDivide(a + b, 3)
    select $""{a} + {b} = {a + b}, / 3 = {c}"";");

        var query =
            from a in FinT.Succ<IO, int>(10)
            from b in FinT.Succ<IO, int>(20)
            from c in SafeDivide(a + b, 3)
            select $"{a} + {b} = {a + b}, / 3 = {c}";

        MenuHelper.PrintResult("LINQ 쿼리 결과", query.Run().Run());

        // 중간에 Fail이 있으면
        var queryWithFail =
            from a in FinT.Succ<IO, int>(10)
            from b in FinT.Fail<IO, int>(Error.New("중간에 실패!"))
            from c in FinT.Succ<IO, int>(30)
            select a + b + c;

        MenuHelper.PrintResult("중간에 Fail", queryWithFail.Run().Run());

        // ============================================================
        // 5. Match와 IfFail/IfSucc
        // ============================================================
        MenuHelper.PrintSubHeader("5. Match와 IfFail/IfSucc");

        MenuHelper.PrintExplanation("Match: Succ/Fail 케이스 처리");
        MenuHelper.PrintExplanation("IfFail: Fail일 때 기본값 제공");
        MenuHelper.PrintExplanation("IfSucc: Succ일 때 값 추출");
        MenuHelper.PrintBlankLines();

        // Match
        var succValue = FinT.Succ<IO, string>("Hello");
        var matchResult = succValue.Match(
            Succ: s => $"성공: {s}",
            Fail: err => $"실패: {err.Message}"
        );
        MenuHelper.PrintResult("Succ(\"Hello\").Match", matchResult.Run());

        var failValue = FinT.Fail<IO, string>(Error.New("에러 발생"));
        var matchFail = failValue.Match(
            Succ: s => $"성공: {s}",
            Fail: err => $"실패: {err.Message}"
        );
        MenuHelper.PrintResult("Fail(\"에러 발생\").Match", matchFail.Run());

        // IfFail - Fail일 때 기본값
        var withDefault = FinT.Fail<IO, int>(Error.New("에러"))
            .IfFail(0);
        MenuHelper.PrintResult("Fail(\"에러\").IfFail(0)", withDefault.Run());

        var succWithDefault = FinT.Succ<IO, int>(42)
            .IfFail(0);
        MenuHelper.PrintResult("Succ(42).IfFail(0)", succWithDefault.Run());

        // ============================================================
        // 6. BiBind와 BindFail
        // ============================================================
        MenuHelper.PrintSubHeader("6. BiBind와 BindFail");

        MenuHelper.PrintExplanation("BindFail: Fail일 때 복구 시도");
        MenuHelper.PrintExplanation("에러에 따라 다른 복구 전략 적용 가능");
        MenuHelper.PrintBlankLines();

        // BindFail - 에러 복구
        var recovered = FinT.Fail<IO, int>(Error.New(404, "리소스 없음"))
            .BindFail(err => err.Code == 404
                ? FinT.Succ<IO, int>(-1)  // 404는 기본값으로 복구
                : FinT.Fail<IO, int>(err));  // 다른 에러는 그대로
        MenuHelper.PrintResult("Fail(404).BindFail(복구)", recovered.Run().Run());

        var notRecovered = FinT.Fail<IO, int>(Error.New(500, "서버 에러"))
            .BindFail(err => err.Code == 404
                ? FinT.Succ<IO, int>(-1)
                : FinT.Fail<IO, int>(err));
        MenuHelper.PrintResult("Fail(500).BindFail(복구 안됨)", notRecovered.Run().Run());

        // Succ에 BindFail (적용 안됨)
        var succNotRecovered = FinT.Succ<IO, int>(42)
            .BindFail(err => FinT.Succ<IO, int>(-1));
        MenuHelper.PrintResult("Succ(42).BindFail (적용 안됨)", succNotRecovered.Run().Run());

        // ============================================================
        // 7. IO + Fin 실전 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("7. IO + Fin 실전 패턴");

        MenuHelper.PrintExplanation("실전에서 IO와 Fin을 조합하는 패턴입니다.");
        MenuHelper.PrintExplanation("Error 타입을 활용한 구조화된 에러 처리가 가능합니다.");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션 함수들
        FinT<IO, User> FindUser(int id) =>
            id switch
            {
                1 => FinT.Succ<IO, User>(new User(1, "Alice")),
                2 => FinT.Succ<IO, User>(new User(2, "Bob")),
                _ => FinT.Fail<IO, User>(Error.New(404, $"사용자 ID {id}를 찾을 수 없습니다"))
            };

        FinT<IO, Order> FindOrder(int userId) =>
            userId switch
            {
                1 => FinT.Succ<IO, Order>(new Order(101, userId, 50000)),
                _ => FinT.Fail<IO, Order>(Error.New(404, $"사용자 {userId}의 주문이 없습니다"))
            };

        MenuHelper.PrintCode(@"// 사용자와 주문 조회 파이프라인
var orderDetails =
    from user in FindUser(userId)
    from order in FindOrder(user.Id)
    select $""{user.Name}의 주문: {order.Amount}원"";");

        // 성공 케이스
        var orderDetails1 =
            from user in FindUser(1)
            from order in FindOrder(user.Id)
            select $"{user.Name}의 주문: {order.Amount}원";
        MenuHelper.PrintResult("FindUser(1) → FindOrder", orderDetails1.Run().Run());

        // 사용자 없음
        var orderDetails2 =
            from user in FindUser(999)
            from order in FindOrder(user.Id)
            select $"{user.Name}의 주문: {order.Amount}원";
        var result2 = orderDetails2.Run().Run();
        MenuHelper.PrintResult("FindUser(999) → FindOrder", result2);
        result2.IfFail(err => Console.WriteLine($"    에러 코드: {err.Code}"));

        // 에러 복구 파이프라인
        var recovered2 =
            from user in FindUser(999)
                .BindFail(err => err.Code == 404
                    ? FinT.Succ<IO, User>(new User(0, "Guest"))
                    : FinT.Fail<IO, User>(err))
            from order in FindOrder(user.Id)
                .BindFail(_ => FinT.Succ<IO, Order>(new Order(0, 0, 0)))
            select $"{user.Name}의 주문: {order.Amount}원";
        MenuHelper.PrintResult("에러 복구 파이프라인", recovered2.Run().Run());

        // ============================================================
        // 8. 타입 변환 - OptionT/EitherT
        // ============================================================
        MenuHelper.PrintSubHeader("8. 타입 변환 - OptionT/EitherT");

        MenuHelper.PrintExplanation("FinT를 다른 트랜스포머로 변환할 수 있습니다.");
        MenuHelper.PrintExplanation("  - ToOption: Succ → Some, Fail → None (에러 정보 손실)");
        MenuHelper.PrintExplanation("  - ToEither: Fin<A>를 Either<Error, A>로 표현");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// FinT → OptionT 변환
var finSucc = FinT.Succ<IO, int>(42);
var optionFromSucc = finSucc.ToOption();  // Some(42)

var finFail = FinT.Fail<IO, int>(Error.New(""에러""));
var optionFromFail = finFail.ToOption();  // None");

        var finToOption = FinT.Succ<IO, int>(42).ToOption();
        MenuHelper.PrintResult("Succ(42).ToOption().Run().Run()", finToOption.Run().Run());

        var failToOption = FinT.Fail<IO, int>(Error.New("에러")).ToOption();
        MenuHelper.PrintResult("Fail(\"에러\").ToOption().Run().Run()", failToOption.Run().Run());

        // EitherT vs FinT 비교 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== EitherT vs FinT 비교 ===");
        Console.WriteLine("    ┌─────────────────┬───────────────────┬─────────────────┐");
        Console.WriteLine("    │ 특성            │ EitherT<L,M,R>    │ FinT<M,A>       │");
        Console.WriteLine("    ├─────────────────┼───────────────────┼─────────────────┤");
        Console.WriteLine("    │ 실패 타입       │ 제네릭 L (자유)   │ Error (고정)    │");
        Console.WriteLine("    │ 에러 정보       │ 개발자 정의       │ 코드,메시지,스택│");
        Console.WriteLine("    │ 사용 사례       │ 양방향 값 필요    │ 에러 처리 주목적│");
        Console.WriteLine("    └─────────────────┴───────────────────┴─────────────────┘");

        // 실전 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== FinT 요약 ===");
        Console.WriteLine("    구조: FinT<M, A> = M<Fin<A>> = M<Either<Error, A>>");
        Console.WriteLine("    목적: M 모나드와 Error 기반 실패 처리를 하나의 LINQ로 조합");
        Console.WriteLine("    생성: FinT.Succ<M, A>(value), FinT.Fail<M, A>(Error.New(...))");
        Console.WriteLine("    실행: .Run()으로 내부 모나드 획득 후 M 실행");
        Console.WriteLine("    장점: Error 타입의 풍부한 에러 정보와 표준화된 처리");

        MenuHelper.PrintSuccess("FinT 학습 완료!");
    }

    // 도메인 모델
    private record User(int Id, string Name);
    private record Order(int Id, int UserId, decimal Amount);
}
