using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter13_HigherKinded;

/// <summary>
/// K&lt;F, A&gt;를 활용한 범용 알고리즘 작성법을 학습합니다.
///
/// 학습 목표:
/// - Functor, Applicative, Monad trait을 활용한 제네릭 함수 작성
/// - 여러 모나드 타입에서 동작하는 유틸리티 함수 구현
/// - 타입 클래스 제약을 통한 다형성 구현
/// - 실전에서 활용할 수 있는 범용 패턴 학습
///
/// 핵심 개념:
/// Higher-Kinded Types를 사용하면 Option, Either, Seq 등
/// 다양한 모나드에 대해 동일한 알고리즘을 한 번만 작성할 수 있습니다.
/// 이를 통해 코드 재사용성과 추상화 수준을 높일 수 있습니다.
/// </summary>
public static class E03_GenericAlgorithms
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 13-E03: 범용 알고리즘");

        // ============================================================
        // 1. Functor를 활용한 범용 Map 유틸리티
        // ============================================================
        MenuHelper.PrintSubHeader("1. Functor를 활용한 범용 Map 유틸리티");

        MenuHelper.PrintExplanation("Functor<M> 제약을 사용하면 Map을 지원하는 모든 타입에서 동작합니다.");
        MenuHelper.PrintExplanation("Option, Either, Seq 등 어떤 Functor든 사용 가능합니다.");
        MenuHelper.PrintBlankLines();

        // 범용 함수 정의 설명
        MenuHelper.PrintCode("static K<M, B> MapTwice<M, A, B>(K<M, A> ma, Func<A, B> f) where M : Functor<M>");
        MenuHelper.PrintCode("    => ma.Map(f).Map(x => x);  // 두 번 Map 적용 (예시)");
        MenuHelper.PrintBlankLines();

        // Option에 적용
        var optResult = IncrementAndDouble<Option>(Some(5));
        MenuHelper.PrintCode("var optResult = IncrementAndDouble<Option>(Some(5));");
        MenuHelper.PrintResult("Option 결과", optResult.As());

        // Seq에 적용
        var seqResult = IncrementAndDouble<Seq>(Seq(1, 2, 3));
        MenuHelper.PrintCode("var seqResult = IncrementAndDouble<Seq>(Seq(1, 2, 3));");
        MenuHelper.PrintResult("Seq 결과", seqResult.As());

        // ============================================================
        // 2. Applicative를 활용한 범용 조합 함수
        // ============================================================
        MenuHelper.PrintSubHeader("2. Applicative를 활용한 범용 조합 함수");

        MenuHelper.PrintExplanation("Applicative<M>는 여러 값을 조합하는 기능을 제공합니다.");
        MenuHelper.PrintExplanation("Apply를 사용하여 컨텍스트 안의 함수를 컨텍스트 안의 값에 적용할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // 두 값을 조합하는 범용 함수
        MenuHelper.PrintCode("static K<M, C> Combine<M, A, B, C>(K<M, A> ma, K<M, B> mb, Func<A, B, C> f)");
        MenuHelper.PrintCode("    where M : Applicative<M>");
        MenuHelper.PrintBlankLines();

        // Option에서 두 값 조합
        var combinedOpt = CombineValues<Option, int, int, int>(Some(10), Some(20), (a, b) => a + b);
        MenuHelper.PrintCode("var combinedOpt = CombineValues<Option>(Some(10), Some(20), (a, b) => a + b);");
        MenuHelper.PrintResult("Option 조합 결과", combinedOpt.As());

        // None이 있으면 결과도 None
        var combinedWithNone = CombineValues<Option, int, int, int>(Some(10), Option<int>.None, (a, b) => a + b);
        MenuHelper.PrintResult("Some + None 조합", combinedWithNone.As());

        // ============================================================
        // 3. Monad를 활용한 범용 체이닝 함수
        // ============================================================
        MenuHelper.PrintSubHeader("3. Monad를 활용한 범용 체이닝 함수");

        MenuHelper.PrintExplanation("Monad<M>는 Bind(FlatMap)를 제공하여 순차적 연산 체이닝이 가능합니다.");
        MenuHelper.PrintExplanation("이를 통해 의존적인 연산들을 순서대로 실행할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // 범용 체이닝 함수
        MenuHelper.PrintCode("static K<M, C> Chain<M, A, B, C>(K<M, A> ma, Func<A, K<M, B>> f, Func<B, K<M, C>> g)");
        MenuHelper.PrintCode("    where M : Monad<M>");
        MenuHelper.PrintCode("    => ma.Bind(f).Bind(g);");
        MenuHelper.PrintBlankLines();

        // Option에서 체이닝
        var chainResult = ChainOperations<Option, int, int, string>(
            Some(5),
            x => Some(x * 2),
            x => Some($"결과: {x}")
        );
        MenuHelper.PrintCode("ChainOperations<Option>(Some(5), x => Some(x * 2), x => Some($\"결과: {x}\"))");
        MenuHelper.PrintResult("체이닝 결과", chainResult.As());

        // ============================================================
        // 4. 범용 조건부 실행 함수
        // ============================================================
        MenuHelper.PrintSubHeader("4. 범용 조건부 실행 함수");

        MenuHelper.PrintExplanation("조건에 따라 다른 모나드 연산을 실행하는 범용 함수입니다.");
        MenuHelper.PrintExplanation("WhenTrue/WhenFalse 패턴으로 분기 처리를 추상화합니다.");
        MenuHelper.PrintBlankLines();

        // 조건부 실행
        var whenTrue = ConditionalExecute<Option, int>(
            Some(10),
            x => x > 5,
            x => Some(x * 100),  // 조건 참일 때
            x => Some(x)         // 조건 거짓일 때
        );
        MenuHelper.PrintCode("ConditionalExecute<Option>(Some(10), x => x > 5, x => Some(x * 100), x => Some(x))");
        MenuHelper.PrintResult("10 > 5 → 참", whenTrue.As());

        var whenFalse = ConditionalExecute<Option, int>(
            Some(3),
            x => x > 5,
            x => Some(x * 100),
            x => Some(x)
        );
        MenuHelper.PrintResult("3 > 5 → 거짓", whenFalse.As());

        // ============================================================
        // 5. 범용 기본값 제공 함수
        // ============================================================
        MenuHelper.PrintSubHeader("5. 범용 기본값 제공 함수");

        MenuHelper.PrintExplanation("Alternative<M> trait을 사용하면 '실패' 시 대안을 제공할 수 있습니다.");
        MenuHelper.PrintExplanation("Option의 경우 None일 때 기본값을 사용합니다.");
        MenuHelper.PrintBlankLines();

        // Alternative 사용
        var withDefault = WithFallback(Option<int>.None, Some(42));
        MenuHelper.PrintCode("var withDefault = WithFallback(Option<int>.None, Some(42));");
        MenuHelper.PrintResult("None에 기본값 42", withDefault.As());

        var noNeedDefault = WithFallback(Some(100), Some(42));
        MenuHelper.PrintResult("Some(100)은 그대로", noNeedDefault.As());

        // ============================================================
        // 6. 범용 로깅/디버깅 래퍼
        // ============================================================
        MenuHelper.PrintSubHeader("6. 범용 로깅/디버깅 래퍼");

        MenuHelper.PrintExplanation("모나드 연산 전후에 로깅을 삽입하는 범용 패턴입니다.");
        MenuHelper.PrintExplanation("디버깅이나 모니터링에 유용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Tap: 값을 확인하되 변경하지 않음");
        var tapped = Tap<Option, int>(Some(42), x => Console.WriteLine($"    [TAP] 현재 값: {x}"));
        MenuHelper.PrintResult("Tap 후 값", tapped.As());

        // ============================================================
        // 7. 범용 변환 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("7. 범용 변환 파이프라인");

        MenuHelper.PrintExplanation("여러 변환 함수를 순차적으로 적용하는 파이프라인입니다.");
        MenuHelper.PrintExplanation("함수 합성과 Map을 조합하여 구현합니다.");
        MenuHelper.PrintBlankLines();

        // 파이프라인 구성
        var pipeline = BuildPipeline<int>(
            x => x + 1,      // 1 더하기
            x => x * 2,      // 2 곱하기
            x => x - 3       // 3 빼기
        );

        var pipelineResult = Some(10).Map(pipeline);
        MenuHelper.PrintCode("pipeline: x => ((x + 1) * 2) - 3");
        MenuHelper.PrintCode("Some(10).Map(pipeline)");
        MenuHelper.PrintResult("파이프라인 결과", pipelineResult);  // (10 + 1) * 2 - 3 = 19

        // ============================================================
        // 8. 실전 예제: 범용 유효성 검사
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 범용 유효성 검사");

        MenuHelper.PrintExplanation("여러 검증 규칙을 모나드적으로 체이닝하는 패턴입니다.");
        MenuHelper.PrintExplanation("모든 검증을 통과해야 최종 결과를 얻습니다.");
        MenuHelper.PrintBlankLines();

        // 유효성 검사 체인
        var validInput = ValidateAndProcess("hello123");
        var invalidInput = ValidateAndProcess("");

        MenuHelper.PrintCode("ValidateAndProcess(\"hello123\")");
        MenuHelper.PrintResult("유효한 입력", validInput);

        MenuHelper.PrintCode("ValidateAndProcess(\"\")");
        MenuHelper.PrintResult("빈 입력", invalidInput);

        // ============================================================
        // 9. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("9. 정리");

        MenuHelper.PrintExplanation("범용 알고리즘 작성의 핵심:");
        MenuHelper.PrintExplanation("1. Functor<M> - Map만 필요할 때 (가장 약한 제약)");
        MenuHelper.PrintExplanation("2. Applicative<M> - 여러 값 조합이 필요할 때");
        MenuHelper.PrintExplanation("3. Monad<M> - 순차적 체이닝이 필요할 때 (가장 강한 제약)");
        MenuHelper.PrintExplanation("4. 가능한 가장 약한 제약을 사용하여 재사용성을 높입니다.");
        MenuHelper.PrintExplanation("5. As()로 구체 타입 변환 후 타입별 메서드를 사용합니다.");

        MenuHelper.PrintSuccess("범용 알고리즘 학습 완료!");
    }

    // ============================================================
    // 범용 함수 정의들
    // ============================================================

    /// <summary>
    /// 값을 1 증가시킨 후 2배로 만드는 범용 함수
    /// </summary>
    private static K<M, int> IncrementAndDouble<M>(K<M, int> ma)
        where M : Functor<M>
        => ma.Map(x => x + 1).Map(x => x * 2);

    /// <summary>
    /// 두 값을 조합하는 범용 함수 (Applicative 사용)
    /// </summary>
    private static K<M, C> CombineValues<M, A, B, C>(K<M, A> ma, K<M, B> mb, Func<A, B, C> f)
        where M : Applicative<M>
        => ma.Map(curry(f)).Apply(mb);

    /// <summary>
    /// 순차적 연산 체이닝 (Monad 사용)
    /// </summary>
    private static K<M, C> ChainOperations<M, A, B, C>(
        K<M, A> ma,
        Func<A, K<M, B>> f,
        Func<B, K<M, C>> g)
        where M : Monad<M>
        => ma.Bind(f).Bind(g);

    /// <summary>
    /// 조건에 따른 분기 실행
    /// </summary>
    private static K<M, A> ConditionalExecute<M, A>(
        K<M, A> ma,
        Func<A, bool> predicate,
        Func<A, K<M, A>> whenTrue,
        Func<A, K<M, A>> whenFalse)
        where M : Monad<M>
        => ma.Bind(x => predicate(x) ? whenTrue(x) : whenFalse(x));

    /// <summary>
    /// 첫 번째가 실패하면 두 번째를 사용 (Alternative)
    /// </summary>
    private static K<M, A> WithFallback<M, A>(K<M, A> primary, K<M, A> fallback)
        where M : Alternative<M>
        => primary.Choose(fallback);

    /// <summary>
    /// 값을 확인하되 변경하지 않는 Tap 함수
    /// </summary>
    private static K<M, A> Tap<M, A>(K<M, A> ma, Action<A> action)
        where M : Functor<M>
        => ma.Map(x =>
        {
            action(x);
            return x;
        });

    /// <summary>
    /// 여러 함수를 합성하여 파이프라인 생성
    /// </summary>
    private static Func<A, A> BuildPipeline<A>(params Func<A, A>[] funcs)
        => funcs.Aggregate((f, g) => x => g(f(x)));

    /// <summary>
    /// 유효성 검사 후 처리하는 예제
    /// </summary>
    private static Option<string> ValidateAndProcess(string input)
    {
        return ValidateNotEmpty(input)
            .Bind(ValidateMinLength)
            .Bind(ValidateHasDigit)
            .Map(s => s.ToUpper());
    }

    private static Option<string> ValidateNotEmpty(string s)
        => string.IsNullOrEmpty(s) ? None : Some(s);

    private static Option<string> ValidateMinLength(string s)
        => s.Length >= 5 ? Some(s) : None;

    private static Option<string> ValidateHasDigit(string s)
        => s.Any(char.IsDigit) ? Some(s) : None;
}
