using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter12_Traits;

/// <summary>
/// 실습 09: Traits를 활용한 제네릭 함수 구현
///
/// 학습 목표:
/// - Functor, Applicative, Monad, Alternative, Foldable, Traversable 종합 활용
/// - 여러 모나드 타입에서 재사용 가능한 범용 함수 구현
/// - Trait 제약 선택 가이드 실전 적용
/// - 실전 파이프라인 구축
///
/// 이 실습에서는 학습한 모든 Traits를 활용하여
/// 재사용 가능한 유틸리티 함수들을 구현합니다.
/// </summary>
public static class Exercise09_GenericFunctions
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Exercise 09: 제네릭 함수 종합 실습");

        // ============================================================
        // Part 1: Functor 범용 함수
        // ============================================================
        MenuHelper.PrintSubHeader("Part 1: Functor 범용 함수");

        MenuHelper.PrintExplanation("Functor<M> 제약만으로 충분한 함수들입니다.");
        MenuHelper.PrintExplanation("가장 약한 제약 = 가장 넓은 적용 범위");
        MenuHelper.PrintBlankLines();

        // 1-1. MapIf: 조건부 Map
        MenuHelper.PrintCode("// MapIf: 조건이 참일 때만 변환");
        MenuHelper.PrintCode("static K<M, A> MapIf<M, A>(K<M, A> ma, Func<A, bool> pred, Func<A, A> f)");
        MenuHelper.PrintCode("    where M : Functor<M>");
        MenuHelper.PrintBlankLines();

        var mapIfResult1 = MapIf<Option, int>(Some(10), x => x > 5, x => x * 100);
        var mapIfResult2 = MapIf<Option, int>(Some(3), x => x > 5, x => x * 100);
        MenuHelper.PrintResult("MapIf(Some(10), >5, *100)", mapIfResult1.As());
        MenuHelper.PrintResult("MapIf(Some(3), >5, *100)", mapIfResult2.As());

        // 1-2. Tap: 부수 효과 삽입
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// Tap: 부수 효과 후 원래 값 반환");
        var tapResult = Tap<Option, int>(Some(42), x => Console.WriteLine($"    [Tap] 현재 값: {x}"));
        MenuHelper.PrintResult("Tap 후 값", tapResult.As());

        // 1-3. As (타입 변환)
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// AsString: 모든 값을 문자열로");
        var asStr1 = AsString<Option, int>(Some(123));
        var asStr2 = AsString<Seq, double>(Seq(1.5, 2.5, 3.5));
        MenuHelper.PrintResult("AsString Option", asStr1.As());
        MenuHelper.PrintResult("AsString Seq", asStr2.As());

        // ============================================================
        // Part 2: Applicative 범용 함수
        // ============================================================
        MenuHelper.PrintSubHeader("Part 2: Applicative 범용 함수");

        MenuHelper.PrintExplanation("Applicative<M>: 여러 독립적 값을 조합할 때 사용");
        MenuHelper.PrintBlankLines();

        // 2-1. Zip: 두 값을 튜플로
        MenuHelper.PrintCode("// Zip: 두 값을 튜플로 묶기");
        var zipped = Zip<Option, int, string>(Some(1), Some("one"));
        MenuHelper.PrintResult("Zip(Some(1), Some(\"one\"))", zipped.As());

        // 2-2. Lift2: 이항 함수 승격
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// Lift2: 이항 함수를 Applicative 컨텍스트로");
        var lifted = Lift2<Option, int, int, int>(Some(10), Some(3), (a, b) => a * b);
        MenuHelper.PrintResult("Lift2(Some(10), Some(3), *)", lifted.As());

        // 2-3. Lift3: 삼항 함수 승격
        MenuHelper.PrintCode("// Lift3: 삼항 함수");
        var lift3Result = Lift3<Option, string, string, int, string>(
            Some("Hello"),
            Some("World"),
            Some(3),
            (a, b, n) => $"{string.Join(" ", Enumerable.Repeat($"{a} {b}", n))}"
        );
        MenuHelper.PrintResult("Lift3 결과", lift3Result.As());

        // ============================================================
        // Part 3: Monad 범용 함수
        // ============================================================
        MenuHelper.PrintSubHeader("Part 3: Monad 범용 함수");

        MenuHelper.PrintExplanation("Monad<M>: 순차적 의존 연산에 사용");
        MenuHelper.PrintBlankLines();

        // 3-1. FlatMap 체인
        MenuHelper.PrintCode("// ChainThree: 세 개의 의존적 연산 체이닝");
        var chainResult = ChainThree<Option, int, int, int, string>(
            Some(10),
            x => Some(x + 5),
            y => Some(y * 2),
            z => Some($"결과: {z}")
        );
        MenuHelper.PrintResult("ChainThree", chainResult.As()); // (10+5)*2 = 30

        // 3-2. Guard: 조건 검사
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// Guard: 조건 실패 시 빈 값 반환");
        var guarded1 = Guard<Option, int>(Some(10), x => x > 5);
        var guarded2 = Guard<Option, int>(Some(3), x => x > 5);
        MenuHelper.PrintResult("Guard(Some(10), >5)", guarded1.As());
        MenuHelper.PrintResult("Guard(Some(3), >5)", guarded2.As());

        // 3-3. IfThenElse: 조건 분기
        MenuHelper.PrintCode("// IfThenElse: 조건에 따른 분기");
        var branch1 = IfThenElse<Option, int>(Some(10), x => x > 5, x => Some(x * 10), x => Some(x));
        var branch2 = IfThenElse<Option, int>(Some(3), x => x > 5, x => Some(x * 10), x => Some(x));
        MenuHelper.PrintResult("10 > 5 이면 *10", branch1.As());
        MenuHelper.PrintResult("3 > 5 이면 *10", branch2.As());

        // ============================================================
        // Part 4: Alternative 범용 함수
        // ============================================================
        MenuHelper.PrintSubHeader("Part 4: Alternative 범용 함수");

        MenuHelper.PrintExplanation("Alternative<M>: 실패 시 대안 선택에 사용");
        MenuHelper.PrintBlankLines();

        // 4-1. OrElse: 폴백
        MenuHelper.PrintCode("// OrElse: 첫 번째 실패 시 두 번째");
        var orElse1 = OrElse<Option, int>(Option<int>.None, Some(42));
        var orElse2 = OrElse<Option, int>(Some(100), Some(42));
        MenuHelper.PrintResult("OrElse(None, Some(42))", orElse1.As());
        MenuHelper.PrintResult("OrElse(Some(100), Some(42))", orElse2.As());

        // 4-2. FirstOf: 여러 대안 중 첫 성공
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// FirstOf: 여러 대안 중 첫 성공");
        var first = FirstOf<Option, int>(
            Option<int>.None,
            Option<int>.None,
            Some(100),
            Some(200)
        );
        MenuHelper.PrintResult("FirstOf", first.As());

        // 4-3. Optional: 실패를 빈 값으로 변환
        MenuHelper.PrintCode("// Optional: 실패해도 계속 진행 (빈 값 반환)");
        var opt1 = Optional<Option, int>(Some(10));
        var opt2 = Optional<Option, int>(Option<int>.None);
        MenuHelper.PrintResult("Optional(Some(10))", opt1.As());
        MenuHelper.PrintResult("Optional(None)", opt2.As());

        // ============================================================
        // Part 5: Traverse 범용 함수
        // ============================================================
        MenuHelper.PrintSubHeader("Part 5: Traverse 활용");

        MenuHelper.PrintExplanation("Traverse: 이펙트 함수를 컬렉션에 일괄 적용");
        MenuHelper.PrintBlankLines();

        // 5-1. 배치 파싱
        MenuHelper.PrintCode("// 배치 파싱");
        var parseResult1 = TraverseOption(Seq("1", "2", "3"), ParseInt);
        var parseResult2 = TraverseOption(Seq("1", "x", "3"), ParseInt);
        MenuHelper.PrintResult("모두 성공", parseResult1);
        MenuHelper.PrintResult("일부 실패", parseResult2);

        // 5-2. 배치 검증
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// 배치 이메일 검증");
        var emails1 = Seq("a@b.com", "c@d.org");
        var emails2 = Seq("a@b.com", "invalid", "c@d.org");
        MenuHelper.PrintResult("모두 유효", TraverseOption(emails1, ValidateEmail));
        MenuHelper.PrintResult("일부 무효", TraverseOption(emails2, ValidateEmail));

        // ============================================================
        // Part 6: 실전 파이프라인
        // ============================================================
        MenuHelper.PrintSubHeader("Part 6: 실전 파이프라인");

        MenuHelper.PrintExplanation("학습한 Traits를 조합한 실전 파이프라인입니다.");
        MenuHelper.PrintBlankLines();

        // 사용자 처리 파이프라인
        MenuHelper.PrintCode("// 사용자 처리 파이프라인:");
        MenuHelper.PrintCode("// 검증 -> 조회 -> 권한확인 -> 결과생성");
        MenuHelper.PrintBlankLines();

        var result1 = ProcessUser(1);   // 존재하는 관리자
        var result2 = ProcessUser(2);   // 존재하는 일반 사용자
        var result3 = ProcessUser(999); // 존재하지 않는 사용자

        MenuHelper.PrintResult("관리자(1)", result1);
        MenuHelper.PrintResult("일반사용자(2)", result2);
        MenuHelper.PrintResult("없는사용자(999)", result3);

        // ============================================================
        // Part 7: Trait 제약 선택 가이드
        // ============================================================
        MenuHelper.PrintSubHeader("Part 7: Trait 제약 선택 가이드");

        MenuHelper.PrintExplanation("함수 작성 시 제약 선택 기준:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("┌─────────────┬────────────────────────────────────┐");
        MenuHelper.PrintCode("│ 제약        │ 사용 시나리오                      │");
        MenuHelper.PrintCode("├─────────────┼────────────────────────────────────┤");
        MenuHelper.PrintCode("│ Functor     │ 단일 값 변환 (Map만 사용)          │");
        MenuHelper.PrintCode("│ Applicative │ 독립적 여러 값 조합                │");
        MenuHelper.PrintCode("│ Monad       │ 의존적 순차 연산                   │");
        MenuHelper.PrintCode("│ Alternative │ 실패 시 대안 선택                  │");
        MenuHelper.PrintCode("│ Foldable    │ 컬렉션 축적/집계                   │");
        MenuHelper.PrintCode("│ Traversable │ 이펙트와 컬렉션 동시 처리          │");
        MenuHelper.PrintCode("└─────────────┴────────────────────────────────────┘");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("핵심 원칙: 가능한 가장 약한 제약을 사용하세요!");
        MenuHelper.PrintExplanation("약한 제약 = 더 많은 타입에서 재사용 가능");

        // ============================================================
        // Part 8: 정리
        // ============================================================
        MenuHelper.PrintSubHeader("Part 8: 정리");

        MenuHelper.PrintExplanation("이번 실습에서 구현한 범용 함수들:");
        MenuHelper.PrintExplanation("- MapIf, Tap, AsString (Functor)");
        MenuHelper.PrintExplanation("- Zip, Lift2, Lift3 (Applicative)");
        MenuHelper.PrintExplanation("- ChainThree, Guard, IfThenElse (Monad)");
        MenuHelper.PrintExplanation("- OrElse, FirstOf, Optional (Alternative)");
        MenuHelper.PrintExplanation("- Traverse 활용 패턴");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("이 함수들은 Option, Either, Seq, IO 등");
        MenuHelper.PrintExplanation("모든 적합한 타입에서 재사용할 수 있습니다!");

        MenuHelper.PrintSuccess("실습 09 완료!");
    }

    // ============================================================
    // Functor 범용 함수
    // ============================================================

    /// <summary>
    /// 조건이 참일 때만 변환 적용
    /// </summary>
    private static K<M, A> MapIf<M, A>(K<M, A> ma, Func<A, bool> pred, Func<A, A> f)
        where M : Functor<M>
        => ma.Map(x => pred(x) ? f(x) : x);

    /// <summary>
    /// 부수 효과 삽입 후 원래 값 반환
    /// </summary>
    private static K<M, A> Tap<M, A>(K<M, A> ma, Action<A> action)
        where M : Functor<M>
        => ma.Map(x => { action(x); return x; });

    /// <summary>
    /// 모든 값을 문자열로 변환
    /// </summary>
    private static K<M, string> AsString<M, A>(K<M, A> ma)
        where M : Functor<M>
        => ma.Map(x => x?.ToString() ?? "null");

    // ============================================================
    // Applicative 범용 함수
    // ============================================================

    /// <summary>
    /// 두 값을 튜플로 묶기
    /// </summary>
    private static K<M, (A, B)> Zip<M, A, B>(K<M, A> ma, K<M, B> mb)
        where M : Applicative<M>
        => ma.Map(curry<A, B, (A, B)>((a, b) => (a, b))).Apply(mb);

    /// <summary>
    /// 이항 함수를 Applicative 컨텍스트로 승격
    /// </summary>
    private static K<M, C> Lift2<M, A, B, C>(K<M, A> ma, K<M, B> mb, Func<A, B, C> f)
        where M : Applicative<M>
        => ma.Map(curry(f)).Apply(mb);

    /// <summary>
    /// 삼항 함수를 Applicative 컨텍스트로 승격
    /// </summary>
    private static K<M, D> Lift3<M, A, B, C, D>(
        K<M, A> ma, K<M, B> mb, K<M, C> mc, Func<A, B, C, D> f)
        where M : Applicative<M>
        => ma.Map(curry(f)).Apply(mb).Apply(mc);

    // ============================================================
    // Monad 범용 함수
    // ============================================================

    /// <summary>
    /// 세 개의 의존적 연산 체이닝
    /// </summary>
    private static K<M, D> ChainThree<M, A, B, C, D>(
        K<M, A> ma,
        Func<A, K<M, B>> f,
        Func<B, K<M, C>> g,
        Func<C, K<M, D>> h)
        where M : Monad<M>
        => ma.Bind(f).Bind(g).Bind(h);

    /// <summary>
    /// 조건 검사 후 실패하면 빈 값
    /// </summary>
    private static K<M, A> Guard<M, A>(K<M, A> ma, Func<A, bool> pred)
        where M : Monad<M>, Alternative<M>
        => ma.Bind(a => pred(a) ? M.Pure(a) : M.Empty<A>());

    /// <summary>
    /// 조건에 따른 분기
    /// </summary>
    private static K<M, A> IfThenElse<M, A>(
        K<M, A> ma,
        Func<A, bool> pred,
        Func<A, K<M, A>> whenTrue,
        Func<A, K<M, A>> whenFalse)
        where M : Monad<M>
        => ma.Bind(a => pred(a) ? whenTrue(a) : whenFalse(a));

    // ============================================================
    // Alternative 범용 함수
    // ============================================================

    /// <summary>
    /// 첫 번째가 실패하면 두 번째 사용
    /// </summary>
    private static K<M, A> OrElse<M, A>(K<M, A> primary, K<M, A> fallback)
        where M : Alternative<M>
        => primary.Choose(fallback);

    /// <summary>
    /// 여러 대안 중 첫 성공 선택
    /// </summary>
    private static K<M, A> FirstOf<M, A>(params K<M, A>[] alternatives)
        where M : Alternative<M>
    {
        K<M, A> result = M.Empty<A>();
        foreach (var alt in alternatives)
            result = result.Choose(alt);
        return result;
    }

    /// <summary>
    /// 실패를 Option으로 감싸서 계속 진행 가능하게
    /// </summary>
    private static K<M, Option<A>> Optional<M, A>(K<M, A> ma)
        where M : Functor<M>, Alternative<M>
        => ma.Map(Some).Choose(M.Pure(Option<A>.None));

    // ============================================================
    // Traverse 구현
    // ============================================================

    /// <summary>
    /// 수동 Traverse 구현 (Option용)
    /// </summary>
    private static Option<Seq<B>> TraverseOption<A, B>(Seq<A> seq, Func<A, Option<B>> f)
    {
        var results = new System.Collections.Generic.List<B>();
        foreach (var item in seq)
        {
            var result = f(item);
            if (result.IsNone)
                return None;
            results.Add(result.Match(Some: x => x, None: () => default!));
        }
        return Some(toSeq(results));
    }

    // ============================================================
    // 도메인 함수들
    // ============================================================

    private static Option<int> ParseInt(string s)
        => int.TryParse(s, out var n) ? Some(n) : None;

    private static Option<string> ValidateEmail(string email)
        => email.Contains('@') ? Some(email) : None;

    // 실전 파이프라인용
    private record User(int Id, string Name, bool IsAdmin);

    private static Option<User> FindUser(int id)
        => id switch
        {
            1 => Some(new User(1, "Admin", true)),
            2 => Some(new User(2, "User", false)),
            _ => None
        };

    private static Option<string> CheckPermission(User user)
        => user.IsAdmin ? Some($"{user.Name} (관리자 권한)") : Some($"{user.Name} (일반 권한)");

    private static Option<string> ProcessUser(int userId)
    {
        return from user in FindUser(userId)
               from result in CheckPermission(user)
               select $"처리 완료: {result}";
    }
}
