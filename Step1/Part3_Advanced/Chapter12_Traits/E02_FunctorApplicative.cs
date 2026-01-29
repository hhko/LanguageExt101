using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter12_Traits;

/// <summary>
/// Functor와 Applicative 트레이트를 상세히 학습합니다.
///
/// 학습 목표:
/// - Functor&lt;M&gt; 트레이트와 Map 연산 마스터
/// - Functor Laws 이해 및 검증
/// - Applicative&lt;M&gt; 트레이트와 Pure, Apply 연산
/// - Applicative로 여러 독립적 값 조합하기
/// - 커링(Currying)과 Apply의 조합
///
/// 핵심 개념:
/// Functor는 "컨텍스트 안의 값을 변환"합니다. (단일 값)
/// Applicative는 "여러 컨텍스트의 값을 조합"합니다. (독립적 값들)
/// </summary>
public static class E02_FunctorApplicative
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 12-E02: Functor와 Applicative");

        // ============================================================
        // 1. Functor: Map 연산
        // ============================================================
        MenuHelper.PrintSubHeader("1. Functor: Map 연산");

        MenuHelper.PrintExplanation("Functor의 핵심은 Map입니다:");
        MenuHelper.PrintExplanation("컨텍스트(Option, Seq 등)를 유지하면서 내부 값만 변환합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Map의 시그니처");
        MenuHelper.PrintCode("// Map<A, B>(F<A>, Func<A, B>) -> F<B>");
        MenuHelper.PrintBlankLines();

        // 기본 예시
        var opt1 = Some(5).Map(x => x * 2);
        var opt2 = Some("hello").Map(s => s.ToUpper());
        var seq1 = Seq(1, 2, 3).Map(x => x * x);

        MenuHelper.PrintCode("Some(5).Map(x => x * 2)");
        MenuHelper.PrintResult("결과", opt1);

        MenuHelper.PrintCode("Some(\"hello\").Map(s => s.ToUpper())");
        MenuHelper.PrintResult("결과", opt2);

        MenuHelper.PrintCode("Seq(1, 2, 3).Map(x => x * x)");
        MenuHelper.PrintResult("결과", seq1);

        // None의 경우
        var optNone = Option<int>.None.Map(x => x * 2);
        MenuHelper.PrintCode("Option<int>.None.Map(x => x * 2)");
        MenuHelper.PrintResult("None의 Map", optNone);

        // ============================================================
        // 2. Functor Laws
        // ============================================================
        MenuHelper.PrintSubHeader("2. Functor Laws");

        MenuHelper.PrintExplanation("Functor는 두 가지 법칙을 만족해야 합니다:");
        MenuHelper.PrintExplanation("1. Identity: fa.Map(x => x) == fa");
        MenuHelper.PrintExplanation("2. Composition: fa.Map(f).Map(g) == fa.Map(x => g(f(x)))");
        MenuHelper.PrintBlankLines();

        // Identity Law
        var original = Some(42);
        var afterIdentity = original.Map(x => x);
        MenuHelper.PrintCode("// Identity Law: Map(x => x)은 원본과 동일");
        MenuHelper.PrintResult("Some(42) == Some(42).Map(x => x)", original.Equals(afterIdentity));

        // Composition Law
        Func<int, int> addOne = x => x + 1;
        Func<int, int> double_ = x => x * 2;

        var composedSeparate = Some(5).Map(addOne).Map(double_);
        var composedTogether = Some(5).Map(x => double_(addOne(x)));
        MenuHelper.PrintCode("// Composition Law: Map 두 번 == Map 한 번(합성)");
        MenuHelper.PrintResult("Map(f).Map(g) == Map(x => g(f(x)))", composedSeparate.Equals(composedTogether));

        // ============================================================
        // 3. 제네릭 Functor 함수
        // ============================================================
        MenuHelper.PrintSubHeader("3. 제네릭 Functor 함수");

        MenuHelper.PrintExplanation("Functor<M> 제약으로 모든 Functor 타입에서 동작하는 함수를 작성합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 제네릭 함수 정의");
        MenuHelper.PrintCode("static K<M, (A, A)> Duplicate<M, A>(K<M, A> ma) where M : Functor<M>");
        MenuHelper.PrintCode("    => ma.Map(x => (x, x));");
        MenuHelper.PrintBlankLines();

        var dupOpt = Duplicate<Option, int>(Some(5));
        var dupSeq = Duplicate<Seq, string>(Seq("a", "b"));
        MenuHelper.PrintResult("Duplicate<Option>(Some(5))", dupOpt.As());
        MenuHelper.PrintResult("Duplicate<Seq>(Seq(\"a\",\"b\"))", dupSeq.As());

        // ============================================================
        // 4. Applicative: Pure 연산
        // ============================================================
        MenuHelper.PrintSubHeader("4. Applicative: Pure 연산");

        MenuHelper.PrintExplanation("Pure는 일반 값을 컨텍스트로 승격시킵니다.");
        MenuHelper.PrintExplanation("'값을 박스에 넣는다'고 생각하면 됩니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Pure의 시그니처");
        MenuHelper.PrintCode("// Pure<A>(A) -> F<A>");
        MenuHelper.PrintBlankLines();

        var pureOpt = Some(100);  // Option의 Pure와 동등
        var pureSeq = Seq(100);   // Seq의 Pure: 단일 원소
        MenuHelper.PrintCode("Some(100);  // Option의 Pure");
        MenuHelper.PrintResult("결과", pureOpt);

        MenuHelper.PrintCode("Seq(100);  // Seq의 Pure (단일 원소)");
        MenuHelper.PrintResult("결과", pureSeq);

        // ============================================================
        // 5. Applicative: Apply 연산
        // ============================================================
        MenuHelper.PrintSubHeader("5. Applicative: Apply 연산");

        MenuHelper.PrintExplanation("Apply는 컨텍스트 안의 함수를 컨텍스트 안의 값에 적용합니다.");
        MenuHelper.PrintExplanation("여러 값을 조합할 때 사용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Apply의 시그니처");
        MenuHelper.PrintCode("// Apply<A, B>(F<Func<A, B>>, F<A>) -> F<B>");
        MenuHelper.PrintBlankLines();

        // 직접 Apply 사용
        var funcOpt = Some<Func<int, int>>(x => x * 10);
        var applied = funcOpt.Apply(Some(5));
        MenuHelper.PrintCode("Some<Func<int,int>>(x => x * 10).Apply(Some(5))");
        MenuHelper.PrintResult("결과", applied);

        // ============================================================
        // 6. 커링과 Apply 조합
        // ============================================================
        MenuHelper.PrintSubHeader("6. 커링과 Apply 조합");

        MenuHelper.PrintExplanation("두 개 이상의 값을 조합하려면 커링(Currying)을 사용합니다.");
        MenuHelper.PrintExplanation("curry(f)는 (a, b) -> c를 a -> b -> c로 변환합니다.");
        MenuHelper.PrintBlankLines();

        // 두 값 조합
        MenuHelper.PrintCode("// 두 Option 값 더하기");
        MenuHelper.PrintCode("Some(10).Map(curry<int,int,int>((a,b) => a + b)).Apply(Some(20))");

        var sum = Some(10)
            .Map(curry<int, int, int>((a, b) => a + b))
            .Apply(Some(20));
        MenuHelper.PrintResult("10 + 20", sum);

        // 세 값 조합
        MenuHelper.PrintCode("// 세 값 조합 (URL 생성)");
        var url = Some("https")
            .Map(curry<string, string, int, string>((proto, host, port) => $"{proto}://{host}:{port}"))
            .Apply(Some("example.com"))
            .Apply(Some(443));
        MenuHelper.PrintResult("URL", url);

        // 하나라도 None이면 전체 None
        var urlFailed = Some("https")
            .Map(curry<string, string, int, string>((proto, host, port) => $"{proto}://{host}:{port}"))
            .Apply(Option<string>.None)
            .Apply(Some(443));
        MenuHelper.PrintResult("Host가 None", urlFailed);

        // ============================================================
        // 7. 제네릭 Applicative 함수
        // ============================================================
        MenuHelper.PrintSubHeader("7. 제네릭 Applicative 함수");

        MenuHelper.PrintExplanation("Applicative<M> 제약으로 여러 값 조합 로직을 범용화합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Zip: 두 값을 튜플로 묶기");
        MenuHelper.PrintCode("static K<M, (A, B)> Zip<M, A, B>(K<M, A> ma, K<M, B> mb)");
        MenuHelper.PrintCode("    where M : Applicative<M>");
        MenuHelper.PrintCode("    => ma.Map(curry<A, B, (A, B)>((a, b) => (a, b))).Apply(mb);");
        MenuHelper.PrintBlankLines();

        var zippedOpt = Zip<Option, int, string>(Some(1), Some("one"));
        var zippedSeq = Zip<Seq, int, int>(Seq(1, 2), Seq(10, 20));
        MenuHelper.PrintResult("Zip Option", zippedOpt.As());
        MenuHelper.PrintResult("Zip Seq", zippedSeq.As());

        // Lift2 함수
        MenuHelper.PrintCode("// Lift2: 두 값에 이항 함수 적용");
        var lifted = Lift2<Option, int, int, int>(Some(10), Some(3), (a, b) => a - b);
        MenuHelper.PrintResult("Lift2(Some(10), Some(3), -)", lifted.As());

        // ============================================================
        // 8. Applicative vs Monad
        // ============================================================
        MenuHelper.PrintSubHeader("8. Applicative vs Monad 비교");

        MenuHelper.PrintExplanation("핵심 차이: 의존성!");
        MenuHelper.PrintExplanation("Applicative: 값들이 서로 독립적");
        MenuHelper.PrintExplanation("Monad: 다음 연산이 이전 값에 의존");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Applicative (독립적)");
        MenuHelper.PrintCode("// optA와 optB를 동시에 평가해도 됨");
        MenuHelper.PrintCode("optA.Map(curry(f)).Apply(optB)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Monad (의존적)");
        MenuHelper.PrintCode("// b를 얻으려면 a가 먼저 필요함");
        MenuHelper.PrintCode("optA.Bind(a => getOptB(a))");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Applicative가 더 약한 제약이므로:");
        MenuHelper.PrintExplanation("- Applicative로 충분하면 Monad를 쓰지 마세요");
        MenuHelper.PrintExplanation("- 약한 제약 = 더 많은 타입에서 사용 가능");

        // ============================================================
        // 9. 실전 예제: 설정 조합
        // ============================================================
        MenuHelper.PrintSubHeader("9. 실전 예제: 설정 조합");

        MenuHelper.PrintExplanation("여러 Optional 설정을 조합하여 객체를 생성합니다.");
        MenuHelper.PrintBlankLines();

        // 성공 케이스
        var config = CreateDbConfig(
            Some("localhost"),
            Some(5432),
            Some("mydb"),
            Some("admin")
        );
        MenuHelper.PrintCode("CreateDbConfig(Some(\"localhost\"), Some(5432), Some(\"mydb\"), Some(\"admin\"))");
        MenuHelper.PrintResult("설정 생성", config);

        // 실패 케이스
        var configFailed = CreateDbConfig(
            Some("localhost"),
            Option<int>.None,  // 포트 누락
            Some("mydb"),
            Some("admin")
        );
        MenuHelper.PrintResult("포트 누락 시", configFailed);

        // ============================================================
        // 10. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("10. 정리");

        MenuHelper.PrintExplanation("Functor<M>:");
        MenuHelper.PrintExplanation("  - Map으로 컨텍스트 안의 단일 값 변환");
        MenuHelper.PrintExplanation("  - 가장 약한 제약, 가장 넓은 적용 범위");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Applicative<M>:");
        MenuHelper.PrintExplanation("  - Pure + Apply로 여러 독립적 값 조합");
        MenuHelper.PrintExplanation("  - 커링과 함께 사용하여 다중 값 처리");
        MenuHelper.PrintExplanation("  - Validation 등 에러 누적에 유용");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("다음: E03에서 Monad와 Alternative를 학습합니다.");

        MenuHelper.PrintSuccess("Functor와 Applicative 학습 완료!");
    }

    // ============================================================
    // 헬퍼 함수들
    // ============================================================

    /// <summary>
    /// 값을 복제하는 범용 함수 (Functor)
    /// </summary>
    private static K<M, (A, A)> Duplicate<M, A>(K<M, A> ma)
        where M : Functor<M>
        => ma.Map(x => (x, x));

    /// <summary>
    /// 두 값을 튜플로 묶는 범용 함수 (Applicative)
    /// </summary>
    private static K<M, (A, B)> Zip<M, A, B>(K<M, A> ma, K<M, B> mb)
        where M : Applicative<M>
        => ma.Map(curry<A, B, (A, B)>((a, b) => (a, b))).Apply(mb);

    /// <summary>
    /// 이항 함수를 Applicative 컨텍스트로 승격 (Applicative)
    /// </summary>
    private static K<M, C> Lift2<M, A, B, C>(K<M, A> ma, K<M, B> mb, Func<A, B, C> f)
        where M : Applicative<M>
        => ma.Map(curry(f)).Apply(mb);

    /// <summary>
    /// DB 설정 레코드
    /// </summary>
    private record DbConfig(string Host, int Port, string Database, string User);

    /// <summary>
    /// 여러 Optional 값으로 DbConfig 생성 (Applicative)
    /// </summary>
    private static Option<DbConfig> CreateDbConfig(
        Option<string> host,
        Option<int> port,
        Option<string> database,
        Option<string> user)
    {
        return host
            .Map(curry<string, int, string, string, DbConfig>(
                (h, p, d, u) => new DbConfig(h, p, d, u)))
            .Apply(port)
            .Apply(database)
            .Apply(user);
    }
}
