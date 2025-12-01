using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter05_Prelude;

/// <summary>
/// 커링(Currying)과 부분 적용(Partial Application)을 학습합니다.
///
/// 학습 목표:
/// - 커링의 개념 이해
/// - curry() 함수 사용법
/// - 부분 적용(par) 함수 사용법
/// - 커링 vs 부분 적용의 차이
///
/// 핵심 개념:
/// 커링(Currying): f(a, b, c) → f(a)(b)(c)
/// - 여러 인자를 받는 함수를 단일 인자 함수들의 체인으로 변환
///
/// 부분 적용(Partial Application): f(a, b, c) → f(a, _, c) → g(b)
/// - 일부 인자를 미리 고정하고 나머지를 받는 새 함수 생성
/// </summary>
public static class E01_CurryingPartial
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 05-E01: 커링과 부분 적용");

        // ============================================================
        // 1. 커링(Currying) 기본
        // ============================================================
        MenuHelper.PrintSubHeader("1. 커링(Currying) 기본");

        MenuHelper.PrintExplanation("커링은 여러 인자를 받는 함수를 단일 인자 함수의 연쇄로 변환합니다.");
        MenuHelper.PrintExplanation("f(a, b) → f(a)(b)");
        MenuHelper.PrintBlankLines();

        // 일반 함수
        Func<int, int, int> add = (a, b) => a + b;
        MenuHelper.PrintCode("Func<int, int, int> add = (a, b) => a + b;");
        MenuHelper.PrintResult("add(3, 5)", add(3, 5));
        MenuHelper.PrintBlankLines();

        // 커링된 함수
        MenuHelper.PrintCode("var curriedAdd = curry(add);");
        var curriedAdd = curry(add);

        // 커링된 함수 호출
        MenuHelper.PrintCode("curriedAdd(3)(5)");
        MenuHelper.PrintResult("curriedAdd(3)(5)", curriedAdd(3)(5));

        // 중간 함수 저장
        MenuHelper.PrintCode("var add3 = curriedAdd(3);  // 3을 더하는 함수");
        var add3 = curriedAdd(3);
        MenuHelper.PrintResult("add3(5)", add3(5));
        MenuHelper.PrintResult("add3(10)", add3(10));

        // ============================================================
        // 2. 3인자 커링
        // ============================================================
        MenuHelper.PrintSubHeader("2. 3인자 커링");

        MenuHelper.PrintExplanation("3개 이상의 인자도 커링할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        Func<int, int, int, int> multiply3 = (a, b, c) => a * b * c;
        var curriedMul = curry(multiply3);

        MenuHelper.PrintCode("var curriedMul = curry((a, b, c) => a * b * c);");
        MenuHelper.PrintResult("curriedMul(2)(3)(4)", curriedMul(2)(3)(4));

        // 단계별 적용
        var mulBy2 = curriedMul(2);
        var mulBy2And3 = mulBy2(3);
        MenuHelper.PrintResult("mulBy2(3)(4)", mulBy2(3)(4));
        MenuHelper.PrintResult("mulBy2And3(4)", mulBy2And3(4));

        // ============================================================
        // 3. 부분 적용(Partial Application)
        // ============================================================
        MenuHelper.PrintSubHeader("3. 부분 적용(Partial Application)");

        MenuHelper.PrintExplanation("부분 적용은 일부 인자를 미리 고정합니다.");
        MenuHelper.PrintExplanation("par() 함수를 사용합니다.");
        MenuHelper.PrintBlankLines();

        Func<string, string, string> greet = (greeting, name) => $"{greeting}, {name}!";

        // 첫 번째 인자 고정
        MenuHelper.PrintCode("var sayHello = par(greet, \"안녕하세요\");");
        var sayHello = par(greet, "안녕하세요");
        MenuHelper.PrintResult("sayHello(\"철수\")", sayHello("철수"));
        MenuHelper.PrintResult("sayHello(\"영희\")", sayHello("영희"));
        MenuHelper.PrintBlankLines();

        // 다른 인사말 고정
        MenuHelper.PrintCode("var sayGoodbye = par(greet, \"안녕히 가세요\");");
        var sayGoodbye = par(greet, "안녕히 가세요");
        MenuHelper.PrintResult("sayGoodbye(\"철수\")", sayGoodbye("철수"));

        // ============================================================
        // 4. 커링 vs 부분 적용 비교
        // ============================================================
        MenuHelper.PrintSubHeader("4. 커링 vs 부분 적용 비교");

        MenuHelper.PrintExplanation("커링: 모든 인자를 하나씩 받는 함수 체인");
        MenuHelper.PrintExplanation("부분 적용: 일부 인자만 고정, 나머지는 한번에");
        MenuHelper.PrintBlankLines();

        Func<int, int, int, int> calculate = (a, b, c) => (a + b) * c;

        // 커링
        MenuHelper.PrintCode("// 커링: calculate(1)(2)(3)");
        var curriedCalc = curry(calculate);
        MenuHelper.PrintResult("curry(calculate)(1)(2)(3)", curriedCalc(1)(2)(3));

        // 부분 적용 (첫 인자 고정)
        MenuHelper.PrintCode("// 부분 적용: 첫 인자만 고정");
        var calcWith1 = par(calculate, 1);
        MenuHelper.PrintResult("par(calculate, 1)(2, 3)", calcWith1(2, 3));

        // ============================================================
        // 5. 실전 예제: 로깅 함수
        // ============================================================
        MenuHelper.PrintSubHeader("5. 실전 예제: 로깅 함수");

        Func<string, string, string, string> log =
            (level, component, message) => $"[{level}] [{component}] {message}";

        // 레벨별 로거 생성
        var logError = par(log, "ERROR");
        var logInfo = par(log, "INFO");
        var logDebug = par(log, "DEBUG");

        // 컴포넌트별 로거 생성
        var authErrorLog = par(par(log, "ERROR"), "Auth");
        var dbInfoLog = par(par(log, "INFO"), "Database");

        MenuHelper.PrintCode("// 레벨과 컴포넌트를 미리 고정");
        Console.WriteLine($"  {logError("Auth", "로그인 실패")}");
        Console.WriteLine($"  {logInfo("Database", "연결 성공")}");
        Console.WriteLine($"  {authErrorLog("비밀번호 불일치")}");
        Console.WriteLine($"  {dbInfoLog("쿼리 완료")}");

        // ============================================================
        // 6. 커링과 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("6. 커링과 파이프라인");

        MenuHelper.PrintExplanation("커링된 함수는 파이프라인에서 유용합니다.");
        MenuHelper.PrintBlankLines();

        var numbers = List(1, 2, 3, 4, 5);

        // 커링된 곱셈 함수
        var multiply = curry((int a, int b) => a * b);

        // 파이프라인에서 사용
        var doubled = numbers.Map(multiply(2));
        var tripled = numbers.Map(multiply(3));

        MenuHelper.PrintResult("원본", numbers);
        MenuHelper.PrintResult("x2", doubled);
        MenuHelper.PrintResult("x3", tripled);

        // ============================================================
        // 7. flip - 인자 순서 바꾸기
        // ============================================================
        MenuHelper.PrintSubHeader("7. flip - 인자 순서 바꾸기");

        MenuHelper.PrintExplanation("flip은 함수의 인자 순서를 바꿉니다.");
        MenuHelper.PrintExplanation("부분 적용과 함께 사용하면 유용합니다.");
        MenuHelper.PrintBlankLines();

        Func<int, int, int> subtract = (a, b) => a - b;

        MenuHelper.PrintCode("subtract(10, 3) // 10 - 3 = 7");
        MenuHelper.PrintResult("subtract(10, 3)", subtract(10, 3));

        // flip으로 순서 변경
        var flippedSubtract = flip(subtract);
        MenuHelper.PrintCode("flippedSubtract(10, 3) // 3 - 10 = -7");
        MenuHelper.PrintResult("flippedSubtract(10, 3)", flippedSubtract(10, 3));

        // flip + curry + 부분 적용
        MenuHelper.PrintCode("// \"10을 빼는 함수\" 만들기");
        var subtractFrom = curry(flip(subtract));
        var subtract10 = subtractFrom(10);
        MenuHelper.PrintResult("subtract10(25)", subtract10(25));  // 25 - 10 = 15

        // ============================================================
        // 8. apply - 함수 적용
        // ============================================================
        MenuHelper.PrintSubHeader("8. apply - 함수 적용");

        MenuHelper.PrintExplanation("apply는 함수에 인자를 적용합니다.");
        MenuHelper.PrintExplanation("파이프라인 스타일 코드에서 유용합니다.");
        MenuHelper.PrintBlankLines();

        var square = fun((int x) => x * x);

        MenuHelper.PrintCode("var result = square(5);");
        var applyResult = square(5);
        MenuHelper.PrintResult("square(5)", applyResult);

        MenuHelper.PrintSuccess("커링과 부분 적용 학습 완료!");
    }
}
