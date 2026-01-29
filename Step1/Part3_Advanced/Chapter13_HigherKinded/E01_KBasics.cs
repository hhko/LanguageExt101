using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter13_HigherKinded;

/// <summary>
/// K&lt;F, A&gt; (Kind 타입) 기본 개념을 학습합니다.
///
/// 학습 목표:
/// - K&lt;F, A&gt;가 무엇인지 이해
/// - 왜 Kind 타입이 필요한지 이해
/// - Option이 K&lt;Option, A&gt;를 구현하는 방식 이해
/// - 제네릭 모나드 함수 작성 기초
///
/// 핵심 개념:
/// Higher-Kinded Types(HKT)는 타입 생성자를 추상화하는 패턴입니다.
/// C#은 HKT를 네이티브로 지원하지 않지만, language-ext는
/// K&lt;F, A&gt; 인터페이스를 통해 이를 시뮬레이션합니다.
///
/// 이를 통해 Option, Either, List 등 다양한 모나드에 대해
/// 동작하는 제네릭 함수를 작성할 수 있습니다.
/// </summary>
public static class E01_KBasics
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 13-E01: K<F, A> 기본 개념");

        // ============================================================
        // 1. K<F, A>란 무엇인가?
        // ============================================================
        MenuHelper.PrintSubHeader("1. K<F, A>란 무엇인가?");

        MenuHelper.PrintExplanation("K<F, A>는 'Kind'의 약자로, 타입 생성자 F와 타입 인자 A를 가진 타입을 나타냅니다.");
        MenuHelper.PrintExplanation("예: K<Option, int>는 Option<int>를 추상화한 것입니다.");
        MenuHelper.PrintExplanation("이를 통해 Option, Either, Seq 등을 하나의 인터페이스로 다룰 수 있습니다.");
        MenuHelper.PrintBlankLines();

        // Option<int>는 K<Option, int>를 구현합니다
        Option<int> optionValue = Some(42);

        // K<Option, int> 타입으로 할당 가능 (업캐스팅)
        K<Option, int> kindValue = optionValue;

        MenuHelper.PrintCode("Option<int> optionValue = Some(42);");
        MenuHelper.PrintCode("K<Option, int> kindValue = optionValue;  // 업캐스팅");
        MenuHelper.PrintResult("optionValue", optionValue);
        MenuHelper.PrintResult("kindValue (K<Option, int>)", kindValue);

        // ============================================================
        // 2. 왜 Kind 타입이 필요한가?
        // ============================================================
        MenuHelper.PrintSubHeader("2. 왜 Kind 타입이 필요한가?");

        MenuHelper.PrintExplanation("문제: C#에서는 '모나드'라는 개념을 직접 표현할 수 없습니다.");
        MenuHelper.PrintExplanation("Option<T>, Either<L, R>, Seq<T>는 모두 모나드이지만,");
        MenuHelper.PrintExplanation("'아무 모나드나 받는 함수'를 일반적인 방법으로는 작성할 수 없습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("해결: K<F, A>를 사용하면 모나드를 추상화할 수 있습니다.");
        MenuHelper.PrintExplanation("Monad<M> trait과 함께 사용하여 제네릭 모나드 함수를 작성합니다.");
        MenuHelper.PrintBlankLines();

        // 예시: 제네릭하게 동작하는 코드
        MenuHelper.PrintCode("// 제네릭 모나드 함수 시그니처 예시");
        MenuHelper.PrintCode("// static K<M, B> Transform<M, A, B>(K<M, A> ma, Func<A, B> f) where M : Monad<M>");

        // ============================================================
        // 3. Option이 K<Option, A>를 구현하는 방식
        // ============================================================
        MenuHelper.PrintSubHeader("3. Option이 K<Option, A>를 구현하는 방식");

        MenuHelper.PrintExplanation("Option<A>는 K<Option, A> 인터페이스를 구현합니다.");
        MenuHelper.PrintExplanation("이를 통해 Option을 제네릭 모나드 함수에 전달할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// language-ext 내부 구조 (개념적)");
        MenuHelper.PrintCode("// public readonly struct Option<A> : K<Option, A> { ... }");
        MenuHelper.PrintBlankLines();

        // 실제 사용 예시
        var opt1 = Some(10);
        var opt2 = Some(20);

        MenuHelper.PrintCode("var opt1 = Some(10);");
        MenuHelper.PrintCode("var opt2 = Some(20);");

        // K<Option, int>로 다룰 수 있음
        K<Option, int> k1 = opt1;
        K<Option, int> k2 = opt2;

        MenuHelper.PrintResult("k1 (K<Option, int>)", k1);
        MenuHelper.PrintResult("k2 (K<Option, int>)", k2);

        // ============================================================
        // 4. 제네릭 모나드 함수 작성하기
        // ============================================================
        MenuHelper.PrintSubHeader("4. 제네릭 모나드 함수 작성하기");

        MenuHelper.PrintExplanation("K<M, A>와 Monad<M> 제약을 사용하여 제네릭 함수를 작성합니다.");
        MenuHelper.PrintExplanation("이 함수는 Option, Either, Seq 등 모든 모나드에서 동작합니다.");
        MenuHelper.PrintBlankLines();

        // 제네릭 함수 정의 (아래 메서드 참조)
        MenuHelper.PrintCode("static K<M, int> DoubleValue<M>(K<M, int> ma) where M : Functor<M>");
        MenuHelper.PrintCode("    => ma.Map(x => x * 2);");
        MenuHelper.PrintBlankLines();

        // Option으로 테스트
        var optionResult = DoubleValue<Option>(Some(21));
        MenuHelper.PrintResult("DoubleValue<Option>(Some(21))", optionResult);

        // Seq로 테스트
        var seqResult = DoubleValue<Seq>(Seq(1, 2, 3));
        MenuHelper.PrintResult("DoubleValue<Seq>(Seq(1, 2, 3))", seqResult);

        // ============================================================
        // 5. Kind 타입의 한계와 As() 메서드
        // ============================================================
        MenuHelper.PrintSubHeader("5. Kind 타입의 한계와 As() 메서드");

        MenuHelper.PrintExplanation("K<F, A>는 추상화된 타입이므로 구체 타입의 메서드에 직접 접근할 수 없습니다.");
        MenuHelper.PrintExplanation("예: K<Option, int>에서 Option<int>의 Match 메서드를 바로 호출할 수 없습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("이때 As() 확장 메서드를 사용하여 구체 타입으로 변환합니다.");
        MenuHelper.PrintExplanation("자세한 내용은 다음 예제(E02_AsExtension)에서 학습합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("K<Option, int> kindOpt = Some(100);");
        MenuHelper.PrintCode("Option<int> concreteOpt = kindOpt.As();  // 구체 타입으로 변환");

        K<Option, int> kindOpt = Some(100);
        Option<int> concreteOpt = kindOpt.As();

        MenuHelper.PrintResult("kindOpt.As()", concreteOpt);
        MenuHelper.PrintResult("concreteOpt.IsSome", concreteOpt.IsSome);

        // ============================================================
        // 6. 타입 시스템 브릿지 개념 정리
        // ============================================================
        MenuHelper.PrintSubHeader("6. 타입 시스템 브릿지 개념 정리");

        MenuHelper.PrintExplanation("정리:");
        MenuHelper.PrintExplanation("1. K<F, A>는 C#에서 Higher-Kinded Types를 시뮬레이션합니다.");
        MenuHelper.PrintExplanation("2. Option<A>, Either<L, R> 등은 K<F, A>를 구현합니다.");
        MenuHelper.PrintExplanation("3. 제네릭 모나드 함수는 K<M, A>와 Monad<M> 제약을 사용합니다.");
        MenuHelper.PrintExplanation("4. 구체 타입의 메서드가 필요하면 As()로 변환합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 흐름 요약:");
        MenuHelper.PrintCode("// Option<A> → K<Option, A> (업캐스팅, 암시적)");
        MenuHelper.PrintCode("// K<Option, A> → Option<A> (As() 메서드로 변환)");

        MenuHelper.PrintSuccess("K<F, A> 기본 개념 학습 완료!");
    }

    /// <summary>
    /// 제네릭 모나드 함수 예시: 값을 2배로 만듭니다.
    /// M이 Functor이기만 하면 어떤 타입이든 동작합니다.
    /// </summary>
    private static K<M, int> DoubleValue<M>(K<M, int> ma)
        where M : Functor<M>
        => ma.Map(x => x * 2);
}
