using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part2_Intermediate.Chapter11_Transformers;

/// <summary>
/// 복합 모나드 트랜스포머 스택을 학습합니다.
///
/// 학습 목표:
/// - 트랜스포머 스택 설계 원리
/// - StateT + OptionT + IO 조합 이해
/// - 실행 순서와 언래핑 방법
/// - FinalProject.cs의 Game 모나드 구조 분석
///
/// 핵심 개념:
/// 모나드 트랜스포머는 중첩하여 여러 효과를 결합할 수 있습니다.
/// 각 트랜스포머는 특정 효과를 제공하며, 스택의 순서가 의미를 결정합니다.
///
/// FinalProject.cs 연결:
/// PontoonGame&lt;A&gt; = StateT&lt;GameState, OptionT&lt;IO&gt;, A&gt;
/// - IO: 기저 모나드 (콘솔 입출력)
/// - OptionT: IO 위에 옵션 효과 (게임 취소)
/// - StateT: 가장 바깥, 상태 관리 (게임 상태)
/// </summary>
public static class E05_CompositeStack
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 11-E05: 복합 트랜스포머 스택");

        // ============================================================
        // 1. 왜 복합 스택이 필요한가?
        // ============================================================
        MenuHelper.PrintSubHeader("1. 왜 복합 스택이 필요한가?");

        MenuHelper.PrintExplanation("실무에서는 여러 효과가 동시에 필요합니다:");
        MenuHelper.PrintExplanation("  - 상태 관리 + 에러 처리 + IO");
        MenuHelper.PrintExplanation("  - 설정 읽기 + 로깅 + 비동기");
        MenuHelper.PrintExplanation("  - 트랜잭션 + 검증 + 외부 API 호출");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("모나드 트랜스포머를 쌓아서 이 모든 효과를 조합합니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 단일 효과
IO<A>              // 부수효과만
Option<A>          // 실패 가능성만
State<S, A>        // 상태만

// 두 효과 조합
OptionT<IO, A>     // IO + 실패 가능성
StateT<S, IO, A>   // IO + 상태

// 세 효과 조합
StateT<S, OptionT<IO>, A>  // IO + 실패 가능성 + 상태
// FinalProject의 Game 모나드가 바로 이 구조!");

        // ============================================================
        // 2. 스택 구조 분석
        // ============================================================
        MenuHelper.PrintSubHeader("2. 스택 구조 분석");

        MenuHelper.PrintExplanation("StateT<S, OptionT<IO>, A> 구조:");
        MenuHelper.PrintBlankLines();

        Console.WriteLine(@"
    ┌─────────────────────────────────────┐
    │  StateT<S, OptionT<IO>, A>          │  ← 가장 바깥 (상태 관리)
    │  S → OptionT<IO, (A, S)>            │
    │                                     │
    │  ┌───────────────────────────────┐  │
    │  │  OptionT<IO, (A, S)>          │  │  ← 중간 (실패 처리)
    │  │  IO<Option<(A, S)>>           │  │
    │  │                               │  │
    │  │  ┌─────────────────────────┐  │  │
    │  │  │  IO<Option<(A, S)>>     │  │  │  ← 가장 안쪽 (부수효과)
    │  │  │  () → Option<(A, S)>   │  │  │
    │  │  └─────────────────────────┘  │  │
    │  └───────────────────────────────┘  │
    └─────────────────────────────────────┘
");

        MenuHelper.PrintExplanation("각 레이어의 역할:");
        MenuHelper.PrintExplanation("  - IO: 콘솔 입출력, 랜덤, 시간 등 부수효과");
        MenuHelper.PrintExplanation("  - OptionT: None으로 연산 취소/실패 처리");
        MenuHelper.PrintExplanation("  - StateT: 게임 상태 읽기/쓰기");

        // ============================================================
        // 3. 기본 사용 예제
        // ============================================================
        MenuHelper.PrintSubHeader("3. 기본 사용 예제");

        MenuHelper.PrintExplanation("간단한 StateT + OptionT + IO 스택:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 상태 타입
record Counter(int Value);

// 스택 정의
// StateT<Counter, OptionT<IO>, A>

// 순수 값 생성
var pure = StateT<Counter, OptionT<IO>, int>.Pure(42);

// 상태 읽기
var getCount = StateT.gets<OptionT<IO>, Counter, int>(c => c.Value);

// 상태 수정
var increment = StateT.modify<OptionT<IO>, Counter>(c => c with { Value = c.Value + 1 });

// 실패 (None)
var fail = StateT<Counter, OptionT<IO>, int>.Lift(OptionT.None<IO, int>());");

        // 실제 데모
        var pureDemo = StateT<DemoCounter, OptionT<IO>, int>.Pure(42);
        var pureResult = pureDemo.Run(new DemoCounter(0)).Run().Run();
        MenuHelper.PrintResult("Pure(42) 결과", pureResult);

        var getDemo = StateT.gets<OptionT<IO>, DemoCounter, int>(c => c.Value);
        var getResult = getDemo.Run(new DemoCounter(100)).Run().Run();
        MenuHelper.PrintResult("gets(c => c.Value).Run(100) 결과", getResult);

        // ============================================================
        // 4. LINQ로 조합하기
        // ============================================================
        MenuHelper.PrintSubHeader("4. LINQ로 조합하기");

        MenuHelper.PrintExplanation("LINQ 쿼리 구문으로 세 효과를 자연스럽게 조합:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"var computation =
    from count in StateT.gets<OptionT<IO>, Counter, int>(c => c.Value)
    from _ in guard<StateT<Counter, OptionT<IO>>>(count < 10)  // 10 미만이면 계속
    from __ in StateT.modify<OptionT<IO>, Counter>(c => c with { Value = c.Value + 1 })
    from newCount in StateT.gets<OptionT<IO>, Counter, int>(c => c.Value)
    select $""증가: {count} → {newCount}"";

// count >= 10이면 guard가 None을 반환하여 전체가 None");

        var linqDemo =
            from count in StateT.gets<OptionT<IO>, DemoCounter, int>(c => c.Value)
            from newCount in count < 10
                ? StateT.modify<OptionT<IO>, DemoCounter>(c => c with { Value = c.Value + 1 })
                    .Bind(_ => StateT.gets<OptionT<IO>, DemoCounter, int>(c => c.Value))
                : StateT<DemoCounter, OptionT<IO>, int>.Lift(OptionT.None<IO, int>())
            select $"증가: {count} → {newCount}";

        var linqResult5 = linqDemo.Run(new DemoCounter(5)).Run().Run();
        MenuHelper.PrintResult("초기값 5일 때", linqResult5);

        var linqResult15 = linqDemo.Run(new DemoCounter(15)).Run().Run();
        MenuHelper.PrintResult("초기값 15일 때 (guard 실패)", linqResult15);

        // ============================================================
        // 5. 실행 순서
        // ============================================================
        MenuHelper.PrintSubHeader("5. 실행 순서");

        MenuHelper.PrintExplanation("스택을 풀 때는 바깥에서 안쪽으로 실행합니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// StateT<S, OptionT<IO>, A> 실행

// 1단계: StateT.Run(initialState) → OptionT<IO, (A, S)>
var optionT = stateT.Run(initialState);

// 2단계: OptionT.Run() → IO<Option<(A, S)>>
var io = optionT.Run();

// 3단계: IO.Run() → Option<(A, S)>
var result = io.Run();

// 축약: .Run(state).Run().Run()
var result = stateT.Run(state).Run().Run();");

        MenuHelper.PrintBlankLines();

        // 실행 순서 데모
        var execDemo = StateT<DemoCounter, OptionT<IO>, string>.Pure("Hello");

        MenuHelper.PrintExplanation("실행 과정 데모:");
        var step1 = execDemo.Run(new DemoCounter(0));  // K<OptionT<IO>, (string, Counter)>
        MenuHelper.PrintResult("Step 1 (StateT.Run) 타입", "K<OptionT<IO>, (string, Counter)>");

        var step2 = step1.As().Run();  // K<IO, Option<(string, Counter)>>
        MenuHelper.PrintResult("Step 2 (OptionT.Run) 타입", "K<IO, Option<(string, Counter)>>");

        var step3 = step2.As().Run();  // Option<(string, Counter)>
        MenuHelper.PrintResult("Step 3 (IO.Run) 결과", step3);

        // ============================================================
        // 6. 리프팅 (Lifting)
        // ============================================================
        MenuHelper.PrintSubHeader("6. 리프팅 (Lifting)");

        MenuHelper.PrintExplanation("내부 레이어의 연산을 외부로 리프팅:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// IO를 스택으로 리프팅
IO<A> ioAction = IO.Pure(42);
StateT<S, OptionT<IO>, A> lifted = StateT.liftIO<S, OptionT<IO>, A>(ioAction);

// Option을 스택으로 리프팅
Option<A> optionValue = Some(42);
StateT<S, OptionT<IO>, A> liftedOpt =
    StateT<S, OptionT<IO>, A>.Lift(OptionT.lift<IO, A>(optionValue));

// OptionT를 StateT로 리프팅
OptionT<IO, A> optionT = OptionT.Some<IO, int>(42);
StateT<S, OptionT<IO>, A> liftedOptionT = StateT<S, OptionT<IO>, A>.Lift(optionT);");

        // 리프팅 데모
        var liftedIO = StateT.liftIO<DemoCounter, OptionT<IO>, int>(IO.pure(999));
        var liftedResult = liftedIO.Run(new DemoCounter(0)).Run().Run();
        MenuHelper.PrintResult("IO.pure(999) 리프팅 결과", liftedResult);

        // ============================================================
        // 7. FinalProject의 Game 모나드 분석
        // ============================================================
        MenuHelper.PrintSubHeader("7. FinalProject의 Game 모나드 분석");

        MenuHelper.PrintExplanation("PontoonGame<A>의 구조:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// FinalProject.cs Line 312
public record PontoonGame<A>(StateT<PontoonGameState, OptionT<IO>, A> runGame) : K<PontoonGame, A>

// 분석:
// - A: 계산 결과 타입
// - runGame: 내부 StateT 트랜스포머

// 실행 (Line 416-418)
public static IO<Option<(A Value, PontoonGameState State)>> Run<A>(
    this K<PontoonGame, A> ma, PontoonGameState state) =>
    ma.As().runGame.Run(state).As().Run().As();
//                 ↑ StateT.Run ↑ OptionT.Run
// 결과: IO<Option<(A, State)>>");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("게임 흐름에서의 역할:");

        MenuHelper.PrintCode(@"// 게임 시작
PontoonGame.play
    .Run(PontoonGameState.Zero)  // StateT 실행: 초기 상태로 시작
    .Run()                       // OptionT 풀기 → IO<Option<...>>
    .Ignore();                   // IO 실행, 결과 무시

// None이 되는 경우:
// 1. 사용자가 게임 취소
// 2. 덱에 카드가 없음
// 3. 플레이어가 없음");

        // ============================================================
        // 8. 효과 조합 순서의 의미
        // ============================================================
        MenuHelper.PrintSubHeader("8. 효과 조합 순서의 의미");

        MenuHelper.PrintExplanation("스택 순서가 의미론을 결정합니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// StateT<S, OptionT<IO>, A>
// = S → OptionT<IO, (A, S)>
// = S → IO<Option<(A, S)>>
// 의미: 실패하면 상태 변경도 취소됨 (Option이 안쪽)

// vs OptionT<StateT<S, IO>, A>
// = StateT<S, IO, Option<A>>
// = S → IO<(Option<A>, S)>
// 의미: 실패해도 상태 변경은 유지됨 (State가 안쪽)

// FinalProject는 첫 번째 구조 사용:
// 게임 취소 시 상태 변경도 롤백되는 효과");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("다이어그램:");

        Console.WriteLine(@"
    StateT<S, OptionT<IO>, A> (FinalProject 구조)
    ────────────────────────────────────────────
    상태 S=10에서 시작
    ↓
    S를 20으로 변경
    ↓
    None 발생! (게임 취소)
    ↓
    결과: None (상태 변경도 없었던 것처럼)

    OptionT<StateT<S, IO>, A> (대안 구조)
    ────────────────────────────────────────────
    상태 S=10에서 시작
    ↓
    S를 20으로 변경
    ↓
    None 발생!
    ↓
    결과: (None, S=20) (상태 변경은 유지)
");

        // ============================================================
        // 9. 실전 미니 예제
        // ============================================================
        MenuHelper.PrintSubHeader("9. 실전 미니 예제");

        MenuHelper.PrintExplanation("설정 + 에러 처리 + IO를 조합한 미니 앱:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 설정 상태
record AppConfig(string ApiUrl, int MaxRetries);

// 앱 모나드 = StateT<AppConfig, OptionT<IO>, A>

// API 호출 시뮬레이션
StateT<AppConfig, OptionT<IO>, string> fetchData(string endpoint) =>
    from config in StateT.get<OptionT<IO>, AppConfig>()
    from _ in config.MaxRetries > 0
        ? StateT<AppConfig, OptionT<IO>, Unit>.Pure(unit)
        : StateT<AppConfig, OptionT<IO>, Unit>.Lift(OptionT.None<IO, Unit>())
    select $""Fetched from {config.ApiUrl}/{endpoint}"";

// 앱 실행
var app =
    from data1 in fetchData(""users"")
    from data2 in fetchData(""orders"")
    select (data1, data2);

var result = app.Run(new AppConfig(""https://api.example.com"", 3)).Run().Run();");

        StateT<DemoAppConfig, OptionT<IO>, string> FetchData(string endpoint) =>
            StateT.get<OptionT<IO>, DemoAppConfig>().Bind(config =>
                config.MaxRetries > 0
                    ? StateT<DemoAppConfig, OptionT<IO>, string>.Pure($"Fetched from {config.ApiUrl}/{endpoint}")
                    : StateT<DemoAppConfig, OptionT<IO>, string>.Lift(OptionT.None<IO, string>()));

        var appDemo =
            from data1 in FetchData("users")
            from data2 in FetchData("orders")
            select (data1, data2);

        var appResult = appDemo.Run(new DemoAppConfig("https://api.example.com", 3)).Run().Run();
        MenuHelper.PrintResult("앱 실행 결과", appResult);

        var appResultFail = appDemo.Run(new DemoAppConfig("https://api.example.com", 0)).Run().Run();
        MenuHelper.PrintResult("MaxRetries=0일 때 (실패)", appResultFail);

        // ============================================================
        // 10. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("10. 정리");

        MenuHelper.PrintExplanation("복합 트랜스포머 스택:");
        MenuHelper.PrintExplanation("  - 여러 효과를 하나의 모나드로 조합");
        MenuHelper.PrintExplanation("  - StateT<S, OptionT<IO>, A> = 상태 + 실패 + IO");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("실행 순서:");
        MenuHelper.PrintExplanation("  - 바깥에서 안쪽으로: .Run(state).Run().Run()");
        MenuHelper.PrintExplanation("  - StateT → OptionT → IO 순서로 풀기");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("스택 순서의 의미:");
        MenuHelper.PrintExplanation("  - StateT<S, OptionT<M>, A>: 실패 시 상태도 롤백");
        MenuHelper.PrintExplanation("  - OptionT<StateT<S, M>, A>: 실패해도 상태 유지");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("FinalProject 연결:");
        MenuHelper.PrintExplanation("  - PontoonGame<A> = StateT<GameState, OptionT<IO>, A>");
        MenuHelper.PrintExplanation("  - 게임 상태, 취소 가능성, IO를 하나로 통합");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("다음: Chapter 12에서 Deriving 패턴으로 커스텀 모나드를 만드는 방법을 학습합니다.");

        MenuHelper.PrintSuccess("복합 트랜스포머 스택 학습 완료!");
    }

    // ============================================================
    // 데모용 타입들
    // ============================================================

    private record DemoCounter(int Value);

    private record DemoAppConfig(string ApiUrl, int MaxRetries);
}
