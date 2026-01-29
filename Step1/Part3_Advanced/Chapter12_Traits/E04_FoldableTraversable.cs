using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter12_Traits;

/// <summary>
/// Foldable과 Traversable 트레이트를 상세히 학습합니다.
///
/// 학습 목표:
/// - Foldable&lt;T&gt; 트레이트와 Fold 연산 마스터
/// - FoldBack, FoldMap 등 다양한 Fold 패턴
/// - Traversable의 개념과 활용 패턴
/// - 배치 처리 패턴 이해
///
/// 핵심 개념:
/// Foldable은 "컬렉션을 하나의 값으로 축적"합니다.
/// Traversable은 "구조를 유지하면서 이펙트를 적용"합니다.
/// </summary>
public static class E04_FoldableTraversable
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 12-E04: Foldable과 Traversable");

        // ============================================================
        // 1. Foldable: Fold 연산
        // ============================================================
        MenuHelper.PrintSubHeader("1. Foldable: Fold 연산");

        MenuHelper.PrintExplanation("Foldable의 핵심은 Fold(접기)입니다:");
        MenuHelper.PrintExplanation("컬렉션의 모든 원소를 순회하며 하나의 값으로 축적합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Fold의 시그니처");
        MenuHelper.PrintCode("// Fold<S, A>(T<A>, S, Func<S, A, S>) -> S");
        MenuHelper.PrintCode("// S: 초기값 및 누적값 타입, A: 원소 타입");
        MenuHelper.PrintBlankLines();

        // 기본 Fold 예시
        var numbers = Seq(1, 2, 3, 4, 5);
        var sum = numbers.Fold(0, (acc, x) => acc + x);
        MenuHelper.PrintCode("Seq(1, 2, 3, 4, 5).Fold(0, (acc, x) => acc + x)");
        MenuHelper.PrintResult("합계", sum);

        var product = numbers.Fold(1, (acc, x) => acc * x);
        MenuHelper.PrintCode("Seq(1, 2, 3, 4, 5).Fold(1, (acc, x) => acc * x)");
        MenuHelper.PrintResult("곱", product);

        // 문자열 연결
        var words = Seq("Hello", " ", "World", "!");
        var sentence = words.Fold("", (acc, s) => acc + s);
        MenuHelper.PrintResult("문자열 연결", sentence);

        // ============================================================
        // 2. FoldBack (오른쪽에서 접기)
        // ============================================================
        MenuHelper.PrintSubHeader("2. FoldBack (오른쪽에서 접기)");

        MenuHelper.PrintExplanation("FoldBack은 오른쪽에서 왼쪽으로 접습니다.");
        MenuHelper.PrintExplanation("순서가 중요한 연산에서 차이가 납니다.");
        MenuHelper.PrintBlankLines();

        var nums = Seq(1, 2, 3);

        // Fold (왼쪽에서 오른쪽)
        var foldLeft = nums.Fold("", (acc, x) => $"({acc} + {x})");
        MenuHelper.PrintCode("// Fold: 왼쪽 -> 오른쪽");
        MenuHelper.PrintResult("결과", foldLeft);

        // FoldBack (오른쪽에서 왼쪽)
        var foldRight = nums.FoldBack("", (x, acc) => $"({x} + {acc})");
        MenuHelper.PrintCode("// FoldBack: 오른쪽 -> 왼쪽");
        MenuHelper.PrintResult("결과", foldRight);

        // ============================================================
        // 3. Fold 활용 패턴
        // ============================================================
        MenuHelper.PrintSubHeader("3. Fold 활용 패턴");

        MenuHelper.PrintExplanation("Fold로 다양한 집계 연산을 구현할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var data = Seq(3, 1, 4, 1, 5, 9, 2, 6);

        // 최대값 찾기
        var max = data.Fold(int.MinValue, (acc, x) => x > acc ? x : acc);
        MenuHelper.PrintCode("// 최대값");
        MenuHelper.PrintResult("Max", max);

        // 최소값 찾기
        var min = data.Fold(int.MaxValue, (acc, x) => x < acc ? x : acc);
        MenuHelper.PrintResult("Min", min);

        // 개수 세기
        var count = data.Fold(0, (acc, _) => acc + 1);
        MenuHelper.PrintResult("Count", count);

        // 조건부 카운트
        var evenCount = data.Fold(0, (acc, x) => x % 2 == 0 ? acc + 1 : acc);
        MenuHelper.PrintResult("짝수 개수", evenCount);

        // ============================================================
        // 4. Seq의 내장 집계 메서드들
        // ============================================================
        MenuHelper.PrintSubHeader("4. Seq의 내장 집계 메서드들");

        MenuHelper.PrintExplanation("Seq는 자주 쓰는 Fold 패턴을 메서드로 제공합니다.");
        MenuHelper.PrintBlankLines();

        var items = Seq(10, 20, 30, 40, 50);

        var sumResult = items.Fold(0, (a, b) => a + b);
        MenuHelper.PrintResult("Sum (Fold)", sumResult);
        MenuHelper.PrintResult("Count", items.Count);
        MenuHelper.PrintResult("Head", items.Head);
        MenuHelper.PrintResult("Last", items.Last);

        // All, Any
        var allPositive = items.ForAll(x => x > 0);
        var anyOver30 = items.Exists(x => x > 30);
        MenuHelper.PrintResult("All > 0", allPositive);
        MenuHelper.PrintResult("Any > 30", anyOver30);

        // ============================================================
        // 5. Traversable의 개념
        // ============================================================
        MenuHelper.PrintSubHeader("5. Traversable의 개념");

        MenuHelper.PrintExplanation("Traversable은 Map의 이펙트 버전입니다:");
        MenuHelper.PrintExplanation("원소마다 이펙트 함수를 적용하고, 이펙트를 바깥으로 끌어냅니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Map:      Seq<A> -> (A -> B) -> Seq<B>");
        MenuHelper.PrintCode("// Traverse: Seq<A> -> (A -> F<B>) -> F<Seq<B>>");
        MenuHelper.PrintCode("// 이펙트(F)가 결과를 감싸게 됨!");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("예: 문자열 리스트를 정수로 파싱");
        MenuHelper.PrintCode("Seq(\"1\", \"2\", \"3\") -> Traverse(ParseInt) -> Option<Seq<int>>");
        MenuHelper.PrintCode("모두 성공: Some(Seq(1, 2, 3))");
        MenuHelper.PrintCode("하나라도 실패: None");

        // ============================================================
        // 6. 수동 Traverse 구현
        // ============================================================
        MenuHelper.PrintSubHeader("6. 수동 Traverse 구현");

        MenuHelper.PrintExplanation("Traverse의 동작 원리를 이해하기 위해 수동으로 구현합니다.");
        MenuHelper.PrintBlankLines();

        // 수동 traverse 구현 예시
        var validInputs = Seq("1", "2", "3");
        var parsedValid = TraverseOption(validInputs, ParseInt);
        MenuHelper.PrintCode("TraverseOption(Seq(\"1\", \"2\", \"3\"), ParseInt)");
        MenuHelper.PrintResult("모두 성공", parsedValid);

        var invalidInputs = Seq("1", "abc", "3");
        var parsedInvalid = TraverseOption(invalidInputs, ParseInt);
        MenuHelper.PrintCode("TraverseOption(Seq(\"1\", \"abc\", \"3\"), ParseInt)");
        MenuHelper.PrintResult("하나라도 실패", parsedInvalid);

        // ============================================================
        // 7. Sequence의 개념
        // ============================================================
        MenuHelper.PrintSubHeader("7. Sequence의 개념");

        MenuHelper.PrintExplanation("Sequence는 이펙트의 컬렉션을 컬렉션의 이펙트로 뒤집습니다.");
        MenuHelper.PrintExplanation("Traverse(x => x)와 동일합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// Sequence: Seq<F<A>> -> F<Seq<A>>");
        MenuHelper.PrintBlankLines();

        // 수동 sequence 예시
        var allSome = Seq(Some(1), Some(2), Some(3));
        var sequenced = SequenceOption(allSome);
        MenuHelper.PrintCode("SequenceOption(Seq(Some(1), Some(2), Some(3)))");
        MenuHelper.PrintResult("결과", sequenced);

        var withNone = Seq(Some(1), Option<int>.None, Some(3));
        var sequencedNone = SequenceOption(withNone);
        MenuHelper.PrintCode("SequenceOption(Seq(Some(1), None, Some(3)))");
        MenuHelper.PrintResult("None 포함", sequencedNone);

        // ============================================================
        // 8. 실전 예제: 배치 검증
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실전 예제: 배치 검증");

        MenuHelper.PrintExplanation("여러 입력을 한 번에 검증하는 패턴입니다.");
        MenuHelper.PrintExplanation("'하나라도 실패하면 전체 실패' 의미론을 구현합니다.");
        MenuHelper.PrintBlankLines();

        var emails = Seq("user@example.com", "admin@test.com");
        var validatedEmails = TraverseOption(emails, ValidateEmail);
        MenuHelper.PrintCode("TraverseOption(emails, ValidateEmail)");
        MenuHelper.PrintResult("모두 유효", validatedEmails);

        var badEmails = Seq("user@example.com", "invalid-email", "test@test.com");
        var validatedBad = TraverseOption(badEmails, ValidateEmail);
        MenuHelper.PrintResult("일부 무효", validatedBad);

        // ============================================================
        // 9. 실전 예제: 배치 조회
        // ============================================================
        MenuHelper.PrintSubHeader("9. 실전 예제: 배치 조회");

        MenuHelper.PrintExplanation("여러 ID로 데이터를 일괄 조회하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        var userIds = Seq(1, 2, 3);
        var users = TraverseOption(userIds, FindUserById);
        MenuHelper.PrintCode("TraverseOption(Seq(1, 2, 3), FindUserById)");
        MenuHelper.PrintResult("존재하는 사용자들", users);

        var mixedIds = Seq(1, 999, 2);  // 999는 존재하지 않음
        var mixedUsers = TraverseOption(mixedIds, FindUserById);
        MenuHelper.PrintResult("일부 없는 ID 포함", mixedUsers);

        // ============================================================
        // 10. Either와 Traverse
        // ============================================================
        MenuHelper.PrintSubHeader("10. Either와 Traverse");

        MenuHelper.PrintExplanation("Either와 조합하면 에러 메시지를 얻을 수 있습니다.");
        MenuHelper.PrintBlankLines();

        var numbers2 = Seq("42", "abc", "100");
        var parsed = TraverseEither(numbers2, ParseIntWithError);
        MenuHelper.PrintCode("TraverseEither(Seq(\"42\", \"abc\", \"100\"), ParseIntWithError)");
        MenuHelper.PrintResult("결과", parsed);

        var validNumbers = Seq("42", "100", "200");
        var parsedValid2 = TraverseEither(validNumbers, ParseIntWithError);
        MenuHelper.PrintResult("모두 성공", parsedValid2);

        // ============================================================
        // 11. 비동기 Traverse
        // ============================================================
        MenuHelper.PrintSubHeader("11. 비동기 Traverse");

        MenuHelper.PrintExplanation("실제 애플리케이션에서는 비동기 작업이 필수입니다.");
        MenuHelper.PrintExplanation("TraverseAsync는 비동기 함수를 컬렉션에 일괄 적용합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 동기 Traverse");
        MenuHelper.PrintCode("// Seq<A> -> (A -> Option<B>) -> Option<Seq<B>>");
        MenuHelper.PrintBlankLines();
        MenuHelper.PrintCode("// 비동기 Traverse");
        MenuHelper.PrintCode("// Seq<A> -> (A -> Task<Option<B>>) -> Task<Option<Seq<B>>>");
        MenuHelper.PrintBlankLines();

        // 순차 비동기 처리
        MenuHelper.PrintExplanation("순차 처리: 하나씩 순서대로 실행");
        var userIdsAsync = Seq(1, 2, 3);
        var usersSeq = TraverseOptionAsync(userIdsAsync, FetchUserAsync).Result;
        MenuHelper.PrintCode("TraverseOptionAsync(Seq(1, 2, 3), FetchUserAsync)");
        MenuHelper.PrintResult("순차 비동기 결과", usersSeq);

        // 실패 케이스
        var mixedIdsAsync = Seq(1, 999, 2);
        var mixedUsersAsync = TraverseOptionAsync(mixedIdsAsync, FetchUserAsync).Result;
        MenuHelper.PrintResult("일부 없는 ID (비동기)", mixedUsersAsync);

        // ============================================================
        // 12. 병렬 비동기 Traverse
        // ============================================================
        MenuHelper.PrintSubHeader("12. 병렬 비동기 Traverse");

        MenuHelper.PrintExplanation("병렬 처리: 모든 작업을 동시에 실행");
        MenuHelper.PrintExplanation("순차보다 빠르지만, 하나라도 실패하면 전체 실패는 동일합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 순차: 1번 완료 -> 2번 시작 -> 2번 완료 -> 3번 시작...");
        MenuHelper.PrintCode("// 병렬: 1번, 2번, 3번 동시 시작 -> 모두 완료 대기");
        MenuHelper.PrintBlankLines();

        var parallelIds = Seq(1, 2, 3);
        var usersParallel = TraverseOptionParallelAsync(parallelIds, FetchUserAsync).Result;
        MenuHelper.PrintCode("TraverseOptionParallelAsync(Seq(1, 2, 3), FetchUserAsync)");
        MenuHelper.PrintResult("병렬 비동기 결과", usersParallel);

        // ============================================================
        // 13. 비동기 Sequence
        // ============================================================
        MenuHelper.PrintSubHeader("13. 비동기 Sequence");

        MenuHelper.PrintExplanation("Task 컬렉션을 단일 Task로 합칩니다.");
        MenuHelper.PrintExplanation("Task.WhenAll과 유사하지만 Option 의미론을 유지합니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// SequenceAsync: Seq<Task<Option<A>>> -> Task<Option<Seq<A>>>");
        MenuHelper.PrintBlankLines();

        var tasks = Seq(
            FetchUserAsync(1),
            FetchUserAsync(2),
            FetchUserAsync(3)
        );
        var sequencedAsync = SequenceOptionAsync(tasks).Result;
        MenuHelper.PrintResult("SequenceAsync 결과", sequencedAsync);

        // ============================================================
        // 14. 실전 예제: 비동기 API 배치 호출
        // ============================================================
        MenuHelper.PrintSubHeader("14. 실전 예제: 비동기 API 배치 호출");

        MenuHelper.PrintExplanation("여러 API 엔드포인트를 동시에 호출하는 패턴입니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode("// 주문 처리 파이프라인");
        MenuHelper.PrintCode("// 1. 상품 ID 목록으로 재고 확인 (병렬)");
        MenuHelper.PrintCode("// 2. 모든 상품 재고 있으면 주문 진행");
        MenuHelper.PrintCode("// 3. 하나라도 재고 없으면 전체 실패");
        MenuHelper.PrintBlankLines();

        var productIds = Seq(101, 102, 103);
        var stockCheck = TraverseOptionParallelAsync(productIds, CheckStockAsync).Result;
        MenuHelper.PrintCode("TraverseOptionParallelAsync(productIds, CheckStockAsync)");
        MenuHelper.PrintResult("재고 확인 (모두 있음)", stockCheck);

        var withOutOfStock = Seq(101, 999, 103);  // 999는 재고 없음
        var stockCheckFailed = TraverseOptionParallelAsync(withOutOfStock, CheckStockAsync).Result;
        MenuHelper.PrintResult("재고 확인 (일부 없음)", stockCheckFailed);

        // ============================================================
        // 15. 비동기 처리 선택 가이드
        // ============================================================
        MenuHelper.PrintSubHeader("15. 비동기 처리 선택 가이드");

        MenuHelper.PrintExplanation("순차 처리 사용:");
        MenuHelper.PrintExplanation("  - 이전 결과가 다음 요청에 필요할 때");
        MenuHelper.PrintExplanation("  - Rate limiting이 있을 때");
        MenuHelper.PrintExplanation("  - 순서가 중요할 때");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("병렬 처리 사용:");
        MenuHelper.PrintExplanation("  - 독립적인 작업들을 빠르게 처리할 때");
        MenuHelper.PrintExplanation("  - 여러 API를 동시에 호출할 때");
        MenuHelper.PrintExplanation("  - I/O 바운드 작업이 많을 때");

        // ============================================================
        // 16. TraverseM - 모나딕 순회
        // ============================================================
        MenuHelper.PrintSubHeader("16. TraverseM - 모나딕 순회");

        MenuHelper.PrintExplanation("TraverseM은 Monad 기반 순회입니다 (vs Traverse는 Applicative 기반):");
        MenuHelper.PrintExplanation("  - Traverse: Applicative 기반, 독립적인 효과 병합");
        MenuHelper.PrintExplanation("  - TraverseM: Monad 기반, 순차적 효과 (이전 결과에 의존 가능)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Traverse vs TraverseM
// Traverse: Applicative 기반 - 각 연산이 독립적
Seq<A>.Traverse<F, B>(A -> F<B>) -> F<Seq<B>>

// TraverseM: Monad 기반 - 순차적 실행, 이전 결과 참조 가능
Seq<A>.TraverseM<M, B>(A -> K<M, B>) -> K<M, Seq<B>>

// FinalProject.cs Line 672-673에서 사용
players.TraverseM(p => PontoonPlayerOps.with(p, ma))
// 각 플레이어에 대해 순차적으로 ma 연산 실행");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("TraverseM 사용 예:");

        MenuHelper.PrintCode(@"// 플레이어 순회하며 순차적으로 처리
public static PontoonGame<Unit> with<A>(Seq<PontoonPlayer> players, PontoonGame<A> ma) =>
    players.TraverseM(p => PontoonPlayerOps.with(p, ma))  // 순차 실행
           .Map(_ => unit)
           .As();

// 각 플레이어마다:
// 1. 해당 플레이어를 현재 플레이어로 설정
// 2. ma 연산 실행
// 3. 다음 플레이어로 이동
// 순서가 보장됨!");

        // ============================================================
        // 17. IgnoreF - 결과값 무시
        // ============================================================
        MenuHelper.PrintSubHeader("17. IgnoreF - 결과값 무시");

        MenuHelper.PrintExplanation("IgnoreF는 모나드 연산의 결과값을 버리고 효과만 유지합니다:");
        MenuHelper.PrintExplanation("  - K<M, A>.IgnoreF() → K<M, Unit>");
        MenuHelper.PrintExplanation("  - 값은 필요 없고 효과(부수작용)만 필요할 때 사용");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// IgnoreF 사용
// 결과값 A를 버리고 Unit으로 변환
K<M, A> computation = ...;
K<M, Unit> effectOnly = computation.IgnoreF();

// FinalProject.cs Line 667에서 사용
public static PontoonGame<Unit> with<A>(PontoonGame<Seq<PontoonPlayer>> playersM, PontoonGame<A> ma) =>
    playersM.Bind(ps => with(ps, ma))
            .IgnoreF()  // Seq<A> 결과는 필요 없음, Unit으로 변환
            .As();

// 같은 효과:
// .Map(_ => unit)
// 하지만 IgnoreF가 더 명시적이고 간결함");

        // ============================================================
        // 18. >>> 연산자 - 순차 실행
        // ============================================================
        MenuHelper.PrintSubHeader("18. >>> 연산자 - 순차 실행");

        MenuHelper.PrintExplanation(">>> 연산자는 두 액션을 순차 실행하고 두 번째 결과를 반환합니다:");
        MenuHelper.PrintExplanation("  - ma >>> mb: ma 실행 후 mb 실행, mb의 결과 반환");
        MenuHelper.PrintExplanation("  - >> 연산자와 유사하지만 모나드 컨텍스트에서 사용");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// >>> vs >> 연산자
// >> : 함수 합성 (f >> g = x => g(f(x)))
// >>> : 모나드 액션 순차 실행 (ma >>> mb = ma 실행 후 mb 실행)

// FinalProject.cs Line 898-900에서 사용
static PontoonGame<Unit> playHands =>
    from _ in initPlayers >>>  // 1. 플레이어 초기화
              playHand >>>     // 2. 게임 진행
              PontoonDisplay.askPlayAgain  // 3. 재시작 질문
    from key in PontoonConsole.readKey
    from __ in when(key.Key == ConsoleKey.Y, playHands)
    select unit;

// >>> 체인: initPlayers → playHand → askPlayAgain
// 각 단계의 결과값은 무시, 마지막(askPlayAgain)의 결과가 from _에 바인딩");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation(">>> vs from/select 비교:");

        MenuHelper.PrintCode(@"// LINQ 스타일 (값을 참조할 때)
from x in action1
from y in action2  // x 참조 가능
select y

// >>> 스타일 (값 무시, 순차 실행만 필요할 때)
action1 >>> action2  // 간결함!

// 같은 의미:
action1 >>> action2
≡
from _ in action1
from result in action2
select result");

        // ============================================================
        // 19. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("19. 정리");

        MenuHelper.PrintExplanation("Foldable<T>:");
        MenuHelper.PrintExplanation("  - Fold로 컬렉션을 단일 값으로 축적");
        MenuHelper.PrintExplanation("  - Sum, Count, Max 등 집계 연산 구현 가능");
        MenuHelper.PrintExplanation("  - FoldBack으로 역순 처리");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Traversable<T>:");
        MenuHelper.PrintExplanation("  - Traverse로 이펙트 함수를 일괄 적용 (Applicative)");
        MenuHelper.PrintExplanation("  - TraverseM으로 순차적 효과 적용 (Monad)");
        MenuHelper.PrintExplanation("  - Sequence로 이펙트 순서 뒤집기");
        MenuHelper.PrintExplanation("  - 배치 처리, 일괄 검증에 핵심");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("유틸리티 연산:");
        MenuHelper.PrintExplanation("  - IgnoreF: 결과값 무시, 효과만 유지 (K<M, A> → K<M, Unit>)");
        MenuHelper.PrintExplanation("  - >>>: 순차 실행, 두 번째 결과 반환");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("비동기 Traverse:");
        MenuHelper.PrintExplanation("  - TraverseAsync로 비동기 함수 일괄 적용");
        MenuHelper.PrintExplanation("  - 순차 vs 병렬 선택 가능");
        MenuHelper.PrintExplanation("  - Task + Option 조합으로 안전한 비동기 처리");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("핵심: '하나라도 실패하면 전체 실패' 의미론!");
        MenuHelper.PrintExplanation("다음: Exercise09에서 종합 실습을 진행합니다.");

        MenuHelper.PrintSuccess("Foldable과 Traversable 학습 완료!");
    }

    // ============================================================
    // 헬퍼 함수들
    // ============================================================

    /// <summary>
    /// 문자열을 정수로 파싱 (Option)
    /// </summary>
    private static Option<int> ParseInt(string s)
        => int.TryParse(s, out var n) ? Some(n) : None;

    /// <summary>
    /// 문자열을 정수로 파싱 (Either with 에러 메시지)
    /// </summary>
    private static Either<string, int> ParseIntWithError(string s)
        => int.TryParse(s, out var n)
            ? Right<string, int>(n)
            : Left<string, int>($"'{s}'는 유효한 숫자가 아닙니다");

    /// <summary>
    /// 이메일 검증
    /// </summary>
    private static Option<string> ValidateEmail(string email)
        => email.Contains('@') && email.Contains('.')
            ? Some(email)
            : None;

    /// <summary>
    /// ID로 사용자 조회
    /// </summary>
    private static Option<string> FindUserById(int id)
        => id switch
        {
            1 => Some("John"),
            2 => Some("Jane"),
            3 => Some("Bob"),
            _ => None
        };

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

    /// <summary>
    /// 수동 Sequence 구현 (Option용)
    /// </summary>
    private static Option<Seq<A>> SequenceOption<A>(Seq<Option<A>> seq)
        => TraverseOption(seq, x => x);

    /// <summary>
    /// 수동 Traverse 구현 (Either용)
    /// </summary>
    private static Either<L, Seq<R>> TraverseEither<A, L, R>(Seq<A> seq, Func<A, Either<L, R>> f)
    {
        var results = new System.Collections.Generic.List<R>();
        foreach (var item in seq)
        {
            var result = f(item);
            if (result.IsLeft)
                return result.Match(Right: _ => default!, Left: err => Left<L, Seq<R>>(err));
            results.Add(result.Match(Right: x => x, Left: _ => default!));
        }
        return Right<L, Seq<R>>(toSeq(results));
    }

    // ============================================================
    // 비동기 헬퍼 함수들
    // ============================================================

    /// <summary>
    /// 비동기 Traverse (순차 실행, Option용)
    /// </summary>
    private static async Task<Option<Seq<B>>> TraverseOptionAsync<A, B>(
        Seq<A> seq,
        Func<A, Task<Option<B>>> f)
    {
        var results = new System.Collections.Generic.List<B>();
        foreach (var item in seq)
        {
            var result = await f(item);
            if (result.IsNone)
                return None;
            results.Add(result.Match(Some: x => x, None: () => default!));
        }
        return Some(toSeq(results));
    }

    /// <summary>
    /// 비동기 Traverse (병렬 실행, Option용)
    /// </summary>
    private static async Task<Option<Seq<B>>> TraverseOptionParallelAsync<A, B>(
        Seq<A> seq,
        Func<A, Task<Option<B>>> f)
    {
        var tasks = seq.Map(f).ToArray();
        var results = await Task.WhenAll(tasks);

        var output = new System.Collections.Generic.List<B>();
        foreach (var result in results)
        {
            if (result.IsNone)
                return None;
            output.Add(result.Match(Some: x => x, None: () => default!));
        }
        return Some(toSeq(output));
    }

    /// <summary>
    /// 비동기 Sequence (Option용)
    /// </summary>
    private static Task<Option<Seq<A>>> SequenceOptionAsync<A>(Seq<Task<Option<A>>> tasks)
        => TraverseOptionParallelAsync(tasks, x => x);

    /// <summary>
    /// 비동기 사용자 조회 시뮬레이션
    /// </summary>
    private static async Task<Option<string>> FetchUserAsync(int id)
    {
        await Task.Delay(100);  // 네트워크 지연 시뮬레이션
        return id switch
        {
            1 => Some("John (async)"),
            2 => Some("Jane (async)"),
            3 => Some("Bob (async)"),
            _ => None
        };
    }

    /// <summary>
    /// 비동기 재고 확인 시뮬레이션
    /// </summary>
    private static async Task<Option<(int ProductId, int Stock)>> CheckStockAsync(int productId)
    {
        await Task.Delay(50);  // DB 조회 지연 시뮬레이션
        return productId switch
        {
            101 => Some((productId, 10)),
            102 => Some((productId, 5)),
            103 => Some((productId, 20)),
            _ => None  // 상품 없음 또는 재고 없음
        };
    }
}
