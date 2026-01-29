using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter12_Traits;

/// <summary>
/// Language-Ext Traits 시스템의 개요를 학습합니다.
///
/// 학습 목표:
/// - Traits란 무엇이고 왜 필요한가?
/// - Traits 계층 구조 이해 (Functor, Applicative, Monad 등)
/// - K&lt;F, A&gt;와 Traits의 관계
/// - 제네릭 함수에서 Trait 제약 사용법
///
/// 핵심 개념:
/// Traits는 Haskell의 Type Classes에서 영감을 받은 개념입니다.
/// C#에서 Higher-Kinded Types를 시뮬레이션하여
/// 여러 타입에서 동작하는 범용 함수를 작성할 수 있게 합니다.
/// </summary>
public static class E01_TraitsOverview
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 12-E01: Traits 시스템 개요");

        // ============================================================
        // 1. Traits란 무엇인가?
        // ============================================================
        MenuHelper.PrintSubHeader("1. Traits란 무엇인가?");

        MenuHelper.PrintExplanation("Traits는 타입이 가진 '능력(capability)'을 정의합니다.");
        MenuHelper.PrintExplanation("Haskell의 Type Classes와 유사한 개념입니다.");
        MenuHelper.PrintExplanation("예: 'Map 가능', 'Bind 가능', 'Fold 가능' 등");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Trait 예시");
        MenuHelper.PrintCode("// Functor<M>  - Map 연산 가능");
        MenuHelper.PrintCode("// Monad<M>    - Bind 연산 가능");
        MenuHelper.PrintCode("// Foldable<T> - Fold 연산 가능");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Option, Either, Seq 등은 모두 이러한 Traits를 구현합니다.");
        MenuHelper.PrintExplanation("덕분에 '모든 Functor에서 동작하는 함수'를 작성할 수 있습니다.");

        // ============================================================
        // 2. 왜 Traits가 필요한가?
        // ============================================================
        MenuHelper.PrintSubHeader("2. 왜 Traits가 필요한가?");

        MenuHelper.PrintExplanation("문제: Option과 Either에서 동작하는 '같은 로직'을 두 번 작성해야 함");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Traits 없이: 각 타입별로 함수 작성");
        MenuHelper.PrintCode("Option<int> DoubleOption(Option<int> opt) => opt.Map(x => x * 2);");
        MenuHelper.PrintCode("Either<E, int> DoubleEither(Either<E, int> e) => e.Map(x => x * 2);");
        MenuHelper.PrintCode("Seq<int> DoubleSeq(Seq<int> seq) => seq.Map(x => x * 2);");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Traits 사용: 한 번만 작성!");
        MenuHelper.PrintCode("K<M, int> Double<M>(K<M, int> ma) where M : Functor<M>");
        MenuHelper.PrintCode("    => ma.Map(x => x * 2);");
        MenuHelper.PrintBlankLines();

        // 실제 동작 확인
        var optDouble = Double<Option>(Some(5));
        var seqDouble = Double<Seq>(Seq(1, 2, 3));

        MenuHelper.PrintResult("Double<Option>(Some(5))", optDouble.As());
        MenuHelper.PrintResult("Double<Seq>(Seq(1,2,3))", seqDouble.As());

        // ============================================================
        // 3. Traits 계층 구조
        // ============================================================
        MenuHelper.PrintSubHeader("3. Traits 계층 구조");

        MenuHelper.PrintExplanation("Traits는 계층 구조를 가집니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("              Functor");
        MenuHelper.PrintCode("                 |");
        MenuHelper.PrintCode("            Applicative");
        MenuHelper.PrintCode("                 |");
        MenuHelper.PrintCode("              Monad  ----  Alternative");
        MenuHelper.PrintCode("");
        MenuHelper.PrintCode("  Foldable  ----  Traversable");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("각 Trait의 핵심 연산:");
        MenuHelper.PrintCode("Functor<M>     : Map        - 컨텍스트 안의 값 변환");
        MenuHelper.PrintCode("Applicative<M> : Pure, Apply - 여러 값 조합");
        MenuHelper.PrintCode("Monad<M>       : Bind       - 순차적 체이닝");
        MenuHelper.PrintCode("Alternative<M> : Choose     - 실패 시 대안");
        MenuHelper.PrintCode("Foldable<T>    : Fold       - 값 축적");
        MenuHelper.PrintCode("Traversable<T> : Traverse   - 구조 유지하며 이펙트 적용");

        // ============================================================
        // 4. K<F, A>란 무엇인가?
        // ============================================================
        MenuHelper.PrintSubHeader("4. K<F, A>란 무엇인가?");

        MenuHelper.PrintExplanation("K<F, A>는 Higher-Kinded Type을 C#에서 표현하는 방법입니다.");
        MenuHelper.PrintExplanation("'타입 생성자 F에 타입 A를 적용한 것'을 의미합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// K<F, A>의 의미");
        MenuHelper.PrintCode("K<Option, int>    // Option<int>의 추상화");
        MenuHelper.PrintCode("K<Seq, string>    // Seq<string>의 추상화");
        MenuHelper.PrintCode("K<Either<Error>, int> // Either<Error, int>의 추상화");
        MenuHelper.PrintBlankLines();

        // K<F, A> 사용 예시
        K<Option, int> kindOpt = Some(100);  // Option<int>를 K<Option, int>로
        K<Seq, int> kindSeq = Seq(1, 2, 3);  // Seq<int>를 K<Seq, int>로

        MenuHelper.PrintCode("K<Option, int> kindOpt = Some(100);");
        MenuHelper.PrintCode("K<Seq, int> kindSeq = Seq(1, 2, 3);");
        MenuHelper.PrintResult("kindOpt", kindOpt);
        MenuHelper.PrintResult("kindSeq", kindSeq);

        // ============================================================
        // 5. As() 메서드로 구체 타입 변환
        // ============================================================
        MenuHelper.PrintSubHeader("5. As() 메서드로 구체 타입 변환");

        MenuHelper.PrintExplanation("K<F, A>는 추상 타입이므로 구체 타입 메서드에 접근할 수 없습니다.");
        MenuHelper.PrintExplanation("As()를 사용하면 원래 타입으로 변환할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// K<F, A>에서는 구체 메서드 접근 불가");
        MenuHelper.PrintCode("// kindOpt.IsSome;  // 컴파일 에러!");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// As()로 구체 타입 변환 후 접근 가능");
        MenuHelper.PrintCode("Option<int> concrete = kindOpt.As();");
        MenuHelper.PrintCode("concrete.IsSome;  // OK!");
        MenuHelper.PrintBlankLines();

        Option<int> concreteOpt = kindOpt.As();
        Seq<int> concreteSeq = kindSeq.As();
        MenuHelper.PrintResult("concreteOpt.IsSome", concreteOpt.IsSome);
        MenuHelper.PrintResult("concreteSeq.Count", concreteSeq.Count);

        // ============================================================
        // 6. Trait 제약을 사용한 제네릭 함수
        // ============================================================
        MenuHelper.PrintSubHeader("6. Trait 제약을 사용한 제네릭 함수");

        MenuHelper.PrintExplanation("where M : Trait<M> 형태로 제약을 걸어 범용 함수를 작성합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Functor 제약: Map만 사용");
        MenuHelper.PrintCode("static K<M, string> Stringify<M>(K<M, int> ma) where M : Functor<M>");
        MenuHelper.PrintCode("    => ma.Map(x => x.ToString());");
        MenuHelper.PrintBlankLines();

        var optStr = Stringify<Option>(Some(42));
        var seqStr = Stringify<Seq>(Seq(1, 2, 3));
        MenuHelper.PrintResult("Stringify<Option>(Some(42))", optStr.As());
        MenuHelper.PrintResult("Stringify<Seq>(Seq(1,2,3))", seqStr.As());

        // ============================================================
        // 7. 제약 선택 가이드
        // ============================================================
        MenuHelper.PrintSubHeader("7. 제약 선택 가이드");

        MenuHelper.PrintExplanation("가능한 가장 약한 제약을 사용하면 재사용성이 높아집니다!");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 제약 강도: Functor < Applicative < Monad");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Functor<M> 사용:");
        MenuHelper.PrintExplanation("  - Map만 필요할 때 (단순 값 변환)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Applicative<M> 사용:");
        MenuHelper.PrintExplanation("  - 여러 독립적 값을 조합할 때");
        MenuHelper.PrintExplanation("  - 병렬 검증, 에러 누적");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Monad<M> 사용:");
        MenuHelper.PrintExplanation("  - 이전 결과에 다음 연산이 의존할 때");
        MenuHelper.PrintExplanation("  - 순차적 처리, 조건부 분기");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Alternative<M> 사용:");
        MenuHelper.PrintExplanation("  - 실패 시 대안을 제공할 때");
        MenuHelper.PrintExplanation("  - 폴백 패턴");

        // ============================================================
        // 8. 실전 예제: 범용 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 범용 파이프라인");

        MenuHelper.PrintExplanation("Traits를 활용한 범용 파이프라인 예제입니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 세 단계 파이프라인: 검증 -> 변환 -> 포맷");
        MenuHelper.PrintCode("static K<M, string> Process<M>(K<M, int> input) where M : Monad<M>");
        MenuHelper.PrintCode("    => input.Bind(Validate).Bind(Transform).Map(Format);");
        MenuHelper.PrintBlankLines();

        var processedOpt = ProcessPipeline(Some(10));
        var processedOptNone = ProcessPipeline(Option<int>.None);
        MenuHelper.PrintResult("Process(Some(10))", processedOpt);
        MenuHelper.PrintResult("Process(None)", processedOptNone);

        // ============================================================
        // 9. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("9. 정리");

        MenuHelper.PrintExplanation("Traits는 타입의 '능력'을 추상화합니다.");
        MenuHelper.PrintExplanation("K<F, A>는 Higher-Kinded Type을 표현하는 래퍼입니다.");
        MenuHelper.PrintExplanation("Trait 제약으로 여러 타입에서 동작하는 범용 함수를 작성합니다.");
        MenuHelper.PrintExplanation("가능한 가장 약한 제약을 사용하여 재사용성을 높이세요.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("다음 챕터에서 각 Trait를 상세히 학습합니다:");
        MenuHelper.PrintExplanation("- E02: Functor와 Applicative");
        MenuHelper.PrintExplanation("- E03: Monad와 Alternative");
        MenuHelper.PrintExplanation("- E04: Foldable과 Traversable");

        MenuHelper.PrintSuccess("Traits 시스템 개요 학습 완료!");
    }

    // ============================================================
    // 헬퍼 함수들
    // ============================================================

    /// <summary>
    /// 값을 두 배로 만드는 범용 함수 (Functor)
    /// </summary>
    private static K<M, int> Double<M>(K<M, int> ma)
        where M : Functor<M>
        => ma.Map(x => x * 2);

    /// <summary>
    /// 정수를 문자열로 변환하는 범용 함수 (Functor)
    /// </summary>
    private static K<M, string> Stringify<M>(K<M, int> ma)
        where M : Functor<M>
        => ma.Map(x => x.ToString());

    /// <summary>
    /// 세 단계 파이프라인 (Option 전용)
    /// </summary>
    private static Option<string> ProcessPipeline(Option<int> input)
        => input
            .Bind(x => x > 0 ? Some(x) : Option<int>.None)  // 검증
            .Bind(x => Some(x * 10))                         // 변환
            .Map(x => $"결과: {x}");                          // 포맷
}
