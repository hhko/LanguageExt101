using LanguageExt;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part1_Fundamentals.Chapter03_Either;

/// <summary>
/// Either의 체이닝(Map, Bind)을 학습합니다.
///
/// 학습 목표:
/// - Either의 Map과 Bind 사용법
/// - 에러 전파(propagation) 이해
/// - LINQ 쿼리 구문으로 Either 다루기
/// - BiMap과 MapLeft
///
/// 핵심 개념:
/// Either의 Map과 Bind는 Right 값에만 적용됩니다.
/// Left(에러)인 경우 자동으로 에러가 전파되어 체이닝이 중단됩니다.
/// 이를 "Railway Oriented Programming"이라고도 합니다.
/// </summary>
public static class E02_EitherChaining
{
    /// <summary>
    /// 예제를 실행합니다.
    /// </summary>
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 03-E02: Either 체이닝");

        // ============================================================
        // 1. Map으로 Right 값 변환
        // ============================================================
        MenuHelper.PrintSubHeader("1. Map으로 Right 값 변환");

        MenuHelper.PrintExplanation("Map은 Right 값에만 적용됩니다.");
        MenuHelper.PrintExplanation("Left인 경우 변환 없이 그대로 전파됩니다.");
        MenuHelper.PrintBlankLines();

        Either<string, int> right = Right(10);
        Either<string, int> left = Left("에러 발생");

        MenuHelper.PrintCode("var doubled = either.Map(x => x * 2);");

        var doubledRight = right.Map(x => x * 2);
        var doubledLeft = left.Map(x => x * 2);

        MenuHelper.PrintResult("Right(10).Map(x => x * 2)", doubledRight);
        MenuHelper.PrintResult("Left(\"에러\").Map(x => x * 2)", doubledLeft);
        MenuHelper.PrintExplanation("Left는 Map을 무시하고 그대로 전파됩니다");

        // ============================================================
        // 2. Map 체이닝
        // ============================================================
        MenuHelper.PrintSubHeader("2. Map 체이닝");

        MenuHelper.PrintExplanation("여러 Map을 연결할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var chained = Right<string, int>(5)
            .Map(x => x * 2)      // 10
            .Map(x => x + 3)      // 13
            .Map(x => $"결과: {x}"); // "결과: 13"

        MenuHelper.PrintResult("Right(5) → *2 → +3 → 문자열", chained);

        // 중간에 에러가 있으면
        var errorChain = Left<string, int>("초기 에러")
            .Map(x => x * 2)
            .Map(x => x + 3)
            .Map(x => $"결과: {x}");

        MenuHelper.PrintResult("Left(\"초기 에러\") → *2 → +3 → 문자열", errorChain);

        // ============================================================
        // 3. Bind로 Either 반환 함수 연결
        // ============================================================
        MenuHelper.PrintSubHeader("3. Bind로 Either 반환 함수 연결");

        MenuHelper.PrintExplanation("Bind는 Either를 반환하는 함수를 연결합니다.");
        MenuHelper.PrintExplanation("중간에 Left가 반환되면 체인이 중단됩니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 검증 파이프라인 예제");

        var validResult = ValidateName("Alice")
            .Bind(name => ValidateAge("25")
                .Map(age => new Person(name, age)));

        var invalidNameResult = ValidateName("")
            .Bind(name => ValidateAge("25")
                .Map(age => new Person(name, age)));

        var invalidAgeResult = ValidateName("Bob")
            .Bind(name => ValidateAge("-5")
                .Map(age => new Person(name, age)));

        MenuHelper.PrintResult("Valid name + age", validResult);
        MenuHelper.PrintResult("Invalid name", invalidNameResult);
        MenuHelper.PrintResult("Invalid age", invalidAgeResult);

        // ============================================================
        // 4. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("4. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("LINQ로 여러 Either를 조합할 수 있습니다.");
        MenuHelper.PrintExplanation("가독성이 좋아집니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var person = from name in ValidateName(""Charlie"")
              from age in ValidateAge(""30"")
              from email in ValidateEmail(""c@e.com"")
              select new Person(name, age, email);");

        var linqResult =
            from name in ValidateName("Charlie")
            from age in ValidateAge("30")
            from email in ValidateEmail("charlie@example.com")
            select (Name: name, Age: age, Email: email);

        linqResult.Match(
            Right: r => MenuHelper.PrintSuccess($"유효한 데이터: {r.Name}, {r.Age}세, {r.Email}"),
            Left: e => MenuHelper.PrintError($"검증 실패: {e}")
        );

        // 실패하는 경우
        var failedLinq =
            from name in ValidateName("David")
            from age in ValidateAge("invalid")
            from email in ValidateEmail("test@test.com")
            select (Name: name, Age: age, Email: email);

        failedLinq.Match(
            Right: r => MenuHelper.PrintSuccess($"유효한 데이터: {r.Name}, {r.Age}세, {r.Email}"),
            Left: e => MenuHelper.PrintError($"검증 실패: {e}")
        );

        // ============================================================
        // 5. MapLeft로 에러 변환
        // ============================================================
        MenuHelper.PrintSubHeader("5. MapLeft로 에러 변환");

        MenuHelper.PrintExplanation("MapLeft는 Left 값을 변환합니다.");
        MenuHelper.PrintExplanation("에러 타입을 변경하거나 에러 메시지를 가공할 때 사용합니다.");
        MenuHelper.PrintBlankLines();

        Either<string, int> errorResult = Left("원본 에러");

        MenuHelper.PrintCode("var mapped = error.MapLeft(e => $\"[ERROR] {e}\");");
        var mappedError = errorResult.MapLeft(e => $"[ERROR] {e}");
        MenuHelper.PrintResult("MapLeft 결과", mappedError);

        // 에러 타입 변환
        MenuHelper.PrintCode("// 에러 타입 변환: string → ErrorInfo");
        var typedError = errorResult.MapLeft(e => new ErrorInfo(400, e));
        MenuHelper.PrintResult("에러 타입 변환", typedError);

        // ============================================================
        // 6. BiMap (양쪽 모두 변환)
        // ============================================================
        MenuHelper.PrintSubHeader("6. BiMap (양쪽 모두 변환)");

        MenuHelper.PrintExplanation("BiMap은 Left와 Right 모두에 변환을 적용합니다.");
        MenuHelper.PrintBlankLines();

        Either<string, int> value1 = Right(42);
        Either<string, int> value2 = Left("에러");

        MenuHelper.PrintCode("either.BiMap(Right: r => r * 2, Left: l => l.ToUpper())");

        var bi1 = value1.BiMap(Right: r => r * 2, Left: l => l.ToUpper());
        var bi2 = value2.BiMap(Right: r => r * 2, Left: l => l.ToUpper());

        MenuHelper.PrintResult("Right(42).BiMap", bi1);
        MenuHelper.PrintResult("Left(\"에러\").BiMap", bi2);

        // ============================================================
        // 7. IfRight, IfLeft
        // ============================================================
        MenuHelper.PrintSubHeader("7. IfRight, IfLeft");

        MenuHelper.PrintExplanation("한쪽 케이스만 처리하고 싶을 때 사용합니다.");
        MenuHelper.PrintBlankLines();

        Either<string, int> success = Right(100);
        Either<string, int> failure = Left("실패");

        MenuHelper.PrintCode("// IfRight: Right일 때만 실행");
        success.IfRight(v => Console.WriteLine($"  성공 값: {v}"));
        failure.IfRight(v => Console.WriteLine($"  성공 값: {v}"));

        MenuHelper.PrintCode("// IfLeft: Left일 때만 실행");
        success.IfLeft(e => Console.WriteLine($"  에러: {e}"));
        failure.IfLeft(e => Console.WriteLine($"  에러: {e}"));

        // ============================================================
        // 8. 실전 예제: 회원가입 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 회원가입 파이프라인");

        var validSignup = RegisterUser(
            username: "alice",
            email: "alice@example.com",
            password: "SecurePass123"
        );

        var invalidSignup = RegisterUser(
            username: "a",
            email: "invalid-email",
            password: "123"
        );

        validSignup.Match(
            Right: user => MenuHelper.PrintSuccess($"회원가입 성공: {user}"),
            Left: error => MenuHelper.PrintError($"회원가입 실패: {error}")
        );

        invalidSignup.Match(
            Right: user => MenuHelper.PrintSuccess($"회원가입 성공: {user}"),
            Left: error => MenuHelper.PrintError($"회원가입 실패: {error}")
        );

        MenuHelper.PrintSuccess("Either 체이닝 학습 완료!");
    }

    // ============================================================
    // 헬퍼 타입과 함수들
    // ============================================================

    private record Person(string Name, int Age);
    private record ErrorInfo(int Code, string Message);

    private static Either<string, string> ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Left("이름은 필수입니다");
        if (name.Length < 2)
            return Left("이름은 2자 이상이어야 합니다");
        return Right(name);
    }

    private static Either<string, int> ValidateAge(string input)
    {
        if (!int.TryParse(input, out var age))
            return Left("나이는 숫자여야 합니다");
        if (age < 0 || age > 150)
            return Left("나이는 0-150 사이여야 합니다");
        return Right(age);
    }

    private static Either<string, string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Left("이메일은 필수입니다");
        if (!email.Contains('@'))
            return Left("올바른 이메일 형식이 아닙니다");
        return Right(email);
    }

    private static Either<string, string> ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Left("비밀번호는 필수입니다");
        if (password.Length < 8)
            return Left("비밀번호는 8자 이상이어야 합니다");
        return Right(password);
    }

    private static Either<string, string> RegisterUser(
        string username,
        string email,
        string password)
    {
        return from n in ValidateName(username)
               from e in ValidateEmail(email)
               from p in ValidatePassword(password)
               select $"User({n}, {e})";
    }
}
