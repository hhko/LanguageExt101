using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter13_HigherKinded;

/// <summary>
/// As() 확장 메서드의 사용법을 학습합니다.
///
/// 학습 목표:
/// - As() 메서드의 역할 이해
/// - K&lt;Option, A&gt; → Option&lt;A&gt; 변환
/// - K&lt;Either&lt;L&gt;, R&gt; → Either&lt;L, R&gt; 변환
/// - 제네릭 함수에서 As() 활용
/// - 실전 예제를 통한 활용법 학습
///
/// 핵심 개념:
/// As()는 Kind 타입(K&lt;F, A&gt;)을 실제 구체 타입으로 변환하는 브릿지입니다.
/// 제네릭 모나드 코드에서 구체 타입의 메서드에 접근할 때 필수적입니다.
/// </summary>
public static class E02_AsExtension
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 13-E02: As() 확장 메서드");

        // ============================================================
        // 1. 기본 As() 변환 - Option
        // ============================================================
        MenuHelper.PrintSubHeader("1. 기본 As() 변환 - Option");

        MenuHelper.PrintExplanation("K<Option, A>를 Option<A>로 변환합니다.");
        MenuHelper.PrintExplanation("변환 후에는 Option의 모든 메서드(Match, IsSome 등)를 사용할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // K<Option, int>로 시작
        K<Option, int> kindOption = Some(42);

        MenuHelper.PrintCode("K<Option, int> kindOption = Some(42);");
        MenuHelper.PrintResult("kindOption 타입", kindOption.GetType().Name);

        // As()로 변환
        Option<int> concreteOption = kindOption.As();

        MenuHelper.PrintCode("Option<int> concreteOption = kindOption.As();");
        MenuHelper.PrintResult("concreteOption", concreteOption);
        MenuHelper.PrintResult("concreteOption.IsSome", concreteOption.IsSome);

        // Match 메서드 사용 가능
        var matchResult = concreteOption.Match(
            Some: v => $"값: {v}",
            None: () => "값 없음"
        );
        MenuHelper.PrintCode("var matchResult = concreteOption.Match(Some: v => $\"값: {v}\", None: () => \"값 없음\");");
        MenuHelper.PrintResult("Match 결과", matchResult);

        // ============================================================
        // 2. 기본 As() 변환 - Either
        // ============================================================
        MenuHelper.PrintSubHeader("2. 기본 As() 변환 - Either");

        MenuHelper.PrintExplanation("K<Either<L>, R>를 Either<L, R>로 변환합니다.");
        MenuHelper.PrintExplanation("Either는 타입 인자가 2개이므로 K<Either<L>, R> 형태입니다.");
        MenuHelper.PrintBlankLines();

        // Either 생성
        Either<string, int> eitherValue = Right(100);
        K<Either<string>, int> kindEither = eitherValue;

        MenuHelper.PrintCode("Either<string, int> eitherValue = Right(100);");
        MenuHelper.PrintCode("K<Either<string>, int> kindEither = eitherValue;");

        // As()로 변환
        Either<string, int> concreteEither = kindEither.As();

        MenuHelper.PrintCode("Either<string, int> concreteEither = kindEither.As();");
        MenuHelper.PrintResult("concreteEither", concreteEither);
        MenuHelper.PrintResult("concreteEither.IsRight", concreteEither.IsRight);

        // ============================================================
        // 3. As()가 필요한 상황: 제네릭 함수에서 구체 타입 메서드 호출
        // ============================================================
        MenuHelper.PrintSubHeader("3. As()가 필요한 상황");

        MenuHelper.PrintExplanation("제네릭 모나드 함수에서 K<M, A>만으로는 구체 타입의 메서드에 접근할 수 없습니다.");
        MenuHelper.PrintExplanation("예: Map, Bind는 K<M, A>에서 사용 가능하지만, Match는 Option 전용입니다.");
        MenuHelper.PrintBlankLines();

        // 제네릭 함수 결과를 Option으로 사용하려면 As() 필요
        var doubled = DoubleIfPositive<Option>(Some(5));
        MenuHelper.PrintCode("var doubled = DoubleIfPositive<Option>(Some(5));");
        MenuHelper.PrintResult("doubled (K<Option, int>)", doubled);

        // As()로 변환하여 Match 사용
        var message = doubled.As().Match(
            Some: v => $"결과: {v}",
            None: () => "양수가 아님"
        );
        MenuHelper.PrintCode("var message = doubled.As().Match(Some: v => $\"결과: {v}\", None: () => \"양수가 아님\");");
        MenuHelper.PrintResult("Match 결과", message);

        // ============================================================
        // 4. 실전 예제: Option 결과 출력 유틸리티
        // ============================================================
        MenuHelper.PrintSubHeader("4. 실전 예제: Option 결과 출력 유틸리티");

        MenuHelper.PrintExplanation("제네릭 연산 결과를 사용자에게 보여주는 유틸리티입니다.");
        MenuHelper.PrintExplanation("K<Option, A>를 받아 As()로 변환 후 Match로 포맷팅합니다.");
        MenuHelper.PrintBlankLines();

        // 여러 Option 값 출력
        PrintOptionResult("Some(42)", Some(42));
        PrintOptionResult("None", Option<int>.None);
        PrintOptionResult("Some(\"Hello\")", Some("Hello"));

        // ============================================================
        // 5. 실전 예제: 로깅 래퍼 함수
        // ============================================================
        MenuHelper.PrintSubHeader("5. 실전 예제: 로깅 래퍼 함수");

        MenuHelper.PrintExplanation("제네릭 모나드 함수가 연산을 수행하고, As()로 변환 후 로깅합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// LogAndTransform: 제네릭 변환 후 결과 로깅");
        MenuHelper.PrintCode("var result = LogAndTransform(Some(10), x => x * 3);");

        var logResult = LogAndTransform(Some(10), x => x * 3);
        MenuHelper.PrintResult("최종 결과", logResult);

        // ============================================================
        // 6. As() 없이는 어떤 문제가 발생하는가?
        // ============================================================
        MenuHelper.PrintSubHeader("6. As() 없이는 어떤 문제가 발생하는가?");

        MenuHelper.PrintExplanation("K<Option, A>는 인터페이스이므로 Option<A>의 메서드를 직접 호출할 수 없습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 컴파일 에러 예시 (주석 처리됨):");
        MenuHelper.PrintCode("// K<Option, int> k = Some(1);");
        MenuHelper.PrintCode("// k.IsSome;  // 에러! K<Option, int>에는 IsSome이 없음");
        MenuHelper.PrintCode("// k.Match(...);  // 에러! K<Option, int>에는 Match가 없음");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("해결책: As()를 사용하여 구체 타입으로 변환합니다.");
        MenuHelper.PrintCode("// K<Option, int> k = Some(1);");
        MenuHelper.PrintCode("// k.As().IsSome;  // OK!");
        MenuHelper.PrintCode("// k.As().Match(...);  // OK!");
        MenuHelper.PrintBlankLines();

        // 실제 예시
        K<Option, string> kindStr = Some("테스트");
        // kindStr.IsSome;  // 컴파일 에러
        var isSome = kindStr.As().IsSome;  // OK!
        MenuHelper.PrintResult("kindStr.As().IsSome", isSome);

        // ============================================================
        // 7. 체이닝에서 As() 활용
        // ============================================================
        MenuHelper.PrintSubHeader("7. 체이닝에서 As() 활용");

        MenuHelper.PrintExplanation("제네릭 연산 체인의 마지막에 As()를 호출하여 구체 타입으로 마무리합니다.");
        MenuHelper.PrintBlankLines();

        var chainResult = ProcessValue<Option>(Some(5))
            .Map(x => x + 10)
            .Map(x => x * 2)
            .As()  // K<Option, int> → Option<int>
            .Match(
                Some: v => $"처리 완료: {v}",
                None: () => "처리 실패"
            );

        MenuHelper.PrintCode("var chainResult = ProcessValue<Option>(Some(5))");
        MenuHelper.PrintCode("    .Map(x => x + 10)");
        MenuHelper.PrintCode("    .Map(x => x * 2)");
        MenuHelper.PrintCode("    .As()  // K<Option, int> → Option<int>");
        MenuHelper.PrintCode("    .Match(Some: v => $\"처리 완료: {v}\", None: () => \"처리 실패\");");
        MenuHelper.PrintResult("체이닝 결과", chainResult);

        // ============================================================
        // 8. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("8. 정리");

        MenuHelper.PrintExplanation("As() 메서드 핵심 정리:");
        MenuHelper.PrintExplanation("1. K<F, A>를 구체 타입(Option<A>, Either<L, R> 등)으로 변환합니다.");
        MenuHelper.PrintExplanation("2. 제네릭 모나드 함수에서 구체 타입 메서드가 필요할 때 사용합니다.");
        MenuHelper.PrintExplanation("3. Map, Bind 같은 공통 연산은 K<M, A>에서 직접 사용 가능합니다.");
        MenuHelper.PrintExplanation("4. Match, IsSome 같은 타입별 메서드는 As() 변환 후 사용합니다.");
        MenuHelper.PrintExplanation("5. 체이닝 마지막에 As()를 호출하는 패턴이 일반적입니다.");

        MenuHelper.PrintSuccess("As() 확장 메서드 학습 완료!");
    }

    /// <summary>
    /// 제네릭 모나드 함수: 양수이면 2배, 아니면 실패
    /// </summary>
    private static K<M, int> DoubleIfPositive<M>(K<M, int> ma)
        where M : Monad<M>
        => ma.Bind(x => x > 0
            ? M.Pure(x * 2)
            : M.Pure(0).Bind(_ => M.Pure(0)));  // 단순화된 예시

    /// <summary>
    /// Option 결과를 포맷팅하여 출력합니다.
    /// K<Option, A>를 받아 As()로 변환하여 Match를 사용합니다.
    /// </summary>
    private static void PrintOptionResult<A>(string label, K<Option, A> optionK)
    {
        var option = optionK.As();  // K<Option, A> → Option<A>

        var formatted = option.Match(
            Some: v => $"Some({v})",
            None: () => "None"
        );

        MenuHelper.PrintResult(label, formatted);
    }

    /// <summary>
    /// 제네릭 변환을 수행하고 결과를 로깅합니다.
    /// </summary>
    private static Option<B> LogAndTransform<A, B>(K<Option, A> input, Func<A, B> transform)
    {
        Console.WriteLine($"    [LOG] 입력 값 처리 시작");

        var result = input.Map(transform).As();  // As()로 변환

        result.Match(
            Some: v => Console.WriteLine($"    [LOG] 변환 성공: {v}"),
            None: () => Console.WriteLine("    [LOG] 값 없음")
        );

        return result;
    }

    /// <summary>
    /// 제네릭 값 처리 함수 예시
    /// </summary>
    private static K<M, int> ProcessValue<M>(K<M, int> ma)
        where M : Functor<M>
        => ma.Map(x => x);  // 단순히 값을 그대로 반환
}
