using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter11_Transformers;

/// <summary>
/// EitherT 모나드 트랜스포머를 학습합니다.
///
/// 학습 목표:
/// - EitherT&lt;L, M, R&gt;의 구조와 사용법
/// - Left/Right 양방향 처리
/// - IO + Either 조합 (EitherT&lt;string, IO, A&gt;)
/// - LINQ를 통한 합성
///
/// 핵심 개념:
/// EitherT는 두 가지 가능한 값(Left 또는 Right)을 다른 모나드와 결합합니다.
/// OptionT가 "있음/없음"을 표현한다면, EitherT는 "왼쪽 값/오른쪽 값"을 표현합니다.
/// 일반적으로 Right는 성공, Left는 실패(에러 메시지)로 사용합니다.
///
/// 구조: EitherT&lt;L, M, R&gt; = M&lt;Either&lt;L, R&gt;&gt;
/// 예시: EitherT&lt;string, IO, int&gt; = IO&lt;Either&lt;string, int&gt;&gt;
/// </summary>
public static class E02_EitherT
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 11-E02: EitherT 모나드 트랜스포머");

        // ============================================================
        // 1. EitherT 개념 - OptionT와 비교
        // ============================================================
        MenuHelper.PrintSubHeader("1. EitherT 개념 - OptionT와 비교");

        MenuHelper.PrintExplanation("OptionT vs EitherT 비교:");
        MenuHelper.PrintExplanation("  - OptionT<M, A>: 값이 있거나(Some) 없음(None)");
        MenuHelper.PrintExplanation("  - EitherT<L, M, R>: 왼쪽 값(Left) 또는 오른쪽 값(Right)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("EitherT의 장점:");
        MenuHelper.PrintExplanation("  - 실패 시 에러 정보를 포함할 수 있음");
        MenuHelper.PrintExplanation("  - 두 가지 다른 타입의 결과를 표현");
        MenuHelper.PrintExplanation("  - 관례: Right = 성공, Left = 실패");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// OptionT: 값의 유무만 표현
OptionT<IO, int> optionT = OptionT.None<IO, int>();
// 실패 이유를 알 수 없음

// EitherT: 실패 시 이유도 함께 표현
EitherT<string, IO, int> eitherT = EitherT.Left<string, IO, int>(""사용자 없음"");
// 왜 실패했는지 알 수 있음");

        // ============================================================
        // 2. EitherT 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. EitherT 생성");

        MenuHelper.PrintExplanation("EitherT.Right<L, M, R>: 성공 값으로 생성");
        MenuHelper.PrintExplanation("EitherT.Left<L, M, R>: 실패 값으로 생성");
        MenuHelper.PrintExplanation("EitherT.lift: 기존 모나드나 Either를 트랜스포머로 리프팅");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Right 생성 (성공)
var right = EitherT.Right<string, IO, int>(42);

// Left 생성 (실패)
var left = EitherT.Left<string, IO, int>(""에러 발생"");

// Either를 리프팅
var liftedEither = EitherT.lift<string, IO, int>(Right<string, int>(100));

// IO를 리프팅 (Right로 감싸짐)
var liftedIO = EitherT.liftIO<string, int>(IO.pure(200));");

        // Right 생성
        var rightT = EitherT.Right<string, IO, int>(42);
        var resultRight = rightT.Run().Run();
        MenuHelper.PrintResult("EitherT.Right<string, IO, int>(42).Run().Run()", resultRight);

        // Left 생성
        var leftT = EitherT.Left<string, IO, int>("에러 발생");
        var resultLeft = leftT.Run().Run();
        MenuHelper.PrintResult("EitherT.Left<string, IO, int>(\"에러 발생\").Run().Run()", resultLeft);

        // Either 리프팅
        var liftedRight = EitherT.lift<string, IO, int>(Right<string, int>(100));
        MenuHelper.PrintResult("lift(Right(100)).Run().Run()", liftedRight.Run().Run());

        var liftedLeft = EitherT.lift<string, IO, int>(Left<string, int>("리프트된 에러"));
        MenuHelper.PrintResult("lift(Left(\"리프트된 에러\")).Run().Run()", liftedLeft.Run().Run());

        // ============================================================
        // 3. Map과 Bind
        // ============================================================
        MenuHelper.PrintSubHeader("3. Map과 Bind");

        MenuHelper.PrintExplanation("Map: Right 값만 변환 (Left는 그대로 통과)");
        MenuHelper.PrintExplanation("Bind: EitherT를 반환하는 함수와 연결");
        MenuHelper.PrintExplanation("MapLeft: Left 값을 변환");
        MenuHelper.PrintBlankLines();

        // Map
        var mapped = EitherT.Right<string, IO, int>(10)
            .Map(x => x * 2)
            .Map(x => $"결과: {x}");

        MenuHelper.PrintResult("Right(10).Map(*2).Map(문자열)", mapped.Run().Run());

        // Left에 Map (변환 안됨)
        var mappedLeft = EitherT.Left<string, IO, int>("원본 에러")
            .Map(x => x * 2);
        MenuHelper.PrintResult("Left(\"원본 에러\").Map(*2)", mappedLeft.Run().Run());

        // MapLeft
        var mappedLeftValue = EitherT.Left<string, IO, int>("에러")
            .MapLeft(err => $"[ERROR] {err}");
        MenuHelper.PrintResult("Left(\"에러\").MapLeft(포맷팅)", mappedLeftValue.Run().Run());

        // Bind
        EitherT<string, IO, int> SafeDivide(int a, int b) =>
            b == 0
                ? EitherT.Left<string, IO, int>("0으로 나눌 수 없습니다")
                : EitherT.Right<string, IO, int>(a / b);

        var bound = EitherT.Right<string, IO, int>(100)
            .Bind(x => SafeDivide(x, 5));
        MenuHelper.PrintResult("Right(100).Bind(SafeDivide(_, 5))", bound.Run().Run());

        var boundFail = EitherT.Right<string, IO, int>(100)
            .Bind(x => SafeDivide(x, 0));
        MenuHelper.PrintResult("Right(100).Bind(SafeDivide(_, 0))", boundFail.Run().Run());

        // ============================================================
        // 4. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("4. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("EitherT는 LINQ 쿼리 구문을 완벽 지원합니다.");
        MenuHelper.PrintExplanation("IO와 Either 효과를 하나의 쿼리로 조합합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// LINQ로 EitherT 조합
var query =
    from a in EitherT.Right<string, IO, int>(10)
    from b in EitherT.Right<string, IO, int>(20)
    from c in SafeDivide(a + b, 3)
    select $""{a} + {b} = {a + b}, / 3 = {c}"";");

        var query =
            from a in EitherT.Right<string, IO, int>(10)
            from b in EitherT.Right<string, IO, int>(20)
            from c in SafeDivide(a + b, 3)
            select $"{a} + {b} = {a + b}, / 3 = {c}";

        MenuHelper.PrintResult("LINQ 쿼리 결과", query.Run().Run());

        // 중간에 Left가 있으면
        var queryWithLeft =
            from a in EitherT.Right<string, IO, int>(10)
            from b in EitherT.Left<string, IO, int>("중간에 실패!")
            from c in EitherT.Right<string, IO, int>(30)
            select a + b + c;

        MenuHelper.PrintResult("중간에 Left", queryWithLeft.Run().Run());

        // ============================================================
        // 5. Match와 IfLeft/IfRight
        // ============================================================
        MenuHelper.PrintSubHeader("5. Match와 IfLeft/IfRight");

        MenuHelper.PrintExplanation("Match: Left/Right 케이스 처리");
        MenuHelper.PrintExplanation("IfLeft: Left일 때 기본값 제공");
        MenuHelper.PrintExplanation("IfRight: Right일 때 값 추출");
        MenuHelper.PrintBlankLines();

        // Match
        var rightValue = EitherT.Right<string, IO, string>("Hello");
        var matchResult = rightValue.Match(
            Right: s => $"성공: {s}",
            Left: err => $"실패: {err}"
        );
        MenuHelper.PrintResult("Right(\"Hello\").Match", matchResult.Run());

        var leftValue = EitherT.Left<string, IO, string>("에러 발생");
        var matchLeft = leftValue.Match(
            Right: s => $"성공: {s}",
            Left: err => $"실패: {err}"
        );
        MenuHelper.PrintResult("Left(\"에러 발생\").Match", matchLeft.Run());

        // IfLeft - Left일 때 기본값
        var withDefault = EitherT.Left<string, IO, int>("에러")
            .IfLeft(0);
        MenuHelper.PrintResult("Left(\"에러\").IfLeft(0)", withDefault.Run());

        var rightWithDefault = EitherT.Right<string, IO, int>(42)
            .IfLeft(0);
        MenuHelper.PrintResult("Right(42).IfLeft(0)", rightWithDefault.Run());

        // ============================================================
        // 6. BiMap과 BiBind
        // ============================================================
        MenuHelper.PrintSubHeader("6. BiMap과 BiBind");

        MenuHelper.PrintExplanation("BiMap: Left와 Right 모두 변환");
        MenuHelper.PrintExplanation("BiBind: Left와 Right 각각에 다른 EitherT 반환");
        MenuHelper.PrintBlankLines();

        // BiMap
        var biMappedRight = EitherT.Right<string, IO, int>(42)
            .BiMap(
                Left: err => $"에러: {err}",
                Right: val => $"성공: {val}");
        MenuHelper.PrintResult("Right(42).BiMap", biMappedRight.Run().Run());

        var biMappedLeft = EitherT.Left<string, IO, int>("원본 에러")
            .BiMap(
                Left: err => $"에러: {err}",
                Right: val => $"성공: {val}");
        MenuHelper.PrintResult("Left(\"원본 에러\").BiMap", biMappedLeft.Run().Run());

        // ============================================================
        // 7. IO + Either 실전 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("7. IO + Either 실전 패턴");

        MenuHelper.PrintExplanation("실전에서 IO와 Either를 조합하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        // 시뮬레이션 함수들
        EitherT<string, IO, User> FindUser(int id) =>
            id switch
            {
                1 => EitherT.Right<string, IO, User>(new User(1, "Alice")),
                2 => EitherT.Right<string, IO, User>(new User(2, "Bob")),
                _ => EitherT.Left<string, IO, User>($"사용자 ID {id}를 찾을 수 없습니다")
            };

        EitherT<string, IO, Order> FindOrder(int userId) =>
            userId switch
            {
                1 => EitherT.Right<string, IO, Order>(new Order(101, userId, 50000)),
                _ => EitherT.Left<string, IO, Order>($"사용자 {userId}의 주문이 없습니다")
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
        MenuHelper.PrintResult("FindUser(999) → FindOrder", orderDetails2.Run().Run());

        // 사용자는 있지만 주문 없음
        var orderDetails3 =
            from user in FindUser(2)
            from order in FindOrder(user.Id)
            select $"{user.Name}의 주문: {order.Amount}원";
        MenuHelper.PrintResult("FindUser(2) → FindOrder (주문 없음)", orderDetails3.Run().Run());

        // 에러 복구
        var recovered =
            from user in FindUser(999)
                .MapLeft(err => "게스트 사용자로 대체")
                | EitherT.Right<string, IO, User>(new User(0, "Guest"))
            from order in FindOrder(user.Id)
                | EitherT.Right<string, IO, Order>(new Order(0, 0, 0))
            select $"{user.Name}의 주문: {order.Amount}원";
        MenuHelper.PrintResult("에러 복구 파이프라인", recovered.Run().Run());

        // ============================================================
        // 8. OptionT 변환
        // ============================================================
        MenuHelper.PrintSubHeader("8. OptionT 변환");

        MenuHelper.PrintExplanation("EitherT를 OptionT로 변환할 수 있습니다.");
        MenuHelper.PrintExplanation("  - Right → Some");
        MenuHelper.PrintExplanation("  - Left → None (에러 정보 손실)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// EitherT → OptionT 변환
var eitherRight = EitherT.Right<string, IO, int>(42);
var optionFromRight = eitherRight.ToOption();  // Some(42)

var eitherLeft = EitherT.Left<string, IO, int>(""에러"");
var optionFromLeft = eitherLeft.ToOption();    // None");

        var eitherToOption = EitherT.Right<string, IO, int>(42).ToOption();
        MenuHelper.PrintResult("Right(42).ToOption().Run().Run()", eitherToOption.Run().Run());

        var leftToOption = EitherT.Left<string, IO, int>("에러").ToOption();
        MenuHelper.PrintResult("Left(\"에러\").ToOption().Run().Run()", leftToOption.Run().Run());

        // 실전 요약
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("=== EitherT 요약 ===");
        Console.WriteLine("    구조: EitherT<L, M, R> = M<Either<L, R>>");
        Console.WriteLine("    목적: M 모나드와 Either 효과를 하나의 LINQ로 조합");
        Console.WriteLine("    생성: EitherT.Right<L, M, R>(value), EitherT.Left<L, M, R>(error)");
        Console.WriteLine("    실행: .Run()으로 내부 모나드 획득 후 M 실행");
        Console.WriteLine("    장점: 실패 시 에러 정보를 포함하여 OptionT보다 풍부한 표현");

        MenuHelper.PrintSuccess("EitherT 학습 완료!");
    }

    // 도메인 모델
    private record User(int Id, string Name);
    private record Order(int Id, int UserId, decimal Amount);
}
