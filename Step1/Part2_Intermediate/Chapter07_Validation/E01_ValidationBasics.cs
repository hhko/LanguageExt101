using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter07_Validation;

/// <summary>
/// Validation 타입의 기본 사용법을 학습합니다.
///
/// 학습 목표:
/// - Validation<F, S>의 개념
/// - Either와 Validation의 차이
/// - 에러 누적 (Error Accumulation)
/// - Success와 Fail 생성
///
/// 핵심 개념:
/// Validation은 Either와 비슷하지만, 에러를 누적합니다.
/// - Either: 첫 번째 에러에서 중단 (Monad)
/// - Validation: 모든 에러 수집 (Applicative)
///
/// 폼 검증처럼 모든 오류를 한번에 보여줘야 할 때 유용합니다.
/// </summary>
public static class E01_ValidationBasics
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 07-E01: Validation 기본");

        // ============================================================
        // 1. Either vs Validation
        // ============================================================
        MenuHelper.PrintSubHeader("1. Either vs Validation");

        MenuHelper.PrintExplanation("Either는 첫 에러에서 중단됩니다.");
        MenuHelper.PrintExplanation("Validation은 모든 에러를 수집합니다.");
        MenuHelper.PrintBlankLines();

        // Either로 검증 (첫 에러만)
        MenuHelper.PrintCode("// Either: 첫 에러에서 중단");
        var eitherResult = ValidateWithEither("", "bad-email", "12");
        eitherResult.Match(
            Right: _ => { },
            Left: e => Console.WriteLine($"  Either 결과: {e}")
        );
        MenuHelper.PrintBlankLines();

        // Validation으로 검증 (모든 에러)
        MenuHelper.PrintCode("// Validation: 모든 에러 수집");
        var validationResult = ValidateWithValidation("", "bad-email", "12");
        validationResult.Match(
            Succ: _ => { },
            Fail: error => {
                Console.WriteLine("  Validation 결과:");
                Console.WriteLine($"    - {error.Message}");
            }
        );

        // ============================================================
        // 2. Validation 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. Validation 생성");

        MenuHelper.PrintExplanation("Success와 Fail로 Validation을 생성합니다.");
        MenuHelper.PrintBlankLines();

        // 성공 생성
        MenuHelper.PrintCode("var success = Success<Error, int>(42);");
        Validation<Error, int> success = Success<Error, int>(42);
        MenuHelper.PrintResult("Success(42)", success);

        // 실패 생성
        MenuHelper.PrintCode("var fail = Fail<Error, int>(Error.New(\"에러\"));");
        Validation<Error, int> fail = Fail<Error, int>(Error.New("에러 발생"));
        MenuHelper.PrintResult("Fail(에러)", fail);

        // 여러 에러로 실패
        var errors = Error.New("첫 번째 에러") + Error.New("두 번째 에러");
        Validation<Error, int> multiFail = Fail<Error, int>(errors);
        MenuHelper.PrintResult("Fail(여러 에러)", multiFail);

        // ============================================================
        // 3. IsSuccess, IsFail
        // ============================================================
        MenuHelper.PrintSubHeader("3. IsSuccess, IsFail");

        MenuHelper.PrintResult("success.IsSuccess", success.IsSuccess);
        MenuHelper.PrintResult("success.IsFail", success.IsFail);
        MenuHelper.PrintResult("fail.IsSuccess", fail.IsSuccess);
        MenuHelper.PrintResult("fail.IsFail", fail.IsFail);

        // ============================================================
        // 4. Match
        // ============================================================
        MenuHelper.PrintSubHeader("4. Match");

        var result = success.Match(
            Succ: v => $"성공: {v}",
            Fail: e => $"실패: {e.Message}"
        );
        MenuHelper.PrintResult("success.Match", result);

        var failResult = multiFail.Match(
            Succ: v => $"성공: {v}",
            Fail: e => $"실패: {e.Message}"
        );
        MenuHelper.PrintResult("multiFail.Match", failResult);

        // ============================================================
        // 5. Map
        // ============================================================
        MenuHelper.PrintSubHeader("5. Map");

        MenuHelper.PrintExplanation("Map은 성공 값을 변환합니다.");
        MenuHelper.PrintBlankLines();

        var mapped = success.Map(x => x * 2);
        var mappedFail = fail.Map(x => x * 2);

        MenuHelper.PrintResult("Success(42).Map(x => x * 2)", mapped);
        MenuHelper.PrintResult("Fail.Map(x => x * 2)", mappedFail);

        // ============================================================
        // 6. 개별 검증 함수
        // ============================================================
        MenuHelper.PrintSubHeader("6. 개별 검증 함수");

        MenuHelper.PrintExplanation("각 필드를 검증하는 함수를 만듭니다.");
        MenuHelper.PrintBlankLines();

        var validName = ValidateName("Alice");
        var invalidName = ValidateName("");
        var validEmail = ValidateEmail("alice@example.com");
        var invalidEmail = ValidateEmail("bad-email");

        MenuHelper.PrintResult("ValidateName(\"Alice\")", validName);
        MenuHelper.PrintResult("ValidateName(\"\")", invalidName);
        MenuHelper.PrintResult("ValidateEmail(\"alice@...\")", validEmail);
        MenuHelper.PrintResult("ValidateEmail(\"bad-email\")", invalidEmail);

        // ============================================================
        // 7. Validation을 사용한 전체 검증
        // ============================================================
        MenuHelper.PrintSubHeader("7. 전체 검증");

        MenuHelper.PrintExplanation("모든 검증을 수행하고 결과를 조합합니다.");
        MenuHelper.PrintBlankLines();

        // 모두 유효
        var allValid = ValidateUser("Alice", "alice@example.com", "SecurePass123");
        allValid.Match(
            Succ: user => MenuHelper.PrintSuccess($"유효: {user}"),
            Fail: error => PrintError(error)
        );

        // 일부 무효
        var someInvalid = ValidateUser("A", "bad", "12");
        someInvalid.Match(
            Succ: user => MenuHelper.PrintSuccess($"유효: {user}"),
            Fail: error => PrintError(error)
        );

        MenuHelper.PrintSuccess("Validation 기본 학습 완료!");
    }

    // ============================================================
    // 헬퍼 함수들
    // ============================================================

    private record User(string Name, string Email);

    private static Either<string, User> ValidateWithEither(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Left("이름은 필수입니다");
        if (!email.Contains('@'))
            return Left("이메일 형식이 잘못되었습니다");
        if (password.Length < 8)
            return Left("비밀번호는 8자 이상이어야 합니다");
        return Right(new User(name, email));
    }

    private static Validation<Error, User> ValidateWithValidation(string name, string email, string password)
    {
        Error? errors = null;

        if (string.IsNullOrWhiteSpace(name))
            errors = errors == null ? Error.New("이름은 필수입니다") : errors + Error.New("이름은 필수입니다");
        if (!email.Contains('@'))
            errors = errors == null ? Error.New("이메일 형식이 잘못되었습니다") : errors + Error.New("이메일 형식이 잘못되었습니다");
        if (password.Length < 8)
            errors = errors == null ? Error.New("비밀번호는 8자 이상이어야 합니다") : errors + Error.New("비밀번호는 8자 이상이어야 합니다");

        return errors == null
            ? Success<Error, User>(new User(name, email))
            : Fail<Error, User>(errors);
    }

    private static Validation<Error, string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Fail<Error, string>(Error.New("이름은 필수입니다"));
        if (name.Length < 2)
            return Fail<Error, string>(Error.New("이름은 2자 이상이어야 합니다"));
        return Success<Error, string>(name);
    }

    private static Validation<Error, string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Fail<Error, string>(Error.New("이메일은 필수입니다"));
        if (!email.Contains('@'))
            return Fail<Error, string>(Error.New("올바른 이메일 형식이 아닙니다"));
        return Success<Error, string>(email);
    }

    private static Validation<Error, string> ValidatePassword(string password)
    {
        Error? errors = null;

        if (password.Length < 8)
            errors = errors == null ? Error.New("비밀번호는 8자 이상이어야 합니다") : errors + Error.New("비밀번호는 8자 이상이어야 합니다");
        if (!password.Any(char.IsUpper))
            errors = errors == null ? Error.New("대문자가 포함되어야 합니다") : errors + Error.New("대문자가 포함되어야 합니다");
        if (!password.Any(char.IsDigit))
            errors = errors == null ? Error.New("숫자가 포함되어야 합니다") : errors + Error.New("숫자가 포함되어야 합니다");

        return errors == null
            ? Success<Error, string>(password)
            : Fail<Error, string>(errors);
    }

    private static Validation<Error, User> ValidateUser(string name, string email, string password)
    {
        var nameResult = ValidateName(name);
        var emailResult = ValidateEmail(email);
        var passwordResult = ValidatePassword(password);

        // 모든 검증 결과의 에러 수집
        Error? errors = null;

        nameResult.IfFail(e => errors = errors == null ? e : errors + e);
        emailResult.IfFail(e => errors = errors == null ? e : errors + e);
        passwordResult.IfFail(e => errors = errors == null ? e : errors + e);

        return errors == null
            ? Success<Error, User>(new User(name, email))
            : Fail<Error, User>(errors);
    }

    private static void PrintError(Error error)
    {
        MenuHelper.PrintError($"검증 실패: {error.Message}");
    }
}
