using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter02_Option;

/// <summary>
/// Option 생성의 다양한 방법을 학습합니다.
///
/// 학습 목표:
/// - Some(), None을 사용한 명시적 생성
/// - Optional()을 사용한 null-safe 변환
/// - 암시적 변환 이해
///
/// 핵심 개념:
/// Option은 값이 있거나(Some) 없는(None) 상태를 명시적으로 표현합니다.
/// null 대신 Option을 사용하면 NullReferenceException을 컴파일 타임에 방지할 수 있습니다.
///
/// Option은 함수형 프로그래밍에서 가장 기본적이고 중요한 타입 중 하나입니다.
/// Haskell의 Maybe, Rust의 Option, Scala의 Option과 동일한 개념입니다.
/// </summary>
public static class E01_CreatingOptions
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 02-E01: Option 생성");

        // ============================================================
        // 1. Some으로 값 감싸기
        // ============================================================
        MenuHelper.PrintSubHeader("1. Some으로 값 감싸기");

        MenuHelper.PrintExplanation("Some()은 값이 있는 상태를 나타냅니다.");
        MenuHelper.PrintExplanation("값이 확실히 존재할 때 사용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("var intOption = Some(42);");
        var intOption = Some(42);
        MenuHelper.PrintResult("Some(42)", intOption);

        MenuHelper.PrintCode("var stringOption = Some(\"Hello\");");
        var stringOption = Some("Hello");
        MenuHelper.PrintResult("Some(\"Hello\")", stringOption);

        MenuHelper.PrintCode("var listOption = Some(List(1, 2, 3));");
        var listOption = Some(List(1, 2, 3));
        MenuHelper.PrintResult("Some(List(1, 2, 3))", listOption);

        // ============================================================
        // 2. None 사용하기
        // ============================================================
        MenuHelper.PrintSubHeader("2. None 사용하기");

        MenuHelper.PrintExplanation("None은 값이 없는 상태를 나타냅니다.");
        MenuHelper.PrintExplanation("null 대신 None을 사용하여 명시적으로 '값 없음'을 표현합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("Option<int> noInt = None;");
        Option<int> noInt = None;
        MenuHelper.PrintResult("None (int)", noInt);

        MenuHelper.PrintCode("Option<string> noString = None;");
        Option<string> noString = None;
        MenuHelper.PrintResult("None (string)", noString);

        // Option<T>.None도 사용 가능
        MenuHelper.PrintCode("var explicitNone = Option<double>.None;");
        var explicitNone = Option<double>.None;
        MenuHelper.PrintResult("Option<double>.None", explicitNone);

        // ============================================================
        // 3. Optional()로 null 안전하게 변환
        // ============================================================
        MenuHelper.PrintSubHeader("3. Optional()로 null 안전하게 변환");

        MenuHelper.PrintExplanation("Optional()은 null일 수 있는 값을 안전하게 Option으로 변환합니다.");
        MenuHelper.PrintExplanation("값이 있으면 Some, null이면 None이 됩니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("string? maybeNull = GetNullableString();");
        string? maybeNull = GetNullableString();

        MenuHelper.PrintCode("var safeOption = Optional(maybeNull);");
        var safeOption = Optional(maybeNull);
        MenuHelper.PrintResult("Optional(null)", safeOption);
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("string? hasValue = \"실제 값\";");
        string? hasValue = "실제 값";

        MenuHelper.PrintCode("var valueOption = Optional(hasValue);");
        var valueOption = Optional(hasValue);
        MenuHelper.PrintResult("Optional(\"실제 값\")", valueOption);

        // ============================================================
        // 4. 암시적 변환
        // ============================================================
        MenuHelper.PrintSubHeader("4. 암시적 변환");

        MenuHelper.PrintExplanation("Option<T>는 T로부터 암시적 변환을 지원합니다.");
        MenuHelper.PrintExplanation("코드를 더 간결하게 작성할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("Option<int> implicitSome = 100;");
        Option<int> implicitSome = 100;
        MenuHelper.PrintResult("암시적 변환 (100)", implicitSome);

        MenuHelper.PrintCode("Option<string> implicitString = \"World\";");
        Option<string> implicitString = "World";
        MenuHelper.PrintResult("암시적 변환 (\"World\")", implicitString);

        // ============================================================
        // 5. 조건부 생성
        // ============================================================
        MenuHelper.PrintSubHeader("5. 조건부 생성");

        MenuHelper.PrintExplanation("조건에 따라 Some 또는 None을 반환할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        int age = 25;
        MenuHelper.PrintCode("var adultAge = age >= 18 ? Some(age) : None;");
        var adultAge = age >= 18 ? Some(age) : None;
        MenuHelper.PrintResult("age=25일 때 성인 여부", adultAge);

        age = 15;
        MenuHelper.PrintCode("var minorAge = age >= 18 ? Some(age) : None;");
        var minorAge = age >= 18 ? Some(age) : None;
        MenuHelper.PrintResult("age=15일 때 성인 여부", minorAge);

        // ============================================================
        // 6. IsSome, IsNone 검사
        // ============================================================
        MenuHelper.PrintSubHeader("6. IsSome, IsNone 검사");

        MenuHelper.PrintExplanation("Option의 상태를 확인할 수 있습니다.");
        MenuHelper.PrintExplanation("하지만 가능하면 Match나 Map을 사용하는 것이 더 좋습니다.");
        MenuHelper.PrintBlankLines();

        var someValue = Some("테스트");
        Option<string> noneValue = None;

        MenuHelper.PrintResult("Some(\"테스트\").IsSome", someValue.IsSome);
        MenuHelper.PrintResult("Some(\"테스트\").IsNone", someValue.IsNone);
        MenuHelper.PrintResult("None.IsSome", noneValue.IsSome);
        MenuHelper.PrintResult("None.IsNone", noneValue.IsNone);

        // ============================================================
        // 7. 실전 예제: 사용자 조회
        // ============================================================
        MenuHelper.PrintSubHeader("7. 실전 예제: 사용자 조회");

        MenuHelper.PrintExplanation("데이터베이스에서 사용자를 조회하는 시나리오입니다.");
        MenuHelper.PrintExplanation("사용자가 있으면 Some<User>, 없으면 None을 반환합니다.");
        MenuHelper.PrintBlankLines();

        var user1 = FindUser(1);
        var user2 = FindUser(999);

        MenuHelper.PrintResult("FindUser(1)", user1);
        MenuHelper.PrintResult("FindUser(999)", user2);

        MenuHelper.PrintSuccess("Option 생성 방법 학습 완료!");
    }

    // 테스트용 헬퍼 함수들
    private static string? GetNullableString() => null;

    private static Option<User> FindUser(int id)
    {
        // 가상의 사용자 데이터
        var users = new Dictionary<int, User>
        {
            { 1, new User(1, "Alice", "alice@example.com") },
            { 2, new User(2, "Bob", "bob@example.com") }
        };

        return users.TryGetValue(id, out var user) ? Some(user) : None;
    }

    // 간단한 User 레코드
    private record User(int Id, string Name, string Email);
}
