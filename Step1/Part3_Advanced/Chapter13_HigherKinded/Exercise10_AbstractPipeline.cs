using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter13_HigherKinded;

/// <summary>
/// 실습 10: Higher-Kinded Types를 사용한 추상 파이프라인 구축
///
/// 학습 목표:
/// - K&lt;F, A&gt;와 trait 제약을 사용한 추상 파이프라인 설계
/// - 여러 모나드 타입에서 재사용 가능한 데이터 처리 파이프라인 구현
/// - 실제 비즈니스 로직에 HKT 패턴 적용
///
/// 시나리오:
/// 사용자 데이터 처리 파이프라인을 구현합니다.
/// 이 파이프라인은 Option, Either 등 다양한 모나드에서 동작해야 합니다.
///
/// 파이프라인 단계:
/// 1. 입력 검증 (Validation)
/// 2. 데이터 변환 (Transformation)
/// 3. 비즈니스 로직 적용 (Business Logic)
/// 4. 결과 포맷팅 (Formatting)
/// </summary>
public static class Exercise10_AbstractPipeline
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 10: 추상 파이프라인");

        MenuHelper.PrintExplanation("이 실습에서는 Higher-Kinded Types를 사용하여");
        MenuHelper.PrintExplanation("여러 모나드에서 재사용 가능한 데이터 처리 파이프라인을 구축합니다.");
        MenuHelper.PrintBlankLines();

        // ============================================================
        // Part 1: 추상 파이프라인 인터페이스 정의
        // ============================================================
        MenuHelper.PrintSubHeader("Part 1: 추상 파이프라인 개념");

        MenuHelper.PrintExplanation("파이프라인은 여러 단계로 구성됩니다:");
        MenuHelper.PrintExplanation("  Input → Validate → Transform → Process → Format → Output");
        MenuHelper.PrintExplanation("각 단계는 K<M, A> → K<M, B> 형태의 함수입니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 파이프라인 단계 타입");
        MenuHelper.PrintCode("// Func<K<M, A>, K<M, B>> where M : Monad<M>");

        // ============================================================
        // Part 2: 사용자 데이터 모델
        // ============================================================
        MenuHelper.PrintSubHeader("Part 2: 데이터 모델");

        MenuHelper.PrintCode("record UserInput(string Name, int Age, string Email);");
        MenuHelper.PrintCode("record ValidatedUser(string Name, int Age, string Email);");
        MenuHelper.PrintCode("record ProcessedUser(string DisplayName, string AgeGroup, bool IsActive);");
        MenuHelper.PrintBlankLines();

        var sampleInput = new UserInput("Alice", 25, "alice@example.com");
        MenuHelper.PrintResult("샘플 입력", sampleInput);

        // ============================================================
        // Part 3: Option 기반 파이프라인 실행
        // ============================================================
        MenuHelper.PrintSubHeader("Part 3: Option 기반 파이프라인");

        MenuHelper.PrintExplanation("Option을 사용한 파이프라인 - 실패 시 None 반환");
        MenuHelper.PrintBlankLines();

        // 유효한 입력
        var validResult = RunPipelineWithOption(new UserInput("Bob", 30, "bob@test.com"));
        MenuHelper.PrintCode("RunPipelineWithOption(new UserInput(\"Bob\", 30, \"bob@test.com\"))");
        MenuHelper.PrintResult("유효한 입력 결과", validResult);

        // 잘못된 이름 (빈 문자열)
        var invalidNameResult = RunPipelineWithOption(new UserInput("", 25, "test@test.com"));
        MenuHelper.PrintCode("RunPipelineWithOption(new UserInput(\"\", 25, \"test@test.com\"))");
        MenuHelper.PrintResult("빈 이름 결과", invalidNameResult);

        // 잘못된 나이 (음수)
        var invalidAgeResult = RunPipelineWithOption(new UserInput("Charlie", -5, "charlie@test.com"));
        MenuHelper.PrintCode("RunPipelineWithOption(new UserInput(\"Charlie\", -5, \"charlie@test.com\"))");
        MenuHelper.PrintResult("음수 나이 결과", invalidAgeResult);

        // ============================================================
        // Part 4: Either 기반 파이프라인 실행
        // ============================================================
        MenuHelper.PrintSubHeader("Part 4: Either 기반 파이프라인");

        MenuHelper.PrintExplanation("Either를 사용한 파이프라인 - 실패 시 에러 메시지 반환");
        MenuHelper.PrintBlankLines();

        // 유효한 입력
        var eitherValidResult = RunPipelineWithEither(new UserInput("Diana", 45, "diana@company.com"));
        MenuHelper.PrintCode("RunPipelineWithEither(new UserInput(\"Diana\", 45, \"diana@company.com\"))");
        PrintEitherResult("유효한 입력", eitherValidResult);

        // 잘못된 이메일
        var eitherInvalidEmail = RunPipelineWithEither(new UserInput("Eve", 28, "invalid-email"));
        MenuHelper.PrintCode("RunPipelineWithEither(new UserInput(\"Eve\", 28, \"invalid-email\"))");
        PrintEitherResult("잘못된 이메일", eitherInvalidEmail);

        // 나이 제한 초과
        var eitherTooOld = RunPipelineWithEither(new UserInput("Frank", 150, "frank@test.com"));
        PrintEitherResult("나이 초과 (150)", eitherTooOld);

        // ============================================================
        // Part 5: 제네릭 파이프라인 컴포저
        // ============================================================
        MenuHelper.PrintSubHeader("Part 5: 제네릭 파이프라인 컴포저");

        MenuHelper.PrintExplanation("여러 단계를 조합하는 범용 파이프라인 컴포저입니다.");
        MenuHelper.PrintExplanation("K<M, A>와 Monad<M> 제약을 사용하여 어떤 모나드든 지원합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 파이프라인 컴포저 시그니처");
        MenuHelper.PrintCode("static K<M, D> ComposePipeline<M, A, B, C, D>(");
        MenuHelper.PrintCode("    K<M, A> input,");
        MenuHelper.PrintCode("    Func<A, K<M, B>> step1,");
        MenuHelper.PrintCode("    Func<B, K<M, C>> step2,");
        MenuHelper.PrintCode("    Func<C, K<M, D>> step3)");
        MenuHelper.PrintCode("    where M : Monad<M>");
        MenuHelper.PrintCode("    => input.Bind(step1).Bind(step2).Bind(step3);");
        MenuHelper.PrintBlankLines();

        // 제네릭 파이프라인 테스트
        var genericResult = TestGenericPipeline<Option>(Some(10));
        MenuHelper.PrintCode("TestGenericPipeline<Option>(Some(10))");
        MenuHelper.PrintResult("제네릭 파이프라인 결과", genericResult.As());

        // ============================================================
        // Part 6: 파이프라인 단계 로깅
        // ============================================================
        MenuHelper.PrintSubHeader("Part 6: 파이프라인 단계 로깅");

        MenuHelper.PrintExplanation("각 단계를 통과할 때 로깅을 수행하는 파이프라인입니다.");
        MenuHelper.PrintBlankLines();

        var loggedResult = RunLoggedPipeline(new UserInput("Grace", 35, "grace@example.com"));
        MenuHelper.PrintResult("로깅된 파이프라인 결과", loggedResult);

        // ============================================================
        // Part 7: 조건부 파이프라인 분기
        // ============================================================
        MenuHelper.PrintSubHeader("Part 7: 조건부 파이프라인 분기");

        MenuHelper.PrintExplanation("조건에 따라 다른 처리 경로를 선택하는 파이프라인입니다.");
        MenuHelper.PrintBlankLines();

        // 청소년 경로
        var teenResult = RunConditionalPipeline(new UserInput("Henry", 16, "henry@school.edu"));
        MenuHelper.PrintCode("Henry (16세) - 청소년 경로");
        MenuHelper.PrintResult("처리 결과", teenResult);

        // 성인 경로
        var adultResult = RunConditionalPipeline(new UserInput("Irene", 30, "irene@work.com"));
        MenuHelper.PrintCode("Irene (30세) - 성인 경로");
        MenuHelper.PrintResult("처리 결과", adultResult);

        // 시니어 경로
        var seniorResult = RunConditionalPipeline(new UserInput("John", 65, "john@retired.org"));
        MenuHelper.PrintCode("John (65세) - 시니어 경로");
        MenuHelper.PrintResult("처리 결과", seniorResult);

        // ============================================================
        // Part 8: 실습 과제
        // ============================================================
        MenuHelper.PrintSubHeader("Part 8: 실습 과제");

        MenuHelper.PrintExplanation("다음 과제를 직접 구현해 보세요:");
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("과제 1: 새로운 검증 규칙 추가");
        MenuHelper.PrintExplanation("  - 이름 길이가 2자 이상 20자 이하인지 검증");
        MenuHelper.PrintExplanation("  - 이메일에 특수 문자가 포함되지 않았는지 검증");
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("과제 2: 새로운 모나드 타입 지원");
        MenuHelper.PrintExplanation("  - Seq<UserInput>을 입력으로 받아 배치 처리");
        MenuHelper.PrintExplanation("  - 실패한 항목과 성공한 항목을 분리하여 반환");
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("과제 3: 비동기 파이프라인 확장");
        MenuHelper.PrintExplanation("  - 각 단계를 비동기로 실행하는 파이프라인 구현");
        MenuHelper.PrintExplanation("  - 외부 API 호출을 시뮬레이션하는 단계 추가");

        // ============================================================
        // Part 9: 핵심 정리
        // ============================================================
        MenuHelper.PrintSubHeader("Part 9: 핵심 정리");

        MenuHelper.PrintExplanation("추상 파이프라인의 장점:");
        MenuHelper.PrintExplanation("1. 재사용성: 동일한 로직을 Option, Either 등에서 재사용");
        MenuHelper.PrintExplanation("2. 조합 가능성: 작은 단계들을 조합하여 복잡한 파이프라인 구축");
        MenuHelper.PrintExplanation("3. 테스트 용이성: 각 단계를 독립적으로 테스트 가능");
        MenuHelper.PrintExplanation("4. 타입 안전성: 컴파일 타임에 타입 불일치 감지");
        MenuHelper.PrintExplanation("5. 확장성: 새로운 모나드 타입 추가가 용이");

        MenuHelper.PrintSuccess("실습 10 완료!");
    }

    // ============================================================
    // 데이터 모델
    // ============================================================

    private record UserInput(string Name, int Age, string Email);
    private record ValidatedUser(string Name, int Age, string Email);
    private record ProcessedUser(string DisplayName, string AgeGroup, bool IsActive);
    private record FormattedResult(string Summary);

    // ============================================================
    // Option 기반 파이프라인
    // ============================================================

    private static Option<FormattedResult> RunPipelineWithOption(UserInput input)
    {
        return ValidateUserOption(input)
            .Bind(TransformUser)
            .Bind(ProcessUser)
            .Map(FormatResult);
    }

    private static Option<ValidatedUser> ValidateUserOption(UserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) return None;
        if (input.Age < 0 || input.Age > 120) return None;
        if (!input.Email.Contains('@')) return None;

        return Some(new ValidatedUser(input.Name, input.Age, input.Email));
    }

    private static Option<ProcessedUser> TransformUser(ValidatedUser user)
    {
        var displayName = $"{user.Name} ({user.Age}세)";
        var ageGroup = user.Age switch
        {
            < 13 => "어린이",
            < 20 => "청소년",
            < 60 => "성인",
            _ => "시니어"
        };
        var isActive = user.Age >= 13 && user.Age < 65;

        return Some(new ProcessedUser(displayName, ageGroup, isActive));
    }

    private static Option<FormattedResult> ProcessUser(ProcessedUser user)
    {
        var status = user.IsActive ? "활성" : "비활성";
        return Some(new FormattedResult($"[{user.AgeGroup}] {user.DisplayName} - {status}"));
    }

    private static FormattedResult FormatResult(FormattedResult result) => result;

    // ============================================================
    // Either 기반 파이프라인
    // ============================================================

    private static Either<string, FormattedResult> RunPipelineWithEither(UserInput input)
    {
        return ValidateUserEither(input)
            .Bind(TransformUserEither)
            .Bind(ProcessUserEither);
    }

    private static Either<string, ValidatedUser> ValidateUserEither(UserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return Left("이름이 비어있습니다.");

        if (input.Age < 0)
            return Left("나이는 0 이상이어야 합니다.");

        if (input.Age > 120)
            return Left("나이가 너무 큽니다 (최대 120세).");

        if (!input.Email.Contains('@'))
            return Left("유효하지 않은 이메일 형식입니다.");

        return Right(new ValidatedUser(input.Name, input.Age, input.Email));
    }

    private static Either<string, ProcessedUser> TransformUserEither(ValidatedUser user)
    {
        var displayName = $"{user.Name} ({user.Age}세)";
        var ageGroup = user.Age switch
        {
            < 13 => "어린이",
            < 20 => "청소년",
            < 60 => "성인",
            _ => "시니어"
        };
        var isActive = user.Age >= 13 && user.Age < 65;

        return Right(new ProcessedUser(displayName, ageGroup, isActive));
    }

    private static Either<string, FormattedResult> ProcessUserEither(ProcessedUser user)
    {
        var status = user.IsActive ? "활성" : "비활성";
        return Right(new FormattedResult($"[{user.AgeGroup}] {user.DisplayName} - {status}"));
    }

    private static void PrintEitherResult(string label, Either<string, FormattedResult> result)
    {
        var formatted = result.Match(
            Right: r => $"성공: {r.Summary}",
            Left: e => $"실패: {e}"
        );
        MenuHelper.PrintResult(label, formatted);
    }

    // ============================================================
    // 제네릭 파이프라인 컴포저
    // ============================================================

    private static K<M, D> ComposePipeline<M, A, B, C, D>(
        K<M, A> input,
        Func<A, K<M, B>> step1,
        Func<B, K<M, C>> step2,
        Func<C, K<M, D>> step3)
        where M : Monad<M>
        => input.Bind(step1).Bind(step2).Bind(step3);

    private static K<M, string> TestGenericPipeline<M>(K<M, int> input)
        where M : Monad<M>
        => ComposePipeline<M, int, int, int, string>(
            input,
            x => M.Pure(x + 5),           // Step 1: 5 더하기
            x => M.Pure(x * 2),           // Step 2: 2 곱하기
            x => M.Pure($"결과: {x}")     // Step 3: 문자열 변환
        );

    // ============================================================
    // 로깅 파이프라인
    // ============================================================

    private static Option<FormattedResult> RunLoggedPipeline(UserInput input)
    {
        Console.WriteLine($"    [LOG] 파이프라인 시작: {input}");

        return ValidateUserOption(input)
            .Map(v => { Console.WriteLine($"    [LOG] 검증 통과: {v}"); return v; })
            .Bind(TransformUser)
            .Map(p => { Console.WriteLine($"    [LOG] 변환 완료: {p}"); return p; })
            .Bind(ProcessUser)
            .Map(r => { Console.WriteLine($"    [LOG] 처리 완료: {r}"); return r; });
    }

    // ============================================================
    // 조건부 파이프라인
    // ============================================================

    private static Option<string> RunConditionalPipeline(UserInput input)
    {
        return ValidateUserOption(input)
            .Bind(user => user.Age switch
            {
                < 20 => ProcessTeenager(user),
                < 60 => ProcessAdult(user),
                _ => ProcessSenior(user)
            });
    }

    private static Option<string> ProcessTeenager(ValidatedUser user)
        => Some($"[청소년 프로그램] {user.Name}님, 학생 할인이 적용됩니다!");

    private static Option<string> ProcessAdult(ValidatedUser user)
        => Some($"[성인 서비스] {user.Name}님, 모든 기능을 이용하실 수 있습니다.");

    private static Option<string> ProcessSenior(ValidatedUser user)
        => Some($"[시니어 케어] {user.Name}님, 우대 서비스가 제공됩니다.");
}
