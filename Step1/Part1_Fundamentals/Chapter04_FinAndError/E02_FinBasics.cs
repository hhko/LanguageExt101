using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// Fin 타입의 기본 사용법을 학습합니다.
/// </summary>
public static class E02_FinBasics
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 04-E02: Fin 기본 사용");

        // ============================================================
        // 1. Fin vs Either<Error, A>
        // ============================================================
        MenuHelper.PrintSubHeader("1. Fin vs Either<Error, A>");

        MenuHelper.PrintExplanation("Fin<A>는 Either<Error, A>와 같은 의미입니다.");
        MenuHelper.PrintExplanation("에러 타입이 Error로 고정되어 타입 시그니처가 간결해집니다.");
        MenuHelper.PrintBlankLines();

        // ============================================================
        // 2. Fin 성공 값 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. Fin 성공 값 생성");

        MenuHelper.PrintCode("Fin<int> succ1 = 100;");
        Fin<int> succ1 = 100;
        MenuHelper.PrintResult("암시적 변환", succ1);

        MenuHelper.PrintCode("var succ2 = FinSucc(\"Hello\");");
        var succ2 = FinSucc("Hello");
        MenuHelper.PrintResult("FinSucc(\"Hello\")", succ2);

        // ============================================================
        // 3. Fin 실패 값 생성
        // ============================================================
        MenuHelper.PrintSubHeader("3. Fin 실패 값 생성");

        MenuHelper.PrintCode("var fail = FinFail<int>(Error.New(\"에러 발생\"));");
        var fail = FinFail<int>(Error.New("에러 발생"));
        MenuHelper.PrintResult("FinFail(Error)", fail);

        // ============================================================
        // 4. IsSucc, IsFail 검사
        // ============================================================
        MenuHelper.PrintSubHeader("4. IsSucc, IsFail 검사");

        Fin<int> success = 42;
        Fin<int> failure = Error.New("실패");

        MenuHelper.PrintResult("Fin(42).IsSucc", success.IsSucc);
        MenuHelper.PrintResult("Fin(42).IsFail", success.IsFail);
        MenuHelper.PrintResult("Fin(Error).IsSucc", failure.IsSucc);
        MenuHelper.PrintResult("Fin(Error).IsFail", failure.IsFail);

        // ============================================================
        // 5. Match로 Fin 처리
        // ============================================================
        MenuHelper.PrintSubHeader("5. Match로 Fin 처리");

        var result1 = success.Match(
            Succ: v => $"성공: {v}",
            Fail: e => $"실패: {e.Message}"
        );

        var result2 = failure.Match(
            Succ: v => $"성공: {v}",
            Fail: e => $"실패: {e.Message}"
        );

        MenuHelper.PrintResult("success.Match", result1);
        MenuHelper.PrintResult("failure.Match", result2);

        // ============================================================
        // 6. Map과 Bind
        // ============================================================
        MenuHelper.PrintSubHeader("6. Map과 Bind");

        var mapped = FinSucc(10)
            .Map(x => x * 2)
            .Map(x => $"결과: {x}");

        MenuHelper.PrintResult("FinSucc(10).Map(*2).Map(문자열)", mapped);

        // 실패 전파
        var failChain = FinFail<int>(Error.New("초기 에러"))
            .Map(x => x * 2);

        MenuHelper.PrintResult("FinFail.Map", failChain);

        // ============================================================
        // 7. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("7. LINQ 쿼리 구문");

        var linqResult =
            from a in ParseInt("10")
            from b in ParseInt("20")
            from c in SafeDivide(b, a)
            select $"{b} / {a} = {c}";

        var linqFail =
            from a in ParseInt("abc")
            from b in ParseInt("20")
            select a + b;

        MenuHelper.PrintResult("LINQ 성공", linqResult);
        MenuHelper.PrintResult("LINQ 실패", linqFail);

        // ============================================================
        // 8. IfSucc, IfFail
        // ============================================================
        MenuHelper.PrintSubHeader("8. IfSucc, IfFail");

        Fin<string> fin1 = "성공 값";
        Fin<string> fin2 = Error.New("에러");

        MenuHelper.PrintCode("// IfSucc: 성공일 때만 실행");
        fin1.IfSucc(v => Console.WriteLine($"  IfSucc: {v}"));
        fin2.IfSucc(v => Console.WriteLine($"  IfSucc: {v}"));

        MenuHelper.PrintCode("// IfFail: 실패일 때만 실행");
        fin1.IfFail(e => Console.WriteLine($"  IfFail: {e.Message}"));
        fin2.IfFail(e => Console.WriteLine($"  IfFail: {e.Message}"));

        MenuHelper.PrintSuccess("Fin 기본 사용법 학습 완료!");
    }

    private static Fin<int> ParseInt(string s) =>
        int.TryParse(s, out var i) ? FinSucc(i) : FinFail<int>(Error.New($"'{s}'는 정수가 아닙니다"));

    private static Fin<int> SafeDivide(int a, int b) =>
        b == 0 ? FinFail<int>(Error.New("0으로 나눌 수 없습니다")) : FinSucc(a / b);
}
