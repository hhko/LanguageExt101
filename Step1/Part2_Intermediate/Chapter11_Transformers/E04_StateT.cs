using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter11_Transformers;

/// <summary>
/// StateT 모나드 트랜스포머를 학습합니다.
///
/// 학습 목표:
/// - StateT&lt;S, M, A&gt;의 개념과 구조 이해
/// - 상태 관리 핵심 연산 (get, gets, put, modify)
/// - StateT의 실행 방법
/// - 실전 예제 (카운터, 스택)
///
/// 핵심 개념:
/// StateT는 "상태를 가진 계산"을 다른 모나드와 결합합니다.
/// StateT&lt;S, M, A&gt;는 상태 S를 가지고, M 모나드의 효과 안에서
/// A 값을 계산하는 트랜스포머입니다.
///
/// 구조: StateT&lt;S, M, A&gt; = S → M&lt;(A, S)&gt;
/// 예시: StateT&lt;int, IO, string&gt; = int → IO&lt;(string, int)&gt;
///
/// FinalProject.cs 연결:
/// PontoonGame&lt;A&gt; = StateT&lt;GameState, OptionT&lt;IO&gt;, A&gt;
/// Line 312에서 게임 상태를 관리하는 핵심 트랜스포머로 사용됩니다.
/// </summary>
public static class E04_StateT
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 11-E04: StateT 모나드 트랜스포머");

        // ============================================================
        // 1. StateT란?
        // ============================================================
        MenuHelper.PrintSubHeader("1. StateT란?");

        MenuHelper.PrintExplanation("StateT는 '상태를 가진 계산'을 모나드로 표현합니다:");
        MenuHelper.PrintExplanation("  - State<S, A>: 순수한 상태 변환 (S → (A, S))");
        MenuHelper.PrintExplanation("  - StateT<S, M, A>: M 모나드 안에서 상태 변환 (S → M<(A, S)>)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("OptionT, EitherT와의 비교:");
        MenuHelper.PrintExplanation("  - OptionT<M, A>: M 안에 Option을 감쌈 → M<Option<A>>");
        MenuHelper.PrintExplanation("  - EitherT<L, M, R>: M 안에 Either를 감쌈 → M<Either<L, R>>");
        MenuHelper.PrintExplanation("  - StateT<S, M, A>: 상태 함수를 M으로 감쌈 → S → M<(A, S)>");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// StateT의 구조
StateT<S, M, A> = S → M<(A, S)>

// 예: StateT<int, IO, string>
// 입력: int 상태
// 출력: IO<(string, int)> - IO 효과 안에서 (결과값, 새 상태) 반환");

        // ============================================================
        // 2. StateT 생성
        // ============================================================
        MenuHelper.PrintSubHeader("2. StateT 생성");

        MenuHelper.PrintExplanation("StateT.Pure<S, M, A>(value): 상태를 변경하지 않고 값만 반환");
        MenuHelper.PrintExplanation("StateT.get<M, S>(): 현재 상태를 읽음");
        MenuHelper.PrintExplanation("StateT.gets<M, S, A>(f): 상태의 일부를 읽음");
        MenuHelper.PrintExplanation("StateT.put<M, S>(s): 새 상태를 설정");
        MenuHelper.PrintExplanation("StateT.modify<M, S>(f): 상태를 변환");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Pure: 값만 감싸고 상태는 변경 없음
var pure = StateT<int, IO, string>.Pure(""hello"");
// 실행: state → IO.Pure((""hello"", state))

// get: 현재 상태를 값으로 반환
var getState = StateT.get<IO, int>();
// 실행: state → IO.Pure((state, state))

// gets: 상태를 변환하여 반환
var getName = StateT.gets<IO, Person, string>(p => p.Name);
// 실행: person → IO.Pure((person.Name, person))

// put: 새 상태 설정
var setState = StateT.put<IO, int>(42);
// 실행: _ → IO.Pure((unit, 42))

// modify: 상태 변환
var increment = StateT.modify<IO, int>(x => x + 1);
// 실행: state → IO.Pure((unit, state + 1))");

        // ============================================================
        // 3. 기본 연산 데모
        // ============================================================
        MenuHelper.PrintSubHeader("3. 기본 연산 데모");

        // get - 현재 상태 읽기
        var getDemo = StateT.get<IO, int>();
        var getResult = getDemo.Run(100).Run();
        MenuHelper.PrintCode("StateT.get<IO, int>().Run(100)");
        MenuHelper.PrintResult("결과 (값, 상태)", getResult);

        // gets - 상태 일부 읽기
        var getsDemo = StateT.gets<IO, int, string>(n => $"상태는 {n}입니다");
        var getsResult = getsDemo.Run(42).Run();
        MenuHelper.PrintCode("StateT.gets<IO, int, string>(n => $\"상태는 {n}입니다\").Run(42)");
        MenuHelper.PrintResult("결과", getsResult);

        // put - 상태 설정
        var putDemo = StateT.put<IO, int>(999);
        var putResult = putDemo.Run(0).Run();
        MenuHelper.PrintCode("StateT.put<IO, int>(999).Run(0)");
        MenuHelper.PrintResult("결과 (unit, 새 상태)", putResult);

        // modify - 상태 변환
        var modifyDemo = StateT.modify<IO, int>(x => x * 2);
        var modifyResult = modifyDemo.Run(21).Run();
        MenuHelper.PrintCode("StateT.modify<IO, int>(x => x * 2).Run(21)");
        MenuHelper.PrintResult("결과 (unit, 새 상태)", modifyResult);

        // ============================================================
        // 4. LINQ 쿼리 구문
        // ============================================================
        MenuHelper.PrintSubHeader("4. LINQ 쿼리 구문");

        MenuHelper.PrintExplanation("StateT는 LINQ 쿼리 구문을 지원합니다.");
        MenuHelper.PrintExplanation("여러 상태 연산을 순차적으로 합성할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 상태 연산 합성
var computation =
    from current in StateT.get<IO, int>()       // 현재 상태 읽기
    from _ in StateT.modify<IO, int>(x => x + 1) // 상태 증가
    from updated in StateT.get<IO, int>()        // 새 상태 읽기
    select $""변경 전: {current}, 변경 후: {updated}"";

var result = computation.Run(10);  // 초기 상태 10");

        var computation =
            from current in StateT.get<IO, int>()
            from _ in StateT.modify<IO, int>(x => x + 1)
            from updated in StateT.get<IO, int>()
            select $"변경 전: {current}, 변경 후: {updated}";

        var computationResult = computation.Run(10).Run();
        MenuHelper.PrintResult("LINQ 결과", computationResult);

        // ============================================================
        // 5. 실전 예제: 카운터
        // ============================================================
        MenuHelper.PrintSubHeader("5. 실전 예제: 카운터");

        MenuHelper.PrintExplanation("카운터를 StateT로 구현하는 예제입니다.");
        MenuHelper.PrintExplanation("상태: int (카운터 값)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 카운터 연산 정의
StateT<int, IO, Unit> increment = StateT.modify<IO, int>(n => n + 1);
StateT<int, IO, Unit> decrement = StateT.modify<IO, int>(n => n - 1);
StateT<int, IO, int> getCount = StateT.get<IO, int>();
StateT<int, IO, Unit> reset = StateT.put<IO, int>(0);

// 연산 조합
var counterOps =
    from _ in increment
    from __ in increment
    from ___ in increment
    from count1 in getCount  // 3
    from ____ in decrement
    from count2 in getCount  // 2
    select (count1, count2);");

        StateT<int, IO, Unit> increment = StateT.modify<IO, int>(n => n + 1);
        StateT<int, IO, Unit> decrement = StateT.modify<IO, int>(n => n - 1);
        StateT<int, IO, int> getCount = StateT.get<IO, int>();

        var counterOps =
            from _1 in increment
            from _2 in increment
            from _3 in increment
            from count1 in getCount
            from _4 in decrement
            from count2 in getCount
            select (count1, count2);

        var counterResult = counterOps.Run(0).Run();
        MenuHelper.PrintResult("카운터 결과 ((3번 증가 후, 1번 감소 후), 최종 상태)", counterResult);

        // ============================================================
        // 6. 실전 예제: 스택
        // ============================================================
        MenuHelper.PrintSubHeader("6. 실전 예제: 스택");

        MenuHelper.PrintExplanation("스택을 StateT로 구현하는 예제입니다.");
        MenuHelper.PrintExplanation("상태: Seq<int> (스택)");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 스택 연산 정의
StateT<Seq<int>, IO, Unit> push(int x) =>
    StateT.modify<IO, Seq<int>>(stack => x.Cons(stack));

StateT<Seq<int>, IO, Option<int>> pop() =>
    from stack in StateT.get<IO, Seq<int>>()
    from result in stack.IsEmpty
        ? StateT<Seq<int>, IO, Option<int>>.Pure(None)
        : from _ in StateT.put<IO, Seq<int>>(stack.Tail)
          select Some(stack.Head.ValueUnsafe())
    select result;

// 스택 연산 조합
var stackOps =
    from _1 in push(1)
    from _2 in push(2)
    from _3 in push(3)
    from a in pop()   // Some(3)
    from b in pop()   // Some(2)
    select (a, b);");

        StateT<Seq<int>, IO, Unit> Push(int x) =>
            StateT.modify<IO, Seq<int>>(stack => x.Cons(stack));

        StateT<Seq<int>, IO, Option<int>> Pop() =>
            StateT.get<IO, Seq<int>>().Bind(stack =>
                stack.IsEmpty
                    ? StateT<Seq<int>, IO, Option<int>>.Pure(None)
                    : StateT.put<IO, Seq<int>>(stack.Tail)
                        .Map(_ => stack.Head));

        var stackOps =
            from _1 in Push(1)
            from _2 in Push(2)
            from _3 in Push(3)
            from a in Pop()
            from b in Pop()
            select (a, b);

        var stackResult = stackOps.Run(Seq<int>()).Run();
        MenuHelper.PrintResult("스택 결과 ((pop1, pop2), 남은 스택)", stackResult);

        // ============================================================
        // 7. StateT와 IO 결합
        // ============================================================
        MenuHelper.PrintSubHeader("7. StateT와 IO 결합");

        MenuHelper.PrintExplanation("StateT<S, IO, A>는 상태 관리와 IO 효과를 결합합니다.");
        MenuHelper.PrintExplanation("콘솔 출력, 파일 읽기 등 부수효과를 상태와 함께 사용할 수 있습니다.");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// IO를 StateT로 리프팅
var logAndIncrement =
    from count in StateT.get<IO, int>()
    from _ in StateT.liftIO<int, IO, Unit>(
        IO.lift(() => Console.WriteLine($""현재 카운트: {count}"")))
    from __ in StateT.modify<IO, int>(n => n + 1)
    select unit;

// 실행: IO 효과도 함께 실행됨");

        MenuHelper.PrintExplanation("(IO 효과가 있는 예제는 출력 설명으로 대체)");

        // ============================================================
        // 8. 실행 방법
        // ============================================================
        MenuHelper.PrintSubHeader("8. 실행 방법");

        MenuHelper.PrintExplanation("StateT 실행 과정:");
        MenuHelper.PrintExplanation("1. Run(initialState): StateT 실행, M<(A, S)> 반환");
        MenuHelper.PrintExplanation("2. 내부 모나드 실행: IO의 경우 .Run()");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// StateT<S, IO, A> 실행
StateT<int, IO, string> stateT = ...;

// 1단계: StateT 실행 → K<IO, (string, int)>
K<IO, (string State, int Value)> io = stateT.Run(initialState);

// 2단계: IO 실행 → (string, int)
(string Value, int State) result = io.As().Run();

// 축약: .Run(state).Run()
var result = stateT.Run(0).Run();");

        // ============================================================
        // 9. FinalProject.cs 연결
        // ============================================================
        MenuHelper.PrintSubHeader("9. FinalProject.cs 연결");

        MenuHelper.PrintExplanation("FinalProject.cs의 PontoonGame은 StateT를 핵심으로 사용합니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// FinalProject.cs Line 312
public record PontoonGame<A>(StateT<PontoonGameState, OptionT<IO>, A> runGame) : K<PontoonGame, A>

// 분석:
// - S = PontoonGameState: 게임 상태 (덱, 플레이어, 현재 플레이어)
// - M = OptionT<IO>: Option + IO 결합 (취소 가능한 IO)
// - A = 결과 타입

// 상태 연산 (Line 477-479)
public static PontoonGame<PontoonGameState> state =>
    new(StateT.get<OptionT<IO>, PontoonGameState>());

// 상태 수정 (Line 570-571)
public static PontoonGame<Unit> modify(Func<PontoonGameState, PontoonGameState> f) =>
    new(StateT.modify<OptionT<IO>, PontoonGameState>(f));");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("StateT의 역할:");
        MenuHelper.PrintExplanation("  - get: 현재 게임 상태(덱, 플레이어) 조회");
        MenuHelper.PrintExplanation("  - gets: 상태의 일부(현재 플레이어, 남은 카드 수) 조회");
        MenuHelper.PrintExplanation("  - modify: 카드 딜, 플레이어 추가 등 상태 변경");
        MenuHelper.PrintExplanation("  - put: 덱 교체 등 상태 전체 설정");

        // ============================================================
        // 10. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("10. 정리");

        MenuHelper.PrintExplanation("StateT<S, M, A>:");
        MenuHelper.PrintExplanation("  - 구조: S → M<(A, S)>");
        MenuHelper.PrintExplanation("  - 목적: 상태 관리와 다른 효과(IO, Option 등)를 결합");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("핵심 연산:");
        MenuHelper.PrintExplanation("  - get: 현재 상태 읽기");
        MenuHelper.PrintExplanation("  - gets(f): 상태를 변환하여 읽기");
        MenuHelper.PrintExplanation("  - put(s): 새 상태 설정");
        MenuHelper.PrintExplanation("  - modify(f): 상태 변환");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("실행:");
        MenuHelper.PrintExplanation("  - .Run(initialState): 초기 상태로 실행");
        MenuHelper.PrintExplanation("  - 내부 모나드도 실행 필요 (예: .Run().Run())");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("다음: E05_CompositeStack에서 StateT + OptionT + IO 복합 스택을 학습합니다.");

        MenuHelper.PrintSuccess("StateT 학습 완료!");
    }
}
