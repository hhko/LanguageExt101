using LanguageExt;
using LanguageExt.Common;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter07_Validation;

/// <summary>
/// Applicative 스타일의 Validation을 학습합니다.
///
/// 학습 목표:
/// - Applicative Functor 개념
/// - 여러 Validation 조합
/// - 에러 누적 패턴
///
/// 핵심 개념:
/// Applicative는 여러 컨텍스트 안의 값들을 조합하는 패턴입니다.
/// Monad(Bind)와 달리 순차적이지 않고 독립적으로 실행됩니다.
/// 이 특성 덕분에 모든 검증을 병렬로 수행하고 에러를 모을 수 있습니다.
/// </summary>
public static class E02_ApplicativeStyle
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 07-E02: Applicative 스타일");

        // ============================================================
        // 1. Applicative의 개념
        // ============================================================
        MenuHelper.PrintSubHeader("1. Applicative의 개념");

        MenuHelper.PrintExplanation("Applicative는 여러 '박스' 안의 값들을 함수에 적용합니다.");
        MenuHelper.PrintExplanation("Monad와 달리, 값들이 서로 의존하지 않습니다.");
        MenuHelper.PrintBlankLines();

        // Monad: B가 A에 의존
        MenuHelper.PrintCode("// Monad (Bind): B가 A의 결과에 의존");
        MenuHelper.PrintCode("from a in getA()");
        MenuHelper.PrintCode("from b in getB(a)  // a에 의존");
        MenuHelper.PrintCode("select combine(a, b)");
        MenuHelper.PrintBlankLines();

        // Applicative: A와 B가 독립
        MenuHelper.PrintCode("// Applicative: A와 B가 독립적");
        MenuHelper.PrintCode("(getA(), getB())  // 둘 다 독립적으로 실행");
        MenuHelper.PrintCode("  .Apply((a, b) => combine(a, b))");

        // ============================================================
        // 2. 수동 에러 누적
        // ============================================================
        MenuHelper.PrintSubHeader("2. 수동 에러 누적");

        MenuHelper.PrintExplanation("각 검증을 독립적으로 실행하고 에러를 수동으로 모읍니다.");
        MenuHelper.PrintBlankLines();

        var result1 = ValidateUserManual("A", "bad", "12");
        result1.Match(
            Succ: user => MenuHelper.PrintSuccess($"성공: {user}"),
            Fail: error => MenuHelper.PrintError($"실패: {error.Message}")
        );

        // ============================================================
        // 3. Validation 조합 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("3. Validation 조합 패턴");

        MenuHelper.PrintExplanation("여러 Validation을 하나로 조합합니다.");
        MenuHelper.PrintBlankLines();

        // 성공 케이스
        var validResult = CombineValidations(
            Success<Error, string>("Alice"),
            Success<Error, string>("alice@example.com"),
            Success<Error, int>(25)
        );

        validResult.Match(
            Succ: data => MenuHelper.PrintSuccess($"성공: {data}"),
            Fail: error => MenuHelper.PrintError($"실패: {error.Message}")
        );

        // 실패 케이스 (여러 에러)
        var invalidResult = CombineValidations(
            Fail<Error, string>(Error.New("이름 오류")),
            Fail<Error, string>(Error.New("이메일 오류")),
            Fail<Error, int>(Error.New("나이 오류"))
        );

        invalidResult.Match(
            Succ: data => MenuHelper.PrintSuccess($"성공: {data}"),
            Fail: error => MenuHelper.PrintError($"실패: {error.Message}")
        );

        // ============================================================
        // 4. 실전 예제: 신용카드 검증
        // ============================================================
        MenuHelper.PrintSubHeader("4. 실전 예제: 신용카드 검증");

        MenuHelper.PrintExplanation("신용카드 정보의 모든 필드를 검증합니다.");
        MenuHelper.PrintBlankLines();

        // 유효한 카드
        var validCard = ValidateCreditCard(
            cardNumber: "1234-5678-9012-3456",
            expiryMonth: 12,
            expiryYear: 2025,
            cvv: "123"
        );

        validCard.Match(
            Succ: card => MenuHelper.PrintSuccess($"유효한 카드: {card.MaskedNumber}"),
            Fail: error => MenuHelper.PrintError($"카드 검증 실패: {error.Message}")
        );

        // 여러 오류가 있는 카드
        var invalidCard = ValidateCreditCard(
            cardNumber: "1234",
            expiryMonth: 13,
            expiryYear: 2020,
            cvv: "12345"
        );

        invalidCard.Match(
            Succ: card => MenuHelper.PrintSuccess($"유효한 카드: {card.MaskedNumber}"),
            Fail: error => MenuHelper.PrintError($"카드 검증 실패: {error.Message}")
        );

        // ============================================================
        // 5. 부분 성공 케이스
        // ============================================================
        MenuHelper.PrintSubHeader("5. 부분 성공 케이스");

        MenuHelper.PrintExplanation("일부는 유효하고 일부는 무효한 경우");
        MenuHelper.PrintBlankLines();

        var partialResult = CombineValidations(
            Success<Error, string>("Alice"),  // 유효
            Fail<Error, string>(Error.New("이메일 형식 오류")),  // 무효
            Success<Error, int>(25)  // 유효
        );

        partialResult.Match(
            Succ: data => MenuHelper.PrintSuccess($"성공: {data}"),
            Fail: error => MenuHelper.PrintError($"실패 (유효한 필드도 있지만 전체는 실패): {error.Message}")
        );

        MenuHelper.PrintSuccess("Applicative 스타일 학습 완료!");
    }

    // ============================================================
    // 헬퍼 타입과 함수들
    // ============================================================

    private record User(string Name, string Email);
    private record CreditCard(string Number, int ExpiryMonth, int ExpiryYear, string Cvv)
    {
        public string MaskedNumber => $"****-****-****-{Number.Replace("-", "")[^4..]}";
    }

    private static Validation<Error, User> ValidateUserManual(string name, string email, string password)
    {
        var nameV = ValidateName(name);
        var emailV = ValidateEmail(email);
        var passwordV = ValidatePassword(password);

        Error? errors = null;
        nameV.IfFail(e => errors = errors == null ? e : errors + e);
        emailV.IfFail(e => errors = errors == null ? e : errors + e);
        passwordV.IfFail(e => errors = errors == null ? e : errors + e);

        if (errors == null)
        {
            return Success<Error, User>(new User(
                nameV.Match(Succ: n => n, Fail: _ => ""),
                emailV.Match(Succ: e => e, Fail: _ => "")
            ));
        }

        return Fail<Error, User>(errors);
    }

    private static Validation<Error, string> ValidateName(string name) =>
        string.IsNullOrWhiteSpace(name) || name.Length < 2
            ? Fail<Error, string>(Error.New("이름은 2자 이상이어야 합니다"))
            : Success<Error, string>(name);

    private static Validation<Error, string> ValidateEmail(string email) =>
        !email.Contains('@')
            ? Fail<Error, string>(Error.New("올바른 이메일 형식이 아닙니다"))
            : Success<Error, string>(email);

    private static Validation<Error, string> ValidatePassword(string password) =>
        password.Length < 8
            ? Fail<Error, string>(Error.New("비밀번호는 8자 이상이어야 합니다"))
            : Success<Error, string>(password);

    private static Validation<Error, (string Name, string Email, int Age)> CombineValidations(
        Validation<Error, string> nameV,
        Validation<Error, string> emailV,
        Validation<Error, int> ageV)
    {
        Error? errors = null;

        nameV.IfFail(e => errors = errors == null ? e : errors + e);
        emailV.IfFail(e => errors = errors == null ? e : errors + e);
        ageV.IfFail(e => errors = errors == null ? e : errors + e);

        if (errors == null)
        {
            return Success<Error, (string, string, int)>((
                nameV.Match(Succ: n => n, Fail: _ => ""),
                emailV.Match(Succ: e => e, Fail: _ => ""),
                ageV.Match(Succ: a => a, Fail: _ => 0)
            ));
        }

        return Fail<Error, (string, string, int)>(errors);
    }

    private static Validation<Error, CreditCard> ValidateCreditCard(
        string cardNumber,
        int expiryMonth,
        int expiryYear,
        string cvv)
    {
        Error? errors = null;

        // 카드번호 검증
        var cleanNumber = cardNumber.Replace("-", "").Replace(" ", "");
        if (cleanNumber.Length != 16)
            errors = errors == null ? Error.New("카드번호는 16자리여야 합니다") : errors + Error.New("카드번호는 16자리여야 합니다");

        // 만료월 검증
        if (expiryMonth < 1 || expiryMonth > 12)
            errors = errors == null ? Error.New("만료월은 1-12 사이여야 합니다") : errors + Error.New("만료월은 1-12 사이여야 합니다");

        // 만료년 검증
        if (expiryYear < DateTime.Now.Year)
            errors = errors == null ? Error.New("카드가 만료되었습니다") : errors + Error.New("카드가 만료되었습니다");

        // CVV 검증
        if (cvv.Length < 3 || cvv.Length > 4)
            errors = errors == null ? Error.New("CVV는 3-4자리여야 합니다") : errors + Error.New("CVV는 3-4자리여야 합니다");

        return errors == null
            ? Success<Error, CreditCard>(new CreditCard(cardNumber, expiryMonth, expiryYear, cvv))
            : Fail<Error, CreditCard>(errors);
    }
}
