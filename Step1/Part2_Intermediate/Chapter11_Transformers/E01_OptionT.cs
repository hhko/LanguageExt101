using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter11_Transformers;

/// <summary>
/// OptionT 모나드 트랜스포머를 학습합니다.
///
/// 학습 목표:
/// - 모나드 트랜스포머의 개념 이해
/// - OptionT&lt;M, A&gt;의 구조와 사용법
/// - IO + Option 조합 (OptionT&lt;IO, A&gt;)
/// - Eff + Option 조합 (OptionT&lt;Eff, A&gt;)
/// - LINQ를 통한 합성
///
/// 핵심 개념:
/// 모나드 트랜스포머는 두 모나드의 효과를 결합합니다.
/// OptionT&lt;M, A&gt;는 M 모나드 안에 Option&lt;A&gt;를 감싸서
/// 두 모나드의 효과를 모두 사용할 수 있게 합니다.
///
/// 구조: OptionT&lt;M, A&gt; = M&lt;Option&lt;A&gt;&gt;
/// 예시: OptionT&lt;IO, int&gt; = IO&lt;Option&lt;int&gt;&gt;
/// </summary>
public static class E01_OptionT
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 11-E01: OptionT 모나드 트랜스포머");

        // ============================================================
        // 1. 모나드 트랜스포머란?
        // ============================================================
        MenuHelper.PrintSubHeader("1. 모나드 트랜스포머란?");

        MenuHelper.PrintExplanation("모나드는 직접 합성되지 않습니다:");
        MenuHelper.PrintExplanation("  - IO<Option<A>>를 LINQ로 사용하기 어려움");
        MenuHelper.PrintExplanation("  - 중첩된 모나드를 다루려면 매번 언래핑 필요");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("모나드 트랜스포머가 해결:");
        MenuHelper.PrintExplanation("  - OptionT<M, A> = M 안에 Option을 감싼 새로운 모나드");
        MenuHelper.PrintExplanation("  - 두 모나드의 효과를 하나의 LINQ로 사용 가능");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 트랜스포머 없이 (복잡)
IO<Option<int>> result =
    from optA in readFromDb()        // IO<Option<int>>
    select optA.Bind(a =>            // 중첩 처리 필요
        optB.Map(b => a + b));

// 트랜스포머 사용 (간결)
OptionT<IO, int> result =
    from a in readFromDbT()          // OptionT<IO, int>
    from b in anotherQueryT()        // 자연스러운 합성
    select a + b;");

        // ============================================================
        // 2. OptionT 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. OptionT 생성");

        MenuHelper.PrintExplanation("OptionT.Some<M, A>: 값이 있는 상태로 생성");
        MenuHelper.PrintExplanation("OptionT.None<M, A>: 값이 없는 상태로 생성");
        MenuHelper.PrintExplanation("OptionT.lift: 기존 모나드를 트랜스포머로 리프팅");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Some 생성
var some = OptionT.Some<IO, int>(42);

// None 생성
var none = OptionT.None<IO, string>();

// Option을 리프팅
var lifted = OptionT.lift<IO, int>(Some(100));

// IO<A>를 리프팅 (Option.Some으로 감싸짐)
var liftedIO = OptionT.lift<IO, string>(IO.pure(""hello""));");

        // Some 생성
        var someT = OptionT.Some<IO, int>(42);
        var resultSome = someT.Run().Run();
        MenuHelper.PrintResult("OptionT.Some<IO, int>(42).Run().Run()", resultSome);

        // None 생성
        var noneT = OptionT.None<IO, string>();
        var resultNone = noneT.Run().Run();
        MenuHelper.PrintResult("OptionT.None<IO, string>().Run().Run()", resultNone);

        // Option 리프팅
        var liftedOpt = OptionT.lift<IO, int>(Some(100));
        MenuHelper.PrintResult("lift(Some(100)).Run().Run()", liftedOpt.Run().Run());

        var liftedNone = OptionT.lift<IO, int>(Option<int>.None);
        MenuHelper.PrintResult("lift(None).Run().Run()", liftedNone.Run().Run());

        // ============================================================
        // 3. Map과 Bind
        // ============================================================
        MenuHelper.PrintSubHeader("3. Map과 Bind");

        MenuHelper.PrintExplanation("Map: OptionT 내부 값 변환");
        MenuHelper.PrintExplanation("Bind: OptionT를 반환하는 함수와 연결");
        MenuHelper.PrintBlankLines();

        // Map
        var mapped = OptionT.Some<IO, int>(10)
            .Map(x => x * 2)
            .Map(x => $"결과: {x}");

        MenuHelper.PrintResult("Some(10).Map(*2).Map(문자열)", mapped.Run().Run());

        // None에 Map
        var mappedNone = OptionT.None<IO, int>()
            .Map(x => x * 2);
        MenuHelper.PrintResult("None.Map(*2)", mappedNone.Run().Run());

        // Bind
        OptionT<IO, int> SafeDivide(int a, int b) =>
            b == 0
                ? OptionT.None<IO, int>()
                : OptionT.Some<IO, int>(a / b);

        var bound = OptionT.Some<IO, int>(100)
            .Bind(x => SafeDivide(x, 5));
        MenuHelper.PrintResult("Some(100).Bind(SafeDivide(_, 5))", bound.Run().Run());

        var boundNone = OptionT.Some<IO, int>(100)
            .Bind(x => SafeDivide(x, 0));
        MenuHelper.PrintResult("Some(100).Bind(SafeDivide(_, 0))", boundNone.Run().Run());

        // ============================================================
        // 4. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("4. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("OptionT는 LINQ 쿼리 구문을 완벽 지원합니다.");
        MenuHelper.PrintExplanation("IO와 Option 효과를 하나의 쿼리로 조합합니다.");
        MenuHelper.PrintBlankLines();

        // ---------------------------------------------------------
        // LINQ 타입 결정 규칙 (Chapter 10에서 학습한 내용):
        //
        // 첫 번째 from의 타입이 전체 쿼리 타입을 결정합니다.
        // OptionT<M, A>가 첫 번째에 오면 전체가 OptionT 컨텍스트가 됩니다.
        //
        // OptionT의 SelectMany 오버로드:
        // - OptionT<M, B> → OptionT와 OptionT 조합
        // - K<M, B> → OptionT와 M 조합 (M을 OptionT로 리프팅)
        // - Option<B> → OptionT와 Option 조합
        // ---------------------------------------------------------

        MenuHelper.PrintCode(@"// LINQ로 OptionT 조합
var query =
    from a in OptionT.Some<IO, int>(10)
    from b in OptionT.Some<IO, int>(20)
    from c in SafeDivide(a + b, 3)
    select $""{a} + {b} = {a + b}, / 3 = {c}"";");

        var query =
            from a in OptionT.Some<IO, int>(10)
            from b in OptionT.Some<IO, int>(20)
            from c in SafeDivide(a + b, 3)
            select $"{a} + {b} = {a + b}, / 3 = {c}";

        MenuHelper.PrintResult("LINQ 쿼리 결과", query.Run().Run());

        // 중간에 None이 있으면
        var queryWithNone =
            from a in OptionT.Some<IO, int>(10)
            from b in OptionT.None<IO, int>()  // None!
            from c in OptionT.Some<IO, int>(30)
            select a + b + c;

        MenuHelper.PrintResult("중간에 None", queryWithNone.Run().Run());

        // ============================================================
        // 5. Match와 IfNone
        // ============================================================
        MenuHelper.PrintSubHeader("5. Match와 IfNone");

        MenuHelper.PrintExplanation("Match: Some/None 케이스 처리");
        MenuHelper.PrintExplanation("IfNone: None일 때 기본값 제공");
        MenuHelper.PrintBlankLines();

        // Match
        var someValue = OptionT.Some<IO, string>("Hello");
        var matchResult = someValue.Match(
            Some: s => $"값: {s}",
            None: () => "값 없음"
        );
        MenuHelper.PrintResult("Some(\"Hello\").Match", matchResult.Run());

        var noneValue = OptionT.None<IO, string>();
        var matchNone = noneValue.Match(
            Some: s => $"값: {s}",
            None: () => "값 없음"
        );
        MenuHelper.PrintResult("None.Match", matchNone.Run());

        // IfNone
        var withDefault = OptionT.None<IO, int>()
            .IfNone(0);
        MenuHelper.PrintResult("None.IfNone(0)", withDefault.Run());

        var someWithDefault = OptionT.Some<IO, int>(42)
            .IfNone(0);
        MenuHelper.PrintResult("Some(42).IfNone(0)", someWithDefault.Run());

        // ============================================================
        // 6. IO + Option 실전 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("6. IO + Option 실전 패턴");

        MenuHelper.PrintExplanation("실전에서 IO와 Option을 조합하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션 함수들
        OptionT<IO, User> FindUser(int id) =>
            id switch
            {
                1 => OptionT.Some<IO, User>(new User(1, "Alice")),
                2 => OptionT.Some<IO, User>(new User(2, "Bob")),
                _ => OptionT.None<IO, User>()
            };

        OptionT<IO, Order> FindOrder(int userId) =>
            userId switch
            {
                1 => OptionT.Some<IO, Order>(new Order(101, userId, 50000)),
                _ => OptionT.None<IO, Order>()
            };

        MenuHelper.PrintCode(@"// 사용자와 주문 조회 파이프라인
var orderDetails =
    from user in FindUser(userId)
    from order in FindOrder(user.Id)
    select $""{user.Name}의 주문: {order.Amount}원"";");

        // 유효한 조회
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
        MenuHelper.PrintResult("FindUser(999) → FindOrder", orderDetails2.Run().Run());

        // 사용자는 있지만 주문 없음
        var orderDetails3 =
            from user in FindUser(2)
            from order in FindOrder(user.Id)
            select $"{user.Name}의 주문: {order.Amount}원";
        MenuHelper.PrintResult("FindUser(2) → FindOrder (주문 없음)", orderDetails3.Run().Run());

        // ============================================================
        // 7. BiBind와 BindNone
        // ============================================================
        MenuHelper.PrintSubHeader("7. BiBind와 BindNone");

        MenuHelper.PrintExplanation("BiBind: Some/None 케이스에 대해 다른 OptionT 반환");
        MenuHelper.PrintExplanation("BindNone: None일 때만 대체 OptionT 실행");
        MenuHelper.PrintBlankLines();

        // BindNone - None일 때 대체 값 제공
        var withFallback = FindUser(999)
            .BindNone(() => OptionT.Some<IO, User>(new User(0, "Guest")));
        MenuHelper.PrintResult("FindUser(999).BindNone(Guest)", withFallback.Run().Run());

        var someNotFallback = FindUser(1)
            .BindNone(() => OptionT.Some<IO, User>(new User(0, "Guest")));
        MenuHelper.PrintResult("FindUser(1).BindNone (적용 안됨)", someNotFallback.Run().Run());

        // ============================================================
        // 8. Run과 실행
        // ============================================================
        MenuHelper.PrintSubHeader("8. Run과 실행");

        MenuHelper.PrintExplanation("Run(): OptionT를 내부 모나드로 변환 (K<M, Option<A>>)");
        MenuHelper.PrintExplanation("IO의 경우 .Run()을 한 번 더 호출하여 실행");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// OptionT<IO, A> 실행 과정
OptionT<IO, int> optionT = OptionT.Some<IO, int>(42);

// 1단계: OptionT → K<IO, Option<int>>
K<IO, Option<int>> io = optionT.Run();

// 2단계: IO 실행 → Option<int>
Option<int> result = io.Run();");

        var demoT = OptionT.Some<IO, int>(42);
        var demoIO = demoT.Run();  // K<IO, Option<int>>
        var demoResult = demoIO.Run();  // Option<int>

        MenuHelper.PrintResult("OptionT.Some(42).Run().Run()", demoResult);

        // 실전 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== OptionT 요약 ===");
        Console.WriteLine("    구조: OptionT<M, A> = M<Option<A>>");
        Console.WriteLine("    목적: M 모나드와 Option 효과를 하나의 LINQ로 조합");
        Console.WriteLine("    생성: OptionT.Some<M, A>(value), OptionT.None<M, A>()");
        Console.WriteLine("    실행: .Run()으로 내부 모나드 획득 후 M 실행");
        Console.WriteLine("    장점: 중첩 모나드를 단일 컨텍스트로 처리");

        MenuHelper.PrintSuccess("OptionT 학습 완료!");
    }

    // 도메인 모델
    private record User(int Id, string Name);
    private record Order(int Id, int UserId, decimal Amount);
}
