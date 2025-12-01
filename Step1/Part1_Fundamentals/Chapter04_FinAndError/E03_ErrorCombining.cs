using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter04_FinAndError;

/// <summary>
/// Error 결합과 누적을 학습합니다.
/// </summary>
public static class E03_ErrorCombining
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 04-E03: 에러 결합");

        // ============================================================
        // 1. + 연산자로 에러 결합
        // ============================================================
        MenuHelper.PrintSubHeader("1. + 연산자로 에러 결합");

        MenuHelper.PrintExplanation("Error + Error로 여러 에러를 결합합니다.");
        MenuHelper.PrintBlankLines();

        var error1 = Error.New("첫 번째 에러");
        var error2 = Error.New("두 번째 에러");
        var error3 = Error.New("세 번째 에러");

        MenuHelper.PrintCode("var combined = error1 + error2 + error3;");
        var combined = error1 + error2 + error3;

        MenuHelper.PrintResult("결합된 에러", combined.Message);

        // ============================================================
        // 2. 검증 시나리오에서 에러 수집
        // ============================================================
        MenuHelper.PrintSubHeader("2. 검증 시나리오에서 에러 수집");

        MenuHelper.PrintExplanation("모든 검증을 수행하고 에러를 수집합니다.");
        MenuHelper.PrintBlankLines();

        // 유효한 데이터
        var validResult = ValidateUserCollectAll("Alice", "alice@example.com", "SecurePass123");
        validResult.Match(
            Succ: user => MenuHelper.PrintSuccess($"유효한 사용자: {user}"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        // 여러 에러가 있는 데이터
        var invalidResult = ValidateUserCollectAll("A", "invalid-email", "123");
        invalidResult.Match(
            Succ: user => MenuHelper.PrintSuccess($"유효한 사용자: {user}"),
            Fail: error => MenuHelper.PrintError($"검증 실패: {error.Message}")
        );

        // ============================================================
        // 3. Either vs 에러 수집 비교
        // ============================================================
        MenuHelper.PrintSubHeader("3. Either vs 에러 수집 비교");

        MenuHelper.PrintExplanation("Either는 첫 에러에서 중단됩니다.");
        MenuHelper.PrintExplanation("에러 수집은 모든 에러를 보여줍니다.");
        MenuHelper.PrintBlankLines();

        // Either 방식 (첫 에러만)
        var eitherResult = ValidateWithEither("A", "bad", "12");
        eitherResult.Match(
            Right: _ => { },
            Left: e => Console.WriteLine($"  Either 방식: {e}")
        );

        // 에러 수집 방식 (모든 에러)
        var collectResult = ValidateUserCollectAll("A", "bad", "12");
        collectResult.IfFail(error =>
            Console.WriteLine($"  에러 수집 방식: {error.Message}")
        );

        MenuHelper.PrintSuccess("에러 결합 학습 완료!");
    }

    private record User(string Name, string Email);

    private static Fin<User> ValidateUserCollectAll(string name, string email, string password)
    {
        Error? errors = null;

        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            errors = errors == null
                ? Error.New("이름은 2자 이상이어야 합니다")
                : errors + Error.New("이름은 2자 이상이어야 합니다");

        if (!email.Contains('@'))
            errors = errors == null
                ? Error.New("이메일 형식이 올바르지 않습니다")
                : errors + Error.New("이메일 형식이 올바르지 않습니다");

        if (password.Length < 8)
            errors = errors == null
                ? Error.New("비밀번호는 8자 이상이어야 합니다")
                : errors + Error.New("비밀번호는 8자 이상이어야 합니다");

        return errors == null
            ? FinSucc(new User(name, email))
            : FinFail<User>(errors);
    }

    private static Either<string, User> ValidateWithEither(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            return Left("이름은 2자 이상이어야 합니다");

        if (!email.Contains('@'))
            return Left("이메일 형식이 올바르지 않습니다");

        if (password.Length < 8)
            return Left("비밀번호는 8자 이상이어야 합니다");

        return Right(new User(name, email));
    }
}
