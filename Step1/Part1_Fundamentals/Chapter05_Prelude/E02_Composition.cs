using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter05_Prelude;

/// <summary>
/// 함수 합성(Composition)을 학습합니다.
///
/// 학습 목표:
/// - 함수 합성의 개념
/// - compose와 then 사용법
/// - 파이프라인 스타일 코드
/// - 합성의 실용적 활용
///
/// 핵심 개념:
/// 함수 합성: (f ∘ g)(x) = f(g(x))
/// - compose: 오른쪽에서 왼쪽으로 적용 (수학적 순서)
/// - then(>>): 왼쪽에서 오른쪽으로 적용 (파이프라인 순서)
///
/// 작은 함수들을 합성하여 복잡한 변환을 만들 수 있습니다.
/// </summary>
public static class E02_Composition
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 05-E02: 함수 합성");

        // ============================================================
        // 1. 함수 합성 기본
        // ============================================================
        MenuHelper.PrintSubHeader("1. 함수 합성 기본");

        MenuHelper.PrintExplanation("함수 합성은 두 함수를 연결하여 새 함수를 만듭니다.");
        MenuHelper.PrintExplanation("(f ∘ g)(x) = f(g(x)) - 먼저 g 적용, 그 다음 f 적용");
        MenuHelper.PrintBlankLines();

        Func<int, int> addOne = x => x + 1;
        Func<int, int> double_ = x => x * 2;

        // compose: 오른쪽에서 왼쪽 (g 먼저, f 나중)
        MenuHelper.PrintCode("var composed = compose(double_, addOne);");
        MenuHelper.PrintCode("// composed(5) = double_(addOne(5)) = double_(6) = 12");
        var composed = compose(double_, addOne);
        MenuHelper.PrintResult("composed(5)", composed(5));

        // ============================================================
        // 2. compose vs then
        // ============================================================
        MenuHelper.PrintSubHeader("2. compose vs then");

        MenuHelper.PrintExplanation("compose: 오른쪽 → 왼쪽 (수학적 순서)");
        MenuHelper.PrintExplanation("then: 왼쪽 → 오른쪽 (실행 순서, 파이프라인)");
        MenuHelper.PrintBlankLines();

        // compose: double_(addOne(x))
        var withCompose = compose(double_, addOne);
        MenuHelper.PrintResult("compose(double, addOne)(5)", withCompose(5));  // (5+1)*2 = 12

        // then: addOne 먼저, 그 다음 double
        // 확장 메서드 스타일 사용
        Func<int, int> pipeline = x => double_(addOne(x));
        MenuHelper.PrintResult("addOne.then(double)(5)", pipeline(5));  // (5+1)*2 = 12

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("실행 순서가 같으면 결과도 같습니다.");
        MenuHelper.PrintExplanation("then은 코드를 읽는 순서와 실행 순서가 일치합니다.");

        // ============================================================
        // 3. 여러 함수 합성
        // ============================================================
        MenuHelper.PrintSubHeader("3. 여러 함수 합성");

        MenuHelper.PrintExplanation("여러 함수를 연속으로 합성할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        Func<int, int> square = x => x * x;
        Func<int, int> negate = x => -x;

        // 5 → +1 → *2 → ^2 → negate
        // 5 → 6 → 12 → 144 → -144
        Func<int, int> complexPipeline = x => negate(square(double_(addOne(x))));

        MenuHelper.PrintCode("// 5 → +1 → *2 → ^2 → negate");
        MenuHelper.PrintCode("// 5 → 6 → 12 → 144 → -144");
        MenuHelper.PrintResult("파이프라인(5)", complexPipeline(5));

        // ============================================================
        // 4. 문자열 처리 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("4. 문자열 처리 파이프라인");

        MenuHelper.PrintExplanation("실용적인 문자열 변환 파이프라인 예제입니다.");
        MenuHelper.PrintBlankLines();

        Func<string, string> trim = s => s.Trim();
        Func<string, string> toLower = s => s.ToLower();
        Func<string, string> removeSpaces = s => s.Replace(" ", "");
        Func<string, string> addPrefix = s => $"user_{s}";

        // 합성된 변환
        Func<string, string> normalizeUsername = s => addPrefix(removeSpaces(toLower(trim(s))));

        var input = "  Hello World  ";
        MenuHelper.PrintCode("// \"  Hello World  \" → trim → toLower → removeSpaces → addPrefix");
        MenuHelper.PrintResult("변환 전", $"\"{input}\"");
        MenuHelper.PrintResult("변환 후", $"\"{normalizeUsername(input)}\"");

        // ============================================================
        // 5. 타입이 다른 함수 합성
        // ============================================================
        MenuHelper.PrintSubHeader("5. 타입이 다른 함수 합성");

        MenuHelper.PrintExplanation("출력 타입과 입력 타입이 일치하면 합성 가능합니다.");
        MenuHelper.PrintExplanation("A → B 와 B → C 를 합성하면 A → C");
        MenuHelper.PrintBlankLines();

        Func<string, int> parseLength = s => s.Length;
        Func<int, bool> isEven = n => n % 2 == 0;
        Func<bool, string> toYesNo = b => b ? "예" : "아니오";

        // string → int → bool → string
        Func<string, string> checkEvenLength = s => toYesNo(isEven(parseLength(s)));

        MenuHelper.PrintCode("// string → length → isEven → toYesNo");
        MenuHelper.PrintResult("\"Hello\" 길이가 짝수?", checkEvenLength("Hello"));
        MenuHelper.PrintResult("\"Hi\" 길이가 짝수?", checkEvenLength("Hi"));

        // ============================================================
        // 6. Option과 함수 합성
        // ============================================================
        MenuHelper.PrintSubHeader("6. Option과 함수 합성");

        MenuHelper.PrintExplanation("Map을 통해 Option 내부 값에 합성된 함수를 적용합니다.");
        MenuHelper.PrintBlankLines();

        var maybeNumber = Some(5);
        Option<int> noNumber = None;

        // 합성된 변환을 Map에 전달
        Func<int, int> transform = x => square(double_(addOne(x)));

        var transformed1 = maybeNumber.Map(transform);
        var transformed2 = noNumber.Map(transform);

        MenuHelper.PrintResult("Some(5) 변환", transformed1);
        MenuHelper.PrintResult("None 변환", transformed2);

        // ============================================================
        // 7. Point-free 스타일
        // ============================================================
        MenuHelper.PrintSubHeader("7. Point-free 스타일");

        MenuHelper.PrintExplanation("Point-free: 명시적 변수 없이 함수만으로 표현");
        MenuHelper.PrintExplanation("함수 합성을 사용하면 자연스럽게 point-free가 됩니다.");
        MenuHelper.PrintBlankLines();

        // Point-full (변수 명시)
        MenuHelper.PrintCode("// Point-full: x => f(g(x))");
        Func<int, int> pointFull = x => double_(addOne(x));

        // Point-free (변수 없음)
        MenuHelper.PrintCode("// Point-free: compose(f, g)");
        var pointFree = compose(double_, addOne);

        MenuHelper.PrintResult("Point-full(5)", pointFull(5));
        MenuHelper.PrintResult("Point-free(5)", pointFree(5));

        // ============================================================
        // 8. 실전 예제: 데이터 변환 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 데이터 변환 파이프라인");

        var users = List(
            new User("alice", 25, "alice@example.com"),
            new User("bob", 30, "bob@example.com"),
            new User("charlie", 20, "charlie@example.com")
        );

        // 변환 함수들
        Func<User, User> capitalizeUsername = u =>
            u with { Username = u.Username.ToUpper() };

        Func<User, User> addYearToAge = u =>
            u with { Age = u.Age + 1 };

        Func<User, string> formatUser = u =>
            $"{u.Username} ({u.Age}세)";

        // 합성된 파이프라인
        Func<User, string> processUser = u =>
            formatUser(addYearToAge(capitalizeUsername(u)));

        MenuHelper.PrintCode("// User → 대문자화 → 나이+1 → 포맷팅");
        var processed = users.Map(processUser);
        foreach (var p in processed)
        {
            Console.WriteLine($"  - {p}");
        }

        MenuHelper.PrintSuccess("함수 합성 학습 완료!");
    }

    private record User(string Username, int Age, string Email);
}
