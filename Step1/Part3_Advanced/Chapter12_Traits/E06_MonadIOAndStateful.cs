using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter12_Traits;

/// <summary>
/// MonadIO와 Stateful 트레잇을 학습합니다.
///
/// 학습 목표:
/// - MonadIO 트레잇의 개념과 LiftIO 연산
/// - Stateful 트레잇의 개념과 상태 관리 연산
/// - 트레잇을 활용한 추상 프로그래밍
///
/// 핵심 개념:
/// MonadIO: IO 효과를 다른 모나드로 리프팅하는 인터페이스
/// Stateful: 상태 관리 연산(get, put, modify)을 제공하는 인터페이스
///
/// FinalProject.cs 연결:
/// - Line 377: MonadIO&lt;PontoonGame&gt; 구현
/// - Line 388-390: LiftIO 구현으로 IO를 Game으로 리프팅
/// - Line 376: Deriving.Stateful로 상태 관리 연산 획득
/// </summary>
public static class E06_MonadIOAndStateful
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 12-E06: MonadIO와 Stateful 트레잇");

        // ============================================================
        // 1. MonadIO란?
        // ============================================================
        MenuHelper.PrintSubHeader("1. MonadIO란?");

        MenuHelper.PrintExplanation("MonadIO는 IO 모나드의 효과를 다른 모나드로 '리프팅'합니다:");
        MenuHelper.PrintExplanation("  - liftIO: IO<A> → M<A>");
        MenuHelper.PrintExplanation("  - 모나드 트랜스포머 스택에서 IO 효과를 사용할 수 있게 함");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// MonadIO 트레잇 정의 (간소화)
public interface MonadIO<M> : Monad<M>
{
    // IO를 M으로 리프팅
    static abstract K<M, A> LiftIO<A>(IO<A> ma);
}

// 사용 예: OptionT<IO>에서 IO 사용
// liftIO(IO.Pure(42)) → OptionT<IO, int>");

        // ============================================================
        // 2. LiftIO의 필요성
        // ============================================================
        MenuHelper.PrintSubHeader("2. LiftIO의 필요성");

        MenuHelper.PrintExplanation("문제: 트랜스포머 스택에서 기저 IO를 어떻게 사용할까?");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 예: StateT<S, OptionT<IO>, A> 스택에서 콘솔 출력하기

// IO.WriteLine은 IO<Unit>을 반환
IO<Unit> consoleWrite = IO.lift(() => Console.WriteLine(""Hello""));

// 하지만 우리 스택은 StateT<S, OptionT<IO>, Unit>을 기대
// → liftIO로 변환 필요!

StateT<S, OptionT<IO>, Unit> inStack =
    MonadIO.liftIO<StateT<S, OptionT<IO>>, Unit>(consoleWrite);

// 또는 LanguageExt의 암시적 변환 사용
StateT<S, OptionT<IO>, Unit> fromImplicit = consoleWrite;  // 자동 리프팅");

        // ============================================================
        // 3. MonadIO 사용 예제
        // ============================================================
        MenuHelper.PrintSubHeader("3. MonadIO 사용 예제");

        MenuHelper.PrintExplanation("OptionT<IO>에서 liftIO 사용:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// IO 연산을 OptionT로 리프팅
var optionTWithIO =
    from x in OptionT.Some<IO, int>(10)
    from _ in MonadIO.liftIO<OptionT<IO>, Unit>(
        IO.lift(() => Console.WriteLine($""값: {x}"")))
    from y in OptionT.Some<IO, int>(20)
    select x + y;

// 실행하면 콘솔에 ""값: 10"" 출력 후 Some(30) 반환");

        // 실제 데모 (콘솔 출력 대신 값으로 시뮬레이션)
        var optionTDemo =
            from x in OptionT.Some<IO, int>(10)
            from y in OptionT.Some<IO, int>(20)
            select x + y;

        var demoResult = optionTDemo.Run().Run();
        MenuHelper.PrintResult("OptionT<IO> 결과", demoResult);

        // ============================================================
        // 4. Stateful이란?
        // ============================================================
        MenuHelper.PrintSubHeader("4. Stateful이란?");

        MenuHelper.PrintExplanation("Stateful 트레잇은 상태 관리 연산을 제공합니다:");
        MenuHelper.PrintExplanation("  - get: 현재 상태 읽기");
        MenuHelper.PrintExplanation("  - put(s): 새 상태 설정");
        MenuHelper.PrintExplanation("  - gets(f): 상태 일부 읽기");
        MenuHelper.PrintExplanation("  - modify(f): 상태 변환");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Stateful 트레잇 정의 (간소화)
public interface Stateful<M, S> : Monad<M>
{
    // 현재 상태 읽기: M<S>
    static abstract K<M, S> Get { get; }

    // 새 상태 설정: S → M<Unit>
    static abstract K<M, Unit> Put(S state);

    // 상태를 변환하여 읽기: (S → A) → M<A>
    static virtual K<M, A> Gets<A>(Func<S, A> f) =>
        Get.Map(f);

    // 상태 변환: (S → S) → M<Unit>
    static virtual K<M, Unit> Modify(Func<S, S> f) =>
        from s in Get
        from _ in Put(f(s))
        select unit;
}");

        // ============================================================
        // 5. Stateful 연산 데모
        // ============================================================
        MenuHelper.PrintSubHeader("5. Stateful 연산 데모");

        MenuHelper.PrintExplanation("StateT를 통한 Stateful 사용:");
        MenuHelper.PrintBlankLines();

        // get 데모
        var getDemo = StateT.get<IO, int>();
        var getResult = getDemo.Run(42).Run();
        MenuHelper.PrintCode("StateT.get<IO, int>().Run(42)");
        MenuHelper.PrintResult("get 결과 (상태를 값으로)", getResult);

        // put 데모
        var putDemo = StateT.put<IO, int>(100);
        var putResult = putDemo.Run(0).Run();
        MenuHelper.PrintCode("StateT.put<IO, int>(100).Run(0)");
        MenuHelper.PrintResult("put 결과 (unit, 새 상태)", putResult);

        // gets 데모
        var getsDemo = StateT.gets<IO, (int X, int Y), int>(s => s.X + s.Y);
        var getsResult = getsDemo.Run((10, 20)).Run();
        MenuHelper.PrintCode("StateT.gets<IO, (X, Y), int>(s => s.X + s.Y).Run((10, 20))");
        MenuHelper.PrintResult("gets 결과", getsResult);

        // modify 데모
        var modifyDemo = StateT.modify<IO, int>(n => n * 2);
        var modifyResult = modifyDemo.Run(21).Run();
        MenuHelper.PrintCode("StateT.modify<IO, int>(n => n * 2).Run(21)");
        MenuHelper.PrintResult("modify 결과 (unit, 변환된 상태)", modifyResult);

        // ============================================================
        // 6. 복합 상태 관리
        // ============================================================
        MenuHelper.PrintSubHeader("6. 복합 상태 관리");

        MenuHelper.PrintExplanation("record로 정의한 복합 상태를 관리하는 예제:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 복합 상태 정의
record AppState(int Counter, Seq<string> Logs);

// 카운터 증가
StateT<AppState, IO, Unit> incrementCounter =
    StateT.modify<IO, AppState>(s =>
        s with { Counter = s.Counter + 1 });

// 로그 추가
StateT<AppState, IO, Unit> addLog(string msg) =>
    StateT.modify<IO, AppState>(s =>
        s with { Logs = s.Logs.Add(msg) });

// 현재 카운터 읽기
StateT<AppState, IO, int> getCounter =
    StateT.gets<IO, AppState, int>(s => s.Counter);");

        // 실제 데모
        var complexDemo =
            from _1 in IncrementCounter
            from _2 in IncrementCounter
            from _3 in AddLog("카운터를 2번 증가시킴")
            from count in GetCounter
            from _4 in AddLog($"현재 카운터: {count}")
            from state in StateT.get<IO, DemoAppState>()
            select state;

        var complexResult = complexDemo.Run(new DemoAppState(0, Seq<string>())).Run();
        MenuHelper.PrintResult("복합 상태 결과", complexResult);

        // ============================================================
        // 7. FinalProject.cs의 MonadIO/Stateful
        // ============================================================
        MenuHelper.PrintSubHeader("7. FinalProject.cs의 MonadIO/Stateful");

        MenuHelper.PrintExplanation("PontoonGame에서 MonadIO와 Stateful 활용:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// MonadIO 구현 (Line 387-390)
public partial class PontoonGame : MonadIO<PontoonGame>
{
    // IO를 PontoonGame으로 리프팅
    public static K<PontoonGame, A> LiftIO<A>(IO<A> ma) =>
        new PontoonGame<A>(StateT.liftIO<PontoonGameState, OptionT<IO>, A>(ma));
}

// 사용 예: 콘솔 출력을 Game 컨텍스트에서 사용
public static PontoonGame<Unit> writeLine(string line) =>
    PontoonGame.liftIO(IO.lift(() => Console.WriteLine(line)));

// Stateful 활용 (Line 376, Deriving으로 자동 구현)
// 게임 상태 읽기
public static PontoonGame<PontoonGameState> state =>
    new(StateT.get<OptionT<IO>, PontoonGameState>());

// 게임 상태 수정
public static PontoonGame<Unit> modify(Func<PontoonGameState, PontoonGameState> f) =>
    new(StateT.modify<OptionT<IO>, PontoonGameState>(f));");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("LINQ에서의 활용:");

        MenuHelper.PrintCode(@"// FinalProject.cs의 게임 로직
var dealCard =
    from card in PontoonDeckOps.deal    // 상태에서 카드 꺼내기 (Stateful)
    from _ in PontoonPlayerOps.addCard(card)  // 플레이어에 카드 추가 (Stateful)
    select unit;

var showCard =
    from player in PontoonPlayerOps.current
    from _ in PontoonConsole.writeLine($""{player}의 카드"")  // MonadIO
    select unit;");

        // ============================================================
        // 8. 제네릭 함수 작성
        // ============================================================
        MenuHelper.PrintSubHeader("8. 제네릭 함수 작성");

        MenuHelper.PrintExplanation("트레잇을 활용하면 모나드에 독립적인 함수를 작성할 수 있습니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 어떤 Stateful 모나드에서도 사용 가능한 함수
static K<M, int> IncrementAndGet<M, S>()
    where M : Stateful<M, S>
    where S : IHasCounter  // Counter 속성을 가진 상태
{
    return
        from _ in Stateful.modify<M, S>(s => s.WithCounter(s.Counter + 1))
        from count in Stateful.gets<M, S, int>(s => s.Counter)
        select count;
}

// 어떤 MonadIO에서도 사용 가능한 함수
static K<M, string> ReadLineWithPrompt<M>(string prompt)
    where M : MonadIO<M>
{
    return
        from _ in MonadIO.liftIO<M, Unit>(IO.lift(() => Console.Write(prompt)))
        from input in MonadIO.liftIO<M, string>(IO.lift(Console.ReadLine))
        select input ?? """";
}");

        // ============================================================
        // 9. MonadIO vs StateT.liftIO
        // ============================================================
        MenuHelper.PrintSubHeader("9. MonadIO vs StateT.liftIO");

        MenuHelper.PrintExplanation("두 가지 IO 리프팅 방법:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 1. StateT.liftIO - StateT 전용
StateT<S, M, A> stateIO = StateT.liftIO<S, M, A>(ioAction);
// 내부적으로: s → M.liftIO(ioAction).Map(a => (a, s))

// 2. MonadIO.liftIO - 범용 (트레잇 기반)
K<M, A> genericIO = MonadIO.liftIO<M, A>(ioAction);
// M이 MonadIO를 구현하면 어디서든 사용 가능

// FinalProject에서는 둘 다 사용:
// - 내부 구현: StateT.liftIO (Line 389)
// - 공개 API: MonadIO 활용");

        // ============================================================
        // 10. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("10. 정리");

        MenuHelper.PrintExplanation("MonadIO<M> 트레잇:");
        MenuHelper.PrintExplanation("  - LiftIO: IO<A> → M<A>");
        MenuHelper.PrintExplanation("  - 트랜스포머 스택에서 IO 효과 사용");
        MenuHelper.PrintExplanation("  - 콘솔 입출력, 파일 작업 등 부수효과 통합");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Stateful<M, S> 트레잇:");
        MenuHelper.PrintExplanation("  - Get: 현재 상태 읽기");
        MenuHelper.PrintExplanation("  - Put(s): 새 상태 설정");
        MenuHelper.PrintExplanation("  - Gets(f): 상태 일부 읽기");
        MenuHelper.PrintExplanation("  - Modify(f): 상태 변환");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("FinalProject 적용:");
        MenuHelper.PrintExplanation("  - PontoonGame은 MonadIO + Stateful 구현");
        MenuHelper.PrintExplanation("  - IO 효과와 게임 상태를 단일 모나드로 관리");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("다음: FinalProject.cs에서 이 모든 개념이 통합된 실전 예제를 확인하세요!");

        MenuHelper.PrintSuccess("MonadIO와 Stateful 트레잇 학습 완료!");
    }

    // ============================================================
    // 데모용 헬퍼
    // ============================================================

    private record DemoAppState(int Counter, Seq<string> Logs);

    private static StateT<DemoAppState, IO, Unit> IncrementCounter =>
        StateT.modify<IO, DemoAppState>(s => s with { Counter = s.Counter + 1 });

    private static StateT<DemoAppState, IO, Unit> AddLog(string msg) =>
        StateT.modify<IO, DemoAppState>(s => s with { Logs = s.Logs.Add(msg) });

    private static StateT<DemoAppState, IO, int> GetCounter =>
        StateT.gets<IO, DemoAppState, int>(s => s.Counter);
}
