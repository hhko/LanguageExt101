using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// Try 타입의 기본 사용법을 학습합니다.
/// Try는 예외를 던질 수 있는 계산을 안전하게 래핑합니다.
/// </summary>
public static class E04_TryBasics
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 04-E04: Try 기본");

        // ============================================================
        // 1. Try vs Fin 비교
        // ============================================================
        MenuHelper.PrintSubHeader("1. Try vs Fin 비교");

        MenuHelper.PrintExplanation("Fin<A>는 이미 평가된 결과 (성공 또는 실패)를 담습니다.");
        MenuHelper.PrintExplanation("Try<A>는 Func<Fin<A>>를 래핑합니다 - 지연 평가(lazy evaluation)!");
        MenuHelper.PrintExplanation("Try는 실행 시점까지 예외 발생을 미루고, 예외를 Error로 자동 변환합니다.");
        MenuHelper.PrintBlankLines();

        // Fin: 즉시 평가
        MenuHelper.PrintCode("Fin<int> fin = 42;  // 즉시 평가됨");
        Fin<int> fin = 42;
        MenuHelper.PrintResult("Fin<int>", fin);

        // Try: 지연 평가
        MenuHelper.PrintCode("var @try = Try.Succ(42);  // 아직 실행되지 않음");
        var @try = Try.Succ(42);
        MenuHelper.PrintResult("Try<int> (실행 전)", "@try는 Func<Fin<int>>를 래핑");

        MenuHelper.PrintCode("Fin<int> result = @try.Run();  // 이제 실행");
        Fin<int> result = @try.Run();
        MenuHelper.PrintResult("@try.Run()", result);

        // ============================================================
        // 2. Try 성공/실패 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. Try 성공/실패 생성");

        // 성공 Try
        MenuHelper.PrintCode("var trySucc = Try.Succ(100);");
        var trySucc = Try.Succ(100);
        MenuHelper.PrintResult("Try.Succ(100).Run()", trySucc.Run());

        // Try.lift로 함수 래핑 (예외 자동 캐치)
        MenuHelper.PrintCode("var tryFunc = Try.lift(() => 42 * 2);");
        var tryFunc = Try.lift(() => 42 * 2);
        MenuHelper.PrintResult("Try.lift(() => 42 * 2).Run()", tryFunc.Run());

        // 실패 Try - Try.Fail 사용
        MenuHelper.PrintCode("var tryFail = Try.Fail<int>(Error.New(\"실패\"));");
        var tryFail = Try.Fail<int>(Error.New("실패"));
        MenuHelper.PrintResult("tryFail.Run()", tryFail.Run());

        // Error로부터 암시적 변환
        MenuHelper.PrintCode("Try<int> tryFailImplicit = Error.New(\"암시적 실패\");");
        Try<int> tryFailImplicit = Error.New("암시적 실패");
        MenuHelper.PrintResult("암시적 변환.Run()", tryFailImplicit.Run());

        // ============================================================
        // 3. Try.lift로 예외 던지는 함수 래핑
        // ============================================================
        MenuHelper.PrintSubHeader("3. Try.lift로 예외 던지는 함수 래핑");

        MenuHelper.PrintExplanation("Try.lift는 예외를 던지는 함수를 안전하게 래핑합니다.");
        MenuHelper.PrintExplanation("예외가 발생하면 자동으로 Error로 변환됩니다.");
        MenuHelper.PrintBlankLines();

        // 예외를 던지는 파싱
        MenuHelper.PrintCode("var tryParse = Try.lift(() => int.Parse(\"not a number\"));");
        var tryParse = Try.lift(() => int.Parse("not a number"));
        MenuHelper.PrintResult("tryParse.Run()", tryParse.Run());

        // 성공하는 파싱
        MenuHelper.PrintCode("var tryParseOk = Try.lift(() => int.Parse(\"42\"));");
        var tryParseOk = Try.lift(() => int.Parse("42"));
        MenuHelper.PrintResult("tryParseOk.Run()", tryParseOk.Run());

        // ============================================================
        // 4. 지연 평가 확인
        // ============================================================
        MenuHelper.PrintSubHeader("4. 지연 평가 확인");

        MenuHelper.PrintExplanation("Try는 Run()을 호출할 때까지 실행되지 않습니다.");
        MenuHelper.PrintBlankLines();

        var counter = 0;
        MenuHelper.PrintCode("var lazyTry = Try.lift(() => { counter++; return counter; });");
        var lazyTry = Try.lift(() =>
        {
            counter++;
            Console.WriteLine($"    [실행됨] counter = {counter}");
            return counter;
        });

        MenuHelper.PrintResult("lazyTry 생성 후 counter", counter);

        Console.WriteLine("  첫 번째 Run() 호출:");
        var result1 = lazyTry.Run();
        MenuHelper.PrintResult("첫 번째 Run() 결과", result1);

        Console.WriteLine("  두 번째 Run() 호출:");
        var result2 = lazyTry.Run();
        MenuHelper.PrintResult("두 번째 Run() 결과", result2);

        MenuHelper.PrintExplanation("매 Run() 호출마다 함수가 다시 실행됩니다 (순수하지 않은 예시).");

        // ============================================================
        // 5. 전통적 try-catch vs Try 비교
        // ============================================================
        MenuHelper.PrintSubHeader("5. 전통적 try-catch vs Try 비교");

        MenuHelper.PrintCode(@"// 전통적 방식
try
{
    var num = int.Parse(""abc"");
    Console.WriteLine($""결과: {num}"");
}
catch (Exception ex)
{
    Console.WriteLine($""에러: {ex.Message}"");
}");

        Console.WriteLine("  [전통적 방식 실행]");
        try
        {
            var num = int.Parse("abc");
            Console.WriteLine($"    결과: {num}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    에러: {ex.Message}");
        }

        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Try 방식
var tryResult = Try.lift(() => int.Parse(""abc"")).Run();
var message = tryResult.Match(
    Succ: n => $""결과: {n}"",
    Fail: e => $""에러: {e.Message}""
);");

        Console.WriteLine("  [Try 방식 실행]");
        var tryResult = Try.lift(() => int.Parse("abc")).Run();
        var message = tryResult.Match(
            Succ: n => $"결과: {n}",
            Fail: e => $"에러: {e.Message}"
        );
        Console.WriteLine($"    {message}");

        // ============================================================
        // 6. Try의 장점
        // ============================================================
        MenuHelper.PrintSubHeader("6. Try의 장점");

        MenuHelper.PrintExplanation("1. 합성 가능 (composable): Map, Bind로 체이닝 가능");
        MenuHelper.PrintExplanation("2. 지연 평가: 필요할 때까지 실행을 미룸");
        MenuHelper.PrintExplanation("3. 예외 안전: 예외가 Error로 자동 변환됨");
        MenuHelper.PrintExplanation("4. 타입 안전: 실패 가능성이 타입에 명시됨");

        MenuHelper.PrintSuccess("Try 기본 사용법 학습 완료!");
    }
}
