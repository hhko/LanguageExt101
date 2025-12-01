using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter03_Either;

/// <summary>
/// Either의 Left/Right 기본 개념을 학습합니다.
///
/// 학습 목표:
/// - Either<L, R>의 기본 구조 이해
/// - Left와 Right의 의미와 관례
/// - Either 생성 방법
/// - Option과 Either의 차이점
///
/// 핵심 개념:
/// Either<L, R>은 두 타입 중 하나의 값을 가지는 타입입니다.
/// - Right(R): 성공/정상 케이스 (올바른 값)
/// - Left(L): 실패/에러 케이스 (에러 정보)
///
/// 관례적으로 Right가 "올바른(right)" 값을 의미합니다.
/// Option은 값의 유무만 표현하지만, Either는 에러 정보도 함께 전달할 수 있습니다.
/// </summary>
public static class E01_LeftRight
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 03-E01: Left/Right 기본");

        // ============================================================
        // 1. Either의 기본 구조
        // ============================================================
        MenuHelper.PrintSubHeader("1. Either의 기본 구조");

        MenuHelper.PrintExplanation("Either<L, R>은 L 또는 R 중 하나의 값을 가집니다.");
        MenuHelper.PrintExplanation("L은 Left(에러), R은 Right(성공)을 나타냅니다.");
        MenuHelper.PrintExplanation("'Right'는 '올바른(correct)'이라는 의미와 연결됩니다.");
        MenuHelper.PrintBlankLines();

        // 타입 시그니처 설명
        MenuHelper.PrintCode("// Either<에러타입, 성공타입>");
        MenuHelper.PrintCode("// Either<string, int> - 에러 메시지 또는 정수 값");
        MenuHelper.PrintCode("// Either<Error, User> - 에러 또는 사용자 객체");

        // ============================================================
        // 2. Right 생성 (성공 케이스)
        // ============================================================
        MenuHelper.PrintSubHeader("2. Right 생성 (성공 케이스)");

        MenuHelper.PrintExplanation("Right()는 성공적인 결과를 나타냅니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("var success = Right<string, int>(42);");
        Either<string, int> success = Right(42);
        MenuHelper.PrintResult("Right(42)", success);

        MenuHelper.PrintCode("var userResult = Right<string, User>(new User(1, \"Alice\"));");
        Either<string, User> userResult = Right(new User(1, "Alice"));
        MenuHelper.PrintResult("Right(User)", userResult);

        // ============================================================
        // 3. Left 생성 (에러 케이스)
        // ============================================================
        MenuHelper.PrintSubHeader("3. Left 생성 (에러 케이스)");

        MenuHelper.PrintExplanation("Left()는 실패/에러를 나타냅니다.");
        MenuHelper.PrintExplanation("에러 메시지, 에러 코드, 또는 커스텀 에러 타입을 담을 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("var error = Left<string, int>(\"값이 유효하지 않습니다\");");
        Either<string, int> error = Left("값이 유효하지 않습니다");
        MenuHelper.PrintResult("Left(에러 메시지)", error);

        MenuHelper.PrintCode("var userError = Left<string, User>(\"사용자를 찾을 수 없습니다\");");
        Either<string, User> userError = Left("사용자를 찾을 수 없습니다");
        MenuHelper.PrintResult("Left(사용자 에러)", userError);

        // ============================================================
        // 4. IsLeft, IsRight 검사
        // ============================================================
        MenuHelper.PrintSubHeader("4. IsLeft, IsRight 검사");

        MenuHelper.PrintExplanation("Either의 상태를 확인할 수 있습니다.");
        MenuHelper.PrintExplanation("하지만 Match를 사용하는 것이 더 안전합니다.");
        MenuHelper.PrintBlankLines();

        Either<string, int> rightValue = Right(100);
        Either<string, int> leftValue = Left("에러");

        MenuHelper.PrintResult("Right(100).IsRight", rightValue.IsRight);
        MenuHelper.PrintResult("Right(100).IsLeft", rightValue.IsLeft);
        MenuHelper.PrintResult("Left(\"에러\").IsRight", leftValue.IsRight);
        MenuHelper.PrintResult("Left(\"에러\").IsLeft", leftValue.IsLeft);

        // ============================================================
        // 5. Match로 Either 처리
        // ============================================================
        MenuHelper.PrintSubHeader("5. Match로 Either 처리");

        MenuHelper.PrintExplanation("Match는 Left와 Right 케이스를 모두 처리합니다.");
        MenuHelper.PrintExplanation("컴파일러가 두 케이스 모두 처리했는지 확인합니다.");
        MenuHelper.PrintBlankLines();

        Either<string, int> result1 = Right(42);
        Either<string, int> result2 = Left("계산 오류");

        MenuHelper.PrintCode("either.Match(Right: r => ..., Left: l => ...)");

        var message1 = result1.Match(
            Right: r => $"성공: {r}",
            Left: l => $"실패: {l}"
        );
        MenuHelper.PrintResult("Right(42) Match", message1);

        var message2 = result2.Match(
            Right: r => $"성공: {r}",
            Left: l => $"실패: {l}"
        );
        MenuHelper.PrintResult("Left(\"계산 오류\") Match", message2);

        // ============================================================
        // 6. Option vs Either 비교
        // ============================================================
        MenuHelper.PrintSubHeader("6. Option vs Either 비교");

        MenuHelper.PrintCode("// Option<T>: 값이 있거나 없음");
        MenuHelper.PrintCode("// - Some(value) 또는 None");
        MenuHelper.PrintCode("// - 에러 정보 없음");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Either<L, R>: 성공 값 또는 에러 정보");
        MenuHelper.PrintCode("// - Right(value) 또는 Left(error)");
        MenuHelper.PrintCode("// - 에러의 이유를 알 수 있음");
        MenuHelper.PrintBlankLines();

        // 같은 시나리오, 다른 표현
        var optionResult = FindUserOption(999);
        var eitherResult = FindUserEither(999);

        MenuHelper.PrintResult("Option으로 사용자 조회(999)", optionResult);
        MenuHelper.PrintResult("Either로 사용자 조회(999)", eitherResult);

        // ============================================================
        // 7. 다양한 Left 타입
        // ============================================================
        MenuHelper.PrintSubHeader("7. 다양한 Left 타입");

        MenuHelper.PrintExplanation("Left 타입은 상황에 맞게 선택할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // 문자열 에러
        Either<string, int> stringError = Left("단순 에러 메시지");

        // 에러 코드
        Either<int, string> codeError = Left(404);

        // 커스텀 에러 타입
        Either<ValidationError, User> customError = Left(
            new ValidationError("email", "이메일 형식이 잘못되었습니다"));

        MenuHelper.PrintResult("문자열 에러", stringError);
        MenuHelper.PrintResult("에러 코드", codeError);
        MenuHelper.PrintResult("커스텀 에러", customError);

        // ============================================================
        // 8. 실전 예제: 입력 검증
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 입력 검증");

        var valid = ValidateAge("25");
        var invalid1 = ValidateAge("abc");
        var invalid2 = ValidateAge("-5");
        var invalid3 = ValidateAge("200");

        MenuHelper.PrintResult("ValidateAge(\"25\")", valid);
        MenuHelper.PrintResult("ValidateAge(\"abc\")", invalid1);
        MenuHelper.PrintResult("ValidateAge(\"-5\")", invalid2);
        MenuHelper.PrintResult("ValidateAge(\"200\")", invalid3);

        MenuHelper.PrintSuccess("Left/Right 기본 학습 완료!");
    }

    // ============================================================
    // 헬퍼 타입과 함수들
    // ============================================================

    private record User(int Id, string Name);
    private record ValidationError(string Field, string Message);

    private static Option<User> FindUserOption(int id)
    {
        var users = new Dictionary<int, User>
        {
            { 1, new User(1, "Alice") },
            { 2, new User(2, "Bob") }
        };
        return users.TryGetValue(id, out var user) ? Some(user) : None;
    }

    private static Either<string, User> FindUserEither(int id)
    {
        var users = new Dictionary<int, User>
        {
            { 1, new User(1, "Alice") },
            { 2, new User(2, "Bob") }
        };

        return users.TryGetValue(id, out var user)
            ? Right(user)
            : Left($"ID가 {id}인 사용자를 찾을 수 없습니다");
    }

    private static Either<string, int> ValidateAge(string input)
    {
        if (!int.TryParse(input, out var age))
            return Left("나이는 숫자여야 합니다");

        if (age < 0)
            return Left("나이는 음수일 수 없습니다");

        if (age > 150)
            return Left("나이가 너무 큽니다");

        return Right(age);
    }
}
