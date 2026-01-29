using LanguageExt;
using LanguageExt.Traits;
using LanguageExt101.Common;
using static LanguageExt.Prelude;

namespace LanguageExt101.Part3_Advanced.Chapter12_Traits;

/// <summary>
/// Deriving 패턴을 학습합니다.
///
/// 학습 목표:
/// - Deriving의 목적과 필요성 이해
/// - Transform/CoTransform 브릿지 패턴
/// - Deriving.Monad, Deriving.Stateful 사용법
/// - 커스텀 모나드 타입에 트레잇 자동 구현
///
/// 핵심 개념:
/// Deriving은 "기존 모나드를 감싸는 커스텀 타입"에
/// 트레잇 인스턴스를 자동으로 생성해주는 패턴입니다.
///
/// FinalProject.cs 연결:
/// Line 374-377에서 PontoonGame 타입이 Deriving을 사용하여
/// Monad, Stateful 인스턴스를 자동으로 획득합니다.
/// </summary>
public static class E05_Deriving
{
    public static void Run()
    {
        MenuHelper.PrintHeader("Chapter 12-E05: Deriving 패턴");

        // ============================================================
        // 1. Deriving이란?
        // ============================================================
        MenuHelper.PrintSubHeader("1. Deriving이란?");

        MenuHelper.PrintExplanation("문제 상황:");
        MenuHelper.PrintExplanation("  - 기존 모나드(StateT, OptionT 등)를 감싸는 커스텀 타입을 만들 때");
        MenuHelper.PrintExplanation("  - Monad, Applicative 등 트레잇을 수동 구현해야 함");
        MenuHelper.PrintExplanation("  - 매번 같은 패턴의 코드를 반복 작성해야 함");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("Deriving의 해결:");
        MenuHelper.PrintExplanation("  - Transform/CoTransform 두 함수만 정의");
        MenuHelper.PrintExplanation("  - 나머지 트레잇 구현은 자동으로 파생(derive)");
        MenuHelper.PrintExplanation("  - 보일러플레이트 코드 대폭 감소");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Deriving 없이 (수동 구현 - 많은 코드 필요)
public class MyMonad : Monad<MyMonad>
{
    public static K<MyMonad, A> Pure<A>(A value) => ...
    public static K<MyMonad, B> Bind<A, B>(...) => ...
    public static K<MyMonad, B> Map<A, B>(...) => ...
    public static K<MyMonad, B> Apply<A, B>(...) => ...
    // ... 더 많은 메서드들
}

// Deriving 사용 (간결)
public class MyMonad : Deriving.Monad<MyMonad, InnerMonad>
{
    public static K<InnerMonad, A> Transform<A>(K<MyMonad, A> fa) => ...
    public static K<MyMonad, A> CoTransform<A>(K<InnerMonad, A> fa) => ...
    // 나머지는 자동!
}");

        // ============================================================
        // 2. Transform과 CoTransform
        // ============================================================
        MenuHelper.PrintSubHeader("2. Transform과 CoTransform");

        MenuHelper.PrintExplanation("Deriving의 핵심은 두 방향 변환 함수입니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// Transform: 커스텀 타입 → 내부 모나드
public static K<M, A> Transform<A>(K<T, A> fa)
    // T(커스텀) → M(기반)

// CoTransform: 내부 모나드 → 커스텀 타입
public static K<T, A> CoTransform<A>(K<M, A> fa)
    // M(기반) → T(커스텀)

// 이 두 함수가 '브릿지' 역할을 합니다:
// T의 연산 = CoTransform(M의 연산(Transform(...)))");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("다이어그램:");
        Console.WriteLine(@"
    MyMonad<A>  ──Transform──>  InnerMonad<A>
        ↑                            │
        │                            │ (Inner의 연산 수행)
        │                            │
        └───CoTransform───────<──────┘
");

        // ============================================================
        // 3. Deriving.Monad 사용법
        // ============================================================
        MenuHelper.PrintSubHeader("3. Deriving.Monad 사용법");

        MenuHelper.PrintExplanation("Deriving.Monad<T, M>를 구현하면:");
        MenuHelper.PrintExplanation("  - Monad<T> 인스턴스 자동 생성");
        MenuHelper.PrintExplanation("  - Applicative<T> 인스턴스 자동 생성");
        MenuHelper.PrintExplanation("  - Functor<T> 인스턴스 자동 생성");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 간단한 예시: IO를 감싸는 MyIO 타입

// 1. 커스텀 타입 정의
public record MyIO<A>(IO<A> Inner) : K<MyIO, A>;

// 2. Deriving.Monad 구현
public class MyIO : Deriving.Monad<MyIO, IO>
{
    // Transform: MyIO → IO
    public static K<IO, A> Transform<A>(K<MyIO, A> fa) =>
        ((MyIO<A>)fa).Inner;

    // CoTransform: IO → MyIO
    public static K<MyIO, A> CoTransform<A>(K<IO, A> fa) =>
        new MyIO<A>(fa.As());
}

// 이제 MyIO는 자동으로 Monad, Applicative, Functor를 구현!
// Pure, Bind, Map, Apply 등이 모두 사용 가능");

        // 실제 데모
        var myIO = SimpleIO.Pure(42);
        var mapped = SimpleIO.Map(myIO, x => x * 2);
        var result = mapped.Inner.Run();
        MenuHelper.PrintResult("SimpleIO.Map(42, x => x * 2)", result);

        // ============================================================
        // 4. Deriving.Stateful 사용법
        // ============================================================
        MenuHelper.PrintSubHeader("4. Deriving.Stateful 사용법");

        MenuHelper.PrintExplanation("Deriving.Stateful<T, M, S>를 구현하면:");
        MenuHelper.PrintExplanation("  - Stateful<T, S> 인스턴스 자동 생성");
        MenuHelper.PrintExplanation("  - Get, Put, Gets, Modify 연산 자동 제공");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// StateT를 감싸는 커스텀 타입

// 1. 상태 타입
public record AppState(int Counter, string Message);

// 2. 커스텀 StateT 래퍼
public record App<A>(StateT<AppState, IO, A> Inner) : K<App, A>;

// 3. Deriving.Monad + Deriving.Stateful 구현
public class App :
    Deriving.Monad<App, StateT<AppState, IO>>,
    Deriving.Stateful<App, StateT<AppState, IO>, AppState>
{
    public static K<StateT<AppState, IO>, A> Transform<A>(K<App, A> fa) =>
        ((App<A>)fa).Inner;

    public static K<App, A> CoTransform<A>(K<StateT<AppState, IO>, A> fa) =>
        new App<A>(fa.As());
}

// 이제 App 타입에서 상태 연산 사용 가능!
// Stateful.get<App, AppState>()
// Stateful.modify<App, AppState>(s => s with { Counter = s.Counter + 1 })");

        // ============================================================
        // 5. FinalProject.cs의 Deriving 사용
        // ============================================================
        MenuHelper.PrintSubHeader("5. FinalProject.cs의 Deriving 사용");

        MenuHelper.PrintExplanation("PontoonGame은 Deriving.Monad + Deriving.Stateful을 사용합니다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// FinalProject.cs Line 374-390
public partial class PontoonGame :
    Deriving.Monad<PontoonGame, StateT<PontoonGameState, OptionT<IO>>>,
    Deriving.Stateful<PontoonGame, StateT<PontoonGameState, OptionT<IO>>, PontoonGameState>,
    MonadIO<PontoonGame>
{
    // Transform: PontoonGame → StateT
    public static K<StateT<PontoonGameState, OptionT<IO>>, A> Transform<A>(K<PontoonGame, A> fa) =>
        fa.As().runGame;

    // CoTransform: StateT → PontoonGame
    public static K<PontoonGame, A> CoTransform<A>(K<StateT<PontoonGameState, OptionT<IO>>, A> fa) =>
        new PontoonGame<A>(fa.As());

    // MonadIO는 별도 구현 (IO 리프팅)
    public static K<PontoonGame, A> LiftIO<A>(IO<A> ma) =>
        new PontoonGame<A>(StateT.liftIO<PontoonGameState, OptionT<IO>, A>(ma));
}");

        MenuHelper.PrintBlankLines();
        MenuHelper.PrintExplanation("이로써 PontoonGame은:");
        MenuHelper.PrintExplanation("  - Monad: from/select LINQ 쿼리 사용 가능");
        MenuHelper.PrintExplanation("  - Stateful: 게임 상태(get/put/modify) 연산 가능");
        MenuHelper.PrintExplanation("  - MonadIO: IO 효과(콘솔 입출력) 리프팅 가능");

        // ============================================================
        // 6. Deriving의 장점
        // ============================================================
        MenuHelper.PrintSubHeader("6. Deriving의 장점");

        MenuHelper.PrintExplanation("1. 코드 감소:");
        MenuHelper.PrintExplanation("   - 수십 줄의 보일러플레이트 → 2개 메서드로 축소");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("2. 일관성:");
        MenuHelper.PrintExplanation("   - 파생된 구현은 모나드 법칙을 자동으로 준수");
        MenuHelper.PrintExplanation("   - 수동 구현 시 발생할 수 있는 버그 방지");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("3. 조합 가능:");
        MenuHelper.PrintExplanation("   - 여러 Deriving 인터페이스 동시 구현 가능");
        MenuHelper.PrintExplanation("   - Monad + Stateful + MonadIO 등 조합");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("4. 타입 안전:");
        MenuHelper.PrintExplanation("   - 컴파일 타임에 트레잇 구현 보장");
        MenuHelper.PrintExplanation("   - Transform/CoTransform 누락 시 컴파일 에러");

        // ============================================================
        // 7. 주요 Deriving 타입들
        // ============================================================
        MenuHelper.PrintSubHeader("7. 주요 Deriving 타입들");

        MenuHelper.PrintExplanation("LanguageExt에서 제공하는 Deriving 인터페이스:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 모나드 관련
Deriving.Monad<T, M>        // Monad, Applicative, Functor
Deriving.MonadT<T, M>       // Monad Transformer 전용
Deriving.Alternative<T, M>  // Alternative (선택적 실행)

// 상태 관련
Deriving.Stateful<T, M, S>  // 상태 관리 (get, put, modify)

// 리더/환경 관련
Deriving.Readable<T, M, E>  // 환경 읽기 (ask, local)

// 에러 관련
Deriving.Fallible<T, M, E>  // 에러 처리 (throw, catch)");

        // ============================================================
        // 8. 커스텀 모나드 만들기 예제
        // ============================================================
        MenuHelper.PrintSubHeader("8. 커스텀 모나드 만들기 예제");

        MenuHelper.PrintExplanation("간단한 Logger 모나드를 만들어봅시다:");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintCode(@"// 로그를 누적하는 모나드
// 내부: WriterT<Seq<string>, IO, A>를 사용

public record Logger<A>(IO<(A, Seq<string>)> Inner) : K<Logger, A>;

public class Logger : Deriving.Monad<Logger, IO>
{
    // 로그 기록
    public static Logger<Unit> Log(string message) =>
        new Logger<Unit>(IO.Pure((unit, Seq(message))));

    // 값만 반환 (로그 없음)
    public static Logger<A> Pure<A>(A value) =>
        new Logger<A>(IO.Pure((value, Seq<string>())));

    // Transform/CoTransform (간소화된 버전)
    // ... 실제로는 더 복잡한 구현 필요
}

// 사용 예:
var computation =
    from _ in Logger.Log(""시작"")
    from x in Logger.Pure(42)
    from __ in Logger.Log($""계산 결과: {x}"")
    select x;");

        // ============================================================
        // 9. 정리
        // ============================================================
        MenuHelper.PrintSubHeader("9. 정리");

        MenuHelper.PrintExplanation("Deriving 패턴:");
        MenuHelper.PrintExplanation("  - 목적: 커스텀 모나드 타입에 트레잇 자동 구현");
        MenuHelper.PrintExplanation("  - 핵심: Transform(T→M), CoTransform(M→T) 브릿지");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("주요 Deriving 인터페이스:");
        MenuHelper.PrintExplanation("  - Deriving.Monad<T, M>: 모나드 연산 자동 구현");
        MenuHelper.PrintExplanation("  - Deriving.Stateful<T, M, S>: 상태 연산 자동 구현");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("FinalProject 적용:");
        MenuHelper.PrintExplanation("  - PontoonGame이 Deriving으로 Monad + Stateful 획득");
        MenuHelper.PrintExplanation("  - LINQ 쿼리와 상태 관리를 자연스럽게 사용");
        MenuHelper.PrintBlankLines();

        MenuHelper.PrintExplanation("다음: E06_MonadIOAndStateful에서 MonadIO와 Stateful 트레잇을 학습합니다.");

        MenuHelper.PrintSuccess("Deriving 패턴 학습 완료!");
    }
}

// ============================================================
// 데모용 SimpleIO 구현 (간소화 - 직접 실행)
// ============================================================

/// <summary>데모용 SimpleIO 값 타입</summary>
public record SimpleIO<A>(IO<A> Inner) : K<SimpleIO, A>;

/// <summary>데모용 SimpleIO 타입 마커 (Deriving 없이 직접 구현)</summary>
public class SimpleIO
{
    public static SimpleIO<A> Pure<A>(A value) =>
        new(IO.pure(value));

    public static SimpleIO<B> Map<B, A>(SimpleIO<A> ma, Func<A, B> f) =>
        new(ma.Inner.Map(f));
}
