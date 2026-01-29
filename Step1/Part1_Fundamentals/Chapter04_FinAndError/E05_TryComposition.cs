using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// Try의 합성(composition) 기능을 학습합니다.
/// Map, Bind, LINQ, Match, Catch, | 연산자 등
/// </summary>
public static class E05_TryComposition
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 04-E05: Try 합성");

        // ============================================================
        // 1. Map으로 값 변환
        // ============================================================
        MenuHelper.PrintSubHeader("1. Map으로 값 변환");

        MenuHelper.PrintExplanation("Map은 성공 값에 함수를 적용합니다.");
        MenuHelper.PrintExplanation("실패인 경우 함수가 적용되지 않고 실패가 전파됩니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var tryNum = Try.Succ(10);
var mapped = tryNum.Map(x => x * 2).Map(x => $""결과: {x}"");");

        var tryNum = Try.Succ(10);
        var mapped = tryNum.Map(x => x * 2).Map(x => $"결과: {x}");
        MenuHelper.PrintResult("mapped.Run()", mapped.Run());

        // 실패 케이스
        MenuHelper.PrintCode(@"var tryFail = Try.lift(() => int.Parse(""abc""));
var mappedFail = tryFail.Map(x => x * 2);");

        var tryFail = Try.lift(() => int.Parse("abc"));
        var mappedFail = tryFail.Map(x => x * 2);
        MenuHelper.PrintResult("mappedFail.Run()", mappedFail.Run());

        // ============================================================
        // 2. Bind로 Try 체이닝
        // ============================================================
        MenuHelper.PrintSubHeader("2. Bind로 Try 체이닝");

        MenuHelper.PrintExplanation("Bind는 A -> Try<B> 함수를 적용하여 Try<B>를 반환합니다.");
        MenuHelper.PrintExplanation("Try를 반환하는 함수들을 체이닝할 때 사용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"Try<int> ParseInt(string s) => Try.lift(() => int.Parse(s));
Try<int> Divide(int a, int b) => Try.lift(() => a / b);

var result = ParseInt(""20"")
    .Bind(x => ParseInt(""5"").Map(y => (x, y)))
    .Bind(t => Divide(t.x, t.y));");

        var result = ParseInt("20")
            .Bind(x => ParseInt("5").Map(y => (x, y)))
            .Bind(t => Divide(t.x, t.y));

        MenuHelper.PrintResult("result.Run()", result.Run());

        // 실패 전파
        MenuHelper.PrintCode(@"var failResult = ParseInt(""abc"")
    .Bind(x => ParseInt(""5"").Map(y => (x, y)))
    .Bind(t => Divide(t.x, t.y));");

        var failResult = ParseInt("abc")
            .Bind(x => ParseInt("5").Map(y => (x, y)))
            .Bind(t => Divide(t.x, t.y));

        MenuHelper.PrintResult("failResult.Run()", failResult.Run());

        // ============================================================
        // 3. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("3. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("LINQ 구문으로 더 읽기 쉬운 Try 체이닝이 가능합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var linqResult =
    from x in Try.lift(() => int.Parse(""10""))
    from y in Try.lift(() => int.Parse(""20""))
    select x + y;");

        var linqResult =
            from x in Try.lift(() => int.Parse("10"))
            from y in Try.lift(() => int.Parse("20"))
            select x + y;

        MenuHelper.PrintResult("linqResult.Run()", linqResult.Run());

        // 실패 케이스
        MenuHelper.PrintCode(@"var linqFail =
    from x in Try.lift(() => int.Parse(""10""))
    from y in Try.lift(() => int.Parse(""abc""))  // 실패
    from z in Try.Succ(x + y)
    select z * 2;");

        var linqFail =
            from x in Try.lift(() => int.Parse("10"))
            from y in Try.lift(() => int.Parse("abc"))
            from z in Try.Succ(x + y)
            select z * 2;

        MenuHelper.PrintResult("linqFail.Run()", linqFail.Run());

        // ============================================================
        // 4. Match와 IfFail
        // ============================================================
        MenuHelper.PrintSubHeader("4. Match와 IfFail");

        MenuHelper.PrintExplanation("Match: 성공/실패 모두 처리하여 결과 반환");
        MenuHelper.PrintExplanation("IfFail: 실패 시 기본값 반환");
        MenuHelper.PrintBlankLines();

        // Match
        MenuHelper.PrintCode(@"var matchResult = Try.lift(() => int.Parse(""42""))
    .Match(
        Succ: v => $""성공: {v}"",
        Fail: e => $""실패: {e.Message}""
    );");

        var matchResult = Try.lift(() => int.Parse("42"))
            .Match(
                Succ: v => $"성공: {v}",
                Fail: e => $"실패: {e.Message}"
            );
        MenuHelper.PrintResult("matchResult", matchResult);

        var matchFail = Try.lift(() => int.Parse("abc"))
            .Match(
                Succ: v => $"성공: {v}",
                Fail: e => $"실패: {e.Message}"
            );
        MenuHelper.PrintResult("matchFail", matchFail);

        // IfFail
        MenuHelper.PrintCode(@"var ifFailResult = Try.lift(() => int.Parse(""abc""))
    .IfFail(e => 0);  // 실패 시 기본값 0");

        var ifFailResult = Try.lift(() => int.Parse("abc")).IfFail(e => 0);
        MenuHelper.PrintResult("ifFailResult", ifFailResult);

        // ============================================================
        // 5. | 연산자 (Choice/Alternative) - 폴백
        // ============================================================
        MenuHelper.PrintSubHeader("5. | 연산자 (Choice/Alternative) - 폴백");

        MenuHelper.PrintExplanation("| 연산자는 Choice 트레이트의 Choose 메서드를 호출합니다.");
        MenuHelper.PrintExplanation("첫 번째 Try가 실패하면 두 번째 Try를 실행합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Try | Try - 첫 번째 성공 반환
var fallback =
    Try.lift(() => int.Parse(""abc""))    // 실패
    | Try.lift(() => int.Parse(""xyz""))  // 실패
    | Try.Succ(100);                       // 성공");

        var fallback =
            Try.lift(() => int.Parse("abc"))
            | Try.lift(() => int.Parse("xyz"))
            | Try.Succ(100);

        MenuHelper.PrintResult("fallback.Run()", fallback.Run());

        // 첫 번째가 성공하면 나머지는 실행되지 않음
        MenuHelper.PrintCode(@"// 첫 번째 성공 시 나머지는 평가되지 않음 (지연 평가)
var firstSuccess =
    Try.lift(() => { Console.WriteLine(""첫 번째""); return 42; })
    | Try.lift(() => { Console.WriteLine(""두 번째""); return 100; });");

        Console.WriteLine("  [실행 결과]");
        var firstSuccess =
            Try.lift(() =>
            {
                Console.WriteLine("    첫 번째 시도");
                return 42;
            })
            | Try.lift(() =>
            {
                Console.WriteLine("    두 번째 시도 (실행 안됨)");
                return 100;
            });

        MenuHelper.PrintResult("firstSuccess.Run()", firstSuccess.Run());

        // ============================================================
        // 6. | 연산자와 Error/Exception
        // ============================================================
        MenuHelper.PrintSubHeader("6. | 연산자와 Error/Exception");

        MenuHelper.PrintExplanation("| 연산자는 Error, Exception과도 함께 사용할 수 있습니다.");
        MenuHelper.PrintExplanation("실패 시 특정 에러로 대체하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        // | Error
        MenuHelper.PrintCode(@"// Try | Error - 실패 시 다른 에러로 대체
var withError = Try.lift(() => int.Parse(""abc""))
    | Error.New(""파싱 실패: 기본값 사용 불가"");");

        var withError = Try.lift(() => int.Parse("abc"))
            | Error.New("파싱 실패: 기본값 사용 불가");

        MenuHelper.PrintResult("withError.Run()", withError.Run());

        // | Pure (성공값으로 대체)
        MenuHelper.PrintCode(@"// Try | Pure<A> - 실패 시 성공값으로 대체
var withPure = Try.lift(() => int.Parse(""abc""))
    | Pure(0);  // Prelude.Pure");

        var withPure = Try.lift(() => int.Parse("abc"))
            | Pure(0);

        MenuHelper.PrintResult("withPure.Run()", withPure.Run());

        // ============================================================
        // 7. Catch - 에러 복구
        // ============================================================
        MenuHelper.PrintSubHeader("7. Catch - 에러 복구");

        MenuHelper.PrintExplanation("Catch는 Fallible 트레이트의 핵심 메서드입니다.");
        MenuHelper.PrintExplanation("에러를 잡아서 새로운 Try로 복구하거나 에러를 변환합니다.");
        MenuHelper.PrintBlankLines();

        // Catch(Func<Error, K<F, A>>)
        MenuHelper.PrintCode(@"// Catch - 에러를 잡아서 새 Try로 복구
var caught = Try.lift(() => int.Parse(""abc""))
    .Catch(e => Try.Succ(0));  // 실패 시 0으로 복구");

        var caught = Try.lift(() => int.Parse("abc"))
            .Catch(e => Try.Succ(0));

        MenuHelper.PrintResult("caught.Run()", caught.Run());

        // Catch(Func<Error, A>)
        MenuHelper.PrintCode(@"// Catch - 에러를 잡아서 값으로 복구
var caughtValue = Try.lift(() => int.Parse(""abc""))
    .Catch(e => -1);  // 실패 시 -1 반환");

        var caughtValue = Try.lift(() => int.Parse("abc"))
            .Catch(e => -1);

        MenuHelper.PrintResult("caughtValue.Run()", caughtValue.Run());

        // Catch(Func<Error, Error>)
        MenuHelper.PrintCode(@"// Catch - 에러를 다른 에러로 변환
var caughtError = Try.lift(() => int.Parse(""abc""))
    .Catch(e => Error.New($""변환된 에러: {e.Message}""));");

        var caughtError = Try.lift(() => int.Parse("abc"))
            .Catch(e => Error.New($"변환된 에러: {e.Message}"));

        MenuHelper.PrintResult("caughtError.Run()", caughtError.Run());

        // ============================================================
        // 8. Catch with Predicate - 조건부 에러 처리
        // ============================================================
        MenuHelper.PrintSubHeader("8. Catch with Predicate - 조건부 에러 처리");

        MenuHelper.PrintExplanation("특정 조건을 만족하는 에러만 선택적으로 처리할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // Catch(Func<Error, bool>, Func<Error, A>)
        MenuHelper.PrintCode(@"// 조건부 Catch - 특정 메시지 포함 시에만 처리
var conditionalCatch = Try.lift(() => int.Parse(""abc""))
    .Catch(
        e => e.Message.Contains(""Format""),  // 조건
        e => 0                                  // 복구 값
    );");

        var conditionalCatch = Try.lift(() => int.Parse("abc"))
            .Catch(
                e => e.Message.Contains("Format"),
                e => 0
            );

        MenuHelper.PrintResult("conditionalCatch.Run()", conditionalCatch.Run());

        // 조건 불만족 시
        MenuHelper.PrintCode(@"// 조건 불만족 시 - 원래 에러 유지
var notMatched = Try.lift(() => int.Parse(""abc""))
    .Catch(
        e => e.Message.Contains(""NotFound""),  // 매칭 안됨
        e => 0
    );");

        var notMatched = Try.lift(() => int.Parse("abc"))
            .Catch(
                e => e.Message.Contains("NotFound"),
                e => 0
            );

        MenuHelper.PrintResult("notMatched.Run()", notMatched.Run());

        // ============================================================
        // 9. @catch Prelude 함수
        // ============================================================
        MenuHelper.PrintSubHeader("9. @catch Prelude 함수");

        MenuHelper.PrintExplanation("Prelude의 @catch 함수로 CatchM 구조체를 생성합니다.");
        MenuHelper.PrintExplanation("| 연산자와 함께 사용하여 파이프라인에서 에러를 처리합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// @catch - CatchM 생성 후 | 연산자로 적용
var withCatch = Try.lift(() => int.Parse(""abc""))
    | @catch<Try, int>(e => Try.Succ(0));");

        var withCatch = Try.lift(() => int.Parse("abc"))
            | @catch<Try, int>(e => Try.Succ(0));

        MenuHelper.PrintResult("withCatch.Run()", withCatch.Run());

        // @catch with predicate
        MenuHelper.PrintCode(@"// @catch with predicate
var withPredCatch = Try.lift(() => int.Parse(""abc""))
    | @catch<Try, int>(
        e => e.Message.Contains(""Format""),
        e => Try.Succ(-1)
    );");

        var withPredCatch = Try.lift(() => int.Parse("abc"))
            | @catch<Try, int>(
                e => e.Message.Contains("Format"),
                e => Try.Succ(-1)
            );

        MenuHelper.PrintResult("withPredCatch.Run()", withPredCatch.Run());

        // ============================================================
        // 10. BiBind - 양방향 바인딩
        // ============================================================
        MenuHelper.PrintSubHeader("10. BiBind - 양방향 바인딩");

        MenuHelper.PrintExplanation("BiBind로 성공/실패 모두 다른 Try로 변환할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var bibound = Try.lift(() => int.Parse(""abc""))
    .BiBind(
        Succ: v => Try.Succ(v * 2),
        Fail: e => Try.Succ(0)  // 실패 시 0으로 복구
    );");

        var bibound = Try.lift(() => int.Parse("abc"))
            .BiBind(
                Succ: v => Try.Succ(v * 2),
                Fail: e => Try.Succ(0)
            );

        MenuHelper.PrintResult("bibound.Run()", bibound.Run());

        // 성공 케이스
        var biboundSucc = Try.lift(() => int.Parse("21"))
            .BiBind(
                Succ: v => Try.Succ(v * 2),
                Fail: e => Try.Succ(0)
            );

        MenuHelper.PrintResult("biboundSucc.Run()", biboundSucc.Run());

        MenuHelper.PrintSuccess("Try 합성 학습 완료!");
    }

    // 헬퍼 함수들
    private static Try<int> ParseInt(string s) => Try.lift(() => int.Parse(s));
    private static Try<int> Divide(int a, int b) => Try.lift(() => a / b);
}
